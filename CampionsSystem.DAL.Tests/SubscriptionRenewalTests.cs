using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SubscriptionLifecycleTests
{
    private async Task<SubscriptionGrantResult> Renew(Guid id, Guid? planId=null, Guid? op=null, int? version=null)
        => await service.RenewCustomerAsync(new(id,planId??plan.Id,b.Id,op??Guid.NewGuid(),version??(await service.GetDetailsAsync(id)).Subscription.Version));
    [Fact] public async Task ActiveUnlimitedRenewalPreservesExactlyRemainingTimeAndGrantHistory()
    {
        var first=await Grant();clock.Now=clock.Now.AddDays(15);var renewed=await Renew(first.SubscriptionId);
        Assert.Equal(first.SubscriptionId,renewed.SubscriptionId);var d=await service.GetDetailsAsync(first.SubscriptionId);
        Assert.Equal(today.AddDays(59),d.Subscription.EndDate);Assert.Equal(45,d.Subscription.EndDate!.Value.DayNumber-access.Today.DayNumber+1);
        Assert.Equal(2,d.Grants.Count);Assert.Equal(today.AddDays(29),d.Grants.Single(g=>g.Id==first.GrantId).NewEndDate);
        Assert.Null(d.Subscription.RemainingSessions);Assert.Null(d.Subscription.TotalSessions);
    }
    [Fact] public async Task ActiveSessionRenewalCarriesFourPlusTwelveAndTime()
    {
        plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();var first=await Grant();
        await db.MemberSubscriptions.Where(s=>s.Id==first.SubscriptionId).ExecuteUpdateAsync(x=>x.SetProperty(s=>s.RemainingSessions,4));
        clock.Now=clock.Now.AddDays(15);await Renew(first.SubscriptionId);var d=await service.GetDetailsAsync(first.SubscriptionId);
        Assert.Equal(16,d.Subscription.RemainingSessions);Assert.Equal(24,d.Subscription.TotalSessions);Assert.Equal(today.AddDays(59),d.Subscription.EndDate);
        Assert.Equal(4,d.Grants.Single(g=>g.TransactionType=="Renewal").PreviousRemainingSessions);
    }
    [Fact] public async Task ExpiredRenewalCreatesNewCycleWithOnlyNewSessionsAndPreservesUnusedHistory()
    {
        plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();var first=await Grant();
        await db.MemberSubscriptions.Where(s=>s.Id==first.SubscriptionId).ExecuteUpdateAsync(x=>x.SetProperty(s=>s.RemainingSessions,4));
        clock.Now=clock.Now.AddDays(31);var next=await Renew(first.SubscriptionId);
        Assert.NotEqual(first.SubscriptionId,next.SubscriptionId);var d=await service.GetDetailsAsync(next.SubscriptionId);
        Assert.Equal(access.Today,d.Subscription.StartDate);Assert.Equal(access.Today.AddDays(29),d.Subscription.EndDate);Assert.Equal(12,d.Subscription.RemainingSessions);
        var old=await service.GetDetailsAsync(first.SubscriptionId);Assert.Equal("Expired",old.Subscription.Status);Assert.Equal(4,old.Subscription.RemainingSessions);
        Assert.Equal(today.AddDays(29),old.Subscription.EndDate);Assert.Equal(first.SubscriptionId,Assert.Single(d.Grants).RenewedFromSubscriptionId);
    }
    [Theory] [InlineData(true)] [InlineData(false)]
    public async Task DifferentModeQueuesOwnEntitlementWithoutMergingOrEarlyAccess(bool fromSessions)
    {
        if(fromSessions){plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();}
        var first=await Grant();if(fromSessions)await db.MemberSubscriptions.Where(s=>s.Id==first.SubscriptionId).ExecuteUpdateAsync(x=>x.SetProperty(s=>s.RemainingSessions,5));
        var nextPlan=new MembershipPlan{Name="Other mode",BranchId=b.Id,CityId=a.CityId,DurationDays=90,Price=2000,
            AccessMode=fromSessions?AccessMode.Unlimited:AccessMode.SessionBased,SessionCount=fromSessions?null:12};db.Add(nextPlan);await db.SaveChangesAsync();
        clock.Now=clock.Now.AddDays(15);var next=await Renew(first.SubscriptionId,nextPlan.Id);Assert.True(next.Scheduled);
        var future=await service.GetDetailsAsync(next.SubscriptionId);Assert.Equal("Scheduled",future.Subscription.Status);Assert.Equal(today.AddDays(30),future.Subscription.StartDate);
        Assert.Equal(fromSessions?null:12,future.Subscription.RemainingSessions);Assert.Equal(first.SubscriptionId,(await service.GetCurrentEntitlementAsync(member.Id,b.Id)).Subscription!.Id);
        Assert.Equal(fromSessions?5:null,(await service.GetDetailsAsync(first.SubscriptionId)).Subscription.RemainingSessions);
        clock.Now=clock.Now.AddDays(15);Assert.Equal("Expired",(await service.GetDetailsAsync(first.SubscriptionId)).Subscription.Status);
        Assert.Equal(next.SubscriptionId,(await service.GetCurrentEntitlementAsync(member.Id,b.Id)).Subscription!.Id);
    }
    [Fact] public async Task CompatibleRenewalCanSelectAnotherPlanAndRetryIsIdempotent()
    {
        var first=await Grant();var p=new MembershipPlan{Name="Quarter",BranchId=b.Id,CityId=a.CityId,DurationDays=90,Price=2500};db.Add(p);await db.SaveChangesAsync();
        var op=Guid.NewGuid();var next=await Renew(first.SubscriptionId,p.Id,op,0);Assert.Equal(next,await Renew(first.SubscriptionId,p.Id,op,0));
        Assert.Equal(today.AddDays(119),(await service.GetDetailsAsync(first.SubscriptionId)).Subscription.EndDate);
        Assert.Equal(2,await db.MembershipSubscriptionTransactions.CountAsync());
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(()=>Renew(first.SubscriptionId,p.Id,version:0));
    }
    [Fact] public async Task SecondPendingPeriodIsRejected()
    {
        var first=await Grant();var p=new MembershipPlan{Name="Sessions",BranchId=b.Id,CityId=a.CityId,DurationDays=30,AccessMode=AccessMode.SessionBased,SessionCount=12};db.Add(p);await db.SaveChangesAsync();
        await Renew(first.SubscriptionId,p.Id);await Assert.ThrowsAsync<BusinessRuleException>(()=>Renew(first.SubscriptionId,p.Id));
        Assert.Equal(2,await db.MemberSubscriptions.CountAsync());
    }
}
