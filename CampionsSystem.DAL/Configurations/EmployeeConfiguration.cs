using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", table =>
        {
            table.HasCheckConstraint("CK_Employees_EmploymentType", "\"EmploymentType\" IN ('Staff', 'Freelance')");
            table.HasCheckConstraint("CK_Employees_EmploymentStatus", "\"EmploymentStatus\" IN ('Active', 'Resigned', 'Suspended')");
            table.HasCheckConstraint("CK_Employees_HireDateTerminationDate", "\"TerminationDate\" >= \"HireDate\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.FullName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EmploymentType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.JobTitle).HasMaxLength(256);
        builder.Property(x => x.HireDate).HasColumnType("date");
        builder.Property(x => x.TerminationDate).HasColumnType("date");
        builder.Property(x => x.EmploymentStatus).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
