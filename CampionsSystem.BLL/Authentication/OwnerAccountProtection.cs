using System.Data;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CampionsSystem.BLL.Authentication;

// One coordination boundary for role/permission changes and employment changes
// which affect Owner authentication. Acquire it before any employee row lock.
public sealed class OwnerAccountProtection(ApplicationDbContext db, OwnerAdministrationAccess owner,
    UserContextService contexts)
{
    private async Task LockMutationAsync(CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null
            || db.Database.CurrentTransaction.GetDbTransaction().IsolationLevel != IsolationLevel.ReadCommitted)
            throw new Operations.BusinessRuleException("Owner account protection requires ReadCommitted coordination.");
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(173528701, 1)", ct);
    }

    public async Task LockAsync(Guid? targetUser, CancellationToken ct)
    {
        await LockMutationAsync(ct);
        await LockAccountsAsync(targetUser, ct);
    }

    public async Task LockEmployeeAsync(Guid employeeId, CancellationToken ct)
    {
        await LockMutationAsync(ct);
        var user = await db.Employees.AsNoTracking().Where(x => x.Id == employeeId)
            .Select(x => x.UserId).SingleOrDefaultAsync(ct);
        await LockAccountsAsync(user, ct);
    }

    private async Task LockAccountsAsync(Guid? targetUser, CancellationToken ct)
    {
        await db.Employees.FromSqlInterpolated($"""
            SELECT e.* FROM "Employees" e WHERE e."UserId"={targetUser}
            OR e."UserId" IN (SELECT m."UserId" FROM "AspNetUserRoles" m JOIN "AspNetRoles" r ON r."Id"=m."RoleId" WHERE r."NormalizedName"='OWNER')
            ORDER BY e."Id" FOR SHARE
            """).AsNoTracking().ToListAsync(ct);
        // Serialize account eligibility edits without blocking foreign-key KEY SHARE
        // locks when concurrent CRM/financial work records the performing user.
        await db.Users.FromSqlInterpolated($"""
            SELECT u.* FROM "AspNetUsers" u WHERE u."Id"={targetUser}
            OR u."Id" IN (SELECT m."UserId" FROM "AspNetUserRoles" m JOIN "AspNetRoles" r ON r."Id"=m."RoleId" WHERE r."NormalizedName"='OWNER')
            ORDER BY u."Id" FOR NO KEY UPDATE
            """).AsNoTracking().ToListAsync(ct);
    }

    public async Task<bool> RequireEmployeeChangeAsync(Employee employee, Guid actor, bool disablesAccount,
        CancellationToken ct)
    {
        if (employee.UserId is not Guid user || !await owner.IsOwnerAsync(user, ct)) return false;
        if (await owner.RequireAsync(ct: ct) != actor) throw new UnauthorizedAccessException();
        if (user == actor && disablesAccount)
            throw new AccessAdministrationException(409, "The administering Owner cannot disable their own account through employee lifecycle changes.");
        return true;
    }

    public async Task ProtectOwnersAsync(Guid actor, CancellationToken ct)
    {
        var ids = await owner.OwnerIdsAsync(ct);
        var usable = new List<Guid>();
        foreach (var id in ids)
        {
            var context = await contexts.ResolveAsync(id, ct);
            if (context is not null && OwnerAdministrationAccess.RecoveryPermissions.All(context.Permissions.Contains)) usable.Add(id);
        }
        if (usable.Count == 0)
            throw new AccessAdministrationException(409, "At least one active, unlocked eligible Owner must retain all administration capabilities.");
        if (!usable.Contains(actor))
            throw new AccessAdministrationException(409, "The administering Owner must retain Owner membership and all administration capabilities.");
    }
}
