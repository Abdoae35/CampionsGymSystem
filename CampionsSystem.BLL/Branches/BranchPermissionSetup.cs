using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Branches;

// Explicit operator command only. No startup writes, demo users, or schema changes.
public sealed class BranchPermissionSetup(ApplicationDbContext database)
{
    public async Task ConfigureAsync(CancellationToken ct = default)
    {
        var owned = database.Database.CurrentTransaction is null ? await database.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var permission = await database.Permissions.SingleOrDefaultAsync(x => x.Code == BranchManagementService.AccessAllPermission, ct);
            if (permission is null)
            {
                permission = new Permission { Code = BranchManagementService.AccessAllPermission, Name = "Access all branches", Module = "Branches" };
                database.Permissions.Add(permission); await database.SaveChangesAsync(ct);
            }
            // Bootstrap grant to an existing Identity role, never a runtime role-name bypass.
            var owner = await database.Roles.SingleOrDefaultAsync(x => x.NormalizedName == "OWNER", ct);
            if (owner is not null && !await database.RolePermissions.AnyAsync(x => x.RoleId == owner.Id && x.PermissionId == permission.Id, ct))
                database.RolePermissions.Add(new RolePermission { RoleId = owner.Id, PermissionId = permission.Id });
            database.AuditLogs.Add(new AuditLog { Action = "Permission.BranchAccessConfigured", EntityType = nameof(Permission),
                EntityId = permission.Id, Description = "Explicit branch permission setup; existing permission activation state preserved." });
            await database.SaveChangesAsync(ct);
            if (owned is not null) await owned.CommitAsync(ct);
        }
        finally { if (owned is not null) await owned.DisposeAsync(); }
    }
}
