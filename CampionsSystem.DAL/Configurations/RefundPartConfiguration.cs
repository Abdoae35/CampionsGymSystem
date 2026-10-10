using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class RefundPartConfiguration : IEntityTypeConfiguration<RefundPart>
{
    public void Configure(EntityTypeBuilder<RefundPart> builder)
    {
        builder.ToTable("RefundParts", table =>
        {
            table.HasCheckConstraint("CK_RefundParts_Amount", "\"Amount\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Refund).WithMany(x => x.Parts).HasForeignKey(x => x.RefundId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FinancialAccount).WithMany().HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.ProviderReference).HasMaxLength(256);
    }
}
