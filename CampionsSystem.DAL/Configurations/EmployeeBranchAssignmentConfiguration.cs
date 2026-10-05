using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class EmployeeBranchAssignmentConfiguration : IEntityTypeConfiguration<EmployeeBranchAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeBranchAssignment> builder)
    {
        builder.ToTable("EmployeeBranchAssignments", table =>
        {
            table.HasCheckConstraint("CK_EmployeeBranchAssignments_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.EmployeeId, x.EndDate });
        builder.HasIndex(x => x.EmployeeId).IsUnique().HasFilter("\"EndDate\" IS NULL AND \"IsPrimary\"");
    }
}
