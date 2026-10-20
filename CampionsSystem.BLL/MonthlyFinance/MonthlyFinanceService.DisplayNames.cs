using CampionsSystem.BLL.Authentication;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.MonthlyFinance;

public sealed partial class MonthlyFinanceService
{
    // Financial closing JSON and workbooks remain immutable. Enrich only the returned
    // read contract so archived revisions show current names without storing labels.
    private async Task<MonthlyReport> WithCurrentNamesAsync(MonthlyReport report,CancellationToken ct)
    {
        var actorIds=report.Transactions.Select(x=>(Guid?)x.ActorUserId)
            .Concat(report.EmployeePayments.Select(x=>(Guid?)x.ActorUserId))
            .Concat(report.CashCounts.Select(x=>(Guid?)x.ActorId))
            .Concat(report.Shifts.Select(x=>x.ClosedByUserId)).Distinct().ToArray();
        var actors=await ActorDisplayNames.LoadAsync(db,actorIds,ct);
        var employeeIds=report.Advances.Select(x=>x.RecordingEmployeeId)
            .Concat(report.Advances.Select(x=>x.ReceivingEmployeeId))
            .Concat(report.EmployeePayments.Select(x=>x.EmployeeId))
            .Concat(report.Payroll.Select(x=>x.EmployeeId))
            .Concat(report.Salaries.Select(x=>x.EmployeeId))
            .Concat(report.Coaches.Select(x=>x.CoachEmployeeId))
            .Concat(report.SalesPerformance.SelectMany(x=>x.Employees).Select(x=>x.EmployeeId))
            .Concat(report.Shifts.Where(x=>x.EmployeeId.HasValue).Select(x=>x.EmployeeId!.Value))
            .Concat(report.Transactions.Where(x=>x.PerformingEmployeeId.HasValue).Select(x=>x.PerformingEmployeeId!.Value))
            .Concat(report.Transactions.Where(x=>x.CommercialSalesOwnerId.HasValue).Select(x=>x.CommercialSalesOwnerId!.Value))
            .Concat(report.SalesSources.Where(x=>x.EmployeeId.HasValue).Select(x=>x.EmployeeId!.Value))
            .Concat(report.SalesSources.Where(x=>x.OriginalSalesEmployeeId.HasValue).Select(x=>x.OriginalSalesEmployeeId!.Value))
            .Distinct().ToArray();
        var employees=await db.Employees.AsNoTracking().Where(x=>employeeIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.FullName}).ToDictionaryAsync(x=>x.Id,x=>x.FullName,ct);
        var memberIds=report.SalesSources.Select(x=>x.MemberId)
            .Concat(report.Transactions.Where(x=>x.CustomerId.HasValue).Select(x=>x.CustomerId!.Value)).Distinct().ToArray();
        var members=await db.Members.AsNoTracking().Where(x=>memberIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.FullName,x.MemberCode}).ToDictionaryAsync(x=>x.Id,ct);
        var accountIds=report.Advances.Select(x=>x.AccountId).Append(report.Drawer.AccountId)
            .Concat(report.Transactions.Select(x=>x.AccountId)).Distinct().ToArray();
        var accounts=await db.FinancialAccounts.AsNoTracking().Where(x=>accountIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.Name}).ToDictionaryAsync(x=>x.Id,x=>x.Name,ct);
        var branchName=await db.GymBranches.AsNoTracking().Where(x=>x.Id==report.Month.BranchId)
            .Select(x=>x.Name).SingleOrDefaultAsync(ct);
        return report with
        {
            BranchName=branchName??report.BranchName,
            Transactions=report.Transactions.Select(x=>x with
            {
                ActorName=ActorDisplayNames.Get(actors,x.ActorUserId),
                AccountName=accounts.GetValueOrDefault(x.AccountId)??x.AccountName,
                PerformingEmployeeName=x.PerformingEmployeeId.HasValue?employees.GetValueOrDefault(x.PerformingEmployeeId.Value):null,
                CommercialSalesOwnerName=x.CommercialSalesOwnerId.HasValue?employees.GetValueOrDefault(x.CommercialSalesOwnerId.Value):null,
                CustomerName=x.CustomerId.HasValue?members.GetValueOrDefault(x.CustomerId.Value)?.FullName:null
            }).ToArray(),
            EmployeePayments=report.EmployeePayments.Select(x=>x with
            {
                ActorName=ActorDisplayNames.Get(actors,x.ActorUserId),
                EmployeeName=employees.GetValueOrDefault(x.EmployeeId)
            }).ToArray(),
            Payroll=report.Payroll.Select(x=>x with{EmployeeName=employees.GetValueOrDefault(x.EmployeeId)??x.EmployeeName}).ToArray(),
            Salaries=report.Salaries.Select(x=>x with{EmployeeName=employees.GetValueOrDefault(x.EmployeeId)??x.EmployeeName}).ToArray(),
            Coaches=report.Coaches.Select(x=>x with{CoachName=employees.GetValueOrDefault(x.CoachEmployeeId)??x.CoachName}).ToArray(),
            SalesPerformance=report.SalesPerformance.Select(x=>x with
            {
                Employees=x.Employees.Select(y=>y with{EmployeeName=employees.GetValueOrDefault(y.EmployeeId)??y.EmployeeName}).ToArray()
            }).ToArray(),
            Advances=report.Advances.Select(x=>x with
            {
                ReceivingEmployeeName=employees.GetValueOrDefault(x.ReceivingEmployeeId)??x.ReceivingEmployeeName,
                RecordingEmployeeName=employees.GetValueOrDefault(x.RecordingEmployeeId),
                AccountName=accounts.GetValueOrDefault(x.AccountId)
            }).ToArray(),
            CashCounts=report.CashCounts.Select(x=>x with{ActorName=ActorDisplayNames.Get(actors,x.ActorId)}).ToArray(),
            Shifts=report.Shifts.Select(x=>x with
            {
                BranchName=branchName,
                EmployeeName=x.EmployeeId.HasValue?employees.GetValueOrDefault(x.EmployeeId.Value)??x.EmployeeName:x.EmployeeName,
                ClosedByName=ActorDisplayNames.Get(actors,x.ClosedByUserId)
            }).ToArray(),
            Drawer=report.Drawer with{AccountName=accounts.GetValueOrDefault(report.Drawer.AccountId)},
            SalesSources=report.SalesSources.Select(x=>x with
            {
                MemberName=members.GetValueOrDefault(x.MemberId)?.FullName,
                MemberCode=members.GetValueOrDefault(x.MemberId)?.MemberCode,
                OriginalSalesEmployeeName=x.OriginalSalesEmployeeId.HasValue?employees.GetValueOrDefault(x.OriginalSalesEmployeeId.Value):null,
                EmployeeName=x.EmployeeId.HasValue?employees.GetValueOrDefault(x.EmployeeId.Value):null
            }).ToArray()
        };
    }
}
