namespace CampionsSystem.DAL.Entities;

// Independent coach targets; no Sales target or Classes compensation defaults.
public sealed class CoachTargetRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CommissionPeriodId { get; set; }
    public CommissionPeriod CommissionPeriod { get; set; } = null!;
    public Guid CoachEmployeeId { get; set; }
    public Employee CoachEmployee { get; set; } = null!;
    public decimal? PrivateTarget { get; set; }
    public decimal? ClassesAverageTarget { get; set; }
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}

// One actual funding slice of a delivered session or explicit unused-session settlement.
public sealed class CoachEarning
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid CoachEmployeeId { get; set; }
    public Employee CoachEmployee { get; set; } = null!;
    public Guid CommissionPeriodId { get; set; }
    public CommissionPeriod CommissionPeriod { get; set; } = null!;
    public Guid? CoachTargetRuleId { get; set; }
    public CoachTargetRule? CoachTargetRule { get; set; }
    public Guid SaleTransactionId { get; set; }
    public SaleTransaction SaleTransaction { get; set; } = null!;
    public Guid? PrivateSessionId { get; set; }
    public PrivateSession? PrivateSession { get; set; }
    public Guid? PrivateExpirySettlementId { get; set; }
    public PrivateExpirySettlement? PrivateExpirySettlement { get; set; }
    public Guid RecognitionOperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public decimal EligibleValue { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public bool TransferredPackage { get; set; }
    public decimal TargetProgress { get; set; }
    public decimal? Target { get; set; }
    public DateOnly BusinessDate { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CoachPayout
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid CoachEmployeeId { get; set; }
    public Employee CoachEmployee { get; set; } = null!;
    public Guid ExpenseId { get; set; }
    public Expense Expense { get; set; } = null!;
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid AuthorizedByUserId { get; set; }
    public ApplicationUser AuthorizedByUser { get; set; } = null!;
    public DateTimeOffset PaidAt { get; set; }
    public DateOnly BusinessDate { get; set; }
}

public sealed class CoachPayoutAllocation
{
    public Guid CoachPayoutId { get; set; }
    public CoachPayout CoachPayout { get; set; } = null!;
    public Guid CoachEarningId { get; set; }
    public CoachEarning CoachEarning { get; set; } = null!;
    public decimal Amount { get; set; }
}
