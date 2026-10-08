using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Members;
public sealed partial class MemberManagementService
{
    // Assembly-internal, Lead-ID based entry point. No HTTP contract accepts a trusted owner.
    internal async Task<MemberDetails> CreateMemberFromLeadAsync(Guid leadId,CreateMemberRequest r,CancellationToken ct)
    {
        var user=await RequireAsync("Members.Create",ct);
        if(!user.Permissions.Contains("Leads.Convert")||db.Database.CurrentTransaction is null)throw new UnauthorizedAccessException();
        var lead=await db.Leads.FromSqlInterpolated($"SELECT * FROM \"Leads\" WHERE \"Id\"={leadId} FOR UPDATE").SingleOrDefaultAsync(ct)
            ??throw new BusinessRuleException("Lead not found.");
        await db.Entry(lead).ReloadAsync(ct);
        if(lead.Status is LeadStatus.Converted or LeadStatus.Lost or LeadStatus.Inactive||lead.ConvertedMemberId.HasValue)
            throw new BusinessRuleException("An active, unconverted Lead is required.");
        var assignment=await db.LeadAssignments.AsNoTracking().SingleOrDefaultAsync(a=>a.LeadId==leadId&&a.EndedAt==null,ct);
        if(user.Permissions.Contains("Leads.Assign"))BranchAccessService.EnsureScope(user,lead.BranchId);
        else if(user.EmployeeId is null||assignment?.SalesEmployeeId!=user.EmployeeId)throw new UnauthorizedAccessException();
        ValidateProfile(r);
        if(r.SalesEmployeeId.HasValue||r.TargetRegistrationBranchId==Guid.Empty)throw new BusinessRuleException("Conversion resolves ownership from the Lead.");
        Guid? owner=null;
        if(r.CreationType==MemberCreationType.FreelanceHouse)
        {
            if(!user.Permissions.Contains("FreelancePlans.ManageEligibleMembers")||!r.FreelancePlanId.HasValue)throw new UnauthorizedAccessException();
            BranchAccessService.EnsureScope(user,r.TargetRegistrationBranchId);
        }
        else
        {
            if(r.CreationType!=MemberCreationType.NormalSales||r.FreelancePlanId.HasValue||assignment is null)
                throw new BusinessRuleException("Assign a Sales owner before normal conversion, or use explicit FreelanceHouse.");
            owner=(await EligibleOwnerAsync(assignment.SalesEmployeeId,ct)).Id;
            if(r.TargetRegistrationBranchId!=lead.BranchId&&!user.Permissions.Contains("Members.CreateCrossBranch"))throw new UnauthorizedAccessException();
        }
        var code=await db.GymBranches.Where(b=>b.Id==lead.BranchId).Select(b=>b.CountryCallingCode).SingleAsync(ct);
        var canonical=lead.PhoneNormalized??PhoneNormalizer.Normalize(lead.Phone,code);
        // A destination country's calling code must not reinterpret the prospect's persisted number.
        var input=r.PrimaryPhone.Trim()==lead.Phone?canonical:PhoneNormalizer.Normalize(r.PrimaryPhone,code);
        if(input!=canonical)throw new BusinessRuleException("Update the Lead phone explicitly before conversion.");
        return await CreateCoreAsync(r,user,owner,leadId,canonical,ct);
    }
}
