using CampionsSystem.DAL.Enums;
namespace CampionsSystem.DAL.Entities;

// Attribution evidence, not another money ledger. Created with the actual Payment.
public sealed class CommissionCollection
{
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public SaleCategory? Category { get; set; }
    public string EligibilityReason { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CommissionRefundAllocation
{
    public Guid RefundId { get; set; }
    public Refund Refund { get; set; } = null!;
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public decimal Amount { get; set; }
}

// Immutable source reconciliation for a finalized branch period, including House/exclusions.
public sealed class CommissionStatementLine
{
    public Guid CommissionPeriodId { get; set; }
    public CommissionPeriod CommissionPeriod { get; set; } = null!;
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public SaleCategory? Category { get; set; }
    public string EligibilityReason { get; set; } = null!;
    public decimal GrossAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal EligibleAmount { get; set; }
}
