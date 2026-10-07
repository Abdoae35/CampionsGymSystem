using CampionsSystem.BLL.Authentication;
using CampionsSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Branches;

public sealed class EmployeeBranchResolver(ApplicationDbContext database)
{
    public async Task<BranchContext?> ResolveAsync(Guid employeeId, DateOnly today, CancellationToken ct = default)
    {
        // Count all valid assignments, including inactive-branch assignments, before resolving.
        // Legacy overlapping/nonprimary assignments must not silently broaden operational scope.
        var assignments = await database.EmployeeBranchAssignments.AsNoTracking()
            .Where(x => x.EmployeeId == employeeId && x.StartDate <= today && (x.EndDate == null || x.EndDate >= today))
            .OrderByDescending(x => x.IsPrimary).ThenBy(x => x.Id)
            .Select(x => new { x.BranchId, x.Branch.Name, x.Branch.IsActive }).Take(2).ToListAsync(ct);
        return assignments.Count == 1 && assignments[0].IsActive
            ? new BranchContext(assignments[0].BranchId, assignments[0].Name) : null;
    }
}
