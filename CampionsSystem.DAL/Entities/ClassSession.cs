using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class ClassSession
{
    // Nullable for legacy reconciliation; preserve operational date independently of real timestamps.
    public Guid? BusinessDayId { get; set; }
    public BusinessDay? BusinessDay { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupClassId { get; set; }
    public GroupClass GroupClass { get; set; } = null!;
    public Guid? ClassScheduleId { get; set; }
    public ClassSchedule? ClassSchedule { get; set; }
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid CoachEmployeeId { get; set; }
    public Employee CoachEmployee { get; set; } = null!;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public ClassSessionStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
