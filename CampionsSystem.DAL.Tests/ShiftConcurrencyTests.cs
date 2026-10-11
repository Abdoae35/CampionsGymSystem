using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesConcurrencyTests
{
    private (ShiftService Shifts,ExpenseService Expenses,VaultService Vault) Operational(ApplicationDbContext db,Guid? actorId=null)
    {
        var user=new TestBranchUser(new(actorId??actor,"operator",null,null,[],[..SalesPermissions.Codes,..ShiftPermissions.Codes,"Branches.AccessAll"],new(branch,"Branch")));
        var access=new OperationAccess(user,db,new Clock(),Options.Create(new AuthContextOptions()));return(new(db,user,access),new(db,user,access),new(db,user,access));
    }
    private async Task<bool> Attempt(Func<Task> operation)
    {try{await operation();return true;}catch(BusinessRuleException){return false;}}
    [Fact] public async Task ShiftCloseAndCashSaleRaceIncludesOnlyCommittedPriorMovement()
    {
        Guid shift;long version;await using(var db=Context()){var row=await db.Shifts.SingleAsync();shift=row.Id;version=row.Version;}
        var op=Guid.NewGuid();var payment=Attempt(async()=>await Buy(op));
        async Task Close(){await using var db=Context();await Operational(db).Shifts.CloseAsync(shift,new(Guid.NewGuid(),version,0));}
        await Task.WhenAll(payment,Close());await using var verify=Context();var closed=await verify.Shifts.SingleAsync();
        Assert.Equal(await payment?1200:0,closed.ExpectedClosingCash);Assert.Equal(ShiftStatus.Closed,closed.Status);
        Assert.Equal(await payment?1:0,await verify.Payments.CountAsync());
        if(await payment)Assert.Equal((await verify.SaleTransactions.SingleAsync()).Id,(await Buy(op)).SaleId);
    }
    [Fact] public async Task ConcurrentPayoutsCannotSpendFinalPhysicalCashTwice()
    {
        await Buy(Guid.NewGuid());Guid category;await using(var db=Context()){var c=new ExpenseCategory{Name="Operations"};db.Add(c);await db.SaveChangesAsync();category=c.Id;}
        async Task<bool> Pay(){await using var db=Context();return await Attempt(async()=>await Operational(db).Expenses.RecordPaidAsync(new(Guid.NewGuid(),category,account,700,"Actual payout")));}
        Assert.Single(await Task.WhenAll(Pay(),Pay()),x=>x);await using var verify=Context();
        Assert.Single(await verify.Expenses.ToListAsync());Assert.Equal(500,(await Operational(verify).Vault.PositionAsync()).ExpectedPhysicalCash);
    }
    [Fact] public async Task ConcurrentDuplicateExpenseReturnCreatesOneCompensatingFact()
    {
        await Buy(Guid.NewGuid());Guid expense;await using(var db=Context()){var c=new ExpenseCategory{Name="Operations"};db.Add(c);await db.SaveChangesAsync();expense=(await Operational(db).Expenses.RecordPaidAsync(new(Guid.NewGuid(),c.Id,account,700,"Purchase"))).Id;}
        var r=new ReverseExpenseRequest(Guid.NewGuid(),100,true,"Actual returned cash");
        async Task<ExpenseReversalDetails> Return(){await using var db=Context();return await Operational(db).Expenses.ReverseAsync(expense,r);}
        var results=await Task.WhenAll(Return(),Return());Assert.Equal(results[0].Id,results[1].Id);await using var verify=Context();
        Assert.Single(await verify.ExpenseReversals.ToListAsync());Assert.Equal(3,await verify.FinancialTransactions.CountAsync());Assert.Equal(600,(await Operational(verify).Vault.PositionAsync()).ExpectedPhysicalCash);
    }
    [Fact] public async Task EmployeeOpenRaceCannotCreateTwoOwnedShifts()
    {
        await using(var db=Context()){var shift=await db.Shifts.SingleAsync();await Operational(db).Shifts.CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,0));}
        async Task<bool> Open(){await using var db=Context();return await Attempt(async()=>await Operational(db).Shifts.OpenAsync(new(Guid.NewGuid(),0)));}
        Assert.Single(await Task.WhenAll(Open(),Open()),x=>x);await using var verify=Context();Assert.Single(await verify.Shifts.Where(x=>x.Status==ShiftStatus.Open).ToListAsync());
    }
    [Fact] public async Task ConcurrentRefundExpenseAdvanceAndCloseUseOneDrawerBoundary()
    {
        var sale=await Buy(Guid.NewGuid());Guid category,receiver,shift;long version;
        await using(var db=Context()){var c=new ExpenseCategory{Name="Operations"};db.Add(c);await db.SaveChangesAsync();category=c.Id;receiver=(await db.Employees.SingleAsync(x=>x.UserId==actor)).Id;var row=await db.Shifts.SingleAsync();shift=row.Id;version=row.Version;}
        async Task<bool> Refund(){await using var db=Context();return await Attempt(async()=>await Services(db).Sales.RefundAsync(sale.SaleId,new(Guid.NewGuid(),400,[new(account,400)],"Decision")));}
        async Task<bool> Expense(){await using var db=Context();return await Attempt(async()=>await Operational(db).Expenses.RecordPaidAsync(new(Guid.NewGuid(),category,account,300,"Actual expense")));}
        async Task<bool> Advance(){await using var db=Context();return await Attempt(async()=>await Operational(db).Expenses.RecordAdvanceAsync(new(Guid.NewGuid(),receiver,account,200,"Actual advance")));}
        async Task Close(){await using var db=Context();await Operational(db).Shifts.CloseAsync(shift,new(Guid.NewGuid(),version,1200));}
        var refund=Refund();var expense=Expense();var advance=Advance();await Task.WhenAll(refund,expense,advance,Close());
        await using var verify=Context();var closed=await verify.Shifts.SingleAsync();var paid=(await refund?400:0)+(await expense?300:0)+(await advance?200:0);
        Assert.Equal(1200-paid,closed.ExpectedClosingCash);Assert.Equal(paid,closed.Difference);
        Assert.Equal(paid,await verify.FinancialTransactions.Where(x=>x.TransactionType==FinancialTransactionType.Debit).SumAsync(x=>x.Amount));
        Assert.Equal(1200-paid,(await Operational(verify).Vault.PositionAsync()).BookCash);Assert.Equal(1200,(await Operational(verify).Vault.PositionAsync()).ExpectedPhysicalCash);
    }
    [Fact] public async Task TransferAndShiftOpeningCannotStrandOpenEmployeeInOldBranch()
    {
        Guid employee,target;await using(var db=Context())
        {
            var shift=await db.Shifts.SingleAsync();employee=shift.EmployeeId!.Value;
            await Operational(db).Shifts.CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,0));
            var old=await db.GymBranches.SingleAsync();var next=new GymBranch{Name="Transfer destination",CityId=old.CityId};target=next.Id;db.Add(next);await db.SaveChangesAsync();
        }
        async Task<bool> Open(){await using var db=Context();return await Attempt(async()=>await Operational(db).Shifts.OpenAsync(new(Guid.NewGuid(),0)));}
        async Task<bool> Transfer()
        {
            await using var db=Context();var user=new TestBranchUser(new(actor,"transfer",null,null,[],["Employees.Transfer","Branches.AccessAll"],new(branch,"Branch")));
            var access=new OperationAccess(user,db,new Clock(),Options.Create(new AuthContextOptions()));
            return await Attempt(async()=>await new CampionsSystem.BLL.Branches.EmployeeBranchAssignmentService(db,user,access).TransferAsync(employee,new(target,access.Today)));
        }
        var open=Open();var transfer=Transfer();await Task.WhenAll(open,transfer);Assert.NotEqual(await open,await transfer);
        await using var verify=Context();var active=await verify.Shifts.SingleOrDefaultAsync(x=>x.Status==ShiftStatus.Open);
        var assignment=await verify.EmployeeBranchAssignments.SingleAsync(x=>x.EmployeeId==employee&&x.EndDate==null);
        if(active!=null)Assert.Equal(active.BranchId,assignment.BranchId);else Assert.Equal(target,assignment.BranchId);
    }
}
