using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CampionsSystem.DAL.Tests;

public sealed class FinalAlignmentMigrationTests
{
    [Fact]
    public async Task UpgradePreservesLegacySaleShiftAndPlanWithoutInventingBusinessDaysOrCategories()
    {
        await using var fixture = await AuthTestDatabase.CreateAsync("UpdateActivitiesClassesAndFreelanceRules");
        await using var db = fixture.CreateContext();
        var user = new ApplicationUser { UserName = "final-upgrade" };
        var branch = new GymBranch { Name = "Legacy", City = new City { Name = "Legacy" } };
        var member = new Member { MemberCode = "FINAL-LEGACY", FullName = "Legacy", PrimaryPhone = "final-legacy", HomeBranch = branch, CreatedByUser = user };
        db.AddRange(user, branch.City); await db.SaveChangesAsync();
        branch.CityId = branch.City.Id;
        // Insert only columns present at this historical migration boundary.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "GymBranches" ("Id", "CityId", "Name", "MaxBorrowedGymSessions", "QuickRefundWindowHours", "IsActive", "CreatedAt")
            VALUES ({branch.Id}, {branch.CityId}, {branch.Name}, 2, 24, TRUE, {branch.CreatedAt});
            """);
        db.Attach(branch);
        // Insert with the legacy schema shape, not today's Member EF model.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Members" ("Id", "MemberCode", "FullName", "PrimaryPhone", "HomeBranchId", "CreatedByUserId", "CreatedAt", "IsActive", "IsPrimaryPhoneVerified")
            VALUES ({member.Id}, {member.MemberCode}, {member.FullName}, {member.PrimaryPhone}, {branch.Id}, {user.Id}, {member.CreatedAt}, TRUE, FALSE);
            """);
        var saleId = Guid.NewGuid(); var planId = Guid.NewGuid(); var shiftId = Guid.NewGuid();
        var actual = new DateTimeOffset(2026, 10, 31, 1, 30, 0, TimeSpan.Zero);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "SaleTransactions" ("Id", "MemberId", "BranchId", "SaleType", "GrossAmount", "DiscountAmount", "NetAmount", "Status", "CreatedByUserId", "CreatedAt")
            VALUES ({saleId}, {member.Id}, {branch.Id}, 'GymMembership', 500, 0, 500, 'Open', {user.Id}, {actual});
            INSERT INTO "MembershipPlans" ("Id", "CityId", "Name", "AccessMode", "DurationDays", "PlanCategory", "Price", "AllowFreeze", "IncludedFreezeDays", "IsActive", "CreatedAt")
            VALUES ({planId}, {branch.CityId}, 'Legacy monthly', 'Unlimited', 30, 'Normal', 500, FALSE, 0, TRUE, {actual});
            INSERT INTO "Shifts" ("Id", "BranchId", "OpenedAt", "OpeningCash", "Status", "OpenedByUserId")
            VALUES ({shiftId}, {branch.Id}, {actual}, 0, 'Open', {user.Id});
            """);
        var sql = db.GetService<IMigrator>().GenerateScript("UpdateActivitiesClassesAndFreelanceRules", "FinalBusinessSchemaAlignment")
            .Replace("START TRANSACTION;", "").Replace("COMMIT;", "");
        await db.Database.ExecuteSqlRawAsync(sql); db.ChangeTracker.Clear();
        var sale = await db.SaleTransactions.SingleAsync(); Assert.Equal(actual, sale.CreatedAt); Assert.Equal(500, sale.NetAmount); Assert.Null(sale.BusinessDayId);
        Assert.Null((await db.Shifts.SingleAsync()).BusinessDayId); Assert.Null((await db.MembershipPlans.SingleAsync()).MembershipPlanCategoryId);
        Assert.Empty(await db.BusinessDays.ToListAsync()); Assert.Empty(await db.MembershipPlanCategories.ToListAsync());
        Assert.Empty(await db.UserPermissionOverrides.ToListAsync()); Assert.Empty(await db.EmployeeLeaveRequests.ToListAsync());
        Assert.Empty(await db.ShiftEmployeeChanges.ToListAsync());
    }
}
