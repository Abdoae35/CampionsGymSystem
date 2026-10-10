using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;

public sealed partial class OfferService(ApplicationDbContext db, ICurrentUserService current,
    OperationAccess access, SubscriptionService subscriptions)
{
    private async Task<CurrentUserContext> RequireAsync(string permission, Guid? branch, CancellationToken ct)
    {
        var actor = await current.GetAsync(ct);
        if (actor is null || !actor.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        BranchAccessService.EnsureScope(actor, branch);
        return actor;
    }
    private async Task<Guid> SellingBranchAsync(Guid? selected, CancellationToken ct)
    {
        var actor = await current.GetAsync(ct) ?? throw new UnauthorizedAccessException();
        var id = selected ?? actor.CurrentBranchId ?? throw new BusinessRuleException("Select an operational branch.");
        await RequireAsync(SalesPermissions.Create, id, ct);
        if (!await db.GymBranches.AnyAsync(x => x.Id == id && x.IsActive && x.City.IsActive, ct))
            throw new BusinessRuleException("Selling branch must be active.");
        return id;
    }
    private async Task<T> TransactionAsync<T>(Func<Task<T>> change, CancellationToken ct)
    {
        var owned = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        var tx = db.Database.CurrentTransaction!; var point = "offer_" + Guid.NewGuid().ToString("N");
        try { await tx.CreateSavepointAsync(point, ct); var result = await change(); await tx.ReleaseSavepointAsync(point, ct); if (owned != null) await owned.CommitAsync(ct); return result; }
        catch { if (owned != null) await owned.RollbackAsync(CancellationToken.None); else await tx.RollbackToSavepointAsync(point, CancellationToken.None); db.ChangeTracker.Clear(); throw; }
        finally { if (owned != null) await owned.DisposeAsync(); }
    }
    private async Task<Offer> LockedOfferAsync(Guid id, CancellationToken ct)
    {
        var offer = await db.Offers.FromSqlInterpolated($"SELECT * FROM \"Offers\" WHERE \"Id\"={id} FOR UPDATE")
            .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Offer not found.");
        await db.Entry(offer).ReloadAsync(ct); return offer;
    }
    private async Task<OfferDetails> DetailsAsync(Offer o, CancellationToken ct) => new(o.Id, o.BranchId, o.Name,
        o.OfferType.ToString(), o.Value, o.StartDate, o.EndDate, o.IsPermanent, o.IsActive,
        await db.OfferMembershipPlans.Where(x => x.OfferId == o.Id).OrderBy(x => x.MembershipPlanId).Select(x => x.MembershipPlanId).ToListAsync(ct),
        o.EligibilityMode?.ToString(), o.AudienceCategoryId);
    public async Task<OfferDetails> GetAsync(Guid id, CancellationToken ct = default)
    {
        var o = await db.Offers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Offer not found.");
        await RequireAsync(SalesPermissions.Offers, o.BranchId, ct); return await DetailsAsync(o, ct);
    }
    public Task<OfferDetails> CreateAsync(OfferRequest r, CancellationToken ct = default) => SaveAsync(null, r, ct);
    public Task<OfferDetails> UpdateAsync(Guid id, OfferRequest r, CancellationToken ct = default) => SaveAsync(id, r, ct);
    private async Task<OfferDetails> SaveAsync(Guid? id, OfferRequest r, CancellationToken ct)
    {
        var actor = await current.GetAsync(ct) ?? throw new UnauthorizedAccessException();
        var branch = r.BranchId ?? (id.HasValue ? await db.Offers.Where(x => x.Id == id).Select(x => x.BranchId).SingleOrDefaultAsync(ct) : actor.CurrentBranchId);
        actor = await RequireAsync(SalesPermissions.Offers, branch, ct);
        if (string.IsNullOrWhiteSpace(r.Name) || r.Name.Trim().Length > 256 || r.Description?.Length > 2000
            || !Enum.IsDefined(r.Type) || r.EligibilityMode is null || !Enum.IsDefined(r.EligibilityMode.Value)
            || r.StartDate > r.EndDate || r.MembershipPlanIds is null || r.MembershipPlanIds.Count is < 1 or > 100
            || r.MembershipPlanIds.Any(x => x == Guid.Empty) || r.MembershipPlanIds.Distinct().Count() != r.MembershipPlanIds.Count)
            throw new BusinessRuleException("Invalid Offer configuration; explicit eligibility mode and linked plans are required.");
        OperationAccess.Money(r.Value);
        if (r.Value <= 0 || r.Value > 9999999999999999.99m || r.Type == OfferType.PercentageDiscount && r.Value > 100
            || r.Type == OfferType.ExtraDays && (r.Value != decimal.Truncate(r.Value) || r.Value > 36500))
            throw new BusinessRuleException("Invalid discount or extra-day value.");
        return await TransactionAsync(async () =>
        {
            var offer = id.HasValue ? await LockedOfferAsync(id.Value, ct) : new Offer { CreatedByUserId = actor.UserId, CreatedAt = access.Now };
            if (id.HasValue) await RequireAsync(SalesPermissions.Offers, offer.BranchId, ct);
            if (id.HasValue && offer.BranchId != branch) throw new BusinessRuleException("Offer branch ownership cannot be reassigned.");
            if (branch.HasValue && !await db.GymBranches.AnyAsync(x => x.Id == branch && x.IsActive, ct)) throw new BusinessRuleException("Select an active branch.");
            if (r.AudienceCategoryId.HasValue && !await db.MembershipPlanCategories.AnyAsync(x => x.Id == r.AudienceCategoryId && x.IsActive, ct))
                throw new BusinessRuleException("Select an active audience category.");
            var plans = await db.MembershipPlans.Where(x => r.MembershipPlanIds.Contains(x.Id)).ToListAsync(ct);
            if (plans.Count != r.MembershipPlanIds.Count || plans.Any(x => x.PlanCategory == PlanCategory.Employee || x.BranchId == null || branch != null && x.BranchId != branch))
                throw new BusinessRuleException("Offer links must be customer plans in its owning branch.");
            var old = id.HasValue ? new { offer.Name, offer.Value, offer.EligibilityMode, offer.AudienceCategoryId, offer.StartDate, offer.EndDate, offer.IsActive } : null;
            offer.Name = r.Name.Trim(); offer.Description = r.Description?.Trim(); offer.BranchId = branch; offer.OfferType = r.Type;
            offer.Value = r.Value; offer.StartDate = r.StartDate; offer.EndDate = r.EndDate; offer.IsPermanent = r.IsPermanent;
            offer.EligibilityMode = r.EligibilityMode; offer.AudienceCategoryId = r.AudienceCategoryId; offer.IsActive = r.IsActive;
            if (!id.HasValue) db.Add(offer);
            var links = await db.OfferMembershipPlans.Where(x => x.OfferId == offer.Id).ToListAsync(ct);
            db.RemoveRange(links.Where(x => !r.MembershipPlanIds.Contains(x.MembershipPlanId)));
            foreach (var planId in r.MembershipPlanIds.Where(x => links.All(l => l.MembershipPlanId != x)))
                db.Add(new OfferMembershipPlan { OfferId = offer.Id, MembershipPlanId = planId });
            access.Audit(actor.UserId, branch, id.HasValue ? "Offer.Updated" : "Offer.Created", nameof(Offer), offer.Id, old,
                new { offer.Name, offer.Value, offer.OfferType, offer.EligibilityMode, offer.AudienceCategoryId, offer.StartDate, offer.EndDate, offer.IsActive, Plans = r.MembershipPlanIds });
            await db.SaveChangesAsync(ct); return await DetailsAsync(offer, ct);
        }, ct);
    }
    public async Task<PricingResult> QuoteAsync(Guid memberId, Guid planId, Guid? offerId, Guid? branchId = null, CancellationToken ct = default)
    {
        var branch = await SellingBranchAsync(branchId, ct);
        var plan = await db.MembershipPlans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == planId, ct) ?? throw new BranchNotFoundException("Plan not found.");
        var member = await db.Members.AsNoTracking().SingleOrDefaultAsync(x => x.Id == memberId, ct) ?? throw new BranchNotFoundException("Member not found.");
        return await PriceAsync(member, plan, offerId, branch, await subscriptions.EffectiveBusinessDateAsync(branch, ct), ct);
    }
    internal async Task<PricingResult> PriceAsync(Member member, MembershipPlan plan, Guid? offerId, Guid branch, DateOnly date, CancellationToken ct)
    {
        if (!plan.IsActive || plan.BranchId != branch || !await db.Cities.AnyAsync(x => x.Id == plan.CityId && x.IsActive, ct))
            throw new BusinessRuleException("Select an active customer plan sold by the operational branch.");
        await subscriptions.ValidateSaleCustomerAsync(member, plan, ct);
        if (!offerId.HasValue) return new(plan.Price, 0, plan.Price, 0, null, null, null, null);
        var offer = await db.Offers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == offerId, ct);
        if (offer is null || !offer.IsActive || offer.EligibilityMode is null || offer.BranchId != null && offer.BranchId != branch
            || offer.StartDate > date || offer.EndDate < date || !await db.OfferMembershipPlans.AnyAsync(x => x.OfferId == offerId && x.MembershipPlanId == plan.Id, ct))
            throw new BusinessRuleException("Offer is not applicable to this plan, selling branch or business date.");
        if (!await subscriptions.MatchesAudienceAsync(member.Id, offer.AudienceCategoryId, ct)) throw new BusinessRuleException("Member does not match the Offer audience.");
        if (offer.EligibilityMode == OfferEligibilityMode.MemberOnly && !await db.MemberOfferEntitlements.AnyAsync(x => x.OfferId == offer.Id
            && x.MemberId == member.Id && x.RevokedAt == null && x.CreatedAt <= access.Now && (x.StartDate == null || x.StartDate <= date) && (x.EndDate == null || x.EndDate >= date), ct))
            throw new BusinessRuleException("MemberOnly Offer requires a current MemberOfferEntitlement.");
        return Calculate(plan.Price, offer);
    }
    private static PricingResult Calculate(decimal price, Offer offer)
    {
        var discount = offer.OfferType switch { OfferType.FixedDiscount => offer.Value,
            OfferType.PercentageDiscount => decimal.Round(price * offer.Value / 100m, 2, MidpointRounding.AwayFromZero), OfferType.ExtraDays => 0m,
            _ => throw new BusinessRuleException("Unsupported Offer type.") };
        if (discount < 0 || discount >= price || offer.Value <= 0 || offer.OfferType == OfferType.PercentageDiscount && offer.Value > 100
            || offer.OfferType == OfferType.ExtraDays && (offer.Value != decimal.Truncate(offer.Value) || offer.Value > 36500))
            throw new BusinessRuleException("Offer must preserve a positive valid paid price and valid extra-day quantity.");
        return new(price, discount, price - discount, offer.OfferType == OfferType.ExtraDays ? (int)offer.Value : 0,
            offer.Id, offer.Name, offer.OfferType, offer.Value, offer.EligibilityMode, offer.AudienceCategoryId);
    }
    public async Task<IReadOnlyList<OfferDetails>> GetEligibleAsync(Guid memberId, Guid planId, Guid? branchId = null, CancellationToken ct = default)
    {
        var branch = await SellingBranchAsync(branchId, ct);
        // Validate plan eligibility even when there are no Offers, then evaluate the bounded catalog.
        await QuoteAsync(memberId, planId, null, branch, ct);
        var date = await subscriptions.EffectiveBusinessDateAsync(branch, ct);
        var ids = await db.Offers.Where(x => x.IsActive && x.EligibilityMode != null && (x.StartDate == null || x.StartDate <= date) && (x.EndDate == null || x.EndDate >= date) && (x.BranchId == null || x.BranchId == branch)
            && db.OfferMembershipPlans.Any(l => l.OfferId == x.Id && l.MembershipPlanId == planId)).OrderBy(x => x.Name).ThenBy(x => x.Id).Select(x => x.Id).Take(100).ToListAsync(ct);
        var results = new List<OfferDetails>();
        foreach (var id in ids)
        {
            try { await QuoteAsync(memberId, planId, id, branch, ct); results.Add(await DetailsAsync(await db.Offers.AsNoTracking().SingleAsync(x => x.Id == id, ct), ct)); }
            catch (BusinessRuleException) { /* Ineligible offers are omitted; applying one still fails authoritatively. */ }
        }
        return results;
    }
}
