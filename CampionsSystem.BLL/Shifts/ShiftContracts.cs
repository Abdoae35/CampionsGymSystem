using CampionsSystem.DAL.Enums;
namespace CampionsSystem.BLL.Shifts;

public static class ShiftPermissions
{
    public const string View = "Shifts.View", Manage = "Shifts.Manage", VaultView = "Vault.View", VaultManage = "Vault.Manage", Withdraw = "Vault.Withdraw",
        ExpenseView = "Expenses.View", ExpenseCreate = "Expenses.Create", ExpenseManage = "Expenses.Manage", ExpenseReverse = "Expenses.Reverse",
        AdvanceView = "Advances.View", AdvanceCreate = "Advances.Create";
    public static readonly string[] Codes = [View, Manage, VaultView, VaultManage, Withdraw, ExpenseView, ExpenseCreate, ExpenseManage, ExpenseReverse, AdvanceView, AdvanceCreate];
}
public sealed record OpenShiftRequest(Guid OperationId, decimal OpeningCash, Guid? BranchId = null);
public sealed record CloseShiftRequest(Guid OperationId, long ExpectedVersion, decimal CountedCash, string? Reason = null);
public sealed record ShiftDetails(Guid Id, Guid BranchId, Guid? EmployeeId, string? EmployeeName, ShiftStatus Status, long Version,
    DateTimeOffset OpenedAt, DateTimeOffset? ClosedAt, DateOnly? OpeningBusinessDate, DateOnly? ClosingBusinessDate,
    decimal OpeningCash, decimal? ExpectedClosingCash, decimal? ActualClosingCash, decimal? Difference);
public sealed record DrawerPosition(Guid BranchId, Guid AccountId, decimal BookCash, decimal ExpectedPhysicalCash, decimal LastCountedCash,
    long MovementSequence, Guid? LastCountId, decimal ObservedDiscrepancies);
public sealed record SetupDrawerRequest(Guid OperationId, Guid FinancialAccountId, decimal CountedCash, string Reason, Guid? BranchId = null);
public sealed record CountCashRequest(Guid OperationId, decimal CountedCash, string Reason, Guid? BranchId = null);
public sealed record WithdrawalRequest(Guid OperationId, decimal Amount, string Reason, Guid? BranchId = null);
public sealed record OpenBusinessDayRequest(Guid OperationId, DateOnly BusinessDate, Guid? BranchId = null);
public sealed record PaidExpenseRequest(Guid OperationId, Guid ExpenseCategoryId, Guid FinancialAccountId, decimal Amount, string Description, string? ReceiptReference = null, Guid? BranchId = null);
public sealed record ReverseExpenseRequest(Guid OperationId, decimal ReturnedAmount, bool FundsActuallyReturned, string Reason, string? ReturnReference = null);
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
public sealed record ExpenseCategoryRequest(string Name, bool IsActive = true);
public sealed record ExpenseCategoryDetails(Guid Id, string Name, bool IsActive);
