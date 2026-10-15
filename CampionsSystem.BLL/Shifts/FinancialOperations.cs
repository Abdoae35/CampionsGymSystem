using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Shifts;

// Concrete coordination for this database. A branch lock serializes shared-drawer facts,
// including other employees' collections; no employee account or mutable cash balance.
public sealed class FinancialOperations(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    public static string Hash<T>(T value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));
    public static void Operation(Guid id) { if (id == Guid.Empty) throw new BusinessRuleException("A nonempty operation identity is required."); }
    public static void Money(decimal value, bool positive = false)
    { OperationAccess.Money(value); if (value > 9999999999999999.99m || positive && value <= 0) throw new BusinessRuleException("Invalid money amount."); }
    public static void Reason(string? reason) { if (string.IsNullOrWhiteSpace(reason) || reason.Length > 2000) throw new BusinessRuleException("A reason of up to 2000 characters is required."); }
    public static void Retry(string? storedHash, string hash, Guid originalActor, Guid actor)
    { if (storedHash != hash || originalActor != actor) throw new BusinessRuleException("Operation identity belongs to a different request or actor."); }
    public async Task<T> TransactionAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        var owned = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        var tx = db.Database.CurrentTransaction!; var point = "drawer_" + Guid.NewGuid().ToString("N");
        try { await tx.CreateSavepointAsync(point, ct); var result = await action(); await db.SaveChangesAsync(ct); await tx.ReleaseSavepointAsync(point, ct); if (owned != null) await owned.CommitAsync(ct); return result; }
        catch { if (owned != null) await owned.RollbackAsync(CancellationToken.None); else await tx.RollbackToSavepointAsync(point, CancellationToken.None); db.ChangeTracker.Clear(); throw; }
        finally { if (owned != null) await owned.DisposeAsync(); }
    }
    public async Task<(Guid Branch, Guid Actor)> ResolveAsync(string permission, Guid? selected, CancellationToken ct)
    {
        var user = await current.GetAsync(ct) ?? throw new UnauthorizedAccessException();
        var branch = selected ?? user.CurrentBranchId ?? throw new BusinessRuleException("Select an operational branch.");
        var actor = await access.RequireAsync(permission, branch, ct); return (branch, actor);
    }
    public async Task LockBranchAsync(Guid branch, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction == null) throw new InvalidOperationException("Drawer coordination requires a transaction.");
        // Lock the dedicated shared drawer, not the branch exclusively: existing Plan
        // administration takes branch SHARE after Plan locks and must not deadlock here.
        var drawer = await db.CashDrawers.FromSqlInterpolated($"SELECT * FROM \"CashDrawers\" WHERE \"BranchId\"={branch} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (drawer == null) throw new BusinessRuleException("Configure the branch shared drawer first.");
        var row = await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={branch} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (row is null) throw new BranchNotFoundException("Branch not found.");
        if (!row.IsActive) throw new BusinessRuleException("Branch is inactive.");
    }
    public async Task<(DateOnly Date, Guid? Day)> BusinessDateAsync(Guid branch, CancellationToken ct)
    {
        var day = await db.BusinessDays.FromSqlInterpolated($"SELECT * FROM \"BusinessDays\" WHERE \"BranchId\"={branch} AND \"Status\"='Open' FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (day != null && (day.OpenedAt > access.Now || day.BusinessDate > access.Today)) throw new BusinessRuleException("Invalid open BusinessDay.");
        return (day?.BusinessDate ?? access.Today, day?.Id);
    }
    public async Task<Employee> EmployeeAsync(Guid actor, Guid branch, DateOnly date, CancellationToken ct, bool lockEmployee = false)
    {
        var employee = lockEmployee
            ? await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"UserId\"={actor} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct)
            : await db.Employees.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == actor, ct);
        if (employee == null || employee.EmploymentStatus != EmploymentStatus.Active || employee.HireDate > date || employee.TerminationDate <= date)
            throw new BusinessRuleException("An active linked employee is required.");
        if (!await db.EmployeeBranchAssignments.AnyAsync(x => x.EmployeeId == employee.Id && x.BranchId == branch && x.StartDate <= date && (x.EndDate == null || x.EndDate >= date), ct))
            throw new BusinessRuleException("Employee is not assigned to the operational branch.");
        return employee;
    }
    public async Task<Shift> RequireShiftAsync(Guid branch, Guid actor, CancellationToken ct)
    {
        var (date, _) = await BusinessDateAsync(branch, ct); var employee = await EmployeeAsync(actor, branch, date, ct);
        var shift = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.EmployeeId == employee.Id && x.Status == ShiftStatus.Open, ct);
        if (shift == null || shift.BranchId != branch || shift.OpenedByUserId != actor || shift.OperationId == null || shift.OpenedAt > access.Now)
            throw new BusinessRuleException("Open your own employee shift at this branch before performing this operation.");
        return shift;
    }
    public async Task<CashDrawer> DrawerAsync(Guid branch, CancellationToken ct)
    {
        var drawer = await db.CashDrawers.SingleOrDefaultAsync(x => x.BranchId == branch, ct)
            ?? throw new BusinessRuleException("Configure the branch shared drawer first.");
        await db.Entry(drawer).ReloadAsync(ct); return drawer;
    }
    public async Task<decimal> BookAsync(Guid account, CancellationToken ct)
    {
        var opening = await db.FinancialAccounts.Where(x => x.Id == account).Select(x => x.OpeningBalance).SingleAsync(ct);
        return opening + await db.FinancialTransactions.Where(x => x.FinancialAccountId == account)
            .SumAsync(x => x.TransactionType == FinancialTransactionType.Credit ? x.Amount : -x.Amount, ct);
    }
    public async Task<IReadOnlyList<ShiftAccountBalance>> AccountBalancesAsync(Guid branch, CancellationToken ct)
    {
        var accounts = await db.FinancialAccounts.AsNoTracking().Where(x => x.BranchId == branch)
            .Select(x => new { x.Id, x.AccountType, x.OpeningBalance }).ToListAsync(ct);
        var movements = await db.FinancialTransactions.AsNoTracking().Where(x => x.BranchId == branch)
            .GroupBy(x => x.FinancialAccountId)
            .Select(g => new { AccountId = g.Key, Net = g.Sum(x => x.TransactionType == FinancialTransactionType.Credit ? x.Amount : -x.Amount) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Net, ct);
        return accounts.OrderBy(x => x.AccountType).ThenBy(x => x.Id)
            .Select(x => new ShiftAccountBalance(x.Id, x.AccountType, x.OpeningBalance + movements.GetValueOrDefault(x.Id)))
            .ToArray();
    }
    public async Task<DrawerPosition> PositionAsync(Guid branch, CancellationToken ct)
    {
        var drawer = await DrawerAsync(branch, ct);
        var last = await db.CashCounts.AsNoTracking().Where(x => x.BranchId == branch).OrderByDescending(x => x.ObservationOrder).FirstOrDefaultAsync(ct);
        var book = await BookAsync(drawer.FinancialAccountId, ct);
        var physical = last == null ? book : last.CountedCash + await db.FinancialTransactions.Where(x => x.BranchId == branch && x.FinancialAccountId == drawer.FinancialAccountId && x.DrawerSequence > last.MovementSequence)
            .SumAsync(x => x.TransactionType == FinancialTransactionType.Credit ? x.Amount : -x.Amount, ct);
        var differences = await db.CashCounts.Where(x => x.BranchId == branch).SumAsync(x => x.Difference, ct);
        return new(branch, drawer.FinancialAccountId, book, physical, last?.CountedCash ?? book, drawer.MovementSequence, last?.Id, differences);
    }
    public async Task<CashCount> CountAsync(Guid branch, Guid actor, Guid operation, string hash, decimal counted, string kind, Guid? shift, string? reason, CancellationToken ct)
    {
        Money(counted); var pos = await PositionAsync(branch, ct); var (date, day) = await BusinessDateAsync(branch, ct);
        var count = new CashCount { BranchId = branch, FinancialAccountId = pos.AccountId, ShiftId = shift, OperationId = operation, RequestHash = hash,
            BusinessDate = date, BusinessDayId = day, Kind = kind, MovementSequence = pos.MovementSequence, ExpectedCash = pos.ExpectedPhysicalCash,
            CountedCash = counted, BookCash = pos.BookCash, Difference = counted - pos.ExpectedPhysicalCash, OccurredAt = access.Now, CreatedByUserId = actor, Reason = reason };
        db.Add(count); access.Audit(actor, branch, "Vault.CashCounted", nameof(CashCount), count.Id, after: new { kind, shift, count.ExpectedCash, counted, count.Difference, count.BookCash, date });
        return count;
    }
    public async Task<FinancialAccount> AccountAsync(Guid branch, Guid id, CancellationToken ct, bool historicalReversal = false)
    {
        var account = await db.FinancialAccounts.FromSqlInterpolated($"SELECT * FROM \"FinancialAccounts\" WHERE \"Id\"={id} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (account == null || account.BranchId != branch || !historicalReversal && !account.IsActive || account.AccountType is not (FinancialAccountType.Cash or FinancialAccountType.InstaPay))
            throw new BusinessRuleException("Select an active Cash or InstaPay account of this branch.");
        if (account.AccountType == FinancialAccountType.Cash && (await DrawerAsync(branch, ct)).FinancialAccountId != id)
            throw new BusinessRuleException("Cash operations must use the configured shared drawer account.");
        return account;
    }
    public async Task CheckCashPayoutAsync(Guid branch, decimal amount, CancellationToken ct)
    { Money(amount); if ((await PositionAsync(branch, ct)).ExpectedPhysicalCash < amount) throw new BusinessRuleException("Insufficient tracked physical Cash."); }
    public async Task PostAsync(FinancialTransaction row, CancellationToken ct, bool historicalReversal = false)
    {
        if (db.Database.CurrentTransaction == null) throw new InvalidOperationException("Financial posting requires a transaction.");
        var account = await AccountAsync(row.BranchId, row.FinancialAccountId, ct, historicalReversal);
        if (account.AccountType == FinancialAccountType.Cash)
        {
            if (row.TransactionType == FinancialTransactionType.Debit) await CheckCashPayoutAsync(row.BranchId, row.Amount, ct);
            var drawer = await DrawerAsync(row.BranchId, ct); row.DrawerSequence = ++drawer.MovementSequence;
        }
        var (postingDate, _) = await BusinessDateAsync(row.BranchId, ct);
        row.FinancialMonthId = (await CampionsSystem.BLL.MonthlyFinance.FinancialMonthPosting.ResolveAsync(db, row.BranchId, postingDate, ct))?.Id;
        row.PostedAt = access.Now;
        db.Add(row);
        // Flush each ordered post inside the surrounding transaction so later parts
        // observe the previous debit/credit and sequence; failure rolls everything back.
        await db.SaveChangesAsync(ct);
    }
}
