using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Branches;

public sealed record BranchAssignmentDetails(Guid Id, Guid EmployeeId, Guid BranchId, string BranchName,
    DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, Guid AssignedByUserId, DateTimeOffset CreatedAt);
public sealed record AssignBranchRequest(Guid BranchId, DateOnly StartDate);
public sealed record TransferBranchRequest(Guid TargetBranchId, DateOnly EffectiveDate);

public sealed class EmployeeBranchAssignmentService(ApplicationDbContext database, ICurrentUserService current, OperationAccess access)
{
    private async Task<CurrentUserContext> RequireAsync(string permission, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        return user;
    }
    private static void Scope(CurrentUserContext user, Guid? branchId) => BranchAccessService.EnsureScope(user, branchId);
    private async Task<List<EmployeeBranchAssignment>> ActiveAsync(Guid employeeId, CancellationToken ct) =>
        await database.EmployeeBranchAssignments.AsNoTracking().Include(x => x.Branch)
            .Where(x => x.EmployeeId == employeeId && x.StartDate <= access.Today && (x.EndDate == null || x.EndDate >= access.Today))
            .ToListAsync(ct);
    private async Task<EmployeeBranchAssignment?> CurrentAsync(Guid employeeId, CancellationToken ct)
    {
        var active = await ActiveAsync(employeeId, ct);
        if (active.Count > 1) throw new BusinessRuleException("Employee has overlapping assignments; reconcile the history first.");
        return active.SingleOrDefault();
    }
    private static BranchAssignmentDetails Details(EmployeeBranchAssignment x) => new(x.Id, x.EmployeeId,
        x.BranchId, x.Branch.Name, x.StartDate, x.EndDate, x.IsPrimary, x.AssignedByUserId, x.CreatedAt);
    private async Task CheckEmployeeAsync(Guid id, CancellationToken ct)
    {
        if (!await database.Employees.AnyAsync(x => x.Id == id, ct)) throw new BranchNotFoundException("Employee not found.");
    }
    public async Task<BranchAssignmentDetails?> GetCurrentAsync(Guid employeeId, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.View", ct); await CheckEmployeeAsync(employeeId, ct);
        var assignment = await CurrentAsync(employeeId, ct); Scope(user, assignment?.BranchId);
        return assignment is null ? null : Details(assignment);
    }
    public async Task<IReadOnlyList<BranchAssignmentDetails>> HistoryAsync(Guid employeeId, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.View", ct); await CheckEmployeeAsync(employeeId, ct);
        Scope(user, (await CurrentAsync(employeeId, ct))?.BranchId);
        var history = await database.EmployeeBranchAssignments.AsNoTracking().Include(x => x.Branch)
            .Where(x => x.EmployeeId == employeeId).OrderBy(x => x.StartDate).ThenBy(x => x.Id).ToListAsync(ct);
        return history.Select(Details).ToArray();
    }
    public async Task<BranchAssignmentDetails> AssignAsync(Guid employeeId, AssignBranchRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.Manage", ct); Scope(user, request.BranchId);
        return await EmployeeAssignmentTransaction.RunAsync(database, employeeId, async employee =>
        {
            if (employee.EmploymentStatus != EmploymentStatus.Active || request.StartDate < access.Today
                || request.StartDate < employee.HireDate || employee.TerminationDate < request.StartDate)
                throw new BusinessRuleException("Employee must be active and assignment date must be today or later within employment dates.");
            var branch = await database.GymBranches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.BranchId, ct)
                ?? throw new BranchNotFoundException("Branch not found.");
            if (!branch.IsActive) throw new BusinessRuleException("Branch is inactive.");
            if (await database.EmployeeBranchAssignments.AnyAsync(x => x.EmployeeId == employeeId
                && (x.EndDate == null || x.EndDate >= request.StartDate), ct))
                throw new BusinessRuleException("Assignment overlaps existing history; use transfer for an assigned employee.");
            // Unassigned employees/history outside the caller's branch require explicit global scope.
            if (!user.Permissions.Contains(BranchManagementService.AccessAllPermission)
                && await database.EmployeeBranchAssignments.AnyAsync(x => x.EmployeeId == employeeId && x.BranchId != request.BranchId, ct))
                throw new UnauthorizedAccessException();
            var assignment = new EmployeeBranchAssignment { EmployeeId = employeeId, BranchId = branch.Id,
                StartDate = request.StartDate, IsPrimary = true, AssignedByUserId = user.UserId, CreatedAt = access.Now };
            database.EmployeeBranchAssignments.Add(assignment);
            access.Audit(user.UserId, branch.Id, "Employee.BranchAssigned", nameof(EmployeeBranchAssignment), assignment.Id,
                after: new { EmployeeId = employeeId, NewBranchId = branch.Id, EffectiveDate = request.StartDate });
            await database.SaveChangesAsync(ct);
            return new BranchAssignmentDetails(assignment.Id, employeeId, branch.Id, branch.Name, assignment.StartDate,
                assignment.EndDate, assignment.IsPrimary, assignment.AssignedByUserId, assignment.CreatedAt);
        }, ct);
    }
    public async Task<BranchAssignmentDetails> TransferAsync(Guid employeeId, TransferBranchRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.Transfer", ct);
        Scope(user, request.TargetBranchId);
        return await EmployeeAssignmentTransaction.RunAsync(database, employeeId, async employee =>
        {
            if (employee.EmploymentStatus != EmploymentStatus.Active || request.EffectiveDate < access.Today
                || request.EffectiveDate < employee.HireDate || employee.TerminationDate < request.EffectiveDate)
                throw new BusinessRuleException("Employee must be active and transfer date must be today or later within employment dates.");
            var old = await CurrentAsync(employeeId, ct)
                ?? throw new BusinessRuleException("Employee has no current assignment.");
            Scope(user, old.BranchId);
            if (old.BranchId == request.TargetBranchId) throw new BusinessRuleException("Employee is already assigned to the target branch.");
            if (request.EffectiveDate <= old.StartDate)
                throw new BusinessRuleException("Transfer date must follow the current assignment start date.");
            if (old.EndDate is DateOnly end && request.EffectiveDate.DayNumber > end.DayNumber + 1)
                throw new BusinessRuleException("Transfer cannot extend an already scheduled assignment end date.");
            if (await database.EmployeeBranchAssignments.AnyAsync(x => x.EmployeeId == employeeId && x.Id != old.Id
                && (x.StartDate > access.Today || x.EndDate == null || x.EndDate >= request.EffectiveDate), ct))
                throw new BusinessRuleException("Transfer conflicts with another assignment or an already scheduled transfer.");
            var target = await database.GymBranches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.TargetBranchId, ct)
                ?? throw new BranchNotFoundException("Branch not found.");
            if (!target.IsActive) throw new BusinessRuleException("Target branch is inactive.");
            var tracked = await database.EmployeeBranchAssignments.SingleAsync(x => x.Id == old.Id, ct);
            tracked.EndDate = request.EffectiveDate.AddDays(-1);
            // Release the existing partial unique slot before inserting the new primary assignment.
            // Both saves remain protected by the same transaction/savepoint and employee row lock.
            await database.SaveChangesAsync(ct);
            var assignment = new EmployeeBranchAssignment { EmployeeId = employeeId, BranchId = target.Id,
                StartDate = request.EffectiveDate, IsPrimary = true, AssignedByUserId = user.UserId, CreatedAt = access.Now };
            database.EmployeeBranchAssignments.Add(assignment);
            access.Audit(user.UserId, target.Id, "Employee.BranchTransferred", nameof(Employee), employeeId,
                new { EmployeeId = employeeId, OldBranchId = old.BranchId, OldAssignmentId = old.Id, old.StartDate, old.EndDate },
                new { EmployeeId = employeeId, OldBranchId = old.BranchId, NewBranchId = target.Id,
                    NewAssignmentId = assignment.Id, EffectiveDate = request.EffectiveDate, ActorUserId = user.UserId });
            await database.SaveChangesAsync(ct);
            return new BranchAssignmentDetails(assignment.Id, employeeId, target.Id, target.Name, assignment.StartDate,
                assignment.EndDate, assignment.IsPrimary, assignment.AssignedByUserId, assignment.CreatedAt);
        }, ct);
    }

}
