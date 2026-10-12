using System.Net;
using System.Net.Http.Json;
using CampionsSystem.BLL.Commissions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace CampionsSystem.DAL.Tests;
public sealed partial class AuthenticationApiTests
{
    [Fact] public async Task CommissionApiRequiresAuthenticationAndExplicitCapabilities()
    {
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/commissions/periods")).StatusCode);
        await Authenticate();Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/commissions/periods")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/commissions/rules",new ConfigureCommissionRequest(Guid.NewGuid(),null,DateOnly.FromDateTime(DateTime.Today),new(30000,5,7),new(30000,3,5)))).StatusCode);
    }
    [Fact] public async Task CommissionApiProvidesScopedProgressAndOwnerConfiguredRulesWithoutShift()
    {
        var f=await BranchFixture();await GrantPermission("CommissionOperator",CommissionPermissions.View);await GrantPermission("CommissionOperator",CommissionPermissions.Manage);await GrantPermission("CommissionOperator",CommissionPermissions.Close);await Authenticate();
        var date=DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo")).DateTime);
        var r=new ConfigureCommissionRequest(Guid.NewGuid(),null,date,new(30000,5,7),new(30000,3,5));
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/commissions/rules",r)).StatusCode);
        await GrantPermission("CommissionOperator","Branches.AccessAll");
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/commissions/rules",r)).StatusCode);
        var period=(await (await client.PostAsJsonAsync("/api/commissions/periods",new OpenCommissionPeriodRequest(Guid.NewGuid(),"Custom",date,date.AddDays(13)))).Content.ReadFromJsonAsync<CommissionPeriodDetails>())!;
        Assert.Equal(0,(await client.GetFromJsonAsync<CommissionProgress>($"/api/commissions/periods/{period.Id}"))!.TotalCommission);
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync($"/api/commissions/periods/{period.Id}/sources")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.GetAsync("/api/commissions/periods?pageSize=101")).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Assert.Empty(await db.Shifts.ToListAsync());Assert.Empty(await db.Payrolls.ToListAsync());
    }
    [Fact] public async Task CommissionDirectDenyOverridesRoleAndBranchOwnershipCannotLeakReports()
    {
        var f=await BranchFixture();await GrantPermission("CommissionOperator",CommissionPermissions.View);await GrantPermission("CommissionOperator",CommissionPermissions.Manage);await GrantPermission("CommissionOperator","Branches.AccessAll");await Authenticate();
        await using(var scope=factory.Services.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var permission=await db.Permissions.SingleAsync(x=>x.Code==CommissionPermissions.Manage);
            db.Add(new UserPermissionOverride{UserId=userId,PermissionId=permission.Id,Effect=PermissionOverrideEffect.Deny});
            var global=await db.Permissions.SingleAsync(x=>x.Code=="Branches.AccessAll");db.Add(new UserPermissionOverride{UserId=userId,PermissionId=global.Id,Effect=PermissionOverrideEffect.Deny});await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync($"/api/commissions/periods?branchId={f.Target}")).StatusCode);
        var date=DateOnly.FromDateTime(DateTime.Today).AddDays(1);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/commissions/rules",new ConfigureCommissionRequest(Guid.NewGuid(),f.Employee,date,new(30000,5,7),new(30000,3,5)))).StatusCode);
    }
    [Fact] public async Task CommissionRuleContractRejectsClientComputedCommission()
    {
        await BranchFixture();await GrantPermission("CommissionOperator",CommissionPermissions.Manage);await GrantPermission("CommissionOperator","Branches.AccessAll");await Authenticate();
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/commissions/rules",new{operationId=Guid.NewGuid(),earnedCommission=500})).StatusCode);
    }
}
