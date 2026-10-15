using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Shifts;

// Explicit catalog setup. Existing role/custom grants and inactive capabilities
// stay untouched; only an explicitly configured global role receives new grants.
public sealed class ShiftPermissionSetup(ApplicationDbContext db)
{
    public async Task ConfigureAsync(Guid? globalRoleId=null,CancellationToken ct=default)
    {
        await using var tx=db.Database.CurrentTransaction==null?await db.Database.BeginTransactionAsync(ct):null;
        if(globalRoleId.HasValue&&!await db.RolePermissions.AnyAsync(x=>x.RoleId==globalRoleId&&x.Permission.Code=="Branches.AccessAll"&&x.Permission.IsActive,ct))
            throw new InvalidOperationException("Select an existing role with active global branch capability.");
        var rows=await db.Permissions.Where(x=>ShiftPermissions.Codes.Contains(x.Code)).ToListAsync(ct);
        foreach(var code in ShiftPermissions.Codes.Where(code=>rows.All(x=>x.Code!=code)))
        {var row=new Permission{Code=code,Name=code,Module=code.Split('.')[0]};db.Add(row);rows.Add(row);}
        await db.SaveChangesAsync(ct);
        if(globalRoleId.HasValue)
        {
            var assigned=await db.RolePermissions.Where(x=>x.RoleId==globalRoleId).Select(x=>x.PermissionId).ToListAsync(ct);
            foreach(var row in rows.Where(x=>x.IsActive&&x.Code!=ShiftPermissions.CloseOthers&&!assigned.Contains(x.Id)))
                db.Add(new RolePermission{RoleId=globalRoleId.Value,PermissionId=row.Id});
        }
        db.Add(new AuditLog{Action="Permission.ShiftsVaultExpensesConfigured",EntityType=nameof(Permission),Description="Explicit capability setup; existing activation, role grants and user overrides preserved."});
        await db.SaveChangesAsync(ct);if(tx!=null)await tx.CommitAsync(ct);
    }
}
