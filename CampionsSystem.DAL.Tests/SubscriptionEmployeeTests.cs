using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Employees;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SubscriptionLifecycleTests
{
    private async Task<Employee> Staff()
    {
        user.Context=user.Context! with { Permissions=[..SubscriptionPermissions.Codes,"Branches.AccessAll","Employees.Manage","Employees.Transfer"] };
        plan.PlanCategory=PlanCategory.Employee;plan.Price=0;plan.DurationDays=0;
        var e=new Employee{FullName="Staff",Phone="employee-sub",HireDate=today.AddYears(-1)};
        db.Add(new EmployeeBranchAssignment{Employee=e,BranchId=b.Id,StartDate=today.AddMonths(-1),IsPrimary=true,AssignedByUserId=actor.Id});await db.SaveChangesAsync();return e;
    }
    private EmployeeManagementService EmployeeService()=>new(db,user,access,new(db),new(db,user,access),service,OwnerLifecycleTestServices.Protection(db,user,clock));
    [Fact] public async Task EmployeeGrantIsLinkedFreeUnlimitedAndProtected()
    {
        var e=await Staff();var op=Guid.NewGuid();var first=await service.StartEmployeeEntitlementAsync(e.Id,new(member.Id,plan.Id,op));
        Assert.Equal(first,await service.StartEmployeeEntitlementAsync(e.Id,new(member.Id,plan.Id,op)));
        var d=await service.GetDetailsAsync(first.SubscriptionId);Assert.Null(d.Subscription.EndDate);Assert.Null(d.Subscription.RemainingSessions);
        Assert.Equal(e.Id,d.Subscription.EmployeeId);Assert.Equal(member.Id,(await db.Employees.FindAsync(e.Id))!.MemberId);
        Assert.Equal(0,Assert.Single(d.Grants).PlanPrice);Assert.Equal("Employee",d.Grants[0].PlanType);
        Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());
        user.Context=user.Context! with { Permissions=[SubscriptionPermissions.Manage,"Branches.AccessAll"] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.StartEmployeeEntitlementAsync(e.Id,new(member.Id,plan.Id,Guid.NewGuid())));
    }
    [Fact] public async Task BranchTransferKeepsEmployeeEntitlementAndEmploymentClosureEndsIt()
    {
        var e=await Staff();var result=await service.StartEmployeeEntitlementAsync(e.Id,new(member.Id,plan.Id,Guid.NewGuid()));
        await new EmployeeBranchAssignmentService(db,user,access).TransferAsync(e.Id,new(a.Id,today));
        Assert.True((await service.GetCurrentEntitlementAsync(member.Id,a.Id)).HasAccess);Assert.Null((await db.MemberSubscriptions.FindAsync(result.SubscriptionId))!.EndDate);
        await EmployeeService().ChangeStatusAsync(e.Id,new(){Status=EmploymentStatus.Suspended,Reason="Suspended"});
        Assert.False((await service.GetCurrentEntitlementAsync(member.Id,a.Id)).HasAccess);
        var closed=await service.GetDetailsAsync(result.SubscriptionId);Assert.Equal(today,closed.Subscription.EndDate);Assert.Equal("Expired",closed.Subscription.Status);
        Assert.Contains(await db.AuditLogs.ToListAsync(),x=>x.Action=="Subscription.EmployeeEntitlementEnded");
    }
    [Fact] public async Task RehireCreatesNewHistoricalPeriodRatherThanReopeningOldOne()
    {
        var e=await Staff();var old=await service.StartEmployeeEntitlementAsync(e.Id,new(member.Id,plan.Id,Guid.NewGuid()));
        await EmployeeService().ChangeStatusAsync(e.Id,new(){Status=EmploymentStatus.Resigned});clock.Now=clock.Now.AddDays(10);
        await EmployeeService().ChangeStatusAsync(e.Id,new(){Status=EmploymentStatus.Active});
        var next=await service.StartEmployeeEntitlementAsync(e.Id,new(member.Id,plan.Id,Guid.NewGuid()));Assert.NotEqual(old.SubscriptionId,next.SubscriptionId);
        var history=await service.GetHistoryAsync(member.Id,new());Assert.Equal(2,history.TotalCount);
        Assert.Equal(today,(await service.GetDetailsAsync(old.SubscriptionId)).Subscription.EndDate);
        Assert.Null((await service.GetDetailsAsync(next.SubscriptionId)).Subscription.EndDate);
        Assert.Equal(2,await db.MembershipSubscriptionTransactions.CountAsync());
    }
    [Fact] public async Task InactiveEmployeeOrCustomerPlanCannotReceiveEmployeeEntitlement()
    {
        var e=await Staff();e.EmploymentStatus=EmploymentStatus.Suspended;await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.StartEmployeeEntitlementAsync(e.Id,new(member.Id,plan.Id,Guid.NewGuid())));
        e=await db.Employees.SingleAsync();e.EmploymentStatus=EmploymentStatus.Active;
        var p=await db.MembershipPlans.SingleAsync();p.PlanCategory=PlanCategory.Normal;p.Price=1000;p.DurationDays=30;await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.StartEmployeeEntitlementAsync(e.Id,new(member.Id,plan.Id,Guid.NewGuid())));
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());
    }
}
