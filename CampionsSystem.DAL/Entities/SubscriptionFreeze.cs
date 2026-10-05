using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class SubscriptionFreeze
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberSubscriptionId { get; set; }
    public MemberSubscription MemberSubscription { get; set; } = null!;
    public FreezeType FreezeType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? DaysApplied { get; set; }
    public string? Reason { get; set; }
    public Guid ApprovedByUserId { get; set; }
    public ApplicationUser ApprovedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
