using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class FinancialTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public FinancialTransactionType TransactionType { get; set; }
    public FinancialSourceType SourceType { get; set; }
    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public Guid? RefundId { get; set; }
    public Refund? Refund { get; set; }
    public Guid? ExpenseId { get; set; }
    public Expense? Expense { get; set; }
    public Guid? PayrollId { get; set; }
    public Payroll? Payroll { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public string? Notes { get; set; }
}
