using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class PrivateSession
{
    public Guid? OperationId { get; set; }
    public string? RequestHash { get; set; }
    public DateOnly? BusinessDate { get; set; }
    public Guid? PrivateCoachAssignmentId { get; set; }
    public PrivateCoachAssignment? PrivateCoachAssignment { get; set; }
    public decimal? SessionValue { get; set; }
    public int? SessionNumber { get; set; }
    public bool? TransferredPackage { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PrivateSubscriptionId { get; set; }
    public PrivateSubscription PrivateSubscription { get; set; } = null!;
    public Guid PrivateSubscriptionTransactionId { get; set; }
    public PrivateSubscriptionTransaction PrivateSubscriptionTransaction { get; set; } = null!;
    public Guid? CommissionPeriodId { get; set; }
    public CommissionPeriod? CommissionPeriod { get; set; }
    public Guid CoachEmployeeId { get; set; }
    public Employee CoachEmployee { get; set; } = null!;
    public Guid GymCheckInId { get; set; }
    public CheckIn GymCheckIn { get; set; } = null!;
    public DateTimeOffset SessionAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public string? Notes { get; set; }
}
