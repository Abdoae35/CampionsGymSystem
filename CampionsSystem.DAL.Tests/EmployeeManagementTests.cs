using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Employees;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CampionsSystem.DAL.Tests;

public sealed class EmployeeManagementTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private TestBranchUser current = null!;
    private EmployeeManagementService service = null!;
    private OperationAccess access = null!;
    private GymBranch branchA = null!, branchB = null!;
    private Employee employeeA = null!, employeeB = null!;
    private Guid actor;
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        var user = new ApplicationUser { UserName = "employee-admin" }; actor = user.Id;
        branchA = new GymBranch { Name = "A", City = new City { Name = "Employee city" } };
        branchB = new GymBranch { Name = "B", City = branchA.City };
        current = new TestBranchUser(new(actor, user.UserName, null, null, [],
            ["Employees.View", "Employees.Manage", BranchManagementService.AccessAllPermission], null));
        access = new OperationAccess(current, db, TimeProvider.System, Options.Create(new AuthContextOptions()));
        employeeA = new Employee { FullName = "Ahmed Alpha", Phone = "0101", HireDate = access.Today.AddDays(-30) };
        employeeB = new Employee { FullName = "Mona Beta", Phone = "0202", HireDate = access.Today.AddDays(-30), EmploymentType = EmploymentType.Freelance };
        db.AddRange(user, branchB,
            new EmployeeBranchAssignment { Employee = employeeA, Branch = branchA, StartDate = access.Today.AddDays(-20), IsPrimary = true, AssignedByUserId = actor },
            new EmployeeBranchAssignment { Employee = employeeB, Branch = branchB, StartDate = access.Today.AddDays(-20), IsPrimary = true, AssignedByUserId = actor });
        await db.SaveChangesAsync();
        service = new EmployeeManagementService(db, current, access, new EmployeeBranchResolver(db), new EmployeeBranchAssignmentService(db, current, access), new(db, current, access));
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private void Scoped() => current.Context = current.Context! with
        { Permissions = ["Employees.View", "Employees.Manage"], CurrentBranch = new(branchA.Id, branchA.Name) };

    [Fact] public async Task GlobalListAndDetailsReturnBranchWithoutIdentityData()
    {
        var result = await service.ListAsync(new()); Assert.Equal(2, result.TotalCount);
        Assert.Equal(branchA.Id, (await service.GetAsync(employeeA.Id)).CurrentBranch?.Id);
        Assert.All(result.Items, e => Assert.False(e.HasSystemAccount));
    }
    [Fact] public async Task SearchFiltersAndPaginationRunInDatabase()
    {
        Assert.Equal(employeeA.Id, Assert.Single((await service.ListAsync(new() { Search = "aHmEd" })).Items).Id);
        Assert.Equal(employeeB.Id, Assert.Single((await service.ListAsync(new() { Search = "020" })).Items).Id);
        Assert.Equal(employeeB.Id, Assert.Single((await service.ListAsync(new() { EmploymentType = EmploymentType.Freelance })).Items).Id);
        var page = await service.ListAsync(new() { Page = 2, PageSize = 1 });
        Assert.Equal(2, page.TotalCount); Assert.Equal(employeeB.Id, Assert.Single(page.Items).Id);
        Assert.Empty((await service.ListAsync(new() { Status = EmploymentStatus.Suspended })).Items);
    }
    [Fact] public async Task BranchFilterAndDetailsFailClosedForOtherBranch()
    {
        Scoped(); Assert.Equal(employeeA.Id, Assert.Single((await service.ListAsync(new())).Items).Id);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ListAsync(new() { BranchId = branchB.Id }));
        await Assert.ThrowsAsync<BranchNotFoundException>(() => service.GetAsync(employeeB.Id));
        Assert.Equal(1, (await service.ListAsync(new() { BranchId = branchA.Id })).TotalCount);
    }
    [Fact] public async Task PermissionIsRequiredEvenForGlobalUser()
    {
        current.Context = current.Context! with { Permissions = [BranchManagementService.AccessAllPermission] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ListAsync(new()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAsync(employeeA.Id));
    }
    [Fact] public async Task UnassignedAndAmbiguousEmployeesAreOnlyGloballyVisible()
    {
        var unassigned = new Employee { FullName = "No branch", Phone = "0", HireDate = access.Today };
        db.AddRange(unassigned, new EmployeeBranchAssignment { Employee = employeeA, Branch = branchB,
            StartDate = access.Today, IsPrimary = false, AssignedByUserId = actor }); await db.SaveChangesAsync();
        Assert.Equal(3, (await service.ListAsync(new())).TotalCount);
        Assert.Null((await service.GetAsync(employeeA.Id)).CurrentBranch);
        Scoped(); Assert.Empty((await service.ListAsync(new())).Items);
    }
    private CreateEmployeeRequest NewEmployee(Guid? branch = null) => new()
        { FullName = "New Employee", Phone = "0303", HireDate = access.Today, InitialBranchId = branch };

    [Fact] public async Task CreateDoesNotCreateSystemAccount()
    {
        var users = await db.Users.CountAsync(); var created = await service.CreateAsync(NewEmployee());
        Assert.False(created.HasSystemAccount); Assert.Null(created.CurrentBranch);
        Assert.Equal(users, await db.Users.CountAsync());
        Assert.Null((await db.Employees.SingleAsync(e => e.Id == created.Id)).UserId);
        Assert.Contains(await db.AuditLogs.ToListAsync(), a => a.Action == "EmployeeCreated" && a.EntityId == created.Id);
    }
    [Fact] public async Task CreateWithBranchUsesExistingAssignmentAndAudit()
    {
        Scoped(); var created = await service.CreateAsync(NewEmployee(branchA.Id));
        Assert.Equal(branchA.Id, created.CurrentBranch?.Id);
        Assert.Single(await db.EmployeeBranchAssignments.Where(a => a.EmployeeId == created.Id).ToListAsync());
        Assert.Contains(await db.AuditLogs.ToListAsync(), a => a.Action == "Employee.BranchAssigned");
    }
    [Fact] public async Task FailedInitialAssignmentRollsBackEmployeeAndAudit()
    {
        branchA.IsActive = false; await db.SaveChangesAsync(); var count = await db.Employees.CountAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(NewEmployee(branchA.Id)));
        Assert.Equal(count, await db.Employees.CountAsync()); Assert.Empty(await db.AuditLogs.ToListAsync());
    }
    [Fact] public async Task ScopedCreateCannotOmitOrSpoofBranch()
    {
        Scoped(); await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(NewEmployee()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(NewEmployee(branchB.Id)));
        current.Context = current.Context! with { Permissions = ["Employees.View"] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(NewEmployee(branchA.Id)));
    }

    private EmployeeProfileRequest Profile() => new()
        { FullName = "Updated name", Phone = "new-phone", JobTitle = "Trainer", HireDate = employeeA.HireDate };

    [Fact] public async Task UpdateChangesOnlyProfileAndAudits()
    {
        var assignment = await db.EmployeeBranchAssignments.SingleAsync(a => a.EmployeeId == employeeA.Id);
        var updated = await service.UpdateAsync(employeeA.Id, Profile());
        Assert.Equal("Updated name", updated.FullName); Assert.Equal("Trainer", updated.JobTitle);
        Assert.Equal(branchA.Id, updated.CurrentBranch?.Id); Assert.Equal("Active", updated.Status);
        Assert.Null(assignment.EndDate); Assert.Equal(branchA.Id, assignment.BranchId);
        Assert.Contains(await db.AuditLogs.ToListAsync(), a => a.Action == "EmployeeUpdated");
    }
    [Fact] public async Task SuspendReactivateAndEndEmploymentPreserveHistory()
    {
        Assert.Equal("Suspended", (await service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Suspended, Reason = "Review" })).Status);
        Assert.Equal("Active", (await service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Active })).Status);
        var ended = await service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Resigned, Reason = "Employment ended" });
        Assert.Equal(access.Today, ended.TerminationDate); Assert.Equal("Resigned", (await service.GetAsync(employeeA.Id)).Status);
        Assert.Equal(employeeA.Id, Assert.Single((await service.ListAsync(new() { Status = EmploymentStatus.Resigned })).Items).Id);
        Assert.Single(await db.EmployeeBranchAssignments.Where(a => a.EmployeeId == employeeA.Id).ToListAsync());
        Assert.Equal(3, await db.AuditLogs.CountAsync(a => a.Action == "EmployeeStatusChanged"));
        Assert.Null((await service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Active })).TerminationDate);
    }
    [Fact] public async Task UpdateAndStatusRespectScopeAndPermissions()
    {
        Scoped(); await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateAsync(employeeB.Id, Profile()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ChangeStatusAsync(employeeB.Id, new() { Status = EmploymentStatus.Suspended }));
        current.Context = current.Context! with { Permissions = ["Employees.View", BranchManagementService.AccessAllPermission] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateAsync(employeeA.Id, Profile()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Suspended }));
    }
    [Fact] public async Task InvalidProfileOrStatusLeavesEmployeeUnchanged()
    {
        var request = Profile(); request.HireDate = access.Today;
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateAsync(employeeA.Id, request));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.ChangeStatusAsync(employeeA.Id, new() { Status = (EmploymentStatus)99 }));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.ChangeStatusAsync(employeeA.Id, new()));
        Assert.Equal("Ahmed Alpha", (await service.GetAsync(employeeA.Id)).FullName);
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }

}
