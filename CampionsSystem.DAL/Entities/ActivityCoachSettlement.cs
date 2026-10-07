using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class ActivityCoachSettlement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CommissionPeriodId { get; set; }
    public CommissionPeriod CommissionPeriod { get; set; } = null!;
    public Guid ActivityCoachId { get; set; }
    public ActivityCoach ActivityCoach { get; set; } = null!;
    public Guid SportsActivityId { get; set; }
    public SportsActivity SportsActivity { get; set; } = null!;
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public ActivityCoachType CoachTypeSnapshot { get; set; }
    public string CoachNameSnapshot { get; set; } = null!;
    public string ActivityNameSnapshot { get; set; } = null!;
    public int SubscribersCount { get; set; }
    public decimal EligibleRevenue { get; set; }
    public decimal Percentage { get; set; }
    public decimal Entitlement { get; set; }
    public DateTimeOffset FinalizedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
}
