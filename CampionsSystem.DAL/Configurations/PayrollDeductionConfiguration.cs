using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class PayrollDeductionConfiguration : IEntityTypeConfiguration<PayrollDeduction>
{
    public void Configure(EntityTypeBuilder<PayrollDeduction> builder)
    {
        builder.ToTable("PayrollDeductions", table =>
        {
            table.HasCheckConstraint("CK_PayrollDeductions_Amount", "\"Amount\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.EmployeeAdvance).WithMany().HasForeignKey(x => x.EmployeeAdvanceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.EmployeeAdvanceId).IsUnique().HasFilter("\"EmployeeAdvanceId\" IS NOT NULL");
        builder.HasOne(x => x.Payroll).WithMany().HasForeignKey(x => x.PayrollId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Reason).HasColumnType("text").IsRequired();
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
