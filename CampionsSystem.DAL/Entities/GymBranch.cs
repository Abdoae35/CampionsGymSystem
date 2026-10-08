using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class GymBranch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CityId { get; set; }
    public City City { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? CountryCallingCode { get; set; }
    public long LastMemberClientCode { get; set; }
    public string? Phone { get; set; }
    public int MaxBorrowedGymSessions { get; set; }
    public int QuickRefundWindowHours { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
