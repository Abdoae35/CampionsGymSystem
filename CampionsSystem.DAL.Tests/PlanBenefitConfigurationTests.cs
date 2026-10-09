using CampionsSystem.BLL.Plans;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Enums;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class PlanManagementTests
{
    [Fact] public async Task PlanBenefitsCanBeAddedUpdatedRemovedAndInactiveCatalogRetainedOnlyUnchanged()
    {
        var p=await service.CreatePlanAsync(Request());var benefit=await service.CreateBenefitAsync(new(){Code="INBODY",Name="InBody"});
        var r=new SetPlanBenefitsRequest{Benefits=[new(){BenefitId=benefit.Id,GrantType=MembershipBenefitGrantType.Quantity,Quantity=2}]};
        Branch("MembershipPlans.Manage","MembershipPlans.View");Assert.Equal(2,Assert.Single(await service.SetPlanBenefitsAsync(p.Plan.Id,r)).Quantity);
        r.Benefits[0].Quantity=3;Assert.Equal(3,Assert.Single(await service.SetPlanBenefitsAsync(p.Plan.Id,r)).Quantity);
        Global();await service.ChangeBenefitStatusAsync(benefit.Id,new(){IsActive=false});Branch("MembershipPlans.Manage","MembershipPlans.View");
        Assert.Single(await service.SetPlanBenefitsAsync(p.Plan.Id,r));r.Benefits[0].Quantity=4;
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.SetPlanBenefitsAsync(p.Plan.Id,r));
        Assert.Empty(await service.SetPlanBenefitsAsync(p.Plan.Id,new()));Assert.Empty(await service.GetPlanBenefitsAsync(p.Plan.Id));
        Assert.Equal(new[]{"MembershipPlan.BenefitAdded","MembershipPlan.BenefitUpdated","MembershipPlan.BenefitRemoved"},
            await db.AuditLogs.Where(x=>x.EntityType=="MembershipPlanBenefit").OrderBy(x=>x.CreatedAt).Select(x=>x.Action).ToArrayAsync());
    }
    [Theory][InlineData(MembershipBenefitGrantType.Quantity,null)][InlineData(MembershipBenefitGrantType.Quantity,0)]
    [InlineData(MembershipBenefitGrantType.Access,999999)][InlineData((MembershipBenefitGrantType)99,null)]
    public async Task InvalidBenefitGrantConfigurationRejected(MembershipBenefitGrantType type,int? quantity)
    {
        var p=await service.CreatePlanAsync(Request());var benefit=await service.CreateBenefitAsync(new(){Code="SAUNA",Name="Sauna"});
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.SetPlanBenefitsAsync(p.Plan.Id,new(){Benefits=[new(){BenefitId=benefit.Id,GrantType=type,Quantity=quantity}]}));
        Assert.Empty(await db.MembershipPlanBenefits.ToListAsync());
    }
    [Fact] public async Task AccessAndDuplicateBenefitRulesAreEnforced()
    {
        var p=await service.CreatePlanAsync(Request());var b=await service.CreateBenefitAsync(new(){Code="LOCKER",Name="Locker"});
        var entry=new PlanBenefitRequest{BenefitId=b.Id,GrantType=MembershipBenefitGrantType.Access};
        Assert.Null(Assert.Single(await service.SetPlanBenefitsAsync(p.Plan.Id,new(){Benefits=[entry]})).Quantity);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.SetPlanBenefitsAsync(p.Plan.Id,new(){Benefits=[entry,entry]}));
        Branch("MembershipPlans.View");await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.SetPlanBenefitsAsync(p.Plan.Id,new()));
    }
    [Fact] public async Task TemplateBenefitRemovalDoesNotRewriteSubscriptionSnapshot()
    {
        var p=await service.CreatePlanAsync(Request());var benefit=await service.CreateBenefitAsync(new(){Code="GUEST",Name="Guest"});
        await service.SetPlanBenefitsAsync(p.Plan.Id,new(){Benefits=[new(){BenefitId=benefit.Id,GrantType=MembershipBenefitGrantType.Quantity,Quantity=2}]});
        var member=new Member{FullName="Snapshot",PrimaryPhone="snapshot",MemberCode="SNAP",ClientCode=1,HomeBranchId=a.Id,CreatedByUserId=actor.Id};
        var subscription=new MemberSubscription{Member=member,MembershipPlanId=p.Plan.Id,CityId=a.CityId,PurchasedAtBranchId=a.Id,StartDate=new(2026,1,1),EndDate=new(2026,1,31)};
        var transaction=new MembershipSubscriptionTransaction{MemberSubscription=subscription,MembershipPlanId=p.Plan.Id,
            SaleTransaction=new(){Member=member,BranchId=a.Id,CreatedByUserId=actor.Id,SaleType=SaleType.GymMembership,GrossAmount=1200,NetAmount=1200},
            PurchasedDurationDays=30,PlanPriceSnapshot=1200,NetPriceSnapshot=1200,NewEndDate=subscription.EndDate.Value};
        db.Add(new MemberSubscriptionBenefit{MembershipSubscriptionTransaction=transaction,MembershipBenefitId=benefit.Id,
            BenefitCodeSnapshot="GUEST",BenefitNameSnapshot="Guest",GrantTypeSnapshot=MembershipBenefitGrantType.Quantity,GrantedQuantity=2});await db.SaveChangesAsync();
        await service.SetPlanBenefitsAsync(p.Plan.Id,new());await service.UpdateBenefitAsync(benefit.Id,new(){Code="VISIT",Name="Visitor"});
        db.ChangeTracker.Clear();var grant=await db.MemberSubscriptionBenefits.SingleAsync();Assert.Equal("GUEST",grant.BenefitCodeSnapshot);Assert.Equal("Guest",grant.BenefitNameSnapshot);Assert.Equal(2,grant.GrantedQuantity);
        Assert.Single(await db.MembershipSubscriptionTransactions.ToListAsync());
    }

}
