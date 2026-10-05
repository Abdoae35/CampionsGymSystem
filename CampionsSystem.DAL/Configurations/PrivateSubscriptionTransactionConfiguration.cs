using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class PrivateSubscriptionTransactionConfiguration : IEntityTypeConfiguration<PrivateSubscriptionTransaction>
{
    public void Configure(EntityTypeBuilder<PrivateSubscriptionTransaction> builder)
    {
        builder.ToTable("PrivateSubscriptionTransactions", table =>
        {
            table.HasCheckConstraint("CK_PrivateSubscriptionTransactions_TransactionType", "\"TransactionType\" IN ('Initial', 'Renewal')");
            table.HasCheckConstraint("CK_PrivateSubscriptionTransactions_SessionsAdded", "\"SessionsAdded\" > 0");
            table.HasCheckConstraint("CK_PrivateSubscriptionTransactions_DurationDaysAdded", "\"DurationDaysAdded\" > 0");
            table.HasCheckConstraint("CK_PrivateSubscriptionTransactions_NetAmount", "\"NetAmount\" >= 0");
            table.HasCheckConstraint("CK_PrivateSubscriptionTransactions_UnitSessionValue", "\"UnitSessionValue\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.PrivateSubscription).WithMany(x => x.Transactions).HasForeignKey(x => x.PrivateSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SaleTransaction).WithMany().HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PrivatePackage).WithMany().HasForeignKey(x => x.PrivatePackageId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.NetAmount).HasPrecision(18, 2);
        builder.Property(x => x.UnitSessionValue).HasPrecision(18, 2);
        builder.Property(x => x.PreviousEndDate).HasColumnType("date");
        builder.Property(x => x.NewEndDate).HasColumnType("date");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
