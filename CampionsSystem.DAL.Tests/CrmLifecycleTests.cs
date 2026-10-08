using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CrmManagementTests
{
    [Fact] public async Task LostLeadRemainsAndManagerReactivatesWithoutLosingOwner()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        Set(ahmed,"Leads.View","Leads.Manage");var lost=await service.ChangeLeadStatusAsync(l.Id,new(){Status=LeadStatus.Lost,Reason="Not now"});
        Assert.Equal("Lost",lost.Status);Assert.NotNull(lost.LostAt);Assert.Equal("Not now",lost.LostReason);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.ChangeLeadStatusAsync(l.Id,new(){Status=LeadStatus.FollowUp,NextFollowUpAt=access.Now.AddDays(2)}));
        Manager();var live=await service.ChangeLeadStatusAsync(l.Id,new(){Status=LeadStatus.FollowUp,NextFollowUpAt=access.Now.AddDays(2)});
        Assert.Equal(ahmed.Id,live.CurrentAssignment?.SalesEmployeeId);Assert.Null(live.LostAt);
        Assert.Contains(await db.AuditLogs.ToListAsync(),x=>x.Action=="Lead.Reactivated");Assert.Single(await db.Leads.ToListAsync());
    }
    [Fact] public async Task LifecycleCannotPretendToConvertOrEraseContactMeaning()
    {
        var l=await Lead();await Assert.ThrowsAsync<BusinessRuleException>(()=>service.ChangeLeadStatusAsync(l.Id,new(){Status=LeadStatus.Converted}));
        await service.AddLeadInteractionAsync(l.Id,new(){Notes="Contacted"});
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.ChangeLeadStatusAsync(l.Id,new(){Status=LeadStatus.New}));
    }
    [Fact] public async Task LeadProfileCannotChangeOwnerOrOperationalBranch()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        var updated=await service.UpdateLeadAsync(l.Id,new(){FullName="Updated",PrimaryPhone=l.PrimaryPhone,Notes="New notes"});
        Assert.Equal(a.Id,updated.BranchId);Assert.Equal(ahmed.Id,updated.CurrentAssignment?.SalesEmployeeId);Assert.Equal("Updated",updated.FullName);
        await Lead("01299999999");await Assert.ThrowsAsync<BusinessRuleException>(()=>service.UpdateLeadAsync(l.Id,new(){FullName="Duplicate",PrimaryPhone="01299999999"}));
    }
}
