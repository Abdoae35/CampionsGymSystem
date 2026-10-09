using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CheckInTests
{
    private async Task<Guid> BorrowedCycle()
    {
        plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();
        var grant=await subscriptions.CreateCustomerGrantAsync(new(member.Id,plan.Id,a.Id,Guid.NewGuid()));
        await db.MemberSubscriptions.Where(x=>x.Id==grant.SubscriptionId).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.RemainingSessions,4));
        clock.Now=clock.Now.AddDays(30);await Entry(borrow:true);await Entry(borrow:true);return grant.SubscriptionId;
    }
    private async Task<CampionsSystem.BLL.Subscriptions.SubscriptionGrantResult> RenewCycle(Guid id,Guid? op=null)
    {
        var version=(await subscriptions.GetDetailsAsync(id)).Subscription.Version;
        return await subscriptions.RenewCustomerAsync(new(id,plan.Id,a.Id,op??Guid.NewGuid(),version));
    }
    [Theory][InlineData(AccessMode.SessionBased)][InlineData(AccessMode.Unlimited)]
    public async Task RenewalSettlesFullDebtAndPreservesBothCycleAndExactGrantHistory(AccessMode mode)
    {
        var old=await BorrowedCycle();plan.AccessMode=mode;plan.SessionCount=mode==AccessMode.SessionBased?12:null;await db.SaveChangesAsync();
        var op=Guid.NewGuid();var renewed=await RenewCycle(old,op);Assert.NotEqual(old,renewed.SubscriptionId);
        var next=await db.MemberSubscriptions.SingleAsync(x=>x.Id==renewed.SubscriptionId);
        Assert.Equal(mode==AccessMode.SessionBased?10:null,next.RemainingSessions);Assert.Equal(mode==AccessMode.SessionBased?12:null,next.TotalSessions);
        var debts=await db.BorrowedGymSessions.ToListAsync();Assert.Equal(2,debts.Count);Assert.All(debts,x=>
        {Assert.Equal(old,x.ExpiredSubscriptionId);Assert.Equal(BorrowedSessionStatus.Settled,x.Status);Assert.Equal(renewed.SubscriptionId,x.SettledBySubscriptionId);Assert.Equal(renewed.GrantId,x.SettledByTransactionId);Assert.NotNull(x.SettledAt);Assert.NotNull(x.CheckInId);});
        Assert.Equal(4,(await db.MemberSubscriptions.SingleAsync(x=>x.Id==old)).RemainingSessions);
        Assert.Equal(2,await db.CheckIns.CountAsync());Assert.Equal(2,await db.AuditLogs.CountAsync(x=>x.Action=="BorrowedSessionDebt.Settled"));
        Assert.Equal(renewed,await RenewCycle(old,op));Assert.Equal(2,await db.MembershipSubscriptionTransactions.CountAsync());
        var oldOperation=await db.CheckIns.Where(x=>x.MemberSubscriptionId==old).Select(x=>x.OperationId).FirstAsync();
        var replay=await Entry(oldOperation,true);Assert.False(replay.Decision.BorrowAvailable);Assert.Equal(0,replay.Decision.BorrowedRemaining);
        clock.Now=clock.Now.AddDays(30);var d=await Evaluate();Assert.True(d.BorrowAvailable);Assert.Equal(0,d.BorrowedUsed);
        replay=await Entry(oldOperation,true);Assert.False(replay.Decision.BorrowAvailable);Assert.Equal(0,replay.Decision.BorrowedRemaining);
        Assert.True((await Entry(borrow:true)).Decision.Allowed);Assert.True((await Entry(borrow:true)).Decision.Allowed);
        Assert.Equal(AccessReasons.BorrowLimitReached,(await Entry(borrow:true)).Decision.ReasonCode);
        Assert.Equal(2,await db.BorrowedGymSessions.CountAsync(x=>x.ExpiredSubscriptionId==renewed.SubscriptionId));
    }
    [Fact] public async Task InsufficientNormalRenewalRejectsWithoutPartialSettlementOrNewCycle()
    {
        var old=await BorrowedCycle();plan.SessionCount=1;await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>RenewCycle(old));
        Assert.Single(await db.MemberSubscriptions.ToListAsync());Assert.Single(await db.MembershipSubscriptionTransactions.ToListAsync());
        Assert.Equal(2,await db.BorrowedGymSessions.CountAsync(x=>x.Status==BorrowedSessionStatus.Outstanding));Assert.Equal(4,(await db.MemberSubscriptions.SingleAsync()).RemainingSessions);
    }
    [Fact] public async Task InitialCustomerGrantCannotBypassOutstandingDebt()
    {
        await BorrowedCycle();var grant=await subscriptions.CreateCustomerGrantAsync(new(member.Id,plan.Id,a.Id,Guid.NewGuid()));
        Assert.Equal(10,(await db.MemberSubscriptions.SingleAsync(x=>x.Id==grant.SubscriptionId)).RemainingSessions);
        Assert.Empty(await db.BorrowedGymSessions.Where(x=>x.Status==BorrowedSessionStatus.Outstanding).ToListAsync());
    }
    [Fact] public async Task ExactDebtSizedGrantSettlesFullyWithoutNegativeSessions()
    {
        var old=await BorrowedCycle();plan.SessionCount=2;await db.SaveChangesAsync();var renewed=await RenewCycle(old);
        Assert.Equal(0,(await db.MemberSubscriptions.SingleAsync(x=>x.Id==renewed.SubscriptionId)).RemainingSessions);
        Assert.Equal(AccessReasons.NoSessionsRemaining,(await Evaluate()).ReasonCode);Assert.False((await Evaluate()).BorrowAvailable);
    }
}
public sealed partial class CheckInConcurrencyTests
{
    [Fact] public async Task BorrowRacingRenewalCannotLeaveDebtUnsettledOrDeductTwice()
    {
        await Expire();await Entry(Guid.NewGuid(),true);
        await using(var setup=Context())await setup.MembershipPlans.ExecuteUpdateAsync(s=>s.SetProperty(x=>x.SessionCount,12));
        async Task<bool> Renew()
        {
            await using var db=Context();try{await Services(db).Subscriptions.RenewCustomerAsync(new(subscription,plan,branch,Guid.NewGuid(),1));return true;}
            catch(DbUpdateConcurrencyException){return false;}
        }
        var renewal=Renew();await Task.WhenAll(renewal,Entry(Guid.NewGuid(),true));
        if(!await renewal){await using var retry=Context();var svc=Services(retry).Subscriptions;var v=(await svc.GetDetailsAsync(subscription)).Subscription.Version;await svc.RenewCustomerAsync(new(subscription,plan,branch,Guid.NewGuid(),v));}
        await using var verify=Context();var debtCount=await verify.BorrowedGymSessions.CountAsync();Assert.InRange(debtCount,1,2);
        Assert.Equal(0,await verify.BorrowedGymSessions.CountAsync(x=>x.Status==BorrowedSessionStatus.Outstanding));
        Assert.Equal(12-debtCount,(await verify.MemberSubscriptions.SingleAsync(x=>x.Id!=subscription)).RemainingSessions);
    }
}
