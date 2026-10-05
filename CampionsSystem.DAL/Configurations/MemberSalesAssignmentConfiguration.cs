using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MemberSalesAssignmentConfiguration : IEntityTypeConfiguration<MemberSalesAssignment>
{
    public void Configure(EntityTypeBuilder<MemberSalesAssignment> builder)
    {
        builder.ToTable("MemberSalesAssignments", table =>
        {
            table.HasCheckConstraint("CK_MemberSalesAssignments_AssignmentType", "\"AssignmentType\" IN ('Sales', 'House')");
            table.HasCheckConstraint("CK_MemberSalesAssignments_AssignedAtEndedAt", "\"EndedAt\" >= \"AssignedAt\"");
            table.HasCheckConstraint("CK_MemberSalesAssignments_AssignmentEmployee", "(\"AssignmentType\" = 'House' AND \"SalesEmployeeId\" IS NULL) OR (\"AssignmentType\" = 'Sales' AND \"SalesEmployeeId\" IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Member).WithMany(x => x.SalesAssignments).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SalesEmployee).WithMany().HasForeignKey(x => x.SalesEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.AssignmentType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.AssignedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.EndedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.Reason).HasColumnType("text");
        builder.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.MemberId, x.EndedAt });
        builder.HasIndex(x => x.MemberId).IsUnique().HasFilter("\"EndedAt\" IS NULL");
    }
}
