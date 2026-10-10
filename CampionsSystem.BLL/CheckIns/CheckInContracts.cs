using CampionsSystem.DAL.Enums;
using System.Text.Json.Serialization;
namespace CampionsSystem.BLL.CheckIns;

public static class CheckInPermissions
{
    public const string View = "CheckIns.View";
    public const string Create = "CheckIns.Create";
    public static readonly string[] Codes = [View, Create];
}
public static class AccessReasons
{
    public const string InstallmentOverdue = "InstallmentOverdue";
    public const string PendingFirstCheckIn = "PendingFirstCheckIn";
    public const string Allowed = "Allowed";
    public const string MemberNotFound = "MemberNotFound";
    public const string MemberInactive = "MemberInactive";
    public const string NoGymEntitlement = "NoGymEntitlement";
    public const string SubscriptionNotStarted = "SubscriptionNotStarted";
    public const string SubscriptionExpired = "SubscriptionExpired";
    public const string SubscriptionFrozen = "SubscriptionFrozen";
    public const string SubscriptionCancelled = "SubscriptionCancelled";
    public const string SubscriptionSuspended = "SubscriptionSuspended";
    public const string NoSessionsRemaining = "NoSessionsRemaining";
    public const string AccessCityMismatch = "AccessCityMismatch";
    public const string BranchClosed = "BranchClosed";
    public const string BranchInactive = "BranchInactive";
    public const string BorrowLimitReached = "BorrowLimitReached";
    public const string BorrowConfirmationRequired = "BorrowConfirmationRequired";
    public const string BorrowCycleRenewed = "BorrowCycleRenewed";
    public const string BorrowNotEligible = "BorrowNotEligible";
    public const string EmployeeEntitlementInactive = "EmployeeEntitlementInactive";
    public const string AmbiguousEntitlement = "AmbiguousEntitlement";
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AccessRequest(Guid? MemberId, string? MemberCode, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateCheckInRequest(Guid OperationId, CheckInMethod Method, Guid? MemberId = null,
    string? MemberCode = null, bool ConfirmBorrow = false, Guid? BranchId = null);
public sealed record AccessDecision(bool Allowed, string ReasonCode, Guid? MemberId, string? MemberCode,
    string? MemberName, Guid BranchId, DateOnly BusinessDate, Guid? BusinessDayId,
    Guid? SubscriptionId, string? SubscriptionMode, int? RemainingSessions, bool BorrowAvailable,
    int BorrowedUsed, int BorrowedRemaining, bool CanActivateOnCheckIn = false);
public sealed record CheckInDetails(Guid Id, Guid? OperationId, Guid? MemberId, string? MemberCode,
    string? MemberName, Guid BranchId, DateOnly? BusinessDate, Guid? BusinessDayId,
    DateTimeOffset OccurredAt, string Type, string Method, Guid? SubscriptionId, bool SessionConsumed,
    int? SessionsBefore, int? SessionsAfter, Guid ActorId);
public sealed record CheckInResult(AccessDecision Decision, CheckInDetails? CheckIn);
public sealed class CheckInHistoryRequest
{
    public Guid? BranchId { get; set; }
    public Guid? MemberId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public CheckInType? Type { get; set; }
    public CheckInMethod? Method { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public sealed record CheckInHistory(IReadOnlyList<CheckInDetails> Items, int TotalCount, int Page, int PageSize);
public sealed record BorrowedDebtDetails(Guid Id, Guid ExpiredSubscriptionId, Guid? CheckInId, Guid BranchId,
    DateTimeOffset BorrowedAt, string Status, Guid? SettledBySubscriptionId, Guid? SettledByTransactionId, DateTimeOffset? SettledAt,
    IReadOnlyList<BorrowedSettlementDetails>? SettlementHistory = null);
public sealed record BorrowedSettlementDetails(int Sequence, string Action, Guid GrantId, Guid? RefundId, DateTimeOffset CreatedAt);
public sealed class CheckInMemberSearchRequest
{
    public string Search { get; set; } = "";
    public Guid? BranchId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public sealed record CheckInMemberMatch(Guid Id,string MemberCode,string Name,bool IsActive,Guid RegistrationBranchId,long? ClientCode);
public sealed record CheckInMemberMatches(IReadOnlyList<CheckInMemberMatch> Items,int TotalCount,int Page,int PageSize);
public sealed class BorrowedDebtHistoryRequest
{
    public Guid? BranchId { get; set; }
    public Guid? ExpiredSubscriptionId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public sealed record BorrowedDebtHistory(IReadOnlyList<BorrowedDebtDetails> Items,int TotalCount,int Page,int PageSize);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RedeemTrialRequest(Guid OperationId,Guid InvitationId,string Phone,Guid? BranchId=null);
public sealed record TrialCheckInResult(bool Allowed,string ReasonCode,Guid InvitationId,CheckInDetails? CheckIn);
public static class TrialReasons
{
    public const string InvitationNotFound="InvitationNotFound";
    public const string InvitationUnavailable="InvitationUnavailable";
    public const string InvitationBranchMismatch="InvitationBranchMismatch";
    public const string InvitationIdentityMismatch="InvitationIdentityMismatch";
    public const string TrialAlreadyUsed="TrialAlreadyUsed";
    public const string InvitationBenefitInvalid="InvitationBenefitInvalid";
    public const string InvitationBenefitExhausted="InvitationBenefitExhausted";
}
