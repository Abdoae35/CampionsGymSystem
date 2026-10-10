using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    [Fact] public async Task ManualSplitRefundCancelsAccessAndPreservesOriginalMoney()
    {
        Reception();var cash=await Account();var insta=await Account(FinancialAccountType.InstaPay);
        var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,700),new PaymentPartInput(insta.Id,500)));
        var r=new RefundRequest(Guid.NewGuid(),600,[new(cash.Id,100),new(insta.Id,500,"refund-reference")],"Management decision");
        var refund=await Finance().RefundAsync(sale.SaleId,r);Assert.Equal(refund,(await Finance().RefundAsync(sale.SaleId,r)) with{Parts=refund.Parts});
        Assert.Equal(600,refund.Amount);Assert.Equal("refund-reference",refund.Parts.Single(p=>p.Method=="InstaPay").Reference);
        Assert.Equal(1200,(await db.Payments.SingleAsync()).Amount);Assert.Equal(2,await db.PaymentParts.CountAsync());
        Assert.Equal(1200,(await db.SaleTransactions.SingleAsync()).NetAmount);Assert.Equal(SubscriptionStatus.Cancelled,(await db.MemberSubscriptions.SingleAsync()).Status);
        Assert.False((await Entrance().EvaluateAsync(new(member.Id,null))).Allowed);Assert.Single(await db.SubscriptionGrantCancellations.ToListAsync());
        Assert.Equal(600,await db.FinancialTransactions.Where(t=>t.TransactionType==FinancialTransactionType.Debit).SumAsync(t=>t.Amount));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RefundAsync(sale.SaleId,r with{OperationId=Guid.NewGuid(),Amount=601,Parts=[new(cash.Id,601)]}));
        await Finance().RefundAsync(sale.SaleId,r with{OperationId=Guid.NewGuid(),Parts=[new(cash.Id,600)]});Assert.Equal(SaleStatus.Refunded,(await db.SaleTransactions.SingleAsync()).Status);
    }
    [Theory][InlineData(0)][InlineData(100)][InlineData(200)]
    public async Task ReservationManagementChoosesZeroPartialOrFullRefund(int amount)
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,200)) with{PurchaseMode=PurchaseMode.Reservation});
        await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),amount,amount>0?[new(cash.Id,amount)]:[],"Cancel reservation"));
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());Assert.Equal(amount,(await db.Refunds.SingleAsync()).Amount);
        Assert.Equal(200,(await db.Payments.SingleAsync()).Amount);Assert.NotNull((await db.SaleTransactions.SingleAsync()).CancelledAt);
        if(amount<200)Assert.Contains(await db.AuditLogs.ToListAsync(),a=>a.Action=="Reservation.AmountRetained");
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task CancellingAppendedRenewalPreservesEarlierPaidTimeSessionsBenefitsAndAllowance(bool sessions)
    {
        var cash=await Account();plan.AllowFreeze=true;plan.IncludedFreezeDays=7;if(sessions){plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;}
        var benefit=new MembershipBenefit{Code="GUEST",Name="Guest"};db.Add(new MembershipPlanBenefit{MembershipPlan=plan,MembershipBenefit=benefit,GrantType=MembershipBenefitGrantType.Quantity,Quantity=2});await db.SaveChangesAsync();
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));var sub=await db.MemberSubscriptions.SingleAsync();if(sessions){sub.RemainingSessions=4;await db.SaveChangesAsync();}
        var renewal=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));await Finance().RefundAsync(renewal.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)],"Cancel just renewal"));
        sub=await db.MemberSubscriptions.SingleAsync();Assert.Equal(today.AddDays(29),sub.EndDate);Assert.Equal(sessions?4:null,sub.RemainingSessions);Assert.NotEqual(SubscriptionStatus.Cancelled,sub.Status);
        var details=await subscriptions.GetDetailsAsync(sub.Id);Assert.Equal(7,details.FreezeAllowance);Assert.Single(details.Benefits,b=>b.Cancelled);Assert.Single(details.Grants,g=>g.CancellationRefundId!=null);
        Assert.Equal(2,await db.MembershipSubscriptionTransactions.CountAsync());Assert.Equal(2,await db.MemberSubscriptionBenefits.CountAsync());
    }
    [Fact] public async Task CancellationReopensOnlyItsSettledDebtsAndLaterRenewalSettlesAgainWithoutRewritingHistory()
    {
        Reception();var cash=await Account();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));var old=await db.MemberSubscriptions.SingleAsync();clock.Now=clock.Now.AddDays(30);
        for(var i=0;i<2;i++)await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id,ConfirmBorrow:true));
        plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();var renewal=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        var firstSettled=(await db.BorrowedGymSessions.FirstAsync()).SettledByTransactionId;
        await Finance().RefundAsync(renewal.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)],"Reverse renewal"));
        var history=await Entrance().GetBorrowedDebtHistoryAsync(member.Id,new());Assert.All(history.Items,d=>{Assert.Equal("Outstanding",d.Status);Assert.Equal(2,d.SettlementHistory!.Count);Assert.Equal("SettlementReversed",d.SettlementHistory[1].Action);});
        Assert.All(await db.BorrowedGymSessions.ToListAsync(),d=>{Assert.Equal(BorrowedSessionStatus.Settled,d.Status);Assert.Equal(firstSettled,d.SettledByTransactionId);Assert.Equal(old.Id,d.ExpiredSubscriptionId);});
        plan.SessionCount=1;await db.SaveChangesAsync();await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200))));
        plan=await db.MembershipPlans.SingleAsync();plan.SessionCount=12;await db.SaveChangesAsync();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        Assert.Equal(10,(await db.MemberSubscriptions.Where(s=>s.Status!=SubscriptionStatus.Cancelled && s.StartDate==today.AddDays(30)).SingleAsync()).RemainingSessions);
        Assert.All((await Entrance().GetBorrowedDebtHistoryAsync(member.Id,new())).Items,d=>{Assert.Equal("Settled",d.Status);Assert.Equal(3,d.SettlementHistory!.Count);Assert.NotEqual(firstSettled,d.SettledByTransactionId);});
        Assert.All(await db.BorrowedGymSessions.ToListAsync(),d=>Assert.Equal(firstSettled,d.SettledByTransactionId));
        var reversal=await db.BorrowedDebtSettlementEvents.FirstAsync(e=>e.IsReversal);reversal.IsReversal=false;await Assert.ThrowsAsync<InvalidOperationException>(()=>db.SaveChangesAsync());
    }
    [Fact] public async Task RefundPermissionBranchReasonAndPartsAreRequired()
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));var r=new RefundRequest(Guid.NewGuid(),100,[new(cash.Id,100)],"Decision");
        user.Context=user.Context! with{Permissions=[SalesPermissions.Create]};await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Finance().RefundAsync(sale.SaleId,r));
        user.Context=user.Context! with{Permissions=[SalesPermissions.Refund],CurrentBranch=new(a.Id,a.Name)};await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Finance().RefundAsync(sale.SaleId,r));
        user.Context=user.Context! with{CurrentBranch=new(b.Id,b.Name)};await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RefundAsync(sale.SaleId,r with{Reason=" "}));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RefundAsync(sale.SaleId,r with{Parts=[new(cash.Id,99)]}));Assert.Empty(await db.Refunds.ToListAsync());
    }
    [Fact] public async Task CancellingOlderPartlyUsedGrantPreservesLaterPaidSessionsAndTime()
    {
        var cash=await Account();plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();
        var first=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));var sub=await db.MemberSubscriptions.SingleAsync();sub.RemainingSessions=4;await db.SaveChangesAsync();
        clock.Now=clock.Now.AddDays(15);await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        await Finance().RefundAsync(first.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)],"Cancel older grant only"));
        sub=await db.MemberSubscriptions.SingleAsync();Assert.Equal(12,sub.RemainingSessions);Assert.Equal(12,sub.TotalSessions);
        Assert.Equal(today.AddDays(44),sub.EndDate);Assert.Equal(15,(await db.SubscriptionGrantCancellations.SingleAsync()).DaysRemoved);
    }
    [Fact] public async Task RefundInsertFailureDoesNotHalfCancelOrDebit()
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        await db.Database.ExecuteSqlRawAsync("CREATE FUNCTION reject_refund() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'forced refund'; END; $$; CREATE TRIGGER reject_refund BEFORE INSERT ON \"Refunds\" FOR EACH ROW EXECUTE FUNCTION reject_refund();");
        await Assert.ThrowsAsync<DbUpdateException>(()=>Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)],"Decision")));
        Assert.Null((await db.SaleTransactions.SingleAsync()).CancelledAt);Assert.NotEqual(SubscriptionStatus.Cancelled,(await db.MemberSubscriptions.SingleAsync()).Status);
        Assert.Empty(await db.Refunds.ToListAsync());Assert.Empty(await db.SubscriptionGrantCancellations.ToListAsync());Assert.Empty(await db.FinancialTransactions.Where(t=>t.TransactionType==FinancialTransactionType.Debit).ToListAsync());
    }
}
