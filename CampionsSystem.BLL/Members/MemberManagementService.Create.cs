using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Members;

public sealed partial class MemberManagementService
{
    private async Task<T> TransactionAsync<T>(Func<Task<T>> change, CancellationToken ct)
    {
        var owned = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        var tx = db.Database.CurrentTransaction!;
        var point = "member_" + Guid.NewGuid().ToString("N");
        try
        {
            await tx.CreateSavepointAsync(point, ct);
            var result = await change();
            await tx.ReleaseSavepointAsync(point, ct);
            if (owned is not null) await owned.CommitAsync(ct);
            return result;
        }
        catch
        {
            if (owned is not null) await owned.RollbackAsync(CancellationToken.None);
            else await tx.RollbackToSavepointAsync(point, CancellationToken.None);
            db.ChangeTracker.Clear(); throw;
        }
        finally { if (owned is not null) await owned.DisposeAsync(); }
    }
    private void ValidateProfile(UpdateMemberRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 256
            || string.IsNullOrWhiteSpace(request.PrimaryPhone) || request.PrimaryPhone.Length > 64
            || request.SecondaryPhone?.Length > 64 || request.Gender?.Length > 32 || request.Notes?.Length > 4096
            || request.DateOfBirth > access.Today)
            throw new BusinessRuleException("Invalid member profile.");
    }
    private static void Profile(Member m, UpdateMemberRequest r)
    {
        m.FullName = r.FullName.Trim(); m.PrimaryPhone = r.PrimaryPhone.Trim();
        m.SecondaryPhone = string.IsNullOrWhiteSpace(r.SecondaryPhone) ? null : r.SecondaryPhone.Trim();
        m.DateOfBirth = r.DateOfBirth; m.Gender = r.Gender?.Trim(); m.Notes = r.Notes?.Trim();
    }
    private async Task<Employee> EligibleOwnerAsync(Guid id, CancellationToken ct)
    {
        var employee = await db.Employees.AsNoTracking().Include(e => e.User).SingleOrDefaultAsync(e => e.Id == id, ct);
        if (employee?.UserId is not Guid userId || employee.User is not { IsActive: true }
            || (employee.User.LockoutEnabled && employee.User.LockoutEnd > access.Now)
            || employee.EmploymentStatus != EmploymentStatus.Active || employee.HireDate > access.Today
            || employee.TerminationDate < access.Today
            || !(await permissions.GetUserPermissionsAsync(userId, ct)).Contains("Members.OwnSalesCustomers"))
            throw new BusinessRuleException("An active employee/account with Members.OwnSalesCustomers is required.");
        return employee;
    }
    private async Task EnsurePhoneAvailableAsync(string display, string normalized, Guid branch, Guid? except, CancellationToken ct)
    {
        if (await db.Members.AnyAsync(m => m.Id != except && (m.PrimaryPhoneNormalized == normalized
            || (m.PrimaryPhoneNormalized == null && m.HomeBranchId == branch && m.PrimaryPhone == display)), ct))
            throw new BusinessRuleException("A member with this primary phone already exists.");
        // Compare safely derivable legacy phones without rewriting their stored display or guessing a country.
        var legacy = await db.Database.SqlQuery<Guid>($"""
            SELECT m."Id" AS "Value" FROM "Members" m JOIN "GymBranches" b ON b."Id" = m."HomeBranchId"
            WHERE m."PrimaryPhoneNormalized" IS NULL AND ({except}::uuid IS NULL OR m."Id" <> {except})
            AND CASE
                WHEN regexp_replace(btrim(m."PrimaryPhone"), '[[:space:]().-]', '', 'g') ~ '^[+][1-9][0-9]*$'
                    THEN regexp_replace(btrim(m."PrimaryPhone"), '[[:space:]().-]', '', 'g')
                WHEN b."CountryCallingCode" IS NOT NULL AND regexp_replace(btrim(m."PrimaryPhone"), '[[:space:]().-]', '', 'g') ~ '^[0-9]+$'
                    THEN b."CountryCallingCode" || regexp_replace(regexp_replace(btrim(m."PrimaryPhone"), '[[:space:]().-]', '', 'g'), '^0', '')
                ELSE NULL END = {normalized}
            LIMIT 1
            """).ToListAsync(ct);
        if (legacy.Count != 0) throw new BusinessRuleException("An existing legacy member phone requires reconciliation.");
    }
    private MemberSalesAssignment Assignment(Guid member, Guid employee, Guid actor, string? reason) => new()
    {
        MemberId = member, SalesEmployeeId = employee, AssignmentType = AssignmentType.Sales,
        AssignedAt = access.Now, CreatedAt = access.Now, AssignedByUserId = actor, Reason = reason?.Trim()
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
        return await TransactionAsync(async () =>
        {
            // Allocation locks this branch until commit and rechecks IsActive in the database.
            var code = await new BranchClientCodeGenerator(db).NextAsync(request.TargetRegistrationBranchId, ct);
            var branch = await db.GymBranches.AsNoTracking().SingleAsync(b => b.Id == request.TargetRegistrationBranchId, ct);
            var normalized = PhoneNormalizer.Normalize(request.PrimaryPhone, branch.CountryCallingCode);
            await EnsurePhoneAvailableAsync(request.PrimaryPhone.Trim(), normalized, branch.Id, null, ct);
            if (request.CreationType == MemberCreationType.FreelanceHouse
                && !await db.MembershipPlans.AnyAsync(p => p.Id == request.FreelancePlanId && p.IsActive
                    && p.PlanCategory == PlanCategory.Freelance && p.CityId == branch.CityId
                    && p.FreelanceCoachEmployee!.EmploymentStatus == EmploymentStatus.Active
                    && p.FreelanceCoachEmployee.HireDate <= access.Today
                    && (p.FreelanceCoachEmployee.TerminationDate == null || p.FreelanceCoachEmployee.TerminationDate >= access.Today), ct))
                throw new BusinessRuleException("An active Freelance plan/coach in the registration city is required.");
            var member = new Member { HomeBranchId = branch.Id, ClientCode = code,
                MemberCode = "M-" + Guid.NewGuid().ToString("N"), CreatedByUserId = user.UserId,
                CreatedAt = access.Now, PrimaryPhoneNormalized = normalized };
            Profile(member, request);
            db.Members.Add(member);
            if (owner.HasValue)
            {
                db.MemberSalesAssignments.Add(Assignment(member.Id, owner.Value, user.UserId, "Registration"));
                access.Audit(user.UserId, branch.Id, "Member.SalesAssigned", nameof(Member), member.Id, after: new { SalesEmployeeId = owner });
            }
            if (request.FreelancePlanId.HasValue)
            {
                db.MembershipPlanEligibleMembers.Add(new() { MemberId = member.Id, MembershipPlanId = request.FreelancePlanId.Value,
                    CreatedByUserId = user.UserId, CreatedAt = access.Now });
                access.Audit(user.UserId, branch.Id, "FreelancePlan.MemberEligible", nameof(Member), member.Id,
                    after: new { request.FreelancePlanId });
            }
            access.Audit(user.UserId, branch.Id, "Member.Created", nameof(Member), member.Id,
                after: new { member.FullName, member.PrimaryPhone, member.PrimaryPhoneNormalized, member.ClientCode,
                    RegistrationBranchId = branch.Id, request.CreationType, SalesEmployeeId = owner });
            await db.SaveChangesAsync(ct);
            return await DetailsAsync(member, ct);
        }, ct);
    }
}
