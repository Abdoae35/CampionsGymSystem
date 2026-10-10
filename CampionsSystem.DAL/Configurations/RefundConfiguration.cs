using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds", table =>
        {
            table.HasCheckConstraint("CK_Refunds_Amount", "\"Amount\" >= 0");
            table.HasCheckConstraint("CK_Refunds_RefundType", "\"RefundType\" IN ('QuickRefund', 'ManagerRefund')");
        });
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.Property(x => x.RequestHash).HasMaxLength(64);
        builder.Property(x => x.BusinessDate).HasColumnType("date");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.SaleTransaction).WithMany(x => x.Refunds).HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OriginalPayment).WithMany().HasForeignKey(x => x.OriginalPaymentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.RefundType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Reason).HasColumnType("text").IsRequired();
        builder.HasOne(x => x.ProcessedByUser).WithMany().HasForeignKey(x => x.ProcessedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.BusinessDay).WithMany().HasForeignKey(x => x.BusinessDayId).OnDelete(DeleteBehavior.Restrict);
    }
}
