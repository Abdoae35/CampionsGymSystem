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
            table.HasCheckConstraint("CK_FinancialTransactions_SourceType", "\"SourceType\" IN ('Payment', 'Refund', 'Expense', 'Payroll', 'Adjustment')");
            table.HasCheckConstraint("CK_FinancialTransactions_Amount", "\"Amount\" >= 0");
            table.HasCheckConstraint("CK_FinancialTransactions_Source", "(\"SourceType\" = 'Payment' AND \"PaymentId\" IS NOT NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Refund' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NOT NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Expense' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NOT NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Payroll' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NOT NULL) OR (\"SourceType\" = 'Adjustment' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL)");
        });
        builder.Property(x => x.BusinessDate).HasColumnType("date");
        builder.HasKey(x => x.Id);
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
