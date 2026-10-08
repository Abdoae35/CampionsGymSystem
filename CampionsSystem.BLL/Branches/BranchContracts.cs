using System.ComponentModel.DataAnnotations;

namespace CampionsSystem.BLL.Branches;

public sealed record SaveBranchRequest(
    [Required, MaxLength(256)] string Name,
    Guid CityId,
    [MaxLength(256)] string? Address = null,
    [MaxLength(64)] string? Phone = null,
    bool IsActive = true,
    [Range(0, int.MaxValue)] int MaxBorrowedGymSessions = 2,
    [Range(0, int.MaxValue)] int QuickRefundWindowHours = 24,
    [RegularExpression(@"^[+][1-9][0-9]{0,2}$")] string? CountryCallingCode = null);

public sealed record BranchDetails(Guid Id, string Name, Guid CityId, string CityName,
    string? Address, string? Phone, bool IsActive, int MaxBorrowedGymSessions, int QuickRefundWindowHours, string? CountryCallingCode = null);

public sealed class BranchNotFoundException(string message) : Exception(message);
