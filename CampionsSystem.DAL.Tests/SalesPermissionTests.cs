using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    [Fact] public async Task FinancialCatalogPreservesInactiveCapabilitiesAndOverridesAndInfersNoRoleGrants()
    {
        var disabled=new Permission{Code=SalesPermissions.QuickCancel,Name="Quick",Module="Sales",IsActive=false};
        db.AddRange(disabled,new UserPermissionOverride{UserId=actor.Id,Permission=disabled,Effect=PermissionOverrideEffect.Deny});await db.SaveChangesAsync();
        var setup=new SalesPermissionSetup(db);await setup.ConfigureAsync();await setup.ConfigureAsync();Assert.Equal(6,await db.Permissions.CountAsync(p=>SalesPermissions.Codes.Contains(p.Code)));
        Assert.False((await db.Permissions.SingleAsync(p=>p.Code==SalesPermissions.QuickCancel)).IsActive);Assert.Empty(await db.RolePermissions.ToListAsync());Assert.Single(await db.UserPermissionOverrides.ToListAsync());
        var local=new IdentityRole<Guid>{Id=Guid.NewGuid(),Name="Local"};db.Add(local);await db.SaveChangesAsync();await Assert.ThrowsAsync<InvalidOperationException>(()=>setup.ConfigureAsync(local.Id));
    }
}
