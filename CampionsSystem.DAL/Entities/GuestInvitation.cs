using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class GuestInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? InviteeMemberId { get; set; }
    public Member? InviteeMember { get; set; }
    public string InviteeName { get; set; } = null!;
    public string InviteePhone { get; set; } = null!;
    public string InviteePhoneNormalized { get; set; } = null!;
    public GuestInvitationSource Source { get; set; }
    public Guid? InvitedByMemberId { get; set; }
    public Member? InvitedByMember { get; set; }
    public Guid? SourceSubscriptionBenefitId { get; set; }
    public MemberSubscriptionBenefit? SourceSubscriptionBenefit { get; set; }
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public GuestInvitationStatus Status { get; set; } = GuestInvitationStatus.Available;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RedeemedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? RedeemedCheckInId { get; set; }
    public CheckIn? RedeemedCheckIn { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public string? Notes { get; set; }
}
