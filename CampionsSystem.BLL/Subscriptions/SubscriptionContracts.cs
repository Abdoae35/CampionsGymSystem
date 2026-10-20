using CampionsSystem.DAL.Enums;
using System.Text.Json.Serialization;
namespace CampionsSystem.BLL.Subscriptions;

public static class SubscriptionPermissions
{
    public const string View = "Subscriptions.View";
    public const string Manage = "Subscriptions.Manage";
    public const string Freeze = "Subscriptions.Freeze";
    public const string Extend = "Subscriptions.Extend";
    public static readonly string[] Codes = [View, Manage, Freeze, Extend];
}
public sealed class SubscriptionHistoryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public Guid? BranchId { get; set; }
    public SubscriptionStatus? Status { get; set; }
    public PlanCategory? PlanType { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public bool IncludePending { get; set; }
}
public sealed record SubscriptionSummary(Guid Id, Guid MemberId, Guid SourcePlanId, string? PlanName,
    string? PlanType, Guid OriginBranchId, Guid AccessCityId, string Mode, DateOnly StartDate,
    DateOnly? EndDate, string Status, int? TotalSessions, int? RemainingSessions, int Version,
    Guid? EmployeeId, Guid? FollowsSubscriptionId, bool IsStandaloneVisit = false,
    string? OriginBranchName = null, string? AccessCityName = null);
public sealed record SubscriptionHistory(IReadOnlyList<SubscriptionSummary> Items, int TotalCount, int Page, int PageSize);
// Opt-in history view. The legacy history item keeps its required Id and StartDate contract.
public sealed record SubscriptionHistoryItem(Guid? SubscriptionId, Guid? SaleId, Guid MemberId, Guid PlanId,
    string? PlanName, string? PlanType, Guid OriginBranchId, Guid AccessCityId, string Mode,
    DateOnly? StartDate, DateOnly? EndDate, string ActivationStatus, int? TotalSessions,
    int? RemainingSessions, DateOnly? SaleBusinessDate, string? PurchaseMode, int? PurchasedDurationDays,
    decimal? FinalPrice, decimal? Collected, string? PaymentStatus,
    string? OriginBranchName = null, string? AccessCityName = null);
public sealed record SubscriptionPurchaseHistory(IReadOnlyList<SubscriptionHistoryItem> Items, int TotalCount, int Page, int PageSize);
public sealed record BenefitSnapshot(Guid Id, Guid GrantId, string Code, string Name, string GrantType, int? GrantedQuantity, bool Cancelled = false);
public sealed record SubscriptionGrantDetails(Guid Id, Guid PlanId, string? PlanName, string PlanType,
    Guid? PlanBranchId, Guid? AccessCityId, string? Mode, int DurationDays, int? Sessions, int? FreezeDays,
    decimal PlanPrice, decimal NetPrice, Guid? FreelanceCoachId, string TransactionType,
    DateOnly? StartDate, DateOnly? PreviousEndDate, DateOnly? NewEndDate, int? PreviousRemainingSessions,
    Guid? RenewedFromSubscriptionId, Guid? BusinessDayId, Guid? ActorId, DateTimeOffset CreatedAt, Guid? CancellationRefundId = null);
public sealed record FreezeDetails(Guid Id, string Type, DateOnly StartDate, DateOnly? EndDate, int? DaysApplied,
    string? Reason, Guid ApprovedBy, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt, int DaysAlreadyCompensated,
    DateOnly? PreviousSubscriptionEndDate, DateOnly? NewSubscriptionEndDate);
public sealed record HolidayExtensionDetails(Guid ClosureId, DateOnly ProtectedDate, int DaysApplied);
public sealed record SubscriptionDetails(SubscriptionSummary Subscription, string MemberName,
    int? FreezeAllowance, int ConsumedFreezeDays, int? RemainingFreezeDays,
    IReadOnlyList<BenefitSnapshot> Benefits, IReadOnlyList<SubscriptionGrantDetails> Grants,
    IReadOnlyList<FreezeDetails> Freezes, IReadOnlyList<HolidayExtensionDetails> Holidays);
// Integration commands are BLL-only: no public paid activation/renewal route before Finance exists.
public sealed record SubscriptionGrantCommand(Guid MemberId, Guid PlanId, Guid BranchId, Guid OperationId);
public sealed record SubscriptionRenewalCommand(Guid SubscriptionId, Guid PlanId, Guid BranchId, Guid OperationId, int ExpectedVersion);
public sealed record SubscriptionGrantResult(Guid SubscriptionId, Guid GrantId, bool Scheduled);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record EmployeeEntitlementRequest(Guid MemberId, Guid PlanId, Guid OperationId);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record FreezeRequest(Guid OperationId, int ExpectedVersion, DateOnly StartDate, int Days);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CompleteFreezeRequest(int ExpectedVersion);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CustomExtensionRequest(Guid OperationId, int ExpectedVersion, int Days, string Reason);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OfficialHolidayRequest(Guid BranchId, DateOnly StartDate, DateOnly EndDate, bool ExtendSubscriptions, string Reason);
public sealed record HolidayApplicationResult(Guid ClosureId, int ProtectedDays, int ExtendedDays);
public sealed record MemberEntitlement(bool HasAccess, SubscriptionSummary? Subscription);
