using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members", table =>
        {
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MemberCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PrimaryPhone).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SecondaryPhone).HasMaxLength(64);
        builder.Property(x => x.PrimaryPhoneVerifiedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.HomeBranch).WithMany().HasForeignKey(x => x.HomeBranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.DateOfBirth).HasColumnType("date");
        builder.Property(x => x.Gender).HasMaxLength(32);
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => x.MemberCode).IsUnique();
        builder.HasIndex(x => x.PrimaryPhone).IsUnique();
    }
}
