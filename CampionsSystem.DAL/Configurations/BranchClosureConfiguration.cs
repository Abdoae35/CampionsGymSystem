using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class BranchClosureConfiguration : IEntityTypeConfiguration<BranchClosure>
{
    public void Configure(EntityTypeBuilder<BranchClosure> builder)
    {
        builder.ToTable("BranchClosures", table =>
        {
            table.HasCheckConstraint("CK_BranchClosures_ExtensionDays", "\"ExtensionDays\" >= 0");
            table.HasCheckConstraint("CK_BranchClosures_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.Reason).HasColumnType("text").IsRequired();
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
