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
