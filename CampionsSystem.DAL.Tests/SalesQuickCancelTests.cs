using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.BLL.Commissions;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    private async Task QuickReceptionAsync() => await AddRefundRoleAsync("Reception");
    [Theory][InlineData(FinancialAccountType.Cash)][InlineData(FinancialAccountType.InstaPay)]
    public async Task QuickCancelCanReverseAnOriginalPaymentAccountThatLaterBecameInactive(FinancialAccountType method)
    {
        var account=await Account(method);var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(account.Id,1200)));account.IsActive=false;await db.SaveChangesAsync();
        await QuickReceptionAsync();var r=new QuickCancelRequest(Guid.NewGuid(),"Mistake",1200,[new(account.Id,1200)]);
        var refund=await Finance().QuickCancelAsync(sale.SaleId,r);
        Assert.Equal(1200,refund.Amount);Assert.Equal(account.Id,Assert.Single(refund.Parts).AccountId);Assert.Equal(refund.Id,(await Finance().QuickCancelAsync(sale.SaleId,r)).Id);
        Assert.Single(await db.Payments.ToListAsync());Assert.Single(await db.Refunds.ToListAsync());Assert.Equal(SubscriptionStatus.Cancelled,(await db.MemberSubscriptions.SingleAsync()).Status);
    }
    [Fact] public async Task QuickCancelCannotDebitAnUnrelatedInactiveAccount()
    {
        var cash=await Account();var unrelated=await Account(FinancialAccountType.InstaPay);
        var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        unrelated.IsActive=false;await db.SaveChangesAsync();await QuickReceptionAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Wrong inactive source",100,[new(unrelated.Id,100)])));
        Assert.Empty(await db.Refunds.ToListAsync());Assert.Empty(await db.FinancialTransactions.Where(x=>x.TransactionType==FinancialTransactionType.Debit).ToListAsync());
    }
    [Theory][InlineData(SubscriptionActivationMode.Now)][InlineData(SubscriptionActivationMode.FirstCheckIn)][InlineData(SubscriptionActivationMode.SpecificDate)]
    public async Task QuickCancelReversesSplitTenderAndCancelsCurrentFutureOrUndatedGrant(SubscriptionActivationMode mode)
    {
        Reception();var cash=await Account();var insta=await Account(FinancialAccountType.InstaPay);var r=Purchase(new PaymentPartInput(cash.Id,700),new PaymentPartInput(insta.Id,500)) with{ActivationMode=mode,StartDate=mode==SubscriptionActivationMode.SpecificDate?today.AddDays(5):null};
        var sale=await Finance().CreateAsync(r);await QuickReceptionAsync();var refund=await Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Mistake",1200,[new(cash.Id,700),new(insta.Id,500)]));
        Assert.Equal(700,refund.Parts.Single(p=>p.Method=="Cash").Amount);Assert.Equal(500,refund.Parts.Single(p=>p.Method=="InstaPay").Amount);
        Assert.False((await Entrance().EvaluateAsync(new(member.Id,null))).Allowed);Assert.False((await Entrance().EvaluateAsync(new(member.Id,null))).CanActivateOnCheckIn);
        Assert.All(await db.MemberSubscriptions.ToListAsync(),s=>Assert.Equal(SubscriptionStatus.Cancelled,s.Status));
    }
    [Fact] public async Task QuickCancelBoundaryIsExactly24ActualHoursAndExplicitCapability()
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));await QuickReceptionAsync();user.Context=user.Context! with{Permissions=[SalesPermissions.Refund]};
        var request=new QuickCancelRequest(Guid.NewGuid(),"Mistake",1200,[new(cash.Id,1200)]);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Finance().QuickCancelAsync(sale.SaleId,request));
        user.Context=user.Context! with{Permissions=[SalesPermissions.QuickCancel]};clock.Now=clock.Now.AddHours(24).AddTicks(1);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(sale.SaleId,request));
        clock.Now=clock.Now.AddTicks(-1);Assert.Equal(1200,(await Finance().QuickCancelAsync(sale.SaleId,request)).Amount);
    }
    [Fact] public async Task CheckInUseDoesNotRestrictFullQuickRefund()
    {
        Reception();var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id));await QuickReceptionAsync();
        var refund=await Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Customer decision",1200,[new(cash.Id,1200)]));
        Assert.Equal(1200,refund.Amount);Assert.Single(await db.CheckIns.ToListAsync());Assert.Equal(SubscriptionStatus.Cancelled,(await db.MemberSubscriptions.SingleAsync()).Status);
    }
    [Fact] public async Task DebtSettlementCanBeCancelledThroughExistingEntitlementReversal()
    {
        Reception();var cash=await Account();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        clock.Now=clock.Now.AddDays(30);await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id,ConfirmBorrow:true));
        var renewal=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));await QuickReceptionAsync();
        await Finance().QuickCancelAsync(renewal.SaleId,new(Guid.NewGuid(),"Cancel renewal",0,[]));
        Assert.Single(await db.Refunds.ToListAsync());Assert.Equal(2,await db.BorrowedDebtSettlementEvents.CountAsync());
    }
    [Fact] public async Task BenefitUseDoesNotBlockDiscretionaryQuickCancellation()
    {
        var cash=await Account();var benefit=new MembershipBenefit{Code="GUEST",Name="Guest"};db.Add(new MembershipPlanBenefit{MembershipPlan=plan,MembershipBenefit=benefit,GrantType=MembershipBenefitGrantType.Quantity,Quantity=2});await db.SaveChangesAsync();
        var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));var snapshot=await db.MemberSubscriptionBenefits.SingleAsync();
        db.Add(new MemberBenefitUsage{MemberSubscriptionBenefitId=snapshot.Id,QuantityUsed=1,BranchId=b.Id,CreatedByUserId=actor.Id,UsedAt=access.Now});await db.SaveChangesAsync();
        await QuickReceptionAsync();await Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Customer decision",0,[]));
        Assert.Single(await db.Refunds.ToListAsync());Assert.Single(await db.MemberBenefitUsages.ToListAsync());Assert.Empty(await db.FinancialTransactions.Where(t=>t.TransactionType==FinancialTransactionType.Debit).ToListAsync());
    }
    [Fact] public async Task QuickCancellingUnusedAppendedRenewalKeepsPreviousPaidGrant()
    {
        var cash=await Account();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));clock.Now=clock.Now.AddSeconds(1);
        var renewal=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));await QuickReceptionAsync();await Finance().QuickCancelAsync(renewal.SaleId,new(Guid.NewGuid(),"Mistake",1200,[new(cash.Id,1200)]));
        var sub=await db.MemberSubscriptions.SingleAsync();Assert.Equal(today.AddDays(29),sub.EndDate);Assert.NotEqual(SubscriptionStatus.Cancelled,sub.Status);
    }
    [Theory][InlineData(0)][InlineData(300)][InlineData(1200)]
    public async Task ReceptionSelectsZeroPartialOrFullRefundWithoutUsageFormula(int amount)
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        await QuickReceptionAsync();var request=new QuickCancelRequest(Guid.NewGuid(),"Owner discussed verbally",amount,amount==0?[]:[new(cash.Id,amount)]);
        var refund=await Finance().QuickCancelAsync(sale.SaleId,request);
        Assert.Equal(amount,refund.Amount);Assert.Equal(refund.Id,(await Finance().QuickCancelAsync(sale.SaleId,request)).Id);
        Assert.Equal(amount,await db.FinancialTransactions.Where(t=>t.TransactionType==FinancialTransactionType.Debit).SumAsync(t=>t.Amount));
        Assert.Equal(1200-amount,(await Finance().GetDetailsAsync(sale.SaleId)).RetainedCancellationRevenue);
        Assert.Null((await db.Refunds.SingleAsync()).ApprovedByUserId);
    }
    [Fact] public async Task PrivateAndClassesUsageDoNotBlockQuickCancellation()
    {
        var privateSale=await PrivatePurchase();await DeliverPrivate(privateSale.SubscriptionId);
        var classesSale=await ClassesPurchase();var occurrence=await ClassOccurrence();await Coaching().RegisterClassAsync(occurrence.Id,new(Guid.NewGuid(),member.Id,classesSale.SubscriptionId));
        var cash=(await Account()).Id;await QuickReceptionAsync();
        await Finance().QuickCancelAsync(privateSale.SaleId,new(Guid.NewGuid(),"Private discretionary refund",100,[new(cash,100)]));
        await Finance().QuickCancelAsync(classesSale.SaleId,new(Guid.NewGuid(),"Classes discretionary refund",0,[]));
        Assert.Single(await db.PrivateSessions.ToListAsync());Assert.Single(await db.CheckIns.Where(c=>c.CheckInType==CheckInType.Class).ToListAsync());
        Assert.Equal(PrivateSubscriptionStatus.Cancelled,(await db.PrivateSubscriptions.SingleAsync()).Status);
        Assert.NotNull((await db.SaleTransactions.SingleAsync(s=>s.Id==classesSale.SaleId)).CancelledAt);
    }
    [Fact] public async Task ReceptionCannotUseElevatedRefundAndManagerCanRefundAfterQuickWindow()
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        await QuickReceptionAsync();var managerRole=await db.Roles.Where(x=>x.NormalizedName=="BRANCHMANAGER").Select(x=>x.Id).SingleAsync();
        db.UserRoles.RemoveRange(await db.UserRoles.Where(x=>x.UserId==actor.Id && x.RoleId==managerRole).ToListAsync());await db.SaveChangesAsync();
        clock.Now=clock.Now.AddHours(25);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Too late",0,[])));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),0,[],"No elevated role")));
        await AddRefundRoleAsync("BranchManager");
        var result=await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),300,[new(cash.Id,300)],"Manager decision"));
        Assert.Equal(300,result.Amount);Assert.Equal(actor.Id,(await db.Refunds.SingleAsync()).ProcessedByUserId);
    }
    [Theory][InlineData(1)][InlineData(25)]
    public async Task OwnerRefundsAtAnyTimeWithoutInventingAnOwnerShift(int elapsedHours)
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        user.Context=user.Context! with{Permissions=[..user.Context.Permissions,ShiftPermissions.VaultView,ShiftPermissions.Manage]};
        var shift=await db.Shifts.SingleAsync();var position=await new VaultService(db,user,access).PositionAsync(b.Id);
        await new ShiftService(db,user,access).CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,position.ExpectedPhysicalCash));
        await AddRefundRoleAsync("Owner");clock.Now=clock.Now.AddHours(elapsedHours);
        var result=await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),600,[new(cash.Id,600)],"Owner decision"));
        Assert.Equal(600,result.Amount);Assert.Null((await db.Refunds.SingleAsync()).ShiftId);
        var ledger=await db.FinancialTransactions.SingleAsync(t=>t.RefundId==result.Id);
        Assert.Null(ledger.ShiftId);Assert.Equal(actor.Id,ledger.CreatedByUserId);
    }
    [Fact] public async Task QuickCancelCapsRefundAtActualPartialCollectionAndRejectsMissingAmount()
    {
        var cash=await Account();var sale=await Finance().CreateAsync(InstallmentPurchase(cash.Id));await QuickReceptionAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Missing amount")));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Over collection",201,[new(cash.Id,201)])));
        Assert.Empty(await db.Refunds.ToListAsync());
        var result=await Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Keep half",100,[new(cash.Id,100)]));
        Assert.Equal(100,result.Amount);Assert.Equal(100,(await Finance().GetDetailsAsync(sale.SaleId)).RetainedCancellationRevenue);
    }
    [Fact] public async Task EarlierNormalRefundCannotBeRepeatedThroughQuickCancel()
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)],"Manager decision"));await QuickReceptionAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Second cancellation",1100,[new(cash.Id,1100)])));
        Assert.Single(await db.Refunds.ToListAsync());Assert.Equal(100,await db.Refunds.SumAsync(x=>x.Amount));
    }
    [Fact] public async Task QuickCancelCannotReverseFinalizedSalesCommissionMoney()
    {
        await CommercialOwner();var period=await Commissions().OpenAsync(new(Guid.NewGuid(),"Final",today,today));
        var sale=await Collected(1200);var cash=await Account();await QuickReceptionAsync();
        clock.Now=clock.Now.AddDays(1);await Commissions().FinalizeAsync(period.Id,new(Guid.NewGuid(),0));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Cannot reverse final money",100,[new(cash.Id,100)])));
        Assert.Empty(await db.Refunds.ToListAsync());Assert.Null((await db.SaleTransactions.SingleAsync()).CancelledAt);
        await Finance().QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Retain final money",0,[]));
        Assert.Equal(0,await db.FinancialTransactions.Where(t=>t.TransactionType==FinancialTransactionType.Debit).SumAsync(t=>t.Amount));
    }
}
