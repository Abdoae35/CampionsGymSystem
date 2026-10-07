using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MembershipPlanEligibleMemberConfiguration : IEntityTypeConfiguration<MembershipPlanEligibleMember>
{
    public void Configure(EntityTypeBuilder<MembershipPlanEligibleMember> b)
    {
        b.HasKey(x => x.Id);
        b.ToTable("MembershipPlanEligibleMembers", t =>
        {
        });
        b.HasOne(x => x.MembershipPlan).WithMany().HasForeignKey(x => x.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.MembershipPlanId, x.MemberId }).IsUnique();
    }
}
