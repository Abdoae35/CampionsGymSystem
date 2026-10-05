using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class CommissionRuleConfiguration : IEntityTypeConfiguration<CommissionRule>
{
    public void Configure(EntityTypeBuilder<CommissionRule> builder)
    {
        builder.ToTable("CommissionRules", table =>
        {
            table.HasCheckConstraint("CK_CommissionRules_CommissionType", "\"CommissionType\" IN ('SalesNew', 'SalesRenewal', 'Private', 'Activity', 'Class', 'Freelance')");
            table.HasCheckConstraint("CK_CommissionRules_CalculationMethod", "\"CalculationMethod\" IN ('TargetPercentage', 'FlatPercentage', 'FixedPerAttendance')");
            table.HasCheckConstraint("CK_CommissionRules_TargetAmount", "\"TargetAmount\" >= 0");
            table.HasCheckConstraint("CK_CommissionRules_BelowTargetRate", "\"BelowTargetRate\" >= 0 AND \"BelowTargetRate\" <= 100");
            table.HasCheckConstraint("CK_CommissionRules_ReachedTargetRate", "\"ReachedTargetRate\" >= 0 AND \"ReachedTargetRate\" <= 100");
            table.HasCheckConstraint("CK_CommissionRules_FlatRate", "\"FlatRate\" >= 0 AND \"FlatRate\" <= 100");
            table.HasCheckConstraint("CK_CommissionRules_FixedAmountPerAttendance", "\"FixedAmountPerAttendance\" >= 0");
            table.HasCheckConstraint("CK_CommissionRules_EffectiveFromEffectiveTo", "\"EffectiveTo\" >= \"EffectiveFrom\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CommissionType).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.SportsActivity).WithMany().HasForeignKey(x => x.SportsActivityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.GroupClass).WithMany().HasForeignKey(x => x.GroupClassId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CalculationMethod).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.TargetAmount).HasPrecision(18, 2);
        builder.Property(x => x.BelowTargetRate).HasPrecision(5, 2);
        builder.Property(x => x.ReachedTargetRate).HasPrecision(5, 2);
        builder.Property(x => x.FlatRate).HasPrecision(5, 2);
        builder.Property(x => x.FixedAmountPerAttendance).HasPrecision(18, 2);
        builder.Property(x => x.EffectiveFrom).HasColumnType("date");
        builder.Property(x => x.EffectiveTo).HasColumnType("date");
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.BranchId, x.EmployeeId, x.CommissionType, x.EffectiveFrom });
    }
}
