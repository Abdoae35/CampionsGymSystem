using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampionsSystem.DAL.Configurations;
public sealed class SubscriptionHolidayExtensionConfiguration : IEntityTypeConfiguration<SubscriptionHolidayExtension>
{
    public void Configure(EntityTypeBuilder<SubscriptionHolidayExtension> b)
    {
        b.ToTable("SubscriptionHolidayExtensions", t => t.HasCheckConstraint("CK_SubscriptionHolidayExtensions_Days", "\"DaysApplied\" IN (0, 1) AND \"NewEndDate\" = \"PreviousEndDate\" + \"DaysApplied\""));
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.MemberSubscriptionId, x.ProtectedDate }).IsUnique();
        b.HasOne(x => x.MemberSubscription).WithMany().HasForeignKey(x => x.MemberSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.BranchClosure).WithMany().HasForeignKey(x => x.BranchClosureId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.ProtectedDate).HasColumnType("date");
        b.Property(x => x.PreviousEndDate).HasColumnType("date");
        b.Property(x => x.NewEndDate).HasColumnType("date");
        b.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
