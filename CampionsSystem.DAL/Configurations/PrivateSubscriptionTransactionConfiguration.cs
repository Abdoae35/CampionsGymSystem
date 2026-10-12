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
        builder.ToTable("PrivateSubscriptionTransactions", table => table.HasCheckConstraint(
            "CK_PrivateSubscriptionTransactions_RemainingSessions", "\"RemainingSessions\" >= 0 AND \"RemainingSessions\" <= \"SessionsAdded\""));
        // Purchase facts are immutable; RemainingSessions is the only mutable batch counter.
        foreach (var name in new[]
        {
            nameof(PrivateSubscriptionTransaction.PrivateSubscriptionId),
            nameof(PrivateSubscriptionTransaction.SaleTransactionId),
            nameof(PrivateSubscriptionTransaction.PrivatePackageId),
            nameof(PrivateSubscriptionTransaction.TransactionType),
            nameof(PrivateSubscriptionTransaction.SessionsAdded),
            nameof(PrivateSubscriptionTransaction.DurationDaysAdded),
            nameof(PrivateSubscriptionTransaction.NetAmount),
            nameof(PrivateSubscriptionTransaction.UnitSessionValue),
            nameof(PrivateSubscriptionTransaction.PreviousEndDate),
            nameof(PrivateSubscriptionTransaction.NewEndDate),
            nameof(PrivateSubscriptionTransaction.CreatedAt)
        })
            builder.Property(name).Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Throw);
        builder.HasIndex(x => new { x.PrivateSubscriptionId, x.CreatedAt, x.Id });
        builder.HasOne(x => x.OriginalCoachEmployee).WithMany().HasForeignKey(x => x.OriginalCoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OriginalAssignment).WithMany().HasForeignKey(x => x.OriginalAssignmentId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.PackageNameSnapshot).HasMaxLength(256);
        builder.Property(x => x.RemainderValue).HasPrecision(18, 2);
        builder.HasIndex(x => x.SaleTransactionId).IsUnique();
        builder.ToTable("PrivateSubscriptionTransactions", t => t.HasCheckConstraint("CK_PrivateSubscriptionTransactions_Snapshot", "\"OriginalCoachEmployeeId\" IS NULL OR (\"OriginalAssignmentId\" IS NOT NULL AND \"StartDateSnapshot\" IS NOT NULL AND \"PackageNameSnapshot\" IS NOT NULL AND \"NetAmount\" = \"SessionsAdded\" * \"UnitSessionValue\" + \"RemainderValue\" AND \"RemainderValue\" >= 0 AND \"RemainderValue\" < \"SessionsAdded\" * 0.01)"));
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
