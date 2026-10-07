using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ActivitySubscriptionTransactionConfiguration : IEntityTypeConfiguration<ActivitySubscriptionTransaction>
{
    public void Configure(EntityTypeBuilder<ActivitySubscriptionTransaction> builder)
    {
        builder.ToTable("ActivitySubscriptionTransactions", table =>
        {
            table.HasCheckConstraint("CK_ActivitySubscriptionTransactions_TransactionType", "\"TransactionType\" IN ('Initial', 'Renewal')");
            table.HasCheckConstraint("CK_ActivitySubscriptionTransactions_SessionsAdded", "\"SessionsAdded\" > 0");
            table.HasCheckConstraint("CK_ActivitySubscriptionTransactions_DurationDaysAdded", "\"DurationDaysAdded\" > 0");
            table.HasCheckConstraint("CK_ActivitySubscriptionTransactions_NetAmount", "\"NetAmount\" >= 0");
            table.HasCheckConstraint("CK_ActivitySubscriptionTransactions_UnitSessionValue", "\"UnitSessionValue\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.ActivitySubscription).WithMany().HasForeignKey(x => x.ActivitySubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SaleTransaction).WithMany().HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ActivityPackage).WithMany().HasForeignKey(x => x.ActivityPackageId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.NetAmount).HasPrecision(18, 2);
        builder.Property(x => x.UnitSessionValue).HasPrecision(18, 2);
        builder.Property(x => x.PreviousEndDate).HasColumnType("date");
        builder.Property(x => x.NewEndDate).HasColumnType("date");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.ActivityCoachSnapshot).WithMany().HasForeignKey(x => x.ActivityCoachIdSnapshot).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CoachPercentageSnapshot).HasPrecision(5, 2);
        builder.ToTable("ActivitySubscriptionTransactions", t => t.HasCheckConstraint("CK_ActivitySubscriptionTransactions_CoachSnapshot", "(\"ActivityCoachIdSnapshot\" IS NULL AND \"CoachPercentageSnapshot\" IS NULL) OR (\"ActivityCoachIdSnapshot\" IS NOT NULL AND \"CoachPercentageSnapshot\" IS NOT NULL AND \"CoachPercentageSnapshot\" BETWEEN 0 AND 100)"));
    }
}
