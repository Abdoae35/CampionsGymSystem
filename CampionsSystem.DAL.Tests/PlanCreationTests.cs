using CampionsSystem.BLL.Plans;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class PlanManagementTests
{
    private CreateMembershipPlanRequest Request()=>new(){Name="Gold",BranchId=a.Id,CityId=a.CityId,PlanType=PlanCategory.Normal,Mode=AccessMode.Unlimited,DurationDays=30,Price=1200};
    [Fact] public async Task NormalCreationPersistsIndependentBranchCityAndAudienceWithoutPurchase()
    {
        var category=await service.CreateCategoryAsync(new(){Name="Male"});var r=Request();r.AudienceCategoryId=category.Id;r.AllowFreeze=true;r.FreezeDays=7;
        var differentCity=new City{Name="Access elsewhere"};db.Add(differentCity);await db.SaveChangesAsync();r.CityId=differentCity.Id;
        Branch("MembershipPlans.Manage","MembershipPlans.View");var p=await service.CreatePlanAsync(r);
        Assert.Equal(a.Id,p.Plan.Branch!.Id);Assert.Equal(differentCity.Id,p.Plan.City.Id);Assert.Equal(category.Id,p.Plan.AudienceCategory!.Id);
        Assert.Equal("Normal",p.Plan.PlanType);Assert.Null(p.Plan.Sessions);Assert.Equal(7,p.Plan.FreezeDays);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.SaleTransactions.ToListAsync());
        Assert.Contains(await db.AuditLogs.ToListAsync(),l=>l.Action=="MembershipPlan.Created");
        r.BranchId=b.Id;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.CreatePlanAsync(r));
        Global();Assert.Equal(b.Id,(await service.CreatePlanAsync(r)).Plan.Branch!.Id);
    }
    [Fact] public async Task FreelanceAndEmployeeTypesEnforceInternalRules()
    {
        var coach=new Employee{FullName="Freelance",Phone="coach",EmploymentType=EmploymentType.Freelance,HireDate=new(2020,1,1)};db.Add(coach);await db.SaveChangesAsync();
        var r=Request();r.PlanType=PlanCategory.Freelance;r.FreelanceCoachEmployeeId=coach.Id;r.Mode=AccessMode.SessionBased;r.Sessions=12;r.Price=300;
        var freelance=await service.CreatePlanAsync(r);Assert.Equal(300,freelance.Plan.Price);Assert.Equal(coach.Id,freelance.Plan.FreelanceCoach!.Id);
        r=Request();r.PlanType=PlanCategory.Employee;r.Price=0;r.DurationDays=0;var employee=await service.CreatePlanAsync(r);
        Assert.False(employee.Plan.CustomerPurchasable);Assert.Equal("Unlimited",employee.Plan.Mode);Assert.Equal(0,employee.Plan.Price);
        coach.EmploymentStatus=EmploymentStatus.Resigned;await db.SaveChangesAsync();r=Request();r.PlanType=PlanCategory.Freelance;r.FreelanceCoachEmployeeId=coach.Id;
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CreatePlanAsync(r));
    }
    [Theory]
    [InlineData("price")][InlineData("precision")][InlineData("duration")][InlineData("freeze")][InlineData("disabledFreeze")]
    [InlineData("unlimitedSessions")][InlineData("missingSessions")][InlineData("zeroSessions")][InlineData("coach")]
    [InlineData("freelanceCoach")][InlineData("employeePrice")][InlineData("employeeSessions")][InlineData("branch")][InlineData("type")]
    public async Task InvalidPlanConfigurationRollsBack(string kind)
    {
        var r=Request();switch(kind){case "price":r.Price=-1;break;case "precision":r.Price=1.111m;break;case "duration":r.DurationDays=0;break;
        case "freeze":r.FreezeDays=-1;break;case "disabledFreeze":r.FreezeDays=1;break;case "unlimitedSessions":r.Sessions=999999;break;
        case "missingSessions":r.Mode=AccessMode.SessionBased;break;case "zeroSessions":r.Mode=AccessMode.SessionBased;r.Sessions=0;break;
        case "coach":r.FreelanceCoachEmployeeId=Guid.NewGuid();break;case "freelanceCoach":r.PlanType=PlanCategory.Freelance;break;
        case "employeePrice":r.PlanType=PlanCategory.Employee;r.DurationDays=0;break;case "employeeSessions":r.PlanType=PlanCategory.Employee;r.Price=0;r.DurationDays=0;r.Mode=AccessMode.SessionBased;r.Sessions=12;break;
        case "branch":r.BranchId=Guid.Empty;break;case "type":r.PlanType=(PlanCategory)99;break;}
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CreatePlanAsync(r));Assert.Empty(await db.MembershipPlans.ToListAsync());Assert.Empty(await db.AuditLogs.ToListAsync());
    }
    [Fact] public async Task NewPlanCannotSelectInactiveAudienceOrOwningBranch()
    {
        var category=await service.CreateCategoryAsync(new(){Name="Old"});await service.ChangeCategoryStatusAsync(category.Id,new(){IsActive=false});
        var r=Request();r.AudienceCategoryId=category.Id;await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CreatePlanAsync(r));
        await db.GymBranches.Where(x=>x.Id==a.Id).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.IsActive,false));r.AudienceCategoryId=null;await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CreatePlanAsync(r));
    }
}
