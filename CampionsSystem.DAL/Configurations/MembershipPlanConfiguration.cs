using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
{
    public void Configure(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.ToTable("MembershipPlans", table =>
        {
            table.HasCheckConstraint("CK_MembershipPlans_AccessMode", "\"AccessMode\" IN ('Unlimited', 'SessionBased')");
            table.HasCheckConstraint("CK_MembershipPlans_DurationDays", "\"DurationDays\" > 0");
            table.HasCheckConstraint("CK_MembershipPlans_SessionCount", "\"SessionCount\" > 0");
            table.HasCheckConstraint("CK_MembershipPlans_Price", "\"Price\" >= 0");
            table.HasCheckConstraint("CK_MembershipPlans_IncludedFreezeDays", "\"IncludedFreezeDays\" >= 0");
            table.HasCheckConstraint("CK_MembershipPlans_AccessSessions", "(\"AccessMode\" = 'Unlimited' AND \"SessionCount\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"SessionCount\" IS NOT NULL AND \"SessionCount\" > 0)");
        });
        builder.ToTable("MembershipPlans", table => table.HasCheckConstraint(
            "CK_MembershipPlans_PlanCategoryCoach",
            "(\"PlanCategory\" = 'Normal' AND \"FreelanceCoachEmployeeId\" IS NULL) OR (\"PlanCategory\" = 'Freelance' AND \"FreelanceCoachEmployeeId\" IS NOT NULL)"));
        builder.Property(x => x.PlanCategory).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.FreelanceCoachEmployee).WithMany().HasForeignKey(x => x.FreelanceCoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.PlanCategory, x.FreelanceCoachEmployeeId, x.IsActive });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.AccessMode).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.IncludedFreezeDays).HasDefaultValue(0);
        builder.HasOne(x => x.MembershipPlanCategory).WithMany().HasForeignKey(x => x.MembershipPlanCategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
