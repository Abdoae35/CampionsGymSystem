using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class Shift
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal? ExpectedClosingCash { get; set; }
    public decimal? ActualClosingCash { get; set; }
    public decimal? Difference { get; set; }
    public ShiftStatus Status { get; set; }
    public Guid OpenedByUserId { get; set; }
    public ApplicationUser OpenedByUser { get; set; } = null!;
    public Guid? ClosedByUserId { get; set; }
    public ApplicationUser? ClosedByUser { get; set; }
}
