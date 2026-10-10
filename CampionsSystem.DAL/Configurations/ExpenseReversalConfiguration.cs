using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ExpenseReversalConfiguration : IEntityTypeConfiguration<ExpenseReversal>
{
    public void Configure(EntityTypeBuilder<ExpenseReversal> b)
    {
        b.ToTable("ExpenseReversals", t =>
        {
            t.HasCheckConstraint("CK_ExpenseReversals_Amount", "\"Amount\" > 0");
        });
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Expense).WithMany().HasForeignKey(x => x.ExpenseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.BusinessDay).WithMany().HasForeignKey(x => x.BusinessDayId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.BusinessDate).HasColumnType("date");
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Reason).HasColumnType("text").IsRequired();
        b.Property(x => x.ReturnReference).HasMaxLength(512);
        b.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.OperationId).IsUnique();
        b.HasIndex(x => new { x.BranchId, x.OccurredAt });
    }
}
