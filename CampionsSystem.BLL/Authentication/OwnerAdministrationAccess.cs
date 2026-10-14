using CampionsSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Authentication;

// Normalized OWNER is the same canonical store convention used by permission setup.
// Neither a JWT role claim nor Branches.AccessAll is an Owner identity assertion.
public sealed class OwnerAdministrationAccess(ApplicationDbContext db, ICurrentUserService current)
{
    public const string Policy = "OwnerAccessAdministration";
    public static readonly string[] RecoveryPermissions = ["Users.View", "Users.Manage", "Roles.View", "Roles.Manage"];

    public async Task<Guid> RequireAsync(string? permission = null, CancellationToken ct = default)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !await IsOwnerAsync(user.UserId, ct)
            || permission is not null && !user.Permissions.Contains(permission))
            throw new UnauthorizedAccessException();
        return user.UserId;
    }
    public Task<bool> IsOwnerAsync(Guid user, CancellationToken ct = default) =>
        OwnerMemberships().AnyAsync(x => x == user, ct);
    public Task<List<Guid>> OwnerIdsAsync(CancellationToken ct = default) =>
        OwnerMemberships().Distinct().ToListAsync(ct);
    private IQueryable<Guid> OwnerMemberships() =>
        (from membership in db.UserRoles.AsNoTracking()
         join role in db.Roles.AsNoTracking() on membership.RoleId equals role.Id
         where role.NormalizedName == "OWNER"
         select membership.UserId);
}
