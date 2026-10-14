using System.Text.Json;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CampionsSystem.BLL.Authentication;

public sealed class AccessAdministrationService(ApplicationDbContext db, OwnerAdministrationAccess owner,
    IPermissionService permissions, UserContextService contexts, OperationAccess access, OwnerAccountProtection protection)
{
    public const string AuditAction = "AccessAdministration.Changed";
    private sealed record ChangeEvidence(string Hash, string Kind, Guid? TargetUserId, Guid? RoleId,
        Guid? PermissionId, string Reason, string OldValues, string NewValues, AccessChangeResult Result);
    private static AccessAdministrationException Missing(string resource) => new(404, $"{resource} not found.");
    private static AccessAdministrationException Conflict(string message) => new(409, message);
    private static PermissionCatalogItem Catalog(Permission p) => new(p.Id, p.Code, p.Name, p.Description, p.IsActive);
    private static AccessRole Role(IdentityRole<Guid> r) => new(r.Id, r.Name ?? "", r.NormalizedName == "OWNER");

    public async Task<IReadOnlyList<PermissionModule>> CatalogAsync(CancellationToken ct = default)
    {
        await owner.RequireAsync("Roles.View", ct);
        var rows = await db.Permissions.AsNoTracking().OrderBy(x => x.Module).ThenBy(x => x.Code).ToListAsync(ct);
        return rows.GroupBy(x => x.Module).Select(g => new PermissionModule(g.Key, g.Select(Catalog).ToArray())).ToArray();
    }
    public async Task<IReadOnlyList<AccessRole>> RolesAsync(CancellationToken ct = default)
    {
        await owner.RequireAsync("Roles.View", ct);
        return (await db.Roles.AsNoTracking().OrderBy(x => x.Name).ThenBy(x => x.Id).ToListAsync(ct)).Select(Role).ToArray();
    }
    public async Task<RoleAccessDetails> RoleAsync(Guid id, CancellationToken ct = default)
    {
        return await ReadSnapshotAsync(async () => { await owner.RequireAsync("Roles.View", ct); return await RoleSnapshotAsync(id, ct); }, ct);
    }
    private async Task<T> ReadSnapshotAsync<T>(Func<Task<T>> read, CancellationToken ct)
    {
        var tx = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct) : null;
        try { var result = await read(); if (tx is not null) await tx.CommitAsync(ct); return result; }
        finally { if (tx is not null) await tx.DisposeAsync(); }
    }
    private async Task<RoleAccessDetails> RoleSnapshotAsync(Guid id, CancellationToken ct)
    {
        var row = await db.Roles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw Missing("Role");
        var grants = await (from grant in db.RolePermissions.AsNoTracking()
                           join p in db.Permissions.AsNoTracking() on grant.PermissionId equals p.Id
                           where grant.RoleId == id orderby p.Code select p).ToListAsync(ct);
        var items = grants.Select(Catalog).ToArray();
        return new(Role(row), FinancialOperations.Hash(new { Role = Role(row), Grants = items }), items);
    }
    private static int Offset(int page, int size)
    {
        if (page < 1 || size is < 1 or > 100 || (long)(page - 1) * size > int.MaxValue)
            throw new BusinessRuleException("Invalid pagination; page size must be 1–100.");
        return (page - 1) * size;
    }
    public async Task<AccessPage<AccessUserItem>> UsersAsync(AccessInventoryQuery q, CancellationToken ct = default)
    {
        await owner.RequireAsync("Users.View", ct); var offset = Offset(q.Page, q.PageSize);
        if (q.Search?.Length > 256) throw new BusinessRuleException("Search is too long.");
        var rows = db.Users.AsNoTracking().AsQueryable();
        if (q.IsActive.HasValue) rows = rows.Where(x => x.IsActive == q.IsActive.Value);
        if (q.RoleId.HasValue) rows = rows.Where(x => db.UserRoles.Any(r => r.UserId == x.Id && r.RoleId == q.RoleId));
        if (q.BranchId.HasValue) rows = rows.Where(x => db.Employees.Any(e => e.UserId == x.Id &&
            db.EmployeeBranchAssignments.Any(a => a.EmployeeId == e.Id && a.BranchId == q.BranchId
                && a.StartDate <= access.Today && (a.EndDate == null || a.EndDate >= access.Today))));
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.Trim();
            rows = rows.Where(x => x.UserName != null && x.UserName.Contains(search) ||
                db.Employees.Any(e => e.UserId == x.Id && (e.FullName.Contains(search) || e.Phone.Contains(search))));
        }
        var count = await rows.CountAsync(ct);
        var items = await rows.OrderBy(x => x.UserName).ThenBy(x => x.Id).Skip(offset).Take(q.PageSize)
            .Select(x => new AccessUserItem(x.Id, x.UserName, x.IsActive,
                db.Employees.Where(e => e.UserId == x.Id).Select(e => (Guid?)e.Id).FirstOrDefault(),
                db.Employees.Where(e => e.UserId == x.Id).Select(e => e.FullName).FirstOrDefault())).ToListAsync(ct);
        return new(items, count, q.Page, q.PageSize);
    }
    public async Task<UserAccessDetails> UserAsync(Guid id, CancellationToken ct = default)
    {
        return await ReadSnapshotAsync(async () => { await owner.RequireAsync("Users.View", ct); return await UserSnapshotAsync(id, ct); }, ct);
    }
    private async Task<UserAccessDetails> UserSnapshotAsync(Guid id, CancellationToken ct)
    {
        var row = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw Missing("User");
        var employee = await db.Employees.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == id, ct);
        var roles = (await (from membership in db.UserRoles.AsNoTracking()
                           join r in db.Roles.AsNoTracking() on membership.RoleId equals r.Id
                           where membership.UserId == id orderby r.Name, r.Id select r).ToListAsync(ct)).Select(Role).ToArray();
        var assignments = employee is null ? [] : await db.EmployeeBranchAssignments.AsNoTracking()
            .Where(x => x.EmployeeId == employee.Id).OrderBy(x => x.StartDate).ThenBy(x => x.Id)
            .Select(x => new AccessBranchAssignment(x.Id, x.BranchId, x.Branch.Name, x.Branch.IsActive,
                x.StartDate, x.EndDate, x.IsPrimary)).ToArrayAsync(ct);
        var inherited = await (from m in db.UserRoles.AsNoTracking()
                               join g in db.RolePermissions.AsNoTracking() on m.RoleId equals g.RoleId
                               where m.UserId == id select new { g.PermissionId, g.RoleId }).ToListAsync(ct);
        var overrides = await db.UserPermissionOverrides.AsNoTracking().Where(x => x.UserId == id).ToDictionaryAsync(x => x.PermissionId, ct);
        // The protected-endpoint evaluator is authoritative. These rows only explain its result.
        var effective = await permissions.GetUserPermissionsAsync(id, ct);
        var catalog = await db.Permissions.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);
        var explanations = catalog.Select(p => new PermissionExplanation(p.Id, p.Code, p.Name, p.Module, p.IsActive,
            roles.Where(r => inherited.Any(g => g.PermissionId == p.Id && g.RoleId == r.Id)).ToArray(),
            overrides.TryGetValue(p.Id, out var o) ? o.Effect.ToString() : null, effective.Contains(p.Code))).ToArray();
        var context = await contexts.ResolveAsync(id, ct);
        var employment = employee is null ? null : new AccessEmployee(employee.Id, employee.FullName,
            employee.EmploymentStatus.ToString(), employee.HireDate, employee.TerminationDate);
        var locked = row.LockoutEnabled && row.LockoutEnd > access.Now;
        var version = FinancialOperations.Hash(new { row.Id, row.UserName, row.IsActive, row.LockoutEnabled,
            row.LockoutEnd, Employee = employment, Roles = roles, Assignments = assignments, Permissions = explanations });
        return new(id, row.UserName, row.IsActive, locked, context is not null, employment, context?.CurrentBranch,
            context?.Permissions.Contains("Branches.AccessAll") == true, assignments, roles, version, explanations);
    }

    public Task<AccessChangeResult> MembershipAsync(Guid user, Guid role, RoleMembershipRequest r, CancellationToken ct = default)
        => ChangeAsync("Membership", user, role, null, r, r.Assigned, null, ct);
    public Task<AccessChangeResult> GrantAsync(Guid role, Guid permission, RoleGrantRequest r, CancellationToken ct = default)
        => ChangeAsync("RoleGrant", null, role, permission, r, r.Granted, null, ct);
    public Task<AccessChangeResult> OverrideAsync(Guid user, Guid permission, PermissionOverrideRequest r, CancellationToken ct = default)
        => ChangeAsync("Override", user, null, permission, r, null, r.Effect, ct);

    private async Task<AccessChangeResult> ChangeAsync(string kind, Guid? userId, Guid? roleId, Guid? permissionId,
        AccessChangeRequest r, bool? desired, string? effect, CancellationToken ct)
    {
        var capability = kind == "RoleGrant" ? "Roles.Manage" : "Users.Manage";
        await owner.RequireAsync(capability, ct);
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Reason(r.Reason);
        if (string.IsNullOrEmpty(r.ExpectedVersion) || r.ExpectedVersion.Length != 64 || !r.ExpectedVersion.All(Uri.IsHexDigit)) throw new BusinessRuleException("A current resource version is required.");
        if (kind != "Override" && desired is null || effect is not (null or "Allow" or "Deny"))
            throw new BusinessRuleException("Invalid desired access state.");
        var hash = FinancialOperations.Hash(new { kind, userId, roleId, permissionId, r.OperationId, r.ExpectedVersion, r.Reason, desired, effect });
        if (db.Database.CurrentTransaction is not null && db.Database.CurrentTransaction.GetDbTransaction().IsolationLevel != System.Data.IsolationLevel.ReadCommitted)
            throw new BusinessRuleException("Access administration requires ReadCommitted coordination to revalidate current access after locking.");
        var owned = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        var tx = db.Database.CurrentTransaction!; var point = "access_" + Guid.NewGuid().ToString("N");
        try
        {
            await tx.CreateSavepointAsync(point, ct);
            // Separate from financial locks. Serializes all access mutations and last-Owner decisions.
            await protection.LockAsync(userId, ct);
            var actor = await owner.RequireAsync(capability, ct);
            var existing = await db.AuditLogs.AsNoTracking().SingleOrDefaultAsync(x => x.Action == AuditAction && x.EntityId == r.OperationId, ct);
            if (existing is not null)
            {
                var prior = JsonSerializer.Deserialize<ChangeEvidence>(existing.NewValues!)!;
                if (prior.Hash != hash || existing.UserId != actor) throw Conflict("Operation identity belongs to a different request or actor.");
                await tx.ReleaseSavepointAsync(point, ct); if (owned is not null) await owned.CommitAsync(ct);
                return prior.Result;
            }
            var before = userId.HasValue ? JsonSerializer.Serialize(await UserSnapshotAsync(userId.Value, ct))
                : JsonSerializer.Serialize(await RoleSnapshotAsync(roleId!.Value, ct));
            var version = userId.HasValue ? JsonSerializer.Deserialize<UserAccessDetails>(before)!.Version
                : JsonSerializer.Deserialize<RoleAccessDetails>(before)!.Version;
            if (version != r.ExpectedVersion) throw Conflict("Access changed; reload the resource and retry with its current version.");
            var changed = false;
            if (kind == "Membership")
            {
                var role = await db.Roles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == roleId, ct) ?? throw Missing("Role");
                if (role.NormalizedName == "OWNER" && desired == true)
                    throw new BusinessRuleException("Owner provisioning is outside access administration V1.");
                if (role.NormalizedName == "OWNER" && userId == actor)
                    throw Conflict("The administering Owner cannot remove their own Owner membership.");
                var membership = await db.UserRoles.SingleOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
                if (desired == true && membership is null) { db.UserRoles.Add(new() { UserId = userId!.Value, RoleId = roleId!.Value }); changed = true; }
                if (desired == false && membership is not null) { db.UserRoles.Remove(membership); changed = true; }
            }
            else
            {
                var permission = await db.Permissions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == permissionId, ct) ?? throw Missing("Permission");
                if (!permission.IsActive && (desired == true || effect == "Allow")) throw new BusinessRuleException("An inactive permission cannot be granted or allowed.");
                if (kind == "RoleGrant")
                {
                    var grant = await db.RolePermissions.SingleOrDefaultAsync(x => x.RoleId == roleId && x.PermissionId == permissionId, ct);
                    if (desired == true && grant is null) { db.RolePermissions.Add(new() { RoleId = roleId!.Value, PermissionId = permissionId!.Value }); changed = true; }
                    if (desired == false && grant is not null) { db.RolePermissions.Remove(grant); changed = true; }
                }
                else
                {
                    var item = await db.UserPermissionOverrides.SingleOrDefaultAsync(x => x.UserId == userId && x.PermissionId == permissionId, ct);
                    if (effect is null && item is not null) { db.UserPermissionOverrides.Remove(item); changed = true; }
                    else if (effect is not null)
                    {
                        var value = effect == "Allow" ? PermissionOverrideEffect.Allow : PermissionOverrideEffect.Deny;
                        if (item is null) { db.UserPermissionOverrides.Add(new() { UserId = userId!.Value, PermissionId = permissionId!.Value, Effect = value, CreatedByUserId = actor, CreatedAt = access.Now }); changed = true; }
                        else if (item.Effect != value) { item.Effect = value; item.CreatedByUserId = actor; item.CreatedAt = access.Now; changed = true; }
                    }
                }
            }
            await db.SaveChangesAsync(ct);
            await protection.ProtectOwnersAsync(actor, ct);
            var after = userId.HasValue ? JsonSerializer.Serialize(await UserSnapshotAsync(userId.Value, ct))
                : JsonSerializer.Serialize(await RoleSnapshotAsync(roleId!.Value, ct));
            var nextVersion = userId.HasValue ? JsonSerializer.Deserialize<UserAccessDetails>(after)!.Version
                : JsonSerializer.Deserialize<RoleAccessDetails>(after)!.Version;
            var result = new AccessChangeResult(r.OperationId, changed, nextVersion);
            var evidence = new ChangeEvidence(hash, kind, userId, roleId, permissionId, r.Reason, before, after, result);
            db.AuditLogs.Add(new() { UserId = actor, Action = AuditAction, EntityType = "AccessAdministration", EntityId = r.OperationId,
                OldValues = before, NewValues = JsonSerializer.Serialize(evidence), Description = r.Reason, CreatedAt = access.Now });
            await db.SaveChangesAsync(ct); await tx.ReleaseSavepointAsync(point, ct);
            if (owned is not null) await owned.CommitAsync(ct); return result;
        }
        catch
        {
            if (owned is not null) await owned.RollbackAsync(CancellationToken.None);
            else await tx.RollbackToSavepointAsync(point, CancellationToken.None);
            db.ChangeTracker.Clear(); throw;
        }
        finally { if (owned is not null) await owned.DisposeAsync(); }
    }
    public async Task<AccessPage<AccessAuditDetails>> HistoryAsync(AccessAuditQuery q, CancellationToken ct = default)
    {
        await owner.RequireAsync("Users.View", ct); await owner.RequireAsync("Roles.View", ct);
        var offset = Offset(q.Page, q.PageSize); var rows = db.AuditLogs.AsNoTracking().Where(x => x.Action == AuditAction);
        if (q.ActorUserId.HasValue) rows = rows.Where(x => x.UserId == q.ActorUserId);
        if (q.OperationId.HasValue) rows = rows.Where(x => x.EntityId == q.OperationId);
        if (q.TargetUserId.HasValue)
        {
            var filter = JsonSerializer.Serialize(new { TargetUserId = q.TargetUserId.Value });
            rows = rows.Where(x => EF.Functions.JsonContains(x.NewValues!, filter));
        }
        if (q.RoleId.HasValue)
        {
            var filter = JsonSerializer.Serialize(new { RoleId = q.RoleId.Value });
            rows = rows.Where(x => EF.Functions.JsonContains(x.NewValues!, filter));
        }
        var total = await rows.CountAsync(ct);
        var audit = await rows.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Skip(offset).Take(q.PageSize).ToListAsync(ct);
        var items = audit.Select(x =>
        {
            var e = JsonSerializer.Deserialize<ChangeEvidence>(x.NewValues!)!;
            return new AccessAuditDetails(x.Id, x.EntityId!.Value, x.UserId, x.CreatedAt, e.Kind, e.TargetUserId,
                e.RoleId, e.PermissionId, e.Reason, e.OldValues, e.NewValues, e.Result);
        }).ToArray();
        return new(items, total, q.Page, q.PageSize);
    }
}
