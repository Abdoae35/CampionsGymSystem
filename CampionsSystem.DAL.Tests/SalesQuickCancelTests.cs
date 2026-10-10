using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    [Theory][InlineData(FinancialAccountType.Cash)][InlineData(FinancialAccountType.InstaPay)]
    public async Task QuickCancelFullyReversesOriginalAccountEvenIfLaterInactive(FinancialAccountType method)
    {
        var account=await Account(method);var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(account.Id,1200)));account.IsActive=false;await db.SaveChangesAsync();
        user.Context=user.Context! with{Permissions=[SalesPermissions.QuickCancel]};var r=new QuickCancelRequest(Guid.NewGuid(),"Mistake");var refund=await Finance().QuickCancelAsync(sale.SaleId,r);
        Assert.Equal(1200,refund.Amount);Assert.Equal(account.Id,Assert.Single(refund.Parts).AccountId);Assert.Equal(refund.Id,(await Finance().QuickCancelAsync(sale.SaleId,r)).Id);
        Assert.Single(await db.Payments.ToListAsync());Assert.Single(await db.Refunds.ToListAsync());Assert.Equal(SubscriptionStatus.Cancelled,(await db.MemberSubscriptions.SingleAsync()).Status);
    }
    [Theory][InlineData(SubscriptionActivationMode.Now)][InlineData(SubscriptionActivationMode.FirstCheckIn)][InlineData(SubscriptionActivationMode.SpecificDate)]
    public async Task QuickCancelReversesSplitTenderAndCancelsCurrentFutureOrUndatedGrant(SubscriptionActivationMode mode)
    {
        Reception();var cash=await Account();var insta=await Account(FinancialAccountType.InstaPay);var r=Purchase(new PaymentPartInput(cash.Id,700),new PaymentPartInput(insta.Id,500)) with{ActivationMode=mode,StartDate=mode==SubscriptionActivationMode.SpecificDate?today.AddDays(5):null};
        var sale=await Finance().CreateAsync(r);var refund=await Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Mistake"));
        Assert.Equal(700,refund.Parts.Single(p=>p.Method=="Cash").Amount);Assert.Equal(500,refund.Parts.Single(p=>p.Method=="InstaPay").Amount);
        Assert.False((await Entrance().EvaluateAsync(new(member.Id,null))).Allowed);Assert.False((await Entrance().EvaluateAsync(new(member.Id,null))).CanActivateOnCheckIn);
        Assert.All(await db.MemberSubscriptions.ToListAsync(),s=>Assert.Equal(SubscriptionStatus.Cancelled,s.Status));
    }
    [Fact] public async Task QuickCancelBoundaryIsExactly24ActualHoursAndExplicitCapability()
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));user.Context=user.Context! with{Permissions=[SalesPermissions.Refund]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Mistake")));
        user.Context=user.Context! with{Permissions=[SalesPermissions.QuickCancel]};clock.Now=clock.Now.AddHours(24).AddTicks(1);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Mistake")));
        clock.Now=clock.Now.AddTicks(-1);Assert.Equal(1200,(await Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Mistake"))).Amount);
    }
    [Fact] public async Task CheckInUseAndDebtSettlementRejectQuickCancel()
    {
        Reception();var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id));await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Mistake")));
        clock.Now=clock.Now.AddDays(30);await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id,ConfirmBorrow:true));
        var renewal=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(renewal.SaleId,new(Guid.NewGuid(),"Mistake")));
        Assert.Empty(await db.Refunds.ToListAsync());Assert.Single(await db.BorrowedDebtSettlementEvents.ToListAsync());
    }
    [Fact] public async Task BenefitUseRejectsQuickCancellation()
    {
        var cash=await Account();var benefit=new MembershipBenefit{Code="GUEST",Name="Guest"};db.Add(new MembershipPlanBenefit{MembershipPlan=plan,MembershipBenefit=benefit,GrantType=MembershipBenefitGrantType.Quantity,Quantity=2});await db.SaveChangesAsync();
        var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));var snapshot=await db.MemberSubscriptionBenefits.SingleAsync();
        db.Add(new MemberBenefitUsage{MemberSubscriptionBenefitId=snapshot.Id,QuantityUsed=1,BranchId=b.Id,CreatedByUserId=actor.Id,UsedAt=access.Now});await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Mistake")));Assert.Empty(await db.Refunds.ToListAsync());
    }
    [Fact] public async Task QuickCancellingUnusedAppendedRenewalKeepsPreviousPaidGrant()
    {
        var cash=await Account();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));clock.Now=clock.Now.AddSeconds(1);
        var renewal=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));await Finance().QuickCancelAsync(renewal.SaleId,new(Guid.NewGuid(),"Mistake"));
        var sub=await db.MemberSubscriptions.SingleAsync();Assert.Equal(today.AddDays(29),sub.EndDate);Assert.NotEqual(SubscriptionStatus.Cancelled,sub.Status);
    }
}
