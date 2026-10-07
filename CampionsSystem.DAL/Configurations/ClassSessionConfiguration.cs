using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> b)
    {
        b.HasKey(x => x.Id);
        b.ToTable("ClassSessions", t =>
        {
            t.HasCheckConstraint("CK_ClassSessions_Status", "\"Status\" IN ('Scheduled', 'Completed', 'Cancelled')");
            t.HasCheckConstraint("CK_ClassSessions_Times", "(\"EndedAt\" IS NULL OR \"EndedAt\" >= \"StartedAt\") AND (\"Status\" <> 'Completed' OR \"EndedAt\" IS NOT NULL)");
        });
        b.HasOne(x => x.GroupClass).WithMany().HasForeignKey(x => x.GroupClassId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ClassSchedule).WithMany().HasForeignKey(x => x.ClassScheduleId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CoachEmployee).WithMany().HasForeignKey(x => x.CoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        b.HasIndex(x => new { x.CoachEmployeeId, x.StartedAt });
        b.HasIndex(x => new { x.GroupClassId, x.StartedAt });
        b.HasIndex(x => new { x.BranchId, x.StartedAt });
        b.HasOne(x => x.BusinessDay).WithMany().HasForeignKey(x => x.BusinessDayId).OnDelete(DeleteBehavior.Restrict);
    }
}
