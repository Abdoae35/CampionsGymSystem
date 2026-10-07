using CampionsSystem.BLL.Authentication;

namespace CampionsSystem.BLL.Branches;

public sealed record OperationalBranchContext(BranchContext? CurrentBranch, bool HasGlobalAccess)
{
    public Guid? CurrentBranchId => CurrentBranch?.Id;
    public bool HasBranch => CurrentBranch is not null;
}

public interface ICurrentBranchContext
{
    Task<OperationalBranchContext> GetAsync(CancellationToken ct = default);
}

// Reuses the authenticated DB-backed user context, without a second employee/branch lookup path.
public sealed class CurrentBranchContext(ICurrentUserService current) : ICurrentBranchContext
{
    public async Task<OperationalBranchContext> GetAsync(CancellationToken ct = default)
    {
        var user = await current.GetAsync(ct);
        return new(user?.CurrentBranch, user?.Permissions.Contains(BranchManagementService.AccessAllPermission) == true);
    }
}
