using System.Text.Json;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Coaching;

public sealed partial class CoachingService
{
    public Task<CoachTargetDetails> ConfigureTargetsAsync(CoachTargetRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId);
            var period = await PeriodAsync(r.CommissionPeriodId, ct); var actor = await OwnerAsync(CoachingPermissions.Targets, period.BranchId, ct);
            await coordination.LockBranchAsync(period.BranchId, ct); var hash = FinancialOperations.Hash(r);
            var prior = await db.CoachTargetRules.AsNoTracking().SingleOrDefaultAsync(t => t.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return await TargetDetailsAsync(prior,ct); }
            if (r.PrivateTarget == null && r.ClassesAverageTarget == null || r.PrivateTarget <= 0 || r.ClassesAverageTarget <= 0
                || r.ClassesAverageTarget != null && (decimal.Round(r.ClassesAverageTarget.Value, 4) != r.ClassesAverageTarget || r.ClassesAverageTarget > 99999999999999.9999m))
                throw new BusinessRuleException("Supply an explicit positive Private target and/or Classes average target (up to four decimals).");
            if (r.PrivateTarget != null) FinancialOperations.Money(r.PrivateTarget.Value, true);
            if (await IsRecognizedAsync(period.Id, ct)) throw new BusinessRuleException("Coach period has finalized earnings.");
            if (!await db.Employees.AnyAsync(e => e.Id == r.CoachEmployeeId, ct)) throw new BranchNotFoundException("Coach not found.");
            if (!await db.EmployeeBranchAssignments.AnyAsync(a => a.EmployeeId == r.CoachEmployeeId && a.BranchId == period.BranchId
                && a.StartDate <= period.EndBusinessDate && (a.EndDate == null || a.EndDate >= period.StartBusinessDate), ct)) throw new BusinessRuleException("Coach has no assignment overlapping this branch period.");
            if (await db.CoachTargetRules.AnyAsync(t => t.CommissionPeriodId == period.Id && t.CoachEmployeeId == r.CoachEmployeeId, ct)) throw new BusinessRuleException("Targets are already configured for this coach/period; immutable configuration preserves history.");
            var row = new CoachTargetRule { CommissionPeriodId = period.Id, CoachEmployeeId = r.CoachEmployeeId,
                PrivateTarget = r.PrivateTarget, ClassesAverageTarget = r.ClassesAverageTarget, OperationId = r.OperationId, RequestHash = hash, CreatedByUserId = actor, CreatedAt = access.Now };
            db.Add(row); access.Audit(actor, period.BranchId, "CoachEarnings.TargetsConfigured", nameof(CoachTargetRule), row.Id, after: r); return await TargetDetailsAsync(row,ct);
        }, ct);
    private static CoachTargetDetails TargetDetails(CoachTargetRule t) => new(t.Id, t.CommissionPeriodId, t.CoachEmployeeId, t.PrivateTarget, t.ClassesAverageTarget);
    private async Task<CoachTargetDetails> TargetDetailsAsync(CoachTargetRule target,CancellationToken ct)
    {
        var name=await db.Employees.AsNoTracking().Where(x=>x.Id==target.CoachEmployeeId).Select(x=>x.FullName).SingleOrDefaultAsync(ct);
        return TargetDetails(target) with { CoachEmployeeName=name };
    }
    private async Task<CommissionPeriod> PeriodAsync(Guid id, CancellationToken ct)
    {
        var p = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new BranchNotFoundException("Period not found.");
        if (p.StartBusinessDate == null || p.EndBusinessDate == null) throw new BusinessRuleException("Legacy period needs explicit BusinessDate reconciliation."); return p;
    }
    public async Task<IReadOnlyList<CoachTargetDetails>> TargetsAsync(Guid periodId, CancellationToken ct = default)
    {
        var p = await PeriodAsync(periodId, ct); await access.RequireAsync(CoachingPermissions.EarningsView, p.BranchId, ct);
        return await db.CoachTargetRules.AsNoTracking().Where(t => t.CommissionPeriodId == p.Id).OrderBy(t => t.CoachEmployeeId)
            .Select(t => new CoachTargetDetails(t.Id, t.CommissionPeriodId, t.CoachEmployeeId, t.PrivateTarget, t.ClassesAverageTarget,
                t.CoachEmployee.FullName)).ToListAsync(ct);
    }
    private sealed record EarningSource(Guid Sale, Guid Coach, Guid? Session, Guid? Expiry, decimal Value, bool Transferred, int Order, decimal FundedBefore = 0, decimal FundedNow = 0, decimal PriorExactCommission = 0, decimal PriorRoundedCommission = 0);
    private async Task<List<EarningSource>> FundingAsync(CommissionPeriod period, CancellationToken ct)
    {
        var end = period.EndBusinessDate!.Value;
        var sessions = await db.PrivateSessions.AsNoTracking().Where(s => s.OperationId != null && s.BusinessDate <= end
            && s.PrivateSubscription.BranchId == period.BranchId)
            .Select(s => new EarningSource(s.PrivateSubscriptionTransaction.SaleTransactionId, s.CoachEmployeeId, s.Id, null,
                s.SessionValue!.Value, s.TransferredPackage!.Value, s.SessionNumber!.Value, 0, 0)).ToListAsync(ct);
        var expiries = await db.PrivateExpirySettlements.AsNoTracking().Where(s => s.OperationId != null && s.BusinessDate <= end && s.PrivateSubscription.BranchId == period.BranchId)
            .Select(s => new EarningSource(s.PrivateSubscriptionTransaction.SaleTransactionId, s.CoachEmployeeId, null, s.Id, s.EligibleAmount,
                s.TransferredPackage!.Value, int.MaxValue, 0, 0)).ToListAsync(ct);
        var sources = sessions.Concat(expiries).ToArray(); var sales = sources.Select(s => s.Sale).Distinct().ToArray();
        var payments = await db.Payments.AsNoTracking().Include(p => p.Parts).Where(p => sales.Contains(p.SaleTransactionId)
            && p.Status == PaymentStatus.Completed && p.PaidAt <= access.Now).ToListAsync(ct);
        if (payments.Any(p => p.Amount != p.Parts.Sum(part => part.Amount))) throw new BusinessRuleException("Collections and PaymentParts do not reconcile.");
        // Reuse actual newest-first refund allocations, including refunds received
        // after the evaluation end but before recognition. Refunded money is never funded.
        var refunded = await new Commissions.CommissionSources(db, access).RefundedAsync(payments, ct);
        var previous = await db.CoachEarnings.AsNoTracking().Where(e => sales.Contains(e.SaleTransactionId)).ToListAsync(ct);
        if (previous.Any(e => e.CommissionPeriodId != period.Id && e.BusinessDate > end)) throw new BusinessRuleException("Recognize coach periods in chronological order; future funded evidence exists.");
        var result = new List<EarningSource>();
        foreach (var group in sources.GroupBy(s => s.Sale).OrderBy(g => g.Key))
        {
            var funding = payments.Where(p => p.SaleTransactionId == group.Key && p.BusinessDate <= end).Sum(p => p.Amount - refunded.GetValueOrDefault(p.Id));
            if (funding < 0) throw new BusinessRuleException("Refunded value exceeds collected value.");
            foreach (var source in group.OrderBy(s => s.Order).ThenBy(s => s.Session).ThenBy(s => s.Expiry))
            {
                var allowed = Math.Min(source.Value, funding); funding -= allowed;
                var old = previous.Where(e => source.Session != null ? e.PrivateSessionId == source.Session : e.PrivateExpirySettlementId == source.Expiry).Sum(e => e.EligibleValue);
                if (old > allowed) throw new BusinessRuleException("Historical funding no longer reconciles; Owner adjustment policy required.");
                var priorSlices = previous.Where(e => source.Session != null ? e.PrivateSessionId == source.Session : e.PrivateExpirySettlementId == source.Expiry).ToArray();
                result.Add(source with { FundedBefore = old, FundedNow = allowed - old, PriorExactCommission = priorSlices.Sum(e => e.EligibleValue * e.Rate / 100m), PriorRoundedCommission = priorSlices.Sum(e => e.Amount) });
            }
        }
        return result;
    }
    private static decimal SliceCommission(EarningSource source, decimal rate) => decimal.Round(source.PriorExactCommission + source.FundedNow * rate / 100m, 2, MidpointRounding.AwayFromZero) - source.PriorRoundedCommission;
    private async Task<CoachEarningsProgress> ProgressCoreAsync(CommissionPeriod period, CancellationToken ct)
    {
        if (await IsRecognizedAsync(period.Id, ct))
        {
            var snapshot = await db.AuditLogs.AsNoTracking().SingleAsync(a => a.Action == "CoachEarnings.PeriodRecognized" && a.EntityId == period.Id, ct);
            return JsonSerializer.Deserialize<CoachEarningsProgress>(snapshot.NewValues!)!;
        }
        var sources = await FundingAsync(period, ct);
        var targets = await db.CoachTargetRules.AsNoTracking().Where(t => t.CommissionPeriodId == period.Id).ToDictionaryAsync(t => t.CoachEmployeeId, ct);
        return new(period.Id, false, sources.GroupBy(s => s.Coach).OrderBy(g => g.Key).Select(g =>
        {
            var total = g.Sum(s => s.FundedNow); var target = targets.GetValueOrDefault(g.Key)?.PrivateTarget;
            decimal? rate = target == null ? null : total >= target ? 60m : 50m;
            decimal? earnings = g.Any(s => !s.Transferred && s.FundedNow > 0) && rate == null ? null
                : g.Sum(s => SliceCommission(s, s.Transferred ? 50m : rate ?? 50m));
            return new CoachProgress(g.Key, total, target, target == null ? null : total >= target, rate, earnings, g.Sum(s => s.Value - s.FundedBefore - s.FundedNow));
        }).ToArray());
    }
    private async Task<CoachEarningsProgress> WithCoachNamesAsync(CoachEarningsProgress progress,CancellationToken ct)
    {
        var ids=progress.Coaches.Select(x=>x.CoachEmployeeId).Distinct().ToArray();
        if(ids.Length==0)return progress;
        var names=await db.Employees.AsNoTracking().Where(x=>ids.Contains(x.Id))
            .Select(x=>new{x.Id,x.FullName}).ToDictionaryAsync(x=>x.Id,x=>x.FullName,ct);
        return progress with { Coaches=progress.Coaches.Select(x=>x with{CoachEmployeeName=names.GetValueOrDefault(x.CoachEmployeeId)}).ToArray() };
    }
    public async Task<CoachEarningsProgress> ProgressAsync(Guid periodId, CancellationToken ct = default)
    {
        var period = await PeriodAsync(periodId, ct); await access.RequireAsync(CoachingPermissions.EarningsView, period.BranchId, ct);
        if (db.Database.CurrentTransaction != null) return await WithCoachNamesAsync(await ProgressCoreAsync(period, ct),ct);
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
        var result = await ProgressCoreAsync(period, ct); await tx.CommitAsync(ct);
        return await WithCoachNamesAsync(result,ct);
    }
    public Task<CoachEarningsProgress> RecognizeAsync(RecognizeCoachEarningsRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId);
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"coach-recognition:" + r.OperationId}, 0))", ct);
            var p = await PeriodAsync(r.CommissionPeriodId, ct);
            var actor = await OwnerAsync(CoachingPermissions.Recognize, p.BranchId, ct); await coordination.LockBranchAsync(p.BranchId, ct);
            var hash = FinancialOperations.Hash(r);
            var operation = await db.AuditLogs.AsNoTracking().SingleOrDefaultAsync(a => a.Action == "CoachEarnings.RecognitionOperation" && a.EntityId == r.OperationId, ct);
            if (operation != null)
            {
                using var evidence = JsonDocument.Parse(operation.NewValues!);
                if (operation.UserId != actor || evidence.RootElement.GetProperty("PeriodId").GetGuid() != p.Id || evidence.RootElement.GetProperty("Hash").GetString() != hash) throw new BusinessRuleException("Operation belongs to another recognition request.");
                return await WithCoachNamesAsync(await ProgressCoreAsync(p, ct),ct);
            }
            if (await IsRecognizedAsync(p.Id, ct)) throw new BusinessRuleException("Coach earnings period is already finalized.");
            var (date, _) = await coordination.BusinessDateAsync(p.BranchId, ct);
            if (date <= p.EndBusinessDate) throw new BusinessRuleException("Recognize after the period's last BusinessDate ends, when its target result is final.");
            if (await db.CommissionPeriods.AnyAsync(x => x.BranchId == p.BranchId && x.EndBusinessDate < p.StartBusinessDate
                && !db.AuditLogs.Any(a => a.EntityId == x.Id && a.Action == "CoachEarnings.PeriodRecognized"), ct)) throw new BusinessRuleException("Recognize preceding branch coach periods first (including zero earnings).");
            var sources = await FundingAsync(p, ct); var progress = await ProgressCoreAsync(p, ct);
            if (progress.Coaches.Any(c => c.ProjectedEarnings == null)) throw new BusinessRuleException("Configure an independent Private target for every original coach with funded value.");
            var financialMonth = await CampionsSystem.BLL.MonthlyFinance.FinancialMonthPosting.ResolveAsync(db, p.BranchId, date, ct);
            var rules = await db.CoachTargetRules.AsNoTracking().Where(t => t.CommissionPeriodId == p.Id).ToDictionaryAsync(t => t.CoachEmployeeId, ct);
            foreach (var source in sources.Where(s => s.FundedNow > 0))
            {
                var coach = progress.Coaches.Single(c => c.CoachEmployeeId == source.Coach); var rule = rules.GetValueOrDefault(source.Coach);
                var rate = source.Transferred ? 50m : coach.NormalRate!.Value;
                var earning = new CoachEarning { FinancialMonthId = financialMonth?.Id, BranchId = p.BranchId, CoachEmployeeId = source.Coach, CommissionPeriodId = p.Id,
                    CoachTargetRuleId = rule?.Id, SaleTransactionId = source.Sale, PrivateSessionId = source.Session, PrivateExpirySettlementId = source.Expiry,
                    EligibleValue = source.FundedNow, Rate = rate, Amount = SliceCommission(source, rate),
                    TransferredPackage = source.Transferred, TargetProgress = coach.EligibleValue, Target = coach.Target,
                    RecognitionOperationId = r.OperationId, RequestHash = hash, BusinessDate = p.EndBusinessDate!.Value, CreatedAt = access.Now, CreatedByUserId = actor };
                db.Add(earning); access.Audit(actor, p.BranchId, "CoachEarnings.Recognized", nameof(CoachEarning), earning.Id, after: new { earning.CoachEmployeeId, earning.EligibleValue, earning.Rate, earning.Amount, source.Session, source.Expiry });
            }
            access.Audit(actor, p.BranchId, "CoachEarnings.RecognitionOperation", nameof(CommissionPeriod), r.OperationId, after: new { PeriodId = p.Id, Hash = hash });
            access.Audit(actor, p.BranchId, "CoachEarnings.PeriodRecognized", nameof(CommissionPeriod), p.Id, after: progress with { Finalized = true });
            await db.SaveChangesAsync(ct); return await WithCoachNamesAsync(await ProgressCoreAsync(p, ct),ct);
        }, ct);
    private async Task<CoachBalance> BalanceCoreAsync(Guid branch, Guid coach, CancellationToken ct)
    {
        var earned = await db.CoachEarnings.Where(e => e.BranchId == branch && e.CoachEmployeeId == coach).SumAsync(e => e.Amount, ct);
        var paid = await db.CoachPayouts.Where(p => p.BranchId == branch && p.CoachEmployeeId == coach).SumAsync(p => p.Amount, ct);
        return new(branch, coach, earned, paid, earned - paid);
    }
    public async Task<CoachBalance> BalanceAsync(Guid branch, Guid coach, CancellationToken ct = default)
    {
        await access.RequireAsync(CoachingPermissions.EarningsView, branch, ct);
        if (db.Database.CurrentTransaction != null) return await WithCoachNameAsync(await BalanceCoreAsync(branch, coach, ct),ct);
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
        var result = await BalanceCoreAsync(branch, coach, ct); await tx.CommitAsync(ct);
        return await WithCoachNameAsync(result,ct);
    }
    private async Task<CoachBalance> WithCoachNameAsync(CoachBalance balance,CancellationToken ct)
    {
        var name=await db.Employees.AsNoTracking().Where(x=>x.Id==balance.CoachEmployeeId).Select(x=>x.FullName).SingleOrDefaultAsync(ct);
        return balance with { CoachEmployeeName=name };
    }
    public async Task<CoachingPage<CoachEarningDetails>> EarningsHistoryAsync(Guid branch, Guid coach, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.EarningsView, branch, ct);
        var q = db.CoachEarnings.AsNoTracking().Where(e => e.BranchId == branch && e.CoachEmployeeId == coach); var count = await q.CountAsync(ct);
        return new(await q.OrderByDescending(e => e.BusinessDate).ThenBy(e => e.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(e =>
            new CoachEarningDetails(e.Id, e.CoachEmployeeId, e.CommissionPeriodId, e.SaleTransactionId, e.PrivateSessionId, e.PrivateExpirySettlementId,
                e.EligibleValue, e.Rate, e.Amount, e.TransferredPackage, e.BusinessDate,e.CoachEmployee.FullName)).ToListAsync(ct), count, page, pageSize);
    }
}
