using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class Member
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MemberCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string PrimaryPhone { get; set; } = null!;
    public string? SecondaryPhone { get; set; }
    public bool IsPrimaryPhoneVerified { get; set; }
    public DateTimeOffset? PrimaryPhoneVerifiedAt { get; set; }
    public Guid HomeBranchId { get; set; }
    public GymBranch HomeBranch { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<MemberSalesAssignment> SalesAssignments { get; set; } = new List<MemberSalesAssignment>();
    public ICollection<MemberSubscription> Subscriptions { get; set; } = new List<MemberSubscription>();
}
