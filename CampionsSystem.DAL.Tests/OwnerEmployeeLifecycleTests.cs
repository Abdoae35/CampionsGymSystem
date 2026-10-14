using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Employees;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.DAL.Tests;

public sealed partial class EmployeeManagementTests
{
    private async Task<Guid> LinkOwner(Employee employee)
    {
        var role = await db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == "OWNER");
        if (role is null)
        {
            role = new IdentityRole<Guid>("Owner") { Id = Guid.NewGuid(), NormalizedName = "OWNER" };
            db.Add(role);
            foreach (var code in OwnerAdministrationAccess.RecoveryPermissions)
                db.Add(new RolePermission { Role = role, Permission = new Permission { Code = code, Name = code, Module = code.Split('.')[0] } });
        }
        var account = new ApplicationUser { UserName = "owner-" + employee.Id, LockoutEnabled = true };
        db.Add(account);
        employee.User = account;
        db.Add(new IdentityUserRole<Guid> { UserId = account.Id, RoleId = role.Id });
        await db.SaveChangesAsync(); return account.Id;
    }

    private async Task MakeActorOwner()
    {
        var role = await db.Roles.SingleAsync(x => x.NormalizedName == "OWNER");
        db.Add(new IdentityUserRole<Guid> { UserId = actor, RoleId = role.Id }); await db.SaveChangesAsync();
    }

    [Theory]
    [InlineData(EmploymentStatus.Suspended)]
    [InlineData(EmploymentStatus.Resigned)]
    public async Task OwnerLifecycleNonOwnerCannotDisableOwnerEvenWithBroadCapabilities(EmploymentStatus status)
    {
        await LinkOwner(employeeA);
        // Client/context role claims cannot substitute for authoritative membership.
        current.Context = current.Context! with { Roles = ["Owner"], Permissions = ["Employees.Manage", "Branches.AccessAll", ..OwnerAdministrationAccess.RecoveryPermissions] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ChangeStatusAsync(employeeA.Id, new() { Status = status }));
        Assert.Equal(EmploymentStatus.Active, (await db.Employees.AsNoTracking().SingleAsync(x => x.Id == employeeA.Id)).EmploymentStatus);
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }

    [Theory]
    [InlineData(EmploymentStatus.Suspended)]
    [InlineData(EmploymentStatus.Resigned)]
    public async Task OwnerLifecycleSelfLockoutRejectedEvenWithAnotherUsableOwner(EmploymentStatus status)
    {
        var self = await LinkOwner(employeeA); await LinkOwner(employeeB);
        current.Context = current.Context! with { UserId = self };
        var error = await Assert.ThrowsAsync<AccessAdministrationException>(() => service.ChangeStatusAsync(employeeA.Id, new() { Status = status }));
        Assert.Equal(409, error.Status); Assert.Contains("own account", error.Message);
        Assert.Equal(EmploymentStatus.Active, (await db.Employees.AsNoTracking().SingleAsync(x => x.Id == employeeA.Id)).EmploymentStatus);
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }

    [Fact]
    public async Task OwnerLifecycleLastUsableOwnerIncludesEffectiveRecoveryDeniesAndRollsBack()
    {
        await LinkOwner(employeeA); await MakeActorOwner();
        var permission = await db.Permissions.SingleAsync(x => x.Code == "Roles.Manage");
        db.Add(new UserPermissionOverride { UserId = actor, PermissionId = permission.Id, Effect = PermissionOverrideEffect.Deny }); await db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<AccessAdministrationException>(() => service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Suspended }));
        Assert.Contains("At least one", error.Message);
        Assert.Equal(EmploymentStatus.Active, (await db.Employees.AsNoTracking().SingleAsync(x => x.Id == employeeA.Id)).EmploymentStatus);
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }

    [Fact]
    public async Task OwnerLifecycleAuthorizedOwnerCanSuspendAndReactivateAnotherOwner()
    {
        var target = await LinkOwner(employeeA); await MakeActorOwner();
        await service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Suspended, Reason = "Owner authorizes investigation" });
        var contexts = new UserContextService(db, new PermissionService(db), TimeProvider.System,
            Microsoft.Extensions.Options.Options.Create(new AuthContextOptions()), new(db));
        Assert.Null(await contexts.ResolveAsync(target)); Assert.NotNull(await contexts.ResolveAsync(actor));
        Assert.True(await new OwnerAdministrationAccess(db, current).IsOwnerAsync(target));
        await service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Active });
        Assert.NotNull(await contexts.ResolveAsync(target));
        Assert.Equal(2, await db.AuditLogs.CountAsync(x => x.Action == "EmployeeStatusChanged" && x.UserId == actor));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OwnerLifecycleHireDateCannotBeUsedAsAlternativeSelfOrNonOwnerLockout(bool self)
    {
        var owner = await LinkOwner(employeeA);
        db.EmployeeBranchAssignments.RemoveRange(await db.EmployeeBranchAssignments.Where(x => x.EmployeeId == employeeA.Id).ToListAsync()); await db.SaveChangesAsync();
        if (self) current.Context = current.Context! with { UserId = owner };
        var request = Profile(); request.HireDate = access.Today.AddDays(1);
        if (self) await Assert.ThrowsAsync<AccessAdministrationException>(() => service.UpdateAsync(employeeA.Id, request));
        else await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateAsync(employeeA.Id, request));
        Assert.Equal(employeeA.HireDate, (await db.Employees.AsNoTracking().SingleAsync(x => x.Id == employeeA.Id)).HireDate);
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }

    [Fact]
    public async Task OwnerLifecyclePreservesBranchScopeAndNormalEmployeeStatusChanges()
    {
        await LinkOwner(employeeB); await MakeActorOwner(); Scoped();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ChangeStatusAsync(employeeB.Id, new() { Status = EmploymentStatus.Suspended }));
        Assert.Equal("Suspended", (await service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Suspended })).Status);
        Assert.Equal("Active", (await service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Active })).Status);
        Assert.Equal("Resigned", (await service.ChangeStatusAsync(employeeA.Id, new() { Status = EmploymentStatus.Resigned })).Status);
        Assert.Equal(EmploymentStatus.Active, (await db.Employees.AsNoTracking().SingleAsync(x => x.Id == employeeB.Id)).EmploymentStatus);
    }
}
