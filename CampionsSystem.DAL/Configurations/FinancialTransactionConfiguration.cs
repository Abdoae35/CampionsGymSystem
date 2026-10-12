using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("FinancialTransactions", table =>
        {
            table.HasCheckConstraint("CK_FinancialTransactions_TransactionType", "\"TransactionType\" IN ('Credit', 'Debit')");
            table.HasCheckConstraint("CK_FinancialTransactions_SourceType", "\"SourceType\" IN ('Payment', 'Refund', 'Expense', 'Payroll', 'Adjustment', 'EmployeeAdvance', 'CashMovement')");
            table.HasCheckConstraint("CK_FinancialTransactions_Amount", "\"Amount\" >= 0");
            table.HasCheckConstraint("CK_FinancialTransactions_Source", "((\"SourceType\" = 'Payment' AND \"PaymentId\" IS NOT NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Refund' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NOT NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Expense' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NOT NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Payroll' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NOT NULL) OR (\"SourceType\" = 'Adjustment' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL)) AND \"EmployeeAdvanceId\" IS NULL AND \"CashMovementId\" IS NULL OR (\"SourceType\" = 'EmployeeAdvance' AND \"EmployeeAdvanceId\" IS NOT NULL AND \"CashMovementId\" IS NULL AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'CashMovement' AND \"CashMovementId\" IS NOT NULL AND \"EmployeeAdvanceId\" IS NULL AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL)");
        });
        builder.Property(x => x.BusinessDate).HasColumnType("date");
        builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.EmployeeAdvance).WithMany().HasForeignKey(x => x.EmployeeAdvanceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CashMovement).WithMany().HasForeignKey(x => x.CashMovementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.BranchId, x.DrawerSequence }).IsUnique().HasFilter("\"DrawerSequence\" IS NOT NULL");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.FinancialMonth).WithMany().HasForeignKey(x => x.FinancialMonthId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.FinancialMonthId, x.OccurredAt, x.Id });
        builder.HasOne(x => x.FinancialAccount).WithMany().HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Refund).WithMany().HasForeignKey(x => x.RefundId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Expense).WithMany().HasForeignKey(x => x.ExpenseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Payroll).WithMany().HasForeignKey(x => x.PayrollId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasIndex(x => new { x.FinancialAccountId, x.OccurredAt });
        builder.HasOne(x => x.BusinessDay).WithMany().HasForeignKey(x => x.BusinessDayId).OnDelete(DeleteBehavior.Restrict);
    }
}
