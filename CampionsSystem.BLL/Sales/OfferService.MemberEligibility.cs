using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
public sealed partial class OfferService
{
    public async Task<MemberOfferGrantDetails> GrantMemberAsync(Guid offerId, GrantMemberOfferRequest r, CancellationToken ct = default)
    {
        var actor = await current.GetAsync(ct);
        if (actor is null || !actor.Permissions.Contains(SalesPermissions.Offers)) throw new UnauthorizedAccessException();
        if (r.MemberId == Guid.Empty || r.StartDate > r.EndDate) throw new BusinessRuleException("Invalid member grant dates.");
        return await TransactionAsync(async () =>
        {
            var member = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={r.MemberId} FOR UPDATE")
                .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Member not found.");
            await db.Entry(member).ReloadAsync(ct);
            var offer = await LockedOfferAsync(offerId, ct); await RequireAsync(SalesPermissions.Offers, offer.BranchId, ct);
            if (!member.IsActive || !offer.IsActive || offer.EligibilityMode != OfferEligibilityMode.MemberOnly)
                throw new BusinessRuleException("Select an active Member and MemberOnly Offer.");
            var existing = await db.MemberOfferEntitlements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).FirstOrDefaultAsync(x => x.MemberId == member.Id && x.OfferId == offerId
                && x.RevokedAt == null && x.StartDate == r.StartDate && x.EndDate == r.EndDate, ct);
            if (existing != null) return GrantDetails(existing);
            var grant = new MemberOfferEntitlement { MemberId = member.Id, OfferId = offerId, StartDate = r.StartDate, EndDate = r.EndDate,
                CreatedByUserId = actor.UserId, GrantedByEmployeeId = actor.EmployeeId, CreatedAt = access.Now };
            db.Add(grant); access.Audit(actor.UserId, offer.BranchId, "Offer.MemberGranted", nameof(MemberOfferEntitlement), grant.Id,
                after: new { grant.MemberId, grant.OfferId, grant.StartDate, grant.EndDate });
            await db.SaveChangesAsync(ct); return GrantDetails(grant);
        }, ct);
    }
    public async Task<MemberOfferGrantDetails> RevokeMemberAsync(Guid offerId, Guid grantId, CancellationToken ct = default)
    {
        var actor = await current.GetAsync(ct);
        if (actor is null || !actor.Permissions.Contains(SalesPermissions.Offers)) throw new UnauthorizedAccessException();
        var memberId = await db.MemberOfferEntitlements.Where(x => x.Id == grantId && x.OfferId == offerId).Select(x => (Guid?)x.MemberId).SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Offer grant not found.");
        return await TransactionAsync(async () =>
        {
            await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={memberId} FOR UPDATE").SingleAsync(ct);
            var offer = await LockedOfferAsync(offerId, ct); await RequireAsync(SalesPermissions.Offers, offer.BranchId, ct);
            var grant = await db.MemberOfferEntitlements.SingleAsync(x => x.Id == grantId && x.OfferId == offerId, ct); await db.Entry(grant).ReloadAsync(ct);
            if (grant.RevokedAt != null) return GrantDetails(grant);
            grant.RevokedAt = access.Now; grant.RevokedByUserId = actor.UserId;
            access.Audit(actor.UserId, offer.BranchId, "Offer.MemberRevoked", nameof(MemberOfferEntitlement), grant.Id,
                after: new { grant.MemberId, grant.OfferId, grant.RevokedAt });
            await db.SaveChangesAsync(ct); return GrantDetails(grant);
        }, ct);
    }
    private static MemberOfferGrantDetails GrantDetails(MemberOfferEntitlement g) => new(g.Id, g.MemberId, g.OfferId, g.StartDate, g.EndDate, g.RevokedAt);
    public async Task<IReadOnlyList<MemberAudienceDetails>> SetMemberAudiencesAsync(Guid memberId, SetMemberAudienceRequest r, CancellationToken ct = default)
    {
        var actor = await current.GetAsync(ct);
        if (actor is null || !actor.Permissions.Contains("Members.Manage")) throw new UnauthorizedAccessException();
        if (r.AudienceCategoryIds is null || r.AudienceCategoryIds.Count > 100 || r.AudienceCategoryIds.Any(x => x == Guid.Empty)
            || r.AudienceCategoryIds.Distinct().Count() != r.AudienceCategoryIds.Count) throw new BusinessRuleException("Invalid audience assignments.");
        return await TransactionAsync(async () =>
        {
            var member = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={memberId} FOR UPDATE")
                .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Member not found.");
            await db.Entry(member).ReloadAsync(ct); BranchAccessService.EnsureScope(actor, member.HomeBranchId);
            var categories = await db.MembershipPlanCategories.Where(x => r.AudienceCategoryIds.Contains(x.Id) && x.IsActive).ToListAsync(ct);
            if (categories.Count != r.AudienceCategoryIds.Count) throw new BusinessRuleException("Assign only active configured audience categories.");
            var existing = await db.MemberAudienceCategories.Where(x => x.MemberId == memberId && x.EndedAt == null).ToListAsync(ct);
            foreach (var row in existing.Where(x => !r.AudienceCategoryIds.Contains(x.MembershipPlanCategoryId))) row.EndedAt = access.Now;
            foreach (var id in r.AudienceCategoryIds.Where(id => existing.All(x => x.MembershipPlanCategoryId != id)))
                db.Add(new MemberAudienceCategory { MemberId = memberId, MembershipPlanCategoryId = id, AssignedByUserId = actor.UserId, AssignedAt = access.Now });
            access.Audit(actor.UserId, member.HomeBranchId, "Member.AudiencesChanged", nameof(Member), member.Id,
                new { Categories = existing.Select(x => x.MembershipPlanCategoryId).ToArray() }, new { Categories = r.AudienceCategoryIds });
            await db.SaveChangesAsync(ct);
            return await db.MemberAudienceCategories.Where(x => x.MemberId == memberId && x.EndedAt == null).OrderBy(x => x.MembershipPlanCategory.Name)
                .Select(x => new MemberAudienceDetails(x.Id, x.MembershipPlanCategoryId, x.MembershipPlanCategory.Name, x.AssignedAt, x.EndedAt)).ToListAsync(ct);
        }, ct);
    }
}
