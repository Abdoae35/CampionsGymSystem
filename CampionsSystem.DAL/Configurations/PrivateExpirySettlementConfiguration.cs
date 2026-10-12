using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class PrivateExpirySettlementConfiguration : IEntityTypeConfiguration<PrivateExpirySettlement>
{
    public void Configure(EntityTypeBuilder<PrivateExpirySettlement> builder)
    {
        builder.ToTable("PrivateExpirySettlements", table =>
        {
            table.HasCheckConstraint("CK_PrivateExpirySettlements_RemainingSessions", "\"RemainingSessions\" > 0");
            table.HasCheckConstraint("CK_PrivateExpirySettlements_UnitSessionValue", "\"UnitSessionValue\" >= 0");
            table.HasCheckConstraint("CK_PrivateExpirySettlements_EligibleAmount", "\"EligibleAmount\" = \"RemainingSessions\" * \"UnitSessionValue\" + \"RemainderValue\"");
        });
        builder.HasOne(x => x.PrivateCoachAssignment).WithMany().HasForeignKey(x => x.PrivateCoachAssignmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.Property(x => x.RequestHash).HasMaxLength(64);
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.Property(x => x.RemainderValue).HasPrecision(18, 2);
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.PrivateSubscription).WithMany(x => x.ExpirySettlements).HasForeignKey(x => x.PrivateSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PrivateSubscriptionTransaction).WithOne(x => x.ExpirySettlement).HasForeignKey<PrivateExpirySettlement>(x => x.PrivateSubscriptionTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CoachEmployee).WithMany().HasForeignKey(x => x.CoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CommissionPeriod).WithMany().HasForeignKey(x => x.CommissionPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.UnitSessionValue).HasPrecision(18, 2);
        builder.Property(x => x.EligibleAmount).HasPrecision(18, 2);
        builder.Property(x => x.SettledAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
