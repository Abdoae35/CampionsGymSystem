using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Operations;

public sealed record ActivityCoachReport(Guid CoachId, string Coach, Guid ActivityId, string Activity,
    ActivityCoachType CoachType, Guid? EmployeeId, int SubscribersCount, decimal EligibleRevenue,
    decimal Percentage, decimal Entitlement);
public sealed record ClassCoachReport(Guid CoachEmployeeId, string Coach, int CompletedClassesCount,
    int TotalAttendances, decimal AverageAttendance);
public sealed record FreelancePlanReport(Guid MembershipPlanId, string Plan, Guid CoachEmployeeId, string Coach,
    int EligibleMemberCount, int PurchasedMemberCount, decimal ActualGymRevenue, string SalesAttribution);

public sealed class CoachReportService(ApplicationDbContext db, OperationAccess access)
{
    public async Task<IReadOnlyList<ActivityCoachReport>> ActivitiesAsync(Guid periodId, CancellationToken ct = default)
    {
        var period = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(x => x.Id == periodId, ct)
            ?? throw new BusinessRuleException("Commission period not found.");
        await access.RequireAsync("Activities.ViewCoachRevenue", period.BranchId, ct);
        if (period.Status == CommissionPeriodStatus.Closed || await db.AuditLogs.AnyAsync(
            x => x.Action == "ActivityCoach.PeriodFinalized" && x.EntityId == periodId, ct))
        {
            var rows = await db.ActivityCoachSettlements.AsNoTracking().Where(x => x.CommissionPeriodId == periodId)
                .Select(x => new ActivityCoachReport(x.ActivityCoachId, x.CoachNameSnapshot, x.SportsActivityId, x.ActivityNameSnapshot,
                    x.CoachTypeSnapshot, x.EmployeeId, x.SubscribersCount, x.EligibleRevenue, x.Percentage, x.Entitlement)).ToListAsync(ct);
            // A marker in AuditLog also records a finalized zero-revenue period.
            if (rows.Count == 0 && !await db.AuditLogs.AnyAsync(x => x.Action == "ActivityCoach.PeriodFinalized" && x.EntityId == periodId, ct))
                throw new BusinessRuleException("Closed period has no activity snapshot; explicitly finalize/reconcile it first.");
            return rows;
        }
        return await CalculateActivitiesAsync(period, ct);
    }

    // Called explicitly after period closure. No automatic close/reopen, payroll payment or ledger changes.
    public async Task FinalizeActivitiesAsync(Guid periodId, CancellationToken ct = default)
    {
        await using var transaction = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct) : null;
        var period = await db.CommissionPeriods.SingleOrDefaultAsync(x => x.Id == periodId, ct)
            ?? throw new BusinessRuleException("Commission period not found.");
        var actor = await access.RequireAsync("Commission.ClosePeriod", period.BranchId, ct);
        if (period.Status != CommissionPeriodStatus.Closed || period.EndAt is null || period.EndAt > access.Now)
            throw new BusinessRuleException("Close the period with a final end time before taking an activity snapshot.");
        if (await db.ActivityCoachSettlements.AnyAsync(x => x.CommissionPeriodId == periodId, ct)
            || await db.AuditLogs.AnyAsync(x => x.Action == "ActivityCoach.PeriodFinalized" && x.EntityId == periodId, ct))
            throw new BusinessRuleException("Activity period is already finalized; explicit correction required.");
        var rows = await CalculateActivitiesAsync(period, ct);
        foreach (var row in rows)
            db.Add(new ActivityCoachSettlement { CommissionPeriodId = periodId, ActivityCoachId = row.CoachId,
                SportsActivityId = row.ActivityId, EmployeeId = row.EmployeeId, CoachTypeSnapshot = row.CoachType,
                CoachNameSnapshot = row.Coach, ActivityNameSnapshot = row.Activity, SubscribersCount = row.SubscribersCount,
                EligibleRevenue = row.EligibleRevenue, Percentage = row.Percentage, Entitlement = row.Entitlement,
                FinalizedAt = access.Now, CreatedByUserId = actor });
        // Internal settlement is bridged exactly once into the existing employee commission model.
        foreach (var group in rows.Where(x => x.EmployeeId.HasValue).GroupBy(x => new { x.EmployeeId, x.ActivityId }))
        {
            if (await db.EmployeeCommissionSummaries.AnyAsync(x => x.CommissionPeriodId == periodId && x.EmployeeId == group.Key.EmployeeId
                && x.CommissionType == CommissionType.Activity && x.SportsActivityId == group.Key.ActivityId, ct))
                throw new BusinessRuleException("An employee activity summary already exists; reconcile instead of double paying.");
            db.Add(new EmployeeCommissionSummary { CommissionPeriodId = periodId, EmployeeId = group.Key.EmployeeId!.Value,
                BranchId = period.BranchId, CommissionType = CommissionType.Activity, SportsActivityId = group.Key.ActivityId,
                TotalEligibleAmount = group.Sum(x => x.EligibleRevenue), CommissionAmount = group.Sum(x => x.Entitlement),
                AppliedRate = group.Select(x => x.Percentage).Distinct().Count() == 1 ? group.First().Percentage : null,
                FinalizedAt = access.Now });
        }
        access.Audit(actor, period.BranchId, "ActivityCoach.PeriodFinalized", nameof(CommissionPeriod), periodId,
            after: new { Rows = rows.Count });
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
    }

    private async Task<IReadOnlyList<ActivityCoachReport>> CalculateActivitiesAsync(CommissionPeriod period, CancellationToken ct)
    {
        var until = period.EndAt ?? access.Now;
        var payments = await db.Payments.AsNoTracking().Where(x => x.CommissionPeriodId == period.Id
            && x.Status == PaymentStatus.Completed && x.SaleTransaction.SaleType == SaleType.Activity)
            .Select(x => new { x.Id, x.SaleTransactionId, x.BranchId, x.PaidAt, x.Amount,
                PartsAmount = x.Parts.Sum(p => (decimal?)p.Amount) ?? 0m }).ToListAsync(ct);
        if (payments.Count == 0) return [];
        if (payments.Any(x => x.BranchId != period.BranchId || x.PaidAt < period.StartAt || x.PaidAt >= until || x.Amount != x.PartsAmount))
            throw new BusinessRuleException("Activity payment branch/time or PaymentParts total is inconsistent; reconcile first.");
        var saleIds = payments.Select(x => x.SaleTransactionId).Distinct().ToArray();
        var batches = await db.ActivitySubscriptionTransactions.AsNoTracking().Where(x => saleIds.Contains(x.SaleTransactionId))
            .Select(x => new { x.SaleTransactionId, x.ActivityCoachIdSnapshot, x.CoachPercentageSnapshot,
                x.ActivitySubscription.MemberId, x.ActivitySubscription.BranchId, ActivityId = x.ActivityPackage.SportsActivityId,
                ActivityName = x.ActivityPackage.SportsActivity.Name,
                CoachName = x.ActivityCoachSnapshot!.EmployeeId.HasValue ? x.ActivityCoachSnapshot.Employee!.FullName : x.ActivityCoachSnapshot.ExternalName!,
                EmployeeId = x.ActivityCoachSnapshot == null ? null : x.ActivityCoachSnapshot.EmployeeId,
                CoachType = x.ActivityCoachSnapshot == null ? (ActivityCoachType?)null : x.ActivityCoachSnapshot.CoachType }).ToListAsync(ct);
        if (saleIds.Any(id => batches.Count(x => x.SaleTransactionId == id) != 1)
            || batches.Any(x => x.ActivityCoachIdSnapshot is null || x.CoachPercentageSnapshot is null || x.BranchId != period.BranchId))
            throw new BusinessRuleException("Activity purchase has missing/ambiguous coach, rate or sale attribution; reconcile legacy data first.");
        var refunds = await db.Refunds.AsNoTracking().Where(x => saleIds.Contains(x.SaleTransactionId) && x.CreatedAt < until)
            .Select(x => new { x.OriginalPaymentId, x.SaleTransactionId, x.Amount }).ToListAsync(ct);
        if (refunds.Any(x => x.OriginalPaymentId is null))
            throw new BusinessRuleException("Activity refunds require an explicit OriginalPaymentId for commission attribution.");
        if (refunds.Any(x => payments.Any(p => p.Id == x.OriginalPaymentId && p.SaleTransactionId != x.SaleTransactionId)))
            throw new BusinessRuleException("Refund payment and sale disagree.");
        var values = payments.Select(payment =>
        {
            var batch = batches.Single(x => x.SaleTransactionId == payment.SaleTransactionId);
            var refunded = refunds.Where(x => x.OriginalPaymentId == payment.Id).Sum(x => x.Amount);
            if (refunded > payment.Amount) throw new BusinessRuleException("Activity refunds exceed received payment.");
            return new { Batch = batch, Revenue = payment.Amount - refunded };
        });
        return values.GroupBy(x => new { x.Batch.ActivityCoachIdSnapshot, x.Batch.ActivityId, x.Batch.CoachPercentageSnapshot,
                x.Batch.CoachName, x.Batch.ActivityName, x.Batch.EmployeeId, x.Batch.CoachType })
            .Select(g => new ActivityCoachReport(g.Key.ActivityCoachIdSnapshot!.Value, g.Key.CoachName,
                g.Key.ActivityId, g.Key.ActivityName, g.Key.CoachType!.Value, g.Key.EmployeeId,
                g.Select(x => x.Batch.MemberId).Distinct().Count(), g.Sum(x => x.Revenue), g.Key.CoachPercentageSnapshot!.Value,
                decimal.Round(g.Sum(x => x.Revenue) * g.Key.CoachPercentageSnapshot.Value / 100m, 2, MidpointRounding.AwayFromZero))).ToArray();
    }

    public async Task<IReadOnlyList<ClassCoachReport>> ClassesAsync(Guid branchId, DateTimeOffset from,
        DateTimeOffset until, CancellationToken ct = default)
    {
        await access.RequireAsync("Classes.ViewAttendanceReport", branchId, ct);
        if (until <= from) throw new BusinessRuleException("Report end must be after start.");
        from = from.ToUniversalTime(); until = until.ToUniversalTime();
        var rows = await db.ClassSessions.AsNoTracking().Where(x => x.BranchId == branchId && x.Status == ClassSessionStatus.Completed
            && x.StartedAt >= from && x.StartedAt < until)
            .Select(x => new { x.CoachEmployeeId, x.CoachEmployee.FullName,
                Attendance = db.CheckIns.Count(c => c.ClassSessionId == x.Id && c.CheckInType == CheckInType.Class) }).ToListAsync(ct);
        return rows.GroupBy(x => new { x.CoachEmployeeId, x.FullName })
            .Select(g => new ClassCoachReport(g.Key.CoachEmployeeId, g.Key.FullName, g.Count(), g.Sum(x => x.Attendance),
                (decimal)g.Sum(x => x.Attendance) / g.Count())).ToArray();
    }

    public async Task<IReadOnlyList<FreelancePlanReport>> FreelanceAsync(Guid planId, Guid branchId,
        DateTimeOffset from, DateTimeOffset until, CancellationToken ct = default)
    {
        await access.RequireAsync("FreelancePlans.View", branchId, ct);
        if (until <= from) throw new BusinessRuleException("Report end must be after start.");
        from = from.ToUniversalTime(); until = until.ToUniversalTime();
        var plan = await db.MembershipPlans.AsNoTracking().Include(x => x.FreelanceCoachEmployee)
            .SingleOrDefaultAsync(x => x.Id == planId && x.PlanCategory == PlanCategory.Freelance, ct)
            ?? throw new BusinessRuleException("Freelance plan not found.");
        if(plan.BranchId!=branchId)throw new BusinessRuleException("Report branch must own the Freelance plan.");
        var eligible = await db.MembershipPlanEligibleMembers.CountAsync(x => x.MembershipPlanId == planId && x.Member.IsActive, ct);
        var purchases = await db.MembershipSubscriptionTransactions.AsNoTracking().Where(x => x.MembershipPlanId == planId
            && x.PlanCategorySnapshot == PlanCategory.Freelance && x.SaleTransaction!.BranchId == branchId)
            .Select(x => new { x.SaleTransactionId, x.FreelanceCoachEmployeeIdSnapshot,
                Coach = x.FreelanceCoachEmployeeSnapshot!.FullName, x.MemberSubscription.MemberId,
                x.SaleTransaction!.SalesAttributionSnapshot, x.SaleTransaction!.SalesEmployeeId, x.SaleTransaction!.SaleCategory,
                x.SaleTransaction!.OperationId,x.SaleTransaction!.CommissionEligible,x.SaleTransaction!.TargetEligible,x.CreatedAt }).ToListAsync(ct);
        var ids = purchases.Select(x => x.SaleTransactionId).ToArray();
        if (purchases.GroupBy(x => x.SaleTransactionId).Any(g => g.Count() != 1))
            throw new BusinessRuleException("Ambiguous membership sale; reconcile before reporting.");
        var payments = await db.Payments.AsNoTracking().Where(x => ids.Contains(x.SaleTransactionId) && x.BranchId == branchId
            && x.Status == PaymentStatus.Completed && x.PaidAt >= from && x.PaidAt < until)
            .Select(x => new { x.SaleTransactionId, x.Amount, Parts = x.Parts.Sum(p => (decimal?)p.Amount) ?? 0m }).ToListAsync(ct);
        var refunds = await db.Refunds.AsNoTracking().Where(x => ids.Contains(x.SaleTransactionId) && x.BranchId == branchId
            && x.CreatedAt >= from && x.CreatedAt < until).Select(x => new { x.SaleTransactionId, x.Amount }).ToListAsync(ct);
        if (payments.Any(x => x.Parts != x.Amount)) throw new BusinessRuleException("PaymentParts do not match received gym money.");
        if (purchases.Any(x => x.OperationId==null && (x.SalesAttributionSnapshot != AssignmentType.House || x.SalesEmployeeId != null || x.SaleCategory != null)))
            throw new BusinessRuleException("Legacy Freelance sale requires explicit House attribution reconciliation.");
        if(purchases.Any(x=>x.OperationId!=null && (x.CommissionEligible!=false || x.TargetEligible!=false)))
            throw new BusinessRuleException("Managed Freelance revenue must be commission/target ineligible.");
        var result = purchases.GroupBy(x => new { x.FreelanceCoachEmployeeIdSnapshot, x.Coach }).Select(g =>
        {
            var groupIds = g.Select(x => x.SaleTransactionId).ToHashSet();
            return new FreelancePlanReport(plan.Id, plan.Name, g.Key.FreelanceCoachEmployeeIdSnapshot!.Value, g.Key.Coach,
                g.Key.FreelanceCoachEmployeeIdSnapshot == plan.FreelanceCoachEmployeeId ? eligible : 0,
                g.Where(x => x.CreatedAt >= from && x.CreatedAt < until).Select(x => x.MemberId).Distinct().Count(),
                payments.Where(x => groupIds.Contains(x.SaleTransactionId)).Sum(x => x.Amount)
                - refunds.Where(x => groupIds.Contains(x.SaleTransactionId)).Sum(x => x.Amount), g.Any(x=>x.OperationId!=null)?"CommissionIneligible":"House");
        }).ToList();
        if (result.All(x => x.CoachEmployeeId != plan.FreelanceCoachEmployeeId))
            result.Add(new(plan.Id, plan.Name, plan.FreelanceCoachEmployeeId!.Value, plan.FreelanceCoachEmployee!.FullName, eligible, 0, 0, "House"));
        return result;
    }
}
