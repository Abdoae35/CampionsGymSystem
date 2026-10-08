using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Branches;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CrmManagementTests
{
    private async Task<MemberDetails> OwnedMember(Guid? branch=null)
    {
        return await members.CreateMemberAsync(new(){FullName="Owned",PrimaryPhone="+201212345678",TargetRegistrationBranchId=branch??a.Id,
            CreationType=MemberCreationType.ManagementCreated,SalesEmployeeId=ahmed.Id});
    }
    [Fact] public async Task CrossBranchMemberWorkBelongsOnlyToCurrentOwner()
    {
        var m=await OwnedMember(b.Id);var task=await service.OpenMemberFollowUpAsync(m.Id,new(){Reason="Reactivation",DueAt=access.Now.AddHours(1)});
        Set(ahmed,"CRM.ViewMemberFollowUps","CRM.ManageMemberFollowUps","Members.View");
        var work=Assert.Single((await service.GetMyFollowUpsAsync(new())).Items);Assert.Equal(m.Id,work.MemberId);Assert.True(work.CrossBranch);
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>members.GetMemberByIdAsync(m.Id));
        Set(mohamed,"CRM.ViewMemberFollowUps","CRM.ManageMemberFollowUps");Assert.Empty((await service.GetMyFollowUpsAsync(new())).Items);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.CompleteMemberFollowUpAsync(task));
        Set(ahmed,"CRM.ViewMemberFollowUps","CRM.ManageMemberFollowUps");await service.CompleteMemberFollowUpAsync(task);
        Assert.Empty((await service.GetMyFollowUpsAsync(new())).Items);Assert.NotNull((await db.MemberFollowUpTasks.SingleAsync()).CompletedAt);
    }
    [Fact] public async Task HouseQueueDistributionUsesExistingMemberAssignmentAndTracksReassignment()
    {
        var m=await OwnedMember();await members.ReturnMemberToHouseAsync(m.Id,new(){Reason="House"});
        var task=await service.OpenMemberFollowUpAsync(m.Id,new(){Reason="InactiveMember",DueAt=access.Now});
        Assert.Single((await service.GetHouseFollowUpsAsync(new())).Items);Assert.Empty((await service.GetMyFollowUpsAsync(new())).Items);
        await service.AssignHouseFollowUpAsync(task,new(){SalesEmployeeId=ahmed.Id});Assert.Empty((await service.GetHouseFollowUpsAsync(new())).Items);
        Assert.Single((await service.GetMyFollowUpsAsync(new())).Items);
        await members.AssignSalesAsync(m.Id,new(){SalesEmployeeId=mohamed.Id});Assert.Empty((await service.GetMyFollowUpsAsync(new())).Items);
        Set(mohamed,"CRM.ViewMemberFollowUps");Assert.Single((await service.GetMyFollowUpsAsync(new())).Items);
        Assert.Contains(await db.AuditLogs.ToListAsync(),a=>a.Action=="HouseMember.AssignedForFollowUp");
    }
    [Fact] public async Task EquivalentOpenWorkIsUpdatedWithoutDuplicatesAndCompletedWorkRemains()
    {
        var m=await OwnedMember();var first=await service.OpenMemberFollowUpAsync(m.Id,new(){DueAt=access.Now});
        var second=await service.OpenMemberFollowUpAsync(m.Id,new(){DueAt=access.Now.AddDays(1)});Assert.Equal(first,second);
        await service.CompleteMemberFollowUpAsync(first);var third=await service.OpenMemberFollowUpAsync(m.Id,new(){DueAt=access.Now.AddDays(2)});
        Assert.NotEqual(first,third);Assert.Equal(2,await db.MemberFollowUpTasks.CountAsync());Assert.Equal(1,await db.MemberFollowUpTasks.CountAsync(t=>t.CompletedAt==null));
    }
    [Fact] public async Task UnifiedQueueContainsOnlyAssignedLeadAndOwnedMemberWork()
    {
        var m=await OwnedMember();await service.OpenMemberFollowUpAsync(m.Id,new(){DueAt=access.Now.AddDays(1)});
        var l=await Lead("01299999999");await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        await service.AddLeadInteractionAsync(l.Id,new(){Notes="Follow",NextFollowUpAt=access.Now.AddDays(1)});
        Assert.Equal(2,(await service.GetMyFollowUpsAsync(new())).TotalCount);
        Assert.Equal(2,(await service.GetMyFollowUpsAsync(new(){Window=FollowUpWindow.Upcoming})).TotalCount);
    }
}
