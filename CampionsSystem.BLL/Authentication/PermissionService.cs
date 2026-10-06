using CampionsSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Authentication;

public sealed class PermissionService(ApplicationDbContext database) : IPermissionService
{
    public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var codes = await (
            from userRole in database.UserRoles.AsNoTracking()
            join rolePermission in database.RolePermissions.AsNoTracking() on userRole.RoleId equals rolePermission.RoleId
            join permission in database.Permissions.AsNoTracking() on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId && permission.IsActive
                && database.Users.Any(user => user.Id == userId && user.IsActive)
            select permission.Code).Distinct().ToListAsync(cancellationToken);
        return new HashSet<string>(codes, StringComparer.Ordinal);
    }
}
