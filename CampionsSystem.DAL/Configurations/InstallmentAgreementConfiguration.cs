using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class InstallmentAgreementConfiguration : IEntityTypeConfiguration<InstallmentAgreement>
{
    public void Configure(EntityTypeBuilder<InstallmentAgreement> builder)
    {
        builder.ToTable("InstallmentAgreements", table =>
        {
            table.HasCheckConstraint("CK_InstallmentAgreements_TotalAmount", "\"TotalAmount\" >= 0");
            table.HasCheckConstraint("CK_InstallmentAgreements_NumberOfInstallments", "\"NumberOfInstallments\" > 0");
            table.HasCheckConstraint("CK_InstallmentAgreements_Status", "\"Status\" IN ('Active', 'Completed', 'Defaulted', 'Cancelled')");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.SaleTransaction).WithMany().HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
