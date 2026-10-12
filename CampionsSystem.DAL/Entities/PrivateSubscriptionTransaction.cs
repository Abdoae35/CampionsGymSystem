using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class PrivateSubscriptionTransaction
{
    // Nullable only for legacy data. New purchases snapshot all these facts.
    public Guid? OriginalCoachEmployeeId { get; set; }
    public Employee? OriginalCoachEmployee { get; set; }
    public Guid? OriginalAssignmentId { get; set; }
    public PrivateCoachAssignment? OriginalAssignment { get; set; }
    public DateOnly? StartDateSnapshot { get; set; }
    public string? PackageNameSnapshot { get; set; }
    public decimal RemainderValue { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PrivateSubscriptionId { get; set; }
    public PrivateSubscription PrivateSubscription { get; set; } = null!;
    public Guid SaleTransactionId { get; set; }
    public SaleTransaction SaleTransaction { get; set; } = null!;
    public Guid PrivatePackageId { get; set; }
    public PrivatePackage PrivatePackage { get; set; } = null!;
    public SubscriptionTransactionType TransactionType { get; set; }
    public int SessionsAdded { get; set; }
    public int RemainingSessions { get; set; }
    public ICollection<PrivateSession> Sessions { get; set; } = new List<PrivateSession>();
    public PrivateExpirySettlement? ExpirySettlement { get; set; }
    public int DurationDaysAdded { get; set; }
    public decimal NetAmount { get; set; }
    public decimal UnitSessionValue { get; set; }
    public DateOnly? PreviousEndDate { get; set; }
    public DateOnly NewEndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
