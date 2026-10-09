using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class CheckIn
{
    // Null only for legacy attendance; new managed entries preserve explicit operational facts.
    public Guid? OperationId { get; set; }
    public DateOnly? BusinessDate { get; set; }
    public int? SessionsBefore { get; set; }
    public int? SessionsAfter { get; set; }
    // Nullable for legacy reconciliation; preserve operational date independently of real timestamps.
    public Guid? BusinessDayId { get; set; }
    public BusinessDay? BusinessDay { get; set; }
    public Guid? ClassSessionId { get; set; }
    public ClassSession? ClassSession { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public CheckInType CheckInType { get; set; }
    public Guid? MemberSubscriptionId { get; set; }
    public MemberSubscription? MemberSubscription { get; set; }
    public Guid? ActivitySubscriptionId { get; set; }
    public ActivitySubscription? ActivitySubscription { get; set; }
    public Guid? GroupClassId { get; set; }
    public GroupClass? GroupClass { get; set; }
    public Guid? CoachEmployeeId { get; set; }
    public Employee? CoachEmployee { get; set; }
    public CheckInMethod CheckInMethod { get; set; }
    public bool SessionConsumed { get; set; }
    public DateTimeOffset CheckedInAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public string? Notes { get; set; }
}
