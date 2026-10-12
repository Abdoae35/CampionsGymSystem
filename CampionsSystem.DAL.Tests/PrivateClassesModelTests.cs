using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace CampionsSystem.DAL.Tests;

public sealed class PrivateClassesModelTests
{
    private static ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Database=unused").Options);
    [Fact] public void EarningsPayoutsAndSharedPackagesUseRestrictiveForeignKeys()
    {
        using var db = Context();
        foreach (var type in new[] { typeof(CoachEarning), typeof(CoachPayout), typeof(CoachPayoutAllocation), typeof(CoachTargetRule), typeof(ClassesSubscription), typeof(ClassesPackage) })
            Assert.All(db.Model.FindEntityType(type)!.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        Assert.Equal(2, db.Model.FindEntityType(typeof(CoachEarning))!.FindProperty("EligibleValue")!.GetScale());
        Assert.True(db.Model.FindEntityType(typeof(ClassesSubscription))!.FindProperty("Version")!.IsConcurrencyToken);
        Assert.True(db.Model.FindEntityType(typeof(PrivateSubscriptionTransaction))!.FindProperty("OriginalCoachEmployeeId")!.IsNullable);
    }
    [Fact] public async Task EarningsAndPayoutEvidenceRejectMutationBeforeConnecting()
    {
        using var db = Context(); var earning = new CoachEarning(); db.Attach(earning); earning.Amount = 10;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync()); db.ChangeTracker.Clear();
        var payout = new CoachPayout(); db.Attach(payout); db.Remove(payout);
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync()); db.ChangeTracker.Clear();
        var sub = new ClassesSubscription(); db.Attach(sub); sub.Price = 100;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task ForwardMigrationCreatesEvidenceWithoutRoleGrants()
    {
        await using var fixture = await AuthTestDatabase.CreateAsync("SeparatePurchaseCategoryFromOwnership");
        await using var db = fixture.CreateContext();
        var before = await db.RolePermissions.CountAsync();
        var sql = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>().GenerateScript("SeparatePurchaseCategoryFromOwnership")
            .Replace("START TRANSACTION;", "").Replace("COMMIT;", "");
        await db.Database.ExecuteSqlRawAsync(sql);
        Assert.Equal(before, await db.RolePermissions.CountAsync());
        Assert.Empty(await db.CoachEarnings.ToListAsync()); Assert.Empty(await db.CoachPayouts.ToListAsync());
        Assert.Empty(await db.ClassesSubscriptions.ToListAsync());
    }
}
