using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class EmployeeSalarySettingConfiguration : IEntityTypeConfiguration<EmployeeSalarySetting>
{
    public void Configure(EntityTypeBuilder<EmployeeSalarySetting> builder)
    {
        builder.ToTable("EmployeeSalarySettings", table =>
        {
            table.HasCheckConstraint("CK_EmployeeSalarySettings_BasicSalary", "\"BasicSalary\" >= 0");
            table.HasCheckConstraint("CK_EmployeeSalarySettings_EffectiveFromEffectiveTo", "\"EffectiveTo\" >= \"EffectiveFrom\"");
        });
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.OperationId).IsUnique().HasFilter("\"OperationId\" IS NOT NULL");
        builder.Property(x => x.RequestHash).HasMaxLength(64);
        builder.HasIndex(x => new { x.EmployeeId, x.EffectiveFrom }).IsUnique();
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.BasicSalary).HasPrecision(18, 2);
        builder.Property(x => x.EffectiveFrom).HasColumnType("date");
        builder.Property(x => x.EffectiveTo).HasColumnType("date");
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
