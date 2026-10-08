using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Crm;

public static class CrmPermissions
{
    public static readonly string[] Codes = ["Leads.View","Leads.Manage","Leads.Assign","Leads.Convert",
        "CRM.ViewMemberFollowUps","CRM.ManageMemberFollowUps","CRM.ManageHouseFollowUps"];
}

// Explicit setup, following BranchPermissionSetup; never writes on normal startup.
public sealed class CrmPermissionSetup(ApplicationDbContext db)
{
    public async Task ConfigureAsync(CancellationToken ct = default)
    {
        var owned = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var existing = await db.Permissions.Where(p => CrmPermissions.Codes.Contains(p.Code)).ToListAsync(ct);
            foreach (var code in CrmPermissions.Codes.Where(c => existing.All(p => p.Code != c)))
            {
                var permission = new Permission { Code = code, Name = code, Module = code.Split('.')[0] };
                existing.Add(permission); db.Permissions.Add(permission);
            }
            await db.SaveChangesAsync(ct);
            var owner = await db.Roles.SingleOrDefaultAsync(r => r.NormalizedName == "OWNER", ct);
            if (owner is not null)
            {
                var granted = await db.RolePermissions.Where(p => p.RoleId == owner.Id).Select(p => p.PermissionId).ToListAsync(ct);
                foreach (var p in existing.Where(p => !granted.Contains(p.Id)))
                    db.RolePermissions.Add(new() { RoleId = owner.Id, PermissionId = p.Id });
            }
            db.AuditLogs.Add(new() { Action = "Permission.CrmAccessConfigured", EntityType = nameof(Permission),
                Description = "Explicit CRM permission setup; activation states and existing grants preserved." });
            await db.SaveChangesAsync(ct);
            if (owned is not null) await owned.CommitAsync(ct);
        }
        catch
        {
            if (owned is not null) await owned.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally { if (owned is not null) await owned.DisposeAsync(); }
    }
}
