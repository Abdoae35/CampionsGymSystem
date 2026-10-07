using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class MembershipPlanEligibleMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MembershipPlanId { get; set; }
    public MembershipPlan MembershipPlan { get; set; } = null!;
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
