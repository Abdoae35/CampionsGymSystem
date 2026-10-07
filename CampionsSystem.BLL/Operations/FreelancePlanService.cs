using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Operations;

public sealed record SaveFreelancePlanRequest(Guid CityId, Guid CoachEmployeeId, string Name, decimal Price,
    int DurationDays, AccessMode AccessMode, int? SessionCount, bool IsActive = true);

public sealed class FreelancePlanService(ApplicationDbContext db, OperationAccess access)
{
    public async Task<Guid> SaveAsync(Guid? id, SaveFreelancePlanRequest request, CancellationToken ct = default)
    {
        var actor = await access.RequireAsync("FreelancePlans.Manage", null, ct);
        OperationAccess.Money(request.Price);
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 256 || request.DurationDays <= 0
            || !(request.AccessMode == AccessMode.Unlimited && request.SessionCount is null
                 || request.AccessMode == AccessMode.SessionBased && request.SessionCount > 0))
            throw new BusinessRuleException("Invalid plan name, duration or session configuration.");
        if (!await db.Cities.AnyAsync(x => x.Id == request.CityId, ct)) throw new BusinessRuleException("City not found.");
        if (!await db.Employees.AnyAsync(x => x.Id == request.CoachEmployeeId && x.EmploymentType == EmploymentType.Freelance
            && x.EmploymentStatus == EmploymentStatus.Active && x.HireDate <= access.Today
            && (x.TerminationDate == null || x.TerminationDate >= access.Today), ct))
            throw new BusinessRuleException("Plan requires an active Freelance Employee.");
        var plan = id.HasValue ? await db.MembershipPlans.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new BusinessRuleException("Plan not found.") : new MembershipPlan();
        if (id.HasValue && plan.PlanCategory != PlanCategory.Freelance)
            throw new BusinessRuleException("A normal plan cannot be converted by this operation.");
        var before = id.HasValue ? new { plan.Name, plan.Price, plan.FreelanceCoachEmployeeId, plan.IsActive } : null;
        plan.CityId = request.CityId; plan.FreelanceCoachEmployeeId = request.CoachEmployeeId;
        plan.PlanCategory = PlanCategory.Freelance; plan.Name = request.Name.Trim(); plan.Price = request.Price;
        plan.DurationDays = request.DurationDays; plan.AccessMode = request.AccessMode;
        plan.SessionCount = request.SessionCount; plan.IsActive = request.IsActive;
        if (!id.HasValue) db.Add(plan);
        access.Audit(actor, null, id.HasValue ? "FreelancePlan.Edited" : "FreelancePlan.Created", nameof(MembershipPlan), plan.Id,
            before, new { plan.Name, plan.Price, plan.FreelanceCoachEmployeeId, plan.IsActive });
        await db.SaveChangesAsync(ct);
        return plan.Id;
    }

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
