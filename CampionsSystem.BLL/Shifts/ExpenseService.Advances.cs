using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Shifts;
public sealed partial class ExpenseService
{
    private async Task<AdvanceDetails> Details(EmployeeAdvance a,CancellationToken ct)=>new(a.Id,a.BranchId,a.ShiftId,a.ReceivingEmployeeId,
        await db.Employees.Where(x=>x.Id==a.ReceivingEmployeeId).Select(x=>x.FullName).SingleAsync(ct),a.RecordingEmployeeId,a.FinancialAccountId,
        await db.FinancialAccounts.Where(x=>x.Id==a.FinancialAccountId).Select(x=>x.AccountType).SingleAsync(ct),a.Amount,a.Reason,a.Reference,a.OccurredAt,a.BusinessDate);
    public Task<AdvanceDetails> RecordAdvanceAsync(AdvanceRequest r,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Money(r.Amount,true);FinancialOperations.Reason(r.Reason);
        if(r.Reference?.Length>512)throw new BusinessRuleException("Reference must be at most 512 characters.");
        var(branch,actor)=await money.ResolveAsync(ShiftPermissions.AdvanceCreate,r.BranchId,ct);await money.LockBranchAsync(branch,ct);
        var hash=FinancialOperations.Hash(r with{BranchId=branch});var prior=await db.EmployeeAdvances.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null){FinancialOperations.Retry(prior.RequestHash,hash,prior.CreatedByUserId,actor);if(prior.BranchId!=branch)throw new UnauthorizedAccessException();return await Details(prior,ct);}
        var shift=await money.RequireShiftAsync(branch,actor,ct);var account=await money.AccountAsync(branch,r.FinancialAccountId,ct);
        var(date,day)=await money.BusinessDateAsync(branch,ct);
        var recipient=await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\"={r.ReceivingEmployeeId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if(recipient==null||recipient.EmploymentStatus!=EmploymentStatus.Active||recipient.HireDate>date||recipient.TerminationDate<=date)
            throw new BusinessRuleException("Select an active receiving employee.");
        var advance=new EmployeeAdvance{BranchId=branch,ShiftId=shift.Id,ReceivingEmployeeId=recipient.Id,RecordingEmployeeId=shift.EmployeeId!.Value,
            FinancialAccountId=account.Id,OperationId=r.OperationId,RequestHash=hash,Amount=r.Amount,Reason=r.Reason,Reference=r.Reference,
            OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day};db.Add(advance);
        await money.PostAsync(new FinancialTransaction{BranchId=branch,FinancialAccountId=account.Id,ShiftId=shift.Id,EmployeeAdvance=advance,
            SourceType=FinancialSourceType.EmployeeAdvance,TransactionType=FinancialTransactionType.Debit,Amount=r.Amount,OccurredAt=access.Now,CreatedByUserId=actor,
            BusinessDate=date,BusinessDayId=day,Notes=r.Reason},ct);
        access.Audit(actor,branch,"EmployeeAdvance.Paid",nameof(EmployeeAdvance),advance.Id,after:new{r.ReceivingEmployeeId,advance.RecordingEmployeeId,r.Amount,account.AccountType,ShiftId=shift.Id,date});
        return await Details(advance,ct);
    },ct);
    public async Task<AdvanceDetails> GetAdvanceAsync(Guid id,CancellationToken ct=default)
    {
        var row=await db.EmployeeAdvances.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new BranchNotFoundException("Advance not found.");
        await access.RequireAsync(ShiftPermissions.AdvanceView,row.BranchId,ct);return await Details(row,ct);
    }
    public async Task<ShiftPage<AdvanceDetails>> AdvancesAsync(VaultHistoryRequest r,CancellationToken ct=default)
    {
        if(r.Page<1||r.PageSize is <1 or >100||r.From>r.To)throw new BusinessRuleException("Invalid history range or pagination.");
        var(branch,_)=await money.ResolveAsync(ShiftPermissions.AdvanceView,r.BranchId,ct);var q=db.EmployeeAdvances.AsNoTracking().Where(x=>x.BranchId==branch);
        if(r.From!=null)q=q.Where(x=>x.BusinessDate>=r.From);if(r.To!=null)q=q.Where(x=>x.BusinessDate<=r.To);if(r.AccountId!=null)q=q.Where(x=>x.FinancialAccountId==r.AccountId);
        var total=await q.CountAsync(ct);var rows=await q.OrderByDescending(x=>x.OccurredAt).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize).ToListAsync(ct);
        var items=new List<AdvanceDetails>();foreach(var row in rows)items.Add(await Details(row,ct));return new(items,total,r.Page,r.PageSize);
    }
}
