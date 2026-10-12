using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class EmployeeCommissionSummary
{
    public Guid? CommissionRuleId { get; set; }
    public CommissionRule? CommissionRule { get; set; }
    public decimal? BelowTargetRateSnapshot { get; set; }
    public decimal? ReachedTargetRateSnapshot { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CommissionPeriodId { get; set; }
    public CommissionPeriod CommissionPeriod { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public CommissionType CommissionType { get; set; }
    public Guid? SportsActivityId { get; set; }
    public SportsActivity? SportsActivity { get; set; }
    public Guid? GroupClassId { get; set; }
    public GroupClass? GroupClass { get; set; }
    public decimal TotalEligibleAmount { get; set; }
    public int? EligibleQuantity { get; set; }
    public decimal? TargetAmount { get; set; }
    public bool TargetReached { get; set; }
    public decimal? AppliedRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public DateTimeOffset FinalizedAt { get; set; }
}
