namespace CampionsSystem.DAL.Entities;
// Dated staff assignment to the existing audience catalog; no demographic inference.
public sealed class MemberAudienceCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid MembershipPlanCategoryId { get; set; }
    public MembershipPlanCategory MembershipPlanCategory { get; set; } = null!;
    public Guid AssignedByUserId { get; set; }
    public ApplicationUser AssignedByUser { get; set; } = null!;
    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
}
