using CampionsSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

internal sealed class AuthTestDatabase : IAsyncDisposable
{
    public NpgsqlConnection Connection { get; private set; } = null!;
    public NpgsqlTransaction Transaction { get; private set; } = null!;

    public static async Task<AuthTestDatabase> CreateAsync(string? targetMigration = null)
    {
        var fixture = new AuthTestDatabase();
        try
        {
            var connection = Environment.GetEnvironmentVariable("GYM_SCHEMA_TEST_CONNECTION")
                ?? $"Host=localhost;Database=postgres;Username={Environment.UserName};Timeout=5";
            if (new NpgsqlConnectionStringBuilder(connection).Host is not ("localhost" or "127.0.0.1" or "::1"))
                throw new InvalidOperationException("Schema tests require a verified localhost database.");
            fixture.Connection = new NpgsqlConnection(connection);
            await fixture.Connection.OpenAsync();
            fixture.Transaction = await fixture.Connection.BeginTransactionAsync();
            var schema = "gym_auth_test_" + Guid.NewGuid().ToString("N");
            await using var command = new NpgsqlCommand($"CREATE SCHEMA {schema}; SET LOCAL search_path TO {schema};",
                fixture.Connection, fixture.Transaction);
            await command.ExecuteNonQueryAsync();
            await using var db = fixture.CreateContext();
            var sql = db.GetService<IMigrator>().GenerateScript(null, targetMigration)
                .Replace("START TRANSACTION;", "").Replace("BEGIN TRANSACTION;", "").Replace("COMMIT;", "");
            await db.Database.ExecuteSqlRawAsync(sql);
            return fixture;
        }
        catch
        {
            await fixture.DisposeAsync();
            throw;
        }
    }

    public ApplicationDbContext CreateContext()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(Connection).Options);
        // Already-open transaction: wiring only, no synchronous database query.
        db.Database.UseTransaction(Transaction);
        return db;
    }

    public async ValueTask DisposeAsync()
    {
        if (Transaction is not null)
        {
            await Transaction.RollbackAsync();
            await Transaction.DisposeAsync();
        }
        if (Connection is not null) await Connection.DisposeAsync();
    }
}
