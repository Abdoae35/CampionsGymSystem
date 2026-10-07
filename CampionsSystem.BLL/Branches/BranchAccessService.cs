using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Branches;

public interface IBranchAccessService
{
    Task<bool> CanAccessBranchAsync(Guid branchId, CancellationToken ct = default);
    Task EnsureCanAccessBranchAsync(Guid branchId, CancellationToken ct = default);
    Task<Guid> ResolveOperationBranchAsync(Guid? explicitlySelectedBranchId = null, CancellationToken ct = default);
}

public sealed class BranchAccessService(ICurrentBranchContext current, ApplicationDbContext database) : IBranchAccessService
{
    // Shared scope predicate for services that already loaded the authenticated user.
    public static void EnsureScope(CurrentUserContext user, Guid? branchId)
    {
        if (!user.Permissions.Contains(BranchManagementService.AccessAllPermission)
            && (!branchId.HasValue || user.CurrentBranchId != branchId)) throw new UnauthorizedAccessException();
    }

    public async Task<bool> CanAccessBranchAsync(Guid branchId, CancellationToken ct = default)
    {
        var scope = await current.GetAsync(ct);
        return (scope.HasGlobalAccess || (scope.HasBranch && scope.CurrentBranchId == branchId))
            && await database.GymBranches.AsNoTracking().AnyAsync(x => x.Id == branchId && x.IsActive, ct);
    }

    public async Task EnsureCanAccessBranchAsync(Guid branchId, CancellationToken ct = default)
    {
        var scope = await current.GetAsync(ct);
        if (!scope.HasGlobalAccess && (!scope.HasBranch || scope.CurrentBranchId != branchId)) throw new UnauthorizedAccessException();
        var branch = await database.GymBranches.AsNoTracking().Where(x => x.Id == branchId).Select(x => new { x.IsActive }).SingleOrDefaultAsync(ct);
        if (branch is null) throw new BranchNotFoundException("Branch not found.");
        if (!branch.IsActive) throw new BusinessRuleException("Branch is inactive.");
    }

    public async Task<Guid> ResolveOperationBranchAsync(Guid? explicitlySelectedBranchId = null, CancellationToken ct = default)
    {
        var scope = await current.GetAsync(ct);
        var id = explicitlySelectedBranchId ?? scope.CurrentBranchId;
        if (id is null) throw new BusinessRuleException("A current branch or an explicitly authorized target branch is required.");
        await EnsureCanAccessBranchAsync(id.Value, ct);
        return id.Value;
    }
}
