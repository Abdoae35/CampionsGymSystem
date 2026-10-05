using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ActivitySubscriptionConfiguration : IEntityTypeConfiguration<ActivitySubscription>
{
    public void Configure(EntityTypeBuilder<ActivitySubscription> builder)
    {
        builder.ToTable("ActivitySubscriptions", table =>
        {
            table.HasCheckConstraint("CK_ActivitySubscriptions_AccessMode", "\"AccessMode\" IN ('Unlimited', 'SessionBased')");
            table.HasCheckConstraint("CK_ActivitySubscriptions_AccessSessions", "(\"AccessMode\" = 'Unlimited' AND \"TotalSessions\" IS NULL AND \"RemainingSessions\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"TotalSessions\" IS NOT NULL AND \"RemainingSessions\" IS NOT NULL)");
            table.HasCheckConstraint("CK_ActivitySubscriptions_TotalSessions", "\"TotalSessions\" >= 0");
            table.HasCheckConstraint("CK_ActivitySubscriptions_RemainingSessions", "\"RemainingSessions\" >= 0");
            table.HasCheckConstraint("CK_ActivitySubscriptions_Status", "\"Status\" IN ('Scheduled', 'Active', 'Expired', 'Cancelled', 'SuspendedForInstallment')");
            table.HasCheckConstraint("CK_ActivitySubscriptions_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
            table.HasCheckConstraint("CK_ActivitySubscriptions_SessionBounds", "\"RemainingSessions\" <= \"TotalSessions\" AND ((\"TotalSessions\" IS NULL) = (\"RemainingSessions\" IS NULL))");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AccessMode).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ActivityPackage).WithMany().HasForeignKey(x => x.ActivityPackageId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ActivityCoachAssignment).WithMany().HasForeignKey(x => x.ActivityCoachAssignmentId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
