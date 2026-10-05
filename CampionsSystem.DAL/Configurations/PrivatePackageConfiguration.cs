using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class PrivatePackageConfiguration : IEntityTypeConfiguration<PrivatePackage>
{
    public void Configure(EntityTypeBuilder<PrivatePackage> builder)
    {
        builder.ToTable("PrivatePackages", table =>
        {
            table.HasCheckConstraint("CK_PrivatePackages_SessionCount", "\"SessionCount\" > 0");
            table.HasCheckConstraint("CK_PrivatePackages_DurationDays", "\"DurationDays\" > 0");
            table.HasCheckConstraint("CK_PrivatePackages_Price", "\"Price\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
