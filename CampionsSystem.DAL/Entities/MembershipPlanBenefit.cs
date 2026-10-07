using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class MembershipPlanBenefit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MembershipPlanId { get; set; }
    public MembershipPlan MembershipPlan { get; set; } = null!;
    public Guid MembershipBenefitId { get; set; }
    public MembershipBenefit MembershipBenefit { get; set; } = null!;
    public MembershipBenefitGrantType GrantType { get; set; }
    public int? Quantity { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
