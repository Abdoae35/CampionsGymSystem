using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SubscriptionLifecycleTests
{
    [Fact] public async Task CustomMedicalExtensionIsSeparateFromNormalAllowanceAndIdempotent()
    {
        var grant=await Grant();var op=Guid.NewGuid();var request=new CustomExtensionRequest(op,0,21,"Injury treatment");
        var result=await service.GrantCustomExtensionAsync(grant.SubscriptionId,request);
        Assert.Equal(result,await service.GrantCustomExtensionAsync(grant.SubscriptionId,request));
        var d=await service.GetDetailsAsync(grant.SubscriptionId);Assert.Equal("Medical",result.Type);Assert.Equal("Injury treatment",result.Reason);
        Assert.Equal(today.AddDays(50),d.Subscription.EndDate);Assert.Equal(0,d.ConsumedFreezeDays);Assert.Equal(14,d.RemainingFreezeDays);
        Assert.Equal("Active",d.Subscription.Status);Assert.True((await service.GetCurrentEntitlementAsync(member.Id,b.Id)).HasAccess);
        Assert.Single(d.Freezes);Assert.Equal(actor.Id,result.ApprovedBy);Assert.Equal(today.AddDays(29),result.PreviousSubscriptionEndDate);
        Assert.Contains(await db.AuditLogs.ToListAsync(),x=>x.Action=="Subscription.CustomExtensionGranted");
    }
    [Theory] [InlineData(0,"Reason")] [InlineData(-1,"Reason")] [InlineData(7,"")] [InlineData(7,"  ")]
    public async Task CustomExtensionRequiresPositiveDurationAndReason(int days,string reason)
    {
        var grant=await Grant();await Assert.ThrowsAsync<BusinessRuleException>(()=>service.GrantCustomExtensionAsync(grant.SubscriptionId,new(Guid.NewGuid(),0,days,reason)));
        Assert.Empty(await db.SubscriptionFreezes.ToListAsync());Assert.Equal(today.AddDays(29),(await service.GetDetailsAsync(grant.SubscriptionId)).Subscription.EndDate);
    }
    [Fact] public async Task CustomExtensionRequiresElevatedCapabilityAndOriginScope()
    {
        var grant=await Grant();user.Context=user.Context! with {Permissions=[SubscriptionPermissions.Freeze,"Branches.AccessAll"]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.GrantCustomExtensionAsync(grant.SubscriptionId,new(Guid.NewGuid(),0,7,"Medical")));
        user.Context=user.Context! with {Permissions=[SubscriptionPermissions.Extend],CurrentBranch=new(a.Id,a.Name)};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.GrantCustomExtensionAsync(grant.SubscriptionId,new(Guid.NewGuid(),0,7,"Medical")));
    }
    [Fact] public async Task CustomExtensionMovesUnstartedSuccessorWithoutFreezingAccess()
    {
        var current=await Grant();var other=new MembershipPlan{Name="Session",BranchId=b.Id,CityId=a.CityId,DurationDays=30,
            AccessMode=CampionsSystem.DAL.Enums.AccessMode.SessionBased,SessionCount=12};db.Add(other);await db.SaveChangesAsync();
        var future=await Renew(current.SubscriptionId,other.Id);await service.GrantCustomExtensionAsync(current.SubscriptionId,new(Guid.NewGuid(),1,7,"Medical"));
        var d=await service.GetDetailsAsync(current.SubscriptionId);Assert.Equal("Active",d.Subscription.Status);
        Assert.Equal(d.Subscription.EndDate!.Value.AddDays(1),(await service.GetDetailsAsync(future.SubscriptionId)).Subscription.StartDate);
        Assert.Equal(0,d.ConsumedFreezeDays);
    }
}
