using System.Text.Json;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Commissions;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.MonthlyFinance;

// Authorized by MonthlyFinanceService. Bulk source reads, never a second ledger.
internal sealed partial class MonthlyReportBuilder(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    internal const int MaxRows = 50000;
    private readonly FinancialOperations coordination = new(db,current,access);
    private static async Task<List<T>> Bounded<T>(IQueryable<T> query,CancellationToken ct)
    {
        var rows=await query.Take(MaxRows+1).ToListAsync(ct);
        if(rows.Count>MaxRows)throw new BusinessRuleException($"Monthly report exceeds the {MaxRows} rows per dataset limit; choose bounded financial periods before closing.");
        return rows;
    }
    internal async Task<IReadOnlyList<SalaryEntitlement>> SalariesAsync(FinancialMonth month,CancellationToken ct)
    {
        var employees=await Bounded(db.Employees.AsNoTracking().Where(e=>db.EmployeeBranchAssignments.Any(x=>x.EmployeeId==e.Id&&x.BranchId==month.BranchId&&x.StartDate<=month.EndDate&&(x.EndDate==null||x.EndDate>=month.StartDate))),ct);
        var ids=employees.Select(x=>x.Id).ToArray();
        var assignments=await Bounded(db.EmployeeBranchAssignments.AsNoTracking().Where(x=>ids.Contains(x.EmployeeId)&&x.StartDate<=month.EndDate&&(x.EndDate==null||x.EndDate>=month.StartDate)),ct);
        var settings=await Bounded(db.EmployeeSalarySettings.AsNoTracking().Where(x=>ids.Contains(x.EmployeeId)&&x.EffectiveFrom<=month.EndDate&&(x.EffectiveTo==null||x.EffectiveTo>=month.StartDate)),ct);
        var advances=await db.EmployeeAdvances.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&(x.FinancialMonthId==month.Id||x.FinancialMonthId==null&&x.BusinessDate>=month.StartDate&&x.BusinessDate<=month.EndDate))
            .GroupBy(x=>x.ReceivingEmployeeId).Select(x=>new{Id=x.Key,Amount=x.Sum(a=>a.Amount)}).ToDictionaryAsync(x=>x.Id,x=>x.Amount,ct);
        var employmentChanges=await SalaryProjection.EmploymentChangesAsync(db,ids,access,ct);
        var existing=await db.Payrolls.AsNoTracking().Where(x=>x.FinancialMonthId==month.Id&&x.CommissionSummaryId==null).ToDictionaryAsync(x=>x.EmployeeId,ct);
        var assignmentsByEmployee=assignments.ToLookup(x=>x.EmployeeId);var settingsByEmployee=settings.ToLookup(x=>x.EmployeeId);
        return employees.OrderBy(x=>x.Id).Select(e=>existing.TryGetValue(e.Id,out var p)?JsonSerializer.Deserialize<SalaryEntitlement>(p.SalaryEvidenceJson!)!:
            SalaryProjection.Calculate(e,month,assignmentsByEmployee[e.Id].ToArray(),settingsByEmployee[e.Id].ToArray(),advances.GetValueOrDefault(e.Id),employmentChanges.GetValueOrDefault(e.Id))).ToArray();
    }
    internal async Task<MonthlyReport> BuildAsync(FinancialMonth month,int revision,DateTimeOffset cutoff,DrawerPosition? preserved,CancellationToken ct,IReadOnlyList<SalaryEntitlement>? preservedSalaries=null)
    {
        var financialCutoff=month.FirstClosedAt??cutoff;
        var branchName=await db.GymBranches.Where(x=>x.Id==month.BranchId).Select(x=>x.Name).SingleAsync(ct);
        var ledger=await Bounded(db.FinancialTransactions.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.OccurredAt<=financialCutoff&&
            (x.FinancialMonthId==month.Id||x.FinancialMonthId==null&&x.BusinessDate>=month.StartDate&&x.BusinessDate<=month.EndDate)).OrderBy(x=>x.OccurredAt).ThenBy(x=>x.Id),ct);
        var transactions=await TransactionsAsync(ledger,month,branchName,cutoff,ct);
        var salaries=preservedSalaries??await SalariesAsync(month,ct);
        var payrollRows=await Bounded(db.Payrolls.AsNoTracking().Include(x=>x.Employee).Where(x=>x.BranchId==month.BranchId&&x.FinancialMonthId!=null&&x.FinancialMonth!.StartDate<=month.StartDate&&x.CreatedAt<=financialCutoff)
            .OrderBy(x=>x.CreatedAt).ThenBy(x=>x.Id),ct);
        var payrollIds=payrollRows.Select(x=>x.Id).ToArray();
        var employeePayments=await Bounded(db.PayrollPaymentRecords.AsNoTracking().Where(x=>payrollIds.Contains(x.PayrollId)&&x.FinancialMonth.StartDate<=month.StartDate&&x.RecordedAt<=financialCutoff).OrderBy(x=>x.RecordedAt).ThenBy(x=>x.Id),ct);
        var paymentsByPayroll=employeePayments.ToLookup(x=>x.PayrollId);var payrollById=payrollRows.ToDictionary(x=>x.Id);
        var payroll=payrollRows.Select(x=>PayrollService.Details(x,x.Employee.FullName,paymentsByPayroll[x.Id].Sum(p=>p.Amount))).ToArray();
        var payments=employeePayments.Where(x=>x.FinancialMonthId==month.Id).Select(x=>PayrollService.PaymentDetails(x,payrollById[x.PayrollId])).ToArray();
        var payrollThis=payrollRows.Where(x=>x.FinancialMonthId==month.Id).ToArray();
        var earlierIds=payrollRows.Where(x=>x.FinancialMonthId!=month.Id).Select(x=>x.Id).ToHashSet();
        var employeeOpening=payrollRows.Where(x=>earlierIds.Contains(x.Id)).Sum(x=>x.NetAmount)-employeePayments.Where(x=>x.FinancialMonthId!=month.Id).Sum(x=>x.Amount);
        var basicEmployeeIds=payrollThis.Where(x=>x.CommissionSummaryId==null).Select(x=>x.EmployeeId).ToHashSet();
        var pendingBasic=salaries.Where(s=>!basicEmployeeIds.Contains(s.EmployeeId)).Sum(x=>x.PayableBasicSalary);
        var salesPeriods=await Bounded(db.CommissionPeriods.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.StartBusinessDate!=null&&x.EndBusinessDate>=month.StartDate&&x.StartBusinessDate<=month.EndDate).OrderBy(x=>x.StartBusinessDate).ThenBy(x=>x.Id),ct);
        var performance=new List<CommissionProgress>();var salesSources=new List<CommissionSourceLine>();var refundSources=new List<CommissionRefundSource>();
        var commissionEngine=new CommissionService(db,current,access);
        foreach(var period in salesPeriods)
        {
            if(await db.Payments.CountAsync(x=>x.BranchId==month.BranchId&&x.BusinessDate>=period.StartBusinessDate&&x.BusinessDate<=period.EndBusinessDate,ct)>MaxRows)
                throw new BusinessRuleException("Commission evaluation period exceeds bounded report size.");
            var result=await commissionEngine.FinancialReportAsync(period,ct);performance.Add(result.Progress);salesSources.AddRange(result.Sources);refundSources.AddRange(result.Refunds);
            if(salesSources.Count>MaxRows||refundSources.Count>MaxRows)throw new BusinessRuleException("Commission report exceeds bounded source size.");
        }
        var coachEarnings=await Bounded(db.CoachEarnings.AsNoTracking().Include(x=>x.PrivateSession!.PrivateSubscriptionTransaction).Include(x=>x.PrivateSession!.PrivateCoachAssignment).Include(x=>x.PrivateExpirySettlement!.PrivateSubscriptionTransaction).Include(x=>x.PrivateExpirySettlement!.PrivateCoachAssignment).Where(x=>x.BranchId==month.BranchId&&x.CreatedAt<=financialCutoff&&
            (x.FinancialMonthId!=null?x.FinancialMonth!.StartDate<=month.StartDate:x.BusinessDate<=month.EndDate)).OrderBy(x=>x.CreatedAt).ThenBy(x=>x.Id),ct);
        var coachPayouts=await Bounded(db.CoachPayouts.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.PaidAt<=financialCutoff&&
            db.FinancialTransactions.Any(t=>t.ExpenseId==x.ExpenseId&&(t.FinancialMonthId!=null?t.FinancialMonth!.StartDate<=month.StartDate:t.BusinessDate<=month.EndDate))).OrderBy(x=>x.PaidAt).ThenBy(x=>x.Id),ct);
        var currentPayoutExpenseIds=transactions.Where(x=>x.CoachPayoutId!=null&&x.Direction=="Debit").Select(x=>x.ExpenseId).ToHashSet();
        bool CurrentEarning(CoachEarning e)=>e.FinancialMonthId==month.Id||e.FinancialMonthId==null&&e.BusinessDate>=month.StartDate&&e.BusinessDate<=month.EndDate;
        var currentEarnings=coachEarnings.Where(CurrentEarning).ToArray();
        var currentPayouts=coachPayouts.Where(x=>currentPayoutExpenseIds.Contains(x.ExpenseId)).ToArray();
        var classes=await Bounded(db.ClassSessions.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.BusinessDate>=month.StartDate&&x.BusinessDate<=month.EndDate&&x.CreatedAt<=cutoff)
            .OrderBy(x=>x.StartedAt).ThenBy(x=>x.Id).Select(x=>new ClassOccurrenceReportRow(x.Id,x.CoachEmployeeId,x.GroupClass.Name,x.BusinessDate,x.StartedAt,x.EndedAt,x.Status.ToString(),
                db.CheckIns.Count(a=>a.ClassSessionId==x.Id&&a.CheckedInAt<=cutoff&&a.CheckInType==CheckInType.Class&&a.SessionConsumed))),ct);
        var coachIds=coachEarnings.Select(x=>x.CoachEmployeeId).Concat(coachPayouts.Select(x=>x.CoachEmployeeId)).Concat(classes.Select(x=>x.CoachEmployeeId)).Distinct().ToArray();
        var coachNames=await db.Employees.Where(x=>coachIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.FullName,ct);
        var earningsByCoach=coachEarnings.ToLookup(x=>x.CoachEmployeeId);var payoutsByCoach=coachPayouts.ToLookup(x=>x.CoachEmployeeId);var classesByCoach=classes.ToLookup(x=>x.CoachEmployeeId);
        var coaches=coachIds.Order().Select(id=>
        {
            var earned=earningsByCoach[id].ToArray();var now=earned.Where(CurrentEarning).ToArray();var paid=payoutsByCoach[id].ToArray();
            var completed=classesByCoach[id].Where(x=>x.Status=="Completed"&&x.CompletedAt<=cutoff).ToArray();var attendees=completed.Sum(x=>x.Attendance);
            var opening=earned.Except(now).Sum(x=>x.Amount)-paid.Where(x=>!currentPayoutExpenseIds.Contains(x.ExpenseId)).Sum(x=>x.Amount);
            return new CoachMonthRow(id,coachNames[id],opening,now.Where(x=>x.PrivateSessionId!=null).Sum(x=>x.Amount),now.Where(x=>x.PrivateExpirySettlementId!=null).Sum(x=>x.Amount),
                now.Sum(x=>x.Amount),paid.Where(x=>currentPayoutExpenseIds.Contains(x.ExpenseId)).Sum(x=>x.Amount),earned.Sum(x=>x.Amount)-paid.Sum(x=>x.Amount),completed.Length,attendees,
                completed.Length==0?null:(decimal)attendees/completed.Length);
        }).ToArray();
        var advances=await Bounded(db.EmployeeAdvances.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&(x.FinancialMonthId==month.Id||x.FinancialMonthId==null&&x.BusinessDate>=month.StartDate&&x.BusinessDate<=month.EndDate))
            .OrderBy(x=>x.OccurredAt).ThenBy(x=>x.Id).Select(x=>new AdvanceDetails(x.Id,x.BranchId,x.ShiftId,x.ReceivingEmployeeId,x.ReceivingEmployee.FullName,x.RecordingEmployeeId,x.FinancialAccountId,x.FinancialAccount.AccountType,x.Amount,x.Reason,x.Reference,x.OccurredAt,x.BusinessDate)),ct);
        var counts=await Bounded(db.CashCounts.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.OccurredAt<=financialCutoff&&x.BusinessDate>=month.StartDate&&x.BusinessDate<=month.EndDate)
            .OrderBy(x=>x.ObservationOrder).Select(x=>new CashCountDetails(x.Id,x.BranchId,x.ShiftId,x.Kind,x.ExpectedCash,x.CountedCash,x.BookCash,x.Difference,x.OccurredAt,x.BusinessDate,x.CreatedByUserId,x.Reason,x.ObservationOrder)),ct);
        var shifts=await Bounded(db.Shifts.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.OpenedAt<=financialCutoff&&x.OpeningBusinessDate<=month.EndDate&&(x.ClosingBusinessDate==null||x.ClosingBusinessDate>=month.StartDate))
            .OrderBy(x=>x.OpenedAt).ThenBy(x=>x.Id).Select(x=>new ShiftDetails(x.Id,x.BranchId,x.EmployeeId,x.Employee==null?null:x.Employee.FullName,x.Status,x.Version,x.OpenedAt,x.ClosedAt,x.OpeningBusinessDate,x.ClosingBusinessDate,x.OpeningCash,x.ExpectedClosingCash,x.ActualClosingCash,x.Difference)),ct);
        var maintenanceRows=await Bounded(db.MaintenancePlanRevisions.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.RecordedAt<=cutoff&&!db.MaintenancePlanRevisions.Any(y=>y.PlanId==x.PlanId&&y.RecordedAt<=cutoff&&y.Revision>x.Revision)).OrderBy(x=>x.PlanId),ct);
        var maintenance=maintenanceRows.Select(x=>MaintenanceService.Forecast(x,month.EndDate)).ToArray();
        var categories=transactions.Where(x=>x.SourceType=="Expense"&&x.CoachPayoutId==null).GroupBy(x=>new{x.ExpenseCategoryId,Name=x.Category??"Legacy/uncategorized"}).Select(g=>new ExpenseCategoryTotal(g.Key.ExpenseCategoryId??Guid.Empty,g.Key.Name,g.Where(x=>x.Direction=="Debit").Sum(x=>x.Amount),g.Where(x=>x.Direction=="Credit").Sum(x=>x.Amount),g.Sum(x=>x.Direction=="Debit"?x.Amount:-x.Amount))).OrderBy(x=>x.Category).ToArray();
        decimal Sum(string source,string direction)=>transactions.Where(x=>x.SourceType==source&&x.Direction==direction).Sum(x=>x.Amount);
        var gross=Sum("Payment","Credit");var refunds=Sum("Refund","Debit");var ordinary=categories.Sum(x=>x.Net);var basic=salaries.Sum(x=>x.BasicSalary);
        var commissions=payrollThis.Sum(x=>x.CommissionAmount);var privateEarned=currentEarnings.Sum(x=>x.Amount);var credits=transactions.Where(x=>x.Direction=="Credit").Sum(x=>x.Amount);var debits=transactions.Where(x=>x.Direction=="Debit").Sum(x=>x.Amount);
        var drawer=preserved??await coordination.PositionAsync(month.BranchId,ct);
        var summary=new MonthlySummary(gross,refunds,gross-refunds,ordinary,basic,commissions,privateEarned,payments.Sum(x=>x.Amount),currentPayouts.Sum(x=>x.Amount),Sum("EmployeeAdvance","Debit"),credits,debits,credits-debits,
            gross-refunds-ordinary-basic-commissions-privateEarned,employeeOpening,payroll.Sum(x=>x.Outstanding)+pendingBasic,coaches.Sum(x=>x.OpeningOutstanding),coaches.Sum(x=>x.ClosingOutstanding),drawer.BookCash,drawer.ExpectedPhysicalCash,drawer.LastCountedCash,drawer.ExpectedPhysicalCash-drawer.BookCash,counts.Sum(x=>x.Difference),
            "Net member collections - net ordinary paid operating expenses - prorated basic salary - newly recognized finalized Sales commissions - newly recognized Private earnings. Payments/advances/maintenance forecasts are not additional earned costs.");
        var limitations=new List<string>{"Classes compensation: NotConfigured.","Open Sales commission progress is provisional and excluded from payable commissions until finalized and recognized.","Maintenance suggestions and externally funded employee payment records do not create ledger movements.","Salary eligibility uses employment/branch dates and dated status audit evidence; suspended/resigned days excluded, approved leave paid.","Sales performance retains full independent commission-period boundaries; it must not be summed as monthly cashflow.","Each dataset is bounded to 50000 rows; export is refused atomically if its size exceeds the configured implementation limit."};
        var undated=await db.FinancialTransactions.CountAsync(x=>x.BranchId==month.BranchId&&x.FinancialMonthId==null&&x.BusinessDate==null,ct);
        if(undated>0)limitations.Add($"{undated} legacy undated ledger movements cannot be assigned to this month. They remain in the all-time book drawer balance; explicit reconciliation is required.");
        return new(MonthlyFinanceService.Details(month),branchName,revision,cutoff,summary,transactions,categories,performance,salesSources,refundSources,salaries,payroll,payments,advances,coaches,
            currentEarnings.Select(x=>new CoachEarningReportRow(x.Id,x.CoachEmployeeId,x.CommissionPeriodId,x.FinancialMonthId,x.SaleTransactionId,x.PrivateSessionId,x.PrivateExpirySettlementId,x.EligibleValue,x.Rate,x.Amount,x.TransferredPackage,x.BusinessDate,x.CreatedAt,CoachAttribution(x))).ToArray(),
            currentPayouts.Select(x=>new CoachPaymentReportRow(x.Id,x.CoachEmployeeId,x.ExpenseId,x.Amount,x.BusinessDate,x.PaidAt)).ToArray(),classes,drawer,counts,shifts,maintenance,limitations);
    }
    private static string CoachAttribution(CoachEarning e)
    {
        var batch=e.PrivateSession?.PrivateSubscriptionTransaction??e.PrivateExpirySettlement?.PrivateSubscriptionTransaction;
        var assignment=e.PrivateSession?.PrivateCoachAssignment??e.PrivateExpirySettlement?.PrivateCoachAssignment;
        return JsonSerializer.Serialize(new{
            PurchasedBatch=batch==null?null:new{batch.Id,batch.PrivateSubscriptionId,batch.PackageNameSnapshot,batch.SessionsAdded,batch.NetAmount,batch.UnitSessionValue,batch.RemainderValue,batch.OriginalCoachEmployeeId,batch.NewEndDate},
            Delivery=e.PrivateSession==null?null:new{e.PrivateSession.Id,e.PrivateSession.SessionNumber,e.PrivateSession.SessionValue,e.PrivateSession.SessionAt,e.PrivateSession.CoachEmployeeId,e.PrivateSession.PrivateCoachAssignmentId,e.PrivateSession.GymCheckInId},
            UnusedExpiry=e.PrivateExpirySettlement==null?null:new{e.PrivateExpirySettlement.Id,e.PrivateExpirySettlement.RemainingSessions,e.PrivateExpirySettlement.UnitSessionValue,e.PrivateExpirySettlement.EligibleAmount,e.PrivateExpirySettlement.CoachEmployeeId,e.PrivateExpirySettlement.PrivateCoachAssignmentId,e.PrivateExpirySettlement.SettledAt,e.PrivateExpirySettlement.CreatedByUserId,e.PrivateExpirySettlement.Reason},
            Assignment=assignment==null?null:new{assignment.Id,assignment.CoachEmployeeId,assignment.AssignedAt,assignment.TransferReason},e.TransferredPackage,e.EligibleValue,e.Rate,e.Amount,e.Target,e.TargetProgress
        });
    }

}
