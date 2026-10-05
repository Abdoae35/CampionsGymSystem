using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class LeadAssignmentConfiguration : IEntityTypeConfiguration<LeadAssignment>
{
    public void Configure(EntityTypeBuilder<LeadAssignment> builder)
    {
        builder.ToTable("LeadAssignments", table =>
        {
            table.HasCheckConstraint("CK_LeadAssignments_AssignedAtEndedAt", "\"EndedAt\" >= \"AssignedAt\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Lead).WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SalesEmployee).WithMany().HasForeignKey(x => x.SalesEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.AssignedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.EndedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Reason).HasColumnType("text");
    }
}
