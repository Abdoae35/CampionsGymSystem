using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!; private ApplicationDbContext db = null!; private TestBranchUser user = null!;
    private OperationAccess access = null!; private SubscriptionService subscriptions = null!; private OfferService offers = null!;
    private ApplicationUser actor = null!; private GymBranch a = null!, b = null!, c = null!;
    private Member member = null!; private MembershipPlan plan = null!; private MembershipPlanCategory women = null!, kids = null!;
    private readonly DateOnly today = new(2026, 10, 10);
    private sealed class Clock : TimeProvider { public DateTimeOffset Now = new(2026, 10, 10, 10, 0, 0, TimeSpan.Zero); public override DateTimeOffset GetUtcNow() => Now; }
    private Clock clock = null!;
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext(); actor = new() { UserName = "sales-staff" };
        a = new() { Name = "A", City = new() { Name = "Tanta" } }; b = new() { Name = "B", City = a.City }; c = new() { Name = "C", City = new() { Name = "Cairo" } };
        member = new() { MemberCode = "M-SALES", FullName = "Member", PrimaryPhone = "sales-member", HomeBranch = a, CreatedByUser = actor };
        plan = new() { Name = "Monthly", Branch = b, City = a.City, DurationDays = 30, Price = 1200 };
        women = new() { Name = "Women" }; kids = new() { Name = "Kids" }; db.AddRange(member, plan, c, women, kids); await db.SaveChangesAsync();
        user = new(new(actor.Id, "staff", null, null, [], [..SalesPermissions.Codes, ..SubscriptionPermissions.Codes, "Members.Manage", "Branches.AccessAll"], new(b.Id, b.Name)));
        clock = new(); access = new(user, db, clock, Options.Create(new AuthContextOptions())); subscriptions = new(db, user, access); offers = new(db, user, access, subscriptions);
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private OfferRequest OfferInput(OfferEligibilityMode mode = OfferEligibilityMode.General, Guid? audience = null,
        OfferType type = OfferType.FixedDiscount, decimal value = 200) => new("Summer", type, value, [plan.Id], mode, audience, today.AddDays(-1), today.AddDays(1));
    private Task<PricingResult> Quote(Guid offer) => offers.QuoteAsync(member.Id, plan.Id, offer);
    [Fact] public async Task GeneralOfferDoesNotRequireMemberGrantOrRegistrationBranchMatch()
    {
        var offer = await offers.CreateAsync(OfferInput()); user.Context = user.Context! with { Permissions = [SalesPermissions.Create] };
        var p = await Quote(offer.Id); Assert.Equal(1000, p.FinalPrice); Assert.Empty(await db.MemberOfferEntitlements.ToListAsync());
        Assert.Equal(a.Id, member.HomeBranchId); Assert.Equal(b.Id, plan.BranchId);
    }
    [Fact] public async Task MemberOnlyRequiresCurrentGrantAndHonorsRevocation()
    {
        var offer = await offers.CreateAsync(OfferInput(OfferEligibilityMode.MemberOnly));
        await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id));
        var grant = await offers.GrantMemberAsync(offer.Id, new(member.Id, today, today));
        Assert.Equal(1000, (await Quote(offer.Id)).FinalPrice);
        Assert.Equal(grant.Id, (await offers.GrantMemberAsync(offer.Id, new(member.Id, today, today))).Id);
        await offers.RevokeMemberAsync(offer.Id, grant.Id); await offers.RevokeMemberAsync(offer.Id, grant.Id);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id)); Assert.Single(await db.MemberOfferEntitlements.ToListAsync());
    }
    [Theory][InlineData(-2, -1)][InlineData(1, 2)]
    public async Task ExpiredAndFutureMemberGrantsDoNotQualify(int from, int to)
    {
        var offer = await offers.CreateAsync(OfferInput(OfferEligibilityMode.MemberOnly));
        await offers.GrantMemberAsync(offer.Id, new(member.Id, today.AddDays(from), today.AddDays(to)));
        await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id));
    }
    [Fact] public async Task GeneralAudienceUsesExplicitDynamicAssignmentsRatherThanGenderStrings()
    {
        member.Gender = "Women"; await db.SaveChangesAsync(); var offer = await offers.CreateAsync(OfferInput(audience: women.Id));
        await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id));
        await offers.SetMemberAudiencesAsync(member.Id, new([women.Id, kids.Id])); Assert.Equal(1000, (await Quote(offer.Id)).FinalPrice);
        await offers.SetMemberAudiencesAsync(member.Id, new([kids.Id])); await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id));
        Assert.Equal(2, await db.MemberAudienceCategories.CountAsync()); Assert.Single(await db.MemberAudienceCategories.Where(x => x.EndedAt != null).ToListAsync());
    }
    [Fact] public async Task MemberOnlyAudienceRequiresBothIndependentConditions()
    {
        var offer = await offers.CreateAsync(OfferInput(OfferEligibilityMode.MemberOnly, women.Id));
        await offers.GrantMemberAsync(offer.Id, new(member.Id)); await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id));
        await offers.SetMemberAudiencesAsync(member.Id, new([women.Id])); Assert.Equal(1000, (await Quote(offer.Id)).FinalPrice);
    }
    [Fact] public async Task OfferCannotBroadenAuthoritativePlanAudience()
    {
        plan.MembershipPlanCategoryId = women.Id; await db.SaveChangesAsync(); var offer = await offers.CreateAsync(OfferInput());
        await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id));
        await Assert.ThrowsAsync<BusinessRuleException>(() => subscriptions.CreateCustomerGrantAsync(new(member.Id, plan.Id, b.Id, Guid.NewGuid())));
        await offers.SetMemberAudiencesAsync(member.Id, new([women.Id])); Assert.Equal(1000, (await Quote(offer.Id)).FinalPrice);
    }
    [Theory][InlineData(OfferType.PercentageDiscount, 25, 900, 0)][InlineData(OfferType.ExtraDays, 7, 1200, 7)]
    public async Task CommercialPricingSupportsExistingOfferTypes(OfferType type, int value, int price, int days)
    { var offer = await offers.CreateAsync(OfferInput(type: type, value: value)); var p = await Quote(offer.Id); Assert.Equal(price, p.FinalPrice); Assert.Equal(days, p.ExtraDays); }
    [Fact] public async Task WrongBranchPlanInactiveAndOutOfDateOfferAreRejected()
    {
        var offer = await offers.CreateAsync(OfferInput());
        await Assert.ThrowsAsync<BusinessRuleException>(() => offers.QuoteAsync(member.Id, plan.Id, offer.Id, a.Id));
        await offers.UpdateAsync(offer.Id, OfferInput() with { EndDate = today.AddDays(-1) }); await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id));
        await offers.UpdateAsync(offer.Id, OfferInput() with { IsActive = false }); await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id));
        await offers.UpdateAsync(offer.Id, OfferInput()); plan.IsActive = false; await db.SaveChangesAsync(); await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id));
    }
    [Fact] public async Task OfferPermissionsAndMemberAudienceScopeAreAuthoritative()
    {
        var offer = await offers.CreateAsync(OfferInput()); user.Context = user.Context! with { Permissions = [SalesPermissions.Create, "Members.OwnSalesCustomers"] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => offers.UpdateAsync(offer.Id, OfferInput()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => offers.GrantMemberAsync(offer.Id, new(member.Id)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => offers.SetMemberAudiencesAsync(member.Id, new([women.Id])));
        user.Context = user.Context! with { Permissions = ["Members.Manage"] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => offers.SetMemberAudiencesAsync(member.Id, new([women.Id])));
    }
    [Fact] public async Task UnclassifiedLegacyOfferFailsClosedAndCannotBeInferredFromGrantRows()
    {
        var offer = new Offer { BranchId = b.Id, Name = "Legacy", OfferType = OfferType.FixedDiscount, Value = 200, CreatedByUserId = actor.Id };
        db.AddRange(offer, new OfferMembershipPlan { OfferId = offer.Id, MembershipPlanId = plan.Id }, new MemberOfferEntitlement { MemberId = member.Id, OfferId = offer.Id, CreatedByUserId = actor.Id }); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(() => Quote(offer.Id));
        await offers.UpdateAsync(offer.Id, OfferInput()); Assert.Equal(1000, (await Quote(offer.Id)).FinalPrice);
    }
    [Fact] public async Task OfferEditsNeverRewriteStoredSaleTerms()
    {
        var offer = await offers.CreateAsync(OfferInput()); var pricing = await Quote(offer.Id);
        var sale = new SaleTransaction { OperationId = Guid.NewGuid(), RequestHash = new('A', 64), MemberId = member.Id, BranchId = b.Id, BusinessDate = today,
            PurchaseMode = PurchaseMode.Reservation, ActivationMode = SubscriptionActivationMode.Now, GrossAmount = pricing.OriginalPrice,
            DiscountAmount = pricing.Discount, NetAmount = pricing.FinalPrice, OfferId = offer.Id, OfferNameSnapshot = pricing.OfferName,
            OfferTypeSnapshot = pricing.OfferType, OfferValueSnapshot = pricing.OfferValue, OfferEligibilityModeSnapshot = pricing.EligibilityMode,
            CommissionEligible = false, TargetEligible = false, CreatedByUserId = actor.Id };
        db.Add(sale); await db.SaveChangesAsync(); await offers.UpdateAsync(offer.Id, OfferInput(OfferEligibilityMode.MemberOnly, women.Id, value: 300));
        var stored = await db.SaleTransactions.AsNoTracking().SingleAsync(); Assert.Equal(1000, stored.NetAmount); Assert.Equal(200, stored.DiscountAmount);
        Assert.Equal(OfferEligibilityMode.General, stored.OfferEligibilityModeSnapshot); Assert.Null(stored.OfferAudienceCategoryIdSnapshot);
    }
    [Theory][InlineData(OfferType.ExtraDays, 1.5)][InlineData(OfferType.PercentageDiscount, 101)]
    public async Task InvalidOfferValuesAreRejected(OfferType type, double amount)
    { await Assert.ThrowsAsync<BusinessRuleException>(() => offers.CreateAsync(OfferInput(type: type, value: (decimal)amount))); }
}
