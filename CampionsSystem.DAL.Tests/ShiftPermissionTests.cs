using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class EmployeeShiftTests
{
    [Fact] public async Task CapabilityCatalogPreservesDisabledFlagsAndOverridesAndInfersNoDefaultGrants()
    {
        var disabled=new Permission{Code=ShiftPermissions.Withdraw,Name="Withdrawal",Module="Vault",IsActive=false};
        db.AddRange(disabled,new UserPermissionOverride{UserId=a.Id,Permission=disabled,Effect=PermissionOverrideEffect.Deny});await db.SaveChangesAsync();
        var setup=new ShiftPermissionSetup(db);await setup.ConfigureAsync();await setup.ConfigureAsync();
        Assert.Equal(ShiftPermissions.Codes.Length,await db.Permissions.CountAsync(x=>ShiftPermissions.Codes.Contains(x.Code)));
        Assert.False((await db.Permissions.SingleAsync(x=>x.Code==ShiftPermissions.Withdraw)).IsActive);Assert.Single(await db.UserPermissionOverrides.ToListAsync());
        Assert.Empty(await db.RolePermissions.ToListAsync());
        var local=new IdentityRole<Guid>{Id=Guid.NewGuid(),Name="Configurable local role"};db.Add(local);await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(()=>setup.ConfigureAsync(local.Id));
    }
}
