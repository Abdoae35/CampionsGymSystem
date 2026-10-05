using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class ActivityPackage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SportsActivityId { get; set; }
    public SportsActivity SportsActivity { get; set; } = null!;
    public string Name { get; set; } = null!;
    public AccessMode AccessMode { get; set; }
    public int? SessionCount { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public bool AllowFreeze { get; set; }
    public int IncludedFreezeDays { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
