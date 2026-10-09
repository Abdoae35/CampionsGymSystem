using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CampionsSystem.DAL.Tests;

public sealed class MembershipBenefitsTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private GymBranch branch = null!;
    private MembershipPlan plan = null!;
    private MembershipSubscriptionTransaction transaction = null!;

    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync();
        db = fixture.CreateContext();
        var user = new ApplicationUser { UserName = "benefits-test" };
        branch = new GymBranch { Name = "Benefits Branch", City = new City { Name = "Benefits City" } };
        var member = new Member { ClientCode = 1, MemberCode = "BENEFITS", FullName = "Member", PrimaryPhone = "benefits-member", HomeBranch = branch, CreatedByUser = user };
        plan = new MembershipPlan { Name = "Premium", City = branch.City, DurationDays = 90, Price = 1500 };
        var subscription = new MemberSubscription { Member = member, MembershipPlan = plan, City = branch.City,
            PurchasedAtBranch = branch, StartDate = new(2026, 10, 7), EndDate = new(2027, 1, 5) };
        var sale = new SaleTransaction { Member = member, Branch = branch, CreatedByUser = user,
            SaleType = SaleType.GymMembership, GrossAmount = 1500, NetAmount = 1500 };
        transaction = new MembershipSubscriptionTransaction { MemberSubscription = subscription, MembershipPlan = plan,
            SaleTransaction = sale, PurchasedDurationDays = 90, PlanPriceSnapshot = 1500, NetPriceSnapshot = 1500, NewEndDate = subscription.EndDate!.Value };
        db.Add(transaction);
        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private static MembershipBenefit Benefit(string code = "INBODY") => new() { Code = code, Name = code };
    private MembershipPlanBenefit Configuration(MembershipBenefit benefit, int? quantity = 2,
        MembershipBenefitGrantType type = MembershipBenefitGrantType.Quantity) => new()
        { MembershipPlan = plan, MembershipBenefit = benefit, GrantType = type, Quantity = quantity };
    private MemberSubscriptionBenefit Grant(MembershipBenefit benefit, int? quantity = 2,
        MembershipBenefitGrantType type = MembershipBenefitGrantType.Quantity) => new()
        { MembershipSubscriptionTransaction = transaction, MembershipBenefit = benefit,
            BenefitCodeSnapshot = benefit.Code, BenefitNameSnapshot = benefit.Name, GrantTypeSnapshot = type, GrantedQuantity = quantity };
    private MemberBenefitUsage Usage(MemberSubscriptionBenefit grant, int quantity = 1) => new()
        { MemberSubscriptionBenefit = grant, Branch = branch, QuantityUsed = quantity };

    [Fact]
    public async Task PlanMayHaveZeroOrMultipleBenefitsAndCatalogIsReusable()
    {
        Assert.Empty(await db.MembershipPlanBenefits.ToListAsync());
        var benefit = Benefit();
        var otherPlan = new MembershipPlan { Name = "Annual", CityId = plan.CityId, DurationDays = 365 };
        db.AddRange(Configuration(benefit), Configuration(Benefit("GUEST")),
            new MembershipPlanBenefit { MembershipPlan = otherPlan, MembershipBenefit = benefit, Quantity = 5 });
        await db.SaveChangesAsync();
        Assert.Equal(2, await db.MembershipPlanBenefits.CountAsync(x => x.MembershipPlanId == plan.Id));
        Assert.Equal(2, await db.MembershipPlanBenefits.CountAsync(x => x.MembershipBenefitId == benefit.Id));
    }

    [Fact]
    public async Task DuplicatePlanBenefitRejected()
    {
        var benefit = Benefit(); db.Add(Configuration(benefit)); await db.SaveChangesAsync();
        db.Add(Configuration(benefit, 5));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Theory]
    [InlineData(MembershipBenefitGrantType.Quantity, 3)]
    [InlineData(MembershipBenefitGrantType.Access, null)]
    public async Task ValidPlanGrantConfigurationPersists(MembershipBenefitGrantType type, int? quantity)
    {
        db.Add(Configuration(Benefit(), quantity, type)); await db.SaveChangesAsync();
        var row = await db.MembershipPlanBenefits.AsNoTracking().SingleAsync();
        Assert.Equal(type, row.GrantType); Assert.Equal(quantity, row.Quantity);
    }

    [Theory]
    [InlineData(MembershipBenefitGrantType.Quantity, null)]
    [InlineData(MembershipBenefitGrantType.Quantity, 0)]
    [InlineData(MembershipBenefitGrantType.Quantity, -1)]
    [InlineData(MembershipBenefitGrantType.Access, 1)]
    [InlineData((MembershipBenefitGrantType)99, null)]
    public async Task InvalidPlanGrantRejected(MembershipBenefitGrantType type, int? quantity)
    {
        db.Add(Configuration(Benefit(), quantity, type));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task SameCatalogBenefitCanBeQuantityOrAccessOnDifferentPlans()
    {
        var benefit = Benefit(); db.Add(Configuration(benefit));
        db.Add(new MembershipPlanBenefit { MembershipBenefit = benefit,
            MembershipPlan = new MembershipPlan { Name = "Unlimited benefit", CityId = plan.CityId, DurationDays = 365 },
            GrantType = MembershipBenefitGrantType.Access });
        await db.SaveChangesAsync(); Assert.Equal(2, await db.MembershipPlanBenefits.CountAsync());
    }

    [Fact]
    public async Task TransactionHasMultipleSnapshotsIndependentOfConfigurationAndCatalogChanges()
    {
        var benefit = Benefit(); var config = Configuration(benefit); var oldGrant = Grant(benefit);
        db.AddRange(config, oldGrant, Grant(Benefit("SAUNA"), null, MembershipBenefitGrantType.Access));
        await db.SaveChangesAsync();
        config.Quantity = 1; benefit.Code = "INBODY_NEW"; benefit.Name = "Renamed";
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        var grants = await db.MemberSubscriptionBenefits.AsNoTracking().Where(x => x.MembershipSubscriptionTransactionId == transaction.Id).ToListAsync();
        Assert.Equal(2, grants.Count);
        var saved = grants.Single(x => x.Id == oldGrant.Id);
        Assert.Equal(2, saved.GrantedQuantity); Assert.Equal("INBODY", saved.BenefitCodeSnapshot); Assert.Equal("INBODY", saved.BenefitNameSnapshot);
        Assert.Null(grants.Single(x => x.GrantTypeSnapshot == MembershipBenefitGrantType.Access).GrantedQuantity);
    }

    [Fact]
    public async Task RenewalRetainsSeparateSourceBatchAndUpdatedGrantQuantity()
    {
        var benefit = Benefit(); var config = Configuration(benefit); db.AddRange(config, Grant(benefit)); await db.SaveChangesAsync();
        config.Quantity = 3;
        var renewal = new MembershipSubscriptionTransaction { MemberSubscriptionId = transaction.MemberSubscriptionId,
            MembershipPlanId = plan.Id, SaleTransaction = new SaleTransaction { MemberId = transaction.MemberSubscription.MemberId,
                BranchId = branch.Id, CreatedByUserId = transaction.SaleTransaction!.CreatedByUserId, SaleType = SaleType.GymMembership,
                GrossAmount = 1500, NetAmount = 1500 }, TransactionType = SubscriptionTransactionType.Renewal,
            PurchasedDurationDays = 90, PlanPriceSnapshot = 1500, NetPriceSnapshot = 1500,
            PreviousEndDate = transaction.NewEndDate, NewEndDate = transaction.NewEndDate!.Value.AddDays(90) };
        var grant = Grant(benefit, config.Quantity); grant.MembershipSubscriptionTransaction = renewal;
        db.Add(grant); await db.SaveChangesAsync();
        var grants = await db.MemberSubscriptionBenefits.AsNoTracking().OrderBy(x => x.GrantedQuantity).ToListAsync();
        Assert.Equal(new int?[] { 2, 3 }, grants.Select(x => x.GrantedQuantity));
        Assert.NotEqual(grants[0].MembershipSubscriptionTransactionId, grants[1].MembershipSubscriptionTransactionId);
    }

    [Theory]
    [InlineData(MembershipBenefitGrantType.Quantity, null)]
    [InlineData(MembershipBenefitGrantType.Quantity, 0)]
    [InlineData(MembershipBenefitGrantType.Access, 1)]
    public async Task InvalidSnapshotQuantityRejected(MembershipBenefitGrantType type, int? quantity)
    { db.Add(Grant(Benefit(), quantity, type)); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync()); }

    [Fact]
    public async Task DuplicateGrantForSameTransactionAndBenefitRejected()
    {
        var benefit = Benefit(); db.Add(Grant(benefit)); await db.SaveChangesAsync(); db.Add(Grant(benefit));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task UsageTracksSpecificGrantBranchActorAndRemainingQuantityWithoutFinance()
    {
        var grant = Grant(Benefit(), 5); var other = Grant(Benefit("OTHER"), 7);
        db.AddRange(Usage(grant, 2), Usage(grant), other); await db.SaveChangesAsync();
        var used = await db.MemberBenefitUsages.Where(x => x.MemberSubscriptionBenefitId == grant.Id).SumAsync(x => x.QuantityUsed);
        Assert.Equal(5, grant.GrantedQuantity); // Grant is unchanged by usage history.
        Assert.Equal(2, grant.GrantedQuantity - used);
        Assert.False(await db.MemberBenefitUsages.AnyAsync(x => x.MemberSubscriptionBenefitId == other.Id));
        Assert.All(await db.MemberBenefitUsages.ToListAsync(), x => { Assert.Equal(branch.Id, x.BranchId); Assert.Null(x.CreatedByUserId); });
        Assert.Equal(1, await db.SaleTransactions.CountAsync()); Assert.Empty(await db.Payments.ToListAsync());
        Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }

    [Theory] [InlineData(0)] [InlineData(-1)]
    public async Task NonpositiveUsageRejected(int quantity)
    { db.Add(Usage(Grant(Benefit()), quantity)); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync()); }

    [Fact]
    public async Task DeactivationAndPlanConfigurationRemovalPreserveGrantAndUsage()
    {
        var benefit = Benefit(); var config = Configuration(benefit); var grant = Grant(benefit);
        db.AddRange(config, Usage(grant)); await db.SaveChangesAsync();
        benefit.IsActive = false; db.Remove(config); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        Assert.False((await db.MembershipBenefits.SingleAsync()).IsActive);
        Assert.Single(await db.MemberSubscriptionBenefits.ToListAsync()); Assert.Single(await db.MemberBenefitUsages.ToListAsync());
    }

    [Theory] [InlineData("Catalog")] [InlineData("Grant")] [InlineData("Transaction")] [InlineData("Plan")]
    public async Task ReferencedHistoryCannotBeCascadeDeleted(string principal)
    {
        var benefit = Benefit(); var grant = Grant(benefit); db.Add(Usage(grant)); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        object row = principal switch { "Catalog" => await db.MembershipBenefits.SingleAsync(),
            "Grant" => await db.MemberSubscriptionBenefits.SingleAsync(),
            "Transaction" => await db.MembershipSubscriptionTransactions.SingleAsync(), _ => await db.MembershipPlans.SingleAsync() };
        db.Remove(row);
        if (principal is "Grant" or "Transaction") await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        else await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Theory] [InlineData(true)] [InlineData(false)]
    public async Task CatalogCodeAndNameAreUnique(bool code)
    {
        db.Add(Benefit()); await db.SaveChangesAsync();
        db.Add(new MembershipBenefit { Code = code ? "INBODY" : "OTHER", Name = code ? "OTHER" : "INBODY" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public void BenefitModelIsNormalizedAndHistoryRelationshipsRestrictDeletion()
    {
        foreach (var type in new[] { typeof(MembershipBenefit), typeof(MembershipPlanBenefit), typeof(MemberSubscriptionBenefit), typeof(MemberBenefitUsage) })
        {
            var entity = db.Model.FindEntityType(type)!;
            Assert.All(entity.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
            Assert.DoesNotContain(entity.GetProperties(), p => p.IsShadowProperty());
        }
        var grant = db.Model.FindEntityType(typeof(MemberSubscriptionBenefit))!;
        Assert.Null(grant.FindProperty("MemberSubscriptionId")); Assert.Null(grant.FindProperty("ValidUntil"));
        Assert.Null(grant.FindProperty("UsedQuantity")); Assert.Null(db.Model.FindEntityType(typeof(MembershipBenefit))!.FindProperty("GrantType"));
    }

    [Fact]
    public void MigrationOnlyCreatesFourBenefitTablesAndIndexes()
    {
        var assembly = db.GetService<IMigrationsAssembly>();
        var migration = assembly.CreateMigration(assembly.Migrations.Single(x => x.Key.EndsWith("_AddMembershipBenefits")).Value, db.Database.ProviderName!);
        Assert.Equal(new[] { "MemberBenefitUsages", "MemberSubscriptionBenefits", "MembershipBenefits", "MembershipPlanBenefits" },
            migration.UpOperations.OfType<CreateTableOperation>().Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal));
        Assert.All(migration.UpOperations, operation => Assert.True(operation is CreateTableOperation or CreateIndexOperation));
    }
}
