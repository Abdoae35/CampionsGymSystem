using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ActivityScheduleConfiguration : IEntityTypeConfiguration<ActivitySchedule>
{
    public void Configure(EntityTypeBuilder<ActivitySchedule> builder)
    {
        builder.ToTable("ActivitySchedules", table =>
        {
            table.HasCheckConstraint("CK_ActivitySchedules_DayOfWeek", "\"DayOfWeek\" BETWEEN 0 AND 6");
            table.HasCheckConstraint("CK_ActivitySchedules_TimeRange", "\"EndTime\" > \"StartTime\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.ActivityCoachAssignment).WithMany().HasForeignKey(x => x.ActivityCoachAssignmentId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.StartTime).HasColumnType("time without time zone");
        builder.Property(x => x.EndTime).HasColumnType("time without time zone");
    }
}
