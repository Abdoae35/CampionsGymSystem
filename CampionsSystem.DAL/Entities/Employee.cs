using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public EmploymentType EmploymentType { get; set; }
    public string? JobTitle { get; set; }
    public DateOnly HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
