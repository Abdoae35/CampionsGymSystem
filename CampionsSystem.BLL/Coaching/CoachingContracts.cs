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
public sealed record PrivateAssignmentDetails(Guid Id, Guid CoachEmployeeId, DateTimeOffset AssignedAt, DateTimeOffset? EndedAt, string? Reason,
    string? CoachEmployeeName = null);
public sealed record PrivateAttendanceDetails(Guid Id, Guid SubscriptionId, Guid CoachEmployeeId, Guid GymCheckInId,
    DateOnly? BusinessDate, DateTimeOffset SessionAt, int? SessionNumber, decimal? SessionValue, bool? TransferredPackage,
    string? MemberName = null, string? MemberCode = null, string? CoachEmployeeName = null);
public sealed record PrivateDetails(Guid Id, Guid MemberId, Guid BranchId, DateOnly StartDate, DateOnly EndDate,
    int PurchasedSessions, int UsedSessions, int RemainingSessions, int Version, bool Cancelled,
    string? PackageName, decimal Price, decimal UnitSessionValue, decimal RemainderValue,
    Guid? OriginalCoachEmployeeId, IReadOnlyList<PrivateAssignmentDetails> Assignments,
    string? MemberName = null, string? MemberCode = null, string? OriginalCoachEmployeeName = null);
public sealed record PrivateExpiryDetails(Guid Id, Guid SubscriptionId, Guid CoachEmployeeId, int UnusedSessions,
    decimal EligibleValue, bool? TransferredPackage, DateOnly? BusinessDate, DateTimeOffset SettledAt, string? Reason,
    string? CoachEmployeeName = null);
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
