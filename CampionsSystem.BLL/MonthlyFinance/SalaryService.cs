using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.MonthlyFinance;

public sealed class SalaryService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private readonly FinancialOperations coordination = new(db, current, access);
    internal static SalarySettingDetails Details(EmployeeSalarySetting x) => new(x.Id, x.EmployeeId, x.BasicSalary, x.EffectiveFrom, x.EffectiveTo, x.CreatedByUserId, x.CreatedAt);
    public Task<SalarySettingDetails> ConfigureAsync(Guid employeeId, SetSalaryRequest r, CancellationToken ct = default) => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.BasicSalary); FinancialOperations.Reason(r.Reason);
        var actor = await access.RequireAsync(MonthlyFinancePermissions.SalaryManage, null, ct); await access.RequireAsync("Branches.AccessAll", null, ct);
        var employee = await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\"={employeeId} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Employee not found.");
        var hash = FinancialOperations.Hash(new { employeeId, r });
        var prior = await db.EmployeeSalarySettings.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
        if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return Details(prior); }
        if (r.EffectiveFrom == default || r.EffectiveFrom < employee.HireDate)
            throw new BusinessRuleException("Salary must start on or after the employment start date.");
        if (await db.Payrolls.AnyAsync(x => x.EmployeeId == employeeId && x.FinancialMonthId != null && x.CommissionSummaryId == null && x.FinancialMonth!.EndDate >= r.EffectiveFrom, ct))
            throw new BusinessRuleException("Salary changes cannot rewrite recognized basic salary; choose a later effective date.");
        var settings = await db.EmployeeSalarySettings.Where(x => x.EmployeeId == employeeId).OrderBy(x => x.EffectiveFrom).ToListAsync(ct);
        if (settings.Any(x => x.EffectiveFrom >= r.EffectiveFrom)) throw new BusinessRuleException("Append salary history chronologically; existing effective dates cannot be overwritten.");
        if (settings.Count != 0 && r.EffectiveFrom < access.Today) throw new BusinessRuleException("Changes to configured salary must be effective today or later.");
        var last = settings.LastOrDefault();
        if (last?.EffectiveTo == null && last != null) last.EffectiveTo = r.EffectiveFrom.AddDays(-1);
        var row = new EmployeeSalarySetting { EmployeeId = employeeId, BasicSalary = r.BasicSalary, EffectiveFrom = r.EffectiveFrom,
            CreatedByUserId = actor, CreatedAt = access.Now, OperationId = r.OperationId, RequestHash = hash };
        settings.Add(row);
        await SalaryProjection.ValidateHistoryChangeAsync(db, employee, ct, settings: settings);
        db.Add(row); access.Audit(actor, null, "Salary.Configured", nameof(EmployeeSalarySetting), row.Id, after: new { employeeId, r.BasicSalary, r.EffectiveFrom, r.Reason });
        return Details(row);
    }, ct);
    public async Task<IReadOnlyList<SalarySettingDetails>> HistoryAsync(Guid employeeId, Guid? branchId = null, CancellationToken ct = default)
    {
        var (branch, _) = await coordination.ResolveAsync(MonthlyFinancePermissions.SalaryView, branchId, ct);
        if (!await db.EmployeeBranchAssignments.AnyAsync(x => x.EmployeeId == employeeId && x.BranchId == branch, ct)) throw new UnauthorizedAccessException();
        return await db.EmployeeSalarySettings.AsNoTracking().Where(x => x.EmployeeId == employeeId).OrderBy(x => x.EffectiveFrom).ThenBy(x => x.Id)
            .Select(x => new SalarySettingDetails(x.Id, x.EmployeeId, x.BasicSalary, x.EffectiveFrom, x.EffectiveTo, x.CreatedByUserId, x.CreatedAt)).ToListAsync(ct);
    }
    public async Task<SalaryEntitlement> PreviewAsync(Guid monthId, Guid employeeId, CancellationToken ct = default)
    {
        var month = await db.FinancialMonths.AsNoTracking().SingleOrDefaultAsync(x => x.Id == monthId, ct) ?? throw new BranchNotFoundException("Financial month not found.");
        await access.RequireAsync(MonthlyFinancePermissions.SalaryView, month.BranchId, ct);
        var employee = await db.Employees.AsNoTracking().SingleOrDefaultAsync(x => x.Id == employeeId, ct) ?? throw new BranchNotFoundException("Employee not found.");
        if (!await db.EmployeeBranchAssignments.AnyAsync(x => x.EmployeeId == employeeId && x.BranchId == month.BranchId && x.StartDate <= month.EndDate && (x.EndDate == null || x.EndDate >= month.StartDate), ct))
            throw new UnauthorizedAccessException();
        var payroll = await db.Payrolls.AsNoTracking().SingleOrDefaultAsync(x => x.FinancialMonthId == monthId && x.EmployeeId == employeeId && x.CommissionSummaryId == null, ct);
        if (payroll != null) return System.Text.Json.JsonSerializer.Deserialize<SalaryEntitlement>(payroll.SalaryEvidenceJson!)!;
        if (db.Database.CurrentTransaction != null) return await SalaryProjection.ForEmployeeAsync(db, employee, month, ct, access: access);
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
        var result = await SalaryProjection.ForEmployeeAsync(db, employee, month, ct, access: access); await tx.CommitAsync(ct); return result;
    }
}
