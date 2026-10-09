using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace CampionsSystem.DAL.Tests;
public sealed class CheckInMigrationTests
{
    [Fact] public async Task OperationalUpgradePreservesLegacyTrialWithoutInventingDateOperationOrBalance()
    {
        await using var fixture=await AuthTestDatabase.CreateAsync("PreventDuplicateFreezeHolidayCompensation");await using var db=fixture.CreateContext();
        var actor=new ApplicationUser{UserName="legacy-checkin"};var branch=new GymBranch{Name="Legacy",City=new(){Name="Legacy"}};db.AddRange(actor,branch);await db.SaveChangesAsync();
        var id=Guid.NewGuid();var actual=new DateTimeOffset(2026,10,9,22,30,0,TimeSpan.Zero);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "CheckIns" ("Id","BranchId","CheckInType","CheckInMethod","SessionConsumed","CheckedInAt","CreatedByUserId")
            VALUES ({id},{branch.Id},'Trial','Manual',FALSE,{actual},{actor.Id});
            """);
        var sql=db.GetService<IMigrator>().GenerateScript("PreventDuplicateFreezeHolidayCompensation","PrepareOperationalCheckIns")
            .Replace("START TRANSACTION;","").Replace("COMMIT;","");await db.Database.ExecuteSqlRawAsync(sql);db.ChangeTracker.Clear();
        var row=await db.CheckIns.SingleAsync();Assert.Equal(id,row.Id);Assert.Equal(actual,row.CheckedInAt);Assert.Equal(branch.Id,row.BranchId);Assert.Equal(CheckInType.Trial,row.CheckInType);
        Assert.Null(row.MemberId);Assert.Null(row.OperationId);Assert.Null(row.BusinessDate);Assert.Null(row.BusinessDayId);Assert.Null(row.SessionsBefore);Assert.Null(row.SessionsAfter);
    }
}
