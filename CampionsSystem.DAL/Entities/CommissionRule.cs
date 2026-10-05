using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class CommissionRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public CommissionType CommissionType { get; set; }
    public Guid? SportsActivityId { get; set; }
    public SportsActivity? SportsActivity { get; set; }
    public Guid? GroupClassId { get; set; }
    public GroupClass? GroupClass { get; set; }
    public CommissionCalculationMethod CalculationMethod { get; set; }
    public decimal? TargetAmount { get; set; }
    public decimal? BelowTargetRate { get; set; }
    public decimal? ReachedTargetRate { get; set; }
    public decimal? FlatRate { get; set; }
    public decimal? FixedAmountPerAttendance { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
