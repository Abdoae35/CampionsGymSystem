using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ShiftEmployeeChangeConfiguration : IEntityTypeConfiguration<ShiftEmployeeChange>
{
    public void Configure(EntityTypeBuilder<ShiftEmployeeChange> b)
    {
        b.HasKey(x => x.Id);
        b.ToTable("ShiftEmployeeChanges", t =>
        {
            t.HasCheckConstraint("CK_ShiftEmployeeChanges_DifferentEmployees", "\"OriginalEmployeeId\" <> \"ReplacementEmployeeId\"");
        });
        b.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.OriginalEmployee).WithMany().HasForeignKey(x => x.OriginalEmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ReplacementEmployee).WithMany().HasForeignKey(x => x.ReplacementEmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Reason).HasColumnType("text");
        b.Property(x => x.ChangedAt).HasColumnType("timestamp with time zone");
        b.HasIndex(x => new { x.ShiftId, x.ChangedAt });
    }
}
