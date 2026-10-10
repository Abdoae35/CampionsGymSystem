using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
// Explicit guarded Development CLI, not automatic production provisioning or role-name authorization.
public sealed class SalesPermissionSetup(ApplicationDbContext db)
{
    public async Task ConfigureAsync(Guid? managementRoleId=null,CancellationToken ct=default)
    {
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        if(managementRoleId.HasValue && !await db.RolePermissions.AnyAsync(p=>p.RoleId==managementRoleId && p.Permission.Code=="Branches.AccessAll" && p.Permission.IsActive,ct))
            throw new InvalidOperationException("Select an existing role with active global branch capability.");
        var permissions=await db.Permissions.Where(p=>SalesPermissions.Codes.Contains(p.Code)).ToListAsync(ct);
        foreach(var code in SalesPermissions.Codes.Where(code=>permissions.All(p=>p.Code!=code)))
        {var p=new Permission{Code=code,Name=code,Module=code.Split('.')[0]};db.Add(p);permissions.Add(p);}
        await db.SaveChangesAsync(ct);
        if(managementRoleId.HasValue)
        {
            var assigned=await db.RolePermissions.Where(p=>p.RoleId==managementRoleId).Select(p=>p.PermissionId).ToListAsync(ct);
            foreach(var p in permissions.Where(p=>!assigned.Contains(p.Id)))db.Add(new RolePermission{RoleId=managementRoleId.Value,PermissionId=p.Id});
        }
        db.Add(new AuditLog{Action="Permission.SalesPaymentsConfigured",EntityType=nameof(Permission),Description="Explicit financial capability setup; existing activation/grants/overrides preserved."});
        await db.SaveChangesAsync(ct);if(tx!=null)await tx.CommitAsync(ct);
    }
}
