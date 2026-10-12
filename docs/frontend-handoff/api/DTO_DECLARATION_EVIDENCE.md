# Exposed DTO declaration evidence

Completed backend C# declarations; not all BLL commands in a contract file are public endpoints. Only types linked from the catalog are exposed. Defaults, validation attributes and converters are preserved. Full controller catalog is the exposure authority.

## CampionsSystem.API/Controllers/OperationsController.cs

```csharp

using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Branches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampionsSystem.API.Controllers;

[ApiController]
[Route("api")]

```

## CampionsSystem.BLL/Authentication/AuthContracts.cs

```csharp

using System.ComponentModel.DataAnnotations;

namespace CampionsSystem.BLL.Authentication;

public sealed class LoginRequest
{
    [Required, MaxLength(256)]
    public string UserNameOrEmail { get; init; } = string.Empty;
    [Required, MaxLength(256)]
    public string Password { get; init; } = string.Empty;
}

public sealed record BranchContext(Guid Id, string Name);
public sealed record CurrentUserContext(Guid UserId, string? UserName, Guid? EmployeeId,
    string? EmployeeName, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions,
    BranchContext? CurrentBranch)
{
    public Guid? CurrentBranchId => CurrentBranch?.Id;
}
public sealed record LoginUser(Guid Id, string? UserName, Guid? EmployeeId, string? FullName,
    IReadOnlyList<string> Roles);
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, LoginUser User);

public interface IAuthenticationService
{
    Task<CurrentUserContext?> ValidateCredentialsAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
public interface IPermissionService
{
    Task<HashSet<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
// API provides claims access; BLL never depends on HttpContext.
public interface ICurrentUserIdentity
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
}
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    // DB-backed EmployeeId, CurrentBranchId, Roles and Permissions are asynchronous.
    Task<CurrentUserContext?> GetAsync(CancellationToken cancellationToken = default);
}

public sealed class AuthContextOptions
{
    public const string SectionName = "AuthContext";
    public string TimeZoneId { get; init; } = "Africa/Cairo";
}

```

## CampionsSystem.BLL/Branches/BranchContracts.cs

```csharp

using System.ComponentModel.DataAnnotations;

namespace CampionsSystem.BLL.Branches;

public sealed record SaveBranchRequest(
    [Required, MaxLength(256)] string Name,
    Guid CityId,
    [MaxLength(256)] string? Address = null,
    [MaxLength(64)] string? Phone = null,
    bool IsActive = true,
    [Range(0, int.MaxValue)] int MaxBorrowedGymSessions = 2,
    [Range(0, int.MaxValue)] int QuickRefundWindowHours = 24,
    [RegularExpression(@"^[+][1-9][0-9]{0,2}$")] string? CountryCallingCode = null);

public sealed record BranchDetails(Guid Id, string Name, Guid CityId, string CityName,
    string? Address, string? Phone, bool IsActive, int MaxBorrowedGymSessions, int QuickRefundWindowHours, string? CountryCallingCode = null);

public sealed class BranchNotFoundException(string message) : Exception(message);

```

## CampionsSystem.BLL/Branches/EmployeeBranchAssignmentService.cs

```csharp

using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Branches;

public sealed record BranchAssignmentDetails(Guid Id, Guid EmployeeId, Guid BranchId, string BranchName,
    DateOnly StartDate, DateOnly? EndDate, bool IsPrimary, Guid AssignedByUserId, DateTimeOffset CreatedAt);
public sealed record AssignBranchRequest(Guid BranchId, DateOnly StartDate);
public sealed record TransferBranchRequest(Guid TargetBranchId, DateOnly EffectiveDate);

```

## CampionsSystem.BLL/CheckIns/CheckInContracts.cs

```csharp

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

```

## CampionsSystem.BLL/Coaching/CoachingContracts.cs

```csharp

using System.Text.Json.Serialization;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Enums;
namespace CampionsSystem.BLL.Coaching;

public static class CoachingPermissions
{
    public const string PrivateView = "Private.View", PrivateManage = "Private.Manage",
        RegisterPrivate = "Private.RegisterSession", Transfer = "Private.Transfer", Expiry = "Private.SettleExpiry",
        ClassesView = "Classes.View", ClassesManage = "Classes.Manage", RegisterClass = "Classes.RegisterAttendance",
        EarningsView = "CoachEarnings.View", Targets = "CoachEarnings.ManageTargets",
        Recognize = "CoachEarnings.Recognize", Pay = "CoachEarnings.Pay";
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CoachingPackageRequest(Guid BranchId, string Name, int SessionCount, int DurationDays, decimal Price, bool IsActive = true);
public sealed record CoachingPackageDetails(Guid Id, Guid BranchId, string Name, int SessionCount, int DurationDays, decimal Price, bool IsActive);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CoachingPurchaseRequest(Guid OperationId, Guid MemberId, Guid PackageId,
    IReadOnlyList<PaymentPartInput> PaymentParts, Guid? CoachEmployeeId = null,
    PurchaseMode PurchaseMode = PurchaseMode.FullPayment, IReadOnlyList<InstallmentLineInput>? Installments = null);
public sealed record CoachingPurchaseReceipt(Guid SubscriptionId, Guid SaleId, IReadOnlyList<Guid> PaymentIds);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PrivateAttendanceRequest(Guid OperationId, Guid GymCheckInId, string? Notes = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PrivateTransferRequest(Guid OperationId, Guid CoachEmployeeId, int ExpectedVersion, string Reason);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PrivateExpiryRequest(Guid OperationId, Guid CommissionPeriodId, string Reason);
public sealed record PrivateAssignmentDetails(Guid Id, Guid CoachEmployeeId, DateTimeOffset AssignedAt, DateTimeOffset? EndedAt, string? Reason);
public sealed record PrivateAttendanceDetails(Guid Id, Guid SubscriptionId, Guid CoachEmployeeId, Guid GymCheckInId,
    DateOnly? BusinessDate, DateTimeOffset SessionAt, int? SessionNumber, decimal? SessionValue, bool? TransferredPackage);
public sealed record PrivateDetails(Guid Id, Guid MemberId, Guid BranchId, DateOnly StartDate, DateOnly EndDate,
    int PurchasedSessions, int UsedSessions, int RemainingSessions, int Version, bool Cancelled,
    string? PackageName, decimal Price, decimal UnitSessionValue, decimal RemainderValue,
    Guid? OriginalCoachEmployeeId, IReadOnlyList<PrivateAssignmentDetails> Assignments);
public sealed record PrivateExpiryDetails(Guid Id, Guid SubscriptionId, Guid CoachEmployeeId, int UnusedSessions,
    decimal EligibleValue, bool? TransferredPackage, DateOnly? BusinessDate, DateTimeOffset SettledAt, string? Reason);
public sealed record CoachingMemberResult(Guid Id, string MemberCode, long? ClientCode, string Name, string Phone);
public sealed record CoachingPage<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
public sealed record ClassesSubscriptionDetails(Guid Id, Guid MemberId, Guid BranchId, Guid SaleId, string PackageName,
    DateOnly StartDate, DateOnly EndDate, int PurchasedSessions, int RemainingSessions, decimal Price, bool Cancelled, int Version);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ClassTypeRequest(Guid BranchId, string Name, Guid CoachEmployeeId, bool IsActive = true, string? Description = null);
public sealed record ClassTypeDetails(Guid Id, Guid BranchId, string Name, Guid CoachEmployeeId, bool IsActive, string? Description);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ClassOccurrenceRequest(Guid OperationId, Guid ClassTypeId, Guid CoachEmployeeId, DateTimeOffset StartsAt);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CompleteClassRequest(Guid OperationId, int ExpectedVersion, bool Cancelled = false);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ClassAttendanceRequest(Guid OperationId, Guid MemberId, Guid? ClassesSubscriptionId = null);
public sealed record ClassOccurrenceDetails(Guid Id, Guid ClassTypeId, Guid BranchId, Guid CoachEmployeeId,
    DateOnly? BusinessDate, DateTimeOffset StartsAt, DateTimeOffset? EndedAt, string Status, int Attendance, int Version);
public sealed record ClassAttendanceDetails(Guid Id, Guid MemberId, Guid SubscriptionId, Guid ClassSessionId,
    Guid CoachEmployeeId, DateOnly BusinessDate, int RemainingSessions, DateTimeOffset RegisteredAt);
public sealed record ClassCoachStatistics(Guid CoachEmployeeId, int CompletedClasses, int TotalAttendance, decimal AverageAttendance,
    decimal? Target, bool? TargetReached, IReadOnlyList<ClassOccurrenceDetails> Occurrences, string CompensationStatus = "NotConfigured");
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CoachTargetRequest(Guid OperationId, Guid CommissionPeriodId, Guid CoachEmployeeId,
    decimal? PrivateTarget, decimal? ClassesAverageTarget);
public sealed record CoachTargetDetails(Guid Id, Guid PeriodId, Guid CoachEmployeeId, decimal? PrivateTarget, decimal? ClassesAverageTarget);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RecognizeCoachEarningsRequest(Guid OperationId, Guid CommissionPeriodId);
public sealed record CoachProgress(Guid CoachEmployeeId, decimal EligibleValue, decimal? Target, bool? TargetReached,
    decimal? NormalRate, decimal? ProjectedEarnings, decimal DeferredValue);
public sealed record CoachEarningsProgress(Guid PeriodId, bool Finalized, IReadOnlyList<CoachProgress> Coaches);
public sealed record CoachEarningDetails(Guid Id, Guid CoachEmployeeId, Guid PeriodId, Guid SaleId, Guid? SessionId,
    Guid? ExpirySettlementId, decimal EligibleValue, decimal Rate, decimal Earned, bool TransferredPackage, DateOnly BusinessDate);
public sealed record CoachBalance(Guid BranchId, Guid CoachEmployeeId, decimal Earned, decimal Paid, decimal Outstanding);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CoachPayoutRequest(Guid OperationId, Guid BranchId, Guid CoachEmployeeId, decimal Amount,
    Guid FinancialAccountId, Guid ExpenseCategoryId, string Reason, string? ReceiptReference = null);
public sealed record CoachPayoutDetails(Guid Id, Guid BranchId, Guid CoachEmployeeId, Guid ExpenseId,
    Guid AccountId, string Method, decimal Amount, decimal BalanceBefore, decimal BalanceAfter,
    DateOnly BusinessDate, DateTimeOffset PaidAt, Guid AuthorizedByUserId);

```

## CampionsSystem.BLL/Commissions/CommissionContracts.cs

```csharp

using System.Text.Json.Serialization;
namespace CampionsSystem.BLL.Commissions;
public static class CommissionPermissions
{
    public const string View = "Commission.View", Manage = "Commission.ManageRules", Close = "Commission.ClosePeriod";
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CategoryRuleInput(decimal Target, decimal BelowRate, decimal ReachedRate);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ConfigureCommissionRequest(Guid OperationId, Guid? EmployeeId, DateOnly EffectiveFrom,
    CategoryRuleInput NewClients, CategoryRuleInput Renewals, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OpenCommissionPeriodRequest(Guid OperationId, string Name, DateOnly StartDate, DateOnly EndDate, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record FinalizeCommissionPeriodRequest(Guid OperationId, int ExpectedVersion);
public sealed record CommissionPeriodDetails(Guid Id, Guid BranchId, string Name, DateOnly? StartDate, DateOnly? EndDate,
    string Status, int Version, DateTimeOffset? FinalizedAt);
public sealed record CommissionRuleDetails(Guid Id, Guid? EmployeeId, Guid BranchId, string Category, DateOnly EffectiveFrom,
    decimal? Target, decimal? BelowRate, decimal? ReachedRate, int Revision);
public sealed record CommissionCategoryProgress(string Category, decimal GrossCollections, decimal Refunds, decimal EligibleCollections,
    bool Configured, decimal? Target, bool TargetReached, decimal? BelowRate, decimal? ReachedRate, decimal? AppliedRate, decimal Commission, Guid? RuleId)
{
    public decimal RetainedCollections => GrossCollections - Refunds;
}
public sealed record EmployeeCommissionStatement(Guid EmployeeId, string EmployeeName, CommissionCategoryProgress NewClients,
    CommissionCategoryProgress Renewals, decimal TotalCommission, Guid PeriodId, bool Finalized, decimal HouseCollections, decimal ExcludedCollections);
public sealed record CommissionProgress(CommissionPeriodDetails Period, bool Finalized, decimal GrossCollections, decimal Refunds,
    decimal RetainedCollections, decimal EmployeeEligibleCollections, decimal HouseCollections, decimal HouseNewClientCollections, decimal HouseRenewalCollections, decimal ExcludedCollections,
    decimal NewClientEligibleCollections, decimal RenewalEligibleCollections, decimal TotalCommission, int EmployeeCount,
    IReadOnlyList<EmployeeCommissionStatement> Employees, int Page, int PageSize);
public sealed record CommissionSourceLine(Guid PaymentId, Guid SaleId, Guid MemberId, Guid BranchId, Guid? OriginalSalesEmployeeId,
    Guid? EmployeeId, string? Category, string EligibilityReason, DateOnly? BusinessDate, DateTimeOffset PaidAt,
    decimal GrossCollection, decimal Refunds, decimal RetainedCollection, decimal EligibleCollection);
public sealed record CommissionPage<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record CommissionRefundSource(Guid RefundId, Guid PaymentId, decimal Amount, DateOnly? CollectionBusinessDate,
    DateOnly? RefundBusinessDate, DateTimeOffset RefundedAt);

```

## CampionsSystem.BLL/Crm/CrmContracts.cs

```csharp

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CampionsSystem.DAL.Enums;
using CampionsSystem.BLL.Members;
namespace CampionsSystem.BLL.Crm;

[JsonConverter(typeof(JsonStringEnumConverter<FollowUpWindow>))]
public enum FollowUpWindow { Overdue, Today, Upcoming }
public sealed class LeadListRequest
{
    [MaxLength(256)] public string? Search { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? SalesEmployeeId { get; set; }
    public bool? UnassignedOnly { get; set; }
    public LeadStatus? Status { get; set; }
    public FollowUpWindow? FollowUp { get; set; }
    [Range(1,int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1,100)] public int PageSize { get; set; } = 25;
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateLeadRequest
{
    public Guid BranchId { get; set; }
    [MaxLength(256)] public string? FullName { get; set; }
    [Required,MaxLength(64)] public string PrimaryPhone { get; set; } = "";
    [MaxLength(64)] public string? SecondaryPhone { get; set; }
    [EmailAddress,MaxLength(320)] public string? Email { get; set; }
    [MaxLength(256)] public string? Source { get; set; }
    [MaxLength(4096)] public string? Notes { get; set; }
}
public sealed record LeadAssignmentDetails(Guid Id, Guid SalesEmployeeId, string SalesEmployeeName,
    DateTimeOffset AssignedAt, DateTimeOffset? EndedAt, Guid AssignedByUserId, string? Reason);
public sealed record LeadDetails(Guid Id, Guid BranchId, string BranchName, string? FullName,
    string PrimaryPhone, string? PrimaryPhoneNormalized, string? SecondaryPhone, string? Email,
    string? Source, string? Notes, string Status, DateTimeOffset CreatedAt,
    DateTimeOffset? NextFollowUpAt, LeadAssignmentDetails? CurrentAssignment,
    Guid? ConvertedMemberId, DateTimeOffset? ConvertedAt, DateTimeOffset? LostAt, string? LostReason);
public sealed record LeadListResult(IReadOnlyList<LeadDetails> Items, int TotalCount, int Page, int PageSize);
// Private details require visibility. A phone lookup may expose only a Lead ID to an authorized conversion operator.
public sealed record LeadCreationResult(string Result, LeadDetails? Lead = null, Guid? ExistingMemberId = null, string? Error = null, Guid? ExistingLeadId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LeadAssignmentRequest
{
    public Guid SalesEmployeeId { get; set; }
    [MaxLength(2048)] public string? Reason { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LeadInteractionRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter<InteractionType>))]
    public InteractionType InteractionType { get; set; }
    [Required,MaxLength(4096)] public string Notes { get; set; } = "";
    [MaxLength(256)] public string? Outcome { get; set; }
    public DateTimeOffset? ContactedAt { get; set; }
    public DateTimeOffset? NextFollowUpAt { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter<LeadStatus>))]
    public LeadStatus Status { get; set; } = LeadStatus.Contacted;
}
public sealed record LeadInteractionDetails(Guid Id, Guid EmployeeId, Guid? CreatedByUserId,
    string InteractionType, string Notes, string? Outcome, DateTimeOffset ContactedAt,
    DateTimeOffset? NextFollowUpAt, DateTimeOffset CreatedAt);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LeadStatusRequest
{
    [Required, JsonConverter(typeof(JsonStringEnumConverter<LeadStatus>))]
    public LeadStatus? Status { get; set; }
    [MaxLength(2048)] public string? Reason { get; set; }
    public DateTimeOffset? NextFollowUpAt { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ConvertLeadRequest : UpdateMemberRequest
{
    public Guid TargetRegistrationBranchId { get; set; }
    // Explicit exception; never inferred from missing owner.
    public Guid? FreelancePlanId { get; set; }
}
public sealed class FollowUpListRequest
{
    public FollowUpWindow? Window { get; set; }
    public Guid? BranchId { get; set; }
    [Range(1,int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1,100)] public int PageSize { get; set; } = 25;
}
public sealed record FollowUpItem(Guid Id, string SubjectType, Guid? LeadId, Guid? MemberId,
    string Name, string Phone, Guid BranchId, string BranchName, string Reason, DateTimeOffset DueAt,
    bool CrossBranch);
public sealed record FollowUpListResult(IReadOnlyList<FollowUpItem> Items, int TotalCount, int Page, int PageSize);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class MemberFollowUpRequest
{
    [Required] public string Reason { get; set; } = "Reactivation";
    public DateTimeOffset DueAt { get; set; }
    [MaxLength(4096)] public string? Notes { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class UpdateLeadRequest
{
    [MaxLength(256)] public string? FullName { get; set; }
    [Required,MaxLength(64)] public string PrimaryPhone { get; set; } = "";
    [MaxLength(64)] public string? SecondaryPhone { get; set; }
    [EmailAddress,MaxLength(320)] public string? Email { get; set; }
    [MaxLength(256)] public string? Source { get; set; }
    [MaxLength(4096)] public string? Notes { get; set; }
}

```

## CampionsSystem.BLL/Employees/EmployeeContracts.cs

```csharp

using System.ComponentModel.DataAnnotations;
using CampionsSystem.BLL.Authentication;

namespace CampionsSystem.BLL.Employees;

public sealed class EmployeeListRequest
{
    [MaxLength(256)] public string? Search { get; set; }
    public Guid? BranchId { get; set; }
    public DAL.Enums.EmploymentStatus? Status { get; set; }
    public DAL.Enums.EmploymentType? EmploymentType { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed record EmployeeDetails(Guid Id, string FullName, string Phone, string? JobTitle,
    string EmploymentType, string Status, DateOnly HireDate, DateOnly? TerminationDate,
    BranchContext? CurrentBranch, bool HasSystemAccount);
public sealed record EmployeeListResult(IReadOnlyList<EmployeeDetails> Items, int TotalCount, int Page, int PageSize);

// Reject unrelated fields rather than silently accepting account/branch/status changes.
[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public class EmployeeProfileRequest
{
    [Required, MaxLength(256)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string Phone { get; set; } = string.Empty;
    [MaxLength(256)] public string? JobTitle { get; set; }
    [EnumDataType(typeof(DAL.Enums.EmploymentType))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<DAL.Enums.EmploymentType>))]
    public DAL.Enums.EmploymentType EmploymentType { get; set; }
    public DateOnly HireDate { get; set; }
}

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateEmployeeRequest : EmployeeProfileRequest
{
    public Guid? InitialBranchId { get; set; }
}

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed class ChangeEmployeeStatusRequest
{
    [Required, EnumDataType(typeof(DAL.Enums.EmploymentStatus))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<DAL.Enums.EmploymentStatus>))]
    public DAL.Enums.EmploymentStatus? Status { get; set; }
    [MaxLength(2048)] public string? Reason { get; set; }
}

```

## CampionsSystem.BLL/Members/MemberContracts.cs

```csharp

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CampionsSystem.BLL.Authentication;

namespace CampionsSystem.BLL.Members;

public sealed class MemberListRequest
{
    [MaxLength(256)] public string? Search { get; set; }
    public Guid? RegistrationBranchId { get; set; }
    public long? ClientCode { get; set; }
    public bool? IsActive { get; set; }
    public Guid? SalesEmployeeId { get; set; }
    public bool? HouseOnly { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}
public sealed record SalesAssignmentDetails(Guid Id, Guid SalesEmployeeId, string SalesEmployeeName,
    DateTimeOffset AssignedAt, DateTimeOffset? EndedAt, string? Reason);
public sealed record SalesAssignmentHistoryItem(Guid Id, Guid? SalesEmployeeId, string? SalesEmployeeName,
    string AssignmentType, DateTimeOffset AssignedAt, DateTimeOffset? EndedAt, string? Reason);
public sealed record MemberSummary(Guid Id, long? ClientCode, string MemberCode, string FullName,
    string PrimaryPhone, bool IsActive, BranchContext RegistrationBranch, SalesAssignmentDetails? CurrentSalesOwner)
{
    public bool IsHouse => CurrentSalesOwner is null;
}
public sealed record EligibleFreelancePlan(Guid PlanId, string PlanName, string CoachName);
public sealed record MemberEmployeeLink(Guid Id, string FullName);
public sealed record MemberDetails(Guid Id, long? ClientCode, string MemberCode, string FullName,
    string PrimaryPhone, string? PrimaryPhoneNormalized, string? SecondaryPhone, bool IsPrimaryPhoneVerified,
    DateOnly? DateOfBirth, string? Gender, string? Notes, bool IsActive, BranchContext RegistrationBranch,
    DateTimeOffset CreatedAt, SalesAssignmentDetails? CurrentSalesOwner, MemberEmployeeLink? Employee,
    IReadOnlyList<EligibleFreelancePlan> EligibleFreelancePlans)
{
    public bool IsHouse => CurrentSalesOwner is null;
}
public sealed record MemberListResult(IReadOnlyList<MemberSummary> Items, int TotalCount, int Page, int PageSize);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateMemberRequest
{
    [Required, MaxLength(256)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string PrimaryPhone { get; set; } = string.Empty;
    [MaxLength(64)] public string? SecondaryPhone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    [MaxLength(32)] public string? Gender { get; set; }
    [MaxLength(4096)] public string? Notes { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter<MemberCreationType>))]
public enum MemberCreationType { NormalSales, FreelanceHouse, ManagementCreated }

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateMemberRequest : UpdateMemberRequest
{
    public Guid TargetRegistrationBranchId { get; set; }
    [Required] public MemberCreationType? CreationType { get; set; }
    // ManagementCreated only, requires Members.AssignSales. NormalSales resolves the actor.
    public Guid? SalesEmployeeId { get; set; }
    // FreelanceHouse only; eligibility requires FreelancePlans.ManageEligibleMembers.
    public Guid? FreelancePlanId { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ChangeMemberStatusRequest
{
    [Required] public bool? IsActive { get; set; }
    [MaxLength(2048)] public string? Reason { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SalesAssignmentRequest
{
    public Guid SalesEmployeeId { get; set; }
    [MaxLength(2048)] public string? Reason { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ReturnToHouseRequest
{
    [MaxLength(2048)] public string? Reason { get; set; }
}

```

## CampionsSystem.BLL/Operations/ActivityManagementService.cs

```csharp

using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Operations;

public sealed record CreateActivityCoachRequest(ActivityCoachType CoachType, Guid? EmployeeId,
    string? ExternalName, string? ExternalPhone, decimal Percentage);

```

## CampionsSystem.BLL/Operations/ClassSessionService.cs

```csharp

using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Operations;

public sealed record SaveClassSessionRequest(Guid GroupClassId, Guid? ClassScheduleId, Guid CoachEmployeeId,
    DateTimeOffset StartedAt, DateTimeOffset? EndedAt, ClassSessionStatus Status);

```

## CampionsSystem.BLL/Operations/CoachReportService.cs

```csharp

using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Operations;

public sealed record ActivityCoachReport(Guid CoachId, string Coach, Guid ActivityId, string Activity,
    ActivityCoachType CoachType, Guid? EmployeeId, int SubscribersCount, decimal EligibleRevenue,
    decimal Percentage, decimal Entitlement);
public sealed record ClassCoachReport(Guid CoachEmployeeId, string Coach, int CompletedClassesCount,
    int TotalAttendances, decimal AverageAttendance);
public sealed record FreelancePlanReport(Guid MembershipPlanId, string Plan, Guid CoachEmployeeId, string Coach,
    int EligibleMemberCount, int PurchasedMemberCount, decimal ActualGymRevenue, string SalesAttribution);

```

## CampionsSystem.BLL/Operations/FreelancePlanService.cs

```csharp

using CampionsSystem.BLL.Plans;
using CampionsSystem.DAL.Enums;

namespace CampionsSystem.BLL.Operations;

public sealed record SaveFreelancePlanRequest(Guid CityId, Guid CoachEmployeeId, string Name, decimal Price,
    int DurationDays, AccessMode AccessMode, int? SessionCount, bool IsActive = true, Guid? BranchId = null);

```

## CampionsSystem.BLL/Operations/SubscriptionPurchaseService.cs

```csharp

using System.Data;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Operations;

public sealed record ActivityPurchaseRequest(Guid MemberId, Guid ActivityPackageId, Guid ActivityCoachId,
    Guid? ExistingSubscriptionId = null);
public sealed record FreelancePurchaseRequest(Guid MemberId, Guid MembershipPlanId, Guid BranchId,
    Guid? ExistingSubscriptionId = null);
public sealed record PurchaseResult(Guid SubscriptionId, Guid TransactionId, Guid SaleTransactionId, decimal GymAmountDue);

// Creates the entitlement/purchase history and amount due. Receiving funds remains the existing payment model's concern.

```

## CampionsSystem.BLL/Plans/PlanContracts.cs

```csharp

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CampionsSystem.DAL.Enums;
namespace CampionsSystem.BLL.Plans;

public class CatalogListRequest
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public sealed class MembershipPlanListRequest : CatalogListRequest
{
    public Guid? BranchId { get; set; }
    public Guid? CityId { get; set; }
    public PlanCategory? PlanType { get; set; }
    public Guid? AudienceCategoryId { get; set; }
    public AccessMode? Mode { get; set; }
    public Guid? FreelanceCoachEmployeeId { get; set; }
    // Employee products require explicit administrative opt-in and MembershipPlans.Manage.
    public bool IncludeInternal { get; set; }
}
public sealed record PlanReference(Guid Id, string Name);
public sealed record CatalogDetails(Guid Id, string? Code, string Name, string? Description, bool IsActive, DateTimeOffset CreatedAt);
public sealed record CatalogListResult(IReadOnlyList<CatalogDetails> Items, int TotalCount, int Page, int PageSize);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveCategoryRequest
{
    [Required,MaxLength(256)] public string Name { get; set; } = "";
    [MaxLength(4096)] public string? Description { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveBenefitRequest
{
    [Required,MaxLength(128)] public string Code { get; set; } = "";
    [Required,MaxLength(256)] public string Name { get; set; } = "";
    [MaxLength(4096)] public string? Description { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ChangePlanStatusRequest
{
    [Required] public bool? IsActive { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateMembershipPlanRequest
{
    public Guid BranchId { get; set; }
    public Guid CityId { get; set; }
    [Required,MaxLength(256)] public string Name { get; set; } = "";
    [Required,JsonConverter(typeof(JsonStringEnumConverter))] public PlanCategory? PlanType { get; set; }
    public Guid? AudienceCategoryId { get; set; }
    [Required,JsonConverter(typeof(JsonStringEnumConverter))] public AccessMode? Mode { get; set; }
    public int DurationDays { get; set; }
    public int? Sessions { get; set; }
    public decimal Price { get; set; }
    public bool AllowFreeze { get; set; }
    public int FreezeDays { get; set; }
    public Guid? FreelanceCoachEmployeeId { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateMembershipPlanRequest : UpdateMembershipPlanRequest;
public sealed record MembershipPlanSummary(Guid Id, string Name, string PlanType, PlanReference? Branch,
    PlanReference City, PlanReference? AudienceCategory, string Mode, decimal Price, int DurationDays,
    int? Sessions, bool AllowFreeze, int FreezeDays, bool IsActive, PlanReference? FreelanceCoach)
{
    public bool CustomerPurchasable => PlanType != nameof(PlanCategory.Employee) && Branch is not null && IsActive;
}
public sealed record PlanBenefitDetails(Guid Id, Guid BenefitId, string Code, string Name, bool IsActive, string GrantType, int? Quantity);
public sealed record MembershipPlanDetails(MembershipPlanSummary Plan, DateTimeOffset CreatedAt,
    IReadOnlyList<PlanBenefitDetails> Benefits, int EligibleMemberCount);
public sealed record MembershipPlanListResult(IReadOnlyList<MembershipPlanSummary> Items, int TotalCount, int Page, int PageSize);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlanBenefitRequest
{
    public Guid BenefitId { get; set; }
    [Required,JsonConverter(typeof(JsonStringEnumConverter))] public MembershipBenefitGrantType? GrantType { get; set; }
    public int? Quantity { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SetPlanBenefitsRequest
{
    public List<PlanBenefitRequest> Benefits { get; set; } = [];
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EligibleMemberRequest
{
    public Guid MemberId { get; set; }
}
public sealed record EligibleMemberDetails(Guid MemberId, string FullName, string PrimaryPhone, Guid RegistrationBranchId,
    long? ClientCode, string MemberCode, DateTimeOffset? AddedAt);
public sealed record EligibleMemberListResult(IReadOnlyList<EligibleMemberDetails> Items, int TotalCount, int Page, int PageSize);
public sealed class EligibleMemberCandidateRequest
{
    public Guid? MemberId { get; set; }
    [MaxLength(64)] public string? Phone { get; set; }
    [MaxLength(256)] public string? MemberCode { get; set; }
}

```

## CampionsSystem.BLL/Sales/SalesContracts.cs

```csharp

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
    string Reason);
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
    IReadOnlyList<PaymentDetails> Payments, IReadOnlyList<RefundDetails> Refunds, IReadOnlyList<InstallmentDetails> Installments,
    Guid? OfferId = null, string? OfferType = null, decimal? OfferValue = null, string? OfferEligibilityMode = null,
    Guid? OfferAudienceCategoryId = null, string RetainedRevenueClassification = "CancellationFee",
    bool RetainedCommissionEligible = false, bool RetainedTargetEligible = false);
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
public sealed record PaymentHistory(IReadOnlyList<PaymentDetails> Items,int TotalCount,int Page,int PageSize);
public sealed record RefundHistory(IReadOnlyList<RefundDetails> Items,int TotalCount,int Page,int PageSize);
public sealed record OfferHistory(IReadOnlyList<OfferDetails> Items,int TotalCount,int Page,int PageSize);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateFinancialAccountRequest(string Name,FinancialAccountType Method,Guid? BranchId=null);
public sealed record FinancialAccountDetails(Guid Id,Guid BranchId,string Name,string Method,bool IsActive);

```

## CampionsSystem.BLL/Shifts/ShiftContracts.cs

```csharp

using CampionsSystem.DAL.Enums;
using System.Text.Json.Serialization;
namespace CampionsSystem.BLL.Shifts;

public static class ShiftPermissions
{
    public const string View = "Shifts.View", Manage = "Shifts.Manage", VaultView = "Vault.View", VaultManage = "Vault.Manage", Withdraw = "Vault.Withdraw",
        ExpenseView = "Expenses.View", ExpenseCreate = "Expenses.Create", ExpenseManage = "Expenses.Manage", ExpenseReverse = "Expenses.Reverse",
        AdvanceView = "Advances.View", AdvanceCreate = "Advances.Create";
    public static readonly string[] Codes = [View, Manage, VaultView, VaultManage, Withdraw, ExpenseView, ExpenseCreate, ExpenseManage, ExpenseReverse, AdvanceView, AdvanceCreate];
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OpenShiftRequest(Guid OperationId, decimal OpeningCash, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CloseShiftRequest(Guid OperationId, long ExpectedVersion, decimal CountedCash, string? Reason = null);
public sealed record ShiftDetails(Guid Id, Guid BranchId, Guid? EmployeeId, string? EmployeeName, ShiftStatus Status, long Version,
    DateTimeOffset OpenedAt, DateTimeOffset? ClosedAt, DateOnly? OpeningBusinessDate, DateOnly? ClosingBusinessDate,
    decimal OpeningCash, decimal? ExpectedClosingCash, decimal? ActualClosingCash, decimal? Difference);
public sealed record DrawerPosition(Guid BranchId, Guid AccountId, decimal BookCash, decimal ExpectedPhysicalCash, decimal LastCountedCash,
    long MovementSequence, Guid? LastCountId, decimal ObservedDiscrepancies);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SetupDrawerRequest(Guid OperationId, Guid FinancialAccountId, decimal CountedCash, string Reason, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CountCashRequest(Guid OperationId, decimal CountedCash, string Reason, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record WithdrawalRequest(Guid OperationId, decimal Amount, string Reason, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OpenBusinessDayRequest(Guid OperationId, DateOnly BusinessDate, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PaidExpenseRequest(Guid OperationId, Guid ExpenseCategoryId, Guid FinancialAccountId, decimal Amount, string Description, string? ReceiptReference = null, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ReverseExpenseRequest(Guid OperationId, decimal ReturnedAmount, bool FundsActuallyReturned, string Reason, string? ReturnReference = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AdvanceRequest(Guid OperationId, Guid ReceivingEmployeeId, Guid FinancialAccountId, decimal Amount, string Reason, string? Reference = null, Guid? BranchId = null);
public sealed record ShiftHistoryRequest(Guid? BranchId = null, Guid? EmployeeId = null, DateOnly? From = null, DateOnly? To = null, int Page = 1, int PageSize = 25);
public sealed record ShiftPage<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record CashCountDetails(Guid Id, Guid BranchId, Guid? ShiftId, string Kind, decimal ExpectedCash, decimal CountedCash, decimal BookCash,
    decimal Difference, DateTimeOffset OccurredAt, DateOnly BusinessDate, Guid ActorId, string? Reason, long ObservationOrder);
public sealed record CashMovementDetails(Guid Id, Guid BranchId, Guid? ShiftId, string Kind, decimal Amount, DateTimeOffset OccurredAt, DateOnly BusinessDate, string Reason);
public sealed record VaultHistoryRequest(Guid? BranchId = null, Guid? AccountId = null, DateOnly? From = null, DateOnly? To = null, int Page = 1, int PageSize = 25);
public sealed record FinancialMovementDetails(Guid Id, Guid AccountId, Guid BranchId, Guid? ShiftId, FinancialSourceType SourceType,
    FinancialTransactionType Direction, decimal Amount, DateTimeOffset OccurredAt, DateOnly? BusinessDate, Guid ActorId);
public sealed record ExpenseDetails(Guid Id, Guid BranchId, Guid? ShiftId, Guid? RecordingEmployeeId, Guid CategoryId, string CategoryName,
    Guid AccountId, FinancialAccountType Method, decimal Amount, decimal ReturnedAmount, string Description, string? ReceiptReference,
    DateTimeOffset OccurredAt, DateOnly? BusinessDate, DateTimeOffset? PaidAt, ExpenseStatus OriginalStatus);
public sealed record ExpenseReversalDetails(Guid Id, Guid ExpenseId, Guid BranchId, Guid ShiftId, decimal Amount, string Reason,
    string? ReturnReference, DateTimeOffset OccurredAt, DateOnly BusinessDate);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ExpenseCategoryRequest(string Name, bool IsActive = true);
public sealed record ExpenseCategoryDetails(Guid Id, string Name, bool IsActive);
public sealed record AdvanceDetails(Guid Id, Guid BranchId, Guid ShiftId, Guid ReceivingEmployeeId, string ReceivingEmployeeName,
    Guid RecordingEmployeeId, Guid AccountId, FinancialAccountType Method, decimal Amount, string Reason, string? Reference,
    DateTimeOffset OccurredAt, DateOnly BusinessDate);
public sealed record BusinessDayDetails(Guid Id, Guid BranchId, DateOnly BusinessDate, BusinessDayStatus Status,
    DateTimeOffset OpenedAt, DateTimeOffset? ClosedAt, Guid? OpenedByUserId, Guid? ClosedByUserId);
public sealed record MethodFinancialTotals(FinancialAccountType Method, decimal Collected, decimal Refunded, decimal ExpensesPaid,
    decimal ExpenseReturns, decimal AdvancesPaid, decimal Withdrawals, decimal InitialPosition, decimal OtherCredits,
    decimal OtherDebits, decimal NetMovement, decimal LegacyUnshiftedNetMovement);
public sealed record ShiftFinancialSummary(ShiftDetails Shift, IReadOnlyList<MethodFinancialTotals> Activity,
    DrawerPosition CurrentSharedDrawer);
public sealed record BranchFinancialSummary(Guid BranchId, DateOnly? From, DateOnly? To,
    IReadOnlyList<MethodFinancialTotals> Activity, DrawerPosition CurrentSharedDrawer);
public sealed record FinancialAuditDetails(Guid Id, string Event, string? EntityType, Guid? EntityId,
    Guid? ActorId, DateTimeOffset OccurredAt, string? Before, string? After);

```

## CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs

```csharp

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
}
public sealed record SubscriptionSummary(Guid Id, Guid MemberId, Guid SourcePlanId, string? PlanName,
    string? PlanType, Guid OriginBranchId, Guid AccessCityId, string Mode, DateOnly StartDate,
    DateOnly? EndDate, string Status, int? TotalSessions, int? RemainingSessions, int Version,
    Guid? EmployeeId, Guid? FollowsSubscriptionId, bool IsStandaloneVisit = false);
public sealed record SubscriptionHistory(IReadOnlyList<SubscriptionSummary> Items, int TotalCount, int Page, int PageSize);
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

```
