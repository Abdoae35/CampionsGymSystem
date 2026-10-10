using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampionsSystem.DAL.Configurations;
public sealed class SubscriptionGrantCancellationConfiguration : IEntityTypeConfiguration<SubscriptionGrantCancellation>
{
    public void Configure(EntityTypeBuilder<SubscriptionGrantCancellation> b)
    {
        b.ToTable("SubscriptionGrantCancellations",t=>t.HasCheckConstraint("CK_SubscriptionGrantCancellations_Sessions","\"DaysRemoved\" >= 0 AND \"PreviousRemainingSessions\" >= 0 AND \"NewRemainingSessions\" >= 0"));
        b.HasKey(x=>x.Id);b.HasIndex(x=>x.GrantId).IsUnique();b.HasIndex(x=>x.RefundId).IsUnique();
        b.HasOne(x=>x.Grant).WithMany().HasForeignKey(x=>x.GrantId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x=>x.Refund).WithMany().HasForeignKey(x=>x.RefundId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x=>x.PreviousEndDate).HasColumnType("date");b.Property(x=>x.NewEndDate).HasColumnType("date");
        b.Property(x=>x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
