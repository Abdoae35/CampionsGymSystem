using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    private async Task EnsureNoOverlapAsync(MemberSubscription sub, CancellationToken ct)
    {
        // Materialize possible conflicts so a successor shifted in this transaction uses its new tracked dates.
        var others = await db.MemberSubscriptions.Where(x => x.MemberId == sub.MemberId && x.Id != sub.Id
            && x.Status != SubscriptionStatus.Cancelled && (x.EndDate == null || x.EndDate >= sub.StartDate)
            && x.StartDate <= sub.EndDate).ToListAsync(ct);
        if (others.Any(x => (x.AccessMode == AccessMode.Unlimited || x.RemainingSessions > 0)
                && x.StartDate <= sub.EndDate && (x.EndDate == null || x.EndDate >= sub.StartDate)))
            throw new BusinessRuleException("Extension would overlap another entitlement or rewrite an already-started period.");
    }
    public Task<FreezeDetails> GrantCustomExtensionAsync(Guid id, CustomExtensionRequest r, CancellationToken ct = default)
        => MutateSubscriptionAsync(id,SubscriptionPermissions.Extend,async (sub,actor) =>
        {
            if (r.OperationId == Guid.Empty || r.Days <= 0 || string.IsNullOrWhiteSpace(r.Reason) || r.Reason.Length > 2048)
                throw new BusinessRuleException("Positive extension days, operation identity and a reason up to 2048 characters are required.");
            var reason = r.Reason.Trim();
            var prior = await db.SubscriptionFreezes.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId,ct);
            if (prior is not null)
            {
                if (prior.MemberSubscriptionId != id || prior.FreezeType != FreezeType.Medical || prior.DaysApplied != r.Days || prior.Reason != reason)
                    throw new BusinessRuleException("Operation identity belongs to another extension request.");
                return FreezeView(prior);
            }
            Version(sub,r.ExpectedVersion);
            var status = (await Evaluated().SingleAsync(x => x.Subscription.Id == id,ct)).Status;
            if (sub.EmployeeId != null || sub.EndDate == null || status is not (SubscriptionStatus.Active or SubscriptionStatus.Frozen))
                throw new BusinessRuleException("Custom extension requires a current customer entitlement; expired revival is not supported.");
            var date = await EffectiveBusinessDateAsync(sub.PurchasedAtBranchId,ct);var old = sub.EndDate.Value;
            sub.EndDate=PlusDays(old,r.Days);sub.Version++;
            await ShiftSuccessorAsync(sub,r.Days,actor,ct);await EnsureNoOverlapAsync(sub,ct);
            var extension = new SubscriptionFreeze { MemberSubscriptionId=id,OperationId=r.OperationId,FreezeType=FreezeType.Medical,
                StartDate=date,EndDate=PlusDays(date,r.Days-1),DaysApplied=r.Days,Reason=reason,
                ApprovedByUserId=actor,CreatedAt=access.Now,CompletedAt=access.Now,CompletedByUserId=actor,
                PreviousSubscriptionEndDate=old,NewSubscriptionEndDate=sub.EndDate };
            db.Add(extension);
            access.Audit(actor,sub.PurchasedAtBranchId,"Subscription.CustomExtensionGranted",nameof(SubscriptionFreeze),extension.Id,
                new { EndDate=old },new { sub.MemberId,SubscriptionId=id,sub.EndDate,Days=r.Days,Reason=reason });
            await db.SaveChangesAsync(ct);return FreezeView(extension);
        },ct);
}
