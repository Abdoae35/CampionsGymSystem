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
        builder.Property(x => x.OperationId);
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.Property(x => x.RequestHash).HasMaxLength(64);
        builder.Property(x => x.BusinessDate).HasColumnType("date");
        builder.Property(x => x.PurchaseMode).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.ActivationMode).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.InitialPaymentRequired).HasPrecision(18, 2);
        builder.Property(x => x.OfferValueSnapshot).HasPrecision(18, 2);
        builder.Property(x => x.OfferTypeSnapshot).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.OfferNameSnapshot).HasMaxLength(256);
        builder.Property(x => x.CancellationReason).HasMaxLength(2000);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable("SaleTransactions", t => {
            t.HasCheckConstraint("CK_SaleTransactions_Workflow", "\"OperationId\" IS NULL OR (\"OperationId\" <> '00000000-0000-0000-0000-000000000000'::uuid AND \"RequestHash\" IS NOT NULL AND \"BusinessDate\" IS NOT NULL AND \"PurchaseMode\" IS NOT NULL AND \"PurchaseMode\" IN ('FullPayment', 'Reservation', 'Installment', 'StandaloneSingleSession') AND \"ActivationMode\" IS NOT NULL AND \"ActivationMode\" IN ('Now', 'FirstCheckIn', 'SpecificDate') AND \"CommissionEligible\" IS NOT NULL AND \"TargetEligible\" IS NOT NULL AND \"NetAmount\" > 0)");
            t.HasCheckConstraint("CK_SaleTransactions_Activation", "\"OperationId\" IS NULL OR ((\"ActivationMode\" = 'SpecificDate') = (\"RequestedStartDate\" IS NOT NULL))");
            t.HasCheckConstraint("CK_SaleTransactions_ExtraDays", "\"ExtraDaysSnapshot\" >= 0");
            t.HasCheckConstraint("CK_SaleTransactions_Cancellation", "(\"CancelledAt\" IS NULL AND \"CancelledByUserId\" IS NULL AND \"CancellationReason\" IS NULL) OR (\"CancelledAt\" IS NOT NULL AND \"CancelledByUserId\" IS NOT NULL AND \"CancellationReason\" IS NOT NULL AND length(btrim(\"CancellationReason\")) > 0)");
        });
        builder.Property(x => x.OfferEligibilityModeSnapshot).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.OfferAudienceCategorySnapshot).WithMany().HasForeignKey(x => x.OfferAudienceCategoryIdSnapshot).OnDelete(DeleteBehavior.Restrict);
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
        builder.Property(x => x.SalesAttributionSnapshot).HasConversion<string>().HasMaxLength(40);
        builder.ToTable("SaleTransactions", t => t.HasCheckConstraint("CK_SaleTransactions_Attribution", "\"SalesAttributionSnapshot\" IS NULL OR (\"SalesAttributionSnapshot\" = 'House' AND \"SalesEmployeeId\" IS NULL AND \"SaleCategory\" IS NULL) OR (\"SalesAttributionSnapshot\" = 'Sales' AND \"SalesEmployeeId\" IS NOT NULL)"));
        builder.HasOne(x => x.BusinessDay).WithMany().HasForeignKey(x => x.BusinessDayId).OnDelete(DeleteBehavior.Restrict);
    }
}
