using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("Offers", table =>
        {
            table.HasCheckConstraint("CK_Offers_OfferType", "\"OfferType\" IN ('PercentageDiscount', 'FixedDiscount', 'ExtraDays')");
            table.HasCheckConstraint("CK_Offers_Value", "\"Value\" >= 0");
            table.HasCheckConstraint("CK_Offers_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
            table.HasCheckConstraint("CK_Offers_Percentage", "\"OfferType\" <> 'PercentageDiscount' OR \"Value\" <= 100");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.OfferType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Value).HasPrecision(18, 2);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.EligibilityDescription).HasColumnType("text");
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
