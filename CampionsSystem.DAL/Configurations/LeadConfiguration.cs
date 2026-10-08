using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads", table =>
        {
            table.HasCheckConstraint("CK_Leads_Status", "\"Status\" IN ('New', 'Contacted', 'Interested', 'FollowUp', 'Converted', 'Lost', 'Inactive')");
        });
        builder.Property(x => x.PhoneNormalized).HasMaxLength(16);
        builder.Property(x => x.SecondaryPhone).HasMaxLength(64);
        builder.Property(x => x.Notes).HasMaxLength(4096);
        builder.Property(x => x.LostReason).HasMaxLength(2048);
        builder.HasIndex(x => x.PhoneNormalized).IsUnique().HasFilter("\"PhoneNormalized\" IS NOT NULL");
        builder.HasIndex(x => new { x.BranchId, x.Status, x.NextFollowUpAt });
        builder.HasIndex(x => x.ConvertedMemberId).IsUnique().HasFilter("\"ConvertedMemberId\" IS NOT NULL");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.FullName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320);
        builder.Property(x => x.Source).HasMaxLength(256);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.ConvertedMember).WithMany().HasForeignKey(x => x.ConvertedMemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
