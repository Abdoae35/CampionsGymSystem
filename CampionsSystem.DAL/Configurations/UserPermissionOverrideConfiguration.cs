using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class UserPermissionOverrideConfiguration : IEntityTypeConfiguration<UserPermissionOverride>
{
    public void Configure(EntityTypeBuilder<UserPermissionOverride> b)
    {
        b.HasKey(x => new { x.UserId, x.PermissionId });
        b.ToTable("UserPermissionOverrides", t =>
        {
            t.HasCheckConstraint("CK_UserPermissionOverrides_Effect", "\"Effect\" IN ('Allow', 'Deny')");
        });
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Effect).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
