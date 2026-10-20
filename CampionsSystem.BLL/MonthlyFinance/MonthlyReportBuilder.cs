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
        var eligiblePayroll=db.Payrolls.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.FinancialMonthId!=null&&x.FinancialMonth!.StartDate<=month.StartDate&&x.CreatedAt<=financialCutoff);
        var priorPayrollTotal=await eligiblePayroll.Where(x=>x.FinancialMonthId!=month.Id).SumAsync(x=>x.NetAmount,ct);
        var payrollThis=await Bounded(eligiblePayroll.Where(x=>x.FinancialMonthId==month.Id).Include(x=>x.Employee)
            .OrderBy(x=>x.CreatedAt).ThenBy(x=>x.Id),ct);
        var eligiblePayments=db.PayrollPaymentRecords.AsNoTracking().Where(x=>x.Payroll.BranchId==month.BranchId&&x.Payroll.FinancialMonthId!=null&&
            x.Payroll.FinancialMonth!.StartDate<=month.StartDate&&x.Payroll.CreatedAt<=financialCutoff&&x.FinancialMonth.StartDate<=month.StartDate&&x.RecordedAt<=financialCutoff);
        var priorPaymentTotal=await eligiblePayments.Where(x=>x.FinancialMonthId!=month.Id).SumAsync(x=>x.Amount,ct);
        var employeePayments=await Bounded(eligiblePayments.Where(x=>x.FinancialMonthId==month.Id).Include(x=>x.Payroll)
            .OrderBy(x=>x.RecordedAt).ThenBy(x=>x.Id),ct);
        var payrollIds=payrollThis.Select(x=>x.Id).ToArray();
        var paidByPayroll=await eligiblePayments.Where(x=>payrollIds.Contains(x.PayrollId)).GroupBy(x=>x.PayrollId)
            .Select(x=>new{x.Key,Amount=x.Sum(p=>p.Amount)}).ToDictionaryAsync(x=>x.Key,x=>x.Amount,ct);
        var payroll=payrollThis.Select(x=>PayrollService.Details(x,x.Employee.FullName,paidByPayroll.GetValueOrDefault(x.Id))).ToArray();
        var payments=employeePayments.Select(x=>PayrollService.PaymentDetails(x,x.Payroll)).ToArray();
        var employeeOpening=priorPayrollTotal-priorPaymentTotal;
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
        var currentPayoutExpenseIds=transactions.Where(x=>x.CoachPayoutId!=null&&x.Direction=="Debit").Select(x=>x.ExpenseId).ToHashSet();
        var eligibleEarnings=db.CoachEarnings.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.CreatedAt<=financialCutoff&&
            (x.FinancialMonthId!=null?x.FinancialMonth!.StartDate<=month.StartDate:x.BusinessDate<=month.EndDate));
        var priorEarnings=await eligibleEarnings.Where(x=>x.FinancialMonthId!=null?x.FinancialMonthId!=month.Id:x.BusinessDate<month.StartDate)
            .GroupBy(x=>x.CoachEmployeeId).Select(x=>new{x.Key,Amount=x.Sum(e=>e.Amount)}).ToDictionaryAsync(x=>x.Key,x=>x.Amount,ct);
        var currentEarnings=await Bounded(eligibleEarnings.Where(x=>x.FinancialMonthId==month.Id||x.FinancialMonthId==null&&x.BusinessDate>=month.StartDate&&x.BusinessDate<=month.EndDate)
            .Include(x=>x.PrivateSession!.PrivateSubscriptionTransaction).Include(x=>x.PrivateSession!.PrivateCoachAssignment)
            .Include(x=>x.PrivateExpirySettlement!.PrivateSubscriptionTransaction).Include(x=>x.PrivateExpirySettlement!.PrivateCoachAssignment)
            .OrderBy(x=>x.CreatedAt).ThenBy(x=>x.Id),ct);
        var eligiblePayouts=db.CoachPayouts.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.PaidAt<=financialCutoff&&
            db.FinancialTransactions.Any(t=>t.ExpenseId==x.ExpenseId&&(t.FinancialMonthId!=null?t.FinancialMonth!.StartDate<=month.StartDate:t.BusinessDate<=month.EndDate)));
        var priorPayouts=await eligiblePayouts.Where(x=>!currentPayoutExpenseIds.Contains(x.ExpenseId))
            .GroupBy(x=>x.CoachEmployeeId).Select(x=>new{x.Key,Amount=x.Sum(p=>p.Amount)}).ToDictionaryAsync(x=>x.Key,x=>x.Amount,ct);
        var currentPayouts=await Bounded(eligiblePayouts.Where(x=>currentPayoutExpenseIds.Contains(x.ExpenseId)).OrderBy(x=>x.PaidAt).ThenBy(x=>x.Id),ct);
        var classes=await Bounded(db.ClassSessions.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.BusinessDate>=month.StartDate&&x.BusinessDate<=month.EndDate&&x.CreatedAt<=cutoff)
            .OrderBy(x=>x.StartedAt).ThenBy(x=>x.Id).Select(x=>new ClassOccurrenceReportRow(x.Id,x.CoachEmployeeId,x.GroupClass.Name,x.BusinessDate,x.StartedAt,x.EndedAt,x.Status.ToString(),
                db.CheckIns.Count(a=>a.ClassSessionId==x.Id&&a.CheckedInAt<=cutoff&&a.CheckInType==CheckInType.Class&&a.SessionConsumed))),ct);
        var coachIds=priorEarnings.Keys.Concat(priorPayouts.Keys).Concat(currentEarnings.Select(x=>x.CoachEmployeeId)).Concat(currentPayouts.Select(x=>x.CoachEmployeeId)).Concat(classes.Select(x=>x.CoachEmployeeId)).Distinct().ToArray();
        var coachNames=await db.Employees.Where(x=>coachIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.FullName,ct);
        var earningsByCoach=currentEarnings.ToLookup(x=>x.CoachEmployeeId);var payoutsByCoach=currentPayouts.ToLookup(x=>x.CoachEmployeeId);var classesByCoach=classes.ToLookup(x=>x.CoachEmployeeId);
        var coaches=coachIds.Order().Select(id=>
        {
            var now=earningsByCoach[id].ToArray();var paid=payoutsByCoach[id].ToArray();
            var completed=classesByCoach[id].Where(x=>x.Status=="Completed"&&x.CompletedAt<=cutoff).ToArray();var attendees=completed.Sum(x=>x.Attendance);
            var opening=priorEarnings.GetValueOrDefault(id)-priorPayouts.GetValueOrDefault(id);
            return new CoachMonthRow(id,coachNames[id],opening,now.Where(x=>x.PrivateSessionId!=null).Sum(x=>x.Amount),now.Where(x=>x.PrivateExpirySettlementId!=null).Sum(x=>x.Amount),
                now.Sum(x=>x.Amount),paid.Sum(x=>x.Amount),opening+now.Sum(x=>x.Amount)-paid.Sum(x=>x.Amount),completed.Length,attendees,
                completed.Length==0?null:(decimal)attendees/completed.Length);
        }).ToArray();
        var advances=await Bounded(db.EmployeeAdvances.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&(x.FinancialMonthId==month.Id||x.FinancialMonthId==null&&x.BusinessDate>=month.StartDate&&x.BusinessDate<=month.EndDate))
            .OrderBy(x=>x.OccurredAt).ThenBy(x=>x.Id).Select(x=>new AdvanceDetails(x.Id,x.BranchId,x.ShiftId,x.ReceivingEmployeeId,x.ReceivingEmployee.FullName,x.RecordingEmployeeId,x.FinancialAccountId,x.FinancialAccount.AccountType,x.Amount,x.Reason,x.Reference,x.OccurredAt,x.BusinessDate,x.RecordingEmployee.FullName,x.FinancialAccount.Name)),ct);
        var counts=await Bounded(db.CashCounts.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.OccurredAt<=financialCutoff&&x.BusinessDate>=month.StartDate&&x.BusinessDate<=month.EndDate)
            .OrderBy(x=>x.ObservationOrder).Select(x=>new CashCountDetails(x.Id,x.BranchId,x.ShiftId,x.Kind,x.ExpectedCash,x.CountedCash,x.BookCash,x.Difference,x.OccurredAt,x.BusinessDate,x.CreatedByUserId,x.Reason,x.ObservationOrder)),ct);
        var shiftRows=await Bounded(db.Shifts.AsNoTracking().Include(x=>x.Employee).Where(x=>x.BranchId==month.BranchId&&x.OpenedAt<=financialCutoff&&x.OpeningBusinessDate<=month.EndDate&&(x.ClosingBusinessDate==null||x.ClosingBusinessDate>=month.StartDate))
            .OrderBy(x=>x.OpenedAt).ThenBy(x=>x.Id),ct);
        var reportActors=await ActorDisplayNames.LoadAsync(db,shiftRows.Select(x=>x.ClosedByUserId).Concat(counts.Select(x=>(Guid?)x.ActorId)),ct);
        counts=counts.Select(x=>x with{ActorName=ActorDisplayNames.Get(reportActors,x.ActorId)}).ToList();
        var shifts=shiftRows.Select(x=>ShiftService.DetailsOf(x,x.Employee?.FullName,branchName,
            ActorDisplayNames.Get(reportActors,x.ClosedByUserId))).ToArray();
        var maintenanceRows=await Bounded(db.MaintenancePlanRevisions.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.RecordedAt<=cutoff&&!db.MaintenancePlanRevisions.Any(y=>y.PlanId==x.PlanId&&y.RecordedAt<=cutoff&&y.Revision>x.Revision)).OrderBy(x=>x.PlanId),ct);
        var maintenance=maintenanceRows.Select(x=>MaintenanceService.Forecast(x,month.EndDate)).ToArray();
        var categories=transactions.Where(x=>x.SourceType=="Expense"&&x.CoachPayoutId==null).GroupBy(x=>new{x.ExpenseCategoryId,Name=x.Category??"Legacy/uncategorized"}).Select(g=>new ExpenseCategoryTotal(g.Key.ExpenseCategoryId??Guid.Empty,g.Key.Name,g.Where(x=>x.Direction=="Debit").Sum(x=>x.Amount),g.Where(x=>x.Direction=="Credit").Sum(x=>x.Amount),g.Sum(x=>x.Direction=="Debit"?x.Amount:-x.Amount))).OrderBy(x=>x.Category).ToArray();
        decimal Sum(string source,string direction)=>transactions.Where(x=>x.SourceType==source&&x.Direction==direction).Sum(x=>x.Amount);
        var gross=Sum("Payment","Credit");var refunds=Sum("Refund","Debit");var ordinary=categories.Sum(x=>x.Net);var basic=salaries.Sum(x=>x.BasicSalary);
        var commissions=payrollThis.Sum(x=>x.CommissionAmount);var privateEarned=currentEarnings.Sum(x=>x.Amount);var credits=transactions.Where(x=>x.Direction=="Credit").Sum(x=>x.Amount);var debits=transactions.Where(x=>x.Direction=="Debit").Sum(x=>x.Amount);
        var drawer=preserved??await coordination.PositionAsync(month.BranchId,ct);
        var summary=new MonthlySummary(gross,refunds,gross-refunds,ordinary,basic,commissions,privateEarned,payments.Sum(x=>x.Amount),currentPayouts.Sum(x=>x.Amount),Sum("EmployeeAdvance","Debit"),credits,debits,credits-debits,
            gross-refunds-ordinary-basic-commissions-privateEarned,employeeOpening,priorPayrollTotal+payrollThis.Sum(x=>x.NetAmount)-priorPaymentTotal-payments.Sum(x=>x.Amount)+pendingBasic,coaches.Sum(x=>x.OpeningOutstanding),coaches.Sum(x=>x.ClosingOutstanding),drawer.BookCash,drawer.ExpectedPhysicalCash,drawer.LastCountedCash,drawer.ExpectedPhysicalCash-drawer.BookCash,counts.Sum(x=>x.Difference),
            "Net member collections - net ordinary paid operating expenses - prorated basic salary - newly recognized finalized Sales commissions - newly recognized Private earnings. Payments/advances/maintenance forecasts are not additional earned costs.");
        var limitations=new List<string>{"Classes compensation: NotConfigured.","Open Sales commission progress is provisional and excluded from payable commissions until finalized and recognized.","Maintenance suggestions and externally funded employee payment records do not create ledger movements.","Salary eligibility uses employment/branch dates and dated status audit evidence; suspended/resigned days excluded, approved leave paid.","Sales performance retains full independent commission-period boundaries; it must not be summed as monthly cashflow.","Current-period detail datasets are bounded to 50000 rows; export is refused atomically if a detail dataset exceeds the configured implementation limit."};
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
