using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class EmployeeAdvanceConfiguration : IEntityTypeConfiguration<EmployeeAdvance>
{
    public void Configure(EntityTypeBuilder<EmployeeAdvance> b)
    {
        b.ToTable("EmployeeAdvances", t =>
        {
            t.HasCheckConstraint("CK_EmployeeAdvances_Amount", "\"Amount\" > 0");
        });
        b.HasKey(x => x.Id);
        b.HasOne(x => x.FinancialMonth).WithMany().HasForeignKey(x => x.FinancialMonthId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.BasicEntitlementSnapshot).HasPrecision(18, 2);
        b.HasIndex(x => new { x.FinancialMonthId, x.ReceivingEmployeeId });
        b.ToTable("EmployeeAdvances", t => t.HasCheckConstraint("CK_EmployeeAdvances_Entitlement", "\"FinancialMonthId\" IS NULL AND \"BasicEntitlementSnapshot\" IS NULL OR \"FinancialMonthId\" IS NOT NULL AND \"BasicEntitlementSnapshot\" IS NOT NULL AND \"BasicEntitlementSnapshot\" >= \"Amount\""));
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FinancialAccount).WithMany().HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.BusinessDay).WithMany().HasForeignKey(x => x.BusinessDayId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.BusinessDate).HasColumnType("date");
        b.HasOne(x => x.ReceivingEmployee).WithMany().HasForeignKey(x => x.ReceivingEmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RecordingEmployee).WithMany().HasForeignKey(x => x.RecordingEmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Reason).HasColumnType("text").IsRequired();
        b.Property(x => x.Reference).HasMaxLength(512);
        b.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.OperationId).IsUnique();
        b.HasIndex(x => new { x.BranchId, x.OccurredAt });
    }
}
