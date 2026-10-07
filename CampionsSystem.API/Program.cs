using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.API.Authentication;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var seedAuthDemo = args.Contains("--seed-auth-demo", StringComparer.Ordinal);
var builder = WebApplication.CreateBuilder(args.Where(arg => arg != "--seed-auth-demo").ToArray());

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
builder.Services.AddScoped<BranchManagementService>();
builder.Services.AddScoped<EmployeeBranchAssignmentService>();
builder.Services.AddScoped<OperationAccess>();
builder.Services.AddScoped<ActivityManagementService>();
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
