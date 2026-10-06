using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class PrivateSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalSessions { get; set; }
    public int RemainingSessions { get; set; }
    public PrivateSubscriptionStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<PrivateCoachAssignment> CoachAssignments { get; set; } = new List<PrivateCoachAssignment>();
    public ICollection<PrivateSubscriptionTransaction> Transactions { get; set; } = new List<PrivateSubscriptionTransaction>();
    public ICollection<PrivateExpirySettlement> ExpirySettlements { get; set; } = new List<PrivateExpirySettlement>();
    public ICollection<PrivateSession> Sessions { get; set; } = new List<PrivateSession>();
}
