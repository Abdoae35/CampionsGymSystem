namespace CampionsSystem.DAL.Entities;

public sealed class CashCount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
    public Guid? ShiftId { get; set; }
    public Shift? Shift { get; set; }
    public Guid? BusinessDayId { get; set; }
    public BusinessDay? BusinessDay { get; set; }
    public DateOnly BusinessDate { get; set; }
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public string Kind { get; set; } = null!;
    public long MovementSequence { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal CountedCash { get; set; }
    public decimal BookCash { get; set; }
    public decimal Difference { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public string? Reason { get; set; }
}
