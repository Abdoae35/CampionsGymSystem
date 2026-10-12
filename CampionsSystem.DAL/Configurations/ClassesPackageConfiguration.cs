using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampionsSystem.DAL.Configurations;

public sealed class ClassesPackageConfiguration : IEntityTypeConfiguration<ClassesPackage>
{
    public void Configure(EntityTypeBuilder<ClassesPackage> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Name).HasMaxLength(256).IsRequired(); b.Property(x => x.Price).HasPrecision(18, 2);
        b.HasIndex(x => new { x.BranchId, x.Name }).IsUnique();
        b.ToTable("ClassesPackages", t => t.HasCheckConstraint("CK_ClassesPackages_Terms", "length(btrim(\"Name\")) > 0 AND \"SessionCount\" > 0 AND \"DurationDays\" > 0 AND \"Price\" > 0"));
    }
}

public sealed class ClassesSubscriptionConfiguration : IEntityTypeConfiguration<ClassesSubscription>
{
    public void Configure(EntityTypeBuilder<ClassesSubscription> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ClassesPackage).WithMany().HasForeignKey(x => x.ClassesPackageId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SaleTransaction).WithMany().HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.SaleTransactionId).IsUnique();
        b.HasIndex(x => new { x.BranchId, x.MemberId, x.EndDate });
        b.Property(x => x.PackageName).HasMaxLength(256).IsRequired(); b.Property(x => x.Version).IsConcurrencyToken();
        foreach (var name in new[] { "Price", "UnitSessionValue", "RemainderValue" }) b.Property<decimal>(name).HasPrecision(18, 2);
        b.ToTable("ClassesSubscriptions", t =>
        {
            t.HasCheckConstraint("CK_ClassesSubscriptions_Balance", "\"PurchasedSessions\" > 0 AND \"RemainingSessions\" >= 0 AND \"RemainingSessions\" <= \"PurchasedSessions\" AND \"EndDate\" >= \"StartDate\"");
            t.HasCheckConstraint("CK_ClassesSubscriptions_Value", "\"Price\" > 0 AND \"UnitSessionValue\" >= 0 AND \"RemainderValue\" >= 0 AND \"RemainderValue\" < \"PurchasedSessions\" * 0.01 AND \"Price\" = \"PurchasedSessions\" * \"UnitSessionValue\" + \"RemainderValue\"");
        });
    }
}
