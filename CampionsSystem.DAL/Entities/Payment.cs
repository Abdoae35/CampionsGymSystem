using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class Payment
{
    // Nullable for legacy reconciliation; preserve operational date independently of real timestamps.
    public Guid? BusinessDayId { get; set; }
    public BusinessDay? BusinessDay { get; set; }
    public Guid? OperationId { get; set; }
    public string? RequestHash { get; set; }
    public DateOnly? BusinessDate { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SaleTransactionId { get; set; }
    public SaleTransaction SaleTransaction { get; set; } = null!;
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid? ShiftId { get; set; }
    public Shift? Shift { get; set; } = null!;
    public Guid? CommissionPeriodId { get; set; }
    public CommissionPeriod? CommissionPeriod { get; set; } = null!;
    public Guid? InstallmentId { get; set; }
    public Installment? Installment { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public Guid ReceivedByUserId { get; set; }
    public ApplicationUser ReceivedByUser { get; set; } = null!;
    public string? Notes { get; set; }
    public ICollection<PaymentPart> Parts { get; set; } = new List<PaymentPart>();
}
