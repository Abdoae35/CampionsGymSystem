using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class SubscriptionFreezeConfiguration : IEntityTypeConfiguration<SubscriptionFreeze>
{
    public void Configure(EntityTypeBuilder<SubscriptionFreeze> builder)
    {
        builder.ToTable("SubscriptionFreezes", table =>
        {
            table.HasCheckConstraint("CK_SubscriptionFreezes_FreezeType", "\"FreezeType\" IN ('Normal', 'Medical')");
            table.HasCheckConstraint("CK_SubscriptionFreezes_DaysApplied", "\"DaysApplied\" >= 0");
            table.HasCheckConstraint("CK_SubscriptionFreezes_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.MemberSubscription).WithMany().HasForeignKey(x => x.MemberSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.FreezeType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.Reason).HasColumnType("text");
        builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
