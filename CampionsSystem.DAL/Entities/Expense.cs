using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class Expense
{
    // Nullable for legacy reconciliation; preserve operational date independently of real timestamps.
    public Guid? BusinessDayId { get; set; }
    public BusinessDay? BusinessDay { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid? ShiftId { get; set; }
    public Shift? Shift { get; set; }
    public Guid ExpenseCategoryId { get; set; }
    public ExpenseCategory ExpenseCategory { get; set; } = null!;
    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public DateTimeOffset ExpenseDate { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public Guid? ApprovedByUserId { get; set; }
    public ApplicationUser? ApprovedByUser { get; set; }
    public ExpenseStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
