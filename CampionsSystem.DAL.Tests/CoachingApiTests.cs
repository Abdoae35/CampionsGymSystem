using System.Net;
using System.Net.Http.Json;
using CampionsSystem.BLL.Coaching;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace CampionsSystem.DAL.Tests;

public sealed partial class AuthenticationApiTests
{
    [Theory]
    [InlineData("/api/coaching/private/packages")]
    [InlineData("/api/coaching/classes/packages")]
    [InlineData("/api/coaching/earnings/coaches/00000000-0000-0000-0000-000000000001/balance")]
    public async Task CoachingApiRequiresAuthenticationAndExplicitCapabilities(string route)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(route)).StatusCode); await Authenticate();
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(route)).StatusCode);
    }
    [Fact] public async Task CoachingApiPackageCatalogIsBranchScopedAndRejectsSpoofedFinancialTerms()
    {
        var f = await BranchFixture(); await GrantPermission("CoachAdmin", CoachingPermissions.PrivateManage); await GrantPermission("CoachAdmin", CoachingPermissions.PrivateView); await Authenticate();
        var r = new CoachingPackageRequest(f.Source, "Private", 10, 30, 1000); var created = await client.PostAsJsonAsync("/api/coaching/private/packages", r);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        Assert.Single((await client.GetFromJsonAsync<CoachingPage<CoachingPackageDetails>>($"/api/coaching/private/packages?branchId={f.Source}"))!.Items);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/coaching/private/packages?branchId={f.Target}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/api/coaching/private/packages?branchId={f.Source}&pageSize=101")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/coaching/private/packages", new { branchId = f.Source, name = "Forged", sessionCount = 10, durationDays = 30, price = 1000, earnings = 500 })).StatusCode);
    }
    [Fact] public async Task CoachingApiDirectDenyOverridesPayAndTargetRoleCapabilitiesImmediately()
    {
        var f = await BranchFixture(); await GrantPermission("CoachOwner", CoachingPermissions.Pay); await GrantPermission("CoachOwner", CoachingPermissions.Targets); await GrantPermission("CoachOwner", "Branches.AccessAll"); await Authenticate();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            foreach (var code in new[] { CoachingPermissions.Pay, CoachingPermissions.Targets })
            {
                var permission = await db.Permissions.SingleAsync(p => p.Code == code); db.Add(new UserPermissionOverride { UserId = userId, PermissionId = permission.Id, Effect = PermissionOverrideEffect.Deny });
            }
            await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/coaching/earnings/payouts", new CoachPayoutRequest(Guid.NewGuid(), f.Source, f.Employee, 100, Guid.NewGuid(), Guid.NewGuid(), "Part"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/coaching/earnings/targets", new CoachTargetRequest(Guid.NewGuid(), Guid.NewGuid(), f.Employee, 10000, null))).StatusCode);
    }
    [Fact] public async Task CoachingApiLegacyClassAttendanceWriteIsDisabled()
    {
        await GrantPermission("ClassAdmin", "Classes.Manage"); await Authenticate();
        Assert.Equal(HttpStatusCode.Gone, (await client.PostAsJsonAsync($"/api/classes/sessions/{Guid.NewGuid()}/attendance/{Guid.NewGuid()}", new { })).StatusCode);
    }
}
