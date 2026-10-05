using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class AuthorizedDeviceConfiguration : IEntityTypeConfiguration<AuthorizedDevice>
{
    public void Configure(EntityTypeBuilder<AuthorizedDevice> builder)
    {
        builder.ToTable("AuthorizedDevices", table =>
        {
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.DeviceName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.DeviceKeyHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RegisteredAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.RegisteredByUser).WithMany().HasForeignKey(x => x.RegisteredByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.RevokedAt).HasColumnType("timestamp with time zone");
    }
}
