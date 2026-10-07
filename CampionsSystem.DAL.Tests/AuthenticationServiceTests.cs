using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CampionsSystem.DAL.Tests;

public sealed class AuthenticationServiceTests : IAsyncLifetime
{
    private AuthTestDatabase database = null!;
    private ServiceProvider services = null!;
    private AsyncServiceScope scope;
    private ApplicationDbContext db = null!;
    private ApplicationUser user = null!;
    private readonly string password = "Test-password-" + Guid.NewGuid().ToString("N");
    private readonly FixedTimeProvider clock = new();

    public async Task InitializeAsync()
    {
        database = await AuthTestDatabase.CreateAsync();
        var registrations = new ServiceCollection();
        registrations.AddLogging();
        registrations.AddScoped(_ => database.CreateContext());
        registrations.AddSingleton<TimeProvider>(clock);
        registrations.Configure<AuthContextOptions>(options => { });
        registrations.AddIdentityCore<ApplicationUser>(options => options.Password.RequiredLength = 12)
            .AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<ApplicationDbContext>();
        registrations.AddScoped<IPermissionService, PermissionService>();
        registrations.AddScoped<UserContextService>();
        registrations.AddScoped<EmployeeBranchResolver>();
        registrations.AddScoped<IAuthenticationService, AuthenticationService>();
        services = registrations.BuildServiceProvider();
        scope = services.CreateAsyncScope();
        db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        user = new ApplicationUser { UserName = "test-user", Email = "test@example.invalid", LockoutEnabled = true };
        Assert.True((await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().CreateAsync(user, password)).Succeeded);
    }

    public async Task DisposeAsync()
    {
        await scope.DisposeAsync();
        if (services is not null) await services.DisposeAsync();
        if (database is not null) await database.DisposeAsync();
    }
    private Task<CurrentUserContext?> Login(string? identifier = null, string? suppliedPassword = null) =>
        scope.ServiceProvider.GetRequiredService<IAuthenticationService>().ValidateCredentialsAsync(
            new LoginRequest { UserNameOrEmail = identifier ?? user.UserName!, Password = suppliedPassword ?? password });

    [Fact] public async Task ActiveUserLogsInByUserNameOrEmail()
    {
        Assert.Equal(user.Id, (await Login())?.UserId);
        Assert.Equal(user.Id, (await Login("TEST@EXAMPLE.INVALID"))?.UserId);
    }
    [Fact] public async Task InvalidPasswordIsRejectedAndCounted()
    {
        Assert.Null(await Login(suppliedPassword: "wrong"));
        Assert.Equal(1, user.AccessFailedCount);
    }
    [Fact] public async Task InactiveUserIsRejected()
    {
        user.IsActive = false;
        await db.SaveChangesAsync();
        Assert.Null(await Login());
    }
    [Fact] public async Task SuspendedEmployeeIsRejected()
    {
        db.Add(new Employee { User = user, FullName = "Employee", Phone = "TEST", HireDate = new(2026, 1, 1), EmploymentStatus = EmploymentStatus.Suspended });
        await db.SaveChangesAsync();
        Assert.Null(await Login());
    }
    [Fact] public async Task CurrentBranchUsesDateEligiblePrimaryAssignment()
    {
        var employee = new Employee { User = user, FullName = "Employee", Phone = "TEST", HireDate = new(2026, 1, 1) };
        var city = new City { Name = "City" };
        var primary = new GymBranch { City = city, Name = "Primary" };
        var other = new GymBranch { City = city, Name = "Other" };
        db.AddRange(
            new EmployeeBranchAssignment { Employee = employee, Branch = primary, StartDate = new(2026, 10, 1), EndDate = new(2026, 10, 6), IsPrimary = true, AssignedByUser = user },
                        new EmployeeBranchAssignment { Employee = employee, Branch = other, StartDate = new(2026, 10, 7), IsPrimary = true, AssignedByUser = user },
            new EmployeeBranchAssignment { Employee = employee, Branch = other, StartDate = new(2026, 1, 1), EndDate = new(2026, 1, 31), IsPrimary = true, AssignedByUser = user });
        await db.SaveChangesAsync();
        var context = await Login();
        Assert.Equal(employee.Id, context?.EmployeeId);
        Assert.Equal(primary.Id, context?.CurrentBranchId);
    }
    [Fact] public async Task GlobalOwnerDoesNotRequireEmployeeOrBranch()
    {
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        Assert.True((await roles.CreateAsync(new IdentityRole<Guid>("Owner") { Id = Guid.NewGuid() })).Succeeded);
        Assert.True((await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().AddToRoleAsync(user, "Owner")).Succeeded);
        var context = await Login();
        Assert.NotNull(context);
        Assert.Null(context.EmployeeId);
        Assert.Null(context.CurrentBranch);
        Assert.Contains("Owner", context.Roles);
    }
    [Fact] public async Task PermissionsUnionIsDistinctAndIgnoresInactiveCodes()
    {
        var roleA = new IdentityRole<Guid>("A") { Id = Guid.NewGuid() };
        var roleB = new IdentityRole<Guid>("B") { Id = Guid.NewGuid() };
        var view = new Permission { Code = "Members.View", Name = "View", Module = "Members" };
        var edit = new Permission { Code = "Members.Edit", Name = "Edit", Module = "Members" };
        var inactive = new Permission { Code = "Payments.Refund", Name = "Refund", Module = "Payments", IsActive = false };
        db.AddRange(roleA, roleB, new IdentityUserRole<Guid> { UserId = user.Id, RoleId = roleA.Id }, new IdentityUserRole<Guid> { UserId = user.Id, RoleId = roleB.Id },
            new RolePermission { Role = roleA, Permission = view }, new RolePermission { Role = roleB, Permission = view },
            new RolePermission { Role = roleB, Permission = edit }, new RolePermission { Role = roleA, Permission = inactive });
        await db.SaveChangesAsync();
        var permissions = await scope.ServiceProvider.GetRequiredService<IPermissionService>().GetUserPermissionsAsync(user.Id);
        Assert.Equal(2, permissions.Count);
        Assert.Contains("Members.View", permissions);
        Assert.Contains("Members.Edit", permissions);
        Assert.DoesNotContain("Payments.Refund", permissions);
    }
    [Fact] public async Task TwoFactorAccountDoesNotReceivePasswordOnlyLogin()
    {
        user.TwoFactorEnabled = true;
        await db.SaveChangesAsync();
        Assert.Null(await Login());
    }
    [Fact] public async Task LockedOutAccountIsRejected()
    {
        user.LockoutEnd = clock.GetUtcNow().AddHours(1);
        await db.SaveChangesAsync();
        Assert.Null(await Login());
    }
    [Fact] public async Task AnonymousCurrentUserReturnsNull()
    {
        var current = new CurrentUserService(new AnonymousIdentity(), scope.ServiceProvider.GetRequiredService<UserContextService>());
        Assert.False(current.IsAuthenticated);
        Assert.Null(current.UserId);
        Assert.Null(await current.GetAsync());
    }
    private sealed class AnonymousIdentity : ICurrentUserIdentity
    {
        public bool IsAuthenticated => false;
        public Guid? UserId => null;
    }
    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 10, 6, 12, 0, 0, TimeSpan.Zero);
    }
}
