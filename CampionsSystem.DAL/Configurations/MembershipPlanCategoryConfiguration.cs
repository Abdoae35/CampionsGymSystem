using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MembershipPlanCategoryConfiguration : IEntityTypeConfiguration<MembershipPlanCategory>
{
    public void Configure(EntityTypeBuilder<MembershipPlanCategory> b)
    {
        b.HasKey(x => x.Id);
        b.ToTable("MembershipPlanCategories", t =>
        {
            t.HasCheckConstraint("CK_MembershipPlanCategories_Name", "length(trim(\"Name\")) > 0");
        });
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.Description).HasColumnType("text");
        b.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        b.HasIndex(x => x.Name).IsUnique();
    }
}
