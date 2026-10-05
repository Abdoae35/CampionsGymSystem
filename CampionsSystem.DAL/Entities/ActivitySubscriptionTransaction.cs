using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class ActivitySubscriptionTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActivitySubscriptionId { get; set; }
    public ActivitySubscription ActivitySubscription { get; set; } = null!;
    public Guid SaleTransactionId { get; set; }
    public SaleTransaction SaleTransaction { get; set; } = null!;
    public Guid ActivityPackageId { get; set; }
    public ActivityPackage ActivityPackage { get; set; } = null!;
    public SubscriptionTransactionType TransactionType { get; set; }
    public int? SessionsAdded { get; set; }
    public int DurationDaysAdded { get; set; }
    public decimal NetAmount { get; set; }
    public decimal? UnitSessionValue { get; set; }
    public DateOnly? PreviousEndDate { get; set; }
    public DateOnly NewEndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
