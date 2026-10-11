using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;

public sealed partial class EmployeeShiftTests
{
    private VaultService Vault()=>new(db,user,access);
    [Fact] public async Task ExplicitVirginSetupRecordsOpeningPositionOnceWithoutSalesRevenue()
    {
        var account=new FinancialAccount{Branch=other,Name="Other shared drawer"};db.Add(account);await db.SaveChangesAsync();
        var r=new SetupDrawerRequest(Guid.NewGuid(),account.Id,100000,"Physical cash present at initialization",other.Id);
        var pos=await Vault().SetupAsync(r);await Vault().SetupAsync(r);
        Assert.Equal(100000,pos.BookCash);Assert.Equal(100000,pos.ExpectedPhysicalCash);
        var movement=await db.FinancialTransactions.SingleAsync();Assert.Equal(FinancialSourceType.CashMovement,movement.SourceType);
        Assert.Equal("InitialPosition",(await db.CashMovements.SingleAsync()).Kind);Assert.Null(movement.ShiftId);
        Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.SaleTransactions.ToListAsync());
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Vault().SetupAsync(r with{OperationId=Guid.NewGuid()}));
        var money=new FinancialOperations(db,user,access);
        await money.TransactionAsync(async()=>{await money.LockBranchAsync(other.Id,default);await money.PostAsync(new FinancialTransaction{
            BranchId=other.Id,FinancialAccountId=account.Id,SourceType=FinancialSourceType.Adjustment,TransactionType=FinancialTransactionType.Credit,
            Amount=500,OccurredAt=clock.Now,CreatedByUserId=a.Id},default);return true;},default);
        Assert.Equal(pos,await Vault().SetupAsync(r)); // return the original setup snapshot, not today's changed balance
        Assert.Equal(100500,(await Vault().PositionAsync(other.Id)).ExpectedPhysicalCash);
    }
    [Fact] public async Task ExistingOpeningBookBalanceIsNeverCreditedAgainBySetupOrShiftCounts()
    {
        var account=new FinancialAccount{Branch=other,Name="Existing cash",OpeningBalance=1000};db.Add(account);await db.SaveChangesAsync();
        var pos=await Vault().SetupAsync(new(Guid.NewGuid(),account.Id,900,"Initial physical verification",other.Id));
        Assert.Equal(1000,pos.BookCash);Assert.Equal(900,pos.ExpectedPhysicalCash);Assert.Equal(-100,pos.ObservedDiscrepancies);
        Assert.Empty(await db.FinancialTransactions.ToListAsync());
        await Service().OpenAsync(new(Guid.NewGuid(),100000));Actor(b);await Service().OpenAsync(new(Guid.NewGuid(),100000));
        Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task CountCarryForwardPreservesBookDiscrepancyWithoutEmployeeLiability()
    {
        await Service().OpenAsync(new(Guid.NewGuid(),100000));var before=await db.FinancialTransactions.CountAsync();
        var count=await Vault().CountAsync(new(Guid.NewGuid(),99800,"Observed branch drawer shortage"));
        Assert.Equal(-200,count.Difference);var pos=await Vault().PositionAsync();Assert.Equal(99800,pos.ExpectedPhysicalCash);Assert.Equal(100000,pos.BookCash);
        Assert.Equal(before,await db.FinancialTransactions.CountAsync());Assert.Empty(await db.PayrollDeductions.ToListAsync());
        var again=await Vault().CountAsync(new(Guid.NewGuid(),99800,"Recount confirms physical cash"));Assert.Equal(0,again.Difference);
        Assert.Equal(-200,(await Vault().PositionAsync()).ObservedDiscrepancies);Assert.Equal(3,await db.CashCounts.CountAsync());
    }
    [Fact] public async Task CashWithdrawalUsesPhysicalFundsRatherThanHigherBookBalanceAndRetriesAfterClose()
    {
        var shift=await Service().OpenAsync(new(Guid.NewGuid(),99800));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Vault().WithdrawAsync(new(Guid.NewGuid(),99900,"Owner withdrawal")));
        var r=new WithdrawalRequest(Guid.NewGuid(),99800,"Owner discretionary withdrawal");var withdrawal=await Vault().WithdrawAsync(r);
        Assert.Equal(0,(await Vault().PositionAsync()).ExpectedPhysicalCash);Assert.Equal(200,(await Vault().PositionAsync()).BookCash);
        await Service().CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,0));Assert.Equal(withdrawal.Id,(await Vault().WithdrawAsync(r)).Id);
        Assert.Single(await db.CashMovements.ToListAsync());Assert.Single(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task WithdrawalRequiresBothGlobalCapabilityAndOwnOpenEmployeeShift()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Vault().WithdrawAsync(new(Guid.NewGuid(),100,"Withdrawal before shift")));
        await Service().OpenAsync(new(Guid.NewGuid(),100000));user.Context=user.Context! with{Permissions=[ShiftPermissions.Withdraw]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Vault().WithdrawAsync(new(Guid.NewGuid(),100,"Branch-only employee")));
        user.Context=user.Context! with{Permissions=["Branches.AccessAll"]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Vault().WithdrawAsync(new(Guid.NewGuid(),100,"Missing explicit withdrawal permission")));
        Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task SequenceBoundsIncludeCashMovementsAtIdenticalCountTimestamps()
    {
        await Service().OpenAsync(new(Guid.NewGuid(),100000));var money=new FinancialOperations(db,user,access);
        await money.TransactionAsync(async()=>{await money.LockBranchAsync(branch.Id,default);await money.PostAsync(new FinancialTransaction{
            BranchId=branch.Id,FinancialAccountId=cash.Id,SourceType=FinancialSourceType.Adjustment,TransactionType=FinancialTransactionType.Credit,
            Amount=8000,OccurredAt=clock.Now,CreatedByUserId=a.Id,BusinessDate=today},default);return true;},default);
        Assert.Equal(108000,(await Vault().PositionAsync()).ExpectedPhysicalCash);
        await Vault().CountAsync(new(Guid.NewGuid(),108000,"After cash collection"));
        await money.TransactionAsync(async()=>{await money.LockBranchAsync(branch.Id,default);await money.PostAsync(new FinancialTransaction{
            BranchId=branch.Id,FinancialAccountId=cash.Id,SourceType=FinancialSourceType.Adjustment,TransactionType=FinancialTransactionType.Credit,
            Amount=500,OccurredAt=clock.Now,CreatedByUserId=a.Id,BusinessDate=today},default);return true;},default);
        Assert.Equal(108500,(await Vault().PositionAsync()).ExpectedPhysicalCash);Assert.Equal(2,(await db.CashDrawers.SingleAsync()).MovementSequence);
    }
    [Fact] public async Task VaultHistoryIsBoundedAndBranchScopedWithLegacyNullShiftMovements()
    {
        var account=new FinancialAccount{Branch=other,Name="B Cash"};db.Add(account);await db.SaveChangesAsync();
        await Vault().SetupAsync(new(Guid.NewGuid(),account.Id,50,"Initial position",other.Id));
        Assert.Empty((await Vault().MovementsAsync(new())).Items);Assert.Single((await Vault().MovementsAsync(new(other.Id))).Items);
        user.Context=user.Context! with{Permissions=[ShiftPermissions.VaultView]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Vault().MovementsAsync(new(other.Id)));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Vault().MovementsAsync(new(PageSize:101)));
    }
}
