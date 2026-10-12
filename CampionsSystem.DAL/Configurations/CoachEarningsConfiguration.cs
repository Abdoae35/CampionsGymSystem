using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class CoachTargetRuleConfiguration : IEntityTypeConfiguration<CoachTargetRule>
{
    public void Configure(EntityTypeBuilder<CoachTargetRule> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.CommissionPeriod).WithMany().HasForeignKey(x => x.CommissionPeriodId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CoachEmployee).WithMany().HasForeignKey(x => x.CoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.CommissionPeriodId, x.CoachEmployeeId }).IsUnique();
        b.HasIndex(x => x.OperationId).IsUnique();
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.PrivateTarget).HasPrecision(18, 2);
        b.Property(x => x.ClassesAverageTarget).HasPrecision(18, 4);
        b.ToTable("CoachTargetRules", t => t.HasCheckConstraint("CK_CoachTargetRules_Targets",
            "(\"PrivateTarget\" IS NULL OR \"PrivateTarget\" > 0) AND (\"ClassesAverageTarget\" IS NULL OR \"ClassesAverageTarget\" > 0) AND (\"PrivateTarget\" IS NOT NULL OR \"ClassesAverageTarget\" IS NOT NULL)"));
    }
}

public sealed class CoachEarningConfiguration : IEntityTypeConfiguration<CoachEarning>
{
    public void Configure(EntityTypeBuilder<CoachEarning> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CoachEmployee).WithMany().HasForeignKey(x => x.CoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CommissionPeriod).WithMany().HasForeignKey(x => x.CommissionPeriodId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CoachTargetRule).WithMany().HasForeignKey(x => x.CoachTargetRuleId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SaleTransaction).WithMany().HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PrivateSession).WithMany().HasForeignKey(x => x.PrivateSessionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PrivateExpirySettlement).WithMany().HasForeignKey(x => x.PrivateExpirySettlementId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        foreach (var name in new[] { "EligibleValue", "Amount", "TargetProgress", "Target" }) b.Property(name).HasPrecision(18, 2);
        b.Property(x => x.Rate).HasPrecision(5, 2);
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => new { x.BranchId, x.CoachEmployeeId, x.BusinessDate });
        b.HasIndex(x => new { x.CommissionPeriodId, x.PrivateSessionId }).IsUnique().HasFilter("\"PrivateSessionId\" IS NOT NULL");
        b.HasIndex(x => new { x.CommissionPeriodId, x.PrivateExpirySettlementId }).IsUnique().HasFilter("\"PrivateExpirySettlementId\" IS NOT NULL");
        b.HasIndex(x => x.RecognitionOperationId);
        b.ToTable("CoachEarnings", t =>
        {
            t.HasCheckConstraint("CK_CoachEarnings_Source", "(\"PrivateSessionId\" IS NOT NULL) <> (\"PrivateExpirySettlementId\" IS NOT NULL)");
            t.HasCheckConstraint("CK_CoachEarnings_Amounts", "\"EligibleValue\" > 0 AND \"Amount\" >= 0 AND \"Amount\" <= \"EligibleValue\" AND \"TargetProgress\" >= 0");
            t.HasCheckConstraint("CK_CoachEarnings_Rate", "(\"TransferredPackage\" AND \"Rate\" = 50) OR (NOT \"TransferredPackage\" AND \"CoachTargetRuleId\" IS NOT NULL AND \"Target\" IS NOT NULL AND \"Target\" > 0 AND ((\"TargetProgress\" >= \"Target\" AND \"Rate\" = 60) OR (\"TargetProgress\" < \"Target\" AND \"Rate\" = 50)))");
        });
    }
}

public sealed class CoachPayoutConfiguration : IEntityTypeConfiguration<CoachPayout>
{
    public void Configure(EntityTypeBuilder<CoachPayout> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CoachEmployee).WithMany().HasForeignKey(x => x.CoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Expense).WithMany().HasForeignKey(x => x.ExpenseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AuthorizedByUser).WithMany().HasForeignKey(x => x.AuthorizedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.ExpenseId).IsUnique(); b.HasIndex(x => x.OperationId).IsUnique();
        b.HasIndex(x => new { x.BranchId, x.CoachEmployeeId, x.PaidAt });
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        foreach (var name in new[] { "Amount", "BalanceBefore", "BalanceAfter" }) b.Property<decimal>(name).HasPrecision(18, 2);
        b.ToTable("CoachPayouts", t => t.HasCheckConstraint("CK_CoachPayouts_Balance", "\"Amount\" > 0 AND \"BalanceBefore\" >= \"Amount\" AND \"BalanceAfter\" = \"BalanceBefore\" - \"Amount\""));
    }
}

public sealed class CoachPayoutAllocationConfiguration : IEntityTypeConfiguration<CoachPayoutAllocation>
{
    public void Configure(EntityTypeBuilder<CoachPayoutAllocation> b)
    {
        b.HasKey(x => new { x.CoachPayoutId, x.CoachEarningId });
        b.HasOne(x => x.CoachPayout).WithMany().HasForeignKey(x => x.CoachPayoutId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CoachEarning).WithMany().HasForeignKey(x => x.CoachEarningId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.ToTable("CoachPayoutAllocations", t => t.HasCheckConstraint("CK_CoachPayoutAllocations_Amount", "\"Amount\" > 0"));
    }
}
