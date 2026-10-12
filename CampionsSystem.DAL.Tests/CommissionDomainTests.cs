using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed class CommissionDomainTests
{
    private static ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Database=unused").Options);
    [Fact] public void EvidenceUsesRestrictiveSourceReferencesAndExactPrecision()
    {
        using var db = Context();
        foreach (var type in new[] { typeof(CommissionCollection), typeof(CommissionRefundAllocation), typeof(CommissionStatementLine) })
            Assert.All(db.Model.FindEntityType(type)!.GetForeignKeys(), k => Assert.Equal(DeleteBehavior.Restrict, k.DeleteBehavior));
        var amount = db.Model.FindEntityType(typeof(CommissionRefundAllocation))!.FindProperty("Amount")!;
        Assert.Equal(18, amount.GetPrecision()); Assert.Equal(2, amount.GetScale());
        Assert.True(db.Model.FindEntityType(typeof(CommissionPeriod))!.FindProperty("StartBusinessDate")!.IsNullable);
        Assert.True(db.Model.FindEntityType(typeof(Payment))!.FindProperty("CommissionPeriodId")!.IsNullable);
    }
    [Fact] public async Task SourceEvidenceAndFinalizedSummariesCannotBeRewritten()
    {
        using var db = Context();
        var evidence = new CommissionCollection { PaymentId = Guid.NewGuid(), EligibilityReason = "Eligible" };
        db.Attach(evidence); evidence.EligibilityReason = "House";
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync()); db.ChangeTracker.Clear();
        var summary = new EmployeeCommissionSummary(); db.Attach(summary); summary.CommissionAmount = 100;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task ManagedRulesAndClosedPeriodsCannotBeEditedOrReopened()
    {
        using var db = Context(); var rule = new CommissionRule { OperationId = Guid.NewGuid() };
        db.Attach(rule); rule.TargetAmount = 42;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync()); db.ChangeTracker.Clear();
        var period = new CommissionPeriod { OperationId = Guid.NewGuid(), Status = CommissionPeriodStatus.Closed };
        db.Attach(period); period.Status = CommissionPeriodStatus.Open;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
}
