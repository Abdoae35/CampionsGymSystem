using CampionsSystem.BLL.Plans;
using CampionsSystem.DAL.Enums;

namespace CampionsSystem.BLL.Operations;

public sealed record SaveFreelancePlanRequest(Guid CityId, Guid CoachEmployeeId, string Name, decimal Price,
    int DurationDays, AccessMode AccessMode, int? SessionCount, bool IsActive = true, Guid? BranchId = null);

public sealed class FreelancePlanService(MembershipPlanManagementService management)
{
    // Compatibility API delegates to the same branch-scoped template rules.
    public Task<Guid> SaveAsync(Guid? id, SaveFreelancePlanRequest request, CancellationToken ct = default)
        => management.SaveFreelancePlanAsync(id, request, ct);

    public async Task SetEligibilityAsync(Guid planId, Guid memberId, bool eligible, CancellationToken ct = default)
        => await management.SetEligibilityAsync(planId, memberId, eligible, ct);
}
