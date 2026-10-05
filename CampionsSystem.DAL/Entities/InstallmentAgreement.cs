using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class InstallmentAgreement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SaleTransactionId { get; set; }
    public SaleTransaction SaleTransaction { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public int NumberOfInstallments { get; set; }
    public DateOnly StartDate { get; set; }
    public InstallmentAgreementStatus Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<Installment> Installments { get; set; } = new List<Installment>();
}
