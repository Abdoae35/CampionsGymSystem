using CampionsSystem.DAL.Data;
using CampionsSystem.BLL.Plans;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Operations;

public sealed record SaveFreelancePlanRequest(Guid CityId, Guid CoachEmployeeId, string Name, decimal Price,
    int DurationDays, AccessMode AccessMode, int? SessionCount, bool IsActive = true, Guid? BranchId = null);

public sealed class FreelancePlanService(ApplicationDbContext db, OperationAccess access, MembershipPlanManagementService management)
{
    // Compatibility API delegates to the same branch-scoped template rules.
    public Task<Guid> SaveAsync(Guid? id, SaveFreelancePlanRequest request, CancellationToken ct = default)
        => management.SaveFreelancePlanAsync(id, request, ct);

    public async Task SetEligibilityAsync(Guid planId, Guid memberId, bool eligible, CancellationToken ct = default)
    {
        var member = await db.Members.SingleOrDefaultAsync(x => x.Id == memberId, ct)
            ?? throw new BusinessRuleException("Member not found.");
        var actor = await access.RequireAsync("FreelancePlans.ManageEligibleMembers", member.HomeBranchId, ct);
        if (!await db.MembershipPlans.AnyAsync(x => x.Id == planId && x.PlanCategory == PlanCategory.Freelance, ct))
            throw new BusinessRuleException("Freelance plan not found.");
        var row = await db.MembershipPlanEligibleMembers.SingleOrDefaultAsync(x => x.MembershipPlanId == planId && x.MemberId == memberId, ct);
        if (eligible && row is null)
        {
            row = new MembershipPlanEligibleMember { MembershipPlanId = planId, MemberId = memberId,
                CreatedByUserId = actor, CreatedAt = access.Now };
            db.Add(row);
        }
        else if (!eligible && row is not null) db.Remove(row);
        else return;
        access.Audit(actor, member.HomeBranchId, eligible ? "FreelancePlan.MemberEligible" : "FreelancePlan.MemberEligibilityRemoved",
            nameof(MembershipPlanEligibleMember), row.Id, after: new { planId, memberId, eligible });
        await db.SaveChangesAsync(ct);
    }
}
