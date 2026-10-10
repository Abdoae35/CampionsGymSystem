using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    internal Task ValidateSaleCustomerAsync(Member member, MembershipPlan plan, CancellationToken ct) => ValidateCustomerAsync(member, plan, ct);
    internal async Task<bool> MatchesAudienceAsync(Guid memberId, Guid? categoryId, CancellationToken ct)
        => categoryId == null || await db.MemberAudienceCategories.AnyAsync(x => x.MemberId == memberId
            && x.MembershipPlanCategoryId == categoryId && x.MembershipPlanCategory.IsActive && x.AssignedAt <= access.Now
            && (x.EndedAt == null || x.EndedAt > access.Now), ct);
    private async Task ValidateCustomerAsync(Member member, MembershipPlan plan, CancellationToken ct)
    {
        if (!member.IsActive || plan.PlanCategory == PlanCategory.Employee)
            throw new BusinessRuleException("Active customer Member and Normal/Freelance plan required.");
        if (!await MatchesAudienceAsync(member.Id, plan.MembershipPlanCategoryId, ct))
            throw new BusinessRuleException("Member does not match the Plan audience.");
        if (plan.PlanCategory != PlanCategory.Freelance) return;
        var coach = await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\"={plan.FreelanceCoachEmployeeId} FOR SHARE")
            .AsNoTracking().SingleOrDefaultAsync(ct);
        if (coach is not { EmploymentType: EmploymentType.Freelance, EmploymentStatus: EmploymentStatus.Active }
            || coach.HireDate > access.Today || coach.TerminationDate < access.Today)
            throw new BusinessRuleException("Freelance plan requires an active Freelance Coach.");
        if (!await db.MembershipPlanEligibleMembers.AnyAsync(x => x.MembershipPlanId == plan.Id && x.MemberId == member.Id, ct))
            throw new BusinessRuleException("Member is not eligible for this Freelance plan.");
        // Member origin and Sales assignment are deliberately not changed by this operation.
    }
}
