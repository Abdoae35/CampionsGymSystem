using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class LeadAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LeadId { get; set; }
    public Lead Lead { get; set; } = null!;
    public Guid SalesEmployeeId { get; set; }
    public Employee SalesEmployee { get; set; } = null!;
    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public Guid AssignedByUserId { get; set; }
    public ApplicationUser AssignedByUser { get; set; } = null!;
    public string? Reason { get; set; }
}
