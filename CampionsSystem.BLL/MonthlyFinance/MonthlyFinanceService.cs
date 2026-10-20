using System.Text.Json;
using System.Security.Cryptography;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.MonthlyFinance;

public sealed partial class MonthlyFinanceService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private readonly FinancialOperations coordination = new(db, current, access);
    private readonly MonthlyReportBuilder reports = new(db, current, access);
    private readonly PayrollService payroll = new(db, current, access);
    internal static void Page(int page, int size)
    { if (page is < 1 or > 1000000 || size is < 1 or > 100) throw new BusinessRuleException("page must be 1..1000000; pageSize must be 1..100."); }
    internal static FinancialMonthDetails Details(FinancialMonth m) => new(m.Id, m.BranchId, m.Name, m.StartDate, m.EndDate, m.Status, m.Version, m.LastRevision, m.FirstClosedAt);
    private async Task<FinancialMonth> ReadAsync(Guid id, string permission, CancellationToken ct)
    {
        var m = await db.FinancialMonths.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Financial month not found.");
        await access.RequireAsync(permission, m.BranchId, ct); return m;
    }
    private void Event(FinancialMonth m, Guid operation, string hash, Guid actor, string kind, string reason, int? revision = null)
    {
        db.Add(new FinancialMonthEvent { FinancialMonthId = m.Id, OperationId = operation, RequestHash = hash, ActorUserId = actor,
            Kind = kind, Reason = reason.Trim(), RecordedAt = access.Now, ResultVersion = m.Version, Revision = revision });
        access.Audit(actor, m.BranchId, "FinancialMonth." + kind, nameof(FinancialMonth), m.Id, after: new { operation, reason, m.Status, m.Version, revision });
    }
    private async Task<FinancialMonthEvent?> RetryAsync(Guid id, Guid operation, string hash, Guid actor, CancellationToken ct)
    {
        var prior = await db.FinancialMonthEvents.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == operation, ct);
        if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.ActorUserId, actor); if (prior.FinancialMonthId != id) throw new BusinessRuleException("Operation belongs to another month."); }
        return prior;
    }
    public Task<FinancialMonthDetails> OpenAsync(OpenFinancialMonthRequest r, CancellationToken ct = default) => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Reason(r.Reason);
        if (string.IsNullOrWhiteSpace(r.Name) || r.Name.Length > 256 || r.StartDate == default || r.EndDate < r.StartDate || r.EndDate.DayNumber-r.StartDate.DayNumber >= 62 || r.EndDate == DateOnly.MaxValue)
            throw new BusinessRuleException("Name and inclusive financial month bounds of 1..62 days are required.");
        var (branch, actor) = await coordination.ResolveAsync(MonthlyFinancePermissions.Manage, r.BranchId, ct); await coordination.LockBranchAsync(branch, ct);
        var hash = FinancialOperations.Hash(r with { BranchId = branch });
        var prior = await db.FinancialMonthEvents.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
        if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.ActorUserId, actor); return Details(await ReadAsync(prior.FinancialMonthId, MonthlyFinancePermissions.Manage, ct)); }
        var last = await db.FinancialMonths.AsNoTracking().Where(x => x.BranchId == branch).OrderByDescending(x => x.StartDate).FirstOrDefaultAsync(ct);
        if (last != null && (last.Status == "Open" || r.StartDate != last.EndDate.AddDays(1)))
            throw new BusinessRuleException("Close the previous financial month and use its next inclusive BusinessDate for the successor.");
        var month = new FinancialMonth { BranchId = branch, Name = r.Name.Trim(), StartDate = r.StartDate, EndDate = r.EndDate, CreatedAt = access.Now, CreatedByUserId = actor };
        db.Add(month); Event(month, r.OperationId, hash, actor, "Opened", r.Reason); return Details(month);
    }, ct);
    public Task<FinancialMonthDetails> CloseAsync(Guid id, CloseFinancialMonthRequest r, CancellationToken ct = default) => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Reason(r.Reason); if (r.CountedCash != null) FinancialOperations.Money(r.CountedCash.Value);
        var info = await ReadAsync(id, MonthlyFinancePermissions.Close, ct); var actor = await access.RequireAsync(MonthlyFinancePermissions.Close, info.BranchId, ct);
        await coordination.LockBranchAsync(info.BranchId, ct); var hash = FinancialOperations.Hash(new { id, r });
        var month = await db.FinancialMonths.SingleAsync(x => x.Id == id, ct); await db.Entry(month).ReloadAsync(ct);
        if (await RetryAsync(id, r.OperationId, hash, actor, ct) != null) return Details(month);
        if (month.Version != r.ExpectedVersion) throw new DbUpdateConcurrencyException();
        if (month.Status is not ("Open" or "Reopened")) throw new BusinessRuleException("Financial month is already closed.");
        var (date, _) = await coordination.BusinessDateAsync(month.BranchId, ct);
        if (date < month.EndDate) throw new BusinessRuleException("The financial month cannot close before its last BusinessDate.");
        if (await db.Shifts.AnyAsync(x => x.BranchId == month.BranchId && x.Status == ShiftStatus.Open, ct)) throw new BusinessRuleException("Close every employee Shift at this branch before monthly closing.");
        // Serialize salary/assignment changes with closing; financial posts and shift
        // opens already serialize with this same branch drawer lock.
        var employees = await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\" IN (SELECT \"EmployeeId\" FROM \"EmployeeBranchAssignments\" WHERE \"BranchId\"={month.BranchId} AND \"StartDate\"<={month.EndDate} AND (\"EndDate\" IS NULL OR \"EndDate\">={month.StartDate})) ORDER BY \"Id\" FOR UPDATE").AsNoTracking().ToListAsync(ct);
        _ = employees;
        var first = month.FirstClosedAt == null;
        DrawerPosition? preserved = null;
        IReadOnlyList<SalaryEntitlement>? preservedSalaries = null;
        if (!first)
        {
            var original = await db.FinancialClosingRevisions.AsNoTracking().Where(x => x.FinancialMonthId == id).OrderBy(x => x.Revision).Select(x => x.ReportJson).FirstAsync(ct);
            var originalReport = JsonSerializer.Deserialize<MonthlyReport>(original)!;
            preserved = originalReport.Drawer;
            preservedSalaries = originalReport.Salaries;
            if (r.CountedCash != null) throw new BusinessRuleException("Historical reopening preserves the original drawer cutoff; record a present-day recount through Vault.");
        }
        if (r.CountedCash != null) await coordination.CountAsync(month.BranchId, actor, r.OperationId, hash, r.CountedCash.Value, "Recount", null, r.Reason, ct);
        await db.SaveChangesAsync(ct);
        if (first)
        {
            var salaries = await reports.SalariesAsync(month, ct);
            await payroll.RecognizeBasicAsync(month, salaries, actor, ct);
            await payroll.RecognizeCommissionsAsync(month, actor, null, ct);
        }
        month.Status = "Closed"; month.FirstClosedAt ??= access.Now; month.Version++; month.LastRevision++;
        var report = await reports.BuildAsync(month, month.LastRevision, access.Now, preserved, ct, preservedSalaries);
        var bytes = MonthlyWorkbook.Create(report, ct); var sha = Convert.ToHexString(SHA256.HashData(bytes));
        var reportJson = JsonSerializer.Serialize(report);
        if (System.Text.Encoding.UTF8.GetByteCount(reportJson) > 64*1024*1024) throw new BusinessRuleException("Report snapshot exceeds the 64 MiB limit.");
        db.Add(new FinancialClosingRevision { FinancialMonthId = month.Id, Revision = month.LastRevision, SnapshotAt = access.Now, ActorUserId = actor,
            Reason = r.Reason.Trim(), ReportJson = reportJson, Workbook = bytes, WorkbookSha256 = sha });
        Event(month, r.OperationId, hash, actor, "Closed", r.Reason, month.LastRevision); return Details(month);
    }, ct);
    public Task<FinancialMonthDetails> ReopenAsync(Guid id, ReopenFinancialMonthRequest r, CancellationToken ct = default) => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Reason(r.Reason);
        var info = await ReadAsync(id, MonthlyFinancePermissions.Reopen, ct); var actor = await access.RequireAsync(MonthlyFinancePermissions.Reopen, info.BranchId, ct);
        await coordination.LockBranchAsync(info.BranchId, ct); var hash = FinancialOperations.Hash(new { id, r });
        var month = await db.FinancialMonths.SingleAsync(x => x.Id == id, ct); await db.Entry(month).ReloadAsync(ct);
        if (await RetryAsync(id, r.OperationId, hash, actor, ct) != null) return Details(month);
        if (month.Version != r.ExpectedVersion) throw new DbUpdateConcurrencyException();
        if (month.Status != "Closed") throw new BusinessRuleException("Only a closed financial month can be reopened.");
        month.Status = "Reopened"; month.Version++; Event(month, r.OperationId, hash, actor, "Reopened", r.Reason); return Details(month);
    }, ct);
    public async Task<FinancialMonthDetails> GetAsync(Guid id, CancellationToken ct = default) => Details(await ReadAsync(id, MonthlyFinancePermissions.View, ct));
    public async Task<ShiftPage<FinancialMonthDetails>> HistoryAsync(Guid? branchId = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); var (branch, _) = await coordination.ResolveAsync(MonthlyFinancePermissions.View, branchId, ct);
        var q = db.FinancialMonths.AsNoTracking().Where(x => x.BranchId == branch); var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.StartDate).ThenBy(x => x.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(Details).ToArray(), total, page, pageSize);
    }
    public async Task<IReadOnlyList<RevisionDetails>> RevisionsAsync(Guid id, CancellationToken ct = default)
    {
        await ReadAsync(id, MonthlyFinancePermissions.View, ct);
        var rows=await db.FinancialClosingRevisions.AsNoTracking().Where(x => x.FinancialMonthId == id).OrderBy(x => x.Revision)
            .Select(x => new RevisionDetails(x.Id,x.Revision,x.SnapshotAt,x.ActorUserId,x.Reason,x.WorkbookSha256,x.SchemaVersion)).ToListAsync(ct);
        var names=await ActorDisplayNames.LoadAsync(db,rows.Select(x=>x.ActorUserId),ct);
        return rows.Select(x=>x with{ActorName=ActorDisplayNames.Get(names,x.ActorUserId)}).ToArray();
    }
    public async Task<ShiftPage<MonthEventDetails>> EventsAsync(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await ReadAsync(id, MonthlyFinancePermissions.View, ct);
        var q = db.FinancialMonthEvents.AsNoTracking().Where(x => x.FinancialMonthId == id); var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.ResultVersion).ThenBy(x => x.Id).Skip((page-1)*pageSize).Take(pageSize)
            .Select(x => new MonthEventDetails(x.Id,x.Kind,x.OperationId,x.ActorUserId,x.Reason,x.RecordedAt,x.ResultVersion,x.Revision)).ToListAsync(ct);
        var names=await ActorDisplayNames.LoadAsync(db,rows.Select(x=>x.ActorUserId),ct);
        return new(rows.Select(x=>x with{ActorName=ActorDisplayNames.Get(names,x.ActorUserId)}).ToArray(),total,page,pageSize);
    }
    public async Task<MonthlyReport> ReportAsync(Guid id, int? revision = null, CancellationToken ct = default)
    {
        var m = await ReadAsync(id, MonthlyFinancePermissions.View, ct);
        if (revision != null || m.LastRevision > 0)
        {
            var number = revision ?? m.LastRevision;
            var json = await db.FinancialClosingRevisions.AsNoTracking().Where(x => x.FinancialMonthId == id && x.Revision == number).Select(x => x.ReportJson).SingleOrDefaultAsync(ct)
                ?? throw new BranchNotFoundException("Closing revision not found.");
            return await WithCurrentNamesAsync(JsonSerializer.Deserialize<MonthlyReport>(json)!,ct);
        }
        if (db.Database.CurrentTransaction != null) return await WithCurrentNamesAsync(await reports.BuildAsync(m,0,access.Now,null,ct),ct);
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
        var report = await reports.BuildAsync(m,0,access.Now,null,ct); await tx.CommitAsync(ct);
        return await WithCurrentNamesAsync(report,ct);
    }
    public async Task<ShiftPage<FinancialTransactionReportRow>> TransactionsAsync(Guid id, int? revision = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page,pageSize); var report = await ReportAsync(id,revision,ct); var rows=report.Transactions;
        return new(rows.Skip((page-1)*pageSize).Take(pageSize).ToArray(),rows.Count,page,pageSize);
    }
    public async Task<WorkbookDownload> ExportAsync(Guid id, int? revision = null, CancellationToken ct = default)
    {
        var m=await ReadAsync(id,MonthlyFinancePermissions.Export,ct); var number=revision??m.LastRevision;
        if(number<1)throw new BusinessRuleException("Close the financial month before downloading its immutable workbook.");
        var r=await db.FinancialClosingRevisions.AsNoTracking().Where(x=>x.FinancialMonthId==id&&x.Revision==number).Select(x=>new{x.Workbook,x.WorkbookSha256}).SingleOrDefaultAsync(ct)
            ??throw new BranchNotFoundException("Closing revision not found.");
        return new(r.Workbook,$"Campeones-{m.BranchId}-{m.StartDate:yyyyMMdd}-{m.EndDate:yyyyMMdd}-r{number}.xlsx",r.WorkbookSha256);
    }
    public async Task<ConsolidatedMonthSummary> ConsolidatedAsync(IReadOnlyList<Guid> monthIds, CancellationToken ct = default)
    {
        await access.RequireAsync(MonthlyFinancePermissions.Consolidated,null,ct); await access.RequireAsync("Branches.AccessAll",null,ct);
        if(monthIds.Count is <1 or >100 || monthIds.Distinct().Count()!=monthIds.Count)throw new BusinessRuleException("Select 1..100 distinct financial months, one per branch.");
        var data=new List<MonthlyReport>();foreach(var id in monthIds)data.Add(await ReportAsync(id,null,ct));
        if(data.Select(x=>x.Month.BranchId).Distinct().Count()!=data.Count)throw new BusinessRuleException("Select at most one financial month per branch.");
        return new(monthIds,data.Sum(x=>x.Summary.NetCollections),data.Sum(x=>x.Summary.OrdinaryExpenses),data.Sum(x=>x.Summary.BasicSalaryEarned+x.Summary.SalesCommissionRecognized),
            data.Sum(x=>x.Summary.PrivateCoachEarningsRecognized),data.Sum(x=>x.Summary.NetOperatingResult),data.Sum(x=>x.Summary.EmployeeClosingOutstanding),data.Sum(x=>x.Summary.CoachClosingOutstanding));
    }
}
