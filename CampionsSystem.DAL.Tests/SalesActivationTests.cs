using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    private CheckInService Entrance() => new(db,user,access,subscriptions);
    private void Reception() => user.Context = user.Context! with { Permissions = [..user.Context!.Permissions, ..CheckInPermissions.Codes] };
    [Fact] public async Task PaidNowCreatesExactlyOneEntitlementInTheFinancialTransaction()
    {
        var cash = await Account(); var r = Purchase(new PaymentPartInput(cash.Id,1200)); user.Context = user.Context! with { Permissions = [SalesPermissions.Create] };
        var first = await Finance().CreateAsync(r); var retry = await Finance().CreateAsync(r); Assert.Equal(first.SaleId,retry.SaleId);
        var sub = await db.MemberSubscriptions.SingleAsync(); var grant = await db.MembershipSubscriptionTransactions.SingleAsync();
        Assert.Equal(today,sub.StartDate); Assert.Equal(today.AddDays(29),sub.EndDate); Assert.Null(sub.RemainingSessions);
        Assert.Equal(first.SaleId,grant.SaleTransactionId); Assert.Equal(1200,grant.NetPriceSnapshot); Assert.Single(await db.Payments.ToListAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>subscriptions.CreateCustomerGrantAsync(new(member.Id,plan.Id,b.Id,Guid.NewGuid())));
    }
    [Fact] public async Task SpecificDateDoesNotGrantEarlyAccessAndStartsOnChosenDay()
    {
        Reception(); var cash = await Account(); await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)) with { ActivationMode=SubscriptionActivationMode.SpecificDate,StartDate=today.AddDays(5) });
        Assert.Equal(AccessReasons.SubscriptionNotStarted,(await Entrance().EvaluateAsync(new(member.Id,null))).ReasonCode);
        clock.Now=clock.Now.AddDays(5); Assert.True((await Entrance().EvaluateAsync(new(member.Id,null))).Allowed);
        var sub=await db.MemberSubscriptions.SingleAsync(); Assert.Equal(today.AddDays(5),sub.StartDate); Assert.Equal(today.AddDays(34),sub.EndDate);
    }
    [Fact] public async Task FirstEntryIsUndatedUntilValidEntranceAndRetryActivatesOnlyOnce()
    {
        Reception(); var cash = await Account(); plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)) with { ActivationMode=SubscriptionActivationMode.FirstCheckIn });
        Assert.Empty(await db.MemberSubscriptions.ToListAsync()); var preview=await Entrance().EvaluateAsync(new(member.Id,null));
        Assert.False(preview.Allowed); Assert.True(preview.CanActivateOnCheckIn); Assert.Equal(AccessReasons.PendingFirstCheckIn,preview.ReasonCode);
        clock.Now=clock.Now.AddDays(4); var r=new CreateCheckInRequest(Guid.NewGuid(),CheckInMethod.Manual,member.Id);
        var entry=await Entrance().CreateAsync(r);var retry=await Entrance().CreateAsync(r);Assert.Equal(entry.CheckIn!.Id,retry.CheckIn!.Id);
        var sub=await db.MemberSubscriptions.SingleAsync();Assert.Equal(today.AddDays(4),sub.StartDate);Assert.Equal(today.AddDays(33),sub.EndDate);Assert.Equal(11,sub.RemainingSessions);
        Assert.Single(await db.MembershipSubscriptionTransactions.ToListAsync());Assert.Single(await db.CheckIns.ToListAsync());
    }
    [Fact] public async Task ClosureAndWrongCityDoNotActivateFirstEntryEntitlement()
    {
        Reception(); var cash=await Account();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)) with {ActivationMode=SubscriptionActivationMode.FirstCheckIn});
        db.Add(new BranchClosure {BranchId=b.Id,Reason="Closed",StartDate=today,EndDate=today,AppliesToGym=true,CreatedByUserId=actor.Id});await db.SaveChangesAsync();
        var blocked=await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id));Assert.Equal(AccessReasons.BranchClosed,blocked.Decision.ReasonCode);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());var wrong=await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id,BranchId:c.Id));Assert.False(wrong.Decision.Allowed);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task EarlyRenewalIsImmediateDespiteActivationChoiceAndPreservesCarry(bool sessions)
    {
        var cash=await Account();if(sessions){plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();}
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));var sub=await db.MemberSubscriptions.SingleAsync();
        if(sessions){sub.RemainingSessions=4;await db.SaveChangesAsync();}clock.Now=clock.Now.AddDays(15);
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)) with{ActivationMode=SubscriptionActivationMode.FirstCheckIn});
        sub=await db.MemberSubscriptions.SingleAsync();Assert.Equal(today.AddDays(59),sub.EndDate);Assert.Equal(sessions?16:null,sub.RemainingSessions);
        Assert.Equal(2,await db.MembershipSubscriptionTransactions.CountAsync());
    }
    [Fact] public async Task ScheduledModeChangePreservesCurrentPeriodAndFutureOwnQuantity()
    {
        var cash=await Account();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)) with{ActivationMode=SubscriptionActivationMode.SpecificDate,StartDate=today.AddDays(10)});
        var rows=await db.MemberSubscriptions.OrderBy(x=>x.StartDate).ToListAsync();Assert.Equal(2,rows.Count);Assert.Equal(today.AddDays(29),rows[0].EndDate);
        Assert.Equal(today.AddDays(30),rows[1].StartDate);Assert.Equal(12,rows[1].RemainingSessions);Assert.Equal(rows[0].Id,rows[1].FollowsSubscriptionId);
    }
    [Fact] public async Task MultiplePaidFirstEntryEntitlementsCannotConflict()
    {
        var cash=await Account();var r=Purchase(new PaymentPartInput(cash.Id,1200)) with{ActivationMode=SubscriptionActivationMode.FirstCheckIn};await Finance().CreateAsync(r);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().CreateAsync(r with{OperationId=Guid.NewGuid()}));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().CreateAsync(r with{OperationId=Guid.NewGuid(),ActivationMode=SubscriptionActivationMode.Now}));
        Assert.Single(await db.SaleTransactions.ToListAsync());Assert.Single(await db.Payments.ToListAsync());
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task CompatibleEarlyRenewalShiftsOneQueuedSuccessorWithoutAllowingAnotherFuturePurchase(bool sessions)
    {
        Reception();var cash=await Account();
        if(sessions){plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();}
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        var source=await db.MemberSubscriptions.SingleAsync();
        if(sessions){source.RemainingSessions=4;await db.SaveChangesAsync();}
        plan.AccessMode=sessions?AccessMode.Unlimited:AccessMode.SessionBased;plan.SessionCount=sessions?null:12;await db.SaveChangesAsync();
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        var future=await db.MemberSubscriptions.SingleAsync(x=>x.FollowsSubscriptionId==source.Id);
        var futureGrant=await db.MembershipSubscriptionTransactions.SingleAsync(x=>x.MemberSubscriptionId==future.Id);
        Assert.Equal(today.AddDays(30),future.StartDate);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200))));
        Assert.Equal(2,await db.SaleTransactions.CountAsync());Assert.Equal(2,await db.Payments.CountAsync());
        // Reattach after the rejected operation cleared tracking; snapshot history must remain unchanged.
        plan=await db.MembershipPlans.SingleAsync(x=>x.Id==plan.Id);
        plan.AccessMode=sessions?AccessMode.SessionBased:AccessMode.Unlimited;plan.SessionCount=sessions?12:null;await db.SaveChangesAsync();
        var renewal=Purchase(new PaymentPartInput(cash.Id,1200)) with{ActivationMode=SubscriptionActivationMode.FirstCheckIn};
        var first=await Finance().CreateAsync(renewal);var retry=await Finance().CreateAsync(renewal);Assert.Equal(first.SaleId,retry.SaleId);
        source=await db.MemberSubscriptions.SingleAsync(x=>x.Id==source.Id);future=await db.MemberSubscriptions.SingleAsync(x=>x.Id==future.Id);
        Assert.Equal(today.AddDays(59),source.EndDate);Assert.Equal(sessions?16:null,source.RemainingSessions);
        Assert.Equal(today.AddDays(60),future.StartDate);Assert.Equal(today.AddDays(89),future.EndDate);Assert.Equal(sessions?null:12,future.RemainingSessions);
        Assert.Equal(2,await db.MemberSubscriptions.CountAsync());Assert.Equal(3,await db.MembershipSubscriptionTransactions.CountAsync());
        Assert.Equal(today.AddDays(30),(await db.MembershipSubscriptionTransactions.SingleAsync(x=>x.Id==futureGrant.Id)).StartDateSnapshot);
        Assert.True((await Entrance().EvaluateAsync(new(member.Id,null))).Allowed);
    }
    [Fact] public async Task EntitlementInsertFailureRollsBackRealMoneyAndAllSnapshots()
    {
        var cash=await Account();await db.Database.ExecuteSqlRawAsync("CREATE FUNCTION reject_paid_grant() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'forced grant failure'; END; $$; CREATE TRIGGER reject_grant BEFORE INSERT ON \"MemberSubscriptions\" FOR EACH ROW EXECUTE FUNCTION reject_paid_grant();");
        await Assert.ThrowsAsync<DbUpdateException>(()=>Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200))));
        Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.PaymentParts.ToListAsync());Assert.Empty(await db.FinancialTransactions.ToListAsync());
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());Assert.Empty(await db.AuditLogs.ToListAsync());
    }
    [Fact] public async Task DeferredActivationUsesOriginalSoldTermsAfterPlanAndBenefitEdits()
    {
        Reception();var cash=await Account();var benefit=new MembershipBenefit{Code="GUEST",Name="Guest"};
        var config=new MembershipPlanBenefit{MembershipPlan=plan,MembershipBenefit=benefit,GrantType=MembershipBenefitGrantType.Quantity,Quantity=2};db.Add(config);await db.SaveChangesAsync();
        var offer=await offers.CreateAsync(OfferInput(type:OfferType.ExtraDays,value:7));
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)) with{OfferId=offer.Id,ActivationMode=SubscriptionActivationMode.FirstCheckIn});
        plan.DurationDays=25;plan.Price=2000;plan.CityId=c.CityId;plan.IsActive=false;config.Quantity=1;benefit.Name="Changed";await db.SaveChangesAsync();
        var result=await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id));Assert.True(result.Decision.Allowed);
        var grant=await db.MembershipSubscriptionTransactions.SingleAsync();Assert.Equal(37,grant.PurchasedDurationDays);Assert.Equal(1200,grant.PlanPriceSnapshot);
        Assert.Equal(today.AddDays(36),grant.NewEndDate);Assert.Equal(a.CityId,grant.CityIdSnapshot);var snap=await db.MemberSubscriptionBenefits.SingleAsync();Assert.Equal(2,snap.GrantedQuantity);Assert.Equal("Guest",snap.BenefitNameSnapshot);
    }
    [Fact] public async Task FirstEntryUsesActualVisitedBranchBusinessDateRatherThanSaleOriginDate()
    {
        Reception();var cash=await Account();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)) with{ActivationMode=SubscriptionActivationMode.FirstCheckIn});
        var day=new BusinessDay{BranchId=a.Id,BusinessDate=today.AddDays(-1),OpenedAt=clock.Now.AddHours(-14),Status=BusinessDayStatus.Open};db.Add(day);await db.SaveChangesAsync();
        var result=await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id,BranchId:a.Id));Assert.True(result.Decision.Allowed);
        var sub=await db.MemberSubscriptions.SingleAsync();Assert.Equal(today.AddDays(-1),sub.StartDate);Assert.Equal(a.Id,result.CheckIn!.BranchId);
        Assert.Equal(day.Id,(await db.MembershipSubscriptionTransactions.SingleAsync()).BusinessDayId);Assert.Equal(b.Id,sub.PurchasedAtBranchId);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task PaidExpiredRenewalSettlesCycleDebtAndPreservesUnusedSessions(bool unlimited)
    {
        Reception();var cash=await Account();plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        var old=await db.MemberSubscriptions.SingleAsync();old.RemainingSessions=4;await db.SaveChangesAsync();clock.Now=clock.Now.AddDays(30);
        await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id,ConfirmBorrow:true));await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id,ConfirmBorrow:true));
        if(unlimited){plan.AccessMode=AccessMode.Unlimited;plan.SessionCount=null;await db.SaveChangesAsync();}
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));var rows=await db.MemberSubscriptions.OrderBy(x=>x.StartDate).ToListAsync();
        Assert.Equal(4,rows[0].RemainingSessions);Assert.Equal(today.AddDays(30),rows[1].StartDate);Assert.Equal(unlimited?null:10,rows[1].RemainingSessions);
        var debts=await db.BorrowedGymSessions.ToListAsync();Assert.Equal(2,debts.Count);Assert.All(debts,d=>{Assert.Equal(BorrowedSessionStatus.Settled,d.Status);Assert.Equal(old.Id,d.ExpiredSubscriptionId);Assert.Equal(rows[1].Id,d.SettledBySubscriptionId);Assert.NotNull(d.SettledByTransactionId);});
    }

    [Fact] public async Task ExhaustedPeriodCanReceivePaidRenewalWithoutBlockingNewSessionAccess()
    {
        Reception();var cash=await Account();plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        var old=await db.MemberSubscriptions.SingleAsync();old.RemainingSessions=0;await db.SaveChangesAsync();
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)) with{ActivationMode=SubscriptionActivationMode.FirstCheckIn});
        var entry=await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id));Assert.True(entry.Decision.Allowed);
        Assert.Equal(2,await db.MemberSubscriptions.CountAsync());Assert.Equal(11,entry.CheckIn!.SessionsAfter);Assert.Equal(0,(await db.MemberSubscriptions.SingleAsync(x=>x.Id==old.Id)).RemainingSessions);
    }

}
