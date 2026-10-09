using System.Net;
using System.Net.Http.Json;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace CampionsSystem.DAL.Tests;
public sealed partial class AuthenticationApiTests
{
    private async Task<(Guid Subscription,Guid Member,Guid Source,Guid Target,Guid Employee,Guid Plan)> SubscriptionHttpFixture(bool otherBranch=false)
    {
        var f=await BranchFixture();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var branch=otherBranch?f.Target:f.Source;var city=await db.GymBranches.Where(x=>x.Id==branch).Select(x=>x.CityId).SingleAsync();
        var member=new Member{FullName="HTTP entitlement",PrimaryPhone="http-sub",MemberCode="HTTP-SUB",HomeBranchId=f.Source,CreatedByUserId=userId};
        var plan=new MembershipPlan{Name="HTTP paid",BranchId=branch,CityId=city,DurationDays=30,Price=1000,AllowFreeze=true,IncludedFreezeDays=14};
        var sub=new MemberSubscription{Member=member,MembershipPlan=plan,CityId=city,PurchasedAtBranchId=branch,StartDate=BranchToday,EndDate=BranchToday.AddDays(29)};
        var grant=new MembershipSubscriptionTransaction{MemberSubscription=sub,MembershipPlan=plan,PlanNameSnapshot=plan.Name,PlanBranchIdSnapshot=branch,CityIdSnapshot=city,
            PlanCategorySnapshot=PlanCategory.Normal,AccessModeSnapshot=AccessMode.Unlimited,FreezeDaysSnapshot=14,PurchasedDurationDays=30,
            StartDateSnapshot=BranchToday,NewEndDate=sub.EndDate,PlanPriceSnapshot=1000,NetPriceSnapshot=1000,CreatedByUserId=userId,OperationId=Guid.NewGuid()};
        db.Add(grant);await db.SaveChangesAsync();return(sub.Id,member.Id,f.Source,f.Target,f.Employee,plan.Id);
    }
    [Fact] public async Task SubscriptionHttpReadsRequirePermissionAndUseOriginScope()
    {
        var f=await SubscriptionHttpFixture(true);var route=$"/api/members/{f.Member}/subscriptions";
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync(route)).StatusCode);await Authenticate();
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync(route)).StatusCode);
        await GrantPermission("SubscriptionReader",SubscriptionPermissions.View);
        Assert.Empty((await client.GetFromJsonAsync<SubscriptionHistory>(route))!.Items);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync($"/api/subscriptions/{f.Subscription}")).StatusCode);
        Assert.True((await client.GetFromJsonAsync<MemberEntitlement>($"{route}/current"))!.HasAccess); // access city, not administration
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync($"{route}/current?branchId={f.Target}")).StatusCode);
        await GrantPermission("SubscriptionReader","Branches.AccessAll");
        Assert.Single((await client.GetFromJsonAsync<SubscriptionHistory>(route))!.Items);
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync($"/api/subscriptions/{f.Subscription}")).StatusCode);
    }
    [Fact] public async Task FreezeHttpValidatesVersionIdempotencyAndMassAssignment()
    {
        var f=await SubscriptionHttpFixture();await GrantPermission("FreezeOperator",SubscriptionPermissions.Freeze);await Authenticate();
        var r=new FreezeRequest(Guid.NewGuid(),0,BranchToday,2);var route=$"/api/subscriptions/{f.Subscription}/freeze";
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync(route,r)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync(route,r)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await client.PostAsJsonAsync(route,new FreezeRequest(Guid.NewGuid(),0,BranchToday.AddDays(3),1))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync(route,new{operationId=Guid.NewGuid(),expectedVersion=1,startDate=BranchToday,days=2,saleTransactionId=Guid.NewGuid()})).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Assert.Single(await db.SubscriptionFreezes.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());
    }
    [Fact] public async Task ExtensionRequiresDistinctManagementCapabilityAndReason()
    {
        var f=await SubscriptionHttpFixture();await GrantPermission("FreezeOnly",SubscriptionPermissions.Freeze);await Authenticate();
        var route=$"/api/subscriptions/{f.Subscription}/custom-extension";
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync(route,new CustomExtensionRequest(Guid.NewGuid(),0,3,"Injury"))).StatusCode);
        await GrantPermission("MedicalManager",SubscriptionPermissions.Extend);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync(route,new CustomExtensionRequest(Guid.NewGuid(),0,3,""))).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync(route,new CustomExtensionRequest(Guid.NewGuid(),0,3,"Injury"))).StatusCode);
    }
    [Fact] public async Task HolidayHttpRequiresGlobalCapabilityEvenInOwnBranch()
    {
        var f=await SubscriptionHttpFixture();await GrantPermission("BranchManager",SubscriptionPermissions.Extend);await Authenticate();
        var r=new OfficialHolidayRequest(f.Source,BranchToday,BranchToday,true,"Eid");
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/subscriptions/official-holidays",r)).StatusCode);
        await GrantPermission("GlobalManagement","Branches.AccessAll");
        var response=await client.PostAsJsonAsync("/api/subscriptions/official-holidays",r);Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var result=(await response.Content.ReadFromJsonAsync<HolidayApplicationResult>())!;Assert.Equal(1,result.ExtendedDays);
        var retry=await client.PostAsync($"/api/subscriptions/official-holidays/{result.ClosureId}/apply",null);
        Assert.Equal(0,(await retry.Content.ReadFromJsonAsync<HolidayApplicationResult>())!.ExtendedDays);
    }
    [Fact] public async Task EmployeeGrantRequiresBothCapabilitiesAndRejectsPaidPlan()
    {
        var f=await SubscriptionHttpFixture();await GrantPermission("EmployeeOperator",SubscriptionPermissions.Manage);await Authenticate();
        var route=$"/api/employees/{f.Employee}/membership-entitlement";var r=new EmployeeEntitlementRequest(f.Member,f.Plan,Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync(route,r)).StatusCode);
        await GrantPermission("EmployeeOperator","Employees.Manage");
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync(route,r)).StatusCode);
    }
    [Fact] public async Task PaidHttpActivationAndRenewalAreAbsentAndLegacyFreelanceIsGone()
    {
        await GrantPermission("LegacyBuyer","FreelancePlans.Manage");await Authenticate();
        Assert.Equal(HttpStatusCode.NotFound,(await client.PostAsJsonAsync("/api/subscriptions",new{})).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await client.PostAsJsonAsync($"/api/subscriptions/{Guid.NewGuid()}/renew",new{})).StatusCode);
        Assert.Equal(HttpStatusCode.Gone,(await client.PostAsJsonAsync("/api/freelance-plans/purchases",new{memberId=Guid.NewGuid(),planId=Guid.NewGuid(),businessDayId=Guid.NewGuid(),netPrice=1})).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.MemberSalesAssignments.ToListAsync());
    }
}
