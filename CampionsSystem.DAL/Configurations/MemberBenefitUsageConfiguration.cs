using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MemberBenefitUsageConfiguration : IEntityTypeConfiguration<MemberBenefitUsage>
{
    public void Configure(EntityTypeBuilder<MemberBenefitUsage> builder)
    {
        builder.ToTable("MemberBenefitUsages", table => table.HasCheckConstraint("CK_MemberBenefitUsages_QuantityUsed", "\"QuantityUsed\" > 0"));
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.MemberSubscriptionBenefit).WithMany().HasForeignKey(x => x.MemberSubscriptionBenefitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.MemberSubscriptionBenefitId, x.UsedAt });
        builder.HasIndex(x => new { x.BranchId, x.UsedAt });
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.Property(x => x.UsedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
