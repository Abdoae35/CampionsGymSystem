using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class FinancialAccountConfiguration : IEntityTypeConfiguration<FinancialAccount>
{
    public void Configure(EntityTypeBuilder<FinancialAccount> builder)
    {
        builder.ToTable("FinancialAccounts", table =>
        {
            table.HasCheckConstraint("CK_FinancialAccounts_AccountType", "\"AccountType\" IN ('Cash', 'InstaPay', 'Bank', 'Card', 'Wallet')");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.AccountType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.OpeningBalance).HasPrecision(18, 2);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
