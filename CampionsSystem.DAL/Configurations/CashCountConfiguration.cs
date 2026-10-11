using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class CashCountConfiguration : IEntityTypeConfiguration<CashCount>
{
    public void Configure(EntityTypeBuilder<CashCount> b)
    {
        b.ToTable("CashCounts", t =>
        {
            t.HasCheckConstraint("CK_CashCounts_Count", "\"CountedCash\" >= 0");
            t.HasCheckConstraint("CK_CashCounts_Difference", "\"Difference\" = \"CountedCash\" - \"ExpectedCash\"");
            t.HasCheckConstraint("CK_CashCounts_Kind", "\"Kind\" IN ('Initial', 'Opening', 'Closing', 'Recount')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.ObservationOrder).UseIdentityAlwaysColumn();
        b.HasIndex(x => new { x.BranchId, x.ObservationOrder });
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FinancialAccount).WithMany().HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.BusinessDay).WithMany().HasForeignKey(x => x.BusinessDayId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.BusinessDate).HasColumnType("date");
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Kind).HasMaxLength(40).IsRequired();
        b.Property(x => x.ExpectedCash).HasPrecision(18, 2);
        b.Property(x => x.CountedCash).HasPrecision(18, 2);
        b.Property(x => x.BookCash).HasPrecision(18, 2);
        b.Property(x => x.Difference).HasPrecision(18, 2);
        b.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Reason).HasColumnType("text");
        b.HasIndex(x => x.OperationId).IsUnique();
        b.HasIndex(x => new { x.BranchId, x.OccurredAt });
        b.HasIndex(x => new { x.BranchId, x.MovementSequence });
    }
}
