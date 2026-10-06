using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class PrivateSessionConfiguration : IEntityTypeConfiguration<PrivateSession>
{
    public void Configure(EntityTypeBuilder<PrivateSession> builder)
    {
        builder.ToTable("PrivateSessions", table =>
        {
        });
        builder.HasOne(x => x.PrivateSubscriptionTransaction).WithMany(x => x.Sessions).HasForeignKey(x => x.PrivateSubscriptionTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CommissionPeriod).WithMany().HasForeignKey(x => x.CommissionPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CoachEmployeeId, x.SessionAt });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.PrivateSubscription).WithMany(x => x.Sessions).HasForeignKey(x => x.PrivateSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CoachEmployee).WithMany().HasForeignKey(x => x.CoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.GymCheckIn).WithMany().HasForeignKey(x => x.GymCheckInId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.SessionAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasIndex(x => new { x.PrivateSubscriptionId, x.SessionAt });
    }
}
