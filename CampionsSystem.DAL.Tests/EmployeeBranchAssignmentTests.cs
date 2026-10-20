using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CampionsSystem.DAL.Tests;

public sealed class EmployeeBranchAssignmentTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private TestBranchUser current = null!;
    private EmployeeBranchAssignmentService service = null!;
    private Employee employee = null!;
    private GymBranch branch = null!;
    private OperationAccess access = null!;
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        var user = new ApplicationUser { UserName = "assignment-admin" };
        branch = new GymBranch { Name = "A", City = new City { Name = "Assignments" } };
        employee = new Employee { FullName = "Employee", Phone = "assignment-employee", HireDate = new(2026, 1, 1) };
        db.AddRange(user, branch, employee); await db.SaveChangesAsync();
        current = new TestBranchUser(new(user.Id, user.UserName, null, null, [], ["Employees.View", "Employees.Manage", "Branches.AccessAll"], null));
        access = new OperationAccess(current, db, TimeProvider.System, Options.Create(new AuthContextOptions()));
        service = new EmployeeBranchAssignmentService(db, current, access);
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    [Fact] public async Task InitialAssignmentCanBeReadWithHistoryAndAudit()
    {
        Assert.Null(await service.GetCurrentAsync(employee.Id));
        var assigned = await service.AssignAsync(employee.Id, new(branch.Id, access.Today));
        Assert.Null(assigned.AssignedByName);
        employee.UserId=current.UserId;await db.SaveChangesAsync();
        Assert.Equal("Employee",(await service.GetCurrentAsync(employee.Id))!.AssignedByName);
        Assert.True(assigned.IsPrimary); Assert.Equal(branch.Id, (await service.GetCurrentAsync(employee.Id))!.BranchId);
        Assert.Equal("Employee",Assert.Single(await service.HistoryAsync(employee.Id)).AssignedByName);
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "Employee.BranchAssigned");
    }
    [Fact] public async Task SecondOpenAssignmentIsRejected()
    {
        await service.AssignAsync(employee.Id, new(branch.Id, access.Today));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.AssignAsync(employee.Id, new(branch.Id, access.Today.AddDays(1))));
        Assert.Single(await db.EmployeeBranchAssignments.ToListAsync());
    }
    [Fact] public async Task BoundedNonprimaryOverlapAlsoRejected()
    {
        db.Add(new EmployeeBranchAssignment { Employee = employee, Branch = branch, AssignedByUserId = current.UserId!.Value,
            StartDate = access.Today, EndDate = access.Today.AddDays(3), IsPrimary = false }); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.AssignAsync(employee.Id, new(branch.Id, access.Today.AddDays(2))));
    }
    [Fact] public async Task FutureAssignmentDoesNotBecomeCurrentEarly()
    {
        await service.AssignAsync(employee.Id, new(branch.Id, access.Today.AddDays(2)));
        Assert.Null(await service.GetCurrentAsync(employee.Id)); Assert.Single(await service.HistoryAsync(employee.Id));
    }
    [Fact] public async Task InactiveBranchRejectedWithoutPartialAssignmentOrAudit()
    {
        branch.IsActive = false; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.AssignAsync(employee.Id, new(branch.Id, access.Today)));
        Assert.Empty(await db.EmployeeBranchAssignments.ToListAsync()); Assert.Empty(await db.AuditLogs.ToListAsync());
    }
    [Fact] public async Task BackdatedInitialAssignmentRejected() =>
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.AssignAsync(employee.Id, new(branch.Id, access.Today.AddDays(-1))));
    [Fact] public async Task ScopeAndPermissionEnforcedForAssignment()
    {
        current.Context = current.Context! with { Permissions = ["Employees.Manage"], CurrentBranch = null };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AssignAsync(employee.Id, new(branch.Id, access.Today)));
    }
}
