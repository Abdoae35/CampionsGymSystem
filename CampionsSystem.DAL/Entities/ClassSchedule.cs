using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class ClassSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupClassId { get; set; }
    public GroupClass GroupClass { get; set; } = null!;
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}
