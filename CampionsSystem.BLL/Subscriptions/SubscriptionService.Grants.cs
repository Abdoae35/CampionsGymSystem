using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    private async Task<T> TransactionAsync<T>(Func<Task<T>> change, CancellationToken ct)
    {
        var owned = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        var tx = db.Database.CurrentTransaction!;
        var point = "subscription_" + Guid.NewGuid().ToString("N");
        try
        {
            await tx.CreateSavepointAsync(point, ct); var result = await change();
            await tx.ReleaseSavepointAsync(point, ct); if (owned is not null) await owned.CommitAsync(ct); return result;
        }
        catch
        {
            if (owned is not null) await owned.RollbackAsync(CancellationToken.None);
            else await tx.RollbackToSavepointAsync(point, CancellationToken.None);
            db.ChangeTracker.Clear(); throw;
        }
        finally { if (owned is not null) await owned.DisposeAsync(); }
    }
    private async Task<Member> LockMemberAsync(Guid id, CancellationToken ct)
    {
        var member = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={id} FOR UPDATE")
            .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Member not found.");
        await db.Entry(member).ReloadAsync(ct); return member;
    }
    private async Task<MembershipPlan> LockPlanAsync(Guid id, Guid branch, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FromSqlInterpolated($"SELECT * FROM \"MembershipPlans\" WHERE \"Id\"={id} FOR UPDATE")
            .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Plan not found.");
        await db.Entry(plan).ReloadAsync(ct);
        if (!plan.IsActive || plan.BranchId != branch || !await db.GymBranches.AnyAsync(x => x.Id == branch && x.IsActive, ct)
            || !await db.Cities.AnyAsync(x => x.Id == plan.CityId && x.IsActive, ct))
            throw new BusinessRuleException("Select an active plan from the originating branch with active access city.");
        return plan;
    }
    private async Task<(DateOnly Date, Guid? DayId)> GrantDateAsync(Guid branch, CancellationToken ct)
    {
        var day = await db.BusinessDays.FromSqlInterpolated($"SELECT * FROM \"BusinessDays\" WHERE \"BranchId\"={branch} AND \"Status\"='Open' FOR SHARE")
            .AsNoTracking().SingleOrDefaultAsync(ct);
        if (day is not null && (day.OpenedAt > access.Now || day.BusinessDate > access.Today))
            throw new BusinessRuleException("Invalid open business day.");
        return (day?.BusinessDate ?? access.Today, day?.Id);
    }
    private async Task<SubscriptionGrantResult?> RetryGrantAsync(Guid operation, Guid member, Guid plan, Guid branch,
        Guid? source, CancellationToken ct)
    {
        if (operation == Guid.Empty) throw new BusinessRuleException("A nonempty operation identity is required.");
        var prior = await db.MembershipSubscriptionTransactions.AsNoTracking().Include(x => x.MemberSubscription)
            .SingleOrDefaultAsync(x => x.OperationId == operation, ct);
        if (prior is null) return null;
        if (prior.MemberSubscription.MemberId != member || prior.MembershipPlanId != plan
            || prior.MemberSubscription.PurchasedAtBranchId != branch || prior.RenewedFromSubscriptionId != source)
            throw new BusinessRuleException("Operation identity already belongs to another request.");
        return new(prior.MemberSubscriptionId, prior.Id, prior.MemberSubscription.StartDate > await EffectiveBusinessDateAsync(branch, ct));
    }
    private async Task<MembershipSubscriptionTransaction> SnapshotAsync(MemberSubscription subscription, MembershipPlan plan,
        Guid operation, Guid actor, Guid? businessDay, DateOnly grantStart, SubscriptionTransactionType type,
        MemberSubscription? source, DateOnly? previousEnd, int? previousSessions, CancellationToken ct)
    {
        var grant = new MembershipSubscriptionTransaction
        {
            MemberSubscription = subscription, MembershipPlanId = plan.Id, OperationId = operation,
            PlanNameSnapshot = plan.Name, PlanBranchIdSnapshot = plan.BranchId, CityIdSnapshot = plan.CityId,
            AccessModeSnapshot = plan.AccessMode, FreezeDaysSnapshot = plan.AllowFreeze ? plan.IncludedFreezeDays : 0,
            PlanCategorySnapshot = plan.PlanCategory, FreelanceCoachEmployeeIdSnapshot = plan.FreelanceCoachEmployeeId,
            PurchasedDurationDays = plan.DurationDays, PurchasedSessions = plan.SessionCount,
            PlanPriceSnapshot = plan.Price, NetPriceSnapshot = plan.Price, TransactionType = type,
            PreviousEndDate = previousEnd, NewEndDate = subscription.EndDate, PreviousRemainingSessions = previousSessions,
            RenewedFromSubscriptionId = source?.Id, StartDateSnapshot = grantStart, BusinessDayId = businessDay,
            CreatedByUserId = actor, CreatedAt = access.Now
        };
        db.Add(grant);
        var configs = await db.MembershipPlanBenefits.AsNoTracking().Where(x => x.MembershipPlanId == plan.Id)
            .OrderBy(x => x.MembershipBenefitId).ToListAsync(ct);
        foreach (var config in configs)
        {
            var benefit = await db.MembershipBenefits.FromSqlInterpolated($"SELECT * FROM \"MembershipBenefits\" WHERE \"Id\"={config.MembershipBenefitId} FOR SHARE")
                .AsNoTracking().SingleAsync(ct);
            if (!benefit.IsActive) throw new BusinessRuleException("Plan contains an inactive benefit; update its configuration before granting.");
            db.Add(new MemberSubscriptionBenefit { MembershipSubscriptionTransaction = grant,
                MembershipBenefitId = benefit.Id, BenefitCodeSnapshot = benefit.Code, BenefitNameSnapshot = benefit.Name,
                GrantTypeSnapshot = config.GrantType, GrantedQuantity = config.Quantity, GrantedAt = access.Now, CreatedAt = access.Now });
        }
        return grant;
    }
    // Intentionally protected BLL integration boundary. No HTTP customer grant route before Finance.
    public async Task<SubscriptionGrantResult> CreateCustomerGrantAsync(SubscriptionGrantCommand r, CancellationToken ct = default)
    {
        var actor = await RequireAsync(SubscriptionPermissions.Manage, r.BranchId, true, ct);
        return await TransactionAsync(async () =>
        {
            var plan = await LockPlanAsync(r.PlanId, r.BranchId, ct);
            var member = await LockMemberAsync(r.MemberId, ct);
            var retry = await RetryGrantAsync(r.OperationId, r.MemberId, r.PlanId, r.BranchId, null, ct);
            if (retry is not null) return retry;
            await ValidateCustomerAsync(member, plan, ct);
            var (date, day) = await GrantDateAsync(r.BranchId, ct);
            if (await db.MemberSubscriptions.AnyAsync(x => x.MemberId == member.Id && x.Status != SubscriptionStatus.Cancelled
                && (x.EndDate == null || x.EndDate >= date), ct))
                throw new BusinessRuleException("Member already has an open or future entitlement; use the renewal integration.");
            var sub = new MemberSubscription { MemberId = member.Id, MembershipPlanId = plan.Id, PurchasedAtBranchId = r.BranchId,
                CityId = plan.CityId, AccessMode = plan.AccessMode, StartDate = date, EndDate = PlusDays(date,plan.DurationDays - 1),
                TotalSessions = plan.SessionCount, RemainingSessions = plan.SessionCount, Status = SubscriptionStatus.Active, CreatedAt = access.Now };
            db.Add(sub);
            var grant = await SnapshotAsync(sub, plan, r.OperationId, actor.UserId, day, date, SubscriptionTransactionType.Initial, null, null, null, ct);
            access.Audit(actor.UserId, r.BranchId, "Subscription.Created", nameof(MemberSubscription), sub.Id,
                after: new { sub.MemberId, GrantId=grant.Id, sub.StartDate, sub.EndDate, sub.CityId, sub.AccessMode, sub.RemainingSessions });
            await db.SaveChangesAsync(ct); return new SubscriptionGrantResult(sub.Id, grant.Id, false);
        }, ct);
    }
}
