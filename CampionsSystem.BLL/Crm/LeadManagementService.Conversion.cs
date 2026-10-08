using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Crm;
public sealed partial class LeadManagementService
{
    public async Task<MemberDetails> ConvertLeadAsync(Guid id,ConvertLeadRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.Convert",ct);
        return await TransactionAsync(async()=>{
            var lead=await LockedAsync(id,user,ct);
            if(lead.Status is LeadStatus.Converted or LeadStatus.Lost or LeadStatus.Inactive||lead.ConvertedMemberId.HasValue)
                throw new BusinessRuleException("An active unconverted Lead is required.");
            var assignment=await db.LeadAssignments.SingleOrDefaultAsync(a=>a.LeadId==id&&a.EndedAt==null,ct);
            if(assignment is not null){await db.Entry(assignment).ReloadAsync(ct);if(assignment.AssignedAt>access.Now)throw new BusinessRuleException("Reconcile future assignment first.");}
            var member=await members.CreateMemberFromLeadAsync(id,new(){FullName=r.FullName,PrimaryPhone=r.PrimaryPhone,
                SecondaryPhone=r.SecondaryPhone,DateOfBirth=r.DateOfBirth,Gender=r.Gender,Notes=r.Notes,
                TargetRegistrationBranchId=r.TargetRegistrationBranchId,FreelancePlanId=r.FreelancePlanId,
                CreationType=r.FreelancePlanId.HasValue?MemberCreationType.FreelanceHouse:MemberCreationType.NormalSales},ct);
            var old=new{lead.Status,lead.NextFollowUpAt,SalesEmployeeId=assignment?.SalesEmployeeId};
            if(assignment is not null)assignment.EndedAt=access.Now;
            lead.Status=LeadStatus.Converted;lead.ConvertedMemberId=member.Id;lead.ConvertedAt=access.Now;lead.NextFollowUpAt=null;
            access.Audit(user.UserId,lead.BranchId,"Lead.Converted",nameof(Lead),id,old,
                new{lead.ConvertedMemberId,lead.ConvertedAt,member.RegistrationBranch,member.CurrentSalesOwner,ExplicitFreelanceHouse=r.FreelancePlanId.HasValue});
            await db.SaveChangesAsync(ct);return member;
        },ct);
    }
}
