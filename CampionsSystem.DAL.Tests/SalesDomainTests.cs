using System.Text.Json;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed class SalesDomainTests
{
    private static ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Database=unused").Options);
    [Fact] public void FinanceReusesExistingAccountsAndAddsExplicitAllocation()
    {
        using var db = Context();
        foreach (var type in new[] { typeof(SaleTransaction), typeof(Payment), typeof(Refund) })
            Assert.Contains(db.Model.FindEntityType(type)!.GetIndexes(), x => x.IsUnique && x.Properties.SingleOrDefault()?.Name == "OperationId");
        Assert.True(db.Model.FindEntityType(typeof(Payment))!.FindProperty("ShiftId")!.IsNullable);
        Assert.True(db.Model.FindEntityType(typeof(Payment))!.FindProperty("CommissionPeriodId")!.IsNullable);
        foreach (var type in new[] { typeof(SaleMembershipSnapshot), typeof(SaleBenefitSnapshot), typeof(InstallmentPaymentAllocation) })
            Assert.All(db.Model.FindEntityType(type)!.GetForeignKeys(), x => Assert.Equal(DeleteBehavior.Restrict, x.DeleteBehavior));
        Assert.True(db.Model.FindEntityType(typeof(SaleTransaction))!.FindProperty("Version")!.IsConcurrencyToken);
    }
    [Fact] public async Task SoldTermsAndFinancialPartsCannotBeRewritten()
    {
        using var db = Context();
        var snapshot = new SaleMembershipSnapshot { SaleTransactionId = Guid.NewGuid(), PlanName = "Original" };
        db.Attach(snapshot); snapshot.PlanName = "Edited";
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear(); var part = new PaymentPart { Amount = 100 }; db.Attach(part); db.Remove(part);
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
    [Theory][InlineData("FinalPrice", "1200")][InlineData("SalesEmployeeId", "\"00000000-0000-0000-0000-000000000001\"")]
    public void CreateContractRejectsAuthoritySpoofing(string property, string value)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CreateSaleRequest>("{\"" + property + "\":" + value + "}"));
    }
    [Fact] public void PendingActivationNeedsNoFakeSubscriptionDates()
    {
        var sale = new SaleTransaction { ActivationMode = DAL.Enums.SubscriptionActivationMode.FirstCheckIn };
        Assert.Null(sale.RequestedStartDate);
        Assert.Null(sale.MembershipSnapshot);
        Assert.False(new MemberSubscription().IsStandaloneVisit);
    }
}
