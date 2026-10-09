using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    private static FreezeDetails FreezeView(SubscriptionFreeze f) => new(f.Id,f.FreezeType.ToString(),f.StartDate,f.EndDate,f.DaysApplied,
        f.Reason,f.ApprovedByUserId,f.CreatedAt,f.CompletedAt,f.DaysAlreadyCompensated,f.PreviousSubscriptionEndDate,f.NewSubscriptionEndDate);
    private async Task<T> MutateSubscriptionAsync<T>(Guid id, string permission, Func<MemberSubscription,Guid,Task<T>> change, CancellationToken ct)
    {
        var identity = await db.MemberSubscriptions.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.MemberId,x.PurchasedAtBranchId }).SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Subscription not found.");
        await RequireAsync(permission,identity.PurchasedAtBranchId,false,ct);
        return await TransactionAsync(async () =>
        {
            await LockMemberAsync(identity.MemberId,ct);var sub = await LockedSubscriptionAsync(id,ct);
            var actor = await RequireAsync(permission,sub.PurchasedAtBranchId,false,ct);
            return await change(sub,actor.UserId);
        },ct);
    }
    public Task<FreezeDetails> StartFreezeAsync(Guid id, FreezeRequest r, CancellationToken ct = default)
        => MutateSubscriptionAsync(id,SubscriptionPermissions.Freeze,async (sub,actor) =>
        {
            if (r.OperationId == Guid.Empty || r.Days <= 0) throw new BusinessRuleException("Positive freeze days and operation identity are required.");
            var prior = await db.SubscriptionFreezes.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId,ct);
            if (prior != null)
            {
                if (prior.MemberSubscriptionId != id || prior.FreezeType != FreezeType.Normal || prior.StartDate != r.StartDate || prior.DaysApplied != r.Days)
                    throw new BusinessRuleException("Operation identity belongs to another freeze request.");
                return FreezeView(prior);
            }
            Version(sub,r.ExpectedVersion);
            var date = await EffectiveBusinessDateAsync(sub.PurchasedAtBranchId,ct);
            var status = (await Evaluated().SingleAsync(x => x.Subscription.Id == id,ct)).Status;
            if (sub.EmployeeId != null || sub.EndDate == null || status is not (SubscriptionStatus.Active or SubscriptionStatus.Frozen)
                || r.StartDate < date || r.StartDate > sub.EndDate)
                throw new BusinessRuleException("Normal freeze requires a current customer entitlement and a start within its remaining period.");
            var end = PlusDays(r.StartDate,r.Days-1);
            var grants = await db.MembershipSubscriptionTransactions.AsNoTracking().Where(x => x.MemberSubscriptionId == id).ToListAsync(ct);
            var freezes = await db.SubscriptionFreezes.AsNoTracking().Where(x => x.MemberSubscriptionId == id && x.FreezeType == FreezeType.Normal).ToListAsync(ct);
            if (grants.Count == 0 || grants.Any(x => x.FreezeDaysSnapshot == null) || freezes.Any(x => x.DaysApplied == null || x.EndDate == null))
                throw new BusinessRuleException("Legacy freeze allowance/history requires explicit reconciliation.");
            if (freezes.Any(x => x.StartDate <= end && x.EndDate >= r.StartDate)) throw new BusinessRuleException("Freeze periods cannot overlap.");
            var remaining = grants.Sum(x => (long)x.FreezeDaysSnapshot!.Value) - freezes.Sum(x => (long)x.DaysApplied!.Value);
            if (r.Days > remaining) throw new BusinessRuleException("Freeze exceeds remaining snapshot allowance.");
            var already = await db.SubscriptionHolidayExtensions.CountAsync(x => x.MemberSubscriptionId == id && x.DaysApplied == 1
                && x.ProtectedDate >= r.StartDate && x.ProtectedDate <= end,ct);
            var oldEnd = sub.EndDate.Value; var extension = r.Days - already;
            sub.EndDate = PlusDays(oldEnd,extension);sub.Version++;
            await ShiftSuccessorAsync(sub,extension,actor,ct);
            var freeze = new SubscriptionFreeze { MemberSubscriptionId=id,OperationId=r.OperationId,FreezeType=FreezeType.Normal,
                StartDate=r.StartDate,EndDate=end,DaysApplied=r.Days,DaysAlreadyCompensated=already,ApprovedByUserId=actor,CreatedAt=access.Now,
                PreviousSubscriptionEndDate=oldEnd,NewSubscriptionEndDate=sub.EndDate };
            db.Add(freeze);
            access.Audit(actor,sub.PurchasedAtBranchId,"Subscription.FreezeStarted",nameof(SubscriptionFreeze),freeze.Id,
                new { EndDate=oldEnd },new { sub.MemberId,SubscriptionId=id,freeze.StartDate,freeze.EndDate,freeze.DaysApplied,freeze.DaysAlreadyCompensated,SubscriptionEndDate=sub.EndDate });
            await db.SaveChangesAsync(ct);return FreezeView(freeze);
        },ct);
    public Task<FreezeDetails> CompleteFreezeAsync(Guid id, Guid freezeId, CompleteFreezeRequest r, CancellationToken ct = default)
        => MutateSubscriptionAsync(id,SubscriptionPermissions.Freeze,async (sub,actor) =>
        {
            var freeze = await db.SubscriptionFreezes.SingleOrDefaultAsync(x => x.Id == freezeId && x.MemberSubscriptionId == id && x.FreezeType == FreezeType.Normal,ct)
                ?? throw new BranchNotFoundException("Normal freeze not found for this subscription.");
            await db.Entry(freeze).ReloadAsync(ct);if (freeze.CompletedAt != null) return FreezeView(freeze);
            Version(sub,r.ExpectedVersion);
            if (freeze.EndDate == null || await EffectiveBusinessDateAsync(sub.PurchasedAtBranchId,ct) <= freeze.EndDate)
                throw new BusinessRuleException("Completion is recorded after the approved freeze period ends.");
            freeze.CompletedAt=access.Now;freeze.CompletedByUserId=actor;sub.Version++;
            access.Audit(actor,sub.PurchasedAtBranchId,"Subscription.FreezeCompleted",nameof(SubscriptionFreeze),freeze.Id,
                after:new { sub.MemberId,SubscriptionId=id,freeze.CompletedAt });
            await db.SaveChangesAsync(ct);return FreezeView(freeze);
        },ct);
}
