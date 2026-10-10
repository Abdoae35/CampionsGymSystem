using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MembershipSubscriptionTransactionConfiguration : IEntityTypeConfiguration<MembershipSubscriptionTransaction>
{
    public void Configure(EntityTypeBuilder<MembershipSubscriptionTransaction> builder)
    {
        builder.ToTable("MembershipSubscriptionTransactions", table =>
        {
            table.HasCheckConstraint("CK_MembershipSubscriptionTransactions_TransactionType", "\"TransactionType\" IN ('Initial', 'Renewal')");
            table.HasCheckConstraint("CK_MembershipSubscriptionTransactions_PurchasedDurationDays", "(\"PlanCategorySnapshot\" = 'Employee' AND \"PurchasedDurationDays\" = 0 AND \"NewEndDate\" IS NULL AND \"SaleTransactionId\" IS NULL AND \"PlanPriceSnapshot\" = 0 AND \"NetPriceSnapshot\" = 0 AND \"PurchasedSessions\" IS NULL) OR (\"PlanCategorySnapshot\" <> 'Employee' AND \"PurchasedDurationDays\" > 0 AND \"NewEndDate\" IS NOT NULL)");
            table.HasCheckConstraint("CK_MembershipSubscriptionTransactions_PurchasedSessions", "\"PurchasedSessions\" > 0");
            table.HasCheckConstraint("CK_MembershipSubscriptionTransactions_PlanPriceSnapshot", "\"PlanPriceSnapshot\" >= 0");
            table.HasCheckConstraint("CK_MembershipSubscriptionTransactions_NetPriceSnapshot", "\"NetPriceSnapshot\" >= 0");
        });
        builder.ToTable("MembershipSubscriptionTransactions", table => table.HasCheckConstraint(
            "CK_MembershipSubscriptionTransactions_PlanCategoryCoachSnapshot",
            "(\"PlanCategorySnapshot\" IN ('Normal', 'Employee') AND \"FreelanceCoachEmployeeIdSnapshot\" IS NULL) OR (\"PlanCategorySnapshot\" = 'Freelance' AND \"FreelanceCoachEmployeeIdSnapshot\" IS NOT NULL)"));
        builder.Property(x => x.PlanCategorySnapshot).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.FreelanceCoachEmployeeSnapshot).WithMany().HasForeignKey(x => x.FreelanceCoachEmployeeIdSnapshot).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.PlanNameSnapshot).HasMaxLength(256);
        builder.Property(x => x.AccessModeSnapshot).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.StartDateSnapshot).HasColumnType("date");
        builder.HasOne(x => x.PlanBranchSnapshot).WithMany().HasForeignKey(x => x.PlanBranchIdSnapshot).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CitySnapshot).WithMany().HasForeignKey(x => x.CityIdSnapshot).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.BusinessDay).WithMany().HasForeignKey(x => x.BusinessDayId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RenewedFromSubscription).WithMany().HasForeignKey(x => x.RenewedFromSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.HasIndex(x => new { x.MemberSubscriptionId, x.CreatedAt });
        builder.ToTable("MembershipSubscriptionTransactions", t => t.HasCheckConstraint("CK_MembershipSubscriptionTransactions_FreezeSnapshot", "\"FreezeDaysSnapshot\" >= 0"));
        builder.HasIndex(x => x.SaleTransactionId).IsUnique().HasFilter("\"OperationId\" IS NOT NULL");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.MemberSubscription).WithMany().HasForeignKey(x => x.MemberSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SaleTransaction).WithMany().HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.MembershipPlan).WithMany().HasForeignKey(x => x.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.PlanPriceSnapshot).HasPrecision(18, 2);
        builder.Property(x => x.NetPriceSnapshot).HasPrecision(18, 2);
        builder.Property(x => x.PreviousEndDate).HasColumnType("date");
        builder.Property(x => x.NewEndDate).HasColumnType("date");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
