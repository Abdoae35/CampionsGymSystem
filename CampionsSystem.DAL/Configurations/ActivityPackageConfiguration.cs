using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ActivityPackageConfiguration : IEntityTypeConfiguration<ActivityPackage>
{
    public void Configure(EntityTypeBuilder<ActivityPackage> builder)
    {
        builder.ToTable("ActivityPackages", table =>
        {
            table.HasCheckConstraint("CK_ActivityPackages_AccessMode", "\"AccessMode\" IN ('Unlimited', 'SessionBased')");
            table.HasCheckConstraint("CK_ActivityPackages_SessionCount", "\"SessionCount\" > 0");
            table.HasCheckConstraint("CK_ActivityPackages_DurationDays", "\"DurationDays\" > 0");
            table.HasCheckConstraint("CK_ActivityPackages_Price", "\"Price\" >= 0");
            table.HasCheckConstraint("CK_ActivityPackages_IncludedFreezeDays", "\"IncludedFreezeDays\" >= 0");
            table.HasCheckConstraint("CK_ActivityPackages_AccessSessions", "(\"AccessMode\" = 'Unlimited' AND \"SessionCount\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"SessionCount\" IS NOT NULL AND \"SessionCount\" > 0)");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.SportsActivity).WithMany().HasForeignKey(x => x.SportsActivityId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.AccessMode).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.IncludedFreezeDays).HasDefaultValue(0);
    }
}
