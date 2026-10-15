using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CampionsSystem.DAL.Tests;

public sealed partial class SalesTests
{
    [Fact]
    public async Task BranchPeriodExplainsSharedBalancesAndFreezesAtClosedShiftCutoff()
    {
        var shifts=Shifts();var own=await db.Shifts.SingleAsync();
        var cash=await Account();var insta=await Account(FinancialAccountType.InstaPay);
        var category=new ExpenseCategory{Name="Branch operations"};db.Add(category);await db.SaveChangesAsync();

        clock.Now=clock.Now.AddMinutes(1);
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,700),new PaymentPartInput(insta.Id,500)));
        var colleague=new ApplicationUser{UserName="period-colleague"};db.Add(colleague);await db.SaveChangesAsync();
        clock.Now=clock.Now.AddMinutes(1);
        var other=await OperationalShiftFixture.OpenAsync(db,colleague.Id,b.Id,clock);
        var colleagueUser=new TestBranchUser(user.Context! with{UserId=colleague.Id});
        var colleagueAccess=new OperationAccess(colleagueUser,db,clock,Options.Create(new AuthContextOptions()));
        clock.Now=clock.Now.AddMinutes(1);
        await new ExpenseService(db,colleagueUser,colleagueAccess).RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,50,"Colleague expense"));

        var owner=new ApplicationUser{UserName="period-owner"};var role=new IdentityRole<Guid>("Owner"){Id=Guid.NewGuid(),NormalizedName="OWNER"};
        db.AddRange(owner,role,new IdentityUserRole<Guid>{UserId=owner.Id,RoleId=role.Id});await db.SaveChangesAsync();
        var ownerUser=new TestBranchUser(user.Context! with{UserId=owner.Id});
        var ownerAccess=new OperationAccess(ownerUser,db,clock,Options.Create(new AuthContextOptions()));
        var ownerExpenses=new ExpenseService(db,ownerUser,ownerAccess);
        clock.Now=clock.Now.AddMinutes(1);
        var ownerCash=await ownerExpenses.RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,100,"Owner Cash expense"));
        var ownerInsta=await ownerExpenses.RecordPaidAsync(new(Guid.NewGuid(),category.Id,insta.Id,40,"Owner InstaPay expense"));
        Assert.Null(ownerCash.ShiftId);Assert.Null(ownerInsta.ShiftId);
        Assert.Equal(owner.Id,ownerCash.RecordedByUserId);

        var reports=new ShiftReportService(db,user,access);
        var open=await reports.ShiftAsync(own.Id);
        Assert.Equal(5,open.BranchPeriod!.TransactionCount);
        Assert.Equal(2,open.BranchPeriod.Operators.Where(x=>x.ActorId==owner.Id).Sum(x=>x.TransactionCount));
        Assert.Equal(550,open.BranchPeriod.Activity.Single(x=>x.Method==FinancialAccountType.Cash).NetMovement);
        Assert.Equal(460,open.BranchPeriod.Activity.Single(x=>x.Method==FinancialAccountType.InstaPay).NetMovement);
        var cashAccount=open.BranchPeriod.Accounts.Single(x=>x.AccountId==cash.Id);
        Assert.Equal(100000,cashAccount.OpeningBookBalance);
        Assert.Equal(700,cashAccount.Inflows);Assert.Equal(150,cashAccount.Outflows);
        Assert.Equal(100550,cashAccount.CalculatedClosingBookBalance);
        Assert.Equal(100550,cashAccount.ExpectedClosingBookBalance);
        Assert.True(cashAccount.MatchesExpectedClosingBalance);
        Assert.Equal(700,open.Activity.Single(x=>x.Method==FinancialAccountType.Cash).NetMovement);
        Assert.Equal(100550,open.CurrentSharedDrawer.ExpectedPhysicalCash);
        var first=await reports.BranchTransactionsAsync(own.Id,pageSize:2);
        var second=await reports.BranchTransactionsAsync(own.Id,page:2,pageSize:2);
        var third=await reports.BranchTransactionsAsync(own.Id,page:3,pageSize:2);
        var rows=first.Items.Concat(second.Items).Concat(third.Items).ToArray();
        Assert.Equal(5,first.Total);Assert.Equal(5,rows.Select(x=>x.Id).Distinct().Count());
        Assert.Contains(rows,x=>x.ActorId==colleague.Id&&x.ShiftId==other.Id&&x.ActorName!="");
        Assert.Contains(rows,x=>x.ActorId==owner.Id&&x.ShiftId==null&&x.Method==FinancialAccountType.Cash&&x.Direction==FinancialTransactionType.Debit);
        Assert.Contains(rows,x=>x.ActorId==owner.Id&&x.ShiftId==null&&x.Method==FinancialAccountType.InstaPay&&x.Direction==FinancialTransactionType.Debit);

        clock.Now=clock.Now.AddMinutes(1);
        await shifts.CloseAsync(own.Id,new(Guid.NewGuid(),own.Version,100550));
        clock.Now=clock.Now.AddMinutes(1);
        await new ExpenseService(db,colleagueUser,colleagueAccess).RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,25,"After colleague closure"));
        db.Add(new FinancialTransaction{BranchId=b.Id,FinancialAccountId=insta.Id,
            SourceType=FinancialSourceType.Adjustment,TransactionType=FinancialTransactionType.Credit,
            Amount=7,OccurredAt=own.OpenedAt.AddMinutes(2),PostedAt=clock.Now,CreatedByUserId=owner.Id});
        await db.SaveChangesAsync(); // Later-discovered occurrence stays out of the closed report.
        var closed=await reports.ShiftAsync(own.Id);
        Assert.Equal(5,closed.BranchPeriod!.TransactionCount);
        Assert.Equal(100550,closed.BranchPeriod.ExpectedPhysicalCashAtCutoff);
        Assert.Equal(100550,closed.BranchPeriod.Accounts.Single(x=>x.AccountId==cash.Id).ExpectedClosingBookBalance);
        Assert.Equal(460,closed.BranchPeriod.Accounts.Single(x=>x.AccountId==insta.Id).ExpectedClosingBookBalance);
        Assert.Equal(467,closed.CurrentSharedAccountBalances.Single(x=>x.AccountId==insta.Id).BookBalance);
        Assert.Equal(100525,closed.CurrentSharedDrawer.ExpectedPhysicalCash); // Live branch position is separate.
        Assert.Equal(5,(await reports.BranchTransactionsAsync(own.Id)).Total);
        Assert.Equal(ShiftStatus.Open,(await shifts.GetAsync(other.Id)).Status);
    }
}
