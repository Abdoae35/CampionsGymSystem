using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Members;

public static class MemberPermissions
{
    public static readonly string[] Codes = ["Members.View", "Members.Create", "Members.Edit", "Members.ChangeStatus",
        "Members.ChangePrimaryPhone", "Members.AssignSales", "Members.ViewSalesHistory", "Members.CreateCrossBranch", "Members.OwnSalesCustomers"];
}

// Explicit setup, following BranchPermissionSetup; never writes on normal startup.
public sealed class MemberPermissionSetup(ApplicationDbContext db)
{
    public async Task ConfigureAsync(CancellationToken ct = default)
    {
        var owned = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var existing = await db.Permissions.Where(p => MemberPermissions.Codes.Contains(p.Code)).ToListAsync(ct);
            foreach (var code in MemberPermissions.Codes.Where(c => existing.All(p => p.Code != c)))
            {
                var permission = new Permission { Code = code, Name = code, Module = "Members" };
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
            db.AuditLogs.Add(new() { Action = "Permission.MemberAccessConfigured", EntityType = nameof(Permission),
                Description = "Explicit Member permission setup; activation states and existing grants preserved." });
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
