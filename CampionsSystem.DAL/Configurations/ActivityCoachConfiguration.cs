using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ActivityCoachConfiguration : IEntityTypeConfiguration<ActivityCoach>
{
    public void Configure(EntityTypeBuilder<ActivityCoach> b)
    {
        b.HasKey(x => x.Id);
        b.ToTable("ActivityCoaches", t =>
        {
            t.HasCheckConstraint("CK_ActivityCoaches_Type", "(\"CoachType\" = 'Internal' AND \"EmployeeId\" IS NOT NULL AND \"ExternalName\" IS NULL AND \"ExternalPhone\" IS NULL) OR (\"CoachType\" = 'External' AND \"EmployeeId\" IS NULL AND length(trim(\"ExternalName\")) > 0 AND \"ExternalName\" IS NOT NULL)");
            t.HasCheckConstraint("CK_ActivityCoaches_Percentage", "\"Percentage\" BETWEEN 0 AND 100");
        });
        b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.CoachType).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.ExternalName).HasMaxLength(256);
        b.Property(x => x.ExternalPhone).HasMaxLength(64);
        b.Property(x => x.Percentage).HasPrecision(5, 2);
        b.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        b.HasIndex(x => x.EmployeeId).IsUnique().HasFilter("\"EmployeeId\" IS NOT NULL");
    }
}
