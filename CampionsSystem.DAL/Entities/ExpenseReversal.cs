namespace CampionsSystem.DAL.Entities;

public sealed class ExpenseReversal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExpenseId { get; set; }
    public Expense Expense { get; set; } = null!;
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid ShiftId { get; set; }
    public Shift Shift { get; set; } = null!;
    public Guid? BusinessDayId { get; set; }
    public BusinessDay? BusinessDay { get; set; }
    public DateOnly BusinessDate { get; set; }
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public string? ReturnReference { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
}
