# Branches rule/validation evidence

Completed source; deployment/configuration prerequisites are separate. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Branches/BranchManagementService.cs

Lines 8–18

```csharp


public sealed class BranchManagementService(ApplicationDbContext database, ICurrentUserService current, OperationAccess access)
{
    public const string AccessAllPermission = "Branches.AccessAll";
    private async Task<CurrentUserContext> RequireAsync(string permission, Guid? branch, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        BranchAccessService.EnsureScope(user, branch);
        return user;
    }

```

Lines 24–54

```csharp

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
            || (request.CountryCallingCode != null && !System.Text.RegularExpressions.Regex.IsMatch(request.CountryCallingCode, @"^[+][1-9][0-9]{0,2}$"))
            || request.MaxBorrowedGymSessions < 0 || request.QuickRefundWindowHours < 0)
            throw new BusinessRuleException("Invalid branch details.");
        var branch = id.HasValue ? await database.GymBranches.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new BranchNotFoundException("Branch not found.") : new GymBranch();
        // An unchanged inactive city is permitted for editing an existing branch's history/settings.
        if (!await database.Cities.AnyAsync(x => x.Id == request.CityId && (x.IsActive || (id != null && x.Id == branch.CityId)), ct))
            throw new BusinessRuleException("An active city is required.");
        var before = id.HasValue ? new { branch.Name, branch.CityId, branch.Address, branch.Phone, branch.IsActive,
            branch.MaxBorrowedGymSessions, branch.QuickRefundWindowHours, branch.CountryCallingCode } : null;

```

## CampionsSystem.BLL/Branches/BranchAccessService.cs

Lines 8–47

```csharp

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

```

## CampionsSystem.BLL/Branches/EmployeeBranchResolver.cs

Lines 6–18

```csharp


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

```

## CampionsSystem.BLL/Branches/CurrentBranchContext.cs

Lines 2–7

```csharp


namespace CampionsSystem.BLL.Branches;

public sealed record OperationalBranchContext(BranchContext? CurrentBranch, bool HasGlobalAccess)
{
    public Guid? CurrentBranchId => CurrentBranch?.Id;

```

Lines 19–24

```csharp

    public async Task<OperationalBranchContext> GetAsync(CancellationToken ct = default)
    {
        var user = await current.GetAsync(ct);
        return new(user?.CurrentBranch, user?.Permissions.Contains(BranchManagementService.AccessAllPermission) == true);
    }
}

```

## CampionsSystem.BLL/Branches/EmployeeAssignmentTransaction.cs

Lines 19–24

```csharp

            await transaction.CreateSavepointAsync(savepoint, ct);
            var employees = await database.Employees.FromSqlInterpolated(
                $"SELECT * FROM \"Employees\" WHERE \"Id\" = {employeeId} FOR UPDATE").AsNoTracking().ToListAsync(ct);
            var employee = employees.SingleOrDefault() ?? throw new BranchNotFoundException("Employee not found.");
            var result = await change(employee);
            await transaction.ReleaseSavepointAsync(savepoint, ct);

```

## CampionsSystem.BLL/Branches/EmployeeBranchAssignmentService.cs

Lines 8–25

```csharp

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

```

Lines 28–33

```csharp

    private async Task<EmployeeBranchAssignment?> CurrentAsync(Guid employeeId, CancellationToken ct)
    {
        var active = await ActiveAsync(employeeId, ct);
        if (active.Count > 1) throw new BusinessRuleException("Employee has overlapping assignments; reconcile the history first.");
        return active.SingleOrDefault();
    }

```

Lines 35–73

```csharp

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

```

Lines 81–86

```csharp

    }
    public async Task<BranchAssignmentDetails> TransferAsync(Guid employeeId, TransferBranchRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.Transfer", ct);
        Scope(user, request.TargetBranchId);
        return await EmployeeAssignmentTransaction.RunAsync(database, employeeId, async employee =>

```

Lines 88–110

```csharp

            // Opening an owned shift takes this same employee row lock. Transfers
            // must not strand an open shift outside the employee's new branch scope.
            if (await database.Shifts.AnyAsync(x => x.EmployeeId == employeeId && x.Status == ShiftStatus.Open, ct))
                throw new BusinessRuleException("Close your own shift before transferring branches.");
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

```
