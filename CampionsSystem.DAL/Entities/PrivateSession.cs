using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class PrivateSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PrivateSubscriptionId { get; set; }
    public PrivateSubscription PrivateSubscription { get; set; } = null!;
    public Guid CoachEmployeeId { get; set; }
    public Employee CoachEmployee { get; set; } = null!;
    public Guid GymCheckInId { get; set; }
    public CheckIn GymCheckIn { get; set; } = null!;
    public DateTimeOffset SessionAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public string? Notes { get; set; }
}
