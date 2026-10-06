namespace CampionsSystem.DAL.Entities;

/// <summary>Immutable financial snapshot of unused batch value at final expiry.</summary>
public sealed class PrivateExpirySettlement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PrivateSubscriptionId { get; set; }
    public PrivateSubscription PrivateSubscription { get; set; } = null!;
    public Guid PrivateSubscriptionTransactionId { get; set; }
    public PrivateSubscriptionTransaction PrivateSubscriptionTransaction { get; set; } = null!;
    public Guid CoachEmployeeId { get; set; }
    public Employee CoachEmployee { get; set; } = null!;
    public Guid CommissionPeriodId { get; set; }
    public CommissionPeriod CommissionPeriod { get; set; } = null!;
    public int RemainingSessions { get; set; }
    public decimal UnitSessionValue { get; set; }
    public decimal EligibleAmount { get; set; }
    public DateTimeOffset SettledAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
