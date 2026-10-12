using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.MonthlyFinance;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesConcurrencyTests
{
    private (MonthlyFinanceService Months,PayrollService Payroll,SalaryService Salary,ExpenseService Expenses,ShiftService Shifts,VaultService Vault) MonthlyServices(CampionsSystem.DAL.Data.ApplicationDbContext db)
    {
        var user=new TestBranchUser(new(actor,"monthly",null,null,[],[..MonthlyFinancePermissions.Codes,..ShiftPermissions.Codes,"Branches.AccessAll","Payroll.Manage"],new(branch,"Branch")));
        var access=new OperationAccess(user,db,new Clock(),Options.Create(new AuthContextOptions()));
        return(new(db,user,access),new(db,user,access),new(db,user,access),new(db,user,access),new(db,user,access),new(db,user,access));
    }
    private async Task<(Guid Month,Guid Employee)> MonthlyReady(bool closeShift=true)
    {
        await using var db=Context();var s=MonthlyServices(db);var employee=await db.Employees.SingleAsync(x=>x.UserId==actor);
        await s.Salary.ConfigureAsync(employee.Id,new(Guid.NewGuid(),1000,employee.HireDate,"Explicit salary"));
        var month=await s.Months.OpenAsync(new(Guid.NewGuid(),"Monthly",new(2026,10,10),new(2026,10,10),"Test month"));
        if(closeShift){var shift=await db.Shifts.SingleAsync();await s.Shifts.CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,1000));}
        else await s.Vault.CountAsync(new(Guid.NewGuid(),1000,"Explicit physically counted fixture cash"));
        return(month.Id,employee.Id);
    }
    [Fact] public async Task MonthlyClosingAndShiftOpeningCannotBothWin()
    {
        var f=await MonthlyReady();
        async Task<bool> Close(){await using var db=Context();return await Attempt(async()=>await MonthlyServices(db).Months.CloseAsync(f.Month,new(Guid.NewGuid(),0,"Close")));}
        async Task<bool> Open(){await using var db=Context();return await Attempt(async()=>await MonthlyServices(db).Shifts.OpenAsync(new(Guid.NewGuid(),1000)));}
        Assert.Single(await Task.WhenAll(Close(),Open()),x=>x);await using var verify=Context();
        if((await verify.FinancialMonths.SingleAsync()).Status=="Closed")Assert.False(await verify.Shifts.AnyAsync(x=>x.Status==CampionsSystem.DAL.Enums.ShiftStatus.Open));
        else Assert.True(await verify.Shifts.AnyAsync(x=>x.Status==CampionsSystem.DAL.Enums.ShiftStatus.Open));
    }
    [Fact] public async Task ConcurrentAdvanceCapAndSameOperationRetriesAreAtomic()
    {
        var f=await MonthlyReady(false);var r=new AdvanceRequest(Guid.NewGuid(),f.Employee,account,600,"Advance");
        async Task<bool> Advance(AdvanceRequest request){await using var db=Context();return await Attempt(async()=>await MonthlyServices(db).Expenses.RecordAdvanceAsync(request));}
        Assert.Single(await Task.WhenAll(Advance(r),Advance(r with{OperationId=Guid.NewGuid()})),x=>x);
        await using var verify=Context();Assert.Equal(600,await verify.EmployeeAdvances.SumAsync(x=>x.Amount));Assert.Single(await verify.FinancialTransactions.ToListAsync());
        var existing=await verify.EmployeeAdvances.SingleAsync();var retry=r with{OperationId=existing.OperationId};
        Assert.All(await Task.WhenAll(Advance(retry),Advance(retry)),Assert.True);Assert.Single(await verify.EmployeeAdvances.ToListAsync());
    }
    [Fact] public async Task ConcurrentEmployeePaymentRecordsCannotOverpayAndNeverTouchLedger()
    {
        var f=await MonthlyReady();Guid payroll;
        await using(var db=Context())
        {
            var s=MonthlyServices(db);await s.Months.CloseAsync(f.Month,new(Guid.NewGuid(),0,"Close"));payroll=(await s.Payroll.HistoryAsync()).Items.Single().Id;
            await s.Months.OpenAsync(new(Guid.NewGuid(),"Next",new(2026,10,11),new(2026,11,9),"Next"));
        }
        var r=new RecordPayrollPaymentRequest(Guid.NewGuid(),700,new Clock().GetUtcNow(),"Actually paid");
        async Task<bool> Pay(RecordPayrollPaymentRequest request){await using var db=Context();return await Attempt(async()=>await MonthlyServices(db).Payroll.RecordPaymentAsync(payroll,request));}
        Assert.Single(await Task.WhenAll(Pay(r),Pay(r with{OperationId=Guid.NewGuid()})),x=>x);
        await using var verify=Context();Assert.Equal(700,await verify.PayrollPaymentRecords.SumAsync(x=>x.Amount));Assert.Empty(await verify.FinancialTransactions.ToListAsync());
        Assert.Equal(300,(await MonthlyServices(verify).Payroll.HistoryAsync()).Items.Single().Outstanding);
    }
    [Fact] public async Task SalaryDecreaseAndAdvancePaymentCannotBreakBasicSalaryCeiling()
    {
        var f=await MonthlyReady(false);
        async Task<bool> Advance(){await using var db=Context();return await Attempt(async()=>await MonthlyServices(db).Expenses.RecordAdvanceAsync(new(Guid.NewGuid(),f.Employee,account,700,"Advance")));}
        async Task<bool> Reduce(){await using var db=Context();return await Attempt(async()=>await MonthlyServices(db).Salary.ConfigureAsync(f.Employee,new(Guid.NewGuid(),500,new(2026,10,10),"Reduce")));}
        Assert.Single(await Task.WhenAll(Advance(),Reduce()),x=>x);await using var verify=Context();
        var salary=await MonthlyServices(verify).Salary.PreviewAsync(f.Month,f.Employee);Assert.True(salary.Advances<=salary.BasicSalary);
    }
}
