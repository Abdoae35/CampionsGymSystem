namespace CampionsSystem.DAL.Entities;

// One protected day per subscription, even when multiple official closures overlap.
public sealed class SubscriptionHolidayExtension
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberSubscriptionId { get; set; }
    public MemberSubscription MemberSubscription { get; set; } = null!;
    public Guid BranchClosureId { get; set; }
    public BranchClosure BranchClosure { get; set; } = null!;
    public DateOnly ProtectedDate { get; set; }
    public int DaysApplied { get; set; }
    public DateOnly PreviousEndDate { get; set; }
    public DateOnly NewEndDate { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
