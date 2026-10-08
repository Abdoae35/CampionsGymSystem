using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Members;

public sealed partial class MemberManagementService(ApplicationDbContext db, ICurrentUserService current)
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
        return query;
    }
    private IQueryable<MemberSalesAssignment> Sales() => db.MemberSalesAssignments.AsNoTracking()
        .Where(a => a.EndedAt == null && a.AssignmentType == AssignmentType.Sales);

    public async Task<MemberListResult> GetMembersAsync(MemberListRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.View", ct);
        if (request.Page < 1 || request.PageSize is < 1 or > 100 || request.Search?.Length > 256
            || (long)(request.Page - 1) * request.PageSize > int.MaxValue || request.ClientCode <= 0)
            throw new BusinessRuleException("Invalid member filters or pagination.");
        var query = Scoped(user, request.RegistrationBranchId);
        if (request.IsActive.HasValue) query = query.Where(m => m.IsActive == request.IsActive);
        if (request.ClientCode.HasValue) query = query.Where(m => m.ClientCode == request.ClientCode);
        var sales = Sales();
        if (request.SalesEmployeeId.HasValue) query = query.Where(m => sales.Any(a => a.MemberId == m.Id && a.SalesEmployeeId == request.SalesEmployeeId));
        if (request.HouseOnly.HasValue) query = query.Where(m => !sales.Any(a => a.MemberId == m.Id) == request.HouseOnly);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            long? code = long.TryParse(search, out var parsed) && parsed > 0 ? parsed : null;
            query = query.Where(m => m.FullName.ToLower().Contains(search) || m.PrimaryPhone.Contains(search)
                || (m.PrimaryPhoneNormalized != null && m.PrimaryPhoneNormalized.Contains(search))
                || m.MemberCode.ToLower().Contains(search) || (code != null && m.ClientCode == code));
        }
        var count = await query.CountAsync(ct);
        var items = await query.OrderBy(m => m.FullName).ThenBy(m => m.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(m => new MemberSummary(m.Id, m.ClientCode, m.MemberCode, m.FullName, m.PrimaryPhone,
                m.IsActive, new BranchContext(m.HomeBranchId, m.HomeBranch.Name), sales.Where(a => a.MemberId == m.Id)
                    .Select(a => new SalesAssignmentDetails(a.Id, a.SalesEmployeeId!.Value, a.SalesEmployee!.FullName,
                        a.AssignedAt, a.EndedAt, a.Reason)).FirstOrDefault())).ToListAsync(ct);
        return new(items, count, request.Page, request.PageSize);
    }
    public async Task<MemberDetails> GetMemberByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.View", ct);
        var member = await Scoped(user).Include(m => m.HomeBranch).SingleOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new BranchNotFoundException("Member not found in accessible scope.");
        return await DetailsAsync(member, ct);
    }
    private async Task<MemberDetails> DetailsAsync(Member member, CancellationToken ct)
    {
        var owner = await Sales().Where(a => a.MemberId == member.Id)
            .Select(a => new SalesAssignmentDetails(a.Id, a.SalesEmployeeId!.Value, a.SalesEmployee!.FullName,
                a.AssignedAt, a.EndedAt, a.Reason)).SingleOrDefaultAsync(ct);
        var employee = await db.Employees.AsNoTracking().Where(e => e.MemberId == member.Id)
            .Select(e => new MemberEmployeeLink(e.Id, e.FullName)).SingleOrDefaultAsync(ct);
        var eligible = await db.MembershipPlanEligibleMembers.AsNoTracking()
            .Where(e => e.MemberId == member.Id && e.MembershipPlan.PlanCategory == PlanCategory.Freelance)
            .OrderBy(e => e.MembershipPlan.Name).Select(e => new EligibleFreelancePlan(e.MembershipPlanId,
                e.MembershipPlan.Name, e.MembershipPlan.FreelanceCoachEmployee!.FullName)).ToListAsync(ct);
        return new(member.Id, member.ClientCode, member.MemberCode, member.FullName, member.PrimaryPhone,
            member.PrimaryPhoneNormalized, member.SecondaryPhone, member.IsPrimaryPhoneVerified, member.DateOfBirth, member.Gender,
            member.Notes, member.IsActive, new(member.HomeBranchId, member.HomeBranch.Name), member.CreatedAt,
            owner, employee, eligible);
    }
}
