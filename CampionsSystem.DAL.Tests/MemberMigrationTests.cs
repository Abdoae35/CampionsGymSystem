using CampionsSystem.BLL.Members;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CampionsSystem.DAL.Tests;

public sealed class MemberMigrationTests
{
    [Fact] public async Task UpgradeSeedsCounterPreservesAmbiguousPhonesAndBackfillsOnlyUniqueInternationalPhones()
    {
        await using var fixture = await AuthTestDatabase.CreateAsync("AddEmployeeGymMembershipSupport");
        await using var db = fixture.CreateContext();
        var actor = new ApplicationUser { UserName = "phone-migration" }; var city = new City { Name = "Migration city" };
        db.AddRange(actor, city); await db.SaveChangesAsync();
        var branch = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "GymBranches" ("Id", "CityId", "Name", "MaxBorrowedGymSessions", "QuickRefundWindowHours", "IsActive", "CreatedAt")
            VALUES ({branch}, {city.Id}, 'Original', 2, 24, true, {DateTimeOffset.UtcNow});
            """);
        var phones = new[] { "01212345678", "+44 (12) 3456-7890", "+201212345678", "+20 (12) 1234-5678" };
        for (var i = 0; i < phones.Length; i++)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Members" ("Id", "ClientCode", "MemberCode", "FullName", "PrimaryPhone", "HomeBranchId", "IsPrimaryPhoneVerified", "IsActive", "CreatedByUserId", "CreatedAt")
                VALUES ({Guid.NewGuid()}, {100L + i}, {"MIG-" + i}, 'Legacy', {phones[i]}, {branch}, false, true, {actor.Id}, {DateTimeOffset.UtcNow});
                """);
        }
        var sql = db.GetService<IMigrator>().GenerateScript("AddEmployeeGymMembershipSupport", "AddBranchClientCodeSequenceSupport")
            .Replace("START TRANSACTION;", "").Replace("COMMIT;", "");
        await db.Database.ExecuteSqlRawAsync(sql);
        var members = await db.Members.OrderBy(m => m.ClientCode).ToListAsync();
        Assert.Equal(phones, members.Select(m => m.PrimaryPhone));
        Assert.Null(members[0].PrimaryPhoneNormalized); Assert.Equal("+441234567890", members[1].PrimaryPhoneNormalized);
        Assert.Null(members[2].PrimaryPhoneNormalized); Assert.Null(members[3].PrimaryPhoneNormalized);
        Assert.Null((await db.GymBranches.SingleAsync()).CountryCallingCode);
        Assert.Equal(104, await new BranchClientCodeGenerator(db).NextAsync(branch));
    }
}
