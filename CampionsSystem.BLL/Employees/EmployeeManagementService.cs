using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Employees;

public sealed partial class EmployeeManagementService(ApplicationDbContext database, ICurrentUserService current,
    OperationAccess access, EmployeeBranchResolver resolver, EmployeeBranchAssignmentService assignments, CampionsSystem.BLL.Subscriptions.SubscriptionService subscriptions)
{
    private async Task<CurrentUserContext> RequireAsync(string permission, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        return user;
    }

    private IQueryable<EmployeeBranchAssignment> CurrentAssignments() => database.EmployeeBranchAssignments.AsNoTracking()
        .Where(a => a.StartDate <= access.Today && (a.EndDate == null || a.EndDate >= access.Today));

    private IQueryable<Employee> ScopedEmployees(CurrentUserContext user, Guid? branchId)
    {
        var query = database.Employees.AsNoTracking();
        var global = user.Permissions.Contains(BranchManagementService.AccessAllPermission);
        if (branchId.HasValue) BranchAccessService.EnsureScope(user, branchId);
        if (!global || branchId.HasValue)
        {
            var scopeId = branchId ?? user.CurrentBranchId;
            if (scopeId is null) throw new UnauthorizedAccessException();
            var assignments = CurrentAssignments();
            // Ambiguous legacy assignments never broaden branch access.
            query = query.Where(e => assignments.Count(a => a.EmployeeId == e.Id) == 1
                && assignments.Any(a => a.EmployeeId == e.Id && a.BranchId == scopeId && a.Branch.IsActive));
        }
        return query;
    }

    public async Task<EmployeeListResult> ListAsync(EmployeeListRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.View", ct);
        if (request.Page < 1 || request.PageSize is < 1 or > 100 || request.Search?.Length > 256
            || (long)(request.Page - 1) * request.PageSize > int.MaxValue
            || (request.Status.HasValue && !Enum.IsDefined(request.Status.Value))
            || (request.EmploymentType.HasValue && !Enum.IsDefined(request.EmploymentType.Value)))
            throw new BusinessRuleException("Invalid employee filters or pagination.");
        var query = ScopedEmployees(user, request.BranchId);
        if (request.BranchId.HasValue && !await database.GymBranches.AnyAsync(x => x.Id == request.BranchId, ct))
            throw new BranchNotFoundException("Branch not found.");
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e => e.FullName.ToLower().Contains(search) || e.Phone.ToLower().Contains(search));
        }
        if (request.Status.HasValue) query = query.Where(e => e.EmploymentStatus == request.Status);
        if (request.EmploymentType.HasValue) query = query.Where(e => e.EmploymentType == request.EmploymentType);
        var count = await query.CountAsync(ct);
        var assignments = CurrentAssignments();
        var rows = await query.OrderBy(e => e.FullName).ThenBy(e => e.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(e => new
            {
                e.Id, e.FullName, e.Phone, e.JobTitle, e.EmploymentType, e.EmploymentStatus, e.HireDate, e.TerminationDate,
                HasSystemAccount = e.UserId != null,
                Branch = assignments.Count(a => a.EmployeeId == e.Id) == 1
                    ? assignments.Where(a => a.EmployeeId == e.Id && a.Branch.IsActive)
                        .Select(a => new BranchContext(a.BranchId, a.Branch.Name)).FirstOrDefault() : null
            }).ToListAsync(ct);
        return new(rows.Select(e => new EmployeeDetails(e.Id, e.FullName, e.Phone, e.JobTitle,
            e.EmploymentType.ToString(), e.EmploymentStatus.ToString(), e.HireDate, e.TerminationDate, e.Branch, e.HasSystemAccount)).ToArray(),
            count, request.Page, request.PageSize);
    }

    public async Task<EmployeeDetails> GetAsync(Guid id, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.View", ct);
        var employee = await ScopedEmployees(user, null).SingleOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new BranchNotFoundException("Employee not found in accessible scope.");
        return await DetailsAsync(employee, ct);
    }

    private static void ValidateProfile(EmployeeProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 256
            || string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Length > 64
            || request.JobTitle?.Length > 256 || request.HireDate == default || !Enum.IsDefined(request.EmploymentType))
            throw new BusinessRuleException("Name, phone, hire date and a valid employment type are required.");
    }

    private static void SetProfile(Employee employee, EmployeeProfileRequest request)
    {
        employee.FullName = request.FullName.Trim(); employee.Phone = request.Phone.Trim();
        employee.JobTitle = string.IsNullOrWhiteSpace(request.JobTitle) ? null : request.JobTitle.Trim();
        employee.EmploymentType = request.EmploymentType; employee.HireDate = request.HireDate;
    }

    public async Task<EmployeeDetails> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.Manage", ct);
        ValidateProfile(request);
        BranchAccessService.EnsureScope(user, request.InitialBranchId);
        var owned = database.Database.CurrentTransaction is null ? await database.Database.BeginTransactionAsync(ct) : null;
        var transaction = database.Database.CurrentTransaction!;
        var savepoint = "employee_create_" + Guid.NewGuid().ToString("N");
        try
        {
            await transaction.CreateSavepointAsync(savepoint, ct);
            var employee = new Employee { CreatedAt = access.Now };
            SetProfile(employee, request);
            database.Employees.Add(employee);
            await database.SaveChangesAsync(ct);
            if (request.InitialBranchId.HasValue)
                await assignments.AssignAsync(employee.Id, new(request.InitialBranchId.Value,
                    request.HireDate > access.Today ? request.HireDate : access.Today), ct);
            access.Audit(user.UserId, request.InitialBranchId, "EmployeeCreated", nameof(Employee), employee.Id,
                after: new { employee.FullName, employee.Phone, employee.JobTitle, employee.EmploymentType, employee.HireDate });
            await database.SaveChangesAsync(ct);
            var result = await DetailsAsync(employee, ct);
            await transaction.ReleaseSavepointAsync(savepoint, ct);
            if (owned is not null) await owned.CommitAsync(ct);
            return result;
        }
        catch
        {
            if (owned is not null) await owned.RollbackAsync(CancellationToken.None);
            else await transaction.RollbackToSavepointAsync(savepoint, CancellationToken.None);
            database.ChangeTracker.Clear(); throw;
        }
        finally { if (owned is not null) await owned.DisposeAsync(); }
    }

    private async Task<Employee> TrackedEmployeeAsync(Guid id, CancellationToken ct)
    {
        var employee = await database.Employees.SingleAsync(e => e.Id == id, ct);
        // Assignment transaction owns the row lock; refresh any already-tracked values.
        await database.Entry(employee).ReloadAsync(ct);
        return employee;
    }

    public async Task<EmployeeDetails> UpdateAsync(Guid id, EmployeeProfileRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.Manage", ct);
        ValidateProfile(request);
        return await EmployeeAssignmentTransaction.RunAsync(database, id, async locked =>
        {
            var branch = await resolver.ResolveAsync(id, access.Today, ct);
            BranchAccessService.EnsureScope(user, branch?.Id);
            if (locked.TerminationDate < request.HireDate
                || await database.EmployeeBranchAssignments.AnyAsync(a => a.EmployeeId == id && a.StartDate < request.HireDate, ct))
                throw new BusinessRuleException("Hire date cannot follow an existing assignment or termination date.");
            var employee = await TrackedEmployeeAsync(id, ct);
            var before = new { employee.FullName, employee.Phone, employee.JobTitle, employee.EmploymentType, employee.HireDate };
            SetProfile(employee, request);
            access.Audit(user.UserId, branch?.Id, "EmployeeUpdated", nameof(Employee), id, before,
                new { employee.FullName, employee.Phone, employee.JobTitle, employee.EmploymentType, employee.HireDate });
            await database.SaveChangesAsync(ct);
            return await DetailsAsync(employee, ct);
        }, ct);
    }

    public async Task<EmployeeDetails> ChangeStatusAsync(Guid id, ChangeEmployeeStatusRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.Manage", ct);
        if (!request.Status.HasValue || !Enum.IsDefined(request.Status.Value) || request.Reason?.Length > 2048)
            throw new BusinessRuleException("A valid employment status and optional reason up to 2048 characters are required.");
        return await EmployeeAssignmentTransaction.RunAsync(database, id, async locked =>
        {
            var branch = await resolver.ResolveAsync(id, access.Today, ct);
            BranchAccessService.EnsureScope(user, branch?.Id);
            if (locked.EmploymentStatus == request.Status) throw new BusinessRuleException("Employee already has this status.");
            if (locked.EmploymentStatus == DAL.Enums.EmploymentStatus.Resigned && request.Status != DAL.Enums.EmploymentStatus.Active)
                throw new BusinessRuleException("Reactivate ended employment before suspending it.");
            if (request.Status == DAL.Enums.EmploymentStatus.Resigned && locked.HireDate > access.Today)
                throw new BusinessRuleException("Termination date cannot precede hire date.");
            var employee = await TrackedEmployeeAsync(id, ct);
            var before = new { employee.EmploymentStatus, employee.TerminationDate };
            employee.EmploymentStatus = request.Status.Value;
            employee.TerminationDate = request.Status == DAL.Enums.EmploymentStatus.Resigned ? access.Today : null;
            if (request.Status != DAL.Enums.EmploymentStatus.Active)
            {
                await CloseSalesOwnershipAsync(id, user.UserId, request.Reason, ct);
                await subscriptions.CloseEmployeeEntitlementsAsync(id, user.UserId, request.Reason, ct);
            }
            access.Audit(user.UserId, branch?.Id, "EmployeeStatusChanged", nameof(Employee), id, before,
                new { employee.EmploymentStatus, employee.TerminationDate, Reason = request.Reason?.Trim() });
            await database.SaveChangesAsync(ct);
            return await DetailsAsync(employee, ct);
        }, ct);
    }

    private async Task<EmployeeDetails> DetailsAsync(Employee employee, CancellationToken ct) => new(employee.Id,
        employee.FullName, employee.Phone, employee.JobTitle, employee.EmploymentType.ToString(),
        employee.EmploymentStatus.ToString(), employee.HireDate, employee.TerminationDate,
        await resolver.ResolveAsync(employee.Id, access.Today, ct), employee.UserId.HasValue);
}
