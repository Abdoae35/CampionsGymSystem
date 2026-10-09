using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Employees;
using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.DAL.Tests;
public sealed partial class CrmManagementTests
{
    private EmployeeBranchAssignmentService EmployeeBranches() => new(db,current,access);
    private EmployeeManagementService Employees() => new(db,current,access,new(db),EmployeeBranches(),new(db,current,access));
    private void EmploymentManager() => Set(mohamed,"Employees.Transfer","Employees.Manage","Employees.View","Branches.AccessAll");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TransferPreservesOwnershipAndFollowUpsAcrossBranches(bool canDistribute)
    {
        var member=await OwnedMember();
        var second=await members.CreateMemberAsync(new(){FullName="Referral",PrimaryPhone="+201299999999",TargetRegistrationBranchId=b.Id,
            CreationType=MemberCreationType.ManagementCreated,SalesEmployeeId=ahmed.Id});
        await service.OpenMemberFollowUpAsync(member.Id,new(){DueAt=access.Now});
        var lead=await Lead("01288888888");await service.AssignLeadAsync(lead.Id,new(){SalesEmployeeId=ahmed.Id});
        await service.AddLeadInteractionAsync(lead.Id,new(){Notes="Before transfer",NextFollowUpAt=access.Now.AddDays(1)});
        var unrelated=await Lead("01277777777");
        var sale=new SaleTransaction{MemberId=member.Id,BranchId=a.Id,SalesEmployeeId=ahmed.Id,
            SalesAttributionSnapshot=AssignmentType.Sales,CreatedByUserId=mohamed.UserId!.Value,GrossAmount=100,NetAmount=100};
        db.Add(sale);await db.SaveChangesAsync();
        var assignments=await db.MemberSalesAssignments.AsNoTracking().OrderBy(x=>x.Id).Select(x=>new{x.Id,x.SalesEmployeeId,x.AssignedAt,x.EndedAt}).ToListAsync();
        EmploymentManager();await EmployeeBranches().TransferAsync(ahmed.Id,new(b.Id,access.Today));
        Assert.Equal(assignments,await db.MemberSalesAssignments.AsNoTracking().OrderBy(x=>x.Id).Select(x=>new{x.Id,x.SalesEmployeeId,x.AssignedAt,x.EndedAt}).ToListAsync());
        Assert.All(await db.Members.ToListAsync(),m=>Assert.Equal(m.Id==second.Id?b.Id:a.Id,m.HomeBranchId));
        var historical=await db.SaleTransactions.AsNoTracking().SingleAsync();
        Assert.Equal(a.Id,historical.BranchId);Assert.Equal(ahmed.Id,historical.SalesEmployeeId);Assert.Equal(100,historical.NetAmount);
        Set(ahmed,"Leads.View","Leads.Manage","CRM.ViewMemberFollowUps","Members.View");
        if(canDistribute)current.Context=current.Context! with{Permissions=current.Context.Permissions.Append("Leads.Assign").ToArray()};
        current.Context=current.Context! with{CurrentBranch=new(b.Id,b.Name)};
        Assert.Equal(lead.Id,Assert.Single((await service.GetMyLeadsAsync(new())).Items).Id);
        await service.AddLeadInteractionAsync(lead.Id,new(){Notes="After transfer",NextFollowUpAt=access.Now.AddDays(2)});
        Assert.Equal(2,(await service.GetLeadInteractionsAsync(lead.Id)).Count);
        Assert.Equal(2,(await service.GetMyFollowUpsAsync(new())).TotalCount);
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.GetLeadByIdAsync(unrelated.Id));
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>members.GetMemberByIdAsync(member.Id));
        if(canDistribute)await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.AssignLeadAsync(lead.Id,new(){SalesEmployeeId=mohamed.Id}));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>access.RequireAsync("Members.View",a.Id,default));
        Set(mohamed,"Leads.Manage","Leads.Assign","Leads.View");current.Context=current.Context! with{CurrentBranch=new(b.Id,b.Name)};
        var next=(await service.CreateLeadAsync(new(){BranchId=b.Id,PrimaryPhone="+447123456789"})).Lead!;
        await service.AssignLeadAsync(next.Id,new(){SalesEmployeeId=ahmed.Id});
        current.Context=current.Context! with{CurrentBranch=new(a.Id,a.Name)};
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.AssignLeadAsync(unrelated.Id,new(){SalesEmployeeId=ahmed.Id}));
        Set(ahmed,"Leads.View");current.Context=current.Context! with{CurrentBranch=new(b.Id,b.Name)};
        Assert.Equal(2,(await service.GetMyLeadsAsync(new())).TotalCount);
        Assert.Null((await db.LeadAssignments.AsNoTracking().SingleAsync(x=>x.LeadId==lead.Id)).EndedAt);
        Assert.Equal(a.Id,(await db.Leads.AsNoTracking().SingleAsync(x=>x.Id==lead.Id)).BranchId);
    }

    [Theory]
    [InlineData(EmploymentStatus.Resigned)]
    [InlineData(EmploymentStatus.Suspended)]
    public async Task InactiveEmploymentClosesOwnershipToHouseAndManagementWithoutDeletingHistory(EmploymentStatus status)
    {
        var member=await OwnedMember(b.Id);await service.OpenMemberFollowUpAsync(member.Id,new(){DueAt=access.Now});
        var lead=await Lead("01288888888");await service.AssignLeadAsync(lead.Id,new(){SalesEmployeeId=ahmed.Id});
        await service.AddLeadInteractionAsync(lead.Id,new(){Notes="History",NextFollowUpAt=access.Now.AddDays(1)});
        var due=(await db.Leads.SingleAsync()).NextFollowUpAt;
        EmploymentManager();await Employees().ChangeStatusAsync(ahmed.Id,new(){Status=status,Reason="No longer working"});
        Assert.All(await db.MemberSalesAssignments.ToListAsync(),a=>Assert.NotNull(a.EndedAt));
        Assert.All(await db.LeadAssignments.ToListAsync(),a=>Assert.NotNull(a.EndedAt));
        Assert.Equal(due,(await db.Leads.SingleAsync()).NextFollowUpAt);Assert.Single(await db.LeadInteractions.ToListAsync());
        Manager();Assert.True((await members.GetMemberByIdAsync(member.Id)).IsHouse);
        Assert.Equal(lead.Id,Assert.Single((await service.GetLeadsAsync(new(){UnassignedOnly=true})).Items).Id);
        Assert.Single((await service.GetHouseFollowUpsAsync(new())).Items);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.AssignLeadAsync(lead.Id,new(){SalesEmployeeId=ahmed.Id}));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>members.AssignSalesAsync(member.Id,new(){SalesEmployeeId=ahmed.Id}));
        Set(ahmed,"Leads.View","CRM.ViewMemberFollowUps");Assert.Empty((await service.GetMyLeadsAsync(new())).Items);Assert.Empty((await service.GetMyFollowUpsAsync(new())).Items);
        EmploymentManager();await Employees().ChangeStatusAsync(ahmed.Id,new(){Status=EmploymentStatus.Active});
        Assert.False(await db.MemberSalesAssignments.AnyAsync(x=>x.EndedAt==null));Assert.False(await db.LeadAssignments.AnyAsync(x=>x.EndedAt==null));
        Set(mohamed,"Leads.View","Leads.Assign","Members.AssignSales","Members.View","Branches.AccessAll");
        await service.AssignLeadAsync(lead.Id,new(){SalesEmployeeId=mohamed.Id});await members.AssignSalesAsync(member.Id,new(){SalesEmployeeId=mohamed.Id});
        Assert.Equal(2,await db.LeadAssignments.CountAsync());Assert.Equal(2,await db.MemberSalesAssignments.CountAsync());
        Assert.Contains(await db.AuditLogs.ToListAsync(),a=>a.Action=="Lead.Unassigned");
    }

    [Fact] public async Task FutureDatedOwnershipPreventsPartialTermination()
    {
        var member=await OwnedMember();var assignment=await db.MemberSalesAssignments.SingleAsync();assignment.AssignedAt=access.Now.AddDays(1);await db.SaveChangesAsync();
        EmploymentManager();await Assert.ThrowsAsync<BusinessRuleException>(()=>Employees().ChangeStatusAsync(ahmed.Id,new(){Status=EmploymentStatus.Resigned}));
        Assert.Equal(EmploymentStatus.Active,(await db.Employees.AsNoTracking().SingleAsync(x=>x.Id==ahmed.Id)).EmploymentStatus);
        Assert.Null((await db.MemberSalesAssignments.AsNoTracking().SingleAsync()).EndedAt);
    }
}
