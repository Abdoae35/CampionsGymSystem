using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Shifts;

// Operational collections/payouts, not commercial sales revenue or commission.
public sealed class ShiftReportService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private readonly FinancialOperations money = new(db,current,access);
    private static IQueryable<FinancialTransaction> DuringShift(IQueryable<FinancialTransaction> transactions,
        ShiftDetails shift, DateTimeOffset cutoff)
    {
        // Book balances change when money is posted. OccurredAt remains the
        // original event date for late-discovered operations; legacy rows without
        // PostedAt use their only persisted timestamp as an explicit fallback.
        return transactions.Where(x=>x.BranchId==shift.BranchId
            &&(x.PostedAt??x.OccurredAt)>=shift.OpenedAt
            &&(x.PostedAt??x.OccurredAt)<=cutoff);
    }
    private async Task<ShiftDetails> ReviewShiftAsync(Guid id,CancellationToken ct)
    {
        var branch=await db.Shifts.AsNoTracking().Where(x=>x.Id==id).Select(x=>(Guid?)x.BranchId).SingleOrDefaultAsync(ct)
            ??throw new BranchNotFoundException("Shift not found.");
        await access.RequireAsync(ShiftPermissions.View,branch,ct);
        await access.RequireAsync(ShiftPermissions.VaultView,branch,ct);
        await money.LockBranchAsync(branch,ct);
        return await new ShiftService(db,current,access).GetAsync(id,ct);
    }
    private async Task<Dictionary<Guid,string>> OperatorNamesAsync(Guid[] ids,CancellationToken ct)
    {
        var names=await db.Users.AsNoTracking().Where(x=>ids.Contains(x.Id))
            .ToDictionaryAsync(x=>x.Id,x=>x.UserName??"Unknown operator",ct);
        var employees=await db.Employees.AsNoTracking().Where(x=>x.UserId!=null&&ids.Contains(x.UserId.Value))
            .Select(x=>new{x.UserId,x.FullName}).ToListAsync(ct);
        foreach(var employee in employees)names[employee.UserId!.Value]=employee.FullName;
        return names;
    }
    private async Task<ShiftBranchPeriodSummary> BranchPeriodAsync(ShiftDetails shift,DrawerPosition currentDrawer,
        IReadOnlyList<ShiftAccountBalance> currentAccounts,CancellationToken ct)
    {
        var historical=shift.ClosedAt.HasValue;
        var cutoff=shift.ClosedAt??access.Now;
        var all=db.FinancialTransactions.AsNoTracking().Where(x=>x.BranchId==shift.BranchId);
        var period=DuringShift(all,shift,cutoff);
        var previous=all.Where(x=>(x.PostedAt??x.OccurredAt)<shift.OpenedAt);
        var openingMovements=await previous.GroupBy(x=>x.FinancialAccountId)
            .Select(g=>new{AccountId=g.Key,Net=g.Sum(x=>x.TransactionType==FinancialTransactionType.Credit?x.Amount:-x.Amount)})
            .ToDictionaryAsync(x=>x.AccountId,x=>x.Net,ct);
        var accountMovements=await period.GroupBy(x=>x.FinancialAccountId)
            .Select(g=>new{AccountId=g.Key,Inflows=g.Where(x=>x.TransactionType==FinancialTransactionType.Credit).Sum(x=>x.Amount),
                Outflows=g.Where(x=>x.TransactionType==FinancialTransactionType.Debit).Sum(x=>x.Amount)})
            .ToDictionaryAsync(x=>x.AccountId,ct);
        var accountRows=await db.FinancialAccounts.AsNoTracking().Where(x=>x.BranchId==shift.BranchId)
            .Select(x=>new{x.Id,x.AccountType,x.OpeningBalance,x.CreatedAt}).ToListAsync(ct);
        if(historical)
        {
            var snapshotIds=shift.ClosingAccountBalances?.Select(x=>x.AccountId).ToHashSet();
            accountRows=accountRows.Where(x=>snapshotIds?.Contains(x.Id)??x.CreatedAt<=cutoff).ToList();
        }
        var closing=(historical?shift.ClosingAccountBalances:currentAccounts)?.ToDictionary(x=>x.AccountId,x=>x.BookBalance);
        var accounts=accountRows.OrderBy(x=>x.AccountType).ThenBy(x=>x.Id).Select(x=>
        {
            var opening=x.OpeningBalance+openingMovements.GetValueOrDefault(x.Id);
            accountMovements.TryGetValue(x.Id,out var movement);
            var calculated=opening+(movement?.Inflows??0)-(movement?.Outflows??0);
            decimal? expected=closing!=null&&closing.TryGetValue(x.Id,out var value)?value:null;
            return new ShiftBranchAccountReconciliation(x.Id,x.AccountType,opening,movement?.Inflows??0,movement?.Outflows??0,
                calculated,expected,expected.HasValue?calculated==expected.Value:null);
        }).ToArray();
        var operatorRows=await period.GroupBy(x=>new{x.CreatedByUserId,x.FinancialAccount.AccountType})
            .Select(g=>new{ActorId=g.Key.CreatedByUserId,Method=g.Key.AccountType,Count=g.Count(),
                Inflows=g.Where(x=>x.TransactionType==FinancialTransactionType.Credit).Sum(x=>x.Amount),
                Outflows=g.Where(x=>x.TransactionType==FinancialTransactionType.Debit).Sum(x=>x.Amount)})
            .ToListAsync(ct);
        var names=await OperatorNamesAsync(operatorRows.Select(x=>x.ActorId).Distinct().ToArray(),ct);
        var operators=operatorRows.OrderBy(x=>names.GetValueOrDefault(x.ActorId,"Unknown operator"))
            .ThenBy(x=>x.ActorId).ThenBy(x=>x.Method)
            .Select(x=>new ShiftBranchOperatorTotals(x.ActorId,names.GetValueOrDefault(x.ActorId,"Unknown operator"),x.Method,
                x.Count,x.Inflows,x.Outflows,x.Inflows-x.Outflows)).ToArray();
        var limitations=new List<string>();
        if(historical&&shift.ClosingAccountBalances==null)limitations.Add("Legacy closed Shift has no frozen account snapshot; historical book balances cannot be verified.");
        if(historical&&accountMovements.Keys.Any(id=>accounts.All(x=>x.AccountId!=id)))limitations.Add("A historical ledger row refers to an account absent from the frozen closure snapshot.");
        if(historical&&await period.AnyAsync(x=>x.PostedAt==null,ct))limitations.Add("Some legacy transactions have no posting timestamp, so their presence at closure cannot be proven.");
        if(historical&&await all.AnyAsync(x=>(x.PostedAt??x.OccurredAt)==cutoff,ct))limitations.Add("Transactions sharing the exact closure timestamp cannot be ordered against closure for every account.");
        if(accounts.Any(x=>x.MatchesExpectedClosingBalance==false))limitations.Add("Calculated period movement does not reconcile to one or more frozen closing account balances; use the closure snapshot as authoritative.");
        return new(shift.OpenedAt,cutoff,historical,await period.CountAsync(ct),await Totals(period,ct),operators,accounts,
            historical?shift.ExpectedClosingCash:currentDrawer.ExpectedPhysicalCash,shift.ActualClosingCash,limitations);
    }
    private async Task<IReadOnlyList<MethodFinancialTotals>> Totals(IQueryable<FinancialTransaction> q,CancellationToken ct)
    {
        var rows=await q.GroupBy(x=>new{x.FinancialAccount.AccountType,x.SourceType,x.TransactionType,Legacy=x.ShiftId==null})
            .Select(g=>new{g.Key.AccountType,g.Key.SourceType,g.Key.TransactionType,g.Key.Legacy,Amount=g.Sum(x=>x.Amount)}).ToListAsync(ct);
        return new[]{FinancialAccountType.Cash,FinancialAccountType.InstaPay}.Select(method=>
        {
            var selected=rows.Where(x=>x.AccountType==method).ToArray();
            decimal Sum(FinancialSourceType source,FinancialTransactionType direction)=>selected.Where(x=>x.SourceType==source&&x.TransactionType==direction).Sum(x=>x.Amount);
            decimal Signed(bool legacyOnly=false)=>selected.Where(x=>!legacyOnly||x.Legacy).Sum(x=>x.TransactionType==FinancialTransactionType.Credit?x.Amount:-x.Amount);
            var known=new[]{FinancialSourceType.Payment,FinancialSourceType.Refund,FinancialSourceType.Expense,FinancialSourceType.EmployeeAdvance,FinancialSourceType.CashMovement};
            return new MethodFinancialTotals(method,Sum(FinancialSourceType.Payment,FinancialTransactionType.Credit),Sum(FinancialSourceType.Refund,FinancialTransactionType.Debit),
                Sum(FinancialSourceType.Expense,FinancialTransactionType.Debit),Sum(FinancialSourceType.Expense,FinancialTransactionType.Credit),
                Sum(FinancialSourceType.EmployeeAdvance,FinancialTransactionType.Debit),Sum(FinancialSourceType.CashMovement,FinancialTransactionType.Debit),
                Sum(FinancialSourceType.CashMovement,FinancialTransactionType.Credit),
                selected.Where(x=>!known.Contains(x.SourceType)&&x.TransactionType==FinancialTransactionType.Credit).Sum(x=>x.Amount),
                selected.Where(x=>!known.Contains(x.SourceType)&&x.TransactionType==FinancialTransactionType.Debit).Sum(x=>x.Amount),Signed(),Signed(true));
        }).ToArray();
    }
    public Task<ShiftFinancialSummary> ShiftAsync(Guid id,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        var shift=await ReviewShiftAsync(id,ct);
        // Only explicit associations count as employee activity; overlapping time windows
        // and legacy null links never assign another employee's money to this shift.
        var drawer=await money.PositionAsync(shift.BranchId,ct);
        var accounts=await money.AccountBalancesAsync(shift.BranchId,ct);
        return new ShiftFinancialSummary(shift,
            await Totals(db.FinancialTransactions.AsNoTracking().Where(x=>x.BranchId==shift.BranchId&&x.ShiftId==id),ct),
            drawer,accounts,await BranchPeriodAsync(shift,drawer,accounts,ct));
    },ct);
    public Task<ShiftPage<ShiftFinancialTransactionDetails>> TransactionsAsync(Guid id,int page=1,int pageSize=25,CancellationToken ct=default)
        =>money.TransactionAsync(async()=>
    {
        if(page<1||pageSize is <1 or >100||(long)(page-1)*pageSize>int.MaxValue)
            throw new BusinessRuleException("Invalid shift transaction query.");
        var shift=await ReviewShiftAsync(id,ct);
        var branch=shift.BranchId;
        var query=db.FinancialTransactions.AsNoTracking().Where(x=>x.BranchId==branch&&x.ShiftId==id);
        var total=await query.CountAsync(ct);
        var items=await query.OrderByDescending(x=>x.OccurredAt).ThenBy(x=>x.Id)
            .Skip((page-1)*pageSize).Take(pageSize)
            .Select(x=>new ShiftFinancialTransactionDetails(x.Id,id,x.FinancialAccountId,x.FinancialAccount.AccountType,
                x.SourceType,x.TransactionType,x.Amount,x.OccurredAt,x.BusinessDate,x.CreatedByUserId,
                x.PaymentId,x.RefundId,x.ExpenseId,x.EmployeeAdvanceId,x.CashMovementId,x.PayrollId,
                db.Employees.Where(e=>e.UserId==x.CreatedByUserId).Select(e=>e.FullName).FirstOrDefault()??x.CreatedByUser.UserName??"Unknown operator")).ToListAsync(ct);
        return new ShiftPage<ShiftFinancialTransactionDetails>(items,total,page,pageSize);
    },ct);
    public Task<ShiftPage<ShiftBranchTransactionDetails>> BranchTransactionsAsync(Guid id,int page=1,int pageSize=25,CancellationToken ct=default)
        =>money.TransactionAsync(async()=>
    {
        if(page<1||pageSize is <1 or >100||(long)(page-1)*pageSize>int.MaxValue)
            throw new BusinessRuleException("Invalid shift branch transaction query.");
        var shift=await ReviewShiftAsync(id,ct);
        var cutoff=shift.ClosedAt??access.Now;
        var query=DuringShift(db.FinancialTransactions.AsNoTracking(),shift,cutoff);
        var total=await query.CountAsync(ct);
        var items=await query.OrderByDescending(x=>x.PostedAt??x.OccurredAt).ThenBy(x=>x.Id)
            .Skip((page-1)*pageSize).Take(pageSize)
            .Select(x=>new ShiftBranchTransactionDetails(x.Id,x.ShiftId,x.FinancialAccountId,x.FinancialAccount.AccountType,
                x.SourceType,x.TransactionType,x.Amount,x.OccurredAt,x.PostedAt,x.BusinessDate,x.CreatedByUserId,
                db.Employees.Where(e=>e.UserId==x.CreatedByUserId).Select(e=>e.FullName).FirstOrDefault()??x.CreatedByUser.UserName??"Unknown operator",
                x.PaymentId,x.RefundId,x.ExpenseId,x.EmployeeAdvanceId,x.CashMovementId,x.PayrollId)).ToListAsync(ct);
        return new ShiftPage<ShiftBranchTransactionDetails>(items,total,page,pageSize);
    },ct);
    public Task<BranchFinancialSummary> BranchAsync(Guid? branchId=null,DateOnly? from=null,DateOnly? to=null,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        if(from>to)throw new BusinessRuleException("Invalid business date range.");
        var(branch,_)=await money.ResolveAsync(ShiftPermissions.VaultView,branchId,ct); await money.LockBranchAsync(branch,ct);
        var q=db.FinancialTransactions.AsNoTracking().Where(x=>x.BranchId==branch);
        if(from!=null)q=q.Where(x=>x.BusinessDate>=from); if(to!=null)q=q.Where(x=>x.BusinessDate<=to);
        return new BranchFinancialSummary(branch,from,to,await Totals(q,ct),await money.PositionAsync(branch,ct));
    },ct);
    public async Task<ShiftPage<FinancialAuditDetails>> AuditAsync(Guid? branchId=null,Guid? entityId=null,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        if(page<1||pageSize is <1 or >100)throw new BusinessRuleException("Invalid audit query.");
        var(branch,_)=await money.ResolveAsync(ShiftPermissions.VaultView,branchId,ct);
        var q=db.AuditLogs.AsNoTracking().Where(x=>x.BranchId==branch&&(x.Action.StartsWith("Shift.")||x.Action.StartsWith("Vault.")||x.Action.StartsWith("Expense.")||x.Action.StartsWith("EmployeeAdvance.")||x.Action.StartsWith("BusinessDay.")));
        if(entityId!=null)q=q.Where(x=>x.EntityId==entityId);
        var total=await q.CountAsync(ct);
        var items=await q.OrderByDescending(x=>x.CreatedAt).ThenBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize)
            .Select(x=>new FinancialAuditDetails(x.Id,x.Action,x.EntityType,x.EntityId,x.UserId,x.CreatedAt,x.OldValues,x.NewValues)).ToListAsync(ct);
        var names=await ActorDisplayNames.LoadAsync(db,items.Select(x=>x.ActorId),ct);
        return new(items.Select(x=>x with{ActorName=ActorDisplayNames.Get(names,x.ActorId)}).ToList(),total,page,pageSize);
    }
}
