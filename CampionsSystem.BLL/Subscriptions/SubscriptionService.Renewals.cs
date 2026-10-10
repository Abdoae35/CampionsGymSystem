using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    private static DateOnly PlusDays(DateOnly date, int days)
    {
        try { return date.AddDays(days); }
        catch (ArgumentOutOfRangeException) { throw new BusinessRuleException("Entitlement date is outside the supported date range."); }
    }
    private static void Version(MemberSubscription sub, int expected)
    {
        if (expected != sub.Version) throw new DbUpdateConcurrencyException("Subscription changed; reload before submitting a new operation.");
    }
    private async Task<MemberSubscription> LockedSubscriptionAsync(Guid id, CancellationToken ct)
    {
        var sub = await db.MemberSubscriptions.FromSqlInterpolated($"SELECT * FROM \"MemberSubscriptions\" WHERE \"Id\"={id} FOR UPDATE")
            .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Subscription not found.");
        await db.Entry(sub).ReloadAsync(ct); return sub;
    }
    private async Task ShiftSuccessorAsync(MemberSubscription source, int days, Guid actor, CancellationToken ct)
    {
        if (days == 0) return;
        var next = await db.MemberSubscriptions.SingleOrDefaultAsync(x => x.FollowsSubscriptionId == source.Id, ct);
        if (next is null) return;
        await db.Entry(next).ReloadAsync(ct);
        if (next.MemberId != source.MemberId || next.EmployeeId != null || next.EndDate == null
            || next.Status == SubscriptionStatus.Cancelled || next.StartDate <= await EffectiveBusinessDateAsync(next.PurchasedAtBranchId, ct))
            throw new BusinessRuleException("A successor already started or has conflicting history; this extension cannot rewrite it.");
        var old = new { next.StartDate, next.EndDate };
        next.StartDate = PlusDays(next.StartDate, days); next.EndDate = PlusDays(next.EndDate.Value, days); next.Version++;
        access.Audit(actor,next.PurchasedAtBranchId,"Subscription.ScheduleShifted",nameof(MemberSubscription),next.Id,old,
            new { next.StartDate,next.EndDate,PredecessorId=source.Id });
    }
    public async Task<SubscriptionGrantResult> RenewCustomerAsync(SubscriptionRenewalCommand r, CancellationToken ct = default)
    {
        var actor = await RequireAsync(SubscriptionPermissions.Manage,r.BranchId,true,ct);
        return await RenewCustomerCoreAsync(r, actor.UserId, null, null, ct);
    }
    private async Task<SubscriptionGrantResult> RenewCustomerCoreAsync(SubscriptionRenewalCommand r, Guid actor,
        SaleTransaction? sale, DateOnly? requestedStart, CancellationToken ct, (DateOnly Date, Guid? Day)? activationDate = null)
    {
        var sourceMember = await db.MemberSubscriptions.AsNoTracking().Where(x => x.Id == r.SubscriptionId)
            .Select(x => (Guid?)x.MemberId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Subscription not found.");
        return await TransactionAsync(async () =>
        {
            var plan = sale == null ? await LockPlanAsync(r.PlanId,r.BranchId,ct) : SoldPlan(sale);
            var member = await LockMemberAsync(sourceMember,ct);
            var source = await LockedSubscriptionAsync(r.SubscriptionId,ct);
            if (sale == null) await RequireAsync(SubscriptionPermissions.Manage,source.PurchasedAtBranchId,true,ct);
            if (source.EmployeeId != null) throw new BusinessRuleException("Employee periods cannot be renewed as customer memberships.");
            if (source.IsStandaloneVisit) throw new BusinessRuleException("Standalone visits are not membership renewal cycles.");
            var retry = await RetryGrantAsync(r.OperationId,member.Id,plan.Id,r.BranchId,source.Id,ct);
            if (retry is not null) return retry;
            Version(source,r.ExpectedVersion);
            if (sale == null) await ValidateCustomerAsync(member,plan,ct);
            else if (!member.IsActive) throw new BusinessRuleException("Member must be active to activate purchased entitlement.");
            var borrowedDebt=await OutstandingBorrowedDebtAsync(member.Id,plan,ct);
            var (date,day) = activationDate ?? await GrantDateAsync(r.BranchId,ct);
            var state = await Evaluated(sale == null ? null : date).SingleAsync(x => x.Subscription.Id == source.Id,ct);
            if (state.Status is not (SubscriptionStatus.Active or SubscriptionStatus.Frozen or SubscriptionStatus.Expired))
                throw new BusinessRuleException("Renewal requires a current or expired customer period.");
            if (state.PlanType is null) throw new BusinessRuleException("Legacy grant history must be reconciled before renewal.");
            var previousEnd = source.EndDate; var previousSessions = source.RemainingSessions;
            var ongoing = state.Status is SubscriptionStatus.Active or SubscriptionStatus.Frozen;
            var compatible = source.AccessMode == plan.AccessMode && source.CityId == plan.CityId
                && source.PurchasedAtBranchId == r.BranchId && state.PlanType == plan.PlanCategory;
            MemberSubscription target; DateOnly grantStart;
            if (ongoing && compatible)
            {
                target = source; grantStart = PlusDays(source.EndDate!.Value,1);
                source.EndDate = PlusDays(source.EndDate.Value,plan.DurationDays);
                if (source.AccessMode == AccessMode.SessionBased)
                {
                    if ((long)source.TotalSessions!.Value + plan.SessionCount!.Value > int.MaxValue
                        || (long)source.RemainingSessions!.Value + plan.SessionCount.Value > int.MaxValue)
                        throw new BusinessRuleException("Session entitlement exceeds supported quantity.");
                    source.TotalSessions += plan.SessionCount; source.RemainingSessions += plan.SessionCount;
                }
                await ShiftSuccessorAsync(source,plan.DurationDays,actor,ct);
            }
            else
            {
                if (await db.MemberSubscriptions.AnyAsync(x => x.MemberId == member.Id && x.Id != source.Id
                    && x.Status != SubscriptionStatus.Cancelled && (!x.IsStandaloneVisit || x.RemainingSessions > 0) && (x.EndDate == null || x.EndDate >= date),ct))
                    throw new BusinessRuleException("Member already has another open/future period. Only one pending successor is supported.");
                grantStart = ongoing ? PlusDays(source.EndDate!.Value,1) : requestedStart ?? date;
                target = new MemberSubscription { MemberId=member.Id,MembershipPlanId=plan.Id,PurchasedAtBranchId=r.BranchId,
                    CityId=plan.CityId,AccessMode=plan.AccessMode,StartDate=grantStart,EndDate=PlusDays(grantStart,plan.DurationDays-1),
                    TotalSessions=plan.SessionCount,RemainingSessions=plan.SessionCount,FollowsSubscriptionId=ongoing?source.Id:null,
                    Status=grantStart>date?SubscriptionStatus.Scheduled:SubscriptionStatus.Active,CreatedAt=access.Now };
                db.Add(target);
            }
            source.Version++;
            var grant = await SnapshotAsync(target,plan,r.OperationId,actor,day,grantStart,SubscriptionTransactionType.Renewal,
                source,previousEnd,previousSessions,ct,sale);
            SettleBorrowedDebt(target,grant,borrowedDebt,actor);
            access.Audit(actor,r.BranchId,"Subscription.Renewed",nameof(MemberSubscription),target.Id,
                new { SourceSubscriptionId=source.Id,EndDate=previousEnd,RemainingSessions=previousSessions },
                new { target.MemberId,grant.Id,target.StartDate,target.EndDate,target.RemainingSessions,target.FollowsSubscriptionId });
            await db.SaveChangesAsync(ct); await ApplyDeclaredHolidaysForMemberAsync(member.Id,actor,ct);
            return new SubscriptionGrantResult(target.Id,grant.Id,target.StartDate>date);
        },ct);
    }
}
