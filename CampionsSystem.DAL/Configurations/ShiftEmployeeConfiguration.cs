using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ShiftEmployeeConfiguration : IEntityTypeConfiguration<ShiftEmployee>
{
    public void Configure(EntityTypeBuilder<ShiftEmployee> builder)
    {
        builder.ToTable("ShiftEmployees", table =>
        {
            table.HasCheckConstraint("CK_ShiftEmployees_JoinedAtLeftAt", "\"LeftAt\" >= \"JoinedAt\"");
        });
        builder.HasKey(x => new { x.ShiftId, x.EmployeeId });
        builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.JoinedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.LeftAt).HasColumnType("timestamp with time zone");
    }
}
