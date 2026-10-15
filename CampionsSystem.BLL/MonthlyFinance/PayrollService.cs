using System.Text.Json;
using System.Buffers.Binary;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.MonthlyFinance;

public sealed class PayrollService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private readonly FinancialOperations coordination = new(db, current, access);
    internal static PayrollDetails Details(Payroll p, string name, decimal paid) => new(p.Id, p.EmployeeId, name, p.BranchId,
        p.FinancialMonthId, p.CommissionPeriodId, p.CommissionSummaryId, p.BasicSalary, p.CommissionAmount, p.DeductionAmount,
        p.NetAmount, paid, p.NetAmount - paid, p.NetAmount == 0 ? "NoAmountDue" : paid == p.NetAmount ? "Paid" : paid > 0 ? "PartiallyPaid" : "Outstanding");
    internal async Task RecognizeBasicAsync(FinancialMonth month, IReadOnlyList<SalaryEntitlement> salaries, Guid actor, CancellationToken ct)
    {
        foreach (var salary in salaries)
        {
            if (await db.Payrolls.AnyAsync(x => x.FinancialMonthId == month.Id && x.EmployeeId == salary.EmployeeId && x.CommissionSummaryId == null, ct)) continue;
            var payroll = new Payroll { EmployeeId = salary.EmployeeId, BranchId = month.BranchId, FinancialMonthId = month.Id,
                BasicSalary = salary.BasicSalary, DeductionAmount = salary.Advances, NetAmount = salary.PayableBasicSalary,
                SalaryEvidenceJson = JsonSerializer.Serialize(salary), RecognizedByUserId = actor, CreatedAt = access.Now };
            db.Add(payroll);
            var advances = await db.EmployeeAdvances.AsNoTracking().Where(x => x.ReceivingEmployeeId == salary.EmployeeId && x.BranchId == month.BranchId &&
                (x.FinancialMonthId == month.Id || x.FinancialMonthId == null && x.BusinessDate >= month.StartDate && x.BusinessDate <= month.EndDate)).ToListAsync(ct);
            foreach (var advance in advances)
            {
                if (await db.PayrollDeductions.AnyAsync(x => x.EmployeeAdvanceId == advance.Id, ct)) throw new BusinessRuleException("Advance already deducted from another entitlement; explicit historical reconciliation required.");
                db.Add(new PayrollDeduction { Payroll = payroll, EmployeeAdvanceId = advance.Id, Amount = advance.Amount,
                    Reason = "Branch basic salary advance", CreatedByUserId = actor, CreatedAt = access.Now });
            }
            access.Audit(actor, month.BranchId, "Payroll.BasicRecognized", nameof(Payroll), payroll.Id, after: new { month.Id, salary.EmployeeId, salary.BasicSalary, salary.Advances, salary.PayableBasicSalary });
        }
        await db.SaveChangesAsync(ct);
    }
    internal async Task<IReadOnlyList<Payroll>> RecognizeCommissionsAsync(FinancialMonth month, Guid actor, Guid? period, CancellationToken ct)
    {
        var summaries = await db.EmployeeCommissionSummaries.AsNoTracking().Where(x => x.BranchId == month.BranchId &&
            (x.CommissionType == CommissionType.SalesNew || x.CommissionType == CommissionType.SalesRenewal) && x.CommissionPeriod.Status == CommissionPeriodStatus.Closed &&
            (period == null || x.CommissionPeriodId == period) && !db.Payrolls.Any(p => p.CommissionSummaryId == x.Id)).OrderBy(x => x.Id).Take(10001).ToListAsync(ct);
        if (summaries.Count > 10000) throw new BusinessRuleException("Too many unrecognized commission summaries; recognize periods in bounded batches.");
        var result = new List<Payroll>();
        foreach (var s in summaries)
        {
            var p = new Payroll { EmployeeId = s.EmployeeId, BranchId = month.BranchId, FinancialMonthId = month.Id, CommissionSummaryId = s.Id,
                CommissionPeriodId = s.CommissionPeriodId, CommissionAmount = s.CommissionAmount, NetAmount = s.CommissionAmount, RecognizedByUserId = actor, CreatedAt = access.Now };
            db.Add(p); result.Add(p);
            access.Audit(actor, month.BranchId, "Payroll.CommissionRecognized", nameof(Payroll), p.Id, after: new { SummaryId = s.Id, s.EmployeeId, s.CommissionAmount, s.CommissionType, MonthId = month.Id });
        }
        await db.SaveChangesAsync(ct); return result;
    }
    public Task<IReadOnlyList<PayrollDetails>> RecognizeCommissionPeriodAsync(RecognizePayrollCommissionsRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync<IReadOnlyList<PayrollDetails>>(async () =>
    {
        FinancialOperations.Operation(r.OperationId);
        var period = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(x => x.Id == r.CommissionPeriodId, ct) ?? throw new BranchNotFoundException("Commission period not found.");
        var actor = await access.RequireAsync("Payroll.Manage", period.BranchId, ct); await access.RequireAsync("Branches.AccessAll", period.BranchId, ct);
        // The marker's OperationId is global, while the drawer lock is branch-local.
        // A transaction-scoped advisory lock serializes retries before either branch
        // checks the marker. A key collision only delays an unrelated operation.
        var operationKey = BinaryPrimitives.ReadInt64LittleEndian(r.OperationId.ToByteArray());
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({operationKey})", ct);
        await coordination.LockBranchAsync(period.BranchId, ct);
        var hash = FinancialOperations.Hash(r);
        var operation = await db.AuditLogs.AsNoTracking().SingleOrDefaultAsync(x => x.Action == "Payroll.RecognitionOperation" && x.EntityId == r.OperationId, ct);
        if (operation != null)
        {
            using var data = JsonDocument.Parse(operation.NewValues!);
            FinancialOperations.Retry(data.RootElement.GetProperty("Hash").GetString(), hash, operation.UserId!.Value, actor);
            return JsonSerializer.Deserialize<PayrollDetails[]>(data.RootElement.GetProperty("Result").GetRawText())!;
        }
        if (period.Status != CommissionPeriodStatus.Closed) throw new BusinessRuleException("Only existing finalized Sales commission statements become payable; progress is provisional.");
        var (date, _) = await coordination.BusinessDateAsync(period.BranchId, ct);
        var month = await FinancialMonthPosting.ResolveAsync(db, period.BranchId, date, ct) ?? throw new BusinessRuleException("Configure an open branch financial month first.");
        var rows = await RecognizeCommissionsAsync(month, actor, period.Id, ct);
        var result = new List<PayrollDetails>();
        foreach (var row in rows) result.Add(Details(row, await db.Employees.Where(x => x.Id == row.EmployeeId).Select(x => x.FullName).SingleAsync(ct), 0));
        access.Audit(actor, period.BranchId, "Payroll.RecognitionOperation", nameof(CommissionPeriod), r.OperationId, after: new { Hash = hash, Result = result });
        return result;
    }, ct);
    public async Task<ShiftPage<PayrollDetails>> HistoryAsync(Guid? branchId = null, Guid? employeeId = null, Guid? monthId = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        MonthlyFinanceService.Page(page, pageSize); var (branch, _) = await coordination.ResolveAsync(MonthlyFinancePermissions.PayrollView, branchId, ct);
        var q = db.Payrolls.AsNoTracking().Where(x => x.BranchId == branch && x.FinancialMonthId != null);
        if (employeeId != null) q = q.Where(x => x.EmployeeId == employeeId); if (monthId != null) q = q.Where(x => x.FinancialMonthId == monthId);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id).Skip((page-1)*pageSize).Take(pageSize)
            .Select(x => new { Row = x, Name = x.Employee.FullName, Paid = db.PayrollPaymentRecords.Where(p => p.PayrollId == x.Id).Sum(p => p.Amount) }).ToListAsync(ct);
        return new(rows.Select(x => Details(x.Row, x.Name, x.Paid)).ToArray(), total, page, pageSize);
    }
    public Task<PayrollPaymentDetails> RecordPaymentAsync(Guid payrollId, RecordPayrollPaymentRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.Amount, true); FinancialOperations.Reason(r.Reason);
        if (r.PaidAt == default || r.PaidAt > access.Now || r.Reference?.Length > 512) throw new BusinessRuleException("An actual past/current payment date and reference up to 512 characters are required.");
        var p = await db.Payrolls.AsNoTracking().SingleOrDefaultAsync(x => x.Id == payrollId, ct) ?? throw new BranchNotFoundException("Payroll entitlement not found.");
        var actor = await access.RequireAsync(MonthlyFinancePermissions.PayrollPay, p.BranchId, ct); await access.RequireAsync("Branches.AccessAll", p.BranchId, ct);
        await coordination.LockBranchAsync(p.BranchId, ct); var hash = FinancialOperations.Hash(new { payrollId, r });
        var prior = await db.PayrollPaymentRecords.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
        if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.ActorUserId, actor); return PaymentDetails(prior, p); }
        if (p.FinancialMonthId == null) throw new BusinessRuleException("Legacy payroll requires explicit reconciliation; no paid status or funding is inferred.");
        var (date, _) = await coordination.BusinessDateAsync(p.BranchId, ct);
        var month = await FinancialMonthPosting.ResolveAsync(db, p.BranchId, date, ct) ?? throw new BusinessRuleException("Configure an open financial month first.");
        var paid = await db.PayrollPaymentRecords.Where(x => x.PayrollId == payrollId).SumAsync(x => x.Amount, ct); var balance = p.NetAmount - paid;
        if (r.Amount > balance) throw new BusinessRuleException("Payment exceeds this employee's outstanding entitlement at this branch.");
        var record = new PayrollPaymentRecord { PayrollId = p.Id, FinancialMonthId = month.Id, OperationId = r.OperationId, RequestHash = hash,
            Amount = r.Amount, BalanceBefore = balance, BalanceAfter = balance - r.Amount, PaidAt = r.PaidAt, RecordedAt = access.Now,
            BusinessDate = date, ActorUserId = actor, Reason = r.Reason.Trim(), Reference = r.Reference };
        db.Add(record); access.Audit(actor, p.BranchId, "Payroll.PaymentRecorded", nameof(PayrollPaymentRecord), record.Id,
            after: new { payrollId, p.EmployeeId, month.Id, r.Amount, r.PaidAt, record.BalanceBefore, record.BalanceAfter, r.Reference, r.Reason, LedgerPosted = false });
        return PaymentDetails(record, p);
    }, ct);
    internal static PayrollPaymentDetails PaymentDetails(PayrollPaymentRecord r, Payroll p) => new(r.Id, p.Id, p.EmployeeId, p.BranchId, r.FinancialMonthId,
        r.Amount, r.BalanceBefore, r.BalanceAfter, r.PaidAt, r.RecordedAt, r.BusinessDate, r.ActorUserId, r.Reason, r.Reference);
    public async Task<ShiftPage<PayrollPaymentDetails>> PaymentsAsync(Guid payrollId, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        MonthlyFinanceService.Page(page, pageSize); var p = await db.Payrolls.AsNoTracking().SingleOrDefaultAsync(x => x.Id == payrollId, ct) ?? throw new BranchNotFoundException("Payroll entitlement not found.");
        await access.RequireAsync(MonthlyFinancePermissions.PayrollView, p.BranchId, ct);
        var q = db.PayrollPaymentRecords.AsNoTracking().Where(x => x.PayrollId == payrollId); var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.RecordedAt).ThenBy(x => x.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(x => PaymentDetails(x, p)).ToArray(), total, page, pageSize);
    }
}
