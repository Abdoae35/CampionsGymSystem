using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class OfferActivityPackageConfiguration : IEntityTypeConfiguration<OfferActivityPackage>
{
    public void Configure(EntityTypeBuilder<OfferActivityPackage> builder)
    {
        builder.ToTable("OfferActivityPackages", table =>
        {
        });
        builder.HasKey(x => new { x.OfferId, x.ActivityPackageId });
        builder.HasOne(x => x.Offer).WithMany().HasForeignKey(x => x.OfferId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ActivityPackage).WithMany().HasForeignKey(x => x.ActivityPackageId).OnDelete(DeleteBehavior.Restrict);
    }
}
