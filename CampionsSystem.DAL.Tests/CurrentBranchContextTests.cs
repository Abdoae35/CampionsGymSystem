using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.DAL.Tests;

public sealed class CurrentBranchContextTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private Employee employee = null!;
    private ApplicationUser user = null!;
    private GymBranch branch = null!;
    private readonly DateOnly today = new(2026, 10, 7);
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        user = new ApplicationUser { UserName = "context-user" };
        employee = new Employee { User = user, FullName = "Employee", Phone = "context", HireDate = today.AddDays(-30) };
        branch = new GymBranch { Name = "Current", City = new City { Name = "Context" } };
        db.AddRange(employee, branch); await db.SaveChangesAsync();
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private EmployeeBranchAssignment Assignment(DateOnly start, DateOnly? end = null, bool primary = true) =>
        new() { Employee = employee, Branch = branch, AssignedByUser = user, StartDate = start, EndDate = end, IsPrimary = primary };
    [Fact] public async Task ResolverUsesInclusiveDatesAndExcludesHistoricalAndFutureAssignments()
    {
        db.AddRange(Assignment(today.AddDays(-10), today.AddDays(-1)), Assignment(today, today), Assignment(today.AddDays(1)));
        await db.SaveChangesAsync(); var resolved = await new EmployeeBranchResolver(db).ResolveAsync(employee.Id, today);
        Assert.Equal(branch.Id, resolved?.Id);
    }
    [Fact] public async Task OverlappingLegacyAssignmentsFailClosedEvenWithPrimary()
    {
        db.AddRange(Assignment(today), Assignment(today, today.AddDays(5), false)); await db.SaveChangesAsync();
        Assert.Null(await new EmployeeBranchResolver(db).ResolveAsync(employee.Id, today));
    }
    [Fact] public async Task InactiveBranchDoesNotProvideOperationalScope()
    {
        branch.IsActive = false; db.Add(Assignment(today)); await db.SaveChangesAsync();
        Assert.Null(await new EmployeeBranchResolver(db).ResolveAsync(employee.Id, today));
    }
    [Fact] public async Task GlobalAccountWithoutEmployeeOrBranchIsSafeAndPermissionBased()
    {
        var current = new TestBranchUser(new(user.Id, user.UserName, null, null, [], ["Branches.AccessAll"], null));
        var context = await new CurrentBranchContext(current).GetAsync();
        Assert.True(context.HasGlobalAccess); Assert.False(context.HasBranch); Assert.Null(context.CurrentBranchId);
        current.Context = current.Context! with { Roles = ["Owner"], Permissions = [] };
        Assert.False((await new CurrentBranchContext(current).GetAsync()).HasGlobalAccess);
    }
    [Fact] public async Task ContextReusesCurrentUserBranchAndDoesNotCacheOldAssignment()
    {
        var current = new TestBranchUser(new(user.Id, user.UserName, employee.Id, employee.FullName, [], [], new(branch.Id, branch.Name)));
        var service = new CurrentBranchContext(current); Assert.Equal(branch.Id, (await service.GetAsync()).CurrentBranchId);
        current.Context = current.Context! with { CurrentBranch = new(Guid.NewGuid(), "New") };
        Assert.Equal(current.Context.CurrentBranchId, (await service.GetAsync()).CurrentBranchId);
        current.Context = null; Assert.False((await service.GetAsync()).HasBranch);
    }
}
