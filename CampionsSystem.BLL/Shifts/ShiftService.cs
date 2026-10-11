using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Shifts;

public sealed class ShiftService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private readonly FinancialOperations money = new(db, current, access);
    public async Task<BusinessDayDetails?> CurrentBusinessDayAsync(Guid? branchId=null,CancellationToken ct=default)
    {
        var(branch,_)=await money.ResolveAsync(ShiftPermissions.View,branchId,ct);
        var row=await db.BusinessDays.AsNoTracking().SingleOrDefaultAsync(x=>x.BranchId==branch&&x.Status==BusinessDayStatus.Open,ct);
        return row==null?null:DayDetails(row);
    }
    public static BusinessDayDetails DayDetails(BusinessDay day)=>new(day.Id,day.BranchId,day.BusinessDate,day.Status,day.OpenedAt,day.ClosedAt,day.OpenedByUserId,day.ClosedByUserId);
    private async Task<ShiftDetails> Details(Shift row, CancellationToken ct) => new(row.Id, row.BranchId, row.EmployeeId,
        row.EmployeeId == null ? null : await db.Employees.Where(x => x.Id == row.EmployeeId).Select(x => x.FullName).SingleAsync(ct),
        row.Status, row.Version, row.OpenedAt, row.ClosedAt, row.OpeningBusinessDate, row.ClosingBusinessDate, row.OpeningCash,
        row.ExpectedClosingCash, row.ActualClosingCash, row.Difference);

    public Task<ShiftDetails> OpenAsync(OpenShiftRequest r, CancellationToken ct = default) => money.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.OpeningCash);
        var (branch, actor) = await money.ResolveAsync(ShiftPermissions.Manage, r.BranchId, ct); var hash = FinancialOperations.Hash(r with { BranchId = branch });
        await money.LockBranchAsync(branch, ct);
        var prior = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
        if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.OpenedByUserId, actor); if (prior.BranchId != branch) throw new UnauthorizedAccessException(); return await Details(prior, ct); }
        var (date, day) = await money.BusinessDateAsync(branch, ct); var employee = await money.EmployeeAsync(actor, branch, date, ct, true);
        if (await db.EmployeeLeaveRequests.AnyAsync(x => x.EmployeeId == employee.Id && x.Status == EmployeeLeaveRequestStatus.Approved && x.FromDate <= date && x.ToDate >= date, ct))
            throw new BusinessRuleException("Employee has approved leave for this business date.");
        if (await db.Shifts.AnyAsync(x => x.EmployeeId == employee.Id && x.Status == ShiftStatus.Open, ct))
            throw new BusinessRuleException("You already have an open shift; retrieve and close it instead of creating another.");
        var drawer = await money.DrawerAsync(branch, ct);
        var shift = new Shift { BranchId = branch, EmployeeId = employee.Id, FinancialAccountId = drawer.FinancialAccountId,
            OperationId = r.OperationId, RequestHash = hash, OpeningCash = r.OpeningCash, OpenedByUserId = actor, OpenedAt = access.Now,
            OpeningBusinessDate = date, BusinessDayId = day, Status = ShiftStatus.Open, Version = 1 };
        db.Add(shift); db.Add(new ShiftEmployee { Shift = shift, EmployeeId = employee.Id, JoinedAt = access.Now });
        await money.CountAsync(branch, actor, r.OperationId, hash, r.OpeningCash, "Opening", shift.Id, null, ct);
        access.Audit(actor, branch, "Shift.Opened", nameof(Shift), shift.Id, after: new { employee.Id, r.OpeningCash, date });
        await db.SaveChangesAsync(ct); return await Details(shift, ct);
    }, ct);

    public Task<ShiftDetails> CloseAsync(Guid id, CloseShiftRequest r, CancellationToken ct = default) => money.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.CountedCash);
        if(r.Reason?.Length>2000)throw new BusinessRuleException("Closing reason must be at most 2000 characters.");
        var info = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Shift not found.");
        var actor = await access.RequireAsync(ShiftPermissions.Manage, info.BranchId, ct);
        if (info.OpenedByUserId != actor || info.EmployeeId == null || info.OperationId == null) throw new UnauthorizedAccessException();
        await money.LockBranchAsync(info.BranchId, ct);
        var shift = await db.Shifts.FromSqlInterpolated($"SELECT * FROM \"Shifts\" WHERE \"Id\"={id} FOR UPDATE").SingleAsync(ct); await db.Entry(shift).ReloadAsync(ct);
        var hash = FinancialOperations.Hash(new { id, r });
        var prior = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.CloseOperationId == r.OperationId, ct);
        if (prior != null) { FinancialOperations.Retry(prior.CloseRequestHash, hash, prior.ClosedByUserId!.Value, actor); return await Details(prior, ct); }
        if (shift.Status != ShiftStatus.Open) throw new BusinessRuleException("Shift already closed.");
        if (shift.Version != r.ExpectedVersion) throw new DbUpdateConcurrencyException();
        if (access.Now < shift.OpenedAt) throw new BusinessRuleException("Cannot backdate shift closure.");
        var count = await money.CountAsync(shift.BranchId, actor, r.OperationId, hash, r.CountedCash, "Closing", id, r.Reason, ct);
        shift.CloseOperationId = r.OperationId; shift.CloseRequestHash = hash; shift.ClosedAt = access.Now; shift.ClosedByUserId = actor;
        shift.ExpectedClosingCash = count.ExpectedCash; shift.ActualClosingCash = count.CountedCash; shift.Difference = count.Difference;
        shift.ClosingBusinessDate = count.BusinessDate; shift.ClosingReason = r.Reason; shift.Status = ShiftStatus.Closed; shift.Version++;
        var participant = await db.ShiftEmployees.SingleAsync(x => x.ShiftId == id && x.EmployeeId == shift.EmployeeId, ct); participant.LeftAt = access.Now;
        access.Audit(actor, shift.BranchId, "Shift.Closed", nameof(Shift), id, after: new { count.ExpectedCash, count.CountedCash, count.Difference, count.BusinessDate });
        await db.SaveChangesAsync(ct); return await Details(shift, ct);
    }, ct);

    public async Task<ShiftDetails?> CurrentAsync(CancellationToken ct = default)
    {
        var actor = await access.RequireAsync(ShiftPermissions.View, null, ct);
        var row = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.OpenedByUserId == actor && x.EmployeeId != null && x.Status == ShiftStatus.Open, ct);
        if (row == null) return null; await access.RequireAsync(ShiftPermissions.View, row.BranchId, ct); return await Details(row, ct);
    }
    public async Task<ShiftDetails> GetAsync(Guid id, CancellationToken ct = default)
    {
        var row = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Shift not found.");
        await access.RequireAsync(ShiftPermissions.View, row.BranchId, ct); return await Details(row, ct);
    }
    public async Task<ShiftPage<ShiftDetails>> HistoryAsync(ShiftHistoryRequest r, CancellationToken ct = default)
    {
        var (branch, _) = await money.ResolveAsync(ShiftPermissions.View, r.BranchId, ct);
        if (r.Page < 1 || r.PageSize is < 1 or > 100 || r.From > r.To) throw new BusinessRuleException("Invalid history range or pagination.");
        var query = db.Shifts.AsNoTracking().Where(x => x.BranchId == branch);
        if (r.EmployeeId != null) query = query.Where(x => x.EmployeeId == r.EmployeeId);
        if (r.From != null) query = query.Where(x => x.OpeningBusinessDate >= r.From);
        if (r.To != null) query = query.Where(x => x.OpeningBusinessDate <= r.To);
        var total = await query.CountAsync(ct); var rows = await query.OrderByDescending(x => x.OpenedAt).ThenBy(x => x.Id).Skip((r.Page - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
        var items = new List<ShiftDetails>(); foreach (var row in rows) items.Add(await Details(row, ct));
        return new(items, total, r.Page, r.PageSize);
    }

    public Task<BusinessDay> OpenBusinessDayAsync(OpenBusinessDayRequest r, CancellationToken ct = default) => money.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId);
        var (branch, actor) = await money.ResolveAsync(ShiftPermissions.Manage, r.BranchId, ct); await money.LockBranchAsync(branch, ct);
        var prior = await db.BusinessDays.AsNoTracking().SingleOrDefaultAsync(x => x.Id == r.OperationId, ct);
        if (prior != null) { if (prior.BranchId != branch || prior.BusinessDate != r.BusinessDate || prior.OpenedByUserId != actor) throw new BusinessRuleException("Operation identity belongs to another BusinessDay."); return prior; }
        if (r.BusinessDate > access.Today || await db.BusinessDays.AnyAsync(x => x.BranchId == branch && (x.Status == BusinessDayStatus.Open || x.BusinessDate >= r.BusinessDate), ct))
            throw new BusinessRuleException("Close the prior BusinessDay first; business dates must advance and cannot be future dates.");
        var day = new BusinessDay { Id = r.OperationId, BranchId = branch, BusinessDate = r.BusinessDate, OpenedByUserId = actor, OpenedAt = access.Now, CreatedAt = access.Now, Status = BusinessDayStatus.Open };
        db.Add(day); access.Audit(actor, branch, "BusinessDay.Opened", nameof(BusinessDay), day.Id, after: new { day.BusinessDate }); return day;
    }, ct);
    public Task<BusinessDay> CloseBusinessDayAsync(Guid id, CancellationToken ct = default) => money.TransactionAsync(async () =>
    {
        var info = await db.BusinessDays.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("BusinessDay not found.");
        var actor = await access.RequireAsync(ShiftPermissions.Manage, info.BranchId, ct); await money.LockBranchAsync(info.BranchId, ct);
        var day = await db.BusinessDays.SingleAsync(x => x.Id == id, ct); await db.Entry(day).ReloadAsync(ct);
        if (day.Status == BusinessDayStatus.Closed) return day;
        if(access.Now<day.OpenedAt)throw new BusinessRuleException("Cannot backdate BusinessDay closure.");
        day.ClosedAt = access.Now; day.ClosedByUserId = actor; day.Status = BusinessDayStatus.Closed;
        access.Audit(actor, day.BranchId, "BusinessDay.Closed", nameof(BusinessDay), id); return day;
    }, ct);
}
