using System.Text.Json.Serialization;
using CampionsSystem.BLL.Commissions;
using CampionsSystem.BLL.Shifts;
namespace CampionsSystem.BLL.MonthlyFinance;

public static class MonthlyFinancePermissions
{
    public const string View = "FinancialMonths.View", Manage = "FinancialMonths.Manage", Close = "FinancialMonths.Close",
        Reopen = "FinancialMonths.Reopen", Export = "FinancialMonths.Export", Consolidated = "FinancialMonths.ViewConsolidated",
        SalaryView = "Salaries.View", SalaryManage = "Salaries.Manage", PayrollView = "Payroll.View", PayrollPay = "Payroll.RecordPayment",
        MaintenanceView = "Maintenance.View", MaintenanceManage = "Maintenance.Manage";
    public static readonly string[] Codes = [View, Manage, Close, Reopen, Export, Consolidated, SalaryView, SalaryManage, PayrollView, PayrollPay, MaintenanceView, MaintenanceManage];
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OpenFinancialMonthRequest(Guid OperationId, string Name, DateOnly StartDate, DateOnly EndDate, string Reason, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CloseFinancialMonthRequest(Guid OperationId, int ExpectedVersion, string Reason, decimal? CountedCash = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ReopenFinancialMonthRequest(Guid OperationId, int ExpectedVersion, string Reason);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SetSalaryRequest(Guid OperationId, decimal BasicSalary, DateOnly EffectiveFrom, string Reason);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RecognizePayrollCommissionsRequest(Guid OperationId, Guid CommissionPeriodId);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RecordPayrollPaymentRequest(Guid OperationId, decimal Amount, DateTimeOffset PaidAt, string Reason, string? Reference = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record MaintenancePlanRequest(Guid OperationId, Guid BranchId, string Name, decimal ExpectedCost, int FrequencyMonths,
    DateOnly FirstDueDate, string Reason, Guid? PlanId = null, int ExpectedRevision = 0, bool IsActive = true,
    string? Description = null, DateOnly? NextExpectedDueDate = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RecordOccurrenceRequest(Guid OperationId, DateTimeOffset OriginalOccurredAt, string Reason);
public sealed record FinancialMonthDetails(Guid Id, Guid BranchId, string Name, DateOnly StartDate, DateOnly EndDate, string Status, int Version, int LastRevision, DateTimeOffset? FirstClosedAt);
public sealed record MonthEventDetails(Guid Id, string Kind, Guid OperationId, Guid ActorUserId, string Reason, DateTimeOffset RecordedAt, int ResultVersion, int? Revision);
public sealed record RevisionDetails(Guid Id, int Revision, DateTimeOffset SnapshotAt, Guid ActorUserId, string Reason, string WorkbookSha256, int SchemaVersion);
public sealed record SalarySettingDetails(Guid Id, Guid EmployeeId, decimal BasicSalary, DateOnly EffectiveFrom, DateOnly? EffectiveTo, Guid ActorUserId, DateTimeOffset CreatedAt);
public sealed record SalaryDayEvidence(DateOnly Date, Guid AssignmentId, Guid SalarySettingId, decimal MonthlyRate, decimal UnroundedDailyAmount);
public sealed record SalaryEntitlement(Guid EmployeeId, string EmployeeName, Guid BranchId, Guid FinancialMonthId, int PeriodDays, int EligibleDays,
    decimal BasicSalary, decimal Advances, decimal PayableBasicSalary, IReadOnlyList<SalaryDayEvidence> Days);
public sealed record PayrollDetails(Guid Id, Guid EmployeeId, string EmployeeName, Guid BranchId, Guid? FinancialMonthId, Guid? CommissionPeriodId,
    Guid? CommissionSummaryId, decimal BasicSalary, decimal Commission, decimal AdvancesDeducted, decimal Payable, decimal Paid, decimal Outstanding, string Status);
public sealed record PayrollPaymentDetails(Guid Id, Guid PayrollId, Guid EmployeeId, Guid BranchId, Guid FinancialMonthId, decimal Amount,
    decimal BalanceBefore, decimal BalanceAfter, DateTimeOffset PaidAt, DateTimeOffset RecordedAt, DateOnly BusinessDate, Guid ActorUserId, string Reason, string? Reference);
public sealed record MaintenanceForecast(Guid Id, Guid PlanId, Guid BranchId, int Revision, string Name, decimal ExpectedCost, int FrequencyMonths,
    DateOnly FirstDueDate, DateOnly NextExpectedDate, decimal SuggestedMonthlyAmount, bool IsActive, string Status = "InformationalOnly",
    string? Description = null, string DueStatus = "Upcoming", int DaysUntilDue = 0);
public sealed record MaintenanceBranchSummary(Guid BranchId, DateOnly AsOf, int HorizonDays, decimal TotalSuggestedMonthlyAmount,
    int ActivePlanCount, int UpcomingCount, int DueTodayCount, int OverdueCount);
public sealed record FinancialTransactionReportRow(Guid LedgerId, Guid BranchId, string BranchName, Guid? FinancialMonthId, string PeriodName,
    DateTimeOffset Timestamp, DateTimeOffset? PostedAt, DateTimeOffset? OriginalOccurredAt, DateOnly? BusinessDate, Guid AccountId, string AccountName,
    string PaymentMethod, string SourceType, string Direction, decimal Amount, Guid ActorUserId, Guid? PerformingEmployeeId, string? PerformingEmployeeName,
    Guid? ShiftId, Guid? CustomerId, string? CustomerName, Guid? CommercialSalesOwnerId, string? CommercialSalesOwnerName, Guid? SaleId,
    Guid? PaymentId, Guid? RefundId, Guid? ExpenseId, Guid? AdvanceId, Guid? WithdrawalId, Guid? CoachPayoutId, Guid? PayrollId,
    Guid? OperationId, Guid? ExpenseCategoryId, string? Category, string? Notes, string? Receipt, Guid? ReversalId, Guid? OriginalPaymentId, string SourceEvidence, string AuditEvidence,
    string? ProviderReference = null);
public sealed record ExpenseCategoryTotal(Guid CategoryId, string Category, decimal Paid, decimal Returned, decimal Net);
public sealed record CoachMonthRow(Guid CoachEmployeeId, string CoachName, decimal OpeningOutstanding, decimal ActualSessionEarnings,
    decimal ExpirationEarnings, decimal Earned, decimal Paid, decimal ClosingOutstanding, int CompletedClasses, int ClassAttendances,
    decimal? AverageAttendees, string ClassesCompensationStatus = "NotConfigured");
public sealed record CoachEarningReportRow(Guid Id, Guid CoachEmployeeId, Guid CommissionPeriodId, Guid? FinancialMonthId, Guid SaleId,
    Guid? SessionId, Guid? ExpirySettlementId, decimal EligibleValue, decimal Rate, decimal Amount, bool TransferredPackage, DateOnly BusinessDate, DateTimeOffset RecognizedAt, string AttributionEvidence);
public sealed record CoachPaymentReportRow(Guid Id, Guid CoachEmployeeId, Guid ExpenseId, decimal Amount, DateOnly BusinessDate, DateTimeOffset PaidAt);
public sealed record ClassOccurrenceReportRow(Guid Id, Guid CoachEmployeeId, string ClassType, DateOnly? BusinessDate, DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt, string Status, int Attendance);
public sealed record MonthlySummary(decimal GrossCollections, decimal Refunds, decimal NetCollections, decimal OrdinaryExpenses,
    decimal BasicSalaryEarned, decimal SalesCommissionRecognized, decimal PrivateCoachEarningsRecognized, decimal EmployeePaymentRecords,
    decimal CoachCashPayouts, decimal AdvanceCashPayouts, decimal LedgerCredits, decimal LedgerDebits, decimal LedgerNetCashflow,
    decimal NetOperatingResult, decimal EmployeeOpeningOutstanding, decimal EmployeeClosingOutstanding,
    decimal CoachOpeningOutstanding, decimal CoachClosingOutstanding, decimal BookCash, decimal ExpectedPhysicalCash, decimal LastCountedCash, decimal PhysicalVsBook, decimal PeriodObservedDiscrepancies, string OperatingResultBasis);
public sealed record MonthlyReport(FinancialMonthDetails Month, string BranchName, int Revision, DateTimeOffset SnapshotAt, MonthlySummary Summary,
    IReadOnlyList<FinancialTransactionReportRow> Transactions, IReadOnlyList<ExpenseCategoryTotal> ExpenseCategories,
    IReadOnlyList<CommissionProgress> SalesPerformance, IReadOnlyList<CommissionSourceLine> SalesSources,
    IReadOnlyList<CommissionRefundSource> SalesRefundAdjustments, IReadOnlyList<SalaryEntitlement> Salaries,
    IReadOnlyList<PayrollDetails> Payroll, IReadOnlyList<PayrollPaymentDetails> EmployeePayments, IReadOnlyList<AdvanceDetails> Advances,
    IReadOnlyList<CoachMonthRow> Coaches, IReadOnlyList<CoachEarningReportRow> CoachEarnings, IReadOnlyList<CoachPaymentReportRow> CoachPayments,
    IReadOnlyList<ClassOccurrenceReportRow> Classes, DrawerPosition Drawer, IReadOnlyList<CashCountDetails> CashCounts,
    IReadOnlyList<ShiftDetails> Shifts, IReadOnlyList<MaintenanceForecast> Maintenance, IReadOnlyList<string> Limitations);
public sealed record ConsolidatedMonthSummary(IReadOnlyList<Guid> Months, decimal NetCollections, decimal OrdinaryExpenses,
    decimal EarnedEmployeeCosts, decimal EarnedCoachCosts, decimal NetOperatingResult, decimal EmployeeOutstanding, decimal CoachOutstanding);
public sealed record WorkbookDownload(byte[] Bytes, string FileName, string Sha256);
