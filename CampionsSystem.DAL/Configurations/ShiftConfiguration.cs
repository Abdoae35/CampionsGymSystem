using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts", table =>
        {
            table.HasCheckConstraint("CK_Shifts_OpeningCash", "\"OpeningCash\" >= 0");
            table.HasCheckConstraint("CK_Shifts_ExpectedClosingCash", "\"ExpectedClosingCash\" >= 0");
            table.HasCheckConstraint("CK_Shifts_ActualClosingCash", "\"ActualClosingCash\" >= 0");
            table.HasCheckConstraint("CK_Shifts_Status", "\"Status\" IN ('Open', 'Closed')");
            table.HasCheckConstraint("CK_Shifts_OpenedAtClosedAt", "\"ClosedAt\" >= \"OpenedAt\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.OpenedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.OpeningCash).HasPrecision(18, 2);
        builder.Property(x => x.ExpectedClosingCash).HasPrecision(18, 2);
        builder.Property(x => x.ActualClosingCash).HasPrecision(18, 2);
        builder.Property(x => x.Difference).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.OpenedByUser).WithMany().HasForeignKey(x => x.OpenedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ClosedByUser).WithMany().HasForeignKey(x => x.ClosedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
