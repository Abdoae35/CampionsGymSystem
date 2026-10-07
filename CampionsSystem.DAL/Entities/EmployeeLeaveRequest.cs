using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class EmployeeLeaveRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string? Reason { get; set; }
    public EmployeeLeaveRequestStatus Status { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public ApplicationUser? ReviewedByUser { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ManagerNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
