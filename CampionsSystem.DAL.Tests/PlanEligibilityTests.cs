using CampionsSystem.BLL.Plans;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class PlanManagementTests
{
    private async Task<Guid> FreelancePlan()
    {
        var coach=new Employee{FullName="Coach",Phone="coach",EmploymentType=EmploymentType.Freelance,HireDate=new(2020,1,1)};db.Add(coach);await db.SaveChangesAsync();
        var r=Request();r.PlanType=PlanCategory.Freelance;r.FreelanceCoachEmployeeId=coach.Id;return (await service.CreatePlanAsync(r)).Plan.Id;
    }
    [Fact] public async Task CrossRegistrationEligibilityPreservesSalesOwnershipAndHouseState()
    {
        var plan=await FreelancePlan();var sales=new Employee{FullName="Sales",Phone="sales",HireDate=new(2020,1,1)};
        var owned=new Member{FullName="Owned B",PrimaryPhone="01212345678",PrimaryPhoneNormalized="+201212345678",MemberCode="OWNED",ClientCode=1,HomeBranchId=b.Id,CreatedByUserId=actor.Id};
        var house=new Member{FullName="House B",PrimaryPhone="01012345678",PrimaryPhoneNormalized="+201012345678",MemberCode="HOUSE",ClientCode=2,HomeBranchId=b.Id,CreatedByUserId=actor.Id};
        db.AddRange(new MemberSalesAssignment{Member=owned,SalesEmployee=sales,AssignedAt=DateTimeOffset.UtcNow,AssignedByUserId=actor.Id},house);await db.SaveChangesAsync();
        Branch("FreelancePlans.ManageEligibleMembers");Assert.True(await service.AddEligibleMemberAsync(plan,new(){MemberId=owned.Id}));Assert.False(await service.AddEligibleMemberAsync(plan,new(){MemberId=owned.Id}));
        await service.AddEligibleMemberAsync(plan,new(){MemberId=house.Id});Assert.Equal(2,(await service.GetEligibleMembersAsync(plan,new())).TotalCount);
        Assert.Equal(owned.Id,Assert.Single((await service.GetEligibleMembersAsync(plan,new(){Search="owned",PageSize=1})).Items).MemberId);
        Assert.Equal(owned.Id,Assert.Single(await service.FindEligibleMemberCandidatesAsync(plan,new(){Phone="012 1234 5678"})).MemberId);
        Assert.Single(await db.MemberSalesAssignments.ToListAsync());Assert.Null((await db.MemberSalesAssignments.SingleAsync()).EndedAt);
        Assert.Equal(sales.Id,(await db.MemberSalesAssignments.SingleAsync()).SalesEmployeeId);
        await service.RemoveEligibleMemberAsync(plan,owned.Id);Assert.Single((await service.GetEligibleMembersAsync(plan,new())).Items);
        Assert.Single(await db.MemberSalesAssignments.ToListAsync());Assert.Empty(await db.MemberSubscriptions.ToListAsync());
        Assert.Contains(await db.AuditLogs.ToListAsync(),x=>x.Action=="FreelancePlan.MemberEligibilityRemoved"&&x.BranchId==a.Id);
    }
    [Fact] public async Task EligibilityRequiresPlanScopePermissionAndValidFreelanceConfiguration()
    {
        var plan=await FreelancePlan();var member=new Member{FullName="M",PrimaryPhone="p",MemberCode="M",ClientCode=1,HomeBranchId=a.Id,CreatedByUserId=actor.Id};db.Add(member);await db.SaveChangesAsync();
        Branch("MembershipPlans.Manage");await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.AddEligibleMemberAsync(plan,new(){MemberId=member.Id}));
        Branch("FreelancePlans.ManageEligibleMembers");await Assert.ThrowsAsync<BusinessRuleException>(()=>service.FindEligibleMemberCandidatesAsync(plan,new()));
        await db.MembershipPlans.Where(x=>x.Id==plan).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.BranchId,b.Id));
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.AddEligibleMemberAsync(plan,new(){MemberId=member.Id}));Global();
        await db.MembershipPlans.Where(x=>x.Id==plan).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.IsActive,false));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.AddEligibleMemberAsync(plan,new(){MemberId=member.Id}));
        Assert.False(await service.RemoveEligibleMemberAsync(plan,member.Id));
    }
}
