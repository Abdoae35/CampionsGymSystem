using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class GymBranchConfiguration : IEntityTypeConfiguration<GymBranch>
{
    public void Configure(EntityTypeBuilder<GymBranch> builder)
    {
        builder.ToTable("GymBranches", table =>
        {
            table.HasCheckConstraint("CK_GymBranches_MaxBorrowedGymSessions", "\"MaxBorrowedGymSessions\" >= 0");
            table.HasCheckConstraint("CK_GymBranches_QuickRefundWindowHours", "\"QuickRefundWindowHours\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.City).WithMany(x => x.Branches).HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(256);
        builder.Property(x => x.Phone).HasMaxLength(64);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.MaxBorrowedGymSessions).HasDefaultValue(2);
        builder.Property(x => x.QuickRefundWindowHours).HasDefaultValue(24);
    }
}
