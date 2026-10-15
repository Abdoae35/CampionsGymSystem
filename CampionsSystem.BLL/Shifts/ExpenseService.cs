using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Shifts;

public sealed partial class ExpenseService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private readonly FinancialOperations money = new(db,current,access);
    private async Task<ExpenseDetails> Details(Expense e,CancellationToken ct)
    {
        var category=await db.ExpenseCategories.Where(x=>x.Id==e.ExpenseCategoryId).Select(x=>x.Name).SingleAsync(ct);
        var method=await db.FinancialAccounts.Where(x=>x.Id==e.FinancialAccountId).Select(x=>x.AccountType).SingleAsync(ct);
        var returned=await db.ExpenseReversals.Where(x=>x.ExpenseId==e.Id).SumAsync(x=>x.Amount,ct);
        return new(e.Id,e.BranchId,e.ShiftId,e.RecordingEmployeeId,e.ExpenseCategoryId,category,e.FinancialAccountId,method,e.Amount,returned,e.Description,e.ReceiptReference,e.ExpenseDate,e.BusinessDate,e.PaidAt,e.Status,e.CreatedByUserId);
    }
    public Task<ExpenseDetails> RecordPaidAsync(PaidExpenseRequest r,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Money(r.Amount,true);FinancialOperations.Reason(r.Description);
        if(r.ReceiptReference?.Length>512)throw new BusinessRuleException("Receipt reference must be at most 512 characters.");
        var(branch,actor)=await money.ResolveAsync(ShiftPermissions.ExpenseCreate,r.BranchId,ct);await money.LockBranchAsync(branch,ct);
        var hash=FinancialOperations.Hash(r with{BranchId=branch});var prior=await db.Expenses.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null){FinancialOperations.Retry(prior.RequestHash,hash,prior.CreatedByUserId,actor);if(prior.BranchId!=branch)throw new UnauthorizedAccessException();return await Details(prior,ct);}
        var owner=await new OwnerAdministrationAccess(db,current).IsOwnerAsync(actor,ct);
        // Owner administrative expenses carry the real actor, never a fabricated
        // employee Shift. Everyone else retains the existing own-shift guard.
        var shift=owner?null:await money.RequireShiftAsync(branch,actor,ct);
        var recordingEmployee=owner
            ?await db.Employees.AsNoTracking().Where(x=>x.UserId==actor).Select(x=>(Guid?)x.Id).SingleOrDefaultAsync(ct)
            :shift!.EmployeeId;
        var account=await money.AccountAsync(branch,r.FinancialAccountId,ct);
        var category=await db.ExpenseCategories.FromSqlInterpolated($"SELECT * FROM \"ExpenseCategories\" WHERE \"Id\"={r.ExpenseCategoryId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if(category==null||!category.IsActive)throw new BusinessRuleException("Select an active expense category.");
        var(date,day)=await money.BusinessDateAsync(branch,ct);
        var expense=new Expense{BranchId=branch,ShiftId=shift?.Id,RecordingEmployeeId=recordingEmployee,FinancialAccountId=account.Id,ExpenseCategoryId=category.Id,
            OperationId=r.OperationId,RequestHash=hash,Amount=r.Amount,Description=r.Description,ReceiptReference=r.ReceiptReference,ExpenseDate=access.Now,PaidAt=access.Now,
            CreatedAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day,Status=ExpenseStatus.Recorded};db.Add(expense);
        await money.PostAsync(new FinancialTransaction{BranchId=branch,FinancialAccountId=account.Id,ShiftId=shift?.Id,Expense=expense,SourceType=FinancialSourceType.Expense,
            TransactionType=FinancialTransactionType.Debit,Amount=r.Amount,OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day,Notes=r.Description},ct);
        access.Audit(actor,branch,"Expense.Paid",nameof(Expense),expense.Id,after:new{r.Amount,AccountId=account.Id,account.AccountType,ShiftId=shift?.Id,RecordingEmployeeId=recordingEmployee,date,r.Description});
        return await Details(expense,ct);
    },ct);
    public Task<ExpenseReversalDetails> ReverseAsync(Guid id,ReverseExpenseRequest r,CancellationToken ct=default)=>money.TransactionAsync<ExpenseReversalDetails>(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Money(r.ReturnedAmount,true);FinancialOperations.Reason(r.Reason);
        if(!r.FundsActuallyReturned||r.ReturnReference?.Length>512)throw new BusinessRuleException("Confirm actual returned funds; a reversal cannot invent returned money.");
        var info=await db.Expenses.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new BranchNotFoundException("Expense not found.");
        var actor=await access.RequireAsync(ShiftPermissions.ExpenseReverse,info.BranchId,ct);await money.LockBranchAsync(info.BranchId,ct);
        var hash=FinancialOperations.Hash(new{id,r});var prior=await db.ExpenseReversals.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null){FinancialOperations.Retry(prior.RequestHash,hash,prior.CreatedByUserId,actor);return new(prior.Id,prior.ExpenseId,prior.BranchId,prior.ShiftId,prior.Amount,prior.Reason,prior.ReturnReference,prior.OccurredAt,prior.BusinessDate);}
        var shift=await money.RequireShiftAsync(info.BranchId,actor,ct);
        if(info.OperationId==null||info.PaidAt==null)throw new BusinessRuleException("Legacy expense payment must be explicitly reconciled before managed reversal.");
        if(await db.CoachPayouts.AnyAsync(p=>p.ExpenseId==id,ct))throw new BusinessRuleException("Coach payout returns require an explicit Owner earnings adjustment policy.");
        var returned=await db.ExpenseReversals.Where(x=>x.ExpenseId==id).SumAsync(x=>x.Amount,ct);
        if(r.ReturnedAmount>info.Amount-returned)throw new BusinessRuleException("Return exceeds the paid, not-yet-returned expense amount.");
        var account=await money.AccountAsync(info.BranchId,info.FinancialAccountId,ct);var(date,day)=await money.BusinessDateAsync(info.BranchId,ct);
        var reversal=new ExpenseReversal{ExpenseId=id,BranchId=info.BranchId,ShiftId=shift.Id,OperationId=r.OperationId,RequestHash=hash,Amount=r.ReturnedAmount,
            Reason=r.Reason,ReturnReference=r.ReturnReference,OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day};db.Add(reversal);
        var ledger=new FinancialTransaction{BranchId=info.BranchId,FinancialAccountId=account.Id,ShiftId=shift.Id,ExpenseId=id,SourceType=FinancialSourceType.Expense,
            TransactionType=FinancialTransactionType.Credit,Amount=r.ReturnedAmount,OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day,Notes=r.Reason};
        await money.PostAsync(ledger,ct);
        access.Audit(actor,info.BranchId,"Expense.ReturnRecorded",nameof(ExpenseReversal),reversal.Id,after:new{ExpenseId=id,LedgerId=ledger.Id,r.ReturnedAmount,r.Reason,ShiftId=shift.Id,date});
        return new(reversal.Id,id,info.BranchId,shift.Id,reversal.Amount,reversal.Reason,reversal.ReturnReference,reversal.OccurredAt,date);
    },ct);
    public async Task<ExpenseDetails> GetAsync(Guid id,CancellationToken ct=default)
    {
        var row=await db.Expenses.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new BranchNotFoundException("Expense not found.");
        await access.RequireAsync(ShiftPermissions.ExpenseView,row.BranchId,ct);return await Details(row,ct);
    }
    public async Task<ShiftPage<ExpenseReversalDetails>> ReturnsAsync(Guid id,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        if(page<1||pageSize is <1 or >100)throw new BusinessRuleException("Invalid pagination.");
        var branch=await db.Expenses.Where(x=>x.Id==id).Select(x=>(Guid?)x.BranchId).SingleOrDefaultAsync(ct)??throw new BranchNotFoundException("Expense not found.");
        await access.RequireAsync(ShiftPermissions.ExpenseView,branch,ct);
        var q=db.ExpenseReversals.AsNoTracking().Where(x=>x.ExpenseId==id);
        var total=await q.CountAsync(ct);var items=await q.OrderByDescending(x=>x.OccurredAt).ThenBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize)
            .Select(x=>new ExpenseReversalDetails(x.Id,x.ExpenseId,x.BranchId,x.ShiftId,x.Amount,x.Reason,x.ReturnReference,x.OccurredAt,x.BusinessDate)).ToListAsync(ct);
        return new(items,total,page,pageSize);
    }
    public async Task<ShiftPage<ExpenseDetails>> HistoryAsync(VaultHistoryRequest r,CancellationToken ct=default)
    {
        if(r.Page<1||r.PageSize is <1 or >100||r.From>r.To)throw new BusinessRuleException("Invalid history range or pagination.");
        var(branch,_)=await money.ResolveAsync(ShiftPermissions.ExpenseView,r.BranchId,ct);var q=db.Expenses.AsNoTracking().Where(x=>x.BranchId==branch);
        if(r.From!=null)q=q.Where(x=>x.BusinessDate>=r.From);if(r.To!=null)q=q.Where(x=>x.BusinessDate<=r.To);if(r.AccountId!=null)q=q.Where(x=>x.FinancialAccountId==r.AccountId);
        var total=await q.CountAsync(ct);var rows=await q.OrderByDescending(x=>x.ExpenseDate).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize).ToListAsync(ct);
        var items=new List<ExpenseDetails>();foreach(var row in rows)items.Add(await Details(row,ct));return new(items,total,r.Page,r.PageSize);
    }
    public async Task<ShiftPage<ExpenseCategoryDetails>> CategoriesAsync(int page=1,int pageSize=25,CancellationToken ct=default)
    {
        await access.RequireAsync(ShiftPermissions.ExpenseView,null,ct);
        if(page<1||pageSize is <1 or >100)throw new BusinessRuleException("Invalid category pagination.");
        var q=db.ExpenseCategories.AsNoTracking();var total=await q.CountAsync(ct);
        var items=await q.OrderBy(x=>x.Name).ThenBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize).Select(x=>new ExpenseCategoryDetails(x.Id,x.Name,x.IsActive)).ToListAsync(ct);
        return new(items,total,page,pageSize);
    }
    public Task<ExpenseCategoryDetails> SetCategoryAsync(Guid? id,ExpenseCategoryRequest r,CancellationToken ct=default)=>money.TransactionAsync<ExpenseCategoryDetails>(async()=>
    {
        await access.RequireAsync(ShiftPermissions.ExpenseManage,null,ct);var actor=await access.RequireAsync("Branches.AccessAll",null,ct);
        if(string.IsNullOrWhiteSpace(r.Name)||r.Name.Trim().Length>256)throw new BusinessRuleException("A category name of up to 256 characters is required.");
        var row=id==null?new ExpenseCategory():await db.ExpenseCategories.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new BranchNotFoundException("Category not found.");
        var before=new{row.Name,row.IsActive};row.Name=r.Name.Trim();row.IsActive=r.IsActive;if(id==null)db.Add(row);
        access.Audit(actor,null,id==null?"ExpenseCategory.Created":"ExpenseCategory.Updated",nameof(ExpenseCategory),row.Id,before,new{row.Name,row.IsActive});
        return new(row.Id,row.Name,row.IsActive);
    },ct);
}
