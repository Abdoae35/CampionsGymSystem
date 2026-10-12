# Authentication rule/validation evidence

Baseline source, not new Private drafts. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Authentication/DevelopmentAuthSeeder.cs

Lines 30–35

```csharp


    public async Task SeedAsync(string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Configure DemoAuth:Password securely.");
        await using var transaction = database.Database.CurrentTransaction is null
            ? await database.Database.BeginTransactionAsync(cancellationToken) : null;

```

Lines 67–73

```csharp

        await AccountAsync("dev.inactive", false, null, password);
        var suspended = await AccountAsync("dev.suspended", true, "Reception", password);

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(),
            TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId)).DateTime);
        var city = await database.Cities.FindAsync([Id("city")], cancellationToken);
        if (city is null) { city = new City { Id = Id("city"), Name = "Auth Demo City" }; database.Add(city); }

```

Lines 88–93

```csharp

        var user = await users.FindByNameAsync(name);
        if (user is not null)
        {
            if (user.Id != Id(name)) throw new InvalidOperationException("A demo username is occupied by another account; no data was changed.");
            return user; // Do not reset password, activation, lockout or customized role memberships.
        }

```

Lines 98–103

```csharp

    }

    private async Task EmployeeAsync(ApplicationUser user, GymBranch branch, ApplicationUser creator,
        DateOnly today, EmploymentStatus status, CancellationToken cancellationToken)
    {
        var employee = await database.Employees.SingleOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);

```

Lines 114–119

```csharp

    private static Guid Id(string value) => new(SHA256.HashData(Encoding.UTF8.GetBytes("Campions:AuthDemo:" + value)).AsSpan(0, 16));
    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException("Demo Identity setup failed; transaction will roll back. Check password policy or account conflicts.");
    }
}

```

## CampionsSystem.BLL/Authentication/AuthContracts.cs

Lines 45–50

```csharp


public sealed class AuthContextOptions
{
    public const string SectionName = "AuthContext";
    public string TimeZoneId { get; init; } = "Africa/Cairo";
}

```

## CampionsSystem.BLL/Authentication/AuthenticationService.cs

Lines 22–27

```csharp

            // Refuse ambiguous legacy email mappings rather than choosing an account.
            var normalizedEmail = users.NormalizeEmail(identifier);
            var matches = await database.Users.Where(x => x.NormalizedEmail == normalizedEmail)
                .Take(2).ToListAsync(cancellationToken);
            if (matches.Count != 1) return null;
            user = matches[0];

```

## CampionsSystem.BLL/Authentication/UserContextService.cs

Lines 9–19

```csharp

public sealed class UserContextService(ApplicationDbContext database, IPermissionService permissionService,
    TimeProvider clock, IOptions<AuthContextOptions> options, EmployeeBranchResolver branches)
{
    private readonly TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);

    public async Task<CurrentUserContext?> ResolveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow();
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, timeZone).DateTime);
        var user = await database.Users.AsNoTracking()
            .Where(x => x.Id == userId)

```
