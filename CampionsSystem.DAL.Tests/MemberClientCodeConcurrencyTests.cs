using CampionsSystem.BLL.Members;
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
    private Guid a, b, actor;
    private ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
    public async Task InitializeAsync()
    {
        var config = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("GYM_SCHEMA_TEST_CONNECTION")
            ?? $"Host=localhost;Database=postgres;Username={Environment.UserName};Timeout=5");
        await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE SCHEMA {schema}", admin); await command.ExecuteNonQueryAsync();
        config.SearchPath = schema; connection = config.ConnectionString;
        await using var db = Context();
        await db.Database.ExecuteSqlRawAsync(db.GetService<IMigrator>().GenerateScript());
        var user = new ApplicationUser { UserName = "concurrent-member" }; actor = user.Id;
        var first = new GymBranch { Name = "A", City = new City { Name = "Concurrent city" } };
        var second = new GymBranch { Name = "B", City = first.City };
        a = first.Id; b = second.Id; db.AddRange(user, first, second); await db.SaveChangesAsync();
    }
    public async Task DisposeAsync()
    {
        if (connection is null) return;
        await using var cn = new NpgsqlConnection(connection); await cn.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP SCHEMA {schema} CASCADE", cn); await command.ExecuteNonQueryAsync();
    }
    private async Task<long> CreateAsync(Guid branch)
    {
        await using var db = Context(); await using var transaction = await db.Database.BeginTransactionAsync();
        var code = await new BranchClientCodeGenerator(db).NextAsync(branch);
        db.Add(new Member { FullName = "Concurrent", PrimaryPhone = Guid.NewGuid().ToString("N"),
            MemberCode = Guid.NewGuid().ToString("N"), ClientCode = code, HomeBranchId = branch, CreatedByUserId = actor });
        await db.SaveChangesAsync(); await transaction.CommitAsync(); return code;
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
}
