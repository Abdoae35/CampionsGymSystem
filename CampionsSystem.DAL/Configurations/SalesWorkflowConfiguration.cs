using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampionsSystem.DAL.Configurations;
public sealed class SaleMembershipSnapshotConfiguration : IEntityTypeConfiguration<SaleMembershipSnapshot>
{
    public void Configure(EntityTypeBuilder<SaleMembershipSnapshot> b)
    {
        b.HasKey(x => x.SaleTransactionId);
        b.HasOne(x => x.SaleTransaction).WithOne(x => x.MembershipSnapshot).HasForeignKey<SaleMembershipSnapshot>(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.MembershipPlan).WithMany().HasForeignKey(x => x.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FreelanceCoachEmployee).WithMany().HasForeignKey(x => x.FreelanceCoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.PlanName).HasMaxLength(256).IsRequired();
        b.Property(x => x.PlanCategory).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.AccessMode).HasConversion<string>().HasMaxLength(40);
        b.ToTable("SaleMembershipSnapshots", t => {
            t.HasCheckConstraint("CK_SaleMembershipSnapshots_Duration", "\"DurationDays\" > 0 AND \"FreezeDays\" >= 0");
            t.HasCheckConstraint("CK_SaleMembershipSnapshots_Mode", "(\"AccessMode\" = 'Unlimited' AND \"SessionCount\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"SessionCount\" IS NOT NULL AND \"SessionCount\" > 0)");
            t.HasCheckConstraint("CK_SaleMembershipSnapshots_Type", "(\"PlanCategory\" = 'Normal' AND \"FreelanceCoachEmployeeId\" IS NULL) OR (\"PlanCategory\" = 'Freelance' AND \"FreelanceCoachEmployeeId\" IS NOT NULL)");
        });
    }
}
public sealed class SaleBenefitSnapshotConfiguration : IEntityTypeConfiguration<SaleBenefitSnapshot>
{
    public void Configure(EntityTypeBuilder<SaleBenefitSnapshot> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.SaleMembershipSnapshot).WithMany(x => x.Benefits).HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.MembershipBenefit).WithMany().HasForeignKey(x => x.MembershipBenefitId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Code).HasMaxLength(80).IsRequired(); b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.GrantType).HasConversion<string>().HasMaxLength(40);
        b.HasIndex(x => new { x.SaleTransactionId, x.MembershipBenefitId }).IsUnique();
        b.ToTable("SaleBenefitSnapshots", t => t.HasCheckConstraint("CK_SaleBenefitSnapshots_Quantity", "(\"GrantType\" = 'Access' AND \"Quantity\" IS NULL) OR (\"GrantType\" = 'Quantity' AND \"Quantity\" IS NOT NULL AND \"Quantity\" > 0)"));
    }
}
public sealed class InstallmentPaymentAllocationConfiguration : IEntityTypeConfiguration<InstallmentPaymentAllocation>
{
    public void Configure(EntityTypeBuilder<InstallmentPaymentAllocation> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Installment).WithMany().HasForeignKey(x => x.InstallmentId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.HasIndex(x => new { x.PaymentId, x.InstallmentId }).IsUnique();
        b.ToTable("InstallmentPaymentAllocations", t => t.HasCheckConstraint("CK_InstallmentPaymentAllocations_Amount", "\"Amount\" > 0"));
    }
}
