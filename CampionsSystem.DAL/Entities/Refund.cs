using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class Refund
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SaleTransactionId { get; set; }
    public SaleTransaction SaleTransaction { get; set; } = null!;
    public Guid? OriginalPaymentId { get; set; }
    public Payment? OriginalPayment { get; set; }
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid? ShiftId { get; set; }
    public Shift? Shift { get; set; }
    public decimal Amount { get; set; }
    public RefundType RefundType { get; set; }
    public string Reason { get; set; } = null!;
    public Guid ProcessedByUserId { get; set; }
    public ApplicationUser ProcessedByUser { get; set; } = null!;
    public Guid? ApprovedByUserId { get; set; }
    public ApplicationUser? ApprovedByUser { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<RefundPart> Parts { get; set; } = new List<RefundPart>();
}
