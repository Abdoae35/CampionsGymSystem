namespace CampionsSystem.DAL.Entities;
// A reversal fact; the original entitlement grant and benefit snapshots stay immutable.
public sealed class SubscriptionGrantCancellation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GrantId { get; set; }
    public MembershipSubscriptionTransaction Grant { get; set; } = null!;
    public Guid RefundId { get; set; }
    public Refund Refund { get; set; } = null!;
    public DateOnly? PreviousEndDate { get; set; }
    public DateOnly? NewEndDate { get; set; }
    public int? PreviousRemainingSessions { get; set; }
    public int? NewRemainingSessions { get; set; }
    public int DaysRemoved { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
