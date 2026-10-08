namespace CampionsSystem.DAL.Entities;

// Owner is always resolved from current MemberSalesAssignment, never copied here.
public sealed class MemberFollowUpTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public DateTimeOffset DueAt { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public ApplicationUser? CompletedByUser { get; set; }
}
