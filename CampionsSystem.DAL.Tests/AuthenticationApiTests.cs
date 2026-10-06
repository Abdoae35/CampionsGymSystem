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

public sealed class AuthenticationApiTests : IAsyncLifetime
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
            var permission = await db.Permissions.SingleAsync();
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
}
