using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class BusinessDay
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public DateOnly BusinessDate { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public BusinessDayStatus Status { get; set; }
    public Guid? OpenedByUserId { get; set; }
    public ApplicationUser? OpenedByUser { get; set; }
    public Guid? ClosedByUserId { get; set; }
    public ApplicationUser? ClosedByUser { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
