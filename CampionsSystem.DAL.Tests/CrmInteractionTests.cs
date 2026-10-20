using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CrmManagementTests
{
    [Fact] public async Task InteractionsAppendAndNewScheduleIsQueryable()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        Set(ahmed,"Leads.Manage","Leads.View");
        await service.AddLeadInteractionAsync(l.Id,new(){Notes="Call",InteractionType=InteractionType.Call,Status=LeadStatus.Contacted});
        var due=access.DayBounds().End.AddHours(2);
        await service.AddLeadInteractionAsync(l.Id,new(){Notes="Interested",InteractionType=InteractionType.WhatsApp,Status=LeadStatus.FollowUp,NextFollowUpAt=due});
        var interactions=await service.GetLeadInteractionsAsync(l.Id);
        Assert.Equal(2,interactions.Count);
        Assert.All(interactions,x=>Assert.Equal("Ahmed",x.EmployeeName));
        Assert.Equal(due,Assert.Single((await service.GetMyLeadsAsync(new(){FollowUp=FollowUpWindow.Upcoming})).Items).NextFollowUpAt);
        Assert.Contains(await db.AuditLogs.ToListAsync(),x=>x.Action=="Lead.InteractionAdded");
    }
    [Fact] public async Task DueTodayAndOverdueUseGymCalendarAndExcludeTerminalLeads()
    {
        var l=await Lead();var tomorrow=await Lead("01299999999");
        await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});await service.AssignLeadAsync(tomorrow.Id,new(){SalesEmployeeId=ahmed.Id});
        var (start,end)=access.DayBounds();
        await db.Leads.Where(x=>x.Id==l.Id).ExecuteUpdateAsync(x=>x.SetProperty(l=>l.NextFollowUpAt,start.AddHours(-1)));
        await db.Leads.Where(x=>x.Id==tomorrow.Id).ExecuteUpdateAsync(x=>x.SetProperty(l=>l.NextFollowUpAt,start.AddHours(1)));
        Assert.Equal(l.Id,Assert.Single((await service.GetMyLeadsAsync(new(){FollowUp=FollowUpWindow.Overdue})).Items).Id);
        Assert.Equal(tomorrow.Id,Assert.Single((await service.GetMyLeadsAsync(new(){FollowUp=FollowUpWindow.Today})).Items).Id);
    }
    [Fact] public async Task AnotherSalesCannotContactUnownedLeadAndInvalidSchedulingFails()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});Set(mohamed,"Leads.Manage","Leads.View");
        await Assert.ThrowsAsync<CampionsSystem.BLL.Branches.BranchNotFoundException>(()=>service.AddLeadInteractionAsync(l.Id,new(){Notes="Steal"}));
        Set(ahmed,"Leads.Manage","Leads.View");
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.AddLeadInteractionAsync(l.Id,new(){Notes="Missing due",Status=LeadStatus.FollowUp}));
        Assert.Empty(await service.GetLeadInteractionsAsync(l.Id));
    }
}
