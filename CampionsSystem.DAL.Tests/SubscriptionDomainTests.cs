using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed class SubscriptionDomainTests
{
    private static ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Database=unused").Options);
    [Fact] public void ExistingGrantSupportsNonFinancialEmploymentSnapshot()
    {
        using var db = Context(); var grant = db.Model.FindEntityType(typeof(MembershipSubscriptionTransaction))!;
        Assert.True(grant.FindProperty(nameof(MembershipSubscriptionTransaction.SaleTransactionId))!.IsNullable);
        Assert.True(grant.FindProperty(nameof(MembershipSubscriptionTransaction.NewEndDate))!.IsNullable);
        Assert.Contains(grant.GetIndexes(), i => i.IsUnique && i.Properties.SingleOrDefault()?.Name == "OperationId");
        Assert.All(grant.GetForeignKeys(), f => Assert.Equal(DeleteBehavior.Restrict, f.DeleteBehavior));
    }
    [Fact] public void QueueAndProtectedDaysHaveUniqueDatabaseInvariants()
    {
        using var db = Context(); var sub = db.Model.FindEntityType(typeof(MemberSubscription))!;
        Assert.True(sub.FindProperty("Version")!.IsConcurrencyToken);
        Assert.Contains(sub.GetIndexes(), i => i.IsUnique && i.Properties.SingleOrDefault()?.Name == "FollowsSubscriptionId");
        var holiday = db.Model.FindEntityType(typeof(SubscriptionHolidayExtension))!;
        Assert.Contains(holiday.GetIndexes(), i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[]{"MemberSubscriptionId", "ProtectedDate"}));
    }
    [Theory] [InlineData(true)] [InlineData(false)]
    public async Task SnapshotFactsCannotBeEditedOrDeleted(bool delete)
    {
        using var db = Context(); var grant = new MembershipSubscriptionTransaction(); db.Attach(grant);
        if (delete) db.Remove(grant); else grant.PlanNameSnapshot = "Rewrite";
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
}
