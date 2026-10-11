using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

// Separate committed test schema is necessary so independent connections observe competing commits.
public sealed class BranchConcurrencyTests : IAsyncLifetime
{
    private readonly string schema = "gym_branch_race_" + Guid.NewGuid().ToString("N");
    private string adminConnection = null!;
    private string scopedConnection = null!;
    private Guid employeeId, sourceId, firstId, secondId, actorId;
    private DateOnly today;
    public async Task InitializeAsync()
    {
        adminConnection = Environment.GetEnvironmentVariable("GYM_SCHEMA_TEST_CONNECTION")
            ?? $"Host=localhost;Database=postgres;Username={Environment.UserName};Timeout=5";
        await using (var connection = new NpgsqlConnection(adminConnection))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE SCHEMA {schema}", connection); await command.ExecuteNonQueryAsync();
        }
        scopedConnection = new NpgsqlConnectionStringBuilder(adminConnection) { SearchPath = schema, Pooling = false }.ConnectionString;
        await using var db = Context(); await db.Database.MigrateAsync();
        var actor = new ApplicationUser { UserName = "race-admin" };
        var source = new GymBranch { Name = "Race A", City = new City { Name = "Race" } };
        var first = new GymBranch { Name = "Race B", City = source.City };
        var second = new GymBranch { Name = "Race C", City = source.City };
        var employee = new Employee { FullName = "Concurrent Employee", Phone = "race-employee", HireDate = new(2026, 1, 1) };
        today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo")).DateTime);
        db.AddRange(actor, source, first, second, employee); await db.SaveChangesAsync();
        actorId = actor.Id; sourceId = source.Id; firstId = first.Id; secondId = second.Id; employeeId = employee.Id;
    }
    public async Task DisposeAsync()
    {
        // Identifier is generated internally from a fixed prefix and GUID; never accept a caller's schema.
        await using var connection = new NpgsqlConnection(adminConnection); await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS {schema} CASCADE", connection); await command.ExecuteNonQueryAsync();
    }
    private ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(scopedConnection).Options);
    private EmployeeBranchAssignmentService Service(ApplicationDbContext db)
    {
        var current = new TestBranchUser(new(actorId, "race-admin", null, null, [], ["Employees.Transfer", "Employees.Manage", "Branches.AccessAll"], null));
        return new(db, current, new OperationAccess(current, db, TimeProvider.System, Options.Create(new AuthContextOptions())));
    }
    private static async Task<bool> Attempt(Func<Task<BranchAssignmentDetails>> change)
    {
        try { await change(); return true; }
        catch (BusinessRuleException) { return false; }
    }
    [Fact]
    public async Task CompetingTransfersCannotCreateTwoCurrentAssignments()
    {
        await using (var setup = Context())
        {
            setup.Add(new EmployeeBranchAssignment { EmployeeId = employeeId, BranchId = sourceId, AssignedByUserId = actorId,
                StartDate = today.AddDays(-5), IsPrimary = true }); await setup.SaveChangesAsync();
        }
        await using var first = Context(); await using var second = Context();
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<bool> Transfer(ApplicationDbContext db, Guid target) { await start.Task; return await Attempt(() => Service(db).TransferAsync(employeeId, new(target, today))); }
        var a = Transfer(first, firstId); var b = Transfer(second, secondId); start.SetResult();
        var outcomes = await Task.WhenAll(a, b); Assert.Equal(1, outcomes.Count(x => x));
        await using var check = Context();
        var history = await check.EmployeeBranchAssignments.AsNoTracking().ToListAsync();
        Assert.Equal(2, history.Count); Assert.Single(history, x => x.StartDate <= today && (x.EndDate == null || x.EndDate >= today));
        Assert.Equal(today.AddDays(-1), history.Single(x => x.BranchId == sourceId).EndDate);
        Assert.Single(await check.AuditLogs.Where(x => x.Action == "Employee.BranchTransferred").ToListAsync());
    }
    [Fact]
    public async Task CompetingInitialAssignmentsCannotOverlap()
    {
        await using var first = Context(); await using var second = Context();
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<bool> Assign(ApplicationDbContext db, Guid branch) { await start.Task; return await Attempt(() => Service(db).AssignAsync(employeeId, new(branch, today))); }
        var a = Assign(first, firstId); var b = Assign(second, secondId); start.SetResult();
        var outcomes = await Task.WhenAll(a, b); Assert.Equal(1, outcomes.Count(x => x));
        await using var check = Context(); Assert.Single(await check.EmployeeBranchAssignments.ToListAsync());
        Assert.Single(await check.AuditLogs.Where(x => x.Action == "Employee.BranchAssigned").ToListAsync());
    }
}
