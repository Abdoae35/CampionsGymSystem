using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampionsSystem.DAL.Configurations;
public sealed class CommissionCollectionConfiguration : IEntityTypeConfiguration<CommissionCollection>
{
    public void Configure(EntityTypeBuilder<CommissionCollection> b)
    {
        b.ToTable("CommissionCollections", t => t.HasCheckConstraint("CK_CommissionCollections_Category", "\"Category\" IN ('NewMembership','Renewal')"));
        b.HasKey(x => x.PaymentId);
        b.HasOne(x => x.Payment).WithOne().HasForeignKey<CommissionCollection>(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.EligibilityReason).HasMaxLength(80).IsRequired();
    }
}
public sealed class CommissionRefundAllocationConfiguration : IEntityTypeConfiguration<CommissionRefundAllocation>
{
    public void Configure(EntityTypeBuilder<CommissionRefundAllocation> b)
    {
        b.ToTable("CommissionRefundAllocations", t => t.HasCheckConstraint("CK_CommissionRefundAllocations_Amount", "\"Amount\" > 0"));
        b.HasKey(x => new { x.RefundId, x.PaymentId });
        b.HasOne(x => x.Refund).WithMany().HasForeignKey(x => x.RefundId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Amount).HasPrecision(18, 2);
    }
}
public sealed class CommissionStatementLineConfiguration : IEntityTypeConfiguration<CommissionStatementLine>
{
    public void Configure(EntityTypeBuilder<CommissionStatementLine> b)
    {
        b.ToTable("CommissionStatementLines", t =>
        {
            t.HasCheckConstraint("CK_CommissionStatementLines_Amounts", "\"GrossAmount\" >= 0 AND \"RefundedAmount\" >= 0 AND \"RefundedAmount\" <= \"GrossAmount\" AND \"EligibleAmount\" >= 0 AND \"EligibleAmount\" <= \"GrossAmount\" - \"RefundedAmount\"");
            t.HasCheckConstraint("CK_CommissionStatementLines_Category", "\"Category\" IN ('NewMembership','Renewal')");
        });
        b.HasKey(x => new { x.CommissionPeriodId, x.PaymentId });
        b.HasOne(x => x.CommissionPeriod).WithMany().HasForeignKey(x => x.CommissionPeriodId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.EligibilityReason).HasMaxLength(80).IsRequired();
        b.Property(x => x.GrossAmount).HasPrecision(18, 2);
        b.Property(x => x.RefundedAmount).HasPrecision(18, 2);
        b.Property(x => x.EligibleAmount).HasPrecision(18, 2);
    }
}
