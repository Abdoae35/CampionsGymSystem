using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ActivityCoachSettlementConfiguration : IEntityTypeConfiguration<ActivityCoachSettlement>
{
    public void Configure(EntityTypeBuilder<ActivityCoachSettlement> b)
    {
        b.HasKey(x => x.Id);
        b.ToTable("ActivityCoachSettlements", t =>
        {
            t.HasCheckConstraint("CK_ActivityCoachSettlements_Amounts", "\"EligibleRevenue\" >= 0 AND \"Entitlement\" >= 0 AND \"SubscribersCount\" >= 0 AND \"Percentage\" BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_ActivityCoachSettlements_Employee", "(\"CoachTypeSnapshot\" = 'Internal' AND \"EmployeeId\" IS NOT NULL) OR (\"CoachTypeSnapshot\" = 'External' AND \"EmployeeId\" IS NULL)");
        });
        b.HasOne(x => x.CommissionPeriod).WithMany().HasForeignKey(x => x.CommissionPeriodId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActivityCoach).WithMany().HasForeignKey(x => x.ActivityCoachId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SportsActivity).WithMany().HasForeignKey(x => x.SportsActivityId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.CoachTypeSnapshot).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.CoachNameSnapshot).HasMaxLength(256);
        b.Property(x => x.ActivityNameSnapshot).HasMaxLength(256);
        b.Property(x => x.Percentage).HasPrecision(5, 2);
        b.Property(x => x.EligibleRevenue).HasPrecision(18, 2);
        b.Property(x => x.Entitlement).HasPrecision(18, 2);
        b.HasIndex(x => new { x.CommissionPeriodId, x.ActivityCoachId, x.SportsActivityId, x.Percentage }).IsUnique();
    }
}
