using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class SportsActivityConfiguration : IEntityTypeConfiguration<SportsActivity>
{
    public void Configure(EntityTypeBuilder<SportsActivity> builder)
    {
        builder.ToTable("SportsActivities", table =>
        {
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
