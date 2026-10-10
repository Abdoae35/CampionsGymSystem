using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace CampionsSystem.DAL.Tests;
public sealed class SalesMigrationTests
{
    [Fact] public async Task CancellationUpgradePreservesLegacySettlementWithoutInventingEvents()
    {
        await using var f=await AuthTestDatabase.CreateAsync("AddOfferEligibilityAndAudience");await using var db=f.CreateContext();
        var user=new ApplicationUser{UserName="settlement-upgrade"};var branch=new GymBranch{Name="Branch",City=new(){Name="City"}};
        var member=new Member{MemberCode="M-DEBT-UP",FullName="Member",PrimaryPhone="debt-upgrade",HomeBranch=branch,CreatedByUser=user};
        var plan=new MembershipPlan{Name="Plan",Branch=branch,City=branch.City,DurationDays=30,Price=100};
        var sub=new MemberSubscription{Member=member,MembershipPlan=plan,PurchasedAtBranch=branch,City=branch.City,StartDate=new(2026,9,1),EndDate=new(2026,9,30)};
        var grant=new MembershipSubscriptionTransaction{MemberSubscription=sub,MembershipPlan=plan,PurchasedDurationDays=30,NewEndDate=sub.EndDate};
        var debt=new BorrowedGymSession{Member=member,ExpiredSubscription=sub,Branch=branch,BorrowedAt=new(2026,10,1,10,0,0,TimeSpan.Zero),Status=Enums.BorrowedSessionStatus.Settled,
            SettledBySubscription=sub,SettledByTransaction=grant,SettledAt=new(2026,10,2,10,0,0,TimeSpan.Zero)};
        db.Add(debt);await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync(db.GetService<IMigrator>().GenerateScript("AddOfferEligibilityAndAudience","AddSaleGrantCancellationHistory").Replace("START TRANSACTION;","").Replace("COMMIT;",""));
        db.ChangeTracker.Clear();var preserved=await db.BorrowedGymSessions.SingleAsync();Assert.Equal(grant.Id,preserved.SettledByTransactionId);Assert.Equal(debt.SettledAt,preserved.SettledAt);
        Assert.Empty(await db.BorrowedDebtSettlementEvents.ToListAsync());Assert.Empty(await db.SubscriptionGrantCancellations.ToListAsync());
    }
    [Fact] public async Task FinanceUpgradePreservesLegacyMoneyAndUnknownWorkflowFacts()
    {
        await using var f = await AuthTestDatabase.CreateAsync("PrepareOperationalCheckIns");
        await using var db = f.CreateContext();
        var user = new ApplicationUser { UserName = "finance-upgrade" };
        var branch = new GymBranch { Name = "Legacy", City = new City { Name = "Legacy" } };
        var member = new Member { MemberCode = "M-FIN-LEGACY", FullName = "Legacy", PrimaryPhone = "finance-upgrade", HomeBranch = branch, CreatedByUser = user };
        db.Add(member); await db.SaveChangesAsync();
        var id = Guid.NewGuid(); var time = new DateTimeOffset(2026, 10, 9, 22, 30, 0, TimeSpan.Zero);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "SaleTransactions" ("Id", "MemberId", "BranchId", "SaleType", "GrossAmount", "DiscountAmount", "NetAmount", "Status", "CreatedByUserId", "CreatedAt")
            VALUES ({id}, {member.Id}, {branch.Id}, 'GymMembership', 1200, 200, 1000, 'Open', {user.Id}, {time});
            """);
        var script = db.GetService<IMigrator>().GenerateScript("PrepareOperationalCheckIns", "PrepareSalesPayments")
            .Replace("START TRANSACTION;", "").Replace("COMMIT;", "");
        await db.Database.ExecuteSqlRawAsync(script); db.ChangeTracker.Clear();
        var sale = await db.SaleTransactions.Select(s => new { s.Id, s.NetAmount, s.DiscountAmount, s.CreatedAt, s.OperationId, s.BusinessDate, s.CommissionEligible }).SingleAsync();
        Assert.Equal(id, sale.Id); Assert.Equal(1000, sale.NetAmount); Assert.Equal(200, sale.DiscountAmount); Assert.Equal(time, sale.CreatedAt);
        Assert.Null(sale.OperationId); Assert.Null(sale.BusinessDate); Assert.Null(sale.CommissionEligible);
        Assert.False(await db.SaleMembershipSnapshots.AnyAsync()); Assert.Empty(await db.Payments.ToListAsync());
        Assert.Empty(await db.Shifts.ToListAsync()); Assert.Empty(await db.CommissionPeriods.ToListAsync());
    }
}
