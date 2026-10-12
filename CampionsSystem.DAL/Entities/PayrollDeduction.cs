using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class PayrollDeduction
{
    public Guid? EmployeeAdvanceId { get; set; }
    public EmployeeAdvance? EmployeeAdvance { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PayrollId { get; set; }
    public Payroll Payroll { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
