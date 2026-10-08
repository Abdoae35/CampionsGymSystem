using CampionsSystem.BLL.Crm;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed class CrmPermissionTests
{
    [Fact] public async Task ExplicitSetupIsIdempotentAndPreservesInactiveCapabilities()
    {
        await using var fixture=await AuthTestDatabase.CreateAsync();await using var db=fixture.CreateContext();
        db.Permissions.Add(new(){Code="Leads.Convert",Name="Conversion",Module="Leads",IsActive=false});await db.SaveChangesAsync();
        var setup=new CrmPermissionSetup(db);await setup.ConfigureAsync();await setup.ConfigureAsync();
        Assert.Equal(CrmPermissions.Codes.Length,await db.Permissions.CountAsync(p=>CrmPermissions.Codes.Contains(p.Code)));
        Assert.False((await db.Permissions.SingleAsync(p=>p.Code=="Leads.Convert")).IsActive);
    }
}
