using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CheckInTests
{
    [Theory][InlineData(true)][InlineData(false)]
    public async Task FreelanceEntrancePreservesSalesOrHouseAssignmentAndCreatesNoFinance(bool sales)
    {
        var coach=new Employee{FullName="Coach",Phone="ci-coach",EmploymentType=EmploymentType.Freelance,HireDate=today.AddYears(-1)};
        var owner=new Employee{FullName="Owner",Phone="ci-owner",HireDate=today.AddYears(-1)};db.AddRange(coach,owner);await db.SaveChangesAsync();
        plan.PlanCategory=PlanCategory.Freelance;plan.FreelanceCoachEmployeeId=coach.Id;
        db.Add(new MembershipPlanEligibleMember{MembershipPlanId=plan.Id,MemberId=member.Id,CreatedByUserId=actor.Id});
        var assignment=new MemberSalesAssignment{MemberId=member.Id,AssignmentType=sales?AssignmentType.Sales:AssignmentType.House,SalesEmployeeId=sales?owner.Id:null,AssignedByUserId=actor.Id,AssignedAt=clock.Now};db.Add(assignment);await db.SaveChangesAsync();
        await subscriptions.CreateCustomerGrantAsync(new(member.Id,plan.Id,a.Id,Guid.NewGuid()));Assert.True((await Entry()).Decision.Allowed);
        Assert.Null(assignment.EndedAt);Assert.Equal(sales?owner.Id:null,assignment.SalesEmployeeId);Assert.Single(await db.MemberSalesAssignments.ToListAsync());Assert.Equal(a.Id,member.HomeBranchId);
        Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.FinancialTransactions.ToListAsync());Assert.Empty(await db.EmployeeCommissionSummaries.ToListAsync());
    }
    [Fact] public async Task EmployeeEntranceRemainsUnlimitedThroughRealBranchTransferAndStopsOnInactiveEmployment()
    {
        user.Context=user.Context! with{Permissions=[..user.Context!.Permissions,"Employees.Manage","Employees.Transfer"]};
        plan.PlanCategory=PlanCategory.Employee;plan.Price=0;plan.DurationDays=0;
        var employee=new Employee{FullName="Employee",Phone="ci-employee",HireDate=today.AddYears(-1)};
        db.Add(new EmployeeBranchAssignment{Employee=employee,BranchId=a.Id,StartDate=today.AddMonths(-1),IsPrimary=true,AssignedByUserId=actor.Id});await db.SaveChangesAsync();
        var entitlement=await subscriptions.StartEmployeeEntitlementAsync(employee.Id,new(member.Id,plan.Id,Guid.NewGuid()));
        Assert.True((await Entry()).Decision.Allowed);await new EmployeeBranchAssignmentService(db,user,access).TransferAsync(employee.Id,new(b.Id,today));Assert.True((await Entry()).Decision.Allowed);
        var subscription=await db.MemberSubscriptions.SingleAsync(x=>x.Id==entitlement.SubscriptionId);Assert.Null(subscription.EndDate);Assert.Null(subscription.RemainingSessions);
        employee.EmploymentStatus=EmploymentStatus.Suspended;await db.SaveChangesAsync();Assert.Equal("EmployeeEntitlementInactive",(await Entry()).Decision.ReasonCode);Assert.Equal(2,await db.CheckIns.CountAsync());
    }
}
