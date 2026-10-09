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
            table.HasCheckConstraint("CK_MemberSubscriptions_EmploymentPeriod",
                "(\"EmployeeId\" IS NULL AND \"EndDate\" IS NOT NULL) OR (\"EmployeeId\" IS NOT NULL AND \"AccessMode\" = 'Unlimited' AND ((\"EndDate\" IS NULL AND \"Status\" IN ('Scheduled', 'Active')) OR (\"EndDate\" IS NOT NULL AND \"Status\" IN ('Expired', 'Cancelled'))))");
            table.HasCheckConstraint("CK_MemberSubscriptions_AccessMode", "\"AccessMode\" IN ('Unlimited', 'SessionBased')");
            table.HasCheckConstraint("CK_MemberSubscriptions_TotalSessions", "\"TotalSessions\" >= 0");
            table.HasCheckConstraint("CK_MemberSubscriptions_RemainingSessions", "\"RemainingSessions\" >= 0");
            table.HasCheckConstraint("CK_MemberSubscriptions_Status", "\"Status\" IN ('Scheduled', 'Active', 'Frozen', 'Expired', 'Cancelled', 'SuspendedForInstallment')");
            table.HasCheckConstraint("CK_MemberSubscriptions_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
            table.HasCheckConstraint("CK_MemberSubscriptions_SessionBounds", "\"RemainingSessions\" <= \"TotalSessions\" AND ((\"TotalSessions\" IS NULL) = (\"RemainingSessions\" IS NULL))");
            table.HasCheckConstraint("CK_MemberSubscriptions_AccessSessions", "(\"AccessMode\" = 'Unlimited' AND \"TotalSessions\" IS NULL AND \"RemainingSessions\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"TotalSessions\" IS NOT NULL AND \"RemainingSessions\" IS NOT NULL)");
        });
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.EmployeeId).IsUnique().HasFilter("\"EmployeeId\" IS NOT NULL AND \"EndDate\" IS NULL");
        builder.HasIndex(x => x.MemberId, "IX_MemberSubscriptions_OpenEmployeeMember").IsUnique()
            .HasFilter("\"EmployeeId\" IS NOT NULL AND \"EndDate\" IS NULL");
        builder.HasIndex(x => x.MemberId);
        builder.HasIndex(x => new { x.EmployeeId, x.StartDate });
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasOne(x => x.FollowsSubscription).WithMany().HasForeignKey(x => x.FollowsSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.FollowsSubscriptionId).IsUnique();
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
