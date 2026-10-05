using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class EmployeeCommissionSummaryConfiguration : IEntityTypeConfiguration<EmployeeCommissionSummary>
{
    public void Configure(EntityTypeBuilder<EmployeeCommissionSummary> builder)
    {
        builder.ToTable("EmployeeCommissionSummaries", table =>
        {
            table.HasCheckConstraint("CK_EmployeeCommissionSummaries_CommissionType", "\"CommissionType\" IN ('SalesNew', 'SalesRenewal', 'Private', 'Activity', 'Class', 'Freelance')");
            table.HasCheckConstraint("CK_EmployeeCommissionSummaries_TotalEligibleAmount", "\"TotalEligibleAmount\" >= 0");
            table.HasCheckConstraint("CK_EmployeeCommissionSummaries_EligibleQuantity", "\"EligibleQuantity\" >= 0");
            table.HasCheckConstraint("CK_EmployeeCommissionSummaries_TargetAmount", "\"TargetAmount\" >= 0");
            table.HasCheckConstraint("CK_EmployeeCommissionSummaries_AppliedRate", "\"AppliedRate\" >= 0 AND \"AppliedRate\" <= 100");
            table.HasCheckConstraint("CK_EmployeeCommissionSummaries_CommissionAmount", "\"CommissionAmount\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.CommissionPeriod).WithMany().HasForeignKey(x => x.CommissionPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CommissionType).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.SportsActivity).WithMany().HasForeignKey(x => x.SportsActivityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.GroupClass).WithMany().HasForeignKey(x => x.GroupClassId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.TotalEligibleAmount).HasPrecision(18, 2);
        builder.Property(x => x.TargetAmount).HasPrecision(18, 2);
        builder.Property(x => x.AppliedRate).HasPrecision(5, 2);
        builder.Property(x => x.CommissionAmount).HasPrecision(18, 2);
        builder.Property(x => x.FinalizedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.CommissionPeriodId, x.EmployeeId, x.CommissionType });
    }
}
