namespace CampionsSystem.DAL.Entities;
// Ordered append-only settlement/reversal facts. Original BorrowedGymSession settlement fields remain historical.
public sealed class BorrowedDebtSettlementEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BorrowedGymSessionId { get; set; }
    public BorrowedGymSession BorrowedGymSession { get; set; } = null!;
    public int Sequence { get; set; }
    public Guid GrantId { get; set; }
    public MembershipSubscriptionTransaction Grant { get; set; } = null!;
    public bool IsReversal { get; set; }
    public Guid? RefundId { get; set; }
    public Refund? Refund { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid ActorId { get; set; }
    public ApplicationUser Actor { get; set; } = null!;
}
