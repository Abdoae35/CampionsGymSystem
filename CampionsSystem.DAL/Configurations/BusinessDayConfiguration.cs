using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class BusinessDayConfiguration : IEntityTypeConfiguration<BusinessDay>
{
    public void Configure(EntityTypeBuilder<BusinessDay> b)
    {
        b.HasKey(x => x.Id);
        b.ToTable("BusinessDays", t =>
        {
            t.HasCheckConstraint("CK_BusinessDays_Status", "\"Status\" IN ('Open', 'Closed')");
            t.HasCheckConstraint("CK_BusinessDays_Lifecycle", "(\"Status\" = 'Open' AND \"ClosedAt\" IS NULL AND \"ClosedByUserId\" IS NULL) OR (\"Status\" = 'Closed' AND \"ClosedAt\" IS NOT NULL AND \"ClosedAt\" >= \"OpenedAt\")");
        });
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.OpenedByUser).WithMany().HasForeignKey(x => x.OpenedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ClosedByUser).WithMany().HasForeignKey(x => x.ClosedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.BusinessDate).HasColumnType("date");
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.OpenedAt).HasColumnType("timestamp with time zone");
        b.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
        b.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        b.HasIndex(x => new { x.BranchId, x.BusinessDate }).IsUnique();
        b.HasIndex(x => x.BranchId).IsUnique().HasFilter("\"Status\" = 'Open'");
    }
}
