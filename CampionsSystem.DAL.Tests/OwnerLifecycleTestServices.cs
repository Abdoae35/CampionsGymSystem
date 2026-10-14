using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.DAL.Data;
using Microsoft.Extensions.Options;

namespace CampionsSystem.DAL.Tests;

internal static class OwnerLifecycleTestServices
{
    internal static OwnerAccountProtection Protection(ApplicationDbContext db, ICurrentUserService current, TimeProvider? clock = null)
    {
        var contexts = new UserContextService(db, new PermissionService(db), clock ?? TimeProvider.System,
            Options.Create(new AuthContextOptions()), new EmployeeBranchResolver(db));
        return new(db, new(db, current), contexts);
    }
}
