# Members rule/validation evidence

Completed source; deployment/configuration prerequisites are separate. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Members/MemberContracts.cs

Lines 28–33

```csharp

public sealed record MemberEmployeeLink(Guid Id, string FullName);
public sealed record MemberDetails(Guid Id, long? ClientCode, string MemberCode, string FullName,
    string PrimaryPhone, string? PrimaryPhoneNormalized, string? SecondaryPhone, bool IsPrimaryPhoneVerified,
    DateOnly? DateOfBirth, string? Gender, string? Notes, bool IsActive, BranchContext RegistrationBranch,
    DateTimeOffset CreatedAt, SalesAssignmentDetails? CurrentSalesOwner, MemberEmployeeLink? Employee,
    IReadOnlyList<EligibleFreelancePlan> EligibleFreelancePlans)

```

Lines 42–47

```csharp

    [Required, MaxLength(256)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string PrimaryPhone { get; set; } = string.Empty;
    [MaxLength(64)] public string? SecondaryPhone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    [MaxLength(32)] public string? Gender { get; set; }
    [MaxLength(4096)] public string? Notes { get; set; }

```

## CampionsSystem.BLL/Members/MemberManagementService.Create.cs

Lines 37–42

```csharp

            || string.IsNullOrWhiteSpace(request.PrimaryPhone) || request.PrimaryPhone.Length > 64
            || request.SecondaryPhone?.Length > 64 || request.Gender?.Length > 32 || request.Notes?.Length > 4096
            || request.DateOfBirth > access.Today)
            throw new BusinessRuleException("Invalid member profile.");
    }
    private static void Profile(Member m, UpdateMemberRequest r)

```

Lines 56–61

```csharp

            || employee.EmploymentStatus != EmploymentStatus.Active || employee.HireDate > access.Today
            || employee.TerminationDate < access.Today
            || !(await permissions.GetUserPermissionsAsync(userId, ct)).Contains("Members.OwnSalesCustomers"))
            throw new BusinessRuleException("An active employee/account with Members.OwnSalesCustomers is required.");
        return employee;
    }

```

Lines 63–68

```csharp

    {
        if (await db.Members.AnyAsync(m => m.Id != except && (m.PrimaryPhoneNormalized == normalized
            || (m.PrimaryPhoneNormalized == null && m.HomeBranchId == branch && m.PrimaryPhone == display)), ct))
            throw new BusinessRuleException("A member with this primary phone already exists.");
        // Compare safely derivable legacy phones without rewriting their stored display or guessing a country.
        var legacy = await db.Database.SqlQuery<Guid>($"""

```

Lines 76–81

```csharp

                ELSE NULL END = {normalized}
            LIMIT 1
            """).ToListAsync(ct);
        if (legacy.Count != 0) throw new BusinessRuleException("An existing legacy member phone requires reconciliation.");
    }
    private MemberSalesAssignment Assignment(Guid member, Guid employee, Guid actor, string? reason) => new()

```

Lines 85–122

```csharp

    };
    public async Task<MemberDetails> CreateMemberAsync(CreateMemberRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.Create", ct);
        ValidateProfile(request);
        if (!request.CreationType.HasValue || !Enum.IsDefined(request.CreationType.Value)
            || request.TargetRegistrationBranchId == Guid.Empty)
            throw new BusinessRuleException("Registration branch and explicit creation type are required.");
        Guid? owner = null;
        switch (request.CreationType.Value)
        {
            case MemberCreationType.NormalSales:
                if (!user.Permissions.Contains("Members.OwnSalesCustomers") || user.EmployeeId is null)
                    throw new UnauthorizedAccessException();
                if (request.SalesEmployeeId.HasValue || request.FreelancePlanId.HasValue)
                    throw new BusinessRuleException("NormalSales resolves the current employee; explicit ownership/plan is not accepted.");
                owner = (await EligibleOwnerAsync(user.EmployeeId.Value, ct)).Id;
                break;
            case MemberCreationType.ManagementCreated:
                if (!user.Permissions.Contains("Members.AssignSales")) throw new UnauthorizedAccessException();
                if (request.FreelancePlanId.HasValue || !request.SalesEmployeeId.HasValue)
                    throw new BusinessRuleException("ManagementCreated requires an explicit eligible Sales owner; use FreelanceHouse for Freelance eligibility.");
                owner = (await EligibleOwnerAsync(request.SalesEmployeeId.Value, ct)).Id;
                break;
            case MemberCreationType.FreelanceHouse:
                if (!user.Permissions.Contains("FreelancePlans.ManageEligibleMembers")) throw new UnauthorizedAccessException();
                if (request.SalesEmployeeId.HasValue || !request.FreelancePlanId.HasValue)
                    throw new BusinessRuleException("FreelanceHouse requires a Freelance plan and no Sales owner.");
                break;
        }
        if (request.CreationType == MemberCreationType.NormalSales && request.TargetRegistrationBranchId != user.CurrentBranchId)
        {
            if (user.CurrentBranchId is null || !user.Permissions.Contains("Members.CreateCrossBranch"))
                throw new UnauthorizedAccessException();
        }
        else BranchAccessService.EnsureScope(user, request.TargetRegistrationBranchId);
        return await CreateCoreAsync(request,user,owner,null,null,ct);
    }

```

Lines 141–150

```csharp

                // Registration also adds plan eligibility: it must not bypass plan administration scope.
                var plan = await db.MembershipPlans.FromSqlInterpolated($"SELECT * FROM \"MembershipPlans\" WHERE \"Id\"={request.FreelancePlanId!.Value} FOR SHARE")
                    .AsNoTracking().SingleOrDefaultAsync(ct)
                    ?? throw new BusinessRuleException("Freelance plan not found.");
                if (!user.Permissions.Contains("Branches.AccessAll") && (plan.BranchId is null
                    || plan.BranchId != user.CurrentBranchId
                    || !await db.GymBranches.AnyAsync(b => b.Id == plan.BranchId && b.IsActive, ct)))
                    throw new UnauthorizedAccessException();
            }
            // Allocation locks this branch until commit and rechecks IsActive in the database.

```

Lines 154–159

```csharp

            await PhoneIdentity.LockAsync(db,normalized,ct);
            var existingLead = await PhoneIdentity.FindLeadAsync(db,normalized,ct);
            if (existingLead.HasValue && existingLead != sourceLeadId)
                throw new BusinessRuleException("An existing Lead uses this phone; use CRM lookup and conversion to preserve ownership.");
            await EnsurePhoneAvailableAsync(request.PrimaryPhone.Trim(), normalized, branch.Id, null, ct);
            if (request.CreationType == MemberCreationType.FreelanceHouse

```

Lines 163–168

```csharp

                    && p.FreelanceCoachEmployee.EmploymentStatus == EmploymentStatus.Active
                    && p.FreelanceCoachEmployee.HireDate <= access.Today
                    && (p.FreelanceCoachEmployee.TerminationDate == null || p.FreelanceCoachEmployee.TerminationDate >= access.Today), ct))
                throw new BusinessRuleException("An active Freelance plan/coach in the registration city is required.");
            var member = new Member { HomeBranchId = branch.Id, ClientCode = code,
                MemberCode = "M-" + Guid.NewGuid().ToString("N"), CreatedByUserId = user.UserId,

```

## CampionsSystem.BLL/Members/MemberManagementService.Sales.cs

Lines 10–17

```csharp

{
    private async Task RequireVisibleAsync(Guid id, string permission, CancellationToken ct)
    {
        var user = await RequireAsync(permission, ct);
        if (!await Scoped(user).AnyAsync(m => m.Id == id, ct))
            throw new BranchNotFoundException("Member not found in accessible scope.");
    }
    public async Task<SalesAssignmentDetails?> GetCurrentSalesAssignmentAsync(Guid id, CancellationToken ct = default)

```

Lines 24–29

```csharp

    {
        await RequireVisibleAsync(id, "Members.ViewSalesHistory", ct);
        return await db.MemberSalesAssignments.AsNoTracking().Where(a => a.MemberId == id)
            .OrderByDescending(a => a.AssignedAt).ThenByDescending(a => a.CreatedAt).ThenBy(a => a.Id)
            .Select(a => new SalesAssignmentHistoryItem(a.Id, a.SalesEmployeeId,
                a.SalesEmployee == null ? null : a.SalesEmployee.FullName, a.AssignmentType.ToString(),

```

Lines 38–50

```csharp

    private void CloseAssignment(MemberSalesAssignment assignment)
    {
        // Preserve chronological integrity even if a legacy row starts in the future.
        if (assignment.AssignedAt > access.Now) throw new BusinessRuleException("Reconcile the future-dated current assignment first.");
        assignment.EndedAt = access.Now;
    }
    public async Task<MemberDetails> AssignSalesAsync(Guid id, SalesAssignmentRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.AssignSales", ct);
        if (request.SalesEmployeeId == Guid.Empty || request.Reason?.Length > 2048)
            throw new BusinessRuleException("Sales employee and an optional reason up to 2048 characters are required.");
        return await TransactionAsync(async () =>
        {

```

Lines 52–57

```csharp

            var employee = await EligibleOwnerAsync(request.SalesEmployeeId, ct);
            var old = await CurrentAssignmentAsync(id, ct);
            if (old?.AssignmentType == AssignmentType.Sales && old.SalesEmployeeId == employee.Id)
                throw new BusinessRuleException("Member already belongs to this Sales employee.");
            var previous = old is null ? null : new { old.Id, old.SalesEmployeeId, old.AssignedAt, old.AssignmentType };
            if (old is not null)

```

Lines 71–82

```csharp

    }
    public async Task<MemberDetails> ReturnMemberToHouseAsync(Guid id, ReturnToHouseRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.AssignSales", ct);
        if (request.Reason?.Length > 2048) throw new BusinessRuleException("Reason must not exceed 2048 characters.");
        return await TransactionAsync(async () =>
        {
            var member = await LockedMemberAsync(id, user, ct);
            var old = await CurrentAssignmentAsync(id, ct);
            if (old is null) throw new BusinessRuleException("Member is already House.");
            var before = new { old.Id, old.SalesEmployeeId, old.AssignedAt, old.AssignmentType };
            CloseAssignment(old);

```

## CampionsSystem.BLL/Members/MemberManagementService.Update.cs

Lines 12–18

```csharp

    {
        var rows = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\" = {id} FOR UPDATE")
            .AsNoTracking().ToListAsync(ct);
        var snapshot = rows.SingleOrDefault() ?? throw new BranchNotFoundException("Member not found.");
        BranchAccessService.EnsureScope(user, snapshot.HomeBranchId);
        var existing = db.ChangeTracker.Entries<Member>().SingleOrDefault(e => e.Entity.Id == id);
        if (existing is not null) { await existing.ReloadAsync(ct); return existing.Entity; }

```

Lines 24–29

```csharp

    };
    public async Task<MemberDetails> UpdateMemberAsync(Guid id, UpdateMemberRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.Edit", ct);
        ValidateProfile(request);
        return await TransactionAsync(async () =>

```

Lines 32–43

```csharp

            var before = ProfileSnapshot(member);
            if (request.PrimaryPhone.Trim() != member.PrimaryPhone)
            {
                if (!user.Permissions.Contains("Members.ChangePrimaryPhone")) throw new UnauthorizedAccessException();
                var country = await db.GymBranches.Where(b => b.Id == member.HomeBranchId).Select(b => b.CountryCallingCode).SingleAsync(ct);
                var normalized = PhoneNormalizer.Normalize(request.PrimaryPhone, country);
                await PhoneIdentity.LockAsync(db,normalized,ct);
                var leadId = await PhoneIdentity.FindLeadAsync(db,normalized,ct);
                if (leadId.HasValue && !await db.Leads.AnyAsync(l=>l.Id==leadId && l.ConvertedMemberId==id,ct))
                    throw new BusinessRuleException("An existing Lead uses this phone; reconcile through CRM first.");
                await EnsurePhoneAvailableAsync(request.PrimaryPhone.Trim(), normalized, member.HomeBranchId, id, ct);
                var phoneBefore = new { member.PrimaryPhone, member.PrimaryPhoneNormalized, member.IsPrimaryPhoneVerified, member.PrimaryPhoneVerifiedAt };

```

## CampionsSystem.BLL/Members/BranchClientCodeGenerator.cs

Lines 9–14

```csharp

    // The caller's transaction keeps allocation atomic with member/audit/ownership writes.
    public async Task<long> NextAsync(Guid branchId, CancellationToken ct = default)
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("ClientCode allocation requires a transaction.");
        var values = await db.Database.SqlQuery<long>($"""
            UPDATE "GymBranches" SET "LastMemberClientCode" = "LastMemberClientCode" + 1

```

Lines 16–21

```csharp

            RETURNING "LastMemberClientCode" AS "Value"
            """).ToListAsync(ct);
        return values.SingleOrDefault() is > 0 and var code ? code
            : throw new BusinessRuleException("Active registration branch with available ClientCode capacity required.");
    }
}

```

## CampionsSystem.BLL/Members/MemberManagementService.cs

Lines 11–28

```csharp

public sealed partial class MemberManagementService(ApplicationDbContext db, ICurrentUserService current,
    OperationAccess access, IPermissionService permissions)
{
    private async Task<CurrentUserContext> RequireAsync(string permission, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        return user;
    }
    private IQueryable<Member> Scoped(CurrentUserContext user, Guid? branch = null)
    {
        if (branch.HasValue) BranchAccessService.EnsureScope(user, branch);
        var query = db.Members.AsNoTracking();
        if (!user.Permissions.Contains(BranchManagementService.AccessAllPermission) || branch.HasValue)
        {
            var id = branch ?? user.CurrentBranchId ?? throw new UnauthorizedAccessException();
            query = query.Where(m => m.HomeBranchId == id);
        }

```

Lines 33–41

```csharp


    public async Task<MemberListResult> GetMembersAsync(MemberListRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.View", ct);
        if (request.Page < 1 || request.PageSize is < 1 or > 100 || request.Search?.Length > 256
            || (long)(request.Page - 1) * request.PageSize > int.MaxValue || request.ClientCode <= 0)
            throw new BusinessRuleException("Invalid member filters or pagination.");
        var query = Scoped(user, request.RegistrationBranchId);
        if (request.IsActive.HasValue) query = query.Where(m => m.IsActive == request.IsActive);

```

Lines 45–51

```csharp

        if (request.HouseOnly.HasValue) query = query.Where(m => !sales.Any(a => a.MemberId == m.Id) == request.HouseOnly);
        query = MemberSearch.Apply(query, request.Search);
        var count = await query.CountAsync(ct);
        var items = await query.OrderBy(m => m.FullName).ThenBy(m => m.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(m => new MemberSummary(m.Id, m.ClientCode, m.MemberCode, m.FullName, m.PrimaryPhone,
                m.IsActive, new BranchContext(m.HomeBranchId, m.HomeBranch.Name), sales.Where(a => a.MemberId == m.Id)

```

Lines 55–62

```csharp

    }
    public async Task<MemberDetails> GetMemberByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.View", ct);
        var member = await Scoped(user).Include(m => m.HomeBranch).SingleOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new BranchNotFoundException("Member not found in accessible scope.");
        return await DetailsAsync(member, ct);
    }

```

Lines 69–74

```csharp

            .Select(e => new MemberEmployeeLink(e.Id, e.FullName)).SingleOrDefaultAsync(ct);
        var eligible = await db.MembershipPlanEligibleMembers.AsNoTracking()
            .Where(e => e.MemberId == member.Id && e.MembershipPlan.PlanCategory == PlanCategory.Freelance)
            .OrderBy(e => e.MembershipPlan.Name).Select(e => new EligibleFreelancePlan(e.MembershipPlanId,
                e.MembershipPlan.Name, e.MembershipPlan.FreelanceCoachEmployee!.FullName)).ToListAsync(ct);
        var registration = await db.GymBranches.AsNoTracking().Where(b => b.Id == member.HomeBranchId)

```

## CampionsSystem.BLL/Members/PhoneNormalizer.cs

Lines 9–24

```csharp

    public static string Normalize(string input, string? countryCallingCode)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length > 64)
            throw new BusinessRuleException("A phone number up to 64 characters is required.");
        var compact = Regex.Replace(input.Trim(), @"[\s().-]", "");
        if (!compact.StartsWith('+'))
        {
            if (countryCallingCode is null || !Regex.IsMatch(countryCallingCode, @"^[+][1-9][0-9]{0,2}$"))
                throw new BusinessRuleException("Configure the registration branch country calling code before entering local phones.");
            if (!Regex.IsMatch(compact, @"^[0-9]+$")) throw new BusinessRuleException("Invalid phone format.");
            if (compact.StartsWith('0')) compact = compact[1..];
            compact = countryCallingCode + compact;
        }
        if (!Regex.IsMatch(compact, @"^[+][1-9][0-9]{6,14}$")) throw new BusinessRuleException("Invalid international phone format.");
        return compact;
    }

```

## CampionsSystem.BLL/Members/MemberManagementService.LeadConversion.cs

Lines 9–31

```csharp

    // Operational conversion is a narrow capability; it does not grant Lead browsing or assignment.
    internal async Task<CurrentUserContext> RequireLeadConversionAccessAsync(Guid targetBranchId, bool freelanceHouse, CancellationToken ct)
    {
        var user = await RequireAsync("Members.Create", ct);
        if (!user.Permissions.Contains("Leads.Convert") || user.EmployeeId is null)
            throw new UnauthorizedAccessException();
        if (!await db.Employees.AnyAsync(e => e.Id == user.EmployeeId && e.UserId == user.UserId
            && e.EmploymentStatus == EmploymentStatus.Active && e.HireDate <= access.Today
            && (e.TerminationDate == null || e.TerminationDate >= access.Today), ct))
            throw new UnauthorizedAccessException();
        if (freelanceHouse)
        {
            if (!user.Permissions.Contains("FreelancePlans.ManageEligibleMembers")) throw new UnauthorizedAccessException();
            BranchAccessService.EnsureScope(user, targetBranchId);
        }
        else if (targetBranchId != user.CurrentBranchId)
        {
            if (user.CurrentBranchId is null || !user.Permissions.Contains("Members.CreateCrossBranch"))
                throw new UnauthorizedAccessException();
        }
        else BranchAccessService.EnsureScope(user, targetBranchId);
        return user;
    }

```

Lines 34–57

```csharp

    internal async Task<MemberDetails> CreateMemberFromLeadAsync(Guid leadId,CreateMemberRequest r,CancellationToken ct)
    {
        var user=await RequireLeadConversionAccessAsync(r.TargetRegistrationBranchId,r.CreationType==MemberCreationType.FreelanceHouse,ct);
        if(db.Database.CurrentTransaction is null)throw new UnauthorizedAccessException();
        var lead=await db.Leads.FromSqlInterpolated($"SELECT * FROM \"Leads\" WHERE \"Id\"={leadId} FOR UPDATE").SingleOrDefaultAsync(ct)
            ??throw new BusinessRuleException("Lead not found.");
        await db.Entry(lead).ReloadAsync(ct);
        if(lead.Status is LeadStatus.Converted or LeadStatus.Lost or LeadStatus.Inactive||lead.ConvertedMemberId.HasValue)
            throw new BusinessRuleException("An active, unconverted Lead is required.");
        var assignment=await db.LeadAssignments.AsNoTracking().SingleOrDefaultAsync(a=>a.LeadId==leadId&&a.EndedAt==null,ct);
        ValidateProfile(r);
        if(r.SalesEmployeeId.HasValue||r.TargetRegistrationBranchId==Guid.Empty)throw new BusinessRuleException("Conversion resolves ownership from the Lead.");
        Guid? owner=null;
        if(r.CreationType==MemberCreationType.FreelanceHouse)
        {
            if(!user.Permissions.Contains("FreelancePlans.ManageEligibleMembers")||!r.FreelancePlanId.HasValue)throw new UnauthorizedAccessException();
            BranchAccessService.EnsureScope(user,r.TargetRegistrationBranchId);
        }
        else
        {
            if(r.CreationType!=MemberCreationType.NormalSales||r.FreelancePlanId.HasValue||assignment is null)
                throw new BusinessRuleException("Assign a Sales owner before normal conversion, or use explicit FreelanceHouse.");
            owner=(await EligibleOwnerAsync(assignment.SalesEmployeeId,ct)).Id;
        }

```

Lines 59–64

```csharp

        var canonical=lead.PhoneNormalized??PhoneNormalizer.Normalize(lead.Phone,code);
        // A destination country's calling code must not reinterpret the prospect's persisted number.
        var input=r.PrimaryPhone.Trim()==lead.Phone?canonical:PhoneNormalizer.Normalize(r.PrimaryPhone,code);
        if(input!=canonical)throw new BusinessRuleException("Update the Lead phone explicitly before conversion.");
        return await CreateCoreAsync(r,user,owner,leadId,canonical,ct);
    }

```

## CampionsSystem.BLL/Members/MemberManagementService.Status.cs

Lines 8–19

```csharp

{
    public async Task<MemberDetails> ChangeMemberStatusAsync(Guid id, ChangeMemberStatusRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.ChangeStatus", ct);
        if (!request.IsActive.HasValue || request.Reason?.Length > 2048)
            throw new BusinessRuleException("An active/inactive status and optional reason up to 2048 characters are required.");
        return await TransactionAsync(async () =>
        {
            var member = await LockedMemberAsync(id, user, ct);
            if (member.IsActive == request.IsActive) throw new BusinessRuleException("Member already has this status.");
            var before = new { member.IsActive };
            member.IsActive = request.IsActive.Value;

```
