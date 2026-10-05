using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MemberSubscriptionConfiguration : IEntityTypeConfiguration<MemberSubscription>
{
    public void Configure(EntityTypeBuilder<MemberSubscription> builder)
    {
        builder.ToTable("MemberSubscriptions", table =>
        {
            table.HasCheckConstraint("CK_MemberSubscriptions_AccessMode", "\"AccessMode\" IN ('Unlimited', 'SessionBased')");
            table.HasCheckConstraint("CK_MemberSubscriptions_TotalSessions", "\"TotalSessions\" >= 0");
            table.HasCheckConstraint("CK_MemberSubscriptions_RemainingSessions", "\"RemainingSessions\" >= 0");
            table.HasCheckConstraint("CK_MemberSubscriptions_Status", "\"Status\" IN ('Scheduled', 'Active', 'Frozen', 'Expired', 'Cancelled', 'SuspendedForInstallment')");
            table.HasCheckConstraint("CK_MemberSubscriptions_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
            table.HasCheckConstraint("CK_MemberSubscriptions_SessionBounds", "\"RemainingSessions\" <= \"TotalSessions\" AND ((\"TotalSessions\" IS NULL) = (\"RemainingSessions\" IS NULL))");
            table.HasCheckConstraint("CK_MemberSubscriptions_AccessSessions", "(\"AccessMode\" = 'Unlimited' AND \"TotalSessions\" IS NULL AND \"RemainingSessions\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"TotalSessions\" IS NOT NULL AND \"RemainingSessions\" IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Member).WithMany(x => x.Subscriptions).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.MembershipPlan).WithMany().HasForeignKey(x => x.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PurchasedAtBranch).WithMany().HasForeignKey(x => x.PurchasedAtBranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.AccessMode).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
