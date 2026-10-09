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
    private ConvertLeadRequest Conversion(LeadDetails l,Guid? branch=null)=>new(){FullName=l.FullName!,PrimaryPhone=l.PrimaryPhone,TargetRegistrationBranchId=branch??a.Id};
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
    [Fact] public async Task EmployeeWithoutConversionCapabilityCannotConvertOrBypassLeadThroughMemberCreation()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        Set(mohamed,"Members.Create","Members.OwnSalesCustomers");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.ConvertLeadAsync(l.Id,Conversion(l)));
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
        var plan=new MembershipPlan{Name="Special",BranchId=a.Id,CityId=a.CityId,PlanCategory=PlanCategory.Freelance,DurationDays=30,
            FreelanceCoachEmployee=new(){FullName="Coach",Phone="coach",EmploymentType=EmploymentType.Freelance,HireDate=new(2020,1,1)}};
        db.Add(plan);await db.SaveChangesAsync();
        Set(mohamed,"Leads.Convert","Leads.Assign","Members.Create","FreelancePlans.ManageEligibleMembers");
        var r=Conversion(l);r.FreelancePlanId=plan.Id;var m=await service.ConvertLeadAsync(l.Id,r);
        Assert.True(m.IsHouse);Assert.Null(m.CurrentSalesOwner);Assert.Single(m.EligibleFreelancePlans);
        Assert.False(await db.MemberSalesAssignments.AnyAsync(x=>x.MemberId==m.Id));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AuthorizedNonOwnerRegistersWalkInWithoutTakingOwnership(bool ownerTransferred)
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        if(ownerTransferred)
        {
            EmploymentManager();await EmployeeBranches().TransferAsync(ahmed.Id,new(b.Id,access.Today));
        }
        EmploymentManager();await EmployeeBranches().TransferAsync(mohamed.Id,new(b.Id,access.Today));
        // Ali/Mohamed registers at B. Source A is not browsing scope, nor is assignment permission granted.
        Set(mohamed,"Leads.Convert","Members.Create","Leads.View","Members.View");
        current.Context=current.Context! with{CurrentBranch=new(b.Id,b.Name)};
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.GetLeadByIdAsync(l.Id));
        var m=await service.ConvertLeadAsync(l.Id,Conversion(l,b.Id));
        Assert.Equal(ahmed.Id,m.CurrentSalesOwner?.SalesEmployeeId);Assert.False(m.IsHouse);
        Assert.Equal(101,m.ClientCode);Assert.Equal(b.Id,m.RegistrationBranch.Id);
        Assert.Equal(mohamed.UserId,(await db.Members.SingleAsync()).CreatedByUserId);
        Assert.Equal(mohamed.UserId,(await db.AuditLogs.SingleAsync(x=>x.Action=="Lead.Converted")).UserId);
        Assert.Equal(ahmed.Id,(await db.LeadAssignments.SingleAsync()).SalesEmployeeId);
        Assert.NotNull((await db.LeadAssignments.SingleAsync()).EndedAt);
        Assert.False(await db.MemberSalesAssignments.AnyAsync(x=>x.SalesEmployeeId==mohamed.Id));
        Assert.Equal(ownerTransferred?b.Id:a.Id,(await new EmployeeBranchResolver(db).ResolveAsync(ahmed.Id,access.Today))!.Id);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.GetLeadByIdAsync(l.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>access.RequireAsync("Members.View",a.Id,default));
    }

    [Theory]
    [InlineData("create")]
    [InlineData("convert")]
    [InlineData("employee")]
    [InlineData("inactive")]
    [InlineData("destination")]
    public async Task OperationalConversionRequiresCapabilitiesActiveEmployeeAndDestinationScope(string missing)
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        Set(mohamed,"Leads.Convert","Members.Create");
        if(missing=="create")current.Context=current.Context! with{Permissions=["Leads.Convert"]};
        if(missing=="convert")current.Context=current.Context! with{Permissions=["Members.Create"]};
        if(missing=="employee")current.Context=current.Context! with{EmployeeId=null};
        if(missing=="inactive"){mohamed.EmploymentStatus=EmploymentStatus.Resigned;await db.SaveChangesAsync();}
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.ConvertLeadAsync(l.Id,Conversion(l,missing=="destination"?b.Id:a.Id)));
        Assert.Empty(await db.Members.ToListAsync());Assert.Null((await db.LeadAssignments.SingleAsync()).EndedAt);
    }

    [Fact] public async Task NonOwnerConversionCannotChangeLeadPhoneOrUsePublicOwnershipOverride()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        Set(mohamed,"Leads.Convert","Members.Create");
        var r=Conversion(l);r.PrimaryPhone="+201211111111";
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.ConvertLeadAsync(l.Id,r));
        Assert.Empty(await db.Members.ToListAsync());Assert.Equal(ahmed.Id,(await db.LeadAssignments.SingleAsync()).SalesEmployeeId);
    }

    [Fact] public async Task ConversionUsesLatestAuthorizedReassignmentRatherThanOriginalOwnerOrActor()
    {
        var l=await Lead();await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=ahmed.Id});
        await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=mohamed.Id});
        Set(ahmed,"Leads.Convert","Members.Create");
        var member=await service.ConvertLeadAsync(l.Id,Conversion(l));
        Assert.Equal(mohamed.Id,member.CurrentSalesOwner?.SalesEmployeeId);
        Assert.Equal(2,await db.LeadAssignments.CountAsync());Assert.All(await db.LeadAssignments.ToListAsync(),x=>Assert.NotNull(x.EndedAt));
        Assert.Equal(ahmed.UserId,(await db.Members.SingleAsync()).CreatedByUserId);
    }

}
