using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Shifts;
public sealed partial class ExpenseService
{
    private async Task<AdvanceDetails> Details(EmployeeAdvance a,CancellationToken ct)=>(await AdvanceDetailsMany([a],ct))[0];
    private async Task<AdvanceDetails[]> AdvanceDetailsMany(IReadOnlyList<EmployeeAdvance> rows,CancellationToken ct)
    {
        if(rows.Count==0)return [];
        var employeeIds=rows.SelectMany(x=>new[]{x.ReceivingEmployeeId,x.RecordingEmployeeId}).Distinct().ToArray();
        var employees=await db.Employees.AsNoTracking().Where(x=>employeeIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.FullName}).ToDictionaryAsync(x=>x.Id,x=>x.FullName,ct);
        var accountIds=rows.Select(x=>x.FinancialAccountId).Distinct().ToArray();
        var accounts=await db.FinancialAccounts.AsNoTracking().Where(x=>accountIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.AccountType,x.Name}).ToDictionaryAsync(x=>x.Id,ct);
        return rows.Select(a=>new AdvanceDetails(a.Id,a.BranchId,a.ShiftId,a.ReceivingEmployeeId,
            employees[a.ReceivingEmployeeId],a.RecordingEmployeeId,a.FinancialAccountId,
            accounts[a.FinancialAccountId].AccountType,a.Amount,a.Reason,a.Reference,a.OccurredAt,a.BusinessDate,
            employees.GetValueOrDefault(a.RecordingEmployeeId),accounts[a.FinancialAccountId].Name)).ToArray();
    }
    public Task<AdvanceDetails> RecordAdvanceAsync(AdvanceRequest r,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Money(r.Amount,true);FinancialOperations.Reason(r.Reason);
        if(r.Reference?.Length>512)throw new BusinessRuleException("Reference must be at most 512 characters.");
        var(branch,actor)=await money.ResolveAsync(ShiftPermissions.AdvanceCreate,r.BranchId,ct);await money.LockBranchAsync(branch,ct);
        var hash=FinancialOperations.Hash(r with{BranchId=branch});var prior=await db.EmployeeAdvances.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null){FinancialOperations.Retry(prior.RequestHash,hash,prior.CreatedByUserId,actor);if(prior.BranchId!=branch)throw new UnauthorizedAccessException();return await Details(prior,ct);}
        var shift=await money.RequireShiftAsync(branch,actor,ct);var account=await money.AccountAsync(branch,r.FinancialAccountId,ct);
        var(date,day)=await money.BusinessDateAsync(branch,ct);
        var recipient=await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\"={r.ReceivingEmployeeId} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct);
        if(recipient==null||recipient.EmploymentStatus!=EmploymentStatus.Active||recipient.HireDate>date||recipient.TerminationDate<=date)
            throw new BusinessRuleException("Select an active receiving employee.");
        var month=await CampionsSystem.BLL.MonthlyFinance.FinancialMonthPosting.ResolveAsync(db,branch,date,ct)
            ??throw new BusinessRuleException("Configure the branch financial month and employee salary before paying advances.");
        var entitlement=await CampionsSystem.BLL.MonthlyFinance.SalaryProjection.ForEmployeeAsync(db,recipient,month,ct,access:access);
        if(r.Amount>entitlement.BasicSalary-entitlement.Advances)throw new BusinessRuleException("Advance exceeds remaining prorated BASIC salary entitlement for this branch/period; commission does not increase the limit.");
        var advance=new EmployeeAdvance{FinancialMonthId=month.Id,BasicEntitlementSnapshot=entitlement.BasicSalary,BranchId=branch,ShiftId=shift.Id,ReceivingEmployeeId=recipient.Id,RecordingEmployeeId=shift.EmployeeId!.Value,
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
        return new(await AdvanceDetailsMany(rows,ct),total,r.Page,r.PageSize);
    }
}
