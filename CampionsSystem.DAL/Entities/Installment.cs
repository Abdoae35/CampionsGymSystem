using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class Installment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstallmentAgreementId { get; set; }
    public InstallmentAgreement InstallmentAgreement { get; set; } = null!;
    public int InstallmentNumber { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public InstallmentStatus Status { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
}
