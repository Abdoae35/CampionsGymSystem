using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Crm;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CrmManagementTests
{
    [Fact] public async Task AssignmentReassignmentPreservesHistoryAndMyLeads()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=mohamed.Id,Reason="Distribution"});
        var history=await service.GetLeadAssignmentHistoryAsync(l.Id);Assert.Equal(2,history.Count);Assert.Single(history,a=>a.EndedAt==null);
        Set(ahmed,"Leads.View");Assert.Empty((await service.GetMyLeadsAsync(new())).Items);
        Set(mohamed,"Leads.View");Assert.Single((await service.GetMyLeadsAsync(new())).Items);
        Assert.Empty((await service.GetMyLeadsAsync(new(){SalesEmployeeId=ahmed.Id})).Items);
        Assert.Contains(await db.AuditLogs.ToListAsync(),a=>a.Action=="Lead.Reassigned");
    }
    [Fact] public async Task IneligibleAndWrongBranchEmployeesCannotOwnLeads()
    {
        var l=await Lead();capabilities.Eligible=false;
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id}));
        capabilities.Eligible=true;
        await db.EmployeeBranchAssignments.Where(a=>a.EmployeeId==mohamed.Id).ExecuteUpdateAsync(x=>x.SetProperty(a=>a.BranchId,b.Id));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=mohamed.Id}));
    }
    [Fact] public async Task SalesCannotAssignOrStealAnotherLead()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        Set(mohamed,"Leads.View","Leads.Manage","Members.OwnSalesCustomers");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=mohamed.Id}));
        Assert.Empty((await service.GetMyLeadsAsync(new())).Items);
    }
    [Fact] public async Task EligibleSalesCreatingProspectGetsTheirOwnAssignment()
    {
        Set(ahmed,"Leads.View","Leads.Manage","Members.OwnSalesCustomers");var l=await Lead();
        Assert.Equal(ahmed.Id,l.CurrentAssignment?.SalesEmployeeId);Assert.Single((await service.GetMyLeadsAsync(new())).Items);
    }
}
