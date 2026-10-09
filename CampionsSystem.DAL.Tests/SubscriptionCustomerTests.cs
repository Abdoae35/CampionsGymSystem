using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SubscriptionLifecycleTests
{
    private async Task<Employee> FreelancePlan(bool eligible)
    {
        var coach = new Employee { FullName="Coach",Phone="freelance-sub",EmploymentType=EmploymentType.Freelance,HireDate=today.AddYears(-1) };
        db.Add(coach); await db.SaveChangesAsync();plan.PlanCategory=PlanCategory.Freelance;plan.FreelanceCoachEmployeeId=coach.Id;plan.Price=300;
        if(eligible)db.Add(new MembershipPlanEligibleMember{MembershipPlanId=plan.Id,MemberId=member.Id,CreatedByUserId=actor.Id});
        await db.SaveChangesAsync();return coach;
    }
    [Theory] [InlineData(true)] [InlineData(false)]
    public async Task FreelanceGrantPreservesSalesOrHouseAndCreatesNoFinance(bool sales)
    {
        var coach=await FreelancePlan(true);
        var owner=new Employee{FullName="Sales",Phone="sales-sub",HireDate=today.AddYears(-1)};db.Add(owner);await db.SaveChangesAsync();
        var assignment=new MemberSalesAssignment{MemberId=member.Id,AssignmentType=sales?AssignmentType.Sales:AssignmentType.House,
            SalesEmployeeId=sales?owner.Id:null,AssignedByUserId=actor.Id,AssignedAt=access.Now};db.Add(assignment);await db.SaveChangesAsync();
        var result=await Grant();var detail=await service.GetDetailsAsync(result.SubscriptionId);var grant=Assert.Single(detail.Grants);
        Assert.Equal("Freelance",grant.PlanType);Assert.Equal(coach.Id,grant.FreelanceCoachId);Assert.Equal(300,grant.PlanPrice);
        Assert.Null(assignment.EndedAt);Assert.Equal(sales?owner.Id:null,assignment.SalesEmployeeId);Assert.Single(await db.MemberSalesAssignments.ToListAsync());
        Assert.Equal(a.Id,(await db.Members.FindAsync(member.Id))!.HomeBranchId);
        Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.PaymentParts.ToListAsync());
        Assert.Empty(await db.FinancialTransactions.ToListAsync());Assert.Empty(await db.EmployeeCommissionSummaries.ToListAsync());
    }
    [Fact] public async Task FreelanceGrantRequiresEligibilityAndActiveCoach()
    {
        var coach=await FreelancePlan(false);await Assert.ThrowsAsync<BusinessRuleException>(()=>Grant());
        db.Add(new MembershipPlanEligibleMember{MembershipPlanId=plan.Id,MemberId=member.Id,CreatedByUserId=actor.Id});
        coach=await db.Employees.SingleAsync(x=>x.Id==coach.Id);coach.EmploymentStatus=EmploymentStatus.Suspended;await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Grant());Assert.Empty(await db.MemberSubscriptions.ToListAsync());
    }
    [Fact] public async Task PlanOwnershipRequiresOriginBranchRatherThanMemberHome()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CreateCustomerGrantAsync(new(member.Id,plan.Id,a.Id,Guid.NewGuid())));
        var result=await Grant();Assert.Equal(b.Id,(await service.GetDetailsAsync(result.SubscriptionId)).Subscription.OriginBranchId);
        Assert.Equal(a.Id,(await db.Members.FindAsync(member.Id))!.HomeBranchId);
    }
    [Fact] public async Task LegacyFreelanceServiceCannotBypassGrantBoundary()
    {
        await FreelancePlan(true);var legacy=new SubscriptionPurchaseService(db,access);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>legacy.PurchaseFreelanceAsync(new(member.Id,plan.Id,b.Id)));
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.MemberSalesAssignments.ToListAsync());
    }
}
