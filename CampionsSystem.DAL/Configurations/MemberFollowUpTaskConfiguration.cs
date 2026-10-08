using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampionsSystem.DAL.Configurations;
public sealed class MemberFollowUpTaskConfiguration : IEntityTypeConfiguration<MemberFollowUpTask>
{
    public void Configure(EntityTypeBuilder<MemberFollowUpTask> b)
    {
        b.ToTable("MemberFollowUpTasks", t => {
            t.HasCheckConstraint("CK_MemberFollowUpTasks_Reason", "\"Reason\" IN ('RenewalDue','ExpiredMembership','InactiveMember','Reactivation','ManagerAssigned','Other')");
            t.HasCheckConstraint("CK_MemberFollowUpTasks_CompletedAt", "\"CompletedAt\" IS NULL OR \"CompletedAt\" >= \"CreatedAt\"");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Reason).HasMaxLength(40).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(4096);
        b.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.MemberId, x.Reason }).IsUnique().HasFilter("\"CompletedAt\" IS NULL");
        b.HasIndex(x => new { x.CompletedAt, x.DueAt });
    }
}
