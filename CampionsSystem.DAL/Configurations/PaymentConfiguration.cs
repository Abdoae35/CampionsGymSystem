using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", table =>
        {
            table.HasCheckConstraint("CK_Payments_Amount", "\"Amount\" >= 0");
            table.HasCheckConstraint("CK_Payments_Status", "\"Status\" IN ('Completed', 'Cancelled')");
        });
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.Property(x => x.RequestHash).HasMaxLength(64);
        builder.Property(x => x.BusinessDate).HasColumnType("date");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.SaleTransaction).WithMany(x => x.Payments).HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CommissionPeriod).WithMany().HasForeignKey(x => x.CommissionPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Installment).WithMany().HasForeignKey(x => x.InstallmentId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.PaidAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.ReceivedByUser).WithMany().HasForeignKey(x => x.ReceivedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasIndex(x => new { x.BranchId, x.PaidAt });
        builder.HasIndex(x => x.CommissionPeriodId);
        builder.HasOne(x => x.BusinessDay).WithMany().HasForeignKey(x => x.BusinessDayId).OnDelete(DeleteBehavior.Restrict);
    }
}
