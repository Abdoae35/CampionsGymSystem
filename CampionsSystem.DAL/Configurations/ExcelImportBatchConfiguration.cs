using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class ExcelImportBatchConfiguration : IEntityTypeConfiguration<ExcelImportBatch>
{
    public void Configure(EntityTypeBuilder<ExcelImportBatch> b)
    {
        b.ToTable("ExcelImportBatches");
        b.HasKey(x => x.Id);
        b.Property(x => x.Kind).HasMaxLength(32).IsRequired();
        b.HasIndex(x => new { x.CreatedByUserId, x.ExpiresAt });
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ExcelImportRowConfiguration : IEntityTypeConfiguration<ExcelImportRow>
{
    public void Configure(EntityTypeBuilder<ExcelImportRow> b)
    {
        b.ToTable("ExcelImportRows");
        b.HasKey(x => x.Id);
        b.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.Status).HasMaxLength(32).IsRequired();
        b.Property(x => x.Message).HasMaxLength(2048);
        b.HasIndex(x => new { x.BatchId, x.RowNumber }).IsUnique();
        b.HasOne(x => x.Batch).WithMany(x => x.Rows).HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
    }
}
