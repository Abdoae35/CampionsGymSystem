using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class EmployeeLeaveRequestConfiguration : IEntityTypeConfiguration<EmployeeLeaveRequest>
{
    public void Configure(EntityTypeBuilder<EmployeeLeaveRequest> b)
    {
        b.HasKey(x => x.Id);
        b.ToTable("EmployeeLeaveRequests", t =>
        {
            t.HasCheckConstraint("CK_EmployeeLeaveRequests_Dates", "\"ToDate\" >= \"FromDate\"");
            t.HasCheckConstraint("CK_EmployeeLeaveRequests_Status", "\"Status\" IN ('Pending', 'Approved', 'Rejected')");
            t.HasCheckConstraint("CK_EmployeeLeaveRequests_Review", "(\"Status\" = 'Pending' AND \"ReviewedAt\" IS NULL AND \"ReviewedByUserId\" IS NULL) OR (\"Status\" IN ('Approved', 'Rejected') AND \"ReviewedAt\" IS NOT NULL AND \"ReviewedAt\" >= \"CreatedAt\")");
        });
        b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ReviewedByUser).WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.FromDate).HasColumnType("date");
        b.Property(x => x.ToDate).HasColumnType("date");
        b.Property(x => x.Reason).HasColumnType("text");
        b.Property(x => x.ManagerNotes).HasColumnType("text");
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.ReviewedAt).HasColumnType("timestamp with time zone");
        b.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        b.HasIndex(x => new { x.EmployeeId, x.FromDate, x.ToDate });
        b.HasIndex(x => x.Status);
    }
}
