using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class ActivityCoach
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ActivityCoachType CoachType { get; set; }
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string? ExternalName { get; set; }
    public string? ExternalPhone { get; set; }
    public decimal Percentage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
