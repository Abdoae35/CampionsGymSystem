using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace CampionsSystem.DAL.Tests;
public sealed class CommissionMigrationTests
{
    [Fact] public async Task CommissionUpgradePreservesLegacyRulesMoneyAndNullOperationalLinks()
    {
        await using var f=await AuthTestDatabase.CreateAsync("OrderSharedDrawerObservations");await using var db=f.CreateContext();
        var user=new ApplicationUser{UserName="commission-upgrade"};var branch=new GymBranch{Name="Legacy",City=new(){Name="Legacy"}};
        var member=new Member{MemberCode="M-COM-UP",FullName="Legacy",PrimaryPhone="commission-upgrade",HomeBranch=branch,CreatedByUser=user};db.Add(member);await db.SaveChangesAsync();
        var sale=Guid.NewGuid();var payment=Guid.NewGuid();var rule=Guid.NewGuid();var time=new DateTimeOffset(2026,10,1,10,0,0,TimeSpan.Zero);var from=new DateOnly(2026,10,1);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "SaleTransactions" ("Id","MemberId","BranchId","SaleType","GrossAmount","DiscountAmount","NetAmount","Status","CreatedByUserId","CreatedAt")
            VALUES ({sale},{member.Id},{branch.Id},'GymMembership',1000,0,1000,'Completed',{user.Id},{time});
            INSERT INTO "Payments" ("Id","SaleTransactionId","BranchId","Amount","Status","PaidAt","ReceivedByUserId")
            VALUES ({payment},{sale},{branch.Id},1000,'Completed',{time},{user.Id});
            INSERT INTO "CommissionRules" ("Id","BranchId","CommissionType","CalculationMethod","TargetAmount","BelowTargetRate","ReachedTargetRate","EffectiveFrom","IsActive","CreatedByUserId","CreatedAt")
            VALUES ({rule},{branch.Id},'SalesNew','TargetPercentage',40000,4,6,{from},true,{user.Id},{time});
            """);
        var script=db.GetService<IMigrator>().GenerateScript("OrderSharedDrawerObservations").Replace("START TRANSACTION;","").Replace("COMMIT;","");
        await db.Database.ExecuteSqlRawAsync(script);db.ChangeTracker.Clear();
        var p=await db.Payments.SingleAsync();Assert.Equal(1000,p.Amount);Assert.Null(p.ShiftId);Assert.Null(p.CommissionPeriodId);Assert.Null(p.BusinessDate);
        var r=await db.CommissionRules.SingleAsync();Assert.Equal(40000,r.TargetAmount);Assert.Equal(4,r.BelowTargetRate);Assert.Null(r.OperationId);
        Assert.Empty(await db.CommissionCollections.ToListAsync());Assert.Empty(await db.CommissionPeriods.ToListAsync());Assert.Empty(await db.CommissionRefundAllocations.ToListAsync());
    }
}
