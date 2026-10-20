using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Commissions;
public sealed partial class CommissionService
{
    internal static decimal Earned(decimal eligible, decimal rate) => decimal.Round(eligible * rate / 100m, 2, MidpointRounding.AwayFromZero);
    private async Task<List<CommissionSourceLine>> LinesAsync(CommissionPeriod period, CancellationToken ct)
    {
        if (period.Status == CommissionPeriodStatus.Closed)
            return await db.CommissionStatementLines.AsNoTracking().Where(l => l.CommissionPeriodId == period.Id).Select(l =>
                new CommissionSourceLine(l.PaymentId, l.Payment.SaleTransactionId, l.Payment.SaleTransaction.MemberId, period.BranchId,
                    l.Payment.SaleTransaction.SalesEmployeeId, l.EmployeeId, l.Category == null ? null : l.Category.ToString(), l.EligibilityReason,
                    l.Payment.BusinessDate, l.Payment.PaidAt, l.GrossAmount, l.RefundedAmount, l.GrossAmount - l.RefundedAmount, l.EligibleAmount)).ToListAsync(ct);
        var periodPayments = await db.Payments.AsNoTracking().Where(p => p.BranchId == period.BranchId && p.Status == PaymentStatus.Completed
            && p.BusinessDate >= period.StartBusinessDate && p.BusinessDate <= period.EndBusinessDate).ToListAsync(ct);
        var saleIds = periodPayments.Select(p => p.SaleTransactionId).Distinct().ToArray();
        var allPayments = await db.Payments.AsNoTracking().Where(p => saleIds.Contains(p.SaleTransactionId) && p.Status == PaymentStatus.Completed).ToListAsync(ct);
        var sales = await db.SaleTransactions.AsNoTracking().Include(s => s.MembershipSnapshot).Where(s => saleIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, ct);
        var paymentIds = periodPayments.Select(p => p.Id).ToArray();
        var evidence = await db.CommissionCollections.AsNoTracking().Where(e => paymentIds.Contains(e.PaymentId)).ToDictionaryAsync(e => e.PaymentId, ct);
        var sources = new CommissionSources(db, access); var refunds = await sources.RefundedAsync(allPayments, ct); var result = new List<CommissionSourceLine>();
        foreach (var p in periodPayments)
        {
            var sale = sales[p.SaleTransactionId]; evidence.TryGetValue(p.Id, out var fact);
            var (employee, reason) = fact == null ? await sources.AttributionAsync(sale, p.BusinessDate, p.PaidAt, ct) : (fact.EmployeeId, fact.EligibilityReason);
            var refunded = refunds.GetValueOrDefault(p.Id); var net = p.Amount - refunded;
            result.Add(new(p.Id, sale.Id, sale.MemberId, p.BranchId, sale.SalesEmployeeId, employee, (fact?.Category ?? sale.SaleCategory)?.ToString(), reason,
                p.BusinessDate, p.PaidAt, p.Amount, refunded, net, reason == "Eligible" ? net : 0));
        }
        return result;
    }
    private async Task<CommissionSourceLine[]> WithSourceNamesAsync(IEnumerable<CommissionSourceLine> source,CancellationToken ct)
    {
        var rows=source.ToArray();
        if(rows.Length==0)return rows;
        var memberIds=rows.Select(x=>x.MemberId).Distinct().ToArray();
        var members=await db.Members.AsNoTracking().Where(x=>memberIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.FullName,x.MemberCode}).ToDictionaryAsync(x=>x.Id,ct);
        var employeeIds=rows.SelectMany(x=>new[]{x.OriginalSalesEmployeeId,x.EmployeeId}).Where(x=>x.HasValue)
            .Select(x=>x!.Value).Distinct().ToArray();
        var employees=await db.Employees.AsNoTracking().Where(x=>employeeIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.FullName}).ToDictionaryAsync(x=>x.Id,x=>x.FullName,ct);
        return rows.Select(x=>x with{
            MemberName=members.GetValueOrDefault(x.MemberId)?.FullName,
            MemberCode=members.GetValueOrDefault(x.MemberId)?.MemberCode,
            OriginalSalesEmployeeName=x.OriginalSalesEmployeeId.HasValue?employees.GetValueOrDefault(x.OriginalSalesEmployeeId.Value):null,
            EmployeeName=x.EmployeeId.HasValue?employees.GetValueOrDefault(x.EmployeeId.Value):null
        }).ToArray();
    }
    private static CommissionCategoryProgress Calculate(string category, IReadOnlyList<CommissionSourceLine> lines, CommissionRule? rule)
    {
        var gross = lines.Sum(x => x.GrossCollection); var refunds = lines.Sum(x => x.Refunds); var eligible = lines.Sum(x => x.EligibleCollection);
        var configured = rule?.CalculationMethod == CommissionCalculationMethod.TargetPercentage && rule.TargetAmount > 0 && rule.BelowTargetRate != null && rule.ReachedTargetRate != null;
        var reached = configured && eligible >= rule!.TargetAmount; var rate = configured ? reached ? rule!.ReachedTargetRate : rule!.BelowTargetRate : null;
        return new(category, gross, refunds, eligible, configured, rule?.TargetAmount, reached, rule?.BelowTargetRate, rule?.ReachedTargetRate,
            rate, rate.HasValue ? Earned(eligible, rate.Value) : 0, rule?.Id);
    }
    private async Task<List<EmployeeCommissionStatement>> StatementsAsync(CommissionPeriod period, List<CommissionSourceLine> lines, CancellationToken ct)
    {
        var result = new List<EmployeeCommissionStatement>();
        decimal House(Guid employee) => lines.Where(l => l.OriginalSalesEmployeeId == employee && l.EligibilityReason is "House" or "EmploymentEnded" or "EmploymentInactive").Sum(l => l.RetainedCollection);
        decimal Excluded(Guid employee) => lines.Where(l => l.OriginalSalesEmployeeId == employee && l.EligibilityReason is not ("Eligible" or "House" or "EmploymentEnded" or "EmploymentInactive")).Sum(l => l.RetainedCollection);
        if (period.Status == CommissionPeriodStatus.Closed)
        {
            var summaries = await db.EmployeeCommissionSummaries.AsNoTracking().Include(s => s.Employee).Where(s => s.CommissionPeriodId == period.Id
                && (s.CommissionType == CommissionType.SalesNew || s.CommissionType == CommissionType.SalesRenewal)).ToListAsync(ct);
            foreach (var g in summaries.GroupBy(s => s.EmployeeId))
            {
                CommissionCategoryProgress Closed(CommissionType type)
                {
                    var s = g.Single(x => x.CommissionType == type); var cat = type == CommissionType.SalesNew ? "NewMembership" : "Renewal";
                    var source = lines.Where(l => l.EmployeeId == g.Key && l.Category == cat).ToArray();
                    return new(cat, source.Sum(x => x.GrossCollection), source.Sum(x => x.Refunds), s.TotalEligibleAmount, true, s.TargetAmount, s.TargetReached,
                        s.BelowTargetRateSnapshot, s.ReachedTargetRateSnapshot, s.AppliedRate, s.CommissionAmount, s.CommissionRuleId);
                }
                var n = Closed(CommissionType.SalesNew); var r = Closed(CommissionType.SalesRenewal);
                result.Add(new(g.Key, g.First().Employee.FullName, n, r, n.Commission + r.Commission, period.Id, true, House(g.Key), Excluded(g.Key)));
            }
            return result.OrderBy(x => x.EmployeeId).ToList();
        }
        var rules = await db.CommissionRules.AsNoTracking().Where(r => r.BranchId == period.BranchId && r.IsActive && r.EffectiveFrom <= period.StartBusinessDate
            && (r.EffectiveTo == null || r.EffectiveTo >= period.StartBusinessDate) && (r.CommissionType == CommissionType.SalesNew || r.CommissionType == CommissionType.SalesRenewal)).ToListAsync(ct);
        var employees = lines.Where(x => x.EmployeeId != null).Select(x => x.EmployeeId!.Value)
            .Concat(rules.Where(x => x.EmployeeId != null).Select(x => x.EmployeeId!.Value)).Distinct().ToArray();
        var names = await db.Employees.AsNoTracking().Where(e => employees.Contains(e.Id)).ToDictionaryAsync(e => e.Id, e => e.FullName, ct);
        foreach (var employee in employees.Order())
        {
            CommissionRule? Rule(CommissionType type) => rules.Where(r => r.CommissionType == type && (r.EmployeeId == employee || r.EmployeeId == null))
                .OrderByDescending(r => r.EmployeeId != null).ThenByDescending(r => r.EffectiveFrom).ThenByDescending(r => r.Revision).ThenByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id).FirstOrDefault();
            var n = Calculate("NewMembership", lines.Where(x => x.EmployeeId == employee && x.Category == "NewMembership").ToArray(), Rule(CommissionType.SalesNew));
            var r = Calculate("Renewal", lines.Where(x => x.EmployeeId == employee && x.Category == "Renewal").ToArray(), Rule(CommissionType.SalesRenewal));
            result.Add(new(employee, names[employee], n, r, n.Commission + r.Commission, period.Id, false, House(employee), Excluded(employee)));
        }
        return result;
    }
    // One snapshot across source, refund and configuration reads; no operational mutations.
    private async Task<T> SnapshotAsync<T>(Func<Task<T>> read, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction != null) return await read();
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
        var result = await read(); await tx.CommitAsync(ct); return result;
    }
    public Task<CommissionProgress> ProgressAsync(Guid periodId, int page = 1, int pageSize = 25, CancellationToken ct = default)
        => SnapshotAsync(() => ProgressCoreAsync(periodId, page, pageSize, ct), ct);
    private async Task<CommissionProgress> ProgressCoreAsync(Guid periodId, int page, int pageSize, CancellationToken ct)
    {
        Page(page, pageSize); var p = await ReadAsync(periodId, ct); var lines = await LinesAsync(p, ct); var statements = await StatementsAsync(p, lines, ct);
        return ReportProgress(p, lines, statements, page, pageSize);
    }
    private static CommissionProgress ReportProgress(CommissionPeriod p, List<CommissionSourceLine> lines, List<EmployeeCommissionStatement> statements, int page, int pageSize)
    {
        var house = lines.Where(l => l.EligibilityReason is "House" or "EmploymentEnded" or "EmploymentInactive").Sum(l => l.RetainedCollection);
        var eligible = lines.Sum(l => l.EligibleCollection); var retained = lines.Sum(l => l.RetainedCollection);
        return new(Details(p), p.Status == CommissionPeriodStatus.Closed, lines.Sum(l => l.GrossCollection), lines.Sum(l => l.Refunds), retained,
            eligible, house, lines.Where(l => l.EmployeeId == null && l.Category == "NewMembership" && l.EligibilityReason is "House" or "EmploymentEnded" or "EmploymentInactive").Sum(l => l.RetainedCollection),
            lines.Where(l => l.EmployeeId == null && l.Category == "Renewal" && l.EligibilityReason is "House" or "EmploymentEnded" or "EmploymentInactive").Sum(l => l.RetainedCollection),
            retained - eligible - house, lines.Where(l => l.Category == "NewMembership").Sum(l => l.EligibleCollection),
            lines.Where(l => l.Category == "Renewal").Sum(l => l.EligibleCollection), statements.Sum(s => s.TotalCommission), statements.Count,
            statements.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), page, pageSize);
    }
    // Branch-authorized financial composition reuses this calculation unchanged.
    internal async Task<(CommissionProgress Progress, IReadOnlyList<CommissionSourceLine> Sources, IReadOnlyList<CommissionRefundSource> Refunds)> FinancialReportAsync(CommissionPeriod period, CancellationToken ct)
    {
        var lines = await LinesAsync(period, ct); var statements = await StatementsAsync(period, lines, ct);
        var refunds = await db.CommissionRefundAllocations.AsNoTracking().Where(a => a.Payment.BranchId == period.BranchId &&
            a.Payment.BusinessDate >= period.StartBusinessDate && a.Payment.BusinessDate <= period.EndBusinessDate)
            .OrderBy(a => a.Refund.CreatedAt).ThenBy(a => a.RefundId).ThenBy(a => a.PaymentId)
            .Select(a => new CommissionRefundSource(a.RefundId,a.PaymentId,a.Amount,a.Payment.BusinessDate,a.Refund.BusinessDate,a.Refund.CreatedAt)).ToListAsync(ct);
        return (ReportProgress(period,lines,statements,1,Math.Max(1,statements.Count)),
            lines.OrderBy(x=>x.PaidAt).ThenBy(x=>x.PaymentId).ToArray(),refunds);
    }
    public Task<EmployeeCommissionStatement> EmployeeStatementAsync(Guid periodId, Guid employeeId, CancellationToken ct = default)
        => SnapshotAsync(() => EmployeeStatementCoreAsync(periodId, employeeId, ct), ct);
    private async Task<EmployeeCommissionStatement> EmployeeStatementCoreAsync(Guid periodId, Guid employeeId, CancellationToken ct)
    {
        var p = await ReadAsync(periodId, ct); var statements = await StatementsAsync(p, await LinesAsync(p, ct), ct);
        return statements.SingleOrDefault(s => s.EmployeeId == employeeId) ?? throw new Branches.BranchNotFoundException("Employee has no configuration or attributed collection in this period.");
    }
    public Task<CommissionPage<CommissionSourceLine>> SourcesAsync(Guid periodId, Guid? employeeId = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
        => SnapshotAsync(() => SourcesCoreAsync(periodId, employeeId, page, pageSize, ct), ct);
    private async Task<CommissionPage<CommissionSourceLine>> SourcesCoreAsync(Guid periodId, Guid? employeeId, int page, int pageSize, CancellationToken ct)
    {
        Page(page, pageSize); var p = await ReadAsync(periodId, ct); IEnumerable<CommissionSourceLine> lines = await LinesAsync(p, ct);
        if (employeeId.HasValue) lines = lines.Where(l => l.EmployeeId == employeeId || l.OriginalSalesEmployeeId == employeeId);
        var rows = lines.OrderByDescending(l => l.PaidAt).ThenByDescending(l => l.PaymentId).ToArray();
        return new(await WithSourceNamesAsync(rows.Skip((page - 1) * pageSize).Take(pageSize),ct), rows.Length, page, pageSize);
    }
    public Task<CommissionPage<CommissionRefundSource>> RefundSourcesAsync(Guid periodId, int page = 1, int pageSize = 25, CancellationToken ct = default)
        => SnapshotAsync(async () =>
        {
            Page(page, pageSize); var p = await ReadAsync(periodId, ct);
            var q = db.CommissionRefundAllocations.AsNoTracking().Where(a => a.Payment.BranchId == p.BranchId
                && a.Payment.BusinessDate >= p.StartBusinessDate && a.Payment.BusinessDate <= p.EndBusinessDate);
            var total = await q.CountAsync(ct);
            var rows = await q.OrderByDescending(a => a.Refund.CreatedAt).ThenBy(a => a.RefundId).ThenBy(a => a.PaymentId).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(a => new CommissionRefundSource(a.RefundId, a.PaymentId, a.Amount, a.Payment.BusinessDate, a.Refund.BusinessDate, a.Refund.CreatedAt)).ToListAsync(ct);
            return new CommissionPage<CommissionRefundSource>(rows, total, page, pageSize);
        }, ct);
    public Task<CommissionPeriodDetails> FinalizeAsync(Guid id, FinalizeCommissionPeriodRequest r, CancellationToken ct = default) => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId);
        var initial = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new Branches.BranchNotFoundException("Commission period not found.");
        var actor = await AdminAsync(CommissionPermissions.Close, initial.BranchId, ct);
        if (initial.StartBusinessDate == null || initial.EndBusinessDate == null) throw new BusinessRuleException("Legacy commission period needs explicit reconciliation.");
        await LockAsync(initial.BranchId, ct);
        var p = await db.CommissionPeriods.SingleAsync(x => x.Id == id, ct); await db.Entry(p).ReloadAsync(ct);
        var hash = FinancialOperations.Hash(new { Id = id, Request = r, Actor = actor });
        if (p.CloseOperationId == r.OperationId) { if (p.CloseRequestHash != hash) throw new BusinessRuleException("Operation belongs to another finalization request."); return Details(p); }
        if (p.Version != r.ExpectedVersion) throw new DbUpdateConcurrencyException("Period version is stale.");
        if (p.Status != CommissionPeriodStatus.Open) throw new BusinessRuleException("Period is already finalized.");
        var (date, _) = await coordination.BusinessDateAsync(p.BranchId, ct);
        if (date <= p.EndBusinessDate) throw new BusinessRuleException("Finalize only after the period's last effective BusinessDate has ended.");
        var lines = await LinesAsync(p, ct); var statements = await StatementsAsync(p, lines, ct);
        if (statements.Any(s => !s.NewClients.Configured || !s.Renewals.Configured)) throw new BusinessRuleException("Both independent rules must be configured for every attributed employee before finalization.");
        foreach (var line in lines) db.Add(new CommissionStatementLine { CommissionPeriodId = p.Id, PaymentId = line.PaymentId, EmployeeId = line.EmployeeId,
            Category = line.Category == null ? null : Enum.Parse<SaleCategory>(line.Category), EligibilityReason = line.EligibilityReason,
            GrossAmount = line.GrossCollection, RefundedAmount = line.Refunds, EligibleAmount = line.EligibleCollection });
        foreach (var statement in statements)
            foreach (var (type, category) in new[] { (CommissionType.SalesNew, statement.NewClients), (CommissionType.SalesRenewal, statement.Renewals) })
                db.Add(new EmployeeCommissionSummary { CommissionPeriodId = p.Id, EmployeeId = statement.EmployeeId, BranchId = p.BranchId, CommissionType = type,
                    TotalEligibleAmount = category.EligibleCollections, TargetAmount = category.Target, TargetReached = category.TargetReached,
                    AppliedRate = category.AppliedRate, CommissionAmount = category.Commission, CommissionRuleId = category.RuleId,
                    BelowTargetRateSnapshot = category.BelowRate, ReachedTargetRateSnapshot = category.ReachedRate, FinalizedAt = access.Now });
        p.Status = CommissionPeriodStatus.Closed; p.ClosedAt = access.Now; p.ClosedByUserId = actor; p.CloseOperationId = r.OperationId; p.CloseRequestHash = hash; p.Version++;
        access.Audit(actor, p.BranchId, "Commission.PeriodFinalized", nameof(CommissionPeriod), p.Id, after: new { Statements = statements.Count, Commission = statements.Sum(s => s.TotalCommission) });
        return Details(p);
    }, ct);
}
