namespace CampionsSystem.DAL.Entities;

public sealed class EmployeeAdvance
{
    public Guid? FinancialMonthId { get; set; }
    public FinancialMonth? FinancialMonth { get; set; }
    public decimal? BasicEntitlementSnapshot { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
    public Guid ShiftId { get; set; }
    public Shift Shift { get; set; } = null!;
    public Guid? BusinessDayId { get; set; }
    public BusinessDay? BusinessDay { get; set; }
    public DateOnly BusinessDate { get; set; }
    public Guid ReceivingEmployeeId { get; set; }
    public Employee ReceivingEmployee { get; set; } = null!;
    public Guid RecordingEmployeeId { get; set; }
    public Employee RecordingEmployee { get; set; } = null!;
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public string? Reference { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
}
