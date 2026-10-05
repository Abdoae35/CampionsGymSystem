using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ClassScheduleConfiguration : IEntityTypeConfiguration<ClassSchedule>
{
    public void Configure(EntityTypeBuilder<ClassSchedule> builder)
    {
        builder.ToTable("ClassSchedules", table =>
        {
            table.HasCheckConstraint("CK_ClassSchedules_DayOfWeek", "\"DayOfWeek\" BETWEEN 0 AND 6");
            table.HasCheckConstraint("CK_ClassSchedules_TimeRange", "\"EndTime\" > \"StartTime\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.GroupClass).WithMany().HasForeignKey(x => x.GroupClassId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.StartTime).HasColumnType("time without time zone");
        builder.Property(x => x.EndTime).HasColumnType("time without time zone");
    }
}
