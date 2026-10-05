using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class ActivitySchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActivityCoachAssignmentId { get; set; }
    public ActivityCoachAssignment ActivityCoachAssignment { get; set; } = null!;
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}
