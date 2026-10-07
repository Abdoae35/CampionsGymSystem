using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Enums;
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
        var result = new HashSet<string>(codes, StringComparer.Ordinal);
        var overrides = await database.UserPermissionOverrides.AsNoTracking()
            .Where(x => x.UserId == userId && x.Permission.IsActive
                && database.Users.Any(user => user.Id == userId && user.IsActive))
            .Select(x => new { x.Permission.Code, x.Effect }).ToListAsync(cancellationToken);
        result.UnionWith(overrides.Where(x => x.Effect == PermissionOverrideEffect.Allow).Select(x => x.Code));
        result.ExceptWith(overrides.Where(x => x.Effect == PermissionOverrideEffect.Deny).Select(x => x.Code));
        return result;
    }
}
