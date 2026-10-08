using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class Lead
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public string? FullName { get; set; }
    public string Phone { get; set; } = null!;
    public string? PhoneNormalized { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? NextFollowUpAt { get; set; }
    public DateTimeOffset? ConvertedAt { get; set; }
    public DateTimeOffset? LostAt { get; set; }
    public string? LostReason { get; set; }
    public string? Email { get; set; }
    public string? Source { get; set; }
    public LeadStatus Status { get; set; }
    public Guid? ConvertedMemberId { get; set; }
    public Member? ConvertedMember { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
