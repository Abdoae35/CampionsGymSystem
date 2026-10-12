using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Commissions;

public sealed partial class CommissionService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private readonly FinancialOperations coordination = new(db, current, access);
    private async Task<Guid> AdminAsync(string permission, Guid branch, CancellationToken ct)
    {
        var actor = await access.RequireAsync(permission, branch, ct);
        await access.RequireAsync("Branches.AccessAll", branch, ct); return actor;
    }
    private async Task LockAsync(Guid branch, CancellationToken ct)
    {
        if (await db.CashDrawers.AnyAsync(d => d.BranchId == branch, ct)) await coordination.LockBranchAsync(branch, ct);
        else
        {
            var row = await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={branch} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct);
            if (row is null || !row.IsActive) throw new BusinessRuleException("Select an active branch.");
        }
    }
    private static void Page(int page, int size)
    { if (page < 1 || page > 1000000 || size is < 1 or > 100) throw new BusinessRuleException("Page must be positive; pageSize must be 1..100."); }
    internal static CommissionPeriodDetails Details(CommissionPeriod p) => new(p.Id, p.BranchId, p.Name, p.StartBusinessDate, p.EndBusinessDate, p.Status.ToString(), p.Version, p.ClosedAt);
    private static CommissionRuleDetails RuleDetails(CommissionRule r) => new(r.Id, r.EmployeeId!.Value, r.BranchId, r.CommissionType.ToString(), r.EffectiveFrom, r.TargetAmount, r.BelowTargetRate, r.ReachedTargetRate, r.Revision);
    private async Task<CommissionPeriod> ReadAsync(Guid id, CancellationToken ct)
    {
        var p = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Commission period not found.");
        await access.RequireAsync(CommissionPermissions.View, p.BranchId, ct);
        if (p.StartBusinessDate == null || p.EndBusinessDate == null) throw new BusinessRuleException("Legacy commission period needs explicit reconciliation; no BusinessDate bounds were invented.");
        return p;
    }
    public Task<CommissionPeriodDetails> OpenAsync(OpenCommissionPeriodRequest r, CancellationToken ct = default) => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId);
        var (branch, _) = await coordination.ResolveAsync(CommissionPermissions.Close, r.BranchId, ct);
        var actor = await AdminAsync(CommissionPermissions.Close, branch, ct); await LockAsync(branch, ct);
        var hash = FinancialOperations.Hash(r with { BranchId = branch });
        var retry = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.OperationId == r.OperationId, ct);
        if (retry != null) { if (retry.RequestHash != hash || retry.BranchId != branch) throw new BusinessRuleException("Operation belongs to another period request."); return Details(retry); }
        if (string.IsNullOrWhiteSpace(r.Name) || r.Name.Length > 256 || r.StartDate == default || r.EndDate < r.StartDate || r.EndDate == DateOnly.MaxValue)
            throw new BusinessRuleException("A name and valid inclusive BusinessDate bounds are required.");
        var last = await db.CommissionPeriods.AsNoTracking().Where(p => p.BranchId == branch).OrderByDescending(p => p.StartAt).FirstOrDefaultAsync(ct);
        if (last != null && (last.Status != CommissionPeriodStatus.Closed || last.EndBusinessDate == null || r.StartDate != last.EndBusinessDate.Value.AddDays(1)))
            throw new BusinessRuleException("Close the preceding managed period; the next period must start immediately after its EndDate.");
        var period = new CommissionPeriod { BranchId = branch, Name = r.Name.Trim(), StartBusinessDate = r.StartDate, EndBusinessDate = r.EndDate,
            StartAt = access.BusinessDateBounds(r.StartDate).Start, EndAt = access.BusinessDateBounds(r.EndDate).End,
            OperationId = r.OperationId, RequestHash = hash, CreatedAt = access.Now, Status = CommissionPeriodStatus.Open };
        db.Add(period); access.Audit(actor, branch, "Commission.PeriodOpened", nameof(CommissionPeriod), period.Id, after: r); return Details(period);
    }, ct);
    public Task<IReadOnlyList<CommissionRuleDetails>> ConfigureAsync(ConfigureCommissionRequest r, CancellationToken ct = default) => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); var (branch, _) = await coordination.ResolveAsync(CommissionPermissions.Manage, r.BranchId, ct);
        var actor = await AdminAsync(CommissionPermissions.Manage, branch, ct); await LockAsync(branch, ct);
        var hash = FinancialOperations.Hash(r with { BranchId = branch });
        var prior = await db.CommissionRules.AsNoTracking().Where(x => x.OperationId == r.OperationId).ToListAsync(ct);
        if (prior.Count > 0) { if (prior.Count != 2 || prior.Any(x => x.RequestHash != hash || x.CreatedByUserId != actor)) throw new BusinessRuleException("Operation belongs to another configuration request."); return (IReadOnlyList<CommissionRuleDetails>)prior.Select(RuleDetails).ToArray(); }
        if (!await db.Employees.AnyAsync(e => e.Id == r.EmployeeId, ct)) throw new BranchNotFoundException("Employee not found.");
        if (r.NewClients == null || r.Renewals == null) throw new BusinessRuleException("Both independent category rules are required.");
        var period = await db.CommissionPeriods.AsNoTracking().Where(p => p.BranchId == branch).OrderByDescending(p => p.StartAt).FirstOrDefaultAsync(ct);
        if (period != null && (period.EndBusinessDate == null || r.EffectiveFrom != period.EndBusinessDate.Value.AddDays(1)))
            throw new BusinessRuleException("Configuration changes must start on the next applicable period's first BusinessDate.");
        var (today, _) = await coordination.BusinessDateAsync(branch, ct);
        if (r.EffectiveFrom == default || period == null && r.EffectiveFrom < today) throw new BusinessRuleException("Initial rule configuration cannot be backdated.");
        var result = new List<CommissionRuleDetails>();
        foreach (var (type, input) in new[] { (CommissionType.SalesNew, r.NewClients), (CommissionType.SalesRenewal, r.Renewals) })
        {
            FinancialOperations.Money(input.Target); OperationAccess.Percentage(input.BelowRate); OperationAccess.Percentage(input.ReachedRate);
            if (input.Target <= 0 || input.ReachedRate < input.BelowRate) throw new BusinessRuleException("Target must be positive and reached rate cannot be below the base rate.");
            var revision = await db.CommissionRules.Where(x => x.BranchId == branch && x.EmployeeId == r.EmployeeId && x.CommissionType == type).MaxAsync(x => (int?)x.Revision, ct) ?? 0;
            var rule = new CommissionRule { BranchId = branch, EmployeeId = r.EmployeeId, CommissionType = type, CalculationMethod = CommissionCalculationMethod.TargetPercentage,
                TargetAmount = input.Target, BelowTargetRate = input.BelowRate, ReachedTargetRate = input.ReachedRate, EffectiveFrom = r.EffectiveFrom,
                Revision = revision + 1, OperationId = r.OperationId, RequestHash = hash, CreatedByUserId = actor, CreatedAt = access.Now };
            db.Add(rule); result.Add(RuleDetails(rule)); access.Audit(actor, branch, "Commission.RuleConfigured", nameof(CommissionRule), rule.Id, after: result[^1]);
        }
        return (IReadOnlyList<CommissionRuleDetails>)result;
    }, ct);
    public async Task<CommissionPage<CommissionPeriodDetails>> PeriodsAsync(Guid? branchId = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); var (branch, _) = await coordination.ResolveAsync(CommissionPermissions.View, branchId, ct);
        var q = db.CommissionPeriods.AsNoTracking().Where(x => x.BranchId == branch);
        var count = await q.CountAsync(ct); var rows = await q.OrderByDescending(x => x.StartAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(Details).ToArray(), count, page, pageSize);
    }
    public async Task<CommissionPage<CommissionRuleDetails>> RulesAsync(Guid employeeId, Guid? branchId = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); var (branch, _) = await coordination.ResolveAsync(CommissionPermissions.View, branchId, ct);
        var q = db.CommissionRules.AsNoTracking().Where(x => x.BranchId == branch && x.EmployeeId == employeeId && (x.CommissionType == CommissionType.SalesNew || x.CommissionType == CommissionType.SalesRenewal));
        var count = await q.CountAsync(ct); var rows = await q.OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.Revision).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(RuleDetails).ToArray(), count, page, pageSize);
    }
}
