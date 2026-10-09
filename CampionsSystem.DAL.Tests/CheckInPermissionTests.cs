using CampionsSystem.BLL.CheckIns;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CheckInTests
{
    [Fact] public async Task CheckInCatalogPreservesActivationExistingGrantsAndDirectDenies()
    {
        var global=new IdentityRole<Guid>{Id=Guid.NewGuid(),Name="Global setup"};var capability=new Permission{Code="Branches.AccessAll",Name="Global",Module="Branches"};
        var disabled=new Permission{Code=CheckInPermissions.Create,Name="Create",Module="CheckIns",IsActive=false};db.AddRange(global,capability,disabled,new RolePermission{Role=global,Permission=capability},new UserPermissionOverride{UserId=actor.Id,Permission=disabled,Effect=PermissionOverrideEffect.Deny});await db.SaveChangesAsync();
        var setup=new CheckInPermissionSetup(db);await setup.ConfigureAsync(global.Id);await setup.ConfigureAsync(global.Id);Assert.Equal(2,await db.Permissions.CountAsync(x=>CheckInPermissions.Codes.Contains(x.Code)));
        Assert.False((await db.Permissions.SingleAsync(x=>x.Code==CheckInPermissions.Create)).IsActive);Assert.Equal(3,await db.RolePermissions.CountAsync());Assert.Single(await db.UserPermissionOverrides.ToListAsync());
    }
    [Fact] public async Task CheckInSetupDoesNotInferStaffGrantsOrGiveGlobalCapability()
    {
        var local=new IdentityRole<Guid>{Id=Guid.NewGuid(),Name="Local setup"};db.Add(local);await db.SaveChangesAsync();var setup=new CheckInPermissionSetup(db);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>setup.ConfigureAsync(local.Id));await setup.ConfigureAsync();Assert.Empty(await db.RolePermissions.ToListAsync());
    }
}
