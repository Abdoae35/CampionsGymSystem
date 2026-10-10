using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampionsSystem.DAL.Configurations;
public sealed class MemberAudienceCategoryConfiguration : IEntityTypeConfiguration<MemberAudienceCategory>
{
    public void Configure(EntityTypeBuilder<MemberAudienceCategory> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.MembershipPlanCategory).WithMany().HasForeignKey(x => x.MembershipPlanCategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.MemberId, x.MembershipPlanCategoryId }).IsUnique().HasFilter("\"EndedAt\" IS NULL");
        b.ToTable("MemberAudienceCategories", t => t.HasCheckConstraint("CK_MemberAudienceCategories_Period", "\"EndedAt\" >= \"AssignedAt\""));
    }
}
