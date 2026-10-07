using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class MemberBenefitUsage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberSubscriptionBenefitId { get; set; }
    public MemberSubscriptionBenefit MemberSubscriptionBenefit { get; set; } = null!;
    public int QuantityUsed { get; set; }
    public DateTimeOffset UsedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
