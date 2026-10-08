using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using CampionsSystem.API.Authentication;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CampionsSystem.DAL.Tests;

public sealed partial class AuthenticationApiTests : IAsyncLifetime
{
    private AuthTestDatabase database = null!;
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;
    private Guid userId;
    private readonly string password = "Test-password-" + Guid.NewGuid().ToString("N");

    public async Task InitializeAsync()
    {
        database = await AuthTestDatabase.CreateAsync();
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=postgres",
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = "AuthTests",
                ["Jwt:Audience"] = "AuthTests.Client",
                ["Jwt:AccessTokenMinutes"] = "5",
                ["Logging:LogLevel:Default"] = "None"
            }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ApplicationDbContext>();
                services.AddScoped(_ => database.CreateContext());
                services.AddControllers().AddApplicationPart(typeof(PermissionProbeController).Assembly);
                services.AddAuthorization(options => options.AddPolicy("TestAuthenticated", policy => policy.RequireAuthenticatedUser()));
            });
        });
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        await using var scope = factory.Services.CreateAsyncScope();
        var user = new ApplicationUser { UserName = "api-user", Email = "api@example.invalid", LockoutEnabled = true };
        Assert.True((await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().CreateAsync(user, password)).Succeeded);
        userId = user.Id;
    }
    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (factory is not null) await factory.DisposeAsync();
        if (database is not null) await database.DisposeAsync();
    }
    private Task<HttpResponseMessage> Login(string? suppliedPassword = null, string identifier = "api-user") =>
        client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserNameOrEmail = identifier, Password = suppliedPassword ?? password });
    private async Task<LoginResponse> Authenticate()
    {
        using var response = await Login();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
        return result;
    }
    private async Task UpdateUser(Action<ApplicationUser> change)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByIdAsync(userId.ToString()))!;
        change(user);
        Assert.True((await users.UpdateAsync(user)).Succeeded);
    }

    [Fact] public async Task ActiveUserCanLoginAndJwtHasOnlyUsefulClaims()
    {
        var result = await Authenticate();
        Assert.Equal(userId, result.User.Id);
        Assert.True(result.ExpiresAt > DateTimeOffset.UtcNow);
        Assert.True(result.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(5));
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        Assert.Equal(userId.ToString(), token.Subject);
        Assert.DoesNotContain(token.Claims, claim => claim.Type.Contains("permission", StringComparison.OrdinalIgnoreCase)
            || claim.Type.Contains("password", StringComparison.OrdinalIgnoreCase));
    }
    [Fact] public async Task InvalidPasswordAndUnknownUserReturnSame401()
    {
        using var wrongPassword = await Login("wrong");
        using var wrongUser = await Login(identifier: "missing-user");
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongUser.StatusCode);
        var a = await wrongPassword.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var b = await wrongUser.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal(a.GetProperty("title").GetString(), b.GetProperty("title").GetString());
    }
    [Fact] public async Task InactiveUserCannotLogin()
    {
        await UpdateUser(user => user.IsActive = false);
        using var response = await Login();
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact] public async Task MeRequiresAuthentication()
    {
        using var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact] public async Task MeResolvesLinkedEmployeeAndPrimaryCurrentBranch()
    {
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo")).DateTime);
        var employee = new Employee { UserId = userId, FullName = "API Employee", Phone = "API", HireDate = today.AddDays(-30) };
        var branch = new GymBranch { City = new City { Name = "City" }, Name = "Current branch" };
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Add(new EmployeeBranchAssignment { Employee = employee, Branch = branch, StartDate = today, EndDate = today, IsPrimary = true, AssignedByUserId = userId });
            await db.SaveChangesAsync();
        }
        var login = await Authenticate();
        Assert.Equal(employee.Id, login.User.EmployeeId);
        using var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var context = (await response.Content.ReadFromJsonAsync<CurrentUserContext>())!;
        Assert.Equal(employee.Id, context.EmployeeId);
        Assert.Equal("API Employee", context.EmployeeName);
        Assert.Equal(branch.Id, context.CurrentBranchId);
    }
    [Fact] public async Task OwnerWithoutEmployeeOrBranchIsSafe()
    {
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            Assert.True((await roles.CreateAsync(new IdentityRole<Guid>("Owner") { Id = Guid.NewGuid() })).Succeeded);
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True((await users.AddToRoleAsync((await users.FindByIdAsync(userId.ToString()))!, "Owner")).Succeeded);
        }
        await Authenticate();
        using var response = await client.GetAsync("/api/auth/me");
        var context = (await response.Content.ReadFromJsonAsync<CurrentUserContext>())!;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Owner", context.Roles);
        Assert.Null(context.EmployeeId);
        Assert.Null(context.CurrentBranch);
    }
    [Fact] public async Task ExistingJwtIsRejectedWhenUserIsDeactivated()
    {
        await Authenticate();
        await UpdateUser(user => user.IsActive = false);
        using var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact] public async Task TamperedJwtIsRejected()
    {
        var result = await Authenticate();
        var parts = result.AccessToken.Split('.');
        parts[2] = (parts[2][0] == 'A' ? "B" : "A") + parts[2][1..];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", string.Join('.', parts));
        using var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Theory]
    [InlineData("expired")]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("key")]
    public async Task JwtValidationRejectsInvalidTokens(string invalid)
    {
        var settings = factory.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
        var now = DateTime.UtcNow;
        var key = invalid == "key" ? new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)) : settings.GetSigningKey();
        var token = new JwtSecurityToken(invalid == "issuer" ? "wrong" : settings.Issuer,
            invalid == "audience" ? "wrong" : settings.Audience,
            [new System.Security.Claims.Claim("sub", userId.ToString())],
            invalid == "expired" ? now.AddMinutes(-10) : now,
            invalid == "expired" ? now.AddMinutes(-5) : now.AddMinutes(5),
            new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        using var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    private async Task GrantPermission(string roleName, string code, bool active = true)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var role = await roles.FindByNameAsync(roleName);
        if (role is null)
        {
            role = new IdentityRole<Guid>(roleName) { Id = Guid.NewGuid() };
            Assert.True((await roles.CreateAsync(role)).Succeeded);
        }
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByIdAsync(userId.ToString()))!;
        if (!await users.IsInRoleAsync(user, roleName)) Assert.True((await users.AddToRoleAsync(user, roleName)).Succeeded);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var permission = await db.Permissions.SingleOrDefaultAsync(x => x.Code == code);
        permission ??= new Permission { Code = code, Name = code, Module = code.Split('.')[0], IsActive = active };
        db.Add(new RolePermission { RoleId = role.Id, Permission = permission });
        await db.SaveChangesAsync();
    }
    [Fact] public async Task PermissionAllowsProtectedAction()
    {
        await GrantPermission("Reception", "Members.View");
        await Authenticate();
        using var response = await client.GetAsync("/_tests/auth/permission");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    [Fact] public async Task MissingPermissionReturns403()
    {
        await Authenticate();
        using var response = await client.GetAsync("/_tests/auth/permission");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    [Fact] public async Task AnonymousPermissionRequestReturns401()
    {
        using var response = await client.GetAsync("/_tests/auth/permission");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact] public async Task MeReturnsUnionOfRolesWithDistinctActivePermissions()
    {
        await GrantPermission("Reception", "Members.View");
        await GrantPermission("Sales", "Members.View");
        await GrantPermission("Sales", "Members.Edit");
        await GrantPermission("Reception", "Payments.Refund", active: false);
        await Authenticate();
        using var response = await client.GetAsync("/api/auth/me");
        var context = (await response.Content.ReadFromJsonAsync<CurrentUserContext>())!;
        Assert.Equal(new[] { "Members.Edit", "Members.View" }, context.Permissions);
        Assert.Equal(new[] { "Reception", "Sales" }, context.Roles);
    }
    [Fact] public async Task PermissionRevocationAffectsExistingJwt()
    {
        await GrantPermission("Reception", "Members.View");
        await Authenticate();
        using var before = await client.GetAsync("/_tests/auth/permission");
        Assert.Equal(HttpStatusCode.OK, before.StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.RolePermissions.Remove(await db.RolePermissions.SingleAsync());
            await db.SaveChangesAsync();
        }
        using var after = await client.GetAsync("/_tests/auth/permission");
        Assert.Equal(HttpStatusCode.Forbidden, after.StatusCode);
    }
    [Fact] public async Task PermissionDeactivationAffectsExistingJwt()
    {
        await GrantPermission("Reception", "Members.View");
        await Authenticate();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var permission = await db.Permissions.SingleAsync(x => x.Code == "Members.View");
            permission.IsActive = false;
            await db.SaveChangesAsync();
        }
        using var response = await client.GetAsync("/_tests/auth/permission");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    [Fact] public async Task RoleRevocationAffectsEvenRoleOnlyChecksOnExistingJwt()
    {
        await GrantPermission("Owner", "Members.View");
        await Authenticate();
        using var before = await client.GetAsync("/_tests/auth/owner");
        Assert.Equal(HttpStatusCode.OK, before.StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True((await users.RemoveFromRoleAsync((await users.FindByIdAsync(userId.ToString()))!, "Owner")).Succeeded);
        }
        using var after = await client.GetAsync("/_tests/auth/owner");
        Assert.Equal(HttpStatusCode.Forbidden, after.StatusCode);
    }
    [Fact] public async Task DynamicProviderPreservesStandardPolicies()
    {
        using var anonymous = await client.GetAsync("/_tests/auth/standard-policy");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        await Authenticate();
        using var authenticated = await client.GetAsync("/_tests/auth/standard-policy");
        Assert.Equal(HttpStatusCode.OK, authenticated.StatusCode);
    }

    [Fact] public async Task InvalidRequestReturns400()
    {
        using var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Theory]
    [InlineData("/api/activities/coaches")]
    [InlineData("/api/classes/sessions")]
    [InlineData("/api/freelance-plans")]
    public async Task NewManagementEndpointsRequirePermissions(string route)
    {
        using var anonymous = await client.PostAsJsonAsync(route, new { });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        await Authenticate();
        using var forbidden = await client.PostAsJsonAsync(route, new { });
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task AuthorizedActivityManagementCreatesExternalCoachAndAudit()
    {
        await GrantPermission("Manager", "Activities.ManageCoaches");
        await Authenticate();
        using var response = await client.PostAsJsonAsync("/api/activities/coaches", new
        { coachType = 1, employeeId = (Guid?)null, externalName = "External HTTP coach", percentage = 25 });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Single(await db.ActivityCoaches.ToListAsync());
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "ActivityCoach.Created");
    }

    [Fact] public async Task BranchManagementRequiresPermissionAndCreatesAuditWhenAuthorized()
    {
        await Authenticate();
        using var forbidden = await client.PostAsJsonAsync("/api/branches", new { name = "New", cityId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        await GrantPermission("BranchAdministrator", "Branches.Manage");
        await GrantPermission("BranchAdministrator", "Branches.AccessAll");
        Guid cityId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var city = new City { Name = "HTTP branch city" }; db.Add(city); await db.SaveChangesAsync(); cityId = city.Id;
        }
        using var created = await client.PostAsJsonAsync("/api/branches", new { name = "New", cityId });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        await using var checkScope = factory.Services.CreateAsyncScope();
        var check = checkScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Single(await check.GymBranches.ToListAsync());
        Assert.Contains(await check.AuditLogs.ToListAsync(), x => x.Action == "Branch.Created");
    }

    [Fact] public async Task DirectBranchPermissionDenyAffectsExistingJwt()
    {
        await GrantPermission("GlobalOperator", "Branches.View");
        await GrantPermission("GlobalOperator", "Branches.AccessAll");
        await Authenticate();
        Guid branchId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var branch = new GymBranch { Name = "Protected", City = new City { Name = "Scope" } }; db.Add(branch); await db.SaveChangesAsync(); branchId = branch.Id;
        }
        using var allowed = await client.GetAsync($"/api/branches/{branchId}"); Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var permission = await db.Permissions.SingleAsync(x => x.Code == "Branches.AccessAll");
            db.Add(new UserPermissionOverride { UserId = userId, Permission = permission, Effect = CampionsSystem.DAL.Enums.PermissionOverrideEffect.Deny });
            await db.SaveChangesAsync();
        }
        using var denied = await client.GetAsync($"/api/branches/{branchId}"); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
    }

    private static DateOnly BranchToday => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,
        TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo")).DateTime);
    private async Task<(Guid Employee, Guid Source, Guid Target)> BranchFixture()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var source = new GymBranch { Name = "HTTP A", City = new City { Name = "HTTP Branches" } };
        var target = new GymBranch { Name = "HTTP B", City = source.City };
        var employee = new Employee { UserId = userId, FullName = "HTTP Employee", Phone = "http-branch-employee", HireDate = BranchToday.AddDays(-30) };
        db.AddRange(target, new EmployeeBranchAssignment { Employee = employee, Branch = source, AssignedByUserId = userId,
            StartDate = BranchToday.AddDays(-10), IsPrimary = true });
        await db.SaveChangesAsync(); return (employee.Id, source.Id, target.Id);
    }
    [Fact] public async Task BranchListRequiresAuthenticationAndPermission()
    {
        using var anonymous = await client.GetAsync("/api/branches"); Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        await Authenticate(); using var forbidden = await client.GetAsync("/api/branches"); Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }
    [Fact] public async Task BranchEmployeeCannotReadOrUpdateAnotherBranchAndCannotHardDeleteHistory()
    {
        var data = await BranchFixture(); await GrantPermission("BranchOperator", "Branches.View"); await GrantPermission("BranchOperator", "Branches.Manage");
        await Authenticate();
        using var list = await client.GetAsync("/api/branches"); Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var branches = (await list.Content.ReadFromJsonAsync<List<CampionsSystem.BLL.Branches.BranchDetails>>())!;
        Assert.Equal(data.Source, Assert.Single(branches).Id);
        using var get = await client.GetAsync($"/api/branches/{data.Target}"); Assert.Equal(HttpStatusCode.Forbidden, get.StatusCode);
        using var update = await client.PutAsJsonAsync($"/api/branches/{data.Target}", new { name = "Spoofed", cityId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        using var delete = await client.DeleteAsync($"/api/branches/{data.Source}"); Assert.Equal(HttpStatusCode.MethodNotAllowed, delete.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(2, await db.GymBranches.CountAsync()); Assert.Single(await db.EmployeeBranchAssignments.ToListAsync());
    }
    [Fact] public async Task TransferApiPreservesHistoryAndMeResolvesNewBranchOnSameJwt()
    {
        var data = await BranchFixture(); await GrantPermission("TransferOperator", "Employees.Transfer");
        await GrantPermission("TransferOperator", "Employees.View"); await GrantPermission("TransferOperator", "Branches.AccessAll");
        await Authenticate();
        using var before = await client.GetAsync("/api/auth/me");
        Assert.Equal(data.Source, (await before.Content.ReadFromJsonAsync<CurrentUserContext>())!.CurrentBranchId);
        using var transfer = await client.PostAsJsonAsync($"/api/employees/{data.Employee}/transfer-branch", new { targetBranchId = data.Target, effectiveDate = BranchToday });
        Assert.Equal(HttpStatusCode.OK, transfer.StatusCode);
        using var after = await client.GetAsync("/api/auth/me"); Assert.Equal(HttpStatusCode.OK, after.StatusCode);
        Assert.Equal(data.Target, (await after.Content.ReadFromJsonAsync<CurrentUserContext>())!.CurrentBranchId);
        using var history = await client.GetAsync($"/api/employees/{data.Employee}/branch-history"); Assert.Equal(HttpStatusCode.OK, history.StatusCode);
        Assert.Equal(2, (await history.Content.ReadFromJsonAsync<List<CampionsSystem.BLL.Branches.BranchAssignmentDetails>>())!.Count);
        using var duplicate = await client.PostAsJsonAsync($"/api/employees/{data.Employee}/transfer-branch", new { targetBranchId = data.Target, effectiveDate = BranchToday });
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
    }
    [Fact] public async Task TransferWithoutPermissionOrCrossBranchScopeReturns403()
    {
        var data = await BranchFixture(); await Authenticate();
        using var missing = await client.PostAsJsonAsync($"/api/employees/{data.Employee}/transfer-branch", new { targetBranchId = data.Target, effectiveDate = BranchToday });
        Assert.Equal(HttpStatusCode.Forbidden, missing.StatusCode);
        await GrantPermission("ScopedManager", "Employees.Transfer");
        using var outOfScope = await client.PostAsJsonAsync($"/api/employees/{data.Employee}/transfer-branch", new { targetBranchId = data.Target, effectiveDate = BranchToday });
        Assert.Equal(HttpStatusCode.Forbidden, outOfScope.StatusCode);
    }
    [Fact] public async Task InitialAssignmentApiAndCurrentQueryWorkWithoutEmployeeCrud()
    {
        await GrantPermission("AssignmentOperator", "Employees.Manage"); await GrantPermission("AssignmentOperator", "Employees.View");
        await GrantPermission("AssignmentOperator", "Branches.AccessAll");
        Guid employeeId, branchId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var employee = new Employee { FullName = "Unassigned", Phone = "http-unassigned", HireDate = BranchToday.AddDays(-1) };
            var branch = new GymBranch { Name = "Initial", City = new City { Name = "Initial" } }; db.AddRange(employee, branch);
            await db.SaveChangesAsync(); employeeId = employee.Id; branchId = branch.Id;
        }
        await Authenticate();
        using var assigned = await client.PostAsJsonAsync($"/api/employees/{employeeId}/assign-branch", new { branchId, startDate = BranchToday });
        Assert.Equal(HttpStatusCode.OK, assigned.StatusCode);
        using var current = await client.GetAsync($"/api/employees/{employeeId}/branch-assignment"); Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        Assert.Equal(branchId, (await current.Content.ReadFromJsonAsync<CampionsSystem.BLL.Branches.BranchAssignmentDetails>())!.BranchId);
    }
    [Fact] public async Task InvalidBranchRequestReturns400InsteadOfInternalError()
    {
        await GrantPermission("BranchAdmin", "Branches.Manage"); await GrantPermission("BranchAdmin", "Branches.AccessAll"); await Authenticate();
        using var response = await client.PostAsJsonAsync("/api/branches", new { name = "", cityId = Guid.NewGuid(), maxBorrowedGymSessions = -1 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact] public async Task EmployeeEndpointsRequireAuthenticationAndPermissions()
    {
        using var anonymous = await client.GetAsync("/api/employees"); Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        await Authenticate();
        using var list = await client.GetAsync("/api/employees"); Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        using var create = await client.PostAsJsonAsync("/api/employees", new { fullName = "No access", phone = "0", hireDate = BranchToday });
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
        using var status = await client.PatchAsJsonAsync($"/api/employees/{Guid.NewGuid()}/status", new { status = "Suspended" });
        Assert.Equal(HttpStatusCode.Forbidden, status.StatusCode);
    }

    [Fact] public async Task EmployeeListAndBranchRouteCannotEscapeCurrentScope()
    {
        var data = await BranchFixture(); await GrantPermission("EmployeeReader", "Employees.View");
        Guid otherId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var other = new Employee { FullName = "Other employee", Phone = "elsewhere", HireDate = BranchToday.AddDays(-1) };
            db.Add(new EmployeeBranchAssignment { Employee = other, BranchId = data.Target, StartDate = BranchToday, IsPrimary = true, AssignedByUserId = userId });
            await db.SaveChangesAsync(); otherId = other.Id;
        }
        await Authenticate();
        using var list = await client.GetAsync("/api/employees?search=http&pageSize=1"); Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var result = (await list.Content.ReadFromJsonAsync<CampionsSystem.BLL.Employees.EmployeeListResult>())!;
        Assert.Equal(1, result.TotalCount); Assert.Equal(data.Employee, Assert.Single(result.Items).Id);
        using var branch = await client.GetAsync($"/api/branches/{data.Source}/employees"); Assert.Equal(HttpStatusCode.OK, branch.StatusCode);
        using var spoof = await client.GetAsync($"/api/employees?branchId={data.Target}"); Assert.Equal(HttpStatusCode.Forbidden, spoof.StatusCode);
        using var routeSpoof = await client.GetAsync($"/api/branches/{data.Target}/employees?branchId={data.Source}"); Assert.Equal(HttpStatusCode.Forbidden, routeSpoof.StatusCode);
        using var details = await client.GetAsync($"/api/employees/{otherId}"); Assert.Equal(HttpStatusCode.NotFound, details.StatusCode);
        using var update = await client.PutAsJsonAsync($"/api/employees/{otherId}", new { fullName = "Spoof", phone = "0", hireDate = BranchToday });
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
    }

    private async Task GrantEmployeeManagement()
    {
        await GrantPermission("EmployeeAdmin", "Employees.View");
        await GrantPermission("EmployeeAdmin", "Employees.Manage");
        await GrantPermission("EmployeeAdmin", "Branches.AccessAll");
    }

    [Fact] public async Task EmployeeCreateUpdateAndStatusHttpFlowPreservesHistoryWithoutAccountCreation()
    {
        var data = await BranchFixture(); await GrantEmployeeManagement(); await Authenticate();
        using var create = await client.PostAsJsonAsync("/api/employees", new
            { fullName = "New employee", phone = "123", jobTitle = "Trainer", employmentType = "Staff", hireDate = BranchToday, initialBranchId = data.Source });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var employee = (await create.Content.ReadFromJsonAsync<CampionsSystem.BLL.Employees.EmployeeDetails>())!;
        Assert.False(employee.HasSystemAccount); Assert.Equal(data.Source, employee.CurrentBranch?.Id);
        using var update = await client.PutAsJsonAsync($"/api/employees/{employee.Id}", new
            { fullName = "Edited employee", phone = "456", jobTitle = "Coach", employmentType = "Freelance", hireDate = BranchToday });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("Edited employee", (await update.Content.ReadFromJsonAsync<CampionsSystem.BLL.Employees.EmployeeDetails>())!.FullName);
        foreach (var value in new[] { "Suspended", "Active", "Resigned" })
        {
            using var status = await client.PatchAsJsonAsync($"/api/employees/{employee.Id}/status", new { status = value, reason = "HTTP test" });
            Assert.Equal(HttpStatusCode.OK, status.StatusCode);
            Assert.Equal(value, (await status.Content.ReadFromJsonAsync<CampionsSystem.BLL.Employees.EmployeeDetails>())!.Status);
        }
        using var details = await client.GetAsync($"/api/employees/{employee.Id}"); Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        using var delete = await client.DeleteAsync($"/api/employees/{employee.Id}"); Assert.Equal(HttpStatusCode.MethodNotAllowed, delete.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await db.Users.CountAsync());
        Assert.Single(await db.EmployeeBranchAssignments.Where(a => a.EmployeeId == employee.Id).ToListAsync());
        Assert.Equal(3, await db.AuditLogs.CountAsync(a => a.EntityId == employee.Id && a.Action == "EmployeeStatusChanged"));
        Assert.Equal(BranchToday, (await db.Employees.SingleAsync(e => e.Id == employee.Id)).TerminationDate);
    }

    [Fact] public async Task EmployeeProfileRejectsBranchAccountAndStatusMassAssignment()
    {
        var data = await BranchFixture(); await GrantEmployeeManagement(); await Authenticate();
        foreach (var field in new[] { "branchId", "initialBranchId", "userId", "status", "terminationDate" })
        {
            var body = new Dictionary<string, object> { ["fullName"] = "Unchanged", ["phone"] = "0", ["hireDate"] = BranchToday.AddDays(-30), [field] = data.Target };
            using var response = await client.PutAsJsonAsync($"/api/employees/{data.Employee}", body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        using var create = await client.PostAsJsonAsync("/api/employees", new { fullName = "No login", phone = "0", hireDate = BranchToday, userId });
        Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);
        using var details = await client.GetAsync($"/api/employees/{data.Employee}");
        Assert.Equal(data.Source, (await details.Content.ReadFromJsonAsync<CampionsSystem.BLL.Employees.EmployeeDetails>())!.CurrentBranch?.Id);
    }

    [Fact] public async Task EmployeeInvalidRequestsReturn400()
    {
        await GrantEmployeeManagement(); await Authenticate();
        using var profile = await client.PostAsJsonAsync("/api/employees", new { fullName = "", phone = "0", hireDate = BranchToday });
        Assert.Equal(HttpStatusCode.BadRequest, profile.StatusCode);
        using var missingDate = await client.PostAsJsonAsync("/api/employees", new { fullName = "Missing date", phone = "0" });
        Assert.Equal(HttpStatusCode.BadRequest, missingDate.StatusCode);
        using var missingStatus = await client.PatchAsJsonAsync($"/api/employees/{Guid.NewGuid()}/status", new { reason = "No status" });
        Assert.Equal(HttpStatusCode.BadRequest, missingStatus.StatusCode);
        foreach (var query in new[] { "page=0", "pageSize=101", "page=2147483647&pageSize=100", "status=99", "employmentType=99" })
        {
            using var list = await client.GetAsync("/api/employees?" + query); Assert.Equal(HttpStatusCode.BadRequest, list.StatusCode);
        }
    }

    [Fact] public async Task EmployeeApiInitialAssignmentFailureDoesNotLeavePartialEmployee()
    {
        var data = await BranchFixture(); await GrantEmployeeManagement(); await Authenticate();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.GymBranches.SingleAsync(b => b.Id == data.Target)).IsActive = false; await db.SaveChangesAsync();
        }
        using var response = await client.PostAsJsonAsync("/api/employees", new { fullName = "Rollback", phone = "0", hireDate = BranchToday, initialBranchId = data.Target });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await using var checkScope = factory.Services.CreateAsyncScope(); var check = checkScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await check.Employees.CountAsync()); Assert.Empty(await check.AuditLogs.ToListAsync());
    }

}
