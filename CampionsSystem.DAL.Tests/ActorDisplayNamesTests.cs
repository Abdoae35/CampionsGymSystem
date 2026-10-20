using CampionsSystem.BLL.Authentication;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Tests;

public sealed class ActorDisplayNamesTests
{
    [Fact]
    public async Task ResolvesCurrentEmployeeNamesByUserIdAndLeavesOtherActorsUnnamed()
    {
        await using var database = await AuthTestDatabase.CreateAsync();
        await using var db = database.CreateContext();

        var linked = new ApplicationUser { UserName = "linked@example.invalid", Email = "linked@example.invalid" };
        var inactive = new ApplicationUser { UserName = "inactive@example.invalid" };
        var owner = new ApplicationUser { UserName = "owner@example.invalid" };
        var linkedEmployee = new Employee
        {
            User = linked, FullName = "Recorded Operator", Phone = "100", HireDate = new(2026, 1, 1),
            EmploymentStatus = EmploymentStatus.Active
        };
        var inactiveEmployee = new Employee
        {
            User = inactive, FullName = "Former Operator", Phone = "101", HireDate = new(2026, 1, 1),
            EmploymentStatus = EmploymentStatus.Suspended
        };
        db.AddRange(linkedEmployee, inactiveEmployee, owner);
        await db.SaveChangesAsync();

        var missingUserId = Guid.NewGuid();
        var names = await ActorDisplayNames.LoadAsync(db,
            new Guid?[] { linked.Id, inactive.Id, owner.Id, missingUserId, linkedEmployee.Id, null, linked.Id });

        Assert.Equal("Recorded Operator", ActorDisplayNames.Get(names, linked.Id));
        Assert.Equal("Former Operator", ActorDisplayNames.Get(names, inactive.Id));
        Assert.Null(ActorDisplayNames.Get(names, owner.Id));
        Assert.Null(ActorDisplayNames.Get(names, missingUserId));
        Assert.Null(ActorDisplayNames.Get(names, linkedEmployee.Id));
        Assert.Null(ActorDisplayNames.Get(names, null));
        Assert.Equal(2, names.Count);

        linkedEmployee.FullName = "Current Operator";
        await db.SaveChangesAsync();
        var refreshed = await ActorDisplayNames.LoadAsync(db, new[] { linked.Id });
        Assert.Equal("Current Operator", ActorDisplayNames.Get(refreshed, linked.Id));
    }

    [Fact]
    public async Task EmptyActorSetNeedsNoDatabaseLookup()
    {
        await using var database = await AuthTestDatabase.CreateAsync();
        await using var db = database.CreateContext();
        Assert.Empty(await ActorDisplayNames.LoadAsync(db, Array.Empty<Guid?>()));
    }
}
