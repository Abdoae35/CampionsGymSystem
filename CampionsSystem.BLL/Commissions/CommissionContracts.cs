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
    decimal? Target, decimal? BelowRate, decimal? ReachedRate, int Revision, string? EmployeeName = null);
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
    decimal GrossCollection, decimal Refunds, decimal RetainedCollection, decimal EligibleCollection,
    string? MemberName = null, string? MemberCode = null,
    string? OriginalSalesEmployeeName = null, string? EmployeeName = null);
public sealed record CommissionPage<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record CommissionRefundSource(Guid RefundId, Guid PaymentId, decimal Amount, DateOnly? CollectionBusinessDate,
    DateOnly? RefundBusinessDate, DateTimeOffset RefundedAt);
