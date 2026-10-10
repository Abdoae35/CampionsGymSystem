using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    [Fact] public async Task ReservationReceivesDepositsButGrantsNoAccessUntilFullPayment()
    {
        Reception();var cash=await Account();var offer=await offers.CreateAsync(OfferInput());
        var result=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,200)) with{PurchaseMode=PurchaseMode.Reservation,OfferId=offer.Id});
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());Assert.Equal(AccessReasons.NoGymEntitlement,(await Entrance().EvaluateAsync(new(member.Id,null))).ReasonCode);
        await offers.UpdateAsync(offer.Id,OfferInput() with{IsActive=false});plan.Price=2000;plan.DurationDays=25;await db.SaveChangesAsync();
        var p=new RecordPaymentRequest(Guid.NewGuid(),500,[new(cash.Id,500)]);await Finance().RecordPaymentAsync(result.SaleId,p);await Finance().RecordPaymentAsync(result.SaleId,p);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());Assert.Equal(2,await db.Payments.CountAsync());
        await Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),300,[new(cash.Id,300)]));
        var sub=await db.MemberSubscriptions.SingleAsync();Assert.Equal(today.AddDays(29),sub.EndDate);Assert.Equal(1000,(await db.SaleTransactions.SingleAsync()).NetAmount);
        Assert.Equal(1000,await db.Payments.SumAsync(x=>x.Amount));Assert.Equal(SaleStatus.Completed,(await db.SaleTransactions.SingleAsync()).Status);
        Assert.True((await Entrance().EvaluateAsync(new(member.Id,null))).Allowed);Assert.Equal(1200,(await db.MembershipSubscriptionTransactions.SingleAsync()).PlanPriceSnapshot);
    }
    [Fact] public async Task PastReservationDeadlineDoesNotCancelAndLateSpecificDateStartsOnCompletionDay()
    {
        var cash=await Account();var result=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,200)) with{PurchaseMode=PurchaseMode.Reservation,
            ActivationMode=SubscriptionActivationMode.SpecificDate,StartDate=today.AddDays(5),ReservationDueDate=today.AddDays(7)});
        clock.Now=clock.Now.AddDays(10);await Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),1000,[new(cash.Id,1000)]));
        var sale=await db.SaleTransactions.SingleAsync();Assert.Null(sale.CancelledAt);Assert.Equal(today.AddDays(5),sale.RequestedStartDate);
        var sub=await db.MemberSubscriptions.SingleAsync();Assert.Equal(today.AddDays(10),sub.StartDate);Assert.Equal(today.AddDays(39),sub.EndDate);
    }
    [Fact] public async Task CompletedReservationCanRemainPendingFirstEntryWithoutFakeDates()
    {
        var cash=await Account();var result=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,200)) with{PurchaseMode=PurchaseMode.Reservation,ActivationMode=SubscriptionActivationMode.FirstCheckIn});
        await Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),1000,[new(cash.Id,1000)]));
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());Assert.Null((await db.SaleTransactions.SingleAsync()).ReservationDueDate);
        Assert.Equal(1200,await db.FinancialTransactions.SumAsync(x=>x.Amount));
    }
    [Fact] public async Task PaymentPartsMismatchAndOverpaymentCannotChangeReservationBalance()
    {
        var cash=await Account();var result=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,200)) with{PurchaseMode=PurchaseMode.Reservation});
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),500,[new(cash.Id,400)])));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),1001,[new(cash.Id,1001)])));
        Assert.Single(await db.Payments.ToListAsync());Assert.Equal(200,await db.Payments.SumAsync(x=>x.Amount));Assert.Empty(await db.MemberSubscriptions.ToListAsync());
    }
}
public sealed partial class SalesConcurrencyTests
{
    [Fact] public async Task ConcurrentFinalReservationPaymentsCannotOverpayOrActivateTwice()
    {
        Guid sale;await using(var db=Context())sale=(await Services(db).Sales.CreateAsync(new(Guid.NewGuid(),member,plan,PurchaseMode.Reservation,SubscriptionActivationMode.Now,[new(account,200)]))).SaleId;
        async Task<bool> Pay(){await using var db=Context();try{await Services(db).Sales.RecordPaymentAsync(sale,new(Guid.NewGuid(),1000,[new(account,1000)]));return true;}catch(BusinessRuleException){return false;}}
        var results=await Task.WhenAll(Pay(),Pay());Assert.Single(results,x=>x);await using var verify=Context();Assert.Equal(1200,await verify.Payments.SumAsync(x=>x.Amount));
        Assert.Single(await verify.MemberSubscriptions.ToListAsync());Assert.Single(await verify.MembershipSubscriptionTransactions.ToListAsync());
    }
}
