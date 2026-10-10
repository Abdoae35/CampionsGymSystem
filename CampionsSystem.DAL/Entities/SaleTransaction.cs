using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class SaleTransaction
{
    // Nullable for legacy reconciliation; preserve operational date independently of real timestamps.
    public Guid? BusinessDayId { get; set; }
    public BusinessDay? BusinessDay { get; set; }
    // Historical attribution: null on legacy rows; House suppresses Sales commissions.
    public AssignmentType? SalesAttributionSnapshot { get; set; }

    // Null operation identifies legacy records; new sales preserve request identity and sold terms.
    public Guid? OperationId { get; set; }
    public string? RequestHash { get; set; }
    public DateOnly? BusinessDate { get; set; }
    public PurchaseMode? PurchaseMode { get; set; }
    public SubscriptionActivationMode? ActivationMode { get; set; }
    public DateOnly? RequestedStartDate { get; set; }
    public DateOnly? ReservationDueDate { get; set; }
    public decimal? InitialPaymentRequired { get; set; }
    public OfferEligibilityMode? OfferEligibilityModeSnapshot { get; set; }
    public Guid? OfferAudienceCategoryIdSnapshot { get; set; }
    public MembershipPlanCategory? OfferAudienceCategorySnapshot { get; set; }
    public string? OfferNameSnapshot { get; set; }
    public OfferType? OfferTypeSnapshot { get; set; }
    public decimal? OfferValueSnapshot { get; set; }
    public int ExtraDaysSnapshot { get; set; }
    public bool? CommissionEligible { get; set; }
    public bool? TargetEligible { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public ApplicationUser? CancelledByUser { get; set; }
    public int Version { get; set; }
    public SaleMembershipSnapshot? MembershipSnapshot { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid? CreatedShiftId { get; set; }
    public Shift? CreatedShift { get; set; }
    public SaleType SaleType { get; set; }
    public SaleCategory? SaleCategory { get; set; }
    public Guid? SalesEmployeeId { get; set; }
    public Employee? SalesEmployee { get; set; }
    public Guid? OfferId { get; set; }
    public Offer? Offer { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public SaleStatus Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Notes { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
