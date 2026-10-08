using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class LeadInteractionConfiguration : IEntityTypeConfiguration<LeadInteraction>
{
    public void Configure(EntityTypeBuilder<LeadInteraction> builder)
    {
        builder.ToTable("LeadInteractions", table =>
        {
            table.HasCheckConstraint("CK_LeadInteractions_InteractionType", "\"InteractionType\" IN ('Call', 'WhatsApp', 'Visit', 'Other')");
        });
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Lead).WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.InteractionType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Notes).HasColumnType("text").IsRequired();
        builder.Property(x => x.Outcome).HasMaxLength(256);
        builder.Property(x => x.NextFollowUpAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.LeadId, x.NextFollowUpAt });
    }
}
