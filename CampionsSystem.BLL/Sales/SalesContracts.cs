using System.Text.Json.Serialization;
using CampionsSystem.DAL.Enums;
namespace CampionsSystem.BLL.Sales;
public static class SalesPermissions
{
    public const string View = "Payments.View";
    public const string Create = "Payments.Create";
    public const string Refund = "Payments.Refund";
    public const string Offers = "Offers.Manage";
    public const string Installments = "Installments.Manage";
    public const string QuickCancel = "Sales.QuickCancel";
    public static readonly string[] Codes = [View, Create, Refund, Offers, Installments, QuickCancel];
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PaymentPartInput(Guid FinancialAccountId, decimal Amount, string? ProviderReference = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record InstallmentLineInput(decimal Amount, DateOnly DueDate);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record InstallmentAllocationInput(Guid InstallmentId, decimal Amount);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateSaleRequest(Guid OperationId, Guid MemberId, Guid MembershipPlanId,
    PurchaseMode PurchaseMode, SubscriptionActivationMode ActivationMode,
    IReadOnlyList<PaymentPartInput> PaymentParts, Guid? OfferId = null, DateOnly? StartDate = null,
    DateOnly? ReservationDueDate = null, IReadOnlyList<InstallmentLineInput>? Installments = null, Guid? BranchId = null, decimal? InitialPaymentRequired = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RecordPaymentRequest(Guid OperationId, decimal Amount, IReadOnlyList<PaymentPartInput> Parts,
    IReadOnlyList<InstallmentAllocationInput>? Allocations = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RefundRequest(Guid OperationId, decimal Amount, IReadOnlyList<PaymentPartInput> Parts,
    string Reason, bool CancelEntitlement = true);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record QuickCancelRequest(Guid OperationId, string Reason);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OfferRequest(string Name, OfferType Type, decimal Value, IReadOnlyList<Guid> MembershipPlanIds,
    OfferEligibilityMode? EligibilityMode, Guid? AudienceCategoryId = null,
    DateOnly? StartDate = null, DateOnly? EndDate = null, bool IsPermanent = false,
    bool IsActive = true, string? Description = null, Guid? BranchId = null);
public sealed record OfferDetails(Guid Id, Guid? BranchId, string Name, string Type, decimal Value,
    DateOnly? StartDate, DateOnly? EndDate, bool IsPermanent, bool IsActive, IReadOnlyList<Guid> MembershipPlanIds, string? EligibilityMode, Guid? AudienceCategoryId);
public sealed record PricingResult(decimal OriginalPrice, decimal Discount, decimal FinalPrice, int ExtraDays,
    Guid? OfferId, string? OfferName, OfferType? OfferType, decimal? OfferValue,
    OfferEligibilityMode? EligibilityMode = null, Guid? AudienceCategoryId = null);
public sealed record FinancialPartDetails(Guid AccountId, string AccountName, string Method, decimal Amount, string? Reference);
public sealed record PaymentDetails(Guid Id, Guid? OperationId, Guid BranchId, DateOnly? BusinessDate,
    Guid? BusinessDayId, DateTimeOffset PaidAt, decimal Amount, IReadOnlyList<FinancialPartDetails> Parts,
    IReadOnlyList<InstallmentAllocationInput> Allocations);
public sealed record RefundDetails(Guid Id, Guid? OperationId, Guid BranchId, DateOnly? BusinessDate,
    DateTimeOffset CreatedAt, decimal Amount, string Type, string Reason, IReadOnlyList<FinancialPartDetails> Parts);
public sealed record InstallmentDetails(Guid Id, int Number, DateOnly DueDate, decimal Amount, decimal Paid, string Status);
public sealed record SaleSummary(Guid Id, Guid MemberId, Guid BranchId, DateOnly? BusinessDate, DateTimeOffset CreatedAt,
    decimal OriginalPrice, decimal Discount, decimal FinalPrice, decimal Collected, decimal Refunded, decimal Outstanding,
    string Status, string? PurchaseMode, string? ActivationMode, bool Cancelled, int Version);
public sealed record SaleDetails(SaleSummary Sale, string MemberName, string? PlanName, string? PlanType,
    Guid? PlanId, Guid? AccessCityId, Guid? SalesEmployeeId, string? SalesAttribution,
    bool? CommissionEligible, bool? TargetEligible, Guid? SubscriptionId, Guid? GrantId,
    string ActivationStatus, DateOnly? RequestedStartDate, DateOnly? ReservationDueDate,
    string? OfferName, int ExtraDays, decimal RetainedCancellationRevenue,
    IReadOnlyList<PaymentDetails> Payments, IReadOnlyList<RefundDetails> Refunds, IReadOnlyList<InstallmentDetails> Installments);
public sealed class SaleHistoryRequest
{
    public Guid? BranchId { get; set; }
    public Guid? MemberId { get; set; }
    public PurchaseMode? PurchaseMode { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public sealed record SaleHistory(IReadOnlyList<SaleSummary> Items, int TotalCount, int Page, int PageSize);
public sealed record BranchFinancialTotals(Guid BranchId, decimal CashCollected, decimal InstaPayCollected,
    decimal CashRefunded, decimal InstaPayRefunded, decimal GrossCollected, decimal Refunds, decimal NetCollected);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record GrantMemberOfferRequest(Guid MemberId, DateOnly? StartDate = null, DateOnly? EndDate = null);
public sealed record MemberOfferGrantDetails(Guid Id, Guid MemberId, Guid OfferId, DateOnly? StartDate, DateOnly? EndDate, DateTimeOffset? RevokedAt);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SetMemberAudienceRequest(IReadOnlyList<Guid> AudienceCategoryIds);
public sealed record MemberAudienceDetails(Guid Id, Guid CategoryId, string CategoryName, DateTimeOffset AssignedAt, DateTimeOffset? EndedAt);

public sealed record SaleReceipt(Guid SaleId, IReadOnlyList<Guid> PaymentIds);
