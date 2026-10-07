using CampionsSystem.DAL.Data;
using CampionsSystem.BLL.Branches;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CampionsSystem.BLL.Authentication;

public sealed class UserContextService(ApplicationDbContext database, IPermissionService permissionService,
    TimeProvider clock, IOptions<AuthContextOptions> options, EmployeeBranchResolver branches)
{
    private readonly TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);

    public async Task<CurrentUserContext?> ResolveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow();
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, timeZone).DateTime);
        var user = await database.Users.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new { x.Id, x.UserName, x.IsActive, x.LockoutEnabled, x.LockoutEnd })
            .SingleOrDefaultAsync(cancellationToken);
        if (user is null || !user.IsActive || (user.LockoutEnabled && user.LockoutEnd > now)) return null;

        var employee = await database.Employees.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new { x.Id, x.FullName, x.EmploymentStatus, x.HireDate, x.TerminationDate })
            .SingleOrDefaultAsync(cancellationToken);
        if (employee is not null && (employee.EmploymentStatus != EmploymentStatus.Active
            || employee.HireDate > today || employee.TerminationDate < today)) return null;

        var branch = employee is null ? null : await branches.ResolveAsync(employee.Id, today, cancellationToken);
        var roles = await (from userRole in database.UserRoles.AsNoTracking()
                           join role in database.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                           where userRole.UserId == userId && role.Name != null
                           orderby role.Name
                           select role.Name!).ToListAsync(cancellationToken);
        var permissions = await permissionService.GetUserPermissionsAsync(userId, cancellationToken);
        return new CurrentUserContext(user.Id, user.UserName, employee?.Id, employee?.FullName,
            roles, permissions.Order(StringComparer.Ordinal).ToArray(), branch);
    }
}
