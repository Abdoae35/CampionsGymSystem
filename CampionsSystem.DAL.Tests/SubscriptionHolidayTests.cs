using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SubscriptionLifecycleTests
{
    [Fact] public async Task HolidayDoesNotAddArtificialExpiryToEmploymentEntitlement()
    {
        var employee=await Staff();
        var grant=await service.StartEmployeeEntitlementAsync(employee.Id,new(member.Id,plan.Id,Guid.NewGuid()));
        var result=await service.CreateOfficialHolidayAsync(new(b.Id,today,today,true,"Official closure"));
        Assert.Equal(0,result.ExtendedDays);
        Assert.Null((await service.GetDetailsAsync(grant.SubscriptionId)).Subscription.EndDate);
        Assert.Empty(await db.SubscriptionHolidayExtensions.ToListAsync());
    }
    private Task<HolidayApplicationResult> Holiday(bool compensate=true, DateOnly? start=null, DateOnly? end=null)
        => service.CreateOfficialHolidayAsync(new(b.Id,start??access.Today,end??start??access.Today,compensate,"Eid"));
    [Fact] public async Task OwnerSelectedOriginBranchControlsCompensationRatherThanCityOrMemberHome()
    {
        var first=await Grant();
        var otherMember=new Member{FullName="Member B",PrimaryPhone="holiday-other",MemberCode="HOL-2",HomeBranch=b,CreatedByUserId=actor.Id};
        var otherPlan=new MembershipPlan{Name="A plan",BranchId=a.Id,CityId=a.CityId,DurationDays=30};db.AddRange(otherMember,otherPlan);await db.SaveChangesAsync();
        var second=await service.CreateCustomerGrantAsync(new(otherMember.Id,otherPlan.Id,a.Id,Guid.NewGuid()));
        var result=await Holiday();Assert.Equal(1,result.ExtendedDays);Assert.Equal(today.AddDays(30),(await service.GetDetailsAsync(first.SubscriptionId)).Subscription.EndDate);
        Assert.Equal(today.AddDays(29),(await service.GetDetailsAsync(second.SubscriptionId)).Subscription.EndDate);
        Assert.Equal(0,(await service.ApplyOfficialHolidayAsync(result.ClosureId)).ExtendedDays);
        Assert.Single((await service.GetDetailsAsync(first.SubscriptionId)).Holidays);
        Assert.False((await service.GetCurrentEntitlementAsync(member.Id,b.Id)).HasAccess);
        Assert.True((await service.GetCurrentEntitlementAsync(member.Id,a.Id)).HasAccess); // another branch being open never controls compensation
    }
    [Fact] public async Task HolidayWithoutCompensationDoesNotChangeSubscription()
    {
        var first=await Grant();var before=await service.GetDetailsAsync(first.SubscriptionId);var result=await Holiday(false);
        Assert.Equal(0,result.ExtendedDays);Assert.Equal(before.Subscription,(await service.GetDetailsAsync(first.SubscriptionId)).Subscription);
        Assert.Empty(await db.SubscriptionHolidayExtensions.ToListAsync());Assert.False((await service.GetCurrentEntitlementAsync(member.Id,b.Id)).HasAccess);
        Assert.Equal(0,(await db.BranchClosures.SingleAsync()).ExtensionDays);
    }
    [Fact] public async Task SevenFrozenDaysIncludingHolidayProduceSevenDaysOfProtectionOnly()
    {
        var first=await Grant();await Freeze(first.SubscriptionId,7);clock.Now=clock.Now.AddDays(4);
        var result=await Holiday();Assert.Equal(1,result.ProtectedDays);Assert.Equal(0,result.ExtendedDays);
        var d=await service.GetDetailsAsync(first.SubscriptionId);Assert.Equal(today.AddDays(36),d.Subscription.EndDate);
        Assert.Equal(0,Assert.Single(d.Holidays).DaysApplied);Assert.Equal(7,d.ConsumedFreezeDays);
        Assert.Equal(0,(await service.ApplyOfficialHolidayAsync(result.ClosureId)).ProtectedDays);
    }
    [Fact] public async Task HolidayThenFreezeAlsoProtectsSameDayOnlyOnce()
    {
        var first=await Grant();await Holiday();var freeze=await Freeze(first.SubscriptionId,7);
        Assert.Equal(1,freeze.DaysAlreadyCompensated);Assert.Equal(today.AddDays(36),(await service.GetDetailsAsync(first.SubscriptionId)).Subscription.EndDate);
    }
    [Fact] public async Task HolidaySkipsExpiredFutureCancelledAndEmploymentEntitlements()
    {
        var old=await Grant();clock.Now=clock.Now.AddDays(31);var active=await Renew(old.SubscriptionId);
        var other=new MembershipPlan{Name="Future",BranchId=b.Id,CityId=a.CityId,DurationDays=30,AccessMode=AccessMode.SessionBased,SessionCount=12};db.Add(other);await db.SaveChangesAsync();
        var future=await Renew(active.SubscriptionId,other.Id);var result=await Holiday();Assert.Equal(1,result.ExtendedDays);
        Assert.Empty((await service.GetDetailsAsync(old.SubscriptionId)).Holidays);Assert.Empty((await service.GetDetailsAsync(future.SubscriptionId)).Holidays);
        Assert.Equal(today.AddDays(29),(await service.GetDetailsAsync(old.SubscriptionId)).Subscription.EndDate);
        var activeRow=await db.MemberSubscriptions.SingleAsync(x=>x.Id==active.SubscriptionId);activeRow.Status=SubscriptionStatus.Cancelled;await db.SaveChangesAsync();
        clock.Now=clock.Now.AddDays(1);Assert.Equal(0,(await Holiday()).ExtendedDays);
    }
    [Fact] public async Task FutureHolidayIsDeferredUntilItsBusinessDateAndMovesQueuedSuccessor()
    {
        var first=await Grant();var other=new MembershipPlan{Name="Future",BranchId=b.Id,CityId=a.CityId,DurationDays=30,AccessMode=AccessMode.SessionBased,SessionCount=12};db.Add(other);await db.SaveChangesAsync();
        var future=await Renew(first.SubscriptionId,other.Id);var holiday=await Holiday(start:today.AddDays(2));Assert.Equal(0,holiday.ExtendedDays);
        clock.Now=clock.Now.AddDays(2);Assert.Equal(1,(await service.ApplyOfficialHolidayAsync(holiday.ClosureId)).ExtendedDays);
        var current=await service.GetDetailsAsync(first.SubscriptionId);Assert.Equal(current.Subscription.EndDate!.Value.AddDays(1),(await service.GetDetailsAsync(future.SubscriptionId)).Subscription.StartDate);
    }
    [Fact] public async Task GlobalCapabilityIsRequiredForMassHolidayCompensation()
    {
        await Grant();user.Context=user.Context! with {Permissions=[SubscriptionPermissions.Extend],CurrentBranch=new(b.Id,b.Name)};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Holiday());Assert.Empty(await db.BranchClosures.ToListAsync());
    }
    [Fact] public async Task ExistingDeclaredHolidayAppliesToSubsequentGrantWithoutFinance()
    {
        await Holiday();var first=await Grant();Assert.Equal(today.AddDays(30),(await service.GetDetailsAsync(first.SubscriptionId)).Subscription.EndDate);
        Assert.Single(await db.SubscriptionHolidayExtensions.ToListAsync());Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());
    }
    [Fact] public async Task OverlappingOfficialClosuresCannotCompensateSameDayTwice()
    {
        var first=await Grant();clock.Now=clock.Now.AddDays(2);
        Assert.Equal(2,(await Holiday(start:today,end:today.AddDays(1))).ExtendedDays);
        Assert.Equal(1,(await Holiday(start:today.AddDays(1),end:today.AddDays(2))).ExtendedDays);
        Assert.Equal(today.AddDays(32),(await service.GetDetailsAsync(first.SubscriptionId)).Subscription.EndDate);Assert.Equal(3,await db.SubscriptionHolidayExtensions.CountAsync());
    }
}
public sealed partial class SubscriptionConcurrencyTests
{
    private async Task<Guid> DeclaredHoliday()
    {
        await using var db=Context();var c=new BranchClosure{BranchId=branch,StartDate=new(2026,10,10),EndDate=new(2026,10,10),
            ExtensionDays=1,AppliesToGym=true,Reason="Concurrent holiday",CreatedByUserId=actor};db.Add(c);await db.SaveChangesAsync();return c.Id;
    }
    [Fact] public async Task IndependentHolidayApplicationsAreIdempotent()
    {
        var id=await DeclaredHoliday();async Task<HolidayApplicationResult> Apply(){await using var db=Context();return await Service(db).ApplyOfficialHolidayAsync(id);}
        var results=await Task.WhenAll(Apply(),Apply());Assert.Equal(1,results.Sum(x=>x.ExtendedDays));
        await using var verify=Context();Assert.Single(await verify.SubscriptionHolidayExtensions.ToListAsync());
        Assert.Equal(new DateOnly(2026,11,9),(await verify.MemberSubscriptions.SingleAsync()).EndDate);
    }
    [Fact] public async Task RacingFreezeAndHolidayCannotProtectOneDayTwice()
    {
        var id=await DeclaredHoliday();var op=Guid.NewGuid();
        async Task<bool> Freeze(){await using var db=Context();try{await Service(db).StartFreezeAsync(subscription,new(op,0,new(2026,10,10),7));return true;}catch(DbUpdateConcurrencyException){return false;}}
        async Task Holiday(){await using var db=Context();await Service(db).ApplyOfficialHolidayAsync(id);}
        var freezing=Freeze();await Task.WhenAll(freezing,Holiday());
        await using var verify=Context();if(!await freezing)
        {
            var version=(await Service(verify).GetDetailsAsync(subscription)).Subscription.Version;
            await Service(verify).StartFreezeAsync(subscription,new(op,version,new(2026,10,10),7));
        }
        Assert.Equal(new DateOnly(2026,11,15),(await verify.MemberSubscriptions.SingleAsync()).EndDate);
        Assert.Single(await verify.SubscriptionFreezes.ToListAsync());Assert.Single(await verify.SubscriptionHolidayExtensions.ToListAsync());
    }
}
