using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.CheckIns;

// Explicit guarded CLI setup, never a startup seed. No role-name authorization or inferred staff grants.
public sealed class CheckInPermissionSetup(ApplicationDbContext db)
{
    public async Task ConfigureAsync(Guid? managementRoleId = null, CancellationToken ct = default)
    {
        await using var tx=db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        if(managementRoleId.HasValue && !await db.RolePermissions.AnyAsync(x=>x.RoleId==managementRoleId
            && x.Permission.Code=="Branches.AccessAll" && x.Permission.IsActive,ct))
            throw new InvalidOperationException("Select an existing role with active global branch capability.");
        var permissions=await db.Permissions.Where(x=>CheckInPermissions.Codes.Contains(x.Code)).ToListAsync(ct);
        foreach(var code in CheckInPermissions.Codes.Where(x=>permissions.All(p=>p.Code!=x)))
        {
            var p=new Permission{Code=code,Name=code,Module="CheckIns"};db.Add(p);permissions.Add(p);
        }
        await db.SaveChangesAsync(ct);
        if(managementRoleId.HasValue)
        {
            var existing=await db.RolePermissions.Where(x=>x.RoleId==managementRoleId).Select(x=>x.PermissionId).ToListAsync(ct);
            foreach(var p in permissions.Where(x=>!existing.Contains(x.Id)))
                db.Add(new RolePermission{RoleId=managementRoleId.Value,PermissionId=p.Id});
        }
        db.Add(new AuditLog{Action="Permission.CheckInAccessConfigured",EntityType=nameof(Permission),
            Description="Explicit check-in catalog setup; activation, existing grants and user overrides preserved."});
        await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);
    }
}
