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
            table.HasCheckConstraint("CK_SubscriptionFreezes_CompensatedDays", "\"DaysAlreadyCompensated\" >= 0 AND \"DaysAlreadyCompensated\" <= \"DaysApplied\"");
            table.HasCheckConstraint("CK_SubscriptionFreezes_DaysApplied", "\"DaysApplied\" >= 0");
            table.HasCheckConstraint("CK_SubscriptionFreezes_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
        });
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.PreviousSubscriptionEndDate).HasColumnType("date");
        builder.Property(x => x.NewSubscriptionEndDate).HasColumnType("date");
        builder.HasIndex(x => new { x.MemberSubscriptionId, x.StartDate });
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
