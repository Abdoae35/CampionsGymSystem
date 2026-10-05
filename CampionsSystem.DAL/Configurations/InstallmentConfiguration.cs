using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class InstallmentConfiguration : IEntityTypeConfiguration<Installment>
{
    public void Configure(EntityTypeBuilder<Installment> builder)
    {
        builder.ToTable("Installments", table =>
        {
            table.HasCheckConstraint("CK_Installments_InstallmentNumber", "\"InstallmentNumber\" > 0");
            table.HasCheckConstraint("CK_Installments_AmountDue", "\"AmountDue\" >= 0");
            table.HasCheckConstraint("CK_Installments_AmountPaid", "\"AmountPaid\" >= 0");
            table.HasCheckConstraint("CK_Installments_Status", "\"Status\" IN ('Pending', 'PartiallyPaid', 'Paid', 'Overdue')");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.InstallmentAgreement).WithMany(x => x.Installments).HasForeignKey(x => x.InstallmentAgreementId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.DueDate).HasColumnType("date");
        builder.Property(x => x.AmountDue).HasPrecision(18, 2);
        builder.Property(x => x.AmountPaid).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.PaidAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.InstallmentAgreementId, x.InstallmentNumber }).IsUnique();
    }
}
