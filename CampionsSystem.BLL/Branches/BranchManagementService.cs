using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Branches;

public sealed class BranchManagementService(ApplicationDbContext database, ICurrentUserService current, OperationAccess access)
{
    public const string AccessAllPermission = "Branches.AccessAll";
    private async Task<CurrentUserContext> RequireAsync(string permission, Guid? branch, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)
            || (!user.Permissions.Contains(AccessAllPermission) && (branch is null || user.CurrentBranchId != branch)))
            throw new UnauthorizedAccessException();
        return user;
    }

    private static readonly System.Linq.Expressions.Expression<Func<GymBranch, BranchDetails>> Details =
        x => new BranchDetails(x.Id, x.Name, x.CityId, x.City.Name, x.Address, x.Phone,
            x.IsActive, x.MaxBorrowedGymSessions, x.QuickRefundWindowHours);

    public async Task<IReadOnlyList<BranchDetails>> ListAsync(CancellationToken ct = default)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains("Branches.View")) throw new UnauthorizedAccessException();
        var query = database.GymBranches.AsNoTracking();
        if (!user.Permissions.Contains(AccessAllPermission)) query = query.Where(x => x.Id == user.CurrentBranchId);
        return await query.OrderBy(x => x.Name).ThenBy(x => x.Id).Select(Details).ToListAsync(ct);
    }

    public async Task<BranchDetails> GetAsync(Guid id, CancellationToken ct = default)
    {
        await RequireAsync("Branches.View", id, ct);
        return await database.GymBranches.AsNoTracking().Where(x => x.Id == id).Select(Details).SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Branch not found.");
    }

    public async Task<BranchDetails> SaveAsync(Guid? id, SaveBranchRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Branches.Manage", id, ct);
        if (request.CityId == Guid.Empty || string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 256
            || request.Address?.Length > 256 || request.Phone?.Length > 64
            || request.MaxBorrowedGymSessions < 0 || request.QuickRefundWindowHours < 0)
            throw new BusinessRuleException("Invalid branch details.");
        var branch = id.HasValue ? await database.GymBranches.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new BranchNotFoundException("Branch not found.") : new GymBranch();
        // An unchanged inactive city is permitted for editing an existing branch's history/settings.
        if (!await database.Cities.AnyAsync(x => x.Id == request.CityId && (x.IsActive || (id != null && x.Id == branch.CityId)), ct))
            throw new BusinessRuleException("An active city is required.");
        var before = id.HasValue ? new { branch.Name, branch.CityId, branch.Address, branch.Phone, branch.IsActive,
            branch.MaxBorrowedGymSessions, branch.QuickRefundWindowHours } : null;
        var wasActive = branch.IsActive;
        branch.Name = request.Name.Trim(); branch.CityId = request.CityId; branch.Address = request.Address?.Trim();
        branch.Phone = request.Phone?.Trim(); branch.IsActive = request.IsActive;
        branch.MaxBorrowedGymSessions = request.MaxBorrowedGymSessions; branch.QuickRefundWindowHours = request.QuickRefundWindowHours;
        if (!id.HasValue) database.GymBranches.Add(branch);
        var after = new { branch.Name, branch.CityId, branch.Address, branch.Phone, branch.IsActive,
            branch.MaxBorrowedGymSessions, branch.QuickRefundWindowHours };
        access.Audit(user.UserId, branch.Id, id.HasValue ? "Branch.Updated" : "Branch.Created", nameof(GymBranch), branch.Id, before, after);
        if (id.HasValue && wasActive != branch.IsActive)
            access.Audit(user.UserId, branch.Id, branch.IsActive ? "Branch.Activated" : "Branch.Deactivated", nameof(GymBranch), branch.Id, before, after);
        await database.SaveChangesAsync(ct);
        return await database.GymBranches.AsNoTracking().Where(x => x.Id == branch.Id).Select(Details).SingleAsync(ct);
    }
}
