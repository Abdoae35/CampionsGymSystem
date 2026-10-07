using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class ShiftEmployeeChange
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShiftId { get; set; }
    public Shift Shift { get; set; } = null!;
    public Guid OriginalEmployeeId { get; set; }
    public Employee OriginalEmployee { get; set; } = null!;
    public Guid ReplacementEmployeeId { get; set; }
    public Employee ReplacementEmployee { get; set; } = null!;
    public string? Reason { get; set; }
    public Guid ChangedByUserId { get; set; }
    public ApplicationUser ChangedByUser { get; set; } = null!;
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
}
