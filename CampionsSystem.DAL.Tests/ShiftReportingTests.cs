using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    [Fact] public async Task EmployeeActivityAndSharedDrawerRemainSeparateAcrossOverlappingShifts()
    {
        Shifts();var own=await db.Shifts.SingleAsync();var cash=await Account();var insta=await Account(FinancialAccountType.InstaPay);
        var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,700),new PaymentPartInput(insta.Id,500)));
        var category=new ExpenseCategory{Name="Supplies"};db.Add(category);await db.SaveChangesAsync();
        var expenses=new ExpenseService(db,user,access);
        await expenses.RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,200,"Operational supplies"));
        await AdvanceSalaryFixture.ConfigureAsync(db,b.Id,actor.Id,today,own.EmployeeId!.Value);
        await expenses.RecordAdvanceAsync(new(Guid.NewGuid(),own.EmployeeId!.Value,insta.Id,100,"Advance"));
        await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)],"Manual cancellation"));
        var colleague=new ApplicationUser{UserName="overlap-report"};db.Add(colleague);await db.SaveChangesAsync();
        var second=await OperationalShiftFixture.OpenAsync(db,colleague.Id,b.Id,clock);
        var colleagueUser=new TestBranchUser(user.Context! with{UserId=colleague.Id});
        var colleagueAccess=new OperationAccess(colleagueUser,db,clock,Options.Create(new AuthContextOptions()));
        await new ExpenseService(db,colleagueUser,colleagueAccess).RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,50,"Colleague supplies"));
        insta.IsActive=false;await db.SaveChangesAsync(); // Historical method must survive deactivation.
        var reports=new ShiftReportService(db,user,access);var mine=await reports.ShiftAsync(own.Id);var branch=await reports.BranchAsync();
        var c=mine.Activity.Single(x=>x.Method==FinancialAccountType.Cash);var bank=mine.Activity.Single(x=>x.Method==FinancialAccountType.InstaPay);
        Assert.Equal(700,c.Collected);Assert.Equal(100,c.Refunded);Assert.Equal(200,c.ExpensesPaid);Assert.Equal(500,bank.Collected);Assert.Equal(100,bank.AdvancesPaid);
        Assert.Equal(100350,mine.CurrentSharedDrawer.ExpectedPhysicalCash);Assert.Equal(250,branch.Activity.Single(x=>x.Method==FinancialAccountType.Cash).ExpensesPaid);
        Assert.Equal(100350,mine.CurrentSharedAccountBalances.Single(x=>x.Method==FinancialAccountType.Cash).BookBalance);
        Assert.Equal(400,mine.CurrentSharedAccountBalances.Single(x=>x.Method==FinancialAccountType.InstaPay).BookBalance);
        Assert.Null(mine.Shift.ActualClosingCash);
        var firstPage=await reports.TransactionsAsync(own.Id,pageSize:2);
        var secondPage=await reports.TransactionsAsync(own.Id,page:2,pageSize:2);
        var thirdPage=await reports.TransactionsAsync(own.Id,page:3,pageSize:2);
        var movements=firstPage.Items.Concat(secondPage.Items).Concat(thirdPage.Items).ToArray();
        Assert.Equal(5,firstPage.Total);Assert.Equal(2,firstPage.Items.Count);Assert.Single(thirdPage.Items);
        Assert.Equal(5,movements.Select(x=>x.Id).Distinct().Count());
        Assert.All(movements,x=>Assert.Equal(own.Id,x.ShiftId));
        Assert.All(movements,x=>Assert.Equal(actor.Id,x.ActorId));
        Assert.Contains(movements,x=>x.SourceType==FinancialSourceType.Payment&&x.Method==FinancialAccountType.Cash&&x.Amount==700&&x.Direction==FinancialTransactionType.Credit&&x.PaymentId!=null);
        Assert.Contains(movements,x=>x.SourceType==FinancialSourceType.Payment&&x.Method==FinancialAccountType.InstaPay&&x.Amount==500&&x.Direction==FinancialTransactionType.Credit&&x.PaymentId!=null);
        Assert.Contains(movements,x=>x.SourceType==FinancialSourceType.Refund&&x.Direction==FinancialTransactionType.Debit&&x.Amount==100&&x.RefundId!=null);
        Assert.Contains(movements,x=>x.SourceType==FinancialSourceType.Expense&&x.Direction==FinancialTransactionType.Debit&&x.Amount==200&&x.ExpenseId!=null);
        Assert.Contains(movements,x=>x.SourceType==FinancialSourceType.EmployeeAdvance&&x.Direction==FinancialTransactionType.Debit&&x.Method==FinancialAccountType.InstaPay&&x.EmployeeAdvanceId!=null);
        Assert.All(movements,x=>Assert.NotEqual(default,x.OccurredAt));
        Assert.Equal(50,(await reports.TransactionsAsync(second.Id)).Items.Single().Amount);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>reports.TransactionsAsync(own.Id,pageSize:101));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>reports.TransactionsAsync(own.Id,page:int.MaxValue,pageSize:100));
        Assert.Equal(50,(await reports.ShiftAsync(second.Id)).Activity.Single(x=>x.Method==FinancialAccountType.Cash).ExpensesPaid);
        Assert.Equal(2,await db.Shifts.CountAsync(x=>x.Status==ShiftStatus.Open));Assert.Empty(await db.Payrolls.ToListAsync());
    }
    [Fact] public async Task LegacyUnshiftedMovementsRemainVisibleWithoutAttributingToEmployeeShift()
    {
        Shifts();var own=await db.Shifts.SingleAsync();var account=await Account();
        db.Add(new FinancialTransaction{BranchId=b.Id,FinancialAccountId=account.Id,SourceType=FinancialSourceType.Adjustment,
            TransactionType=FinancialTransactionType.Credit,Amount=40,OccurredAt=clock.Now,CreatedByUserId=actor.Id});await db.SaveChangesAsync();
        var reports=new ShiftReportService(db,user,access);var summary=await reports.BranchAsync();
        Assert.Equal(40,summary.Activity.Single(x=>x.Method==FinancialAccountType.Cash).LegacyUnshiftedNetMovement);
        Assert.Equal(0,(await reports.ShiftAsync(own.Id)).Activity.Sum(x=>x.NetMovement));
        Assert.Null((await db.FinancialTransactions.SingleAsync()).ShiftId);
        user.Context=user.Context! with{Permissions=[ShiftPermissions.VaultView,ShiftPermissions.View]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>reports.BranchAsync(a.Id));
        user.Context=user.Context! with{Permissions=[ShiftPermissions.View]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>reports.TransactionsAsync(own.Id));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>reports.AuditAsync(pageSize:101));
    }
    [Fact] public async Task ExpenseReturnsAndAuditHistoryRemainBoundedAndPreserveOriginalPaidFacts()
    {
        Shifts();var account=await Account();var category=new ExpenseCategory{Name="Returned supplies"};db.Add(category);await db.SaveChangesAsync();
        var service=new ExpenseService(db,user,access);var paid=await service.RecordPaidAsync(new(Guid.NewGuid(),category.Id,account.Id,200,"Supplies"));
        var returned=await service.ReverseAsync(paid.Id,new(Guid.NewGuid(),50,true,"Actual cash returned","receipt"));
        Assert.Equal(returned.Id,Assert.Single((await service.ReturnsAsync(paid.Id)).Items).Id);
        var reports=new ShiftReportService(db,user,access);var totals=(await reports.BranchAsync()).Activity.Single(x=>x.Method==FinancialAccountType.Cash);
        Assert.Equal(200,totals.ExpensesPaid);Assert.Equal(50,totals.ExpenseReturns);Assert.Equal(-150,totals.NetMovement);
        Assert.Single((await reports.AuditAsync(entityId:returned.Id)).Items);Assert.Equal(200,(await db.Expenses.SingleAsync()).Amount);
    }
}
