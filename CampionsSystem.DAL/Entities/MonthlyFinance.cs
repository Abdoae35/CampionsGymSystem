namespace CampionsSystem.DAL.Entities;

// Financial reporting boundaries, independent of commission evaluation periods.
public sealed class FinancialMonth
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "Open";
    public int Version { get; set; }
    public int LastRevision { get; set; }
    public DateTimeOffset? FirstClosedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
}

public sealed class FinancialMonthEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FinancialMonthId { get; set; }
    public FinancialMonth FinancialMonth { get; set; } = null!;
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public string Kind { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public int ResultVersion { get; set; }
    public int? Revision { get; set; }
    public Guid ActorUserId { get; set; }
    public ApplicationUser ActorUser { get; set; } = null!;
    public DateTimeOffset RecordedAt { get; set; }
}

public sealed class FinancialClosingRevision
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FinancialMonthId { get; set; }
    public FinancialMonth FinancialMonth { get; set; } = null!;
    public int Revision { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public DateTimeOffset SnapshotAt { get; set; }
    public Guid ActorUserId { get; set; }
    public ApplicationUser ActorUser { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string ReportJson { get; set; } = null!;
    public byte[] Workbook { get; set; } = null!;
    public string WorkbookSha256 { get; set; } = null!;
}

// Owner-declared payment evidence. Funding is intentionally not inferred or posted.
public sealed class PayrollPaymentRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PayrollId { get; set; }
    public Payroll Payroll { get; set; } = null!;
    public Guid FinancialMonthId { get; set; }
    public FinancialMonth FinancialMonth { get; set; } = null!;
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public DateOnly BusinessDate { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public Guid ActorUserId { get; set; }
    public ApplicationUser ActorUser { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string? Reference { get; set; }
}

public sealed class MaintenancePlanRevision
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlanId { get; set; }
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public int Revision { get; set; }
    public string Name { get; set; } = null!;
    public decimal ExpectedCost { get; set; }
    public int FrequencyMonths { get; set; }
    public DateOnly FirstDueDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public Guid ActorUserId { get; set; }
    public ApplicationUser ActorUser { get; set; } = null!;
    public DateTimeOffset RecordedAt { get; set; }
}

// Append-only known original occurrence, separate from posting/collection dates.
public sealed class FinancialOccurrenceEvidence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FinancialTransactionId { get; set; }
    public FinancialTransaction FinancialTransaction { get; set; } = null!;
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public DateTimeOffset OriginalOccurredAt { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public Guid ActorUserId { get; set; }
    public ApplicationUser ActorUser { get; set; } = null!;
    public string Reason { get; set; } = null!;
}
