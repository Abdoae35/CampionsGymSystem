using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MemberSubscriptionBenefitConfiguration : IEntityTypeConfiguration<MemberSubscriptionBenefit>
{
    public void Configure(EntityTypeBuilder<MemberSubscriptionBenefit> builder)
    {
        builder.ToTable("MemberSubscriptionBenefits", table => table.HasCheckConstraint(
            "CK_MemberSubscriptionBenefits_GrantQuantity",
            "(\"GrantTypeSnapshot\" = 'Quantity' AND \"GrantedQuantity\" IS NOT NULL AND \"GrantedQuantity\" > 0) OR (\"GrantTypeSnapshot\" = 'Access' AND \"GrantedQuantity\" IS NULL)"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BenefitCodeSnapshot).HasMaxLength(128).IsRequired();
        builder.Property(x => x.BenefitNameSnapshot).HasMaxLength(256).IsRequired();
        builder.Property(x => x.GrantTypeSnapshot).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.MembershipSubscriptionTransaction).WithMany().HasForeignKey(x => x.MembershipSubscriptionTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.MembershipBenefit).WithMany().HasForeignKey(x => x.MembershipBenefitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.MembershipSubscriptionTransactionId, x.MembershipBenefitId }).IsUnique();
        builder.Property(x => x.GrantedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
