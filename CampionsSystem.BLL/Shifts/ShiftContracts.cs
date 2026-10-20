using CampionsSystem.DAL.Enums;
using System.Text.Json.Serialization;
namespace CampionsSystem.BLL.Shifts;

public static class ShiftPermissions
{
    public const string View = "Shifts.View", Manage = "Shifts.Manage", CloseOthers = "Shifts.CloseOthers", VaultView = "Vault.View", VaultManage = "Vault.Manage", Withdraw = "Vault.Withdraw",
        ExpenseView = "Expenses.View", ExpenseCreate = "Expenses.Create", ExpenseManage = "Expenses.Manage", ExpenseReverse = "Expenses.Reverse",
        AdvanceView = "Advances.View", AdvanceCreate = "Advances.Create";
    public static readonly string[] Codes = [View, Manage, CloseOthers, VaultView, VaultManage, Withdraw, ExpenseView, ExpenseCreate, ExpenseManage, ExpenseReverse, AdvanceView, AdvanceCreate];
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OpenShiftRequest(Guid OperationId, decimal OpeningCash, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CloseShiftRequest(Guid OperationId, long ExpectedVersion, [property: JsonRequired] decimal CountedCash, string? Reason = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OverrideCloseShiftRequest(Guid OperationId, long ExpectedVersion, string Reason, decimal? CountedCash = null);
public sealed record ShiftAccountBalance(Guid AccountId, FinancialAccountType Method, decimal BookBalance);
public sealed record ShiftDetails(Guid Id, Guid BranchId, Guid? EmployeeId, string? EmployeeName, ShiftStatus Status, long Version,
    DateTimeOffset OpenedAt, DateTimeOffset? ClosedAt, DateOnly? OpeningBusinessDate, DateOnly? ClosingBusinessDate,
    decimal OpeningCash, decimal? ExpectedClosingCash, decimal? ActualClosingCash, decimal? Difference,
    Guid? ClosedByUserId = null, string? ClosingReason = null, string? ClosureType = null, string? VerificationStatus = null,
    IReadOnlyList<ShiftAccountBalance>? ClosingAccountBalances = null,
    decimal? ClosingCashBookBalance = null, decimal? ClosingInstaPayBookBalance = null,
    string? BranchName = null, string? ClosedByName = null);
public sealed record DrawerPosition(Guid BranchId, Guid AccountId, decimal BookCash, decimal ExpectedPhysicalCash, decimal LastCountedCash,
    long MovementSequence, Guid? LastCountId, decimal ObservedDiscrepancies, string? AccountName = null);
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
    decimal Difference, DateTimeOffset OccurredAt, DateOnly BusinessDate, Guid ActorId, string? Reason, long ObservationOrder,
    string? ActorName = null);
public sealed record CashMovementDetails(Guid Id, Guid BranchId, Guid? ShiftId, string Kind, decimal Amount, DateTimeOffset OccurredAt, DateOnly BusinessDate, string Reason);
public sealed record VaultHistoryRequest(Guid? BranchId = null, Guid? AccountId = null, DateOnly? From = null, DateOnly? To = null, int Page = 1, int PageSize = 25);
public sealed record FinancialMovementDetails(Guid Id, Guid AccountId, Guid BranchId, Guid? ShiftId, FinancialSourceType SourceType,
    FinancialTransactionType Direction, decimal Amount, DateTimeOffset OccurredAt, DateOnly? BusinessDate, Guid ActorId,
    string? ActorName = null, string? AccountName = null);
public sealed record ShiftFinancialTransactionDetails(Guid Id, Guid ShiftId, Guid AccountId, FinancialAccountType Method,
    FinancialSourceType SourceType, FinancialTransactionType Direction, decimal Amount, DateTimeOffset OccurredAt,
    DateOnly? BusinessDate, Guid ActorId, Guid? PaymentId, Guid? RefundId, Guid? ExpenseId,
    Guid? EmployeeAdvanceId, Guid? CashMovementId, Guid? PayrollId, string ActorName);
public sealed record ShiftBranchTransactionDetails(Guid Id, Guid? ShiftId, Guid AccountId, FinancialAccountType Method,
    FinancialSourceType SourceType, FinancialTransactionType Direction, decimal Amount, DateTimeOffset OccurredAt,
    DateTimeOffset? PostedAt, DateOnly? BusinessDate, Guid ActorId, string ActorName,
    Guid? PaymentId, Guid? RefundId, Guid? ExpenseId, Guid? EmployeeAdvanceId, Guid? CashMovementId, Guid? PayrollId);
public sealed record ShiftBranchAccountReconciliation(Guid AccountId, FinancialAccountType Method,
    decimal OpeningBookBalance, decimal Inflows, decimal Outflows, decimal CalculatedClosingBookBalance,
    decimal? ExpectedClosingBookBalance, bool? MatchesExpectedClosingBalance);
public sealed record ShiftBranchOperatorTotals(Guid ActorId, string ActorName, FinancialAccountType Method,
    int TransactionCount, decimal Inflows, decimal Outflows, decimal NetMovement);
public sealed record ShiftBranchPeriodSummary(DateTimeOffset From, DateTimeOffset CutoffAt, bool IsHistorical,
    int TransactionCount, IReadOnlyList<MethodFinancialTotals> Activity,
    IReadOnlyList<ShiftBranchOperatorTotals> Operators, IReadOnlyList<ShiftBranchAccountReconciliation> Accounts,
    decimal? ExpectedPhysicalCashAtCutoff, decimal? CountedCashAtClosure, IReadOnlyList<string> Limitations);
public sealed record ExpenseDetails(Guid Id, Guid BranchId, Guid? ShiftId, Guid? RecordingEmployeeId, Guid CategoryId, string CategoryName,
    Guid AccountId, FinancialAccountType Method, decimal Amount, decimal ReturnedAmount, string Description, string? ReceiptReference,
    DateTimeOffset OccurredAt, DateOnly? BusinessDate, DateTimeOffset? PaidAt, ExpenseStatus OriginalStatus,
    Guid? RecordedByUserId = null, string? RecordingEmployeeName = null,
    string? RecordedByName = null, string? AccountName = null);
public sealed record ExpenseReversalDetails(Guid Id, Guid ExpenseId, Guid BranchId, Guid ShiftId, decimal Amount, string Reason,
    string? ReturnReference, DateTimeOffset OccurredAt, DateOnly BusinessDate);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ExpenseCategoryRequest(string Name, bool IsActive = true);
public sealed record ExpenseCategoryDetails(Guid Id, string Name, bool IsActive);
public sealed record AdvanceDetails(Guid Id, Guid BranchId, Guid ShiftId, Guid ReceivingEmployeeId, string ReceivingEmployeeName,
    Guid RecordingEmployeeId, Guid AccountId, FinancialAccountType Method, decimal Amount, string Reason, string? Reference,
    DateTimeOffset OccurredAt, DateOnly BusinessDate, string? RecordingEmployeeName = null,
    string? AccountName = null);
public sealed record BusinessDayDetails(Guid Id, Guid BranchId, DateOnly BusinessDate, BusinessDayStatus Status,
    DateTimeOffset OpenedAt, DateTimeOffset? ClosedAt, Guid? OpenedByUserId, Guid? ClosedByUserId,
    string? BranchName = null, string? OpenedByName = null, string? ClosedByName = null);
public sealed record MethodFinancialTotals(FinancialAccountType Method, decimal Collected, decimal Refunded, decimal ExpensesPaid,
    decimal ExpenseReturns, decimal AdvancesPaid, decimal Withdrawals, decimal InitialPosition, decimal OtherCredits,
    decimal OtherDebits, decimal NetMovement, decimal LegacyUnshiftedNetMovement);
public sealed record ShiftFinancialSummary(ShiftDetails Shift, IReadOnlyList<MethodFinancialTotals> Activity,
    DrawerPosition CurrentSharedDrawer, IReadOnlyList<ShiftAccountBalance> CurrentSharedAccountBalances,
    ShiftBranchPeriodSummary? BranchPeriod = null);
public sealed record BranchFinancialSummary(Guid BranchId, DateOnly? From, DateOnly? To,
    IReadOnlyList<MethodFinancialTotals> Activity, DrawerPosition CurrentSharedDrawer);
public sealed record FinancialAuditDetails(Guid Id, string Event, string? EntityType, Guid? EntityId,
    Guid? ActorId, DateTimeOffset OccurredAt, string? Before, string? After, string? ActorName = null);
