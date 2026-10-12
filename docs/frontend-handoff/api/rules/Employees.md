# Employees rule/validation evidence

Baseline source, not new Private drafts. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Employees/EmployeeContracts.cs

Lines 14–19

```csharp

}

public sealed record EmployeeDetails(Guid Id, string FullName, string Phone, string? JobTitle,
    string EmploymentType, string Status, DateOnly HireDate, DateOnly? TerminationDate,
    BranchContext? CurrentBranch, bool HasSystemAccount);
public sealed record EmployeeListResult(IReadOnlyList<EmployeeDetails> Items, int TotalCount, int Page, int PageSize);

```

Lines 28–33

```csharp

    [EnumDataType(typeof(DAL.Enums.EmploymentType))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<DAL.Enums.EmploymentType>))]
    public DAL.Enums.EmploymentType EmploymentType { get; set; }
    public DateOnly HireDate { get; set; }
}


```

## CampionsSystem.BLL/Employees/EmployeeManagementService.SalesOwnership.cs

Lines 26–31

```csharp

            await database.Entry(assignment).ReloadAsync(ct);
            if (assignment.EndedAt.HasValue) continue;
            if (assignment.AssignedAt > access.Now)
                throw new BusinessRuleException("Reconcile future-dated Sales assignments before ending employment.");
            assignment.EndedAt = access.Now;
            var branch = await database.Members.Where(m => m.Id == assignment.MemberId).Select(m => m.HomeBranchId).SingleAsync(ct);

```

Lines 38–43

```csharp

            await database.Entry(assignment).ReloadAsync(ct);
            if (assignment.EndedAt.HasValue) continue;
            if (assignment.AssignedAt > access.Now)
                throw new BusinessRuleException("Reconcile future-dated Lead assignments before ending employment.");
            assignment.EndedAt = access.Now;
            var branch = await database.Leads.Where(l => l.Id == assignment.LeadId).Select(l => l.BranchId).SingleAsync(ct);

```

## CampionsSystem.BLL/Employees/EmployeeManagementService.cs

Lines 10–18

```csharp

public sealed partial class EmployeeManagementService(ApplicationDbContext database, ICurrentUserService current,
    OperationAccess access, EmployeeBranchResolver resolver, EmployeeBranchAssignmentService assignments, CampionsSystem.BLL.Subscriptions.SubscriptionService subscriptions)
{
    private async Task<CurrentUserContext> RequireAsync(string permission, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        return user;
    }

```

Lines 23–33

```csharp

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

```

Lines 39–52

```csharp


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

```

Lines 57–63

```csharp

        if (request.EmploymentType.HasValue) query = query.Where(e => e.EmploymentType == request.EmploymentType);
        var count = await query.CountAsync(ct);
        var assignments = CurrentAssignments();
        var rows = await query.OrderBy(e => e.FullName).ThenBy(e => e.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(e => new
            {

```

Lines 74–81

```csharp


    public async Task<EmployeeDetails> GetAsync(Guid id, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.View", ct);
        var employee = await ScopedEmployees(user, null).SingleOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new BranchNotFoundException("Employee not found in accessible scope.");
        return await DetailsAsync(employee, ct);
    }

```

Lines 85–90

```csharp

        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 256
            || string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Length > 64
            || request.JobTitle?.Length > 256 || request.HireDate == default || !Enum.IsDefined(request.EmploymentType))
            throw new BusinessRuleException("Name, phone, hire date and a valid employment type are required.");
    }


```

Lines 97–104

```csharp


    public async Task<EmployeeDetails> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Employees.Manage", ct);
        ValidateProfile(request);
        BranchAccessService.EnsureScope(user, request.InitialBranchId);
        var owned = database.Database.CurrentTransaction is null ? await database.Database.BeginTransactionAsync(ct) : null;
        var transaction = database.Database.CurrentTransaction!;

```

Lines 140–153

```csharp


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

```

Lines 161–177

```csharp


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

```
