using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CampionsSystem.DAL.Tests;

public sealed class ActivityRuleMigrationTests
{
    [Fact]
    public async Task ExistingEmployeeAssignmentSubscriptionAndClassAttendanceSurviveUpgrade()
    {
        await using var fixture = await AuthTestDatabase.CreateAsync("UpdatePrivateAndFreelanceBusinessRules");
        await using var db = fixture.CreateContext();
        var user = new ApplicationUser { UserName = "legacy-test" };
        var branch = new GymBranch { Name = "Legacy Branch", City = new City { Name = "Legacy City" } };
        var employee = new Employee { FullName = "Legacy coach", Phone = "legacy", HireDate = new(2026, 1, 1) };
        var member = new Member { MemberCode = "L-1", FullName = "Legacy member", PrimaryPhone = "legacy-member", HomeBranch = branch, CreatedByUser = user };
        var activity = new SportsActivity { Branch = branch, Name = "Legacy Boxing" };
        var package = new ActivityPackage { SportsActivity = activity, Name = "Legacy", DurationDays = 30, Price = 500 };
        var group = new GroupClass { Branch = branch, Name = "Legacy class", CoachEmployeeId = employee.Id };
        db.AddRange(user, branch.City); await db.SaveChangesAsync();
        branch.CityId = branch.City.Id;
        // Insert only columns present at this historical migration boundary.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "GymBranches" ("Id", "CityId", "Name", "MaxBorrowedGymSessions", "QuickRefundWindowHours", "IsActive", "CreatedAt")
            VALUES ({branch.Id}, {branch.CityId}, {branch.Name}, 2, 24, TRUE, {branch.CreatedAt});
            """);
        db.Attach(branch);
        db.Add(package); await db.SaveChangesAsync();
        // Insert the historical Employee shape before MemberId existed.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Employees" ("Id", "FullName", "Phone", "EmploymentType", "HireDate", "EmploymentStatus", "CreatedAt")
            VALUES ({employee.Id}, {employee.FullName}, {employee.Phone}, 'Staff', {employee.HireDate}, 'Active', {employee.CreatedAt});
            """);
        db.Add(group); await db.SaveChangesAsync();
        // Insert with the legacy schema shape, not today's Member EF model.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Members" ("Id", "MemberCode", "FullName", "PrimaryPhone", "HomeBranchId", "CreatedByUserId", "CreatedAt", "IsActive", "IsPrimaryPhoneVerified")
            VALUES ({member.Id}, {member.MemberCode}, {member.FullName}, {member.PrimaryPhone}, {branch.Id}, {user.Id}, {member.CreatedAt}, TRUE, FALSE);
            """);
        var assignment = Guid.NewGuid(); var subscription = Guid.NewGuid(); var checkIn = Guid.NewGuid();
        var today = new DateOnly(2026, 10, 1); var now = new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ActivityCoachAssignments" ("Id", "SportsActivityId", "CoachEmployeeId", "StartDate", "IsActive", "CreatedAt")
            VALUES ({assignment}, {activity.Id}, {employee.Id}, {today}, TRUE, {now});
            INSERT INTO "ActivitySubscriptions" ("Id", "MemberId", "ActivityPackageId", "BranchId", "ActivityCoachAssignmentId", "StartDate", "EndDate", "AccessMode", "Status", "CreatedAt")
            VALUES ({subscription}, {member.Id}, {package.Id}, {branch.Id}, {assignment}, {today}, {today.AddDays(30)}, 'Unlimited', 'Active', {now});
            INSERT INTO "CheckIns" ("Id", "MemberId", "BranchId", "CheckInType", "GroupClassId", "CoachEmployeeId", "CheckInMethod", "SessionConsumed", "CheckedInAt", "CreatedByUserId")
            VALUES ({checkIn}, {member.Id}, {branch.Id}, 'Class', {group.Id}, {employee.Id}, 'Manual', FALSE, {now}, {user.Id});
            """);
        var sql = db.GetService<IMigrator>().GenerateScript("UpdatePrivateAndFreelanceBusinessRules", "AddMembershipBenefits")
            .Replace("START TRANSACTION;", "").Replace("COMMIT;", "");
        await db.Database.ExecuteSqlRawAsync(sql);
        db.ChangeTracker.Clear();
        var oldAssignment = await db.ActivityCoachAssignments.SingleAsync();
        Assert.Equal(employee.Id, oldAssignment.CoachEmployeeId); Assert.Null(oldAssignment.ActivityCoachId);
        Assert.Null((await db.ActivitySubscriptions.SingleAsync()).ActivityCoachId);
        // Query the historical shape only; newer CheckIn columns do not exist at this boundary.
        var oldEntry = await db.CheckIns.Select(x => new { x.Id, x.MemberId, x.BranchId, x.CheckInType, x.GroupClassId, x.ClassSessionId }).SingleAsync();
        Assert.Equal(checkIn, oldEntry.Id); Assert.Equal(member.Id, oldEntry.MemberId); Assert.Equal(branch.Id, oldEntry.BranchId);
        Assert.Equal(CheckInType.Class, oldEntry.CheckInType); Assert.Equal(group.Id, oldEntry.GroupClassId); Assert.Null(oldEntry.ClassSessionId);
        Assert.Empty(await db.ActivityCoaches.ToListAsync()); Assert.Empty(await db.ClassSessions.Select(x => x.Id).ToListAsync());
        Assert.Empty(await db.MembershipPlanEligibleMembers.ToListAsync());
    }

    [Theory]
    [InlineData(ActivityCoachType.Internal, null)]
    [InlineData(ActivityCoachType.External, null)]
    [InlineData(ActivityCoachType.External, " ")]
    public async Task DatabaseRejectsInvalidCoachShape(ActivityCoachType type, string? name)
    {
        await using var fixture = await AuthTestDatabase.CreateAsync(); await using var db = fixture.CreateContext();
        db.Add(new ActivityCoach { CoachType = type, ExternalName = name, Percentage = 30 });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task NewRelationshipsNeverCascadeAndMoneyUsesExactPrecision()
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only").Options);
        foreach (var type in new[] { typeof(ActivityCoach), typeof(ClassSession), typeof(ActivityCoachSettlement), typeof(MembershipPlanEligibleMember) })
            Assert.All(db.Model.FindEntityType(type)!.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        var coach = db.Model.FindEntityType(typeof(ActivityCoach))!;
        Assert.Equal(5, coach.FindProperty("Percentage")!.GetPrecision()); Assert.Equal(2, coach.FindProperty("Percentage")!.GetScale());
        var settlement = db.Model.FindEntityType(typeof(ActivityCoachSettlement))!;
        Assert.Equal(18, settlement.FindProperty("Entitlement")!.GetPrecision());
        Assert.DoesNotContain(db.Model.GetEntityTypes().SelectMany(x => x.GetProperties()), x => x.IsShadowProperty());
    }
}
