using System.Net;
using System.Net.Http.Json;
using CampionsSystem.BLL.Plans;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace CampionsSystem.DAL.Tests;
public sealed partial class AuthenticationApiTests
{
    [Theory][InlineData("/api/membership-plans")][InlineData("/api/membership-plan-categories")][InlineData("/api/membership-benefits")]
    public async Task PlanCatalogEndpointsRequireAuthenticationAndPermission(string route)
    {
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync(route)).StatusCode);await Authenticate();
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync(route)).StatusCode);
    }
    [Fact] public async Task PlanHttpAdministrationUsesBranchNotCityAndGlobalCatalogScope()
    {
        var f=await BranchFixture();Guid city;await using(var scope=factory.Services.CreateAsyncScope())city=await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().GymBranches.Where(b=>b.Id==f.Source).Select(b=>b.CityId).SingleAsync();
        await GrantPermission("PlanOperator","MembershipPlans.View");await GrantPermission("PlanOperator","MembershipPlans.Manage");await Authenticate();
        var r=new CreateMembershipPlanRequest{Name="Gold",BranchId=f.Source,CityId=city,PlanType=PlanCategory.Normal,Mode=AccessMode.Unlimited,DurationDays=30,Price=1200};
        var response=await client.PostAsJsonAsync("/api/membership-plans",r);Assert.Equal(HttpStatusCode.OK,response.StatusCode);var plan=(await response.Content.ReadFromJsonAsync<MembershipPlanDetails>())!;
        r.BranchId=f.Target;Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/membership-plans",r)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/membership-plan-categories",new SaveCategoryRequest{Name="Kids"})).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/membership-benefits",new SaveBenefitRequest{Code="SAUNA",Name="Sauna"})).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed,(await client.DeleteAsync($"/api/membership-plans/{plan.Plan.Id}")).StatusCode);
        await GrantPermission("GlobalPlans","Branches.AccessAll");
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/membership-plans",r)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/membership-plan-categories",new SaveCategoryRequest{Name="Kids"})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/membership-benefits",new SaveBenefitRequest{Code="SAUNA",Name="Sauna"})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PatchAsJsonAsync($"/api/membership-plans/{plan.Plan.Id}/status",new ChangePlanStatusRequest{IsActive=false})).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync($"/api/membership-plans/{plan.Plan.Id}/eligible-members")).StatusCode);
    }
    [Fact] public async Task ReadOnlyPlanUserCannotSeeInternalEmployeeProducts()
    {
        var f=await BranchFixture();Guid employeePlan;await using(var scope=factory.Services.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var city=await db.GymBranches.Where(b=>b.Id==f.Source).Select(b=>b.CityId).SingleAsync();
            var plan=new MembershipPlan{Name="Employee internal",BranchId=f.Source,CityId=city,PlanCategory=PlanCategory.Employee,AccessMode=AccessMode.Unlimited,Price=0,DurationDays=0};db.Add(plan);await db.SaveChangesAsync();employeePlan=plan.Id;
        }
        await GrantPermission("PlanReader","MembershipPlans.View");await Authenticate();
        Assert.Empty((await client.GetFromJsonAsync<MembershipPlanListResult>("/api/membership-plans"))!.Items);
        Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync($"/api/membership-plans/{employeePlan}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/membership-plans?includeInternal=true")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PutAsJsonAsync($"/api/membership-plans/{employeePlan}/benefits",new SetPlanBenefitsRequest())).StatusCode);
    }
    [Fact] public async Task FreelanceHttpConfigurationSupportsCrossRegistrationMembersWithoutOwnershipChanges()
    {
        var f=await BranchFixture();Guid city,coachId,memberId,benefitId;
        await using(var scope=factory.Services.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();city=await db.GymBranches.Where(b=>b.Id==f.Source).Select(b=>b.CityId).SingleAsync();
            var coach=new Employee{FullName="HTTP Freelance",Phone="http-freelance",EmploymentType=EmploymentType.Freelance,HireDate=BranchToday.AddDays(-30)};
            var member=new Member{FullName="Other branch member",PrimaryPhone="http-member",MemberCode="HTTP-ELIGIBLE",ClientCode=1,HomeBranchId=f.Target,CreatedByUserId=userId};
            var benefit=new MembershipBenefit{Code="LOCKER",Name="Locker"};db.AddRange(coach,member,benefit);await db.SaveChangesAsync();coachId=coach.Id;memberId=member.Id;benefitId=benefit.Id;
        }
        foreach(var permission in new[]{"MembershipPlans.View","MembershipPlans.Manage","FreelancePlans.ManageEligibleMembers","FreelancePlans.Manage"})await GrantPermission("FreelanceOperator",permission);
        await Authenticate();var r=new CreateMembershipPlanRequest{Name="Coach offer",BranchId=f.Source,CityId=city,PlanType=PlanCategory.Freelance,FreelanceCoachEmployeeId=coachId,Mode=AccessMode.Unlimited,DurationDays=30,Price=300};
        var response=await client.PostAsJsonAsync("/api/membership-plans",r);Assert.Equal(HttpStatusCode.OK,response.StatusCode);var id=(await response.Content.ReadFromJsonAsync<MembershipPlanDetails>())!.Plan.Id;
        Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync($"/api/membership-plans/{id}",r)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync($"/api/membership-plans/{id}/benefits",new SetPlanBenefitsRequest{Benefits=[new(){BenefitId=benefitId,GrantType=MembershipBenefitGrantType.Access}]})).StatusCode);
        Assert.Single((await client.GetFromJsonAsync<MembershipPlanDetails>($"/api/membership-plans/{id}"))!.Benefits);
        Assert.Single((await client.GetFromJsonAsync<List<PlanBenefitDetails>>($"/api/membership-plans/{id}/benefits"))!);
        Assert.Single((await client.GetFromJsonAsync<List<EligibleMemberDetails>>($"/api/membership-plans/{id}/eligible-members/candidates?memberCode=HTTP-ELIGIBLE"))!);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync($"/api/membership-plans/{id}/eligible-members",new EligibleMemberRequest{MemberId=memberId})).StatusCode);
        Assert.Single((await client.GetFromJsonAsync<EligibleMemberListResult>($"/api/membership-plans/{id}/eligible-members"))!.Items);
        Assert.Equal(HttpStatusCode.OK,(await client.DeleteAsync($"/api/membership-plans/{id}/eligible-members/{memberId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/freelance-plans",new CampionsSystem.BLL.Operations.SaveFreelancePlanRequest(city,coachId,"Bypass",300,30,AccessMode.Unlimited,null,BranchId:f.Target))).StatusCode);
        await using var verify=factory.Services.CreateAsyncScope();var context=verify.ServiceProvider.GetRequiredService<ApplicationDbContext>();Assert.Empty(await context.MemberSalesAssignments.ToListAsync());Assert.Empty(await context.MemberSubscriptions.ToListAsync());
        await context.MembershipPlans.Where(p=>p.Id==id).ExecuteUpdateAsync(s=>s.SetProperty(p=>p.BranchId,f.Target));
        Assert.Equal(HttpStatusCode.NotFound,(await client.PutAsJsonAsync($"/api/freelance-plans/{id}/eligible-members/{memberId}",new{})).StatusCode);
    }

}
