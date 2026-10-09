using CampionsSystem.BLL.Plans;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class PlanManagementTests
{
    [Fact] public async Task UpdateAndStatusPreserveHistoricalCommercialData()
    {
        var p=await service.CreatePlanAsync(Request());
        var member=new Member{FullName="Historical",PrimaryPhone="historical",MemberCode="HIST",ClientCode=1,HomeBranchId=a.Id,CreatedByUserId=actor.Id};
        var subscription=new MemberSubscription{Member=member,MembershipPlanId=p.Plan.Id,CityId=a.CityId,PurchasedAtBranchId=a.Id,StartDate=new(2026,1,1),EndDate=new(2026,1,31)};
        var sale=new SaleTransaction{Member=member,BranchId=a.Id,CreatedByUserId=actor.Id,SaleType=SaleType.GymMembership,GrossAmount=1200,NetAmount=1200};
        var transaction=new MembershipSubscriptionTransaction{MemberSubscription=subscription,MembershipPlanId=p.Plan.Id,SaleTransaction=sale,PurchasedDurationDays=30,PlanPriceSnapshot=1200,NetPriceSnapshot=1200,NewEndDate=subscription.EndDate.Value};
        db.Add(transaction);await db.SaveChangesAsync();
        var r=Request();r.Price=1500;r.DurationDays=90;r.Mode=AccessMode.SessionBased;r.Sessions=12;
        Branch("MembershipPlans.Manage");await service.UpdatePlanAsync(p.Plan.Id,r);await service.ChangePlanStatusAsync(p.Plan.Id,new(){IsActive=false});
        db.ChangeTracker.Clear();Assert.False((await db.MembershipPlans.SingleAsync()).IsActive);
        Assert.Equal(1200,(await db.MembershipSubscriptionTransactions.SingleAsync()).PlanPriceSnapshot);
        Assert.Equal(30,(await db.MembershipSubscriptionTransactions.SingleAsync()).PurchasedDurationDays);
        Assert.Equal(new DateOnly(2026,1,31),(await db.MemberSubscriptions.SingleAsync()).EndDate);
        Assert.Equal(1200,(await db.SaleTransactions.SingleAsync()).NetAmount);
        Assert.Contains(await db.AuditLogs.ToListAsync(),x=>x.Action=="MembershipPlan.StatusChanged");
        await service.ChangePlanStatusAsync(p.Plan.Id,new(){IsActive=true});
    }
    [Fact] public async Task UpdateCannotBypassBranchScopeOrChangeBehavioralType()
    {
        var p=await service.CreatePlanAsync(Request());Branch("MembershipPlans.Manage");
        var r=Request();r.BranchId=b.Id;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.UpdatePlanAsync(p.Plan.Id,r));
        r=Request();r.PlanType=PlanCategory.Employee;r.Price=0;r.DurationDays=0;
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.UpdatePlanAsync(p.Plan.Id,r));
        var other=Template("Other",b);other.Branch=null;other.BranchId=b.Id;db.Add(other);await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.ChangePlanStatusAsync(other.Id,new(){IsActive=false}));
    }
    [Fact] public async Task LegacyPlansRequireGlobalExplicitMappingAndInactiveBranchesCanBeDisabled()
    {
        var legacy=Template();legacy.Branch=null;legacy.BranchId=null;db.Add(legacy);await db.SaveChangesAsync();Branch("MembershipPlans.Manage");
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.UpdatePlanAsync(legacy.Id,Request()));Global();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.ChangePlanStatusAsync(legacy.Id,new(){IsActive=true}));
        await service.UpdatePlanAsync(legacy.Id,Request());await db.GymBranches.Where(x=>x.Id==a.Id).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.IsActive,false));
        Assert.False((await service.ChangePlanStatusAsync(legacy.Id,new(){IsActive=false})).Plan.IsActive);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.ChangePlanStatusAsync(legacy.Id,new(){IsActive=true}));
    }
}
