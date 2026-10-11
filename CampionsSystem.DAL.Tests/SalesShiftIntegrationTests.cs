using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    private ShiftService Shifts()
    {user.Context=user.Context! with{Permissions=[..user.Context!.Permissions,..ShiftPermissions.Codes]};return new(db,user,access);}
    [Fact] public async Task PaidSaleRequiresActingEmployeesShiftButIdenticalCommittedRetrySurvivesClosure()
    {
        var account=await Account();var r=Purchase(new PaymentPartInput(account.Id,1200));var result=await Finance().CreateAsync(r);
        var shift=await db.Shifts.SingleAsync();await Shifts().CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,101200));
        Assert.Equal(result.SaleId,(await Finance().CreateAsync(r)).SaleId);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().CreateAsync(r with{OperationId=Guid.NewGuid()}));
        Assert.Single(await db.SaleTransactions.ToListAsync());Assert.Single(await db.Payments.ToListAsync());
        Assert.Equal(shift.Id,(await db.Payments.SingleAsync()).ShiftId);Assert.Equal(shift.Id,(await db.SaleTransactions.SingleAsync()).CreatedShiftId);
    }
    [Fact] public async Task AdditionalReservationCollectionRequiresNewOwnShiftAndRetainsOriginalPaymentLinks()
    {
        var account=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(account.Id,200)) with{PurchaseMode=PurchaseMode.Reservation});
        var first=await db.Shifts.SingleAsync();await Shifts().CloseAsync(first.Id,new(Guid.NewGuid(),first.Version,100200));
        var r=new RecordPaymentRequest(Guid.NewGuid(),1000,[new(account.Id,1000)]);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RecordPaymentAsync(sale.SaleId,r));
        var second=await Shifts().OpenAsync(new(Guid.NewGuid(),100200));var payment=await Finance().RecordPaymentAsync(sale.SaleId,r);
        Assert.Equal(second.Id,(await db.Payments.SingleAsync(x=>x.Id==payment.Id)).ShiftId);
        await Shifts().CloseAsync(second.Id,new(Guid.NewGuid(),second.Version,101200));
        Assert.Equal(payment.Id,(await Finance().RecordPaymentAsync(sale.SaleId,r)).Id);Assert.Equal(2,await db.Payments.CountAsync());
    }
    [Fact] public async Task CashRefundRequiresActualPhysicalFundsWithoutRewritingEarlierMoneyOrGrant()
    {
        var account=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(account.Id,1200)));
        Shifts();await new VaultService(db,user,access).CountAsync(new(Guid.NewGuid(),100,"Actual drawer cash counted"));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),200,[new(account.Id,200)],"Authorized manual decision")));
        Assert.Empty(await db.Refunds.ToListAsync());Assert.Empty(await db.SubscriptionGrantCancellations.ToListAsync());Assert.Null((await db.SaleTransactions.SingleAsync()).CancelledAt);
        Assert.Equal(101200,await new FinancialOperations(db,user,access).BookAsync(account.Id,default));
    }
    [Fact] public async Task AnotherEmployeesOpenShiftDoesNotAuthorizeFinancialOperationsForClosedActor()
    {
        var first=await db.Shifts.SingleAsync();await Shifts().CloseAsync(first.Id,new(Guid.NewGuid(),first.Version,100000));
        var other=new ApplicationUser{UserName="colleague"};db.Add(other);await db.SaveChangesAsync();
        await OperationalShiftFixture.OpenAsync(db,other.Id,b.Id,clock);
        var account=await Account();await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().CreateAsync(Purchase(new PaymentPartInput(account.Id,1200))));
        Assert.Single(await db.Shifts.Where(x=>x.Status==ShiftStatus.Open).ToListAsync());Assert.Empty(await db.Payments.ToListAsync());
    }
}
