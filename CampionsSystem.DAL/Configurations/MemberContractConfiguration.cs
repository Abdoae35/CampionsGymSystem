using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MemberContractConfiguration : IEntityTypeConfiguration<MemberContract>
{
    public void Configure(EntityTypeBuilder<MemberContract> builder)
    {
        builder.ToTable("MemberContracts", table =>
        {
            table.HasCheckConstraint("CK_MemberContracts_TemplateVersion", "\"TemplateVersion\" > 0");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SaleTransaction).WithMany().HasForeignKey(x => x.SaleTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ContractTemplate).WithMany().HasForeignKey(x => x.ContractTemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.TemplateSnapshotText).HasColumnType("text").IsRequired();
        builder.Property(x => x.GeneratedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.GeneratedByUser).WithMany().HasForeignKey(x => x.GeneratedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.PhysicalArchiveReference).HasMaxLength(256);
    }
}
