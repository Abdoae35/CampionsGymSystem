using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class EmployeeShiftTests
{
    [Theory][InlineData(FinancialAccountType.Cash)][InlineData(FinancialAccountType.InstaPay)]
    public async Task DirectAdvanceIsDistinctPayoutWithRecipientRecorderAndNoPayrollOrExpense(FinancialAccountType type)
    {
        var shift=await Service().OpenAsync(new(Guid.NewGuid(),100000));var account=cash;
        if(type==FinancialAccountType.InstaPay){account=new(){BranchId=branch.Id,Name="InstaPay",AccountType=type};db.Add(account);await db.SaveChangesAsync();}
        user.Context=user.Context! with{Permissions=[ShiftPermissions.AdvanceCreate,ShiftPermissions.AdvanceView,ShiftPermissions.VaultView,ShiftPermissions.Manage,ShiftPermissions.View]};
        var r=new AdvanceRequest(Guid.NewGuid(),eb.Id,account.Id,500,"Employee advance","advance-ref");var result=await Expenses().RecordAdvanceAsync(r);
        Assert.Equal(eb.Id,result.ReceivingEmployeeId);Assert.Equal(ea.Id,result.RecordingEmployeeId);Assert.Equal(type,result.Method);Assert.Equal(shift.Id,result.ShiftId);
        Assert.Equal(result.Id,(await Expenses().RecordAdvanceAsync(r)).Id);var ledger=await db.FinancialTransactions.SingleAsync();Assert.Equal(FinancialSourceType.EmployeeAdvance,ledger.SourceType);
        Assert.Equal(result.Id,ledger.EmployeeAdvanceId);Assert.Null(ledger.ExpenseId);Assert.Null(ledger.PayrollId);
        Assert.Empty(await db.Expenses.ToListAsync());Assert.Empty(await db.Payrolls.ToListAsync());Assert.Empty(await db.PayrollDeductions.ToListAsync());
        Assert.Equal(type==FinancialAccountType.Cash?99500:100000,(await Vault().PositionAsync()).ExpectedPhysicalCash);
        await Service().CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,type==FinancialAccountType.Cash?99500:100000));Assert.Equal(result.Id,(await Expenses().RecordAdvanceAsync(r)).Id);
        Assert.Single((await Expenses().AdvancesAsync(new())).Items);
    }
    [Fact] public async Task AdvanceRequiresOwnShiftPermissionActiveRecipientAndCashFunds()
    {
        var r=new AdvanceRequest(Guid.NewGuid(),eb.Id,cash.Id,500,"Advance");
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().RecordAdvanceAsync(r));await Service().OpenAsync(new(Guid.NewGuid(),200));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().RecordAdvanceAsync(r));
        eb=await db.Employees.SingleAsync(x=>x.Id==eb.Id);eb.EmploymentStatus=EmploymentStatus.Suspended;await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().RecordAdvanceAsync(r with{Amount=100}));
        user.Context=user.Context! with{Permissions=[ShiftPermissions.AdvanceView]};await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Expenses().RecordAdvanceAsync(r));
        Assert.Empty(await db.EmployeeAdvances.ToListAsync());Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task AdvanceHistoryRejectsCrossBranchAndChangedOperationAndPreservesOriginal()
    {
        await Service().OpenAsync(new(Guid.NewGuid(),100000));var r=new AdvanceRequest(Guid.NewGuid(),eb.Id,cash.Id,500,"Advance");
        var result=await Expenses().RecordAdvanceAsync(r);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().RecordAdvanceAsync(r with{Amount=501}));
        user.Context=user.Context! with{Permissions=[ShiftPermissions.AdvanceView]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Expenses().AdvancesAsync(new(other.Id)));
        Assert.Equal(500,(await Expenses().GetAdvanceAsync(result.Id)).Amount);
        var row=await db.EmployeeAdvances.SingleAsync();row.Amount=1;await Assert.ThrowsAsync<InvalidOperationException>(()=>db.SaveChangesAsync());
    }
}
