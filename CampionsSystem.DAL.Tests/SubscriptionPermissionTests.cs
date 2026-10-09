using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SubscriptionLifecycleTests
{
    [Fact] public async Task SubscriptionCatalogSetupPreservesDisabledPermissionsGrantsAndDenies()
    {
        var global=new IdentityRole<Guid>{Id=Guid.NewGuid(),Name="Configured global",NormalizedName="CONFIGURED GLOBAL"};
        var capability=new Permission{Code="Branches.AccessAll",Name="Global",Module="Branches"};
        var disabled=new Permission{Code=SubscriptionPermissions.Extend,Name="Extend",Module="Subscriptions",IsActive=false};
        db.AddRange(global,capability,disabled,new RolePermission{Role=global,Permission=capability},
            new UserPermissionOverride{UserId=actor.Id,Permission=disabled,Effect=CampionsSystem.DAL.Enums.PermissionOverrideEffect.Deny});await db.SaveChangesAsync();
        var setup=new SubscriptionPermissionSetup(db);await setup.ConfigureAsync(global.Id);await setup.ConfigureAsync(global.Id);
        Assert.Equal(4,await db.Permissions.CountAsync(x=>SubscriptionPermissions.Codes.Contains(x.Code)));
        Assert.False((await db.Permissions.SingleAsync(x=>x.Code==SubscriptionPermissions.Extend)).IsActive);
        Assert.Equal(5,await db.RolePermissions.CountAsync(x=>x.RoleId==global.Id));Assert.Single(await db.UserPermissionOverrides.ToListAsync());
    }
    [Fact] public async Task SubscriptionSetupDoesNotInferRolesOrGrantBranchRoleGlobalManagement()
    {
        var local=new IdentityRole<Guid>{Id=Guid.NewGuid(),Name="Local",NormalizedName="LOCAL"};db.Add(local);await db.SaveChangesAsync();
        var setup=new SubscriptionPermissionSetup(db);await Assert.ThrowsAsync<InvalidOperationException>(()=>setup.ConfigureAsync(local.Id));
        await setup.ConfigureAsync();Assert.Empty(await db.RolePermissions.ToListAsync());
    }
}
