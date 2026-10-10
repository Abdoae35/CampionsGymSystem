using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class CashDrawerConfiguration : IEntityTypeConfiguration<CashDrawer>
{
    public void Configure(EntityTypeBuilder<CashDrawer> b)
    {
        b.ToTable("CashDrawers", t =>
        {
            t.HasCheckConstraint("CK_CashDrawers_Sequence", "\"MovementSequence\" >= 0");
        });
        b.HasKey(x => x.BranchId);
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FinancialAccount).WithMany().HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.OperationId).IsUnique();
        b.HasIndex(x => x.FinancialAccountId).IsUnique();
    }
}
