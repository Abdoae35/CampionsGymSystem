using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

public sealed class MemberClientCodeTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private GymBranch branch = null!;
    private ApplicationUser user = null!;
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        user = new ApplicationUser { UserName = "client-code-tests" };
        branch = new GymBranch { Name = "Client code branch", City = new City { Name = "Client code city" } };
        db.AddRange(user, branch); await db.SaveChangesAsync();
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private Member Member(long code) => new() { ClientCode = code, MemberCode = Guid.NewGuid().ToString("N"),
        FullName = "Synthetic member", PrimaryPhone = Guid.NewGuid().ToString("N"), HomeBranchId = branch.Id, CreatedByUserId = user.Id };

    [Theory] [InlineData(10542L)] [InlineData(long.MaxValue)]
    public async Task NumericLegacyCodeIsPreservedExactlyWithGuidRelationships(long code)
    {
        var member = Member(code);
        var sale = new SaleTransaction { Member = member, BranchId = branch.Id, CreatedByUserId = user.Id,
            SaleType = SaleType.GymMembership, GrossAmount = 500, NetAmount = 500 };
        db.Add(sale); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        var stored = await db.SaleTransactions.AsNoTracking().Include(s => s.Member).SingleAsync();
        Assert.Equal(member.Id, stored.MemberId); Assert.Equal(code, stored.Member.ClientCode);
        Assert.Equal(member.MemberCode, stored.Member.MemberCode); Assert.Equal(branch.Id, stored.Member.HomeBranchId);
    }
    [Fact] public async Task DuplicateClientCodeIsRejected()
    {
        db.Add(Member(10542)); await db.SaveChangesAsync(); db.Add(Member(10542));
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
    }
    [Theory] [InlineData(0L)] [InlineData(-1L)]
    public async Task NonpositiveClientCodeIsRejected(long code)
    {
        db.Add(Member(code)); var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal("CK_Members_ClientCode_Positive", Assert.IsType<PostgresException>(error.InnerException).ConstraintName);
    }
    [Fact] public async Task NullIsRejectedBecauseDevelopmentMembersWereEmpty()
    {
        var member = Member(10542); db.Add(member); await db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"Members\" SET \"ClientCode\" = NULL WHERE \"Id\" = {member.Id}"));
        Assert.Equal(PostgresErrorCodes.NotNullViolation, error.SqlState);
    }
    [Fact] public void GuidRemainsPrimaryAndAllRelationshipKeysRemainUnchanged()
    {
        var member = db.Model.FindEntityType(typeof(Member))!;
        Assert.Equal("Id", Assert.Single(member.FindPrimaryKey()!.Properties).Name);
        Assert.Equal(typeof(Guid), member.FindPrimaryKey()!.Properties[0].ClrType);
        var code = member.FindProperty("ClientCode")!;
        Assert.Equal(typeof(long), code.ClrType); Assert.False(code.IsNullable); Assert.Equal("bigint", code.GetColumnType());
        Assert.True(member.GetIndexes().Single(i => i.Properties.Count == 1 && i.Properties[0] == code).IsUnique);
        var references = db.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()).Where(fk => fk.PrincipalEntityType == member).ToList();
        Assert.NotEmpty(references);
        Assert.All(references, fk => Assert.Equal("Id", Assert.Single(fk.PrincipalKey.Properties).Name));
        Assert.All(references, fk => Assert.Equal(typeof(Guid), Nullable.GetUnderlyingType(Assert.Single(fk.Properties).ClrType) ?? Assert.Single(fk.Properties).ClrType));
    }
    [Fact] public void MigrationAddsOnlyColumnUniqueIndexAndPositiveCheckWithoutGeneratedDefault()
    {
        var migrations = db.GetService<IMigrationsAssembly>();
        var migration = migrations.CreateMigration(migrations.Migrations.Single(m => m.Key.EndsWith("_AddMemberClientCode")).Value, db.Database.ProviderName!);
        Assert.Equal(3, migration.UpOperations.Count);
        var column = Assert.Single(migration.UpOperations.OfType<AddColumnOperation>());
        Assert.Equal("Members", column.Table); Assert.Equal("ClientCode", column.Name);
        Assert.False(column.IsNullable); Assert.Null(column.DefaultValue); Assert.Null(column.DefaultValueSql);
        var index = Assert.Single(migration.UpOperations.OfType<CreateIndexOperation>());
        Assert.Equal("Members", index.Table); Assert.True(index.IsUnique);
        Assert.Single(migration.UpOperations.OfType<AddCheckConstraintOperation>());
    }
}
