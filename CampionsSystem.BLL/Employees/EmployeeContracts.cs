using System.ComponentModel.DataAnnotations;
using CampionsSystem.BLL.Authentication;

namespace CampionsSystem.BLL.Employees;

public sealed class EmployeeListRequest
{
    [MaxLength(256)] public string? Search { get; set; }
    public Guid? BranchId { get; set; }
    public DAL.Enums.EmploymentStatus? Status { get; set; }
    public DAL.Enums.EmploymentType? EmploymentType { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed record EmployeeDetails(Guid Id, string FullName, string Phone, string? JobTitle,
    string EmploymentType, string Status, DateOnly HireDate, DateOnly? TerminationDate,
    BranchContext? CurrentBranch, bool HasSystemAccount);
public sealed record EmployeeListResult(IReadOnlyList<EmployeeDetails> Items, int TotalCount, int Page, int PageSize);

// Reject unrelated fields rather than silently accepting account/branch/status changes.
[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public class EmployeeProfileRequest
{
    [Required, MaxLength(256)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string Phone { get; set; } = string.Empty;
    [MaxLength(256)] public string? JobTitle { get; set; }
    [EnumDataType(typeof(DAL.Enums.EmploymentType))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<DAL.Enums.EmploymentType>))]
    public DAL.Enums.EmploymentType EmploymentType { get; set; }
    public DateOnly HireDate { get; set; }
}

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateEmployeeRequest : EmployeeProfileRequest
{
    public Guid? InitialBranchId { get; set; }
}
