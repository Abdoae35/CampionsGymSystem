using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class OfferMembershipPlanConfiguration : IEntityTypeConfiguration<OfferMembershipPlan>
{
    public void Configure(EntityTypeBuilder<OfferMembershipPlan> builder)
    {
        builder.ToTable("OfferMembershipPlans", table =>
        {
        });
        builder.HasKey(x => new { x.OfferId, x.MembershipPlanId });
        builder.HasOne(x => x.Offer).WithMany().HasForeignKey(x => x.OfferId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.MembershipPlan).WithMany().HasForeignKey(x => x.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
    }
}
