using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class AuthorizedDevice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public string DeviceName { get; set; } = null!;
    public string DeviceKeyHash { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset RegisteredAt { get; set; }
    public Guid RegisteredByUserId { get; set; }
    public ApplicationUser RegisteredByUser { get; set; } = null!;
    public DateTimeOffset? RevokedAt { get; set; }
}
