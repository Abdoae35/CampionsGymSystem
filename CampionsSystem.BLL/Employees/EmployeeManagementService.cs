using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Employees;

public sealed class EmployeeManagementService(ApplicationDbContext database, ICurrentUserService current,
    OperationAccess access, EmployeeBranchResolver resolver)
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

    private async Task<EmployeeDetails> DetailsAsync(Employee employee, CancellationToken ct) => new(employee.Id,
        employee.FullName, employee.Phone, employee.JobTitle, employee.EmploymentType.ToString(),
        employee.EmploymentStatus.ToString(), employee.HireDate, employee.TerminationDate,
        await resolver.ResolveAsync(employee.Id, access.Today, ct), employee.UserId.HasValue);
}
