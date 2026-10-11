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
        var branch=await db.Shifts.Where(x=>x.Id==id).Select(x=>(Guid?)x.BranchId).SingleOrDefaultAsync(ct)??throw new BranchNotFoundException("Shift not found.");
        await access.RequireAsync(ShiftPermissions.View,branch,ct); await access.RequireAsync(ShiftPermissions.VaultView,branch,ct);
        await money.LockBranchAsync(branch,ct);var shift=await new ShiftService(db,current,access).GetAsync(id,ct);
        // Only explicit associations count as employee activity; overlapping time windows
        // and legacy null links never assign another employee's money to this shift.
        return new ShiftFinancialSummary(shift,await Totals(db.FinancialTransactions.AsNoTracking().Where(x=>x.BranchId==shift.BranchId&&x.ShiftId==id),ct),await money.PositionAsync(shift.BranchId,ct));
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
        return new(items,total,page,pageSize);
    }
}
