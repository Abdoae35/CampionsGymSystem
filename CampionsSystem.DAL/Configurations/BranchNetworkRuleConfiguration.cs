using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class BranchNetworkRuleConfiguration : IEntityTypeConfiguration<BranchNetworkRule>
{
    public void Configure(EntityTypeBuilder<BranchNetworkRule> builder)
    {
        builder.ToTable("BranchNetworkRules", table =>
        {
            table.HasCheckConstraint("CK_BranchNetworkRules_RuleType", "\"RuleType\" IN ('PublicIP', 'CIDR', 'VPN')");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.RuleType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.RuleValue).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
