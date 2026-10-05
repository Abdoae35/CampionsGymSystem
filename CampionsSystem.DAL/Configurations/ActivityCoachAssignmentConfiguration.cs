using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ActivityCoachAssignmentConfiguration : IEntityTypeConfiguration<ActivityCoachAssignment>
{
    public void Configure(EntityTypeBuilder<ActivityCoachAssignment> builder)
    {
        builder.ToTable("ActivityCoachAssignments", table =>
        {
            table.HasCheckConstraint("CK_ActivityCoachAssignments_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.SportsActivity).WithMany().HasForeignKey(x => x.SportsActivityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CoachEmployee).WithMany().HasForeignKey(x => x.CoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
