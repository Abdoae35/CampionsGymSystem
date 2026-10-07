using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class ActivitySubscription
{
    // Nullable only for legacy reconciliation; required on every new BLL purchase.
    public Guid? ActivityCoachId { get; set; }
    public ActivityCoach? ActivityCoach { get; set; }

    // Snapshot access semantics instead of depending on a mutable package.
    public AccessMode AccessMode { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid ActivityPackageId { get; set; }
    public ActivityPackage ActivityPackage { get; set; } = null!;
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid? ActivityCoachAssignmentId { get; set; }
    public ActivityCoachAssignment? ActivityCoachAssignment { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int? TotalSessions { get; set; }
    public int? RemainingSessions { get; set; }
    public ActivitySubscriptionStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
