using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using Microsoft.Extensions.Options;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

// Committed disposable schema is necessary: independent sessions cannot see transactional fixtures.
public sealed class MemberClientCodeConcurrencyTests : IAsyncLifetime
{
    private string connection = null!;
    private string schema = "gym_member_concurrent_" + Guid.NewGuid().ToString("N");
    private Guid a, b, actor, owner;
    private ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
    public async Task InitializeAsync()
    {
        var config = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("GYM_SCHEMA_TEST_CONNECTION")
            ?? $"Host=localhost;Database=postgres;Username={Environment.UserName};Timeout=5");
        await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE SCHEMA {schema}", admin); await command.ExecuteNonQueryAsync();
        config.SearchPath = schema; connection = config.ConnectionString;
        try
        {
            await using var db = Context();
            await db.Database.ExecuteSqlRawAsync(db.GetService<IMigrator>().GenerateScript());
            var user = new ApplicationUser { UserName = "concurrent-member" }; actor = user.Id;
            var first = new GymBranch { Name = "A", City = new City { Name = "Concurrent city" } };
            var second = new GymBranch { Name = "B", City = first.City };
            var sales = new Employee { FullName = "Concurrent Sales", Phone = "sales", UserId = actor, HireDate = new(2026,1,1) };
            owner = sales.Id; a = first.Id; b = second.Id; db.AddRange(user, first, second, sales); await db.SaveChangesAsync();
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }
    public async Task DisposeAsync()
    {
        if (connection is null) return;
        await using var cn = new NpgsqlConnection(connection); await cn.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS {schema} CASCADE", cn); await command.ExecuteNonQueryAsync();
    }
    private int phoneCounter;
    private MemberManagementService Service(ApplicationDbContext db)
    {
        var user = new TestBranchUser(new(actor, "concurrent-member", null, null, [],
            ["Members.Create", "Members.AssignSales", "Branches.AccessAll"], null));
        return new(db, user, new OperationAccess(user, db, TimeProvider.System, Options.Create(new AuthContextOptions())), new Capabilities());
    }
    private sealed class Capabilities : IPermissionService
    {
        public Task<HashSet<string>> GetUserPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(new HashSet<string> { "Members.OwnSalesCustomers" });
    }
    private async Task<long> CreateAsync(Guid branch)
    {
        await using var db = Context();
        var result = await Service(db).CreateMemberAsync(new() { FullName = "Concurrent", PrimaryPhone = "+44123456" + Interlocked.Increment(ref phoneCounter).ToString("D4"),
            TargetRegistrationBranchId = branch, CreationType = MemberCreationType.ManagementCreated, SalesEmployeeId = owner });
        return result.ClientCode!.Value;
    }
    [Fact] public async Task IndependentConnectionsCreateUniqueSameBranchCodes()
    {
        var codes = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => CreateAsync(a)));
        Assert.Equal(12, codes.Distinct().Count()); Assert.Equal(Enumerable.Range(1, 12).Select(x => (long)x), codes.Order());
    }
    [Fact] public async Task DifferentBranchesAllocateIndependently()
    {
        var codes = await Task.WhenAll(CreateAsync(a), CreateAsync(b)); Assert.Equal(new long[] { 1, 1 }, codes);
        await using var db = Context(); Assert.Equal(2, await db.Members.CountAsync(m => m.ClientCode == 1));
    }
    [Fact] public async Task OneBranchLockDoesNotBlockOtherBranch()
    {
        await using var db = Context(); await using var tx = await db.Database.BeginTransactionAsync();
        Assert.Equal(1, await new BranchClientCodeGenerator(db).NextAsync(a));
        var other = CreateAsync(b); Assert.Equal(1, await other.WaitAsync(TimeSpan.FromSeconds(10)));
        await tx.RollbackAsync(); Assert.Equal(1, await CreateAsync(a));
    }
    [Fact] public async Task ConcurrentAssignmentsSerializeAndRetainOneActiveOwner()
    {
        await CreateAsync(a);
        Guid member, first, second;
        await using (var db = Context())
        {
            member = await db.Members.Select(m => m.Id).SingleAsync();
            var e1 = await db.Employees.SingleAsync(e => e.Id == owner);
            var e2 = new Employee { FullName = "Second", Phone = "second", User = new ApplicationUser { UserName = "second" }, HireDate = new(2026,1,1) };
            db.Add(e2); await db.SaveChangesAsync(); first = e1.Id; second = e2.Id;
        }
        await using (var db = Context()) await Service(db).ReturnMemberToHouseAsync(member, new());
        async Task Assign(Guid employee)
        {
            await using var db = Context(); await Service(db).AssignSalesAsync(member, new() { SalesEmployeeId = employee });
        }
        await Task.WhenAll(Assign(first), Assign(second));
        await using var check = Context(); var rows = await check.MemberSalesAssignments.Where(x => x.MemberId == member).ToListAsync();
        Assert.Equal(3, rows.Count); Assert.Single(rows, x => x.EndedAt == null); Assert.Equal(2, rows.Count(x => x.EndedAt != null));
    }

}
