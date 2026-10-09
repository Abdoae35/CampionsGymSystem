using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SubscriptionLifecycleTests
{
    private Task<SubscriptionGrantResult> Grant(Guid? operation=null) => service.CreateCustomerGrantAsync(new(member.Id,plan.Id,b.Id,operation??Guid.NewGuid()));
    [Fact] public async Task PlanAndBenefitEditsCannotChangeGrantedSnapshot()
    {
        var q=new MembershipBenefit{Code="GUEST",Name="Guest"};var acc=new MembershipBenefit{Code="SPA",Name="Spa"};
        var config=new MembershipPlanBenefit{MembershipPlanId=plan.Id,MembershipBenefit=q,GrantType=MembershipBenefitGrantType.Quantity,Quantity=2};
        db.AddRange(config,new MembershipPlanBenefit{MembershipPlanId=plan.Id,MembershipBenefit=acc,GrantType=MembershipBenefitGrantType.Access});await db.SaveChangesAsync();
        var result=await Grant();plan.Name="Edited";plan.Price=1200;plan.DurationDays=25;plan.IncludedFreezeDays=1;
        var cairo=new City{Name="Cairo"};db.Add(cairo);await db.SaveChangesAsync();plan.CityId=cairo.Id;
        config.Quantity=1;q.Name="Renamed";db.Remove(config);await db.SaveChangesAsync();
        var detail=await service.GetDetailsAsync(result.SubscriptionId);var grant=Assert.Single(detail.Grants);
        Assert.Equal("Original",grant.PlanName);Assert.Equal(1000,grant.PlanPrice);Assert.Equal(30,grant.DurationDays);Assert.Equal(14,grant.FreezeDays);
        Assert.Equal(a.CityId,detail.Subscription.AccessCityId);Assert.Equal(a.CityId,grant.AccessCityId);Assert.Equal(b.Id,grant.PlanBranchId);
        Assert.Equal(2,detail.Benefits.Single(x=>x.Code=="GUEST").GrantedQuantity);Assert.Equal("Guest",detail.Benefits.Single(x=>x.Code=="GUEST").Name);
        Assert.Null(detail.Benefits.Single(x=>x.Code=="SPA").GrantedQuantity);Assert.Null(detail.Subscription.RemainingSessions);Assert.Null(detail.Subscription.TotalSessions);
        Assert.Equal(today.AddDays(29),detail.Subscription.EndDate);Assert.Equal(14,detail.RemainingFreezeDays);
        Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.CheckIns.ToListAsync());
        Assert.Equal(a.Id,(await db.Members.FindAsync(member.Id))!.HomeBranchId);
    }
    [Fact] public async Task InitialSessionQuantityAndMidnightBusinessDateAreSnapshotted()
    {
        plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;await db.SaveChangesAsync();
        clock.Now=new(2026,10,10,22,30,0,TimeSpan.Zero);
        var day=new BusinessDay{BranchId=b.Id,BusinessDate=today,OpenedAt=clock.Now.AddHours(-15),Status=BusinessDayStatus.Open};db.Add(day);await db.SaveChangesAsync();
        var result=await Grant();var d=await service.GetDetailsAsync(result.SubscriptionId);
        Assert.Equal(today,d.Subscription.StartDate);Assert.Equal(today.AddDays(29),d.Subscription.EndDate);
        Assert.Equal(12,d.Subscription.TotalSessions);Assert.Equal(12,d.Subscription.RemainingSessions);Assert.Equal(day.Id,Assert.Single(d.Grants).BusinessDayId);
    }
    [Fact] public async Task GrantRetryDoesNotDuplicateEntitlementBenefitsOrAudit()
    {
        var op=Guid.NewGuid();var first=await Grant(op);var second=await Grant(op);Assert.Equal(first,second);
        Assert.Single(await db.MemberSubscriptions.ToListAsync());Assert.Single(await db.MembershipSubscriptionTransactions.ToListAsync());
        Assert.Single(await db.AuditLogs.ToListAsync());
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Grant());
    }
    [Fact] public async Task InternalCustomerGrantRequiresGlobalManagementAndRejectsEmployeePlan()
    {
        user.Context=user.Context! with { Permissions=[SubscriptionPermissions.Manage],CurrentBranch=new(b.Id,b.Name) };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Grant());
        user.Context=user.Context! with { Permissions=[..SubscriptionPermissions.Codes,"Branches.AccessAll"] };
        plan.PlanCategory=PlanCategory.Employee;plan.Price=0;plan.DurationDays=0;await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Grant());Assert.Empty(await db.MemberSubscriptions.ToListAsync());
    }
    [Fact] public async Task InactiveConfiguredBenefitRejectsGrantAtomically()
    {
        db.Add(new MembershipPlanBenefit{MembershipPlanId=plan.Id,MembershipBenefit=new(){Code="OLD",Name="Old",IsActive=false},GrantType=MembershipBenefitGrantType.Access});await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Grant());Assert.Empty(await db.MemberSubscriptions.ToListAsync());Assert.Empty(await db.MembershipSubscriptionTransactions.ToListAsync());
    }
}
