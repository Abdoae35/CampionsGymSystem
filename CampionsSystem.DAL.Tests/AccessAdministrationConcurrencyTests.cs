using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

public sealed class AccessAdministrationConcurrencyTests : IAsyncLifetime
{
    private readonly string schema = "gym_access_test_" + Guid.NewGuid().ToString("N");
    private string? connection;
    private Guid actor, target, role, permission;
    private ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection!).Options);
    private sealed class Identity(Guid id) : ICurrentUserIdentity
    { public bool IsAuthenticated => true; public Guid? UserId => id; }
    private AccessAdministrationService Service(ApplicationDbContext db)
    {
        var permissions = new PermissionService(db);
        var contexts = new UserContextService(db, permissions, TimeProvider.System, Options.Create(new AuthContextOptions()), new EmployeeBranchResolver(db));
        var current = new CurrentUserService(new Identity(actor), contexts);
        return new(db, new(db, current), permissions, contexts, new(current, db, TimeProvider.System, Options.Create(new AuthContextOptions())));
    }
    public async Task InitializeAsync()
    {
        var config = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("GYM_SCHEMA_TEST_CONNECTION")
            ?? $"Host=localhost;Database=postgres;Username={Environment.UserName};Timeout=5");
        if (config.Host is not ("localhost" or "127.0.0.1" or "::1")) throw new InvalidOperationException("Access tests require localhost.");
        await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE SCHEMA {schema}", admin); await command.ExecuteNonQueryAsync();
        config.Pooling = false; config.SearchPath = schema; config.ApplicationName = schema; connection = config.ConnectionString;
        try
        {
            await using var db = Context(); await db.Database.ExecuteSqlRawAsync(db.GetService<IMigrator>().GenerateScript());
            var owner = new ApplicationUser { UserName = "owner", NormalizedUserName = "OWNER" }; actor = owner.Id;
            var user = new ApplicationUser { UserName = "target", NormalizedUserName = "TARGET" }; target = user.Id;
            var ownerRole = new IdentityRole<Guid>("Owner") { Id = Guid.NewGuid(), NormalizedName = "OWNER" };
            var salesRole = new IdentityRole<Guid>("Sales") { Id = Guid.NewGuid(), NormalizedName = "SALES" }; role = salesRole.Id;
            db.AddRange(owner, user, ownerRole, salesRole, new IdentityUserRole<Guid> { UserId = actor, RoleId = ownerRole.Id });
            foreach (var code in OwnerAdministrationAccess.RecoveryPermissions)
            {
                var p = new Permission { Code = code, Name = code, Module = code.Split('.')[0] };
                db.Add(new RolePermission { RoleId = ownerRole.Id, Permission = p });
            }
            var member = new Permission { Code = "Members.View", Name = "Members.View", Module = "Members" }; permission = member.Id;
            db.Add(new RolePermission { RoleId = role, Permission = member }); await db.SaveChangesAsync();
        }
        catch { await DisposeAsync(); throw; }
    }
    public async Task DisposeAsync()
    {
        if (connection is null) return;
        await using var cn = new NpgsqlConnection(connection); await cn.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS {schema} CASCADE", cn); await command.ExecuteNonQueryAsync();
    }
    private async Task<RoleMembershipRequest> MembershipRequest()
    {
        await using var db = Context();
        return new() { OperationId = Guid.NewGuid(), ExpectedVersion = (await Service(db).UserAsync(target)).Version, Reason = "Approved membership", Assigned = true };
    }
    private async Task<AccessChangeResult> Assign(RoleMembershipRequest request)
    { await using var db = Context(); return await Service(db).MembershipAsync(target, role, request); }
    [Fact] public async Task ConcurrentIdenticalAccessRetriesHaveOneMembershipAndAudit()
    {
        var request = await MembershipRequest(); var results = await Task.WhenAll(Assign(request), Assign(request));
        Assert.Equal(results[0], results[1]);
        await using var db = Context(); Assert.Single(await db.UserRoles.Where(x => x.UserId == target).ToListAsync());
        Assert.Single(await db.AuditLogs.Where(x => x.Action == AccessAdministrationService.AuditAction).ToListAsync());
    }
    [Fact] public async Task ConcurrentStaleAccessWritesCannotClobberEachOther()
    {
        var request = await MembershipRequest();
        var second = new RoleMembershipRequest { OperationId = Guid.NewGuid(), ExpectedVersion = request.ExpectedVersion, Reason = "Another assignment", Assigned = true };
        async Task<bool> Try(RoleMembershipRequest r)
        { try { await Assign(r); return true; } catch (AccessAdministrationException e) when (e.Status == 409) { return false; } }
        Assert.Single(await Task.WhenAll(Try(request), Try(second)), x => x);
        await using var db = Context(); Assert.Single(await db.AuditLogs.ToListAsync());
    }
    [Fact] public async Task ConcurrentConflictingOverrideWritesPreserveOneAuditedDecision()
    {
        var request = await MembershipRequest(); await Assign(request);
        await using var read = Context(); var version = (await Service(read).UserAsync(target)).Version;
        async Task<bool> Write(string effect)
        {
            await using var db = Context();
            try { await Service(db).OverrideAsync(target, permission, new() { OperationId = Guid.NewGuid(), ExpectedVersion = version, Reason = "Concurrent decision", Effect = effect }); return true; }
            catch (AccessAdministrationException e) when (e.Status == 409) { return false; }
        }
        Assert.Single(await Task.WhenAll(Write("Allow"), Write("Deny")), x => x);
        await using var check = Context(); Assert.Single(await check.UserPermissionOverrides.ToListAsync());
        Assert.Equal(2, await check.AuditLogs.CountAsync());
    }
    [Fact] public async Task AccessOperationUniquenessIsEnforcedWithoutChangingRoleGrants()
    {
        await using var db = Context(); var grants = await db.RolePermissions.CountAsync();
        var op = Guid.NewGuid();
        db.AuditLogs.Add(new() { UserId = actor, Action = AccessAdministrationService.AuditAction, EntityType = "AccessAdministration", EntityId = op });
        await db.SaveChangesAsync();
        db.AuditLogs.Add(new() { UserId = actor, Action = AccessAdministrationService.AuditAction, EntityType = "AccessAdministration", EntityId = op });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(grants, await db.RolePermissions.CountAsync());
    }
    [Fact] public async Task RevokedOwnerCannotRetryPreviousSuccessfulWriteThroughBll()
    {
        var request = await MembershipRequest(); await Assign(request);
        await using (var db = Context())
        {
            var manage = await db.Permissions.SingleAsync(x => x.Code == "Users.Manage");
            db.UserPermissionOverrides.Add(new() { UserId = actor, PermissionId = manage.Id, Effect = PermissionOverrideEffect.Deny });
            await db.SaveChangesAsync();
        }
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Assign(request));
        await using var check = Context(); Assert.Single(await check.AuditLogs.ToListAsync());
    }
    [Fact] public async Task OwnerAuthorizationIsRecheckedAfterWaitingForMutationLock()
    {
        var request = await MembershipRequest();
        await using var holder = Context(); await using var tx = await holder.Database.BeginTransactionAsync();
        await holder.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(173528701, 1)");
        var waiting = Assign(request);
        try
        {
            var until = DateTime.UtcNow.AddSeconds(10); var blocked = false;
            await using var observer = new NpgsqlConnection(connection); await observer.OpenAsync();
            while (DateTime.UtcNow < until)
            {
                await using var query = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM pg_locks l JOIN pg_stat_activity a ON a.pid=l.pid WHERE a.application_name=@name AND l.locktype='advisory' AND NOT l.granted)", observer);
                query.Parameters.AddWithValue("name", schema);
                if ((bool)(await query.ExecuteScalarAsync())!) { blocked = true; break; }
                await Task.Delay(25);
            }
            Assert.True(blocked, "The mutation must be waiting before revocation commits.");
            var manage = await holder.Permissions.SingleAsync(x => x.Code == "Users.Manage");
            holder.UserPermissionOverrides.Add(new() { UserId = actor, PermissionId = manage.Id, Effect = PermissionOverrideEffect.Deny });
            await holder.SaveChangesAsync(); await tx.CommitAsync();
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => waiting);
            await using var check = Context(); Assert.Empty(await check.UserRoles.Where(x => x.UserId == target).ToListAsync());
            Assert.Empty(await check.AuditLogs.ToListAsync());
        }
        finally { if (!waiting.IsCompleted) { await tx.RollbackAsync(); try { await waiting; } catch (UnauthorizedAccessException) { } } }
    }
    [Fact] public async Task NonCurrentAmbientSnapshotCannotAuthorizeAccessWrites()
    {
        var request = await MembershipRequest(); await using var db = Context();
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Service(db).MembershipAsync(target, role, request));
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }
}
