using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class EmployeeShiftTests
{
    private ExpenseService Expenses()=>new(db,user,access);
    private async Task<ExpenseCategoryDetails> Category()=>await Expenses().SetCategoryAsync(null,new("Maintenance"));
    [Theory][InlineData(FinancialAccountType.Cash)][InlineData(FinancialAccountType.InstaPay)]
    public async Task ReceptionDirectExpenseIsPaidOnceWithoutApprovalsAndUsesCorrectSource(FinancialAccountType type)
    {
        var category=await Category();var account=cash;
        if(type==FinancialAccountType.InstaPay){account=new(){BranchId=branch.Id,Name="InstaPay",AccountType=type};db.Add(account);await db.SaveChangesAsync();}
        var shift=await Service().OpenAsync(new(Guid.NewGuid(),100000));
        user.Context=user.Context! with{Permissions=[ShiftPermissions.ExpenseCreate,ShiftPermissions.ExpenseView,ShiftPermissions.Manage,ShiftPermissions.View,ShiftPermissions.VaultView]};
        var r=new PaidExpenseRequest(Guid.NewGuid(),category.Id,account.Id,500,"Operational repair","receipt-1");
        var paid=await Expenses().RecordPaidAsync(r);Assert.Equal(paid.Id,(await Expenses().RecordPaidAsync(r)).Id);
        var original=await db.Expenses.SingleAsync();Assert.Null(original.ApprovedByUserId);Assert.Equal(ea.Id,paid.RecordingEmployeeId);Assert.Equal(shift.Id,paid.ShiftId);
        var ledger=await db.FinancialTransactions.SingleAsync();Assert.Equal(FinancialSourceType.Expense,ledger.SourceType);Assert.Equal(FinancialTransactionType.Debit,ledger.TransactionType);
        Assert.Equal(account.Id,ledger.FinancialAccountId);Assert.Equal(type,paid.Method);Assert.NotNull(paid.PaidAt);
        Assert.Equal(type==FinancialAccountType.Cash?99500:100000,(await Vault().PositionAsync()).ExpectedPhysicalCash);
        Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.EmployeeCommissionSummaries.ToListAsync());
        await Service().CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,type==FinancialAccountType.Cash?99500:100000));
        Assert.Equal(paid.Id,(await Expenses().RecordPaidAsync(r)).Id);Assert.Single(await db.Expenses.ToListAsync());
    }
    [Fact] public async Task ExpenseCannotExceedPhysicalCashButInstaPayHasNoInventedOverdraftPolicy()
    {
        var category=await Category();await Service().OpenAsync(new(Guid.NewGuid(),200));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,201,"Too much cash")));
        Assert.Empty(await db.Expenses.ToListAsync());
        var insta=new FinancialAccount{BranchId=branch.Id,Name="InstaPay",AccountType=FinancialAccountType.InstaPay};db.Add(insta);await db.SaveChangesAsync();
        await Expenses().RecordPaidAsync(new(Guid.NewGuid(),category.Id,insta.Id,1000,"Actual authorized transfer"));
        Assert.Equal(-1000,await new FinancialOperations(db,user,access).BookAsync(insta.Id,default));Assert.Equal(200,(await Vault().PositionAsync()).ExpectedPhysicalCash);
    }
    [Fact] public async Task ActualExpenseReturnPreservesOriginalAndCannotCreditUnreturnedOrExcessMoney()
    {
        var category=await Category();await Service().OpenAsync(new(Guid.NewGuid(),100000));
        var original=await Expenses().RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,500,"Purchased item"));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().ReverseAsync(original.Id,new(Guid.NewGuid(),500,false,"No returned funds")));
        var r=new ReverseExpenseRequest(Guid.NewGuid(),200,true,"Supplier returned cash","return-receipt");
        var returned=await Expenses().ReverseAsync(original.Id,r);Assert.Equal(returned.Id,(await Expenses().ReverseAsync(original.Id,r)).Id);
        Assert.Equal(99700,(await Vault().PositionAsync()).ExpectedPhysicalCash);Assert.Equal(200,(await Expenses().GetAsync(original.Id)).ReturnedAmount);
        Assert.Equal(500,(await db.Expenses.SingleAsync()).Amount);Assert.NotNull((await db.Expenses.SingleAsync()).PaidAt);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().ReverseAsync(original.Id,new(Guid.NewGuid(),301,true,"Excess return")));
        await Expenses().ReverseAsync(original.Id,new(Guid.NewGuid(),300,true,"Rest returned"));
        Assert.Equal(100000,(await Vault().PositionAsync()).ExpectedPhysicalCash);Assert.Equal(2,await db.ExpenseReversals.CountAsync());
        var row=await db.Expenses.SingleAsync();row.Description="Rewritten";await Assert.ThrowsAsync<InvalidOperationException>(()=>db.SaveChangesAsync());
    }
    [Fact] public async Task ExpensePermissionsBranchAndCategoryValidationCannotBeBypassed()
    {
        var category=await Category();await Service().OpenAsync(new(Guid.NewGuid(),100000));
        var r=new PaidExpenseRequest(Guid.NewGuid(),category.Id,cash.Id,100,"Repair");
        user.Context=user.Context! with{Permissions=[ShiftPermissions.ExpenseView]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Expenses().RecordPaidAsync(r));
        user.Context=user.Context! with{Permissions=[ShiftPermissions.ExpenseCreate]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Expenses().RecordPaidAsync(r with{BranchId=other.Id}));
        user.Context=user.Context! with{Permissions=[..ShiftPermissions.Codes,"Branches.AccessAll"]};
        await Expenses().SetCategoryAsync(category.Id,new("Maintenance",false));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().RecordPaidAsync(r));Assert.Empty(await db.Expenses.ToListAsync());
    }
    [Fact] public async Task MissingEmployeeShiftAndIncorrectCashAccountAreRejected()
    {
        var category=await Category();var r=new PaidExpenseRequest(Guid.NewGuid(),category.Id,cash.Id,100,"Repair");
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().RecordPaidAsync(r));
        await Service().OpenAsync(new(Guid.NewGuid(),100000));
        var wrong=new FinancialAccount{BranchId=branch.Id,Name="Not physical drawer"};db.Add(wrong);await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().RecordPaidAsync(r with{FinancialAccountId=wrong.Id}));
        Assert.Empty(await db.Expenses.ToListAsync());
    }
    [Fact] public async Task LedgerFailureRollsBackExpenseAndAuditWithoutPartialPayout()
    {
        var category=await Category();await Service().OpenAsync(new(Guid.NewGuid(),100000));var audits=await db.AuditLogs.CountAsync();
        await db.Database.ExecuteSqlRawAsync("CREATE FUNCTION reject_expense_money() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'forced expense failure'; END; $$; CREATE TRIGGER reject_expense_money BEFORE INSERT ON \"FinancialTransactions\" FOR EACH ROW EXECUTE FUNCTION reject_expense_money();");
        await Assert.ThrowsAsync<DbUpdateException>(()=>Expenses().RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,100,"Repair")));
        Assert.Empty(await db.Expenses.ToListAsync());Assert.Equal(audits,await db.AuditLogs.CountAsync());Assert.Equal(100000,(await Vault().PositionAsync()).ExpectedPhysicalCash);
    }
    [Fact] public async Task ExpenseBusinessDateUsesOpenDayAndLegacyUnpaidRowsRemainUninferred()
    {
        var category=await Category();await Service().OpenBusinessDayAsync(new(Guid.NewGuid(),today));await Service().OpenAsync(new(Guid.NewGuid(),100000));
        clock.Now=new(2026,10,11,22,0,0,TimeSpan.Zero);var paid=await Expenses().RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,100,"Overnight repair"));
        Assert.Equal(today,paid.BusinessDate);Assert.Equal(clock.Now,paid.PaidAt);
        var legacy=new Expense{BranchId=branch.Id,ExpenseCategoryId=category.Id,FinancialAccountId=cash.Id,Amount=50,Description="Legacy recorded only",ExpenseDate=clock.Now,CreatedByUserId=a.Id};db.Add(legacy);await db.SaveChangesAsync();
        Assert.Null((await Expenses().GetAsync(legacy.Id)).PaidAt);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().ReverseAsync(legacy.Id,new(Guid.NewGuid(),50,true,"No proven payment")));
        Assert.Single(await db.FinancialTransactions.ToListAsync());
    }
}
