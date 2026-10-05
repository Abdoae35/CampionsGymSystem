using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class PaymentPartConfiguration : IEntityTypeConfiguration<PaymentPart>
{
    public void Configure(EntityTypeBuilder<PaymentPart> builder)
    {
        builder.ToTable("PaymentParts", table =>
        {
            table.HasCheckConstraint("CK_PaymentParts_Amount", "\"Amount\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Payment).WithMany(x => x.Parts).HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FinancialAccount).WithMany().HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.ProviderReference).HasMaxLength(256);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
