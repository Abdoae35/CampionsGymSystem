using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
{
    public void Configure(EntityTypeBuilder<Payroll> builder)
    {
        builder.ToTable("Payrolls", table =>
        {
            table.HasCheckConstraint("CK_Payrolls_BasicSalary", "\"BasicSalary\" >= 0");
            table.HasCheckConstraint("CK_Payrolls_CommissionAmount", "\"CommissionAmount\" >= 0");
            table.HasCheckConstraint("CK_Payrolls_DeductionAmount", "\"DeductionAmount\" >= 0");
            table.HasCheckConstraint("CK_Payrolls_BonusAmount", "\"BonusAmount\" >= 0");
            table.HasCheckConstraint("CK_Payrolls_NetAmount", "\"NetAmount\" >= 0");
            table.HasCheckConstraint("CK_Payrolls_Status", "\"Status\" IN ('Pending', 'Paid')");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.FinancialMonth).WithMany().HasForeignKey(x => x.FinancialMonthId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CommissionSummary).WithMany().HasForeignKey(x => x.CommissionSummaryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RecognizedByUser).WithMany().HasForeignKey(x => x.RecognizedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.SalaryEvidenceJson).HasColumnType("jsonb");
        builder.HasIndex(x => x.CommissionSummaryId).IsUnique().HasFilter("\"CommissionSummaryId\" IS NOT NULL");
        builder.HasIndex(x => new { x.FinancialMonthId, x.EmployeeId }).IsUnique().HasFilter("\"FinancialMonthId\" IS NOT NULL AND \"CommissionSummaryId\" IS NULL");
        builder.ToTable("Payrolls", t => t.HasCheckConstraint("CK_Payrolls_Recognized", "\"FinancialMonthId\" IS NULL OR (\"RecognizedByUserId\" IS NOT NULL AND \"BonusAmount\" = 0 AND \"NetAmount\" = \"BasicSalary\" + \"CommissionAmount\" - \"DeductionAmount\" AND (\"CommissionSummaryId\" IS NULL AND \"CommissionAmount\" = 0 AND \"SalaryEvidenceJson\" IS NOT NULL OR \"CommissionSummaryId\" IS NOT NULL AND \"CommissionPeriodId\" IS NOT NULL AND \"BasicSalary\" = 0 AND \"DeductionAmount\" = 0))"));
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CommissionPeriod).WithMany().HasForeignKey(x => x.CommissionPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.BasicSalary).HasPrecision(18, 2);
        builder.Property(x => x.CommissionAmount).HasPrecision(18, 2);
        builder.Property(x => x.DeductionAmount).HasPrecision(18, 2);
        builder.Property(x => x.BonusAmount).HasPrecision(18, 2);
        builder.Property(x => x.NetAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.PaidAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.PaidByUser).WithMany().HasForeignKey(x => x.PaidByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PaidFromFinancialAccount).WithMany().HasForeignKey(x => x.PaidFromFinancialAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.BonusAmount).HasDefaultValue(0m);
    }
}
