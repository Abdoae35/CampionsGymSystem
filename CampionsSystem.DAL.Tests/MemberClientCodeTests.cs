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
    private Member Member(long? code) => new() { ClientCode = code, MemberCode = Guid.NewGuid().ToString("N"),
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
    [Fact] public async Task DuplicateClientCodeInSameBranchIsRejected()
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
    [Fact] public async Task MultipleUncodedMembersRemainNullWithoutInventedValues()
    {
        db.AddRange(Member(null), Member(null)); await db.SaveChangesAsync();
        Assert.Equal(2, await db.Members.CountAsync(m => m.ClientCode == null));
    }
    [Fact] public async Task DifferentBranchesCanHaveSameClientCode()
    {
        var otherBranch = new GymBranch { Name = "Other branch", CityId = branch.CityId };
        db.Add(otherBranch); await db.SaveChangesAsync();
        var first = Member(1500); var second = Member(1500); second.HomeBranchId = otherBranch.Id;
        db.AddRange(first, second); await db.SaveChangesAsync();
        Assert.NotEqual(first.Id, second.Id); Assert.Equal(2, await db.Members.CountAsync(m => m.ClientCode == 1500));
    }
    [Fact] public void GuidRemainsPrimaryAndAllRelationshipKeysRemainUnchanged()
    {
        var member = db.Model.FindEntityType(typeof(Member))!;
        Assert.Equal("Id", Assert.Single(member.FindPrimaryKey()!.Properties).Name);
        Assert.Equal(typeof(Guid), member.FindPrimaryKey()!.Properties[0].ClrType);
        var code = member.FindProperty("ClientCode")!;
        Assert.Equal(typeof(long?), code.ClrType); Assert.True(code.IsNullable); Assert.Equal("bigint", code.GetColumnType());
        var index = member.GetIndexes().Single(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { "HomeBranchId", "ClientCode" }));
        Assert.True(index.IsUnique); Assert.Equal("\"ClientCode\" IS NOT NULL", index.GetFilter());
        Assert.DoesNotContain(member.GetIndexes(), i => i.Properties.Count == 1 && i.Properties[0] == code);
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
    [Fact] public void ScopingMigrationOnlyRelaxesNullabilityAndReplacesGlobalIndex()
    {
        var migrations = db.GetService<IMigrationsAssembly>();
        var migration = migrations.CreateMigration(migrations.Migrations.Single(m => m.Key.EndsWith("_ScopeMemberClientCodeByHomeBranch")).Value, db.Database.ProviderName!);
        Assert.Equal(3, migration.UpOperations.Count);
        Assert.Equal("IX_Members_ClientCode", Assert.Single(migration.UpOperations.OfType<DropIndexOperation>()).Name);
        var column = Assert.Single(migration.UpOperations.OfType<AlterColumnOperation>());
        Assert.Equal("Members", column.Table); Assert.True(column.IsNullable); Assert.Null(column.DefaultValue);
        var index = Assert.Single(migration.UpOperations.OfType<CreateIndexOperation>());
        Assert.True(index.IsUnique); Assert.Equal(new[] { "HomeBranchId", "ClientCode" }, index.Columns);
        Assert.Equal("\"ClientCode\" IS NOT NULL", index.Filter);
        Assert.Null(Assert.Single(migration.DownOperations.OfType<AlterColumnOperation>()).DefaultValue);
    }
    [Fact] public async Task UpgradePreservesExistingMemberIdentityCodeAndRelationships()
    {
        await using var legacy = await AuthTestDatabase.CreateAsync("AddMemberClientCode");
        await using var old = legacy.CreateContext();
        var creator = new ApplicationUser { UserName = "legacy-scoping" };
        var home = new GymBranch { Name = "Legacy home", City = new City { Name = "Legacy city" } };
        var member = new Member { ClientCode = 1500, MemberCode = "LEGACY", FullName = "Legacy", PrimaryPhone = "legacy", HomeBranch = home, CreatedByUser = creator };
        old.Add(creator); old.Add(home.City); await old.SaveChangesAsync();
        await old.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "GymBranches" ("Id", "CityId", "Name", "MaxBorrowedGymSessions", "QuickRefundWindowHours", "IsActive", "CreatedAt")
            VALUES ({home.Id}, {home.City.Id}, {home.Name}, 2, 24, true, {home.CreatedAt});
            INSERT INTO "Members" ("Id", "ClientCode", "MemberCode", "FullName", "PrimaryPhone", "HomeBranchId", "IsPrimaryPhoneVerified", "IsActive", "CreatedByUserId", "CreatedAt")
            VALUES ({member.Id}, {member.ClientCode}, {member.MemberCode}, {member.FullName}, {member.PrimaryPhone}, {home.Id}, false, true, {creator.Id}, {member.CreatedAt});
            """);
        var sql = old.GetService<IMigrator>().GenerateScript("AddMemberClientCode", "ScopeMemberClientCodeByHomeBranch")
            .Replace("START TRANSACTION;", "").Replace("COMMIT;", "");
        await old.Database.ExecuteSqlRawAsync(sql); old.ChangeTracker.Clear();
        var stored = await old.Members.AsNoTracking().Select(m => new { m.Id, m.ClientCode, m.HomeBranchId, m.MemberCode, m.CreatedByUserId }).SingleAsync();
        Assert.Equal(member.Id, stored.Id); Assert.Equal(1500L, stored.ClientCode); Assert.Equal(home.Id, stored.HomeBranchId);
        Assert.Equal("LEGACY", stored.MemberCode); Assert.Equal(creator.Id, stored.CreatedByUserId);
    }

}
