using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Plans;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class PlanManagementTests
{
    private MembershipPlan Template(string name="Gold",GymBranch? branch=null)=>new(){Name=name,Branch=branch??a,CityId=a.CityId,DurationDays=30,Price=1200};
    [Fact] public async Task QueriesRespectBranchNotCityAndHideInternalProducts()
    {
        var own=Template();var other=Template("B Gold",b);var legacy=Template("Unmapped");legacy.Branch=null;
        var employee=Template("Employee");employee.PlanCategory=PlanCategory.Employee;employee.Price=0;employee.DurationDays=0;
        db.AddRange(own,other,legacy,employee);await db.SaveChangesAsync();
        Branch("MembershipPlans.View");Assert.Equal(own.Id,Assert.Single((await service.GetPlansAsync(new())).Items).Id);
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.GetPlanByIdAsync(other.Id));
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.GetPlanByIdAsync(legacy.Id));
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.GetPlanByIdAsync(employee.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.GetPlansAsync(new(){IncludeInternal=true}));
        Global();Assert.Equal(4,(await service.GetPlansAsync(new(){IncludeInternal=true})).TotalCount);
        Assert.False((await service.GetPlanByIdAsync(employee.Id)).Plan.CustomerPurchasable);
        Branch("MembershipPlans.View","MembershipPlans.Manage");Assert.Equal(2,(await service.GetPlansAsync(new(){IncludeInternal=true})).TotalCount);
        a.IsActive=false;await db.SaveChangesAsync();Assert.Empty((await service.GetPlansAsync(new(){IncludeInternal=true})).Items);
    }
    [Fact] public async Task QueriesFilterSearchPageAndReturnLightweightBenefits()
    {
        var category=new MembershipPlanCategory{Name="Kids"};var coach=new Employee{FullName="Coach",Phone="coach",EmploymentType=EmploymentType.Freelance,HireDate=new(2020,1,1)};
        var plan=Template();plan.PlanCategory=PlanCategory.Freelance;plan.FreelanceCoachEmployee=coach;plan.MembershipPlanCategory=category;
        plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=12;
        var benefit=new MembershipBenefit{Code="GUEST",Name="Guest"};db.AddRange(plan,Template("Silver"),new MembershipPlanBenefit{MembershipPlan=plan,MembershipBenefit=benefit,Quantity=2});await db.SaveChangesAsync();
        var list=await service.GetPlansAsync(new(){Search="gold",BranchId=a.Id,CityId=a.CityId,PlanType=PlanCategory.Freelance,AudienceCategoryId=category.Id,Mode=AccessMode.SessionBased,FreelanceCoachEmployeeId=coach.Id,IsActive=true});
        Assert.Equal(plan.Id,Assert.Single(list.Items).Id);Assert.Single((await service.GetPlansAsync(new(){PageSize=1,Page=2})).Items);
        var details=await service.GetPlanByIdAsync(plan.Id);Assert.Single(details.Benefits);Assert.Equal(coach.Id,details.Plan.FreelanceCoach!.Id);
        Assert.Equal(category.Id,details.Plan.AudienceCategory!.Id);
    }
}
