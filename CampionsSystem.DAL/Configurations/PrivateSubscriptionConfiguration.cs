using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class PrivateSubscriptionConfiguration : IEntityTypeConfiguration<PrivateSubscription>
{
    public void Configure(EntityTypeBuilder<PrivateSubscription> builder)
    {
        builder.ToTable("PrivateSubscriptions", table =>
        {
            table.HasCheckConstraint("CK_PrivateSubscriptions_TotalSessions", "\"TotalSessions\" >= 0");
            table.HasCheckConstraint("CK_PrivateSubscriptions_RemainingSessions", "\"RemainingSessions\" >= 0");
            table.HasCheckConstraint("CK_PrivateSubscriptions_Status", "\"Status\" IN ('Scheduled', 'Active', 'Expired', 'Cancelled')");
            table.HasCheckConstraint("CK_PrivateSubscriptions_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
            table.HasCheckConstraint("CK_PrivateSubscriptions_SessionBounds", "\"RemainingSessions\" <= \"TotalSessions\" AND ((\"TotalSessions\" IS NULL) = (\"RemainingSessions\" IS NULL))");
        });
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
