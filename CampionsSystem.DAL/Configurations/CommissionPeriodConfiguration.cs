using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class CommissionPeriodConfiguration : IEntityTypeConfiguration<CommissionPeriod>
{
    public void Configure(EntityTypeBuilder<CommissionPeriod> builder)
    {
        builder.ToTable("CommissionPeriods", table =>
        {
            table.HasCheckConstraint("CK_CommissionPeriods_Status", "\"Status\" IN ('Open', 'Closed')");
            table.HasCheckConstraint("CK_CommissionPeriods_StartAtEndAt", "\"EndAt\" >= \"StartAt\"");
            table.HasCheckConstraint("CK_CommissionPeriods_BusinessDates", "(\"StartBusinessDate\" IS NULL AND \"EndBusinessDate\" IS NULL) OR (\"StartBusinessDate\" IS NOT NULL AND \"EndBusinessDate\" IS NOT NULL AND \"EndBusinessDate\" >= \"StartBusinessDate\")");
        });
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.HasIndex(x => x.CloseOperationId).IsUnique();
        builder.Property(x => x.RequestHash).HasMaxLength(64);
        builder.Property(x => x.CloseRequestHash).HasMaxLength(64);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => new { x.BranchId, x.StartBusinessDate });
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.StartAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.EndAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.ClosedByUser).WithMany().HasForeignKey(x => x.ClosedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.ReopenedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.ReopenedByUser).WithMany().HasForeignKey(x => x.ReopenedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => x.BranchId).IsUnique().HasFilter("\"Status\" = 'Open'");
    }
}
