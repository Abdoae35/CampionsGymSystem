using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class MembershipSubscriptionTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberSubscriptionId { get; set; }
    public MemberSubscription MemberSubscription { get; set; } = null!;
    public Guid SaleTransactionId { get; set; }
    public SaleTransaction SaleTransaction { get; set; } = null!;
    public Guid MembershipPlanId { get; set; }
    public MembershipPlan MembershipPlan { get; set; } = null!;
    public SubscriptionTransactionType TransactionType { get; set; }
    public int PurchasedDurationDays { get; set; }
    public int? PurchasedSessions { get; set; }
    public decimal PlanPriceSnapshot { get; set; }
    public decimal NetPriceSnapshot { get; set; }
    public DateOnly? PreviousEndDate { get; set; }
    public DateOnly NewEndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
