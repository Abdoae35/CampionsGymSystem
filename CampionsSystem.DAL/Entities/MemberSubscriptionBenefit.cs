using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class MemberSubscriptionBenefit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    // The source transaction provides the subscription; no duplicated subscription FK.
    public Guid MembershipSubscriptionTransactionId { get; set; }
    public MembershipSubscriptionTransaction MembershipSubscriptionTransaction { get; set; } = null!;
    public Guid MembershipBenefitId { get; set; }
    public MembershipBenefit MembershipBenefit { get; set; } = null!;
    public string BenefitCodeSnapshot { get; set; } = null!;
    public string BenefitNameSnapshot { get; set; } = null!;
    public MembershipBenefitGrantType GrantTypeSnapshot { get; set; }
    public int? GrantedQuantity { get; set; }
    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
