using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.API.Authentication;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var configureCrmPermissions = args.Contains("--configure-crm-permissions", StringComparer.Ordinal);
var configureBranchPermissions = args.Contains("--configure-branch-permissions", StringComparer.Ordinal);
var configureMemberPermissions = args.Contains("--configure-member-permissions", StringComparer.Ordinal);
var configureSubscriptionPermissions = args.Contains("--configure-subscription-permissions", StringComparer.Ordinal);
var configureCheckInPermissions = args.Contains("--configure-checkin-permissions", StringComparer.Ordinal);
var configureSalesPermissions = args.Contains("--configure-sales-permissions", StringComparer.Ordinal);
var configureShiftPermissions = args.Contains("--configure-shift-permissions", StringComparer.Ordinal);
var seedAuthDemo = args.Contains("--seed-auth-demo", StringComparer.Ordinal);
var builder = WebApplication.CreateBuilder(args.Where(arg => arg != "--configure-crm-permissions" && arg != "--seed-auth-demo" && arg != "--configure-branch-permissions" && arg != "--configure-member-permissions" && arg != "--configure-subscription-permissions" && arg != "--configure-checkin-permissions" && arg != "--configure-sales-permissions" && arg != "--configure-shift-permissions").ToArray());

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException(
            "Configure ConnectionStrings:DefaultConnection using user secrets or a production secret provider.");
    options.UseNpgsql(connectionString, postgres => postgres.SetPostgresVersion(14, 0));
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddGymAuthentication(builder.Configuration);
builder.Services.AddScoped<DevelopmentAuthSeeder>();
builder.Services.AddScoped<CampionsSystem.BLL.Crm.LeadManagementService>();
builder.Services.AddScoped<CampionsSystem.BLL.Imports.ExcelImportService>();
builder.Services.AddScoped<CampionsSystem.BLL.Crm.CrmPermissionSetup>();
builder.Services.AddScoped<BranchManagementService>();
builder.Services.AddScoped<CampionsSystem.BLL.Members.MemberManagementService>();
builder.Services.AddScoped<CampionsSystem.BLL.Members.MemberPermissionSetup>();
builder.Services.AddScoped<CampionsSystem.BLL.Employees.EmployeeManagementService>();
builder.Services.AddScoped<IBranchAccessService, BranchAccessService>();
builder.Services.AddScoped<BranchPermissionSetup>();
builder.Services.AddScoped<EmployeeBranchAssignmentService>();
builder.Services.AddScoped<OperationAccess>();
builder.Services.AddScoped<CampionsSystem.BLL.Subscriptions.SubscriptionService>();
builder.Services.AddScoped<CampionsSystem.BLL.Subscriptions.SubscriptionPermissionSetup>();
builder.Services.AddScoped<CampionsSystem.BLL.CheckIns.CheckInService>();
builder.Services.AddScoped<CampionsSystem.BLL.CheckIns.CheckInPermissionSetup>();
builder.Services.AddScoped<CampionsSystem.BLL.Sales.SalesService>();
builder.Services.AddScoped<CampionsSystem.BLL.Commissions.CommissionService>();
builder.Services.AddScoped<CampionsSystem.BLL.Coaching.CoachingService>();
builder.Services.AddScoped<CampionsSystem.BLL.MonthlyFinance.MonthlyFinanceService>();
builder.Services.AddScoped<CampionsSystem.BLL.MonthlyFinance.SalaryService>();
builder.Services.AddScoped<CampionsSystem.BLL.MonthlyFinance.PayrollService>();
builder.Services.AddScoped<CampionsSystem.BLL.MonthlyFinance.MaintenanceService>();
builder.Services.AddScoped<CampionsSystem.BLL.Sales.OfferService>();
builder.Services.AddScoped<CampionsSystem.BLL.Sales.SalesPermissionSetup>();
builder.Services.AddScoped<CampionsSystem.BLL.Shifts.ShiftService>();
builder.Services.AddScoped<CampionsSystem.BLL.Shifts.VaultService>();
builder.Services.AddScoped<CampionsSystem.BLL.Shifts.ExpenseService>();
builder.Services.AddScoped<CampionsSystem.BLL.Shifts.ShiftReportService>();
builder.Services.AddScoped<CampionsSystem.BLL.Shifts.ShiftPermissionSetup>();
builder.Services.AddScoped<ActivityManagementService>();
builder.Services.AddScoped<CampionsSystem.BLL.Plans.MembershipPlanManagementService>();
builder.Services.AddScoped<FreelancePlanService>();
builder.Services.AddScoped<SubscriptionPurchaseService>();
builder.Services.AddScoped<ClassSessionService>();
builder.Services.AddScoped<CoachReportService>();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
if (string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("DefaultConnection")))
    throw new InvalidOperationException(
        "Configure ConnectionStrings:DefaultConnection using user secrets or a production secret provider.");

if (configureShiftPermissions)
{
    var target=new Npgsql.NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString("DefaultConnection"));
    if(!app.Environment.IsDevelopment() || target.Host is not ("localhost" or "127.0.0.1" or "::1"))
        throw new InvalidOperationException("Shift capability setup is allowed only in Development against localhost.");
    var configuredRole=app.Configuration["Shifts:GlobalRoleId"];
    Guid? role=string.IsNullOrWhiteSpace(configuredRole)?null:Guid.Parse(configuredRole);
    await using(var scope=app.Services.CreateAsyncScope())
        await scope.ServiceProvider.GetRequiredService<CampionsSystem.BLL.Shifts.ShiftPermissionSetup>().ConfigureAsync(role,app.Lifetime.ApplicationStopping);
    Console.WriteLine("Shift/Vault/Expense capabilities configured; existing activation/grants/overrides preserved.");
    await app.DisposeAsync();return;
}

if (configureSalesPermissions)
{
    var target=new Npgsql.NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString("DefaultConnection"));
    if(!app.Environment.IsDevelopment() || target.Host is not ("localhost" or "127.0.0.1" or "::1"))
        throw new InvalidOperationException("Sales permission setup is allowed only in Development against localhost.");
    var configuredRole=app.Configuration["Sales:ManagementRoleId"];
    Guid? role=string.IsNullOrWhiteSpace(configuredRole)?null:Guid.Parse(configuredRole);
    await using(var scope=app.Services.CreateAsyncScope())
        await scope.ServiceProvider.GetRequiredService<CampionsSystem.BLL.Sales.SalesPermissionSetup>().ConfigureAsync(role,app.Lifetime.ApplicationStopping);
    Console.WriteLine("Sales/Payment capabilities configured; existing activation/grants/overrides preserved.");
    await app.DisposeAsync();return;
}

if (configureCheckInPermissions)
{
    var target = new Npgsql.NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString("DefaultConnection"));
    if (!app.Environment.IsDevelopment() || target.Host is not ("localhost" or "127.0.0.1" or "::1"))
        throw new InvalidOperationException("Check-in permission setup is allowed only in Development against localhost.");
    var configuredRole=app.Configuration["CheckIns:ManagementRoleId"];
    Guid? role=string.IsNullOrWhiteSpace(configuredRole)?null:Guid.Parse(configuredRole);
    await using (var scope = app.Services.CreateAsyncScope())
        await scope.ServiceProvider.GetRequiredService<CampionsSystem.BLL.CheckIns.CheckInPermissionSetup>().ConfigureAsync(role,app.Lifetime.ApplicationStopping);
    Console.WriteLine("Check-in permissions configured; existing activation/grants/overrides preserved.");
    await app.DisposeAsync();
    return;
}

if (configureSubscriptionPermissions)
{
    var target = new Npgsql.NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString("DefaultConnection"));
    if (!app.Environment.IsDevelopment() || target.Host is not ("localhost" or "127.0.0.1" or "::1"))
        throw new InvalidOperationException("Subscription permission setup is allowed only in Development against localhost.");
    var configuredRole=app.Configuration["Subscriptions:ManagementRoleId"];
    Guid? role=string.IsNullOrWhiteSpace(configuredRole)?null:Guid.Parse(configuredRole);
    await using (var scope = app.Services.CreateAsyncScope())
        await scope.ServiceProvider.GetRequiredService<CampionsSystem.BLL.Subscriptions.SubscriptionPermissionSetup>().ConfigureAsync(role,app.Lifetime.ApplicationStopping);
    Console.WriteLine("Subscription permissions configured; existing activation/grants/overrides preserved.");
    await app.DisposeAsync();
    return;
}

if (configureCrmPermissions)
{
    var target = new Npgsql.NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString("DefaultConnection"));
    if (!app.Environment.IsDevelopment() || target.Host is not ("localhost" or "127.0.0.1" or "::1"))
        throw new InvalidOperationException("CRM permission setup is allowed only in Development against localhost.");
    await using (var scope = app.Services.CreateAsyncScope())
        await scope.ServiceProvider.GetRequiredService<CampionsSystem.BLL.Crm.CrmPermissionSetup>().ConfigureAsync(app.Lifetime.ApplicationStopping);
    Console.WriteLine("CRM permission setup completed; existing activation/grants preserved.");
    await app.DisposeAsync();
    return;
}

if (configureMemberPermissions)
{
    var target = new Npgsql.NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString("DefaultConnection"));
    if (!app.Environment.IsDevelopment() || target.Host is not ("localhost" or "127.0.0.1" or "::1"))
        throw new InvalidOperationException("Member permission setup is allowed only in Development against localhost.");
    await using (var scope = app.Services.CreateAsyncScope())
        await scope.ServiceProvider.GetRequiredService<CampionsSystem.BLL.Members.MemberPermissionSetup>().ConfigureAsync(app.Lifetime.ApplicationStopping);
    Console.WriteLine("Member permission setup completed; existing activation/grants preserved.");
    await app.DisposeAsync();
    return;
}

if (configureBranchPermissions)
{
    var target = new Npgsql.NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString("DefaultConnection"));
    if (!app.Environment.IsDevelopment() || target.Host is not ("localhost" or "127.0.0.1" or "::1"))
        throw new InvalidOperationException("Branch permission setup is allowed only in Development against localhost.");
    await using (var scope = app.Services.CreateAsyncScope())
        await scope.ServiceProvider.GetRequiredService<BranchPermissionSetup>().ConfigureAsync(app.Lifetime.ApplicationStopping);
    Console.WriteLine("Branch permission setup completed; no users, passwords or migrations changed.");
    await app.DisposeAsync();
    return;
}

if (seedAuthDemo)
{
    var target = new Npgsql.NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString("DefaultConnection"));
    if (!app.Environment.IsDevelopment() || target.Host is not ("localhost" or "127.0.0.1" or "::1"))
        throw new InvalidOperationException("Demo seeding is allowed only in Development against localhost.");
    var password = app.Configuration["DemoAuth:Password"];
    if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Configure DemoAuth:Password securely.");
    await using (var scope = app.Services.CreateAsyncScope())
        await scope.ServiceProvider.GetRequiredService<DevelopmentAuthSeeder>().SeedAsync(password, app.Lifetime.ApplicationStopping);
    Console.WriteLine("Local development auth demo data seeded. No migrations applied; credentials not displayed.");
    await app.DisposeAsync();
    return;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Local connectivity diagnostic; never expose this in production.
    app.MapGet("/health/database", async (ApplicationDbContext database, CancellationToken cancellationToken) =>
        await database.Database.CanConnectAsync(cancellationToken)
            ? Results.Ok(new { status = "healthy" })
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Entry point for automated API integration tests.
public partial class Program { }
