using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CrmManagementTests
{
    private ConvertLeadRequest Conversion(LeadDetails l,Guid? branch=null)=>new(){FullName=l.FullName,PrimaryPhone=l.PrimaryPhone,TargetRegistrationBranchId=branch??a.Id};
    [Fact] public async Task ManagerConversionKeepsOriginalOwnerAndDestinationCodeWithoutPurchase()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        Set(mohamed,"Leads.Convert","Leads.Assign","Members.Create","Members.CreateCrossBranch");
        var m=await service.ConvertLeadAsync(l.Id,Conversion(l,b.Id));Assert.Equal(ahmed.Id,m.CurrentSalesOwner?.SalesEmployeeId);
        Assert.Equal(b.Id,m.RegistrationBranch.Id);Assert.Equal(101,m.ClientCode);Assert.Equal("+201212345678",m.PrimaryPhoneNormalized);
        Assert.Equal("01212345678",m.PrimaryPhone);Assert.Equal(a.Id,(await db.EmployeeBranchAssignments.SingleAsync(a=>a.EmployeeId==ahmed.Id)).BranchId);
        var converted=await db.Leads.SingleAsync(x=>x.Id==l.Id);Assert.Equal(m.Id,converted.ConvertedMemberId);Assert.Null(converted.NextFollowUpAt);
        Assert.All(await db.LeadAssignments.ToListAsync(),a=>Assert.NotNull(a.EndedAt));
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.SaleTransactions.ToListAsync());
        Assert.Empty(await db.CheckIns.ToListAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>access.RequireAsync("Members.Create",b.Id,default));
        Assert.Contains(await db.AuditLogs.ToListAsync(),a=>a.Action=="Lead.Converted");
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.ConvertLeadAsync(l.Id,Conversion(l)));
    }
    [Fact] public async Task AssignedOwnerConvertsWithoutBroadAssignmentPermission()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        Set(ahmed,"Leads.Convert","Members.Create");var m=await service.ConvertLeadAsync(l.Id,Conversion(l));Assert.Equal(ahmed.Id,m.CurrentSalesOwner?.SalesEmployeeId);
        Set(ahmed,"Leads.View");Assert.Single((await service.GetMyLeadsAsync(new(){Status=LeadStatus.Converted})).Items);
        Assert.Empty((await service.GetMyLeadsAsync(new())).Items);
    }
    [Fact] public async Task RandomSalesCannotConvertOrBypassLeadThroughMemberCreation()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        Set(mohamed,"Leads.Convert","Members.Create","Members.OwnSalesCustomers");
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.ConvertLeadAsync(l.Id,Conversion(l)));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>members.CreateMemberAsync(new(){FullName="Walk-in",PrimaryPhone=l.PrimaryPhone,TargetRegistrationBranchId=a.Id,CreationType=MemberCreationType.NormalSales}));
        Assert.Empty(await db.Members.ToListAsync());Assert.Equal(0,await db.GymBranches.Where(b=>b.Id==a.Id).Select(b=>b.LastMemberClientCode).SingleAsync());
    }
    [Fact] public async Task UnassignedOrDuplicateMemberConversionFailsAtomically()
    {
        var l=await Lead();await Assert.ThrowsAsync<BusinessRuleException>(()=>service.ConvertLeadAsync(l.Id,Conversion(l)));
        await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        db.Members.Add(new(){FullName="Existing",PrimaryPhone=l.PrimaryPhone,PrimaryPhoneNormalized=l.PrimaryPhoneNormalized,MemberCode="existing",HomeBranchId=a.Id,CreatedByUserId=ahmed.UserId!.Value});await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.ConvertLeadAsync(l.Id,Conversion(l)));
        Assert.Equal(LeadStatus.New,(await db.Leads.SingleAsync()).Status);Assert.Single(await db.Members.ToListAsync());
    }
    [Fact] public async Task ExplicitFreelanceConversionOverridesLeadOwnerWithAuditedHouseException()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        var plan=new MembershipPlan{Name="Special",CityId=a.CityId,PlanCategory=PlanCategory.Freelance,DurationDays=30,
            FreelanceCoachEmployee=new(){FullName="Coach",Phone="coach",EmploymentType=EmploymentType.Freelance,HireDate=new(2020,1,1)}};
        db.Add(plan);await db.SaveChangesAsync();
        Set(mohamed,"Leads.Convert","Leads.Assign","Members.Create","FreelancePlans.ManageEligibleMembers");
        var r=Conversion(l);r.FreelancePlanId=plan.Id;var m=await service.ConvertLeadAsync(l.Id,r);
        Assert.True(m.IsHouse);Assert.Null(m.CurrentSalesOwner);Assert.Single(m.EligibleFreelancePlans);
        Assert.False(await db.MemberSalesAssignments.AnyAsync(x=>x.MemberId==m.Id));
    }
}
