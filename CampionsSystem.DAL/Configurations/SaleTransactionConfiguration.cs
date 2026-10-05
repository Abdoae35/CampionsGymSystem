using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class SaleTransactionConfiguration : IEntityTypeConfiguration<SaleTransaction>
{
    public void Configure(EntityTypeBuilder<SaleTransaction> builder)
    {
        builder.ToTable("SaleTransactions", table =>
        {
            table.HasCheckConstraint("CK_SaleTransactions_SaleType", "\"SaleType\" IN ('GymMembership', 'Private', 'Activity', 'Other')");
            table.HasCheckConstraint("CK_SaleTransactions_SaleCategory", "\"SaleCategory\" IN ('NewMembership', 'Renewal')");
            table.HasCheckConstraint("CK_SaleTransactions_GrossAmount", "\"GrossAmount\" >= 0");
            table.HasCheckConstraint("CK_SaleTransactions_DiscountAmount", "\"DiscountAmount\" >= 0");
            table.HasCheckConstraint("CK_SaleTransactions_NetAmount", "\"NetAmount\" >= 0");
            table.HasCheckConstraint("CK_SaleTransactions_Status", "\"Status\" IN ('Open', 'Completed', 'Cancelled', 'PartiallyRefunded', 'Refunded')");
            table.HasCheckConstraint("CK_SaleTransactions_Amounts", "\"DiscountAmount\" <= \"GrossAmount\" AND \"NetAmount\" = \"GrossAmount\" - \"DiscountAmount\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedShift).WithMany().HasForeignKey(x => x.CreatedShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.SaleType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.SaleCategory).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.SalesEmployee).WithMany().HasForeignKey(x => x.SalesEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Offer).WithMany().HasForeignKey(x => x.OfferId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.GrossAmount).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.NetAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasIndex(x => new { x.MemberId, x.CreatedAt });
        builder.HasIndex(x => new { x.BranchId, x.CreatedAt });
    }
}
