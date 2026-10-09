using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class MembershipPlan
{
    // Dynamic audience classification, distinct from Normal/Freelance/Employee PlanCategory.
    public Guid? MembershipPlanCategoryId { get; set; }
    public MembershipPlanCategory? MembershipPlanCategory { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    // Administrative ownership; null only for legacy plans awaiting explicit mapping.
    public Guid? BranchId { get; set; }
    public GymBranch? Branch { get; set; }
    public Guid CityId { get; set; }
    public City City { get; set; } = null!;
    public string Name { get; set; } = null!;
    public AccessMode AccessMode { get; set; }
    // Zero only for employment-bound plans; no fixed duration.
    public int DurationDays { get; set; }
    public int? SessionCount { get; set; }
    public PlanCategory PlanCategory { get; set; }
    public Guid? FreelanceCoachEmployeeId { get; set; }
    public Employee? FreelanceCoachEmployee { get; set; }
    /// <summary>Normal selling price, or fixed gym amount for a Freelance plan.</summary>
    public decimal Price { get; set; }
    public bool AllowFreeze { get; set; }
    public int IncludedFreezeDays { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
