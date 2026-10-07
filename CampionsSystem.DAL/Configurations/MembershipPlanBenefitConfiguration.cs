using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MembershipPlanBenefitConfiguration : IEntityTypeConfiguration<MembershipPlanBenefit>
{
    public void Configure(EntityTypeBuilder<MembershipPlanBenefit> builder)
    {
        builder.ToTable("MembershipPlanBenefits", table => table.HasCheckConstraint(
            "CK_MembershipPlanBenefits_GrantQuantity",
            "(\"GrantType\" = 'Quantity' AND \"Quantity\" IS NOT NULL AND \"Quantity\" > 0) OR (\"GrantType\" = 'Access' AND \"Quantity\" IS NULL)"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GrantType).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.MembershipPlan).WithMany().HasForeignKey(x => x.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.MembershipBenefit).WithMany().HasForeignKey(x => x.MembershipBenefitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.MembershipPlanId, x.MembershipBenefitId }).IsUnique();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
