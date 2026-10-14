using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Employees;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

public sealed partial class AccessAdministrationConcurrencyTests
{
    private async Task<(Guid First, Guid Second)> OwnerEmployees()
    {
        await using var db = Context();
        var ownerRole = await db.Roles.SingleAsync(x => x.NormalizedName == "OWNER");
        db.Add(new IdentityUserRole<Guid> { UserId = target, RoleId = ownerRole.Id });
        foreach (var code in new[] { "Employees.Manage", "Branches.AccessAll" })
            db.Add(new RolePermission { RoleId = ownerRole.Id, Permission = new Permission { Code = code, Name = code, Module = code.Split('.')[0] } });
        var branch = new GymBranch { Name = "Owner lifecycle", City = new City { Name = "Owner city" } };
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo")).DateTime);
        var first = new Employee { UserId = actor, FullName = "Owner A", Phone = "owner-a", HireDate = today.AddYears(-1) };
        var second = new Employee { UserId = target, FullName = "Owner B", Phone = "owner-b", HireDate = first.HireDate };
        db.AddRange(new EmployeeBranchAssignment { Employee = first, Branch = branch, StartDate = first.HireDate, AssignedByUserId = actor },
            new EmployeeBranchAssignment { Employee = second, Branch = branch, StartDate = second.HireDate, AssignedByUserId = actor });
        await db.SaveChangesAsync(); return (first.Id, second.Id);
    }

    private EmployeeManagementService OwnerEmployeesService(ApplicationDbContext db, Guid user)
    {
        var contexts = new UserContextService(db, new PermissionService(db), TimeProvider.System, Options.Create(new AuthContextOptions()), new(db));
        var current = new CurrentUserService(new Identity(user), contexts);
        var access = new OperationAccess(current, db, TimeProvider.System, Options.Create(new AuthContextOptions()));
        return new(db, current, access, new(db), new(db, current, access), new(db, current, access), new(db, new(db, current), contexts));
    }

    private async Task<bool> SuspendOwner(Guid user, Guid employee)
    {
        await using var db = Context();
        try { await OwnerEmployeesService(db, user).ChangeStatusAsync(employee, new() { Status = EmploymentStatus.Suspended, Reason = "Owner investigation" }); return true; }
        catch (UnauthorizedAccessException) { return false; }
        catch (AccessAdministrationException e) when (e.Status == 409) { return false; }
    }

    private async Task AssertOneUsableOwner()
    {
        await using var db = Context();
        var contexts = new UserContextService(db, new PermissionService(db), TimeProvider.System, Options.Create(new AuthContextOptions()), new(db));
        var usable = 0;
        foreach (var id in new[] { actor, target })
        {
            var context = await contexts.ResolveAsync(id);
            if (context != null && OwnerAdministrationAccess.RecoveryPermissions.All(context.Permissions.Contains)) usable++;
        }
        Assert.Equal(1, usable);
    }

    [Fact]
    public async Task OwnerLifecycleConcurrentOwnersCannotDisableEachOtherAndLoseLastOwner()
    {
        var employees = await OwnerEmployees();
        var results = await Task.WhenAll(SuspendOwner(actor, employees.Second), SuspendOwner(target, employees.First));
        Assert.Single(results, x => x); await AssertOneUsableOwner();
        await using var db = Context(); Assert.Single(await db.AuditLogs.Where(x => x.Action == "EmployeeStatusChanged").ToListAsync());
    }

    [Fact]
    public async Task OwnerLifecycleAndRecoveryDenyShareLastOwnerCoordination()
    {
        var employees = await OwnerEmployees();
        var permission = Guid.Empty; PermissionOverrideRequest request;
        await using (var db = Context())
        {
            permission = await db.Permissions.Where(x => x.Code == "Roles.Manage").Select(x => x.Id).SingleAsync();
            request = new() { OperationId = Guid.NewGuid(), ExpectedVersion = (await Service(db).UserAsync(actor)).Version, Effect = "Deny", Reason = "Authorized access review" };
        }
        async Task<bool> DenyRecovery()
        {
            await using var db = Context();
            var contexts = new UserContextService(db, new PermissionService(db), TimeProvider.System, Options.Create(new AuthContextOptions()), new(db));
            var current = new CurrentUserService(new Identity(target), contexts); var gate = new OwnerAdministrationAccess(db, current);
            var service = new AccessAdministrationService(db, gate, new PermissionService(db), contexts,
                new(current, db, TimeProvider.System, Options.Create(new AuthContextOptions())), new(db, gate, contexts));
            try { await service.OverrideAsync(actor, permission, request); return true; }
            catch (UnauthorizedAccessException) { return false; }
            catch (AccessAdministrationException e) when (e.Status == 409) { return false; }
        }
        var results = await Task.WhenAll(SuspendOwner(actor, employees.Second), DenyRecovery());
        Assert.Single(results, x => x); await AssertOneUsableOwner();
    }

    [Fact]
    public async Task OwnerLifecycleRevalidatesActorAfterWaitingForAdministrationLock()
    {
        var employees = await OwnerEmployees();
        await using var holder = Context(); await using var tx = await holder.Database.BeginTransactionAsync();
        await holder.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(173528701, 1)");
        var waiting = SuspendOwner(actor, employees.Second);
        try
        {
            await using var observer = new NpgsqlConnection(connection); await observer.OpenAsync();
            var deadline = DateTime.UtcNow.AddSeconds(10); var blocked = false;
            while (DateTime.UtcNow < deadline)
            {
                await using var command = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM pg_locks l JOIN pg_stat_activity a ON a.pid=l.pid WHERE a.application_name=@name AND l.locktype='advisory' AND NOT l.granted)", observer);
                command.Parameters.AddWithValue("name", schema);
                if ((bool)(await command.ExecuteScalarAsync())!) { blocked = true; break; }
                await Task.Delay(25);
            }
            Assert.True(blocked);
            var membership = await holder.UserRoles.SingleAsync(x => x.UserId == actor);
            holder.Remove(membership); await holder.SaveChangesAsync(); await tx.CommitAsync();
            Assert.False(await waiting);
            await using var db = Context(); Assert.All(await db.Employees.ToListAsync(), x => Assert.Equal(EmploymentStatus.Active, x.EmploymentStatus));
            Assert.Empty(await db.AuditLogs.ToListAsync());
        }
        finally { if (!waiting.IsCompleted) { await tx.RollbackAsync(); await waiting; } }
    }

    [Fact]
    public async Task OwnerLifecycleAccountLocksAllowConcurrentUserEvidenceToCommit()
    {
        var employees = await OwnerEmployees();
        await using var holder = Context(); await using var tx = await holder.Database.BeginTransactionAsync();
        // CRM holds a shared commercial-owner lock before writing performer evidence.
        await holder.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\"={employees.Second} FOR SHARE")
            .AsNoTracking().ToListAsync();
        var waiting = SuspendOwner(actor, employees.Second);
        try
        {
            await using var observer = new NpgsqlConnection(connection); await observer.OpenAsync();
            var deadline = DateTime.UtcNow.AddSeconds(20); var blocked = false;
            while (DateTime.UtcNow < deadline)
            {
                await using var command = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM pg_stat_activity WHERE application_name=@name AND wait_event_type='Lock' AND query LIKE '%\"Employees\"%' AND query LIKE '%FOR UPDATE%')", observer);
                command.Parameters.AddWithValue("name", schema);
                if ((bool)(await command.ExecuteScalarAsync())!) { blocked = true; break; }
                await Task.Delay(25);
            }
            Assert.True(blocked, "Lifecycle must reach the employee lock while holding account protection.");
            holder.AuditLogs.Add(new() { UserId = target, Action = "ConcurrentEvidence", EntityType = nameof(Employee), EntityId = employees.Second });
            await holder.SaveChangesAsync(); await tx.CommitAsync();
            Assert.True(await waiting); await AssertOneUsableOwner();
            await using var check = Context(); Assert.Equal(2, await check.AuditLogs.CountAsync());
        }
        finally { if (!waiting.IsCompleted) { await tx.RollbackAsync(); await waiting; } }
    }

    [Fact]
    public async Task OwnerLifecycleRejectsStaleAmbientSnapshotBeforeMutation()
    {
        var employees = await OwnerEmployees(); await using var db = Context();
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        await Assert.ThrowsAsync<BusinessRuleException>(() => OwnerEmployeesService(db, actor).ChangeStatusAsync(employees.Second, new() { Status = EmploymentStatus.Suspended }));
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }
}
