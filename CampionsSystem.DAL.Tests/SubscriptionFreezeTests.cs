using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SubscriptionLifecycleTests
{
    private async Task<FreezeDetails> Freeze(Guid id, int days, DateOnly? start=null, Guid? op=null)
        => await service.StartFreezeAsync(id,new(op??Guid.NewGuid(),(await service.GetDetailsAsync(id)).Subscription.Version,start??access.Today,days));
    [Fact] public async Task FreezeConsumesSnapshotAllowanceExtendsDatesAndCompletesWithoutDeletingHistory()
    {
        var grant=await Grant();var op=Guid.NewGuid();var f=await Freeze(grant.SubscriptionId,7,op:op);
        Assert.Equal(f,await service.StartFreezeAsync(grant.SubscriptionId,new(op,0,today,7)));
        plan.IncludedFreezeDays=1;await db.SaveChangesAsync();var d=await service.GetDetailsAsync(grant.SubscriptionId);
        Assert.Equal(14,d.FreezeAllowance);Assert.Equal(7,d.ConsumedFreezeDays);Assert.Equal(7,d.RemainingFreezeDays);
        Assert.Equal(today.AddDays(36),d.Subscription.EndDate);Assert.Equal("Frozen",d.Subscription.Status);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CompleteFreezeAsync(grant.SubscriptionId,f.Id,new(d.Subscription.Version)));
        clock.Now=clock.Now.AddDays(7);var completed=await service.CompleteFreezeAsync(grant.SubscriptionId,f.Id,new(d.Subscription.Version));
        Assert.NotNull(completed.CompletedAt);Assert.Equal(completed,await service.CompleteFreezeAsync(grant.SubscriptionId,f.Id,new(0)));
        Assert.Equal("Active",(await service.GetDetailsAsync(grant.SubscriptionId)).Subscription.Status);Assert.Single(await db.SubscriptionFreezes.ToListAsync());
        Assert.Contains(await db.AuditLogs.ToListAsync(),x=>x.Action=="Subscription.FreezeCompleted");
    }
    [Theory] [InlineData(0)] [InlineData(-1)] [InlineData(15)]
    public async Task InvalidFreezeDurationOrAllowanceCannotChangeEntitlement(int days)
    {
        var grant=await Grant();await Assert.ThrowsAsync<BusinessRuleException>(()=>Freeze(grant.SubscriptionId,days));
        Assert.Empty(await db.SubscriptionFreezes.ToListAsync());Assert.Equal(today.AddDays(29),(await service.GetDetailsAsync(grant.SubscriptionId)).Subscription.EndDate);
    }
    [Fact] public async Task OverlapExcessAllowanceAndExpiredFreezeAreRejected()
    {
        var grant=await Grant();await Freeze(grant.SubscriptionId,7,start:today.AddDays(2));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Freeze(grant.SubscriptionId,3,start:today.AddDays(4)));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Freeze(grant.SubscriptionId,8,start:today.AddDays(12)));
        clock.Now=clock.Now.AddDays(40);await Assert.ThrowsAsync<BusinessRuleException>(()=>Freeze(grant.SubscriptionId,1));
        Assert.Single(await db.SubscriptionFreezes.ToListAsync());
    }
    [Fact] public async Task FreezeMovesQueuedSuccessorAndDoesNotDeductSessions()
    {
        plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();var current=await Grant();
        var nextPlan=new MembershipPlan{Name="Unlimited",BranchId=b.Id,CityId=a.CityId,DurationDays=30};db.Add(nextPlan);await db.SaveChangesAsync();
        var future=await Renew(current.SubscriptionId,nextPlan.Id);await Freeze(current.SubscriptionId,7);
        var old=await service.GetDetailsAsync(current.SubscriptionId);var next=await service.GetDetailsAsync(future.SubscriptionId);
        Assert.Equal(12,old.Subscription.RemainingSessions);Assert.Equal(old.Subscription.EndDate!.Value.AddDays(1),next.Subscription.StartDate);
        Assert.Equal(today.AddDays(66),next.Subscription.EndDate);Assert.Empty(await db.CheckIns.ToListAsync());
    }
    [Fact] public async Task RegistrationOwnershipDoesNotAllowUnrelatedBranchFreeze()
    {
        var grant=await Grant();user.Context=user.Context! with {Permissions=[SubscriptionPermissions.Freeze,"Members.OwnSalesCustomers"],CurrentBranch=new(a.Id,a.Name)};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.StartFreezeAsync(grant.SubscriptionId,new(Guid.NewGuid(),0,today,1)));
        Assert.Empty(await db.SubscriptionFreezes.ToListAsync());
    }
}
public sealed partial class SubscriptionConcurrencyTests
{
    [Fact] public async Task IndependentFreezeRequestsCannotConsumeOneAllowanceTwice()
    {
        async Task<bool> Attempt()
        {
            await using var db=Context();try{await Service(db).StartFreezeAsync(subscription,new(Guid.NewGuid(),0,new(2026,10,10),10));return true;}
            catch(DbUpdateConcurrencyException){return false;}
        }
        Assert.Single(await Task.WhenAll(Attempt(),Attempt()),x=>x);await using var verify=Context();
        Assert.Single(await verify.SubscriptionFreezes.ToListAsync());Assert.Equal(10,(await verify.SubscriptionFreezes.SingleAsync()).DaysApplied);
        Assert.Equal(new DateOnly(2026,11,18),(await verify.MemberSubscriptions.SingleAsync()).EndDate);
    }
}
public sealed partial class SubscriptionLifecycleTests
{
    [Fact] public async Task FreezeAfterCompensatedHolidayAddsOnlyUnprotectedDays()
    {
        var result=await Grant();var sub=await db.MemberSubscriptions.SingleAsync();var original=sub.EndDate!.Value;
        var closure=new BranchClosure{BranchId=b.Id,StartDate=today,EndDate=today,ExtensionDays=1,AppliesToGym=true,Reason="Official",CreatedByUserId=actor.Id};
        sub.EndDate=original.AddDays(1);db.Add(new SubscriptionHolidayExtension{MemberSubscriptionId=sub.Id,BranchClosure=closure,
            ProtectedDate=today,DaysApplied=1,PreviousEndDate=original,NewEndDate=sub.EndDate.Value,CreatedByUserId=actor.Id,CreatedAt=access.Now});await db.SaveChangesAsync();
        var f=await Freeze(result.SubscriptionId,7);Assert.Equal(7,f.DaysApplied);Assert.Equal(1,f.DaysAlreadyCompensated);
        var d=await service.GetDetailsAsync(sub.Id);Assert.Equal(original.AddDays(7),d.Subscription.EndDate);
        Assert.Equal(7,d.ConsumedFreezeDays);Assert.Equal(7,d.RemainingFreezeDays);
        var row=await db.SubscriptionFreezes.SingleAsync();row.DaysApplied=8;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>db.SaveChangesAsync());
    }
}
