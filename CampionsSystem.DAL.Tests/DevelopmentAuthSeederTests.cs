using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampionsSystem.DAL.Tests;

public sealed class DevelopmentAuthSeederTests : IAsyncLifetime
{
    private AuthTestDatabase database = null!;
    private ServiceProvider services = null!;
    private AsyncServiceScope scope;
    private ApplicationDbContext db = null!;
    private readonly string password = "Demo-test-" + Guid.NewGuid().ToString("N");
    public async Task InitializeAsync()
    {
        database = await AuthTestDatabase.CreateAsync();
        var registrations = new ServiceCollection();
        registrations.AddLogging();
        registrations.AddScoped(_ => database.CreateContext());
        registrations.AddSingleton(TimeProvider.System);
        registrations.Configure<AuthContextOptions>(_ => { });
        registrations.AddIdentityCore<ApplicationUser>(options => options.Password.RequiredLength = 12)
            .AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<ApplicationDbContext>();
        registrations.AddScoped<DevelopmentAuthSeeder>();
        registrations.AddScoped<IPermissionService, PermissionService>();
        registrations.AddScoped<UserContextService>();
        registrations.AddScoped<EmployeeBranchResolver>();
        registrations.AddScoped<IAuthenticationService, AuthenticationService>();
        services = registrations.BuildServiceProvider();
        scope = services.CreateAsyncScope();
        db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await Seed();
    }
    public async Task DisposeAsync()
    {
        await scope.DisposeAsync();
        await services.DisposeAsync();
        await database.DisposeAsync();
    }
    private Task Seed() => scope.ServiceProvider.GetRequiredService<DevelopmentAuthSeeder>().SeedAsync(password);
    private Task<CurrentUserContext?> Login(string name) => scope.ServiceProvider.GetRequiredService<IAuthenticationService>()
        .ValidateCredentialsAsync(new LoginRequest { UserNameOrEmail = name, Password = password });

    [Fact]
    public async Task RepeatedSeedDoesNotDuplicateFixtures()
    {
        await Seed();
        Assert.Equal(5, await db.Users.CountAsync());
        Assert.Equal(6, await db.Roles.CountAsync());
        Assert.Equal(DevelopmentAuthSeeder.PermissionCodes.Length, await db.Permissions.CountAsync());
        Assert.Equal(2, await db.Employees.CountAsync());
        Assert.Equal(2, await db.EmployeeBranchAssignments.CountAsync());
        Assert.Single(await db.GymBranches.ToListAsync());
    }
    [Fact]
    public async Task FixturesExerciseRealIdentityAndCurrentContext()
    {
        var owner = Assert.IsType<CurrentUserContext>(await Login("dev.owner"));
        Assert.Null(owner.EmployeeId);
        Assert.Null(owner.CurrentBranch);
        Assert.Contains("Employees.View", owner.Permissions);
        Assert.Contains("Branches.AccessAll", owner.Permissions);
        Assert.Equal(DevelopmentAuthSeeder.PermissionCodes.Length, owner.Permissions.Count);
        var reception = Assert.IsType<CurrentUserContext>(await Login("dev.reception"));
        Assert.NotNull(reception.EmployeeId);
        Assert.Equal("Auth Demo Branch", reception.CurrentBranch?.Name);
        Assert.Contains("Payments.Create", reception.Permissions);
        Assert.Contains("Shifts.Manage", reception.Permissions);
        Assert.Contains("Expenses.Create", reception.Permissions);
        Assert.Contains("Advances.Create", reception.Permissions);
        Assert.DoesNotContain("Vault.Withdraw", reception.Permissions);
        Assert.DoesNotContain("Branches.AccessAll", reception.Permissions);
        Assert.Empty(Assert.IsType<CurrentUserContext>(await Login("dev.no-permissions")).Permissions);
        Assert.Null(await Login("dev.inactive"));
        Assert.Null(await Login("dev.suspended"));
        var manager=await db.Roles.SingleAsync(x=>x.Name=="BranchManager");
        var management=await db.RolePermissions.Where(x=>x.RoleId==manager.Id).Select(x=>x.Permission.Code).ToListAsync();
        Assert.Contains("Vault.Manage",management);Assert.DoesNotContain("Vault.Withdraw",management);
    }
    [Fact]
    public async Task RerunPreservesChangedAccountsAndRevokedGrants()
    {
        var user = await db.Users.SingleAsync(x => x.UserName == "dev.reception");
        user.IsActive = false;
        var grant = await db.RolePermissions.FirstAsync();
        db.Remove(grant);
        await db.SaveChangesAsync();
        var count = await db.RolePermissions.CountAsync();
        await Seed();
        Assert.False(user.IsActive);
        Assert.Equal(count, await db.RolePermissions.CountAsync());
        Assert.Null(await Login("dev.reception"));
    }
}
