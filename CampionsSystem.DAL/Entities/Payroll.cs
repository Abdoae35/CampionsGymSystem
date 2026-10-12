using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class Payroll
{
    public Guid? FinancialMonthId { get; set; }
    public FinancialMonth? FinancialMonth { get; set; }
    public Guid? CommissionSummaryId { get; set; }
    public EmployeeCommissionSummary? CommissionSummary { get; set; }
    public string? SalaryEvidenceJson { get; set; }
    public Guid? RecognizedByUserId { get; set; }
    public ApplicationUser? RecognizedByUser { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid? CommissionPeriodId { get; set; }
    public CommissionPeriod? CommissionPeriod { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal NetAmount { get; set; }
    public PayrollStatus Status { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public Guid? PaidByUserId { get; set; }
    public ApplicationUser? PaidByUser { get; set; }
    public Guid? PaidFromFinancialAccountId { get; set; }
    public FinancialAccount? PaidFromFinancialAccount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
