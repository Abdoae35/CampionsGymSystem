using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CampionsSystem.DAL.Enums;
namespace CampionsSystem.BLL.MonthlyFinance;

// One salary calculation shared by advance validation, closing and preview.
// Round the cumulative entitlement daily; assign each incremental cent to that day's
// branch. Equal-boundary branch reports therefore conserve the total exactly.
public sealed record SalaryEmploymentChange(DateOnly Date, DateTimeOffset RecordedAt, bool EmployedBefore, bool EmployedAfter);

public static class SalaryProjection
{
    public static SalaryEntitlement Calculate(Employee employee, FinancialMonth month, IReadOnlyList<EmployeeBranchAssignment> assignments,
        IReadOnlyList<EmployeeSalarySetting> settings, decimal advances, IReadOnlyList<SalaryEmploymentChange>? employmentChanges = null)
    {
        if (employee.EmploymentStatus == EmploymentStatus.Suspended && (employmentChanges == null || employmentChanges.Count == 0))
            throw new BusinessRuleException("Suspended employment lacks dated status history; explicit reconciliation is required.");
        var days = month.EndDate.DayNumber - month.StartDate.DayNumber + 1;
        var evidence = new List<SalaryDayEvidence>(); decimal exact = 0, rounded = 0, basic = 0;
        for (var date = month.StartDate; date <= month.EndDate; date = date.AddDays(1))
        {
            if (date < employee.HireDate || employee.TerminationDate <= date) continue;
            if (employmentChanges?.Count > 0)
            {
                var change = employmentChanges.LastOrDefault(x => x.Date <= date);
                if (!(change?.EmployedAfter ?? employmentChanges[0].EmployedBefore)) continue;
            }
            var at = assignments.Where(x => x.StartDate <= date && (x.EndDate == null || x.EndDate >= date)).ToArray();
            if (at.Length > 1) throw new BusinessRuleException($"Overlapping branch salary responsibility for {employee.FullName} on {date}.");
            if (at.Length == 0) continue;
            var salaries = settings.Where(x => x.EffectiveFrom <= date && (x.EffectiveTo == null || x.EffectiveTo >= date)).ToArray();
            if (salaries.Length != 1) throw new BusinessRuleException($"Configure unambiguous salary history for {employee.FullName} on {date}; zero salary must be explicit.");
            var daily = salaries[0].BasicSalary / days;
            exact += daily; var next = decimal.Round(exact, 2, MidpointRounding.AwayFromZero); var increment = next - rounded; rounded = next;
            if (at[0].BranchId != month.BranchId) continue;
            basic += increment; evidence.Add(new(date, at[0].Id, salaries[0].Id, salaries[0].BasicSalary, daily));
        }
        if (advances > basic) throw new BusinessRuleException($"Branch advances exceed prorated basic salary for {employee.FullName}; reconcile history before closing.");
        return new(employee.Id, employee.FullName, month.BranchId, month.Id, days, evidence.Count, basic, advances, basic - advances, evidence);
    }
    public static async Task<SalaryEntitlement> ForEmployeeAsync(ApplicationDbContext db, Employee employee, FinancialMonth month,
        CancellationToken ct, IReadOnlyList<EmployeeBranchAssignment>? assignments = null, IReadOnlyList<EmployeeSalarySetting>? settings = null, OperationAccess? access = null)
    {
        assignments ??= await db.EmployeeBranchAssignments.AsNoTracking().Where(x => x.EmployeeId == employee.Id && x.StartDate <= month.EndDate && (x.EndDate == null || x.EndDate >= month.StartDate)).ToListAsync(ct);
        settings ??= await db.EmployeeSalarySettings.AsNoTracking().Where(x => x.EmployeeId == employee.Id && x.EffectiveFrom <= month.EndDate && (x.EffectiveTo == null || x.EffectiveTo >= month.StartDate)).ToListAsync(ct);
        var advances = await db.EmployeeAdvances.Where(x => x.BranchId == month.BranchId && x.ReceivingEmployeeId == employee.Id &&
            (x.FinancialMonthId == month.Id || x.FinancialMonthId == null && x.BusinessDate >= month.StartDate && x.BusinessDate <= month.EndDate)).SumAsync(x => x.Amount, ct);
        var history = await EmploymentChangesAsync(db, [employee.Id], access, ct);
        return Calculate(employee, month, assignments, settings, advances, history.GetValueOrDefault(employee.Id));
    }
    internal static async Task<Dictionary<Guid, IReadOnlyList<SalaryEmploymentChange>>> EmploymentChangesAsync(ApplicationDbContext db, Guid[] ids, OperationAccess? access, CancellationToken ct)
    {
        var now = access?.Now;
        var audits = await db.AuditLogs.AsNoTracking().Where(x => x.Action == "EmployeeStatusChanged" && x.EntityId != null && ids.Contains(x.EntityId.Value) && (now == null || x.CreatedAt <= now))
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToListAsync(ct);
        audits.AddRange(db.ChangeTracker.Entries<AuditLog>().Where(x => x.State == EntityState.Added && x.Entity.Action == "EmployeeStatusChanged" && x.Entity.EntityId != null && ids.Contains(x.Entity.EntityId.Value)).Select(x => x.Entity));
        audits = audits.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToList();
        bool Employed(string? json)
        {
            if (json == null) throw new BusinessRuleException("Employment audit lacks salary eligibility evidence; reconcile history explicitly.");
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("EmploymentStatus",out var value)) throw new BusinessRuleException("Employment audit lacks status evidence; reconcile history explicitly.");
                EmploymentStatus status;
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) && Enum.IsDefined(typeof(EmploymentStatus),number)) status=(EmploymentStatus)number;
                else if (value.ValueKind == JsonValueKind.String && Enum.TryParse(value.GetString(),out status) && Enum.IsDefined(status)) { }
                else throw new BusinessRuleException("Employment audit has an unknown status; reconcile history explicitly.");
                return status == EmploymentStatus.Active;
            }
            catch (JsonException) { throw new BusinessRuleException("Employment audit has invalid evidence; reconcile history explicitly."); }
        }
        return audits.GroupBy(x => x.EntityId!.Value).ToDictionary(x => x.Key, x => (IReadOnlyList<SalaryEmploymentChange>)x.Select(a =>
            new SalaryEmploymentChange(access?.LocalDate(a.CreatedAt) ?? DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(a.CreatedAt,TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo")).DateTime),
                a.CreatedAt,Employed(a.OldValues),Employed(a.NewValues))).ToArray());
    }
    // Called under the employee lock by employment/transfer/salary administration.
    // Existing advances cannot be stranded by a smaller future entitlement.
    public static async Task ValidateHistoryChangeAsync(ApplicationDbContext db, Employee employee, CancellationToken ct,
        IReadOnlyList<EmployeeBranchAssignment>? assignments = null, IReadOnlyList<EmployeeSalarySetting>? settings = null)
    {
        var months = await db.FinancialMonths.AsNoTracking().Where(m => db.EmployeeAdvances.Any(a => a.ReceivingEmployeeId == employee.Id && a.BranchId == m.BranchId &&
            (a.FinancialMonthId == m.Id || a.FinancialMonthId == null && a.BusinessDate >= m.StartDate && a.BusinessDate <= m.EndDate))).ToListAsync(ct);
        foreach (var month in months) await ForEmployeeAsync(db, employee, month, ct, assignments, settings);
    }
}
