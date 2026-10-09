using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Members;
public sealed partial class MemberManagementService
{
    // Operational conversion is a narrow capability; it does not grant Lead browsing or assignment.
    internal async Task<CurrentUserContext> RequireLeadConversionAccessAsync(Guid targetBranchId, bool freelanceHouse, CancellationToken ct)
    {
        var user = await RequireAsync("Members.Create", ct);
        if (!user.Permissions.Contains("Leads.Convert") || user.EmployeeId is null)
            throw new UnauthorizedAccessException();
        if (!await db.Employees.AnyAsync(e => e.Id == user.EmployeeId && e.UserId == user.UserId
            && e.EmploymentStatus == EmploymentStatus.Active && e.HireDate <= access.Today
            && (e.TerminationDate == null || e.TerminationDate >= access.Today), ct))
            throw new UnauthorizedAccessException();
        if (freelanceHouse)
        {
            if (!user.Permissions.Contains("FreelancePlans.ManageEligibleMembers")) throw new UnauthorizedAccessException();
            BranchAccessService.EnsureScope(user, targetBranchId);
        }
        else if (targetBranchId != user.CurrentBranchId)
        {
            if (user.CurrentBranchId is null || !user.Permissions.Contains("Members.CreateCrossBranch"))
                throw new UnauthorizedAccessException();
        }
        else BranchAccessService.EnsureScope(user, targetBranchId);
        return user;
    }

    // Assembly-internal, Lead-ID based entry point. No HTTP contract accepts a trusted owner.
    internal async Task<MemberDetails> CreateMemberFromLeadAsync(Guid leadId,CreateMemberRequest r,CancellationToken ct)
    {
        var user=await RequireLeadConversionAccessAsync(r.TargetRegistrationBranchId,r.CreationType==MemberCreationType.FreelanceHouse,ct);
        if(db.Database.CurrentTransaction is null)throw new UnauthorizedAccessException();
        var lead=await db.Leads.FromSqlInterpolated($"SELECT * FROM \"Leads\" WHERE \"Id\"={leadId} FOR UPDATE").SingleOrDefaultAsync(ct)
            ??throw new BusinessRuleException("Lead not found.");
        await db.Entry(lead).ReloadAsync(ct);
        if(lead.Status is LeadStatus.Converted or LeadStatus.Lost or LeadStatus.Inactive||lead.ConvertedMemberId.HasValue)
            throw new BusinessRuleException("An active, unconverted Lead is required.");
        var assignment=await db.LeadAssignments.AsNoTracking().SingleOrDefaultAsync(a=>a.LeadId==leadId&&a.EndedAt==null,ct);
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
        }
        var code=await db.GymBranches.Where(b=>b.Id==lead.BranchId).Select(b=>b.CountryCallingCode).SingleAsync(ct);
        var canonical=lead.PhoneNormalized??PhoneNormalizer.Normalize(lead.Phone,code);
        // A destination country's calling code must not reinterpret the prospect's persisted number.
        var input=r.PrimaryPhone.Trim()==lead.Phone?canonical:PhoneNormalizer.Normalize(r.PrimaryPhone,code);
        if(input!=canonical)throw new BusinessRuleException("Update the Lead phone explicitly before conversion.");
        return await CreateCoreAsync(r,user,owner,leadId,canonical,ct);
    }
}
