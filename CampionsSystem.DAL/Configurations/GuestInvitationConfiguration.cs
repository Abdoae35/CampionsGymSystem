using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class GuestInvitationConfiguration : IEntityTypeConfiguration<GuestInvitation>
{
    public void Configure(EntityTypeBuilder<GuestInvitation> builder)
    {
        builder.ToTable("GuestInvitations", table =>
        {
            table.HasCheckConstraint("CK_GuestInvitations_Source", "\"Source\" IN ('Gym', 'Member')");
            table.HasCheckConstraint("CK_GuestInvitations_Status", "\"Status\" IN ('Available', 'Redeemed', 'Cancelled', 'Expired')");
            table.HasCheckConstraint("CK_GuestInvitations_Inviter", "(\"Source\" = 'Gym' AND \"InvitedByMemberId\" IS NULL AND \"SourceSubscriptionBenefitId\" IS NULL) OR (\"Source\" = 'Member' AND \"InvitedByMemberId\" IS NOT NULL)");
            table.HasCheckConstraint("CK_GuestInvitations_Identity", "btrim(\"InviteeName\") <> '' AND btrim(\"InviteePhone\") <> '' AND btrim(\"InviteePhoneNormalized\") <> '' AND \"InviteePhoneNormalized\" = btrim(\"InviteePhoneNormalized\")");
            table.HasCheckConstraint("CK_GuestInvitations_Redemption", "(\"Status\" = 'Redeemed' AND \"RedeemedAt\" IS NOT NULL) OR (\"Status\" <> 'Redeemed' AND \"RedeemedAt\" IS NULL AND \"RedeemedCheckInId\" IS NULL)");
            table.HasCheckConstraint("CK_GuestInvitations_Cancellation", "(\"Status\" = 'Cancelled' AND \"CancelledAt\" IS NOT NULL) OR (\"Status\" <> 'Cancelled' AND \"CancelledAt\" IS NULL)");
            table.HasCheckConstraint("CK_GuestInvitations_Timestamps", "(\"RedeemedAt\" IS NULL OR \"RedeemedAt\" >= \"CreatedAt\") AND (\"CancelledAt\" IS NULL OR \"CancelledAt\" >= \"CreatedAt\")");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InviteeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.InviteePhone).HasMaxLength(32).IsRequired();
        builder.Property(x => x.InviteePhoneNormalized).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasOne(x => x.InviteeMember).WithMany().HasForeignKey(x => x.InviteeMemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.InvitedByMember).WithMany().HasForeignKey(x => x.InvitedByMemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SourceSubscriptionBenefit).WithMany().HasForeignKey(x => x.SourceSubscriptionBenefitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RedeemedCheckIn).WithMany().HasForeignKey(x => x.RedeemedCheckInId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.InviteePhoneNormalized).IsUnique().HasFilter("\"Status\" = 'Redeemed'");
        builder.HasIndex(x => x.InviteeMemberId).IsUnique().HasFilter("\"Status\" = 'Redeemed' AND \"InviteeMemberId\" IS NOT NULL");
        builder.HasIndex(x => x.RedeemedCheckInId).IsUnique().HasFilter("\"RedeemedCheckInId\" IS NOT NULL");
        builder.HasIndex(x => new { x.BranchId, x.Status, x.CreatedAt });
    }
}
