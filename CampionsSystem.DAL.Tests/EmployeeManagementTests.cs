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
        service = new EmployeeManagementService(db, current, access, new EmployeeBranchResolver(db));
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
}
