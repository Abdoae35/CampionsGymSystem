using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MembershipSubscriptionTransactionConfiguration : IEntityTypeConfiguration<MembershipSubscriptionTransaction>
{
    public void Configure(EntityTypeBuilder<MembershipSubscriptionTransaction> builder)
    {
        builder.ToTable("MembershipSubscriptionTransactions", table =>
        {
            table.HasCheckConstraint("CK_MembershipSubscriptionTransactions_TransactionType", "\"TransactionType\" IN ('Initial', 'Renewal')");
            table.HasCheckConstraint("CK_MembershipSubscriptionTransactions_PurchasedDurationDays", "\"PurchasedDurationDays\" > 0");
            table.HasCheckConstraint("CK_MembershipSubscriptionTransactions_PurchasedSessions", "\"PurchasedSessions\" > 0");
            table.HasCheckConstraint("CK_MembershipSubscriptionTransactions_PlanPriceSnapshot", "\"PlanPriceSnapshot\" >= 0");
            table.HasCheckConstraint("CK_MembershipSubscriptionTransactions_NetPriceSnapshot", "\"NetPriceSnapshot\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.MemberSubscription).WithMany().HasForeignKey(x => x.MemberSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SaleTransaction).WithMany().HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.MembershipPlan).WithMany().HasForeignKey(x => x.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.PlanPriceSnapshot).HasPrecision(18, 2);
        builder.Property(x => x.NetPriceSnapshot).HasPrecision(18, 2);
        builder.Property(x => x.PreviousEndDate).HasColumnType("date");
        builder.Property(x => x.NewEndDate).HasColumnType("date");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
