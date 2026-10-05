using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class BorrowedGymSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid ExpiredSubscriptionId { get; set; }
    public MemberSubscription ExpiredSubscription { get; set; } = null!;
    public Guid? CheckInId { get; set; }
    public CheckIn? CheckIn { get; set; }
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public DateTimeOffset BorrowedAt { get; set; }
    public Guid? SettledBySubscriptionId { get; set; }
    public MemberSubscription? SettledBySubscription { get; set; }
    public DateTimeOffset? SettledAt { get; set; }
    public BorrowedSessionStatus Status { get; set; }
}
