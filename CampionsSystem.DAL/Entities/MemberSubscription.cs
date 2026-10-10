using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class MemberSubscription
{
    // Standalone paid visits are not membership/borrow cycles.
    public bool IsStandaloneVisit { get; set; }
    public int Version { get; set; }
    public Guid? FollowsSubscriptionId { get; set; }
    public MemberSubscription? FollowsSubscription { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    // Non-null identifies an employment entitlement and preserves its historical owner.
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid MembershipPlanId { get; set; }
    public MembershipPlan MembershipPlan { get; set; } = null!;
    public Guid CityId { get; set; }
    public City City { get; set; } = null!;
    public Guid PurchasedAtBranchId { get; set; }
    public GymBranch PurchasedAtBranch { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public AccessMode AccessMode { get; set; }
    public int? TotalSessions { get; set; }
    public int? RemainingSessions { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
