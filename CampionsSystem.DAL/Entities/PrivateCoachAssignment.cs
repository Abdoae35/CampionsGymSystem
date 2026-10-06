using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class PrivateCoachAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PrivateSubscriptionId { get; set; }
    public PrivateSubscription PrivateSubscription { get; set; } = null!;
    public Guid CoachEmployeeId { get; set; }
    public Employee CoachEmployee { get; set; } = null!;
    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    [Obsolete("Legacy data only. Use purchased batches, PrivateSessions and PrivateExpirySettlements.")]
    public int SessionsAllocated { get; set; }
    [Obsolete("Legacy data only. Use purchased batches, PrivateSessions and PrivateExpirySettlements.")]
    public int SessionsConsumed { get; set; }
    [Obsolete("Legacy data only. Use purchased batches, PrivateSessions and PrivateExpirySettlements.")]
    public decimal CommissionBasisAmount { get; set; }
    public string? TransferReason { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
