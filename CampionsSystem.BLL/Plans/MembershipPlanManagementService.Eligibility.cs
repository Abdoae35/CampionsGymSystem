using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Plans;
public sealed partial class MembershipPlanManagementService
{
    private static void Freelance(MembershipPlan p)
    {
        if(p.PlanCategory!=PlanCategory.Freelance)throw new BusinessRuleException("Eligibility applies only to Freelance plans.");
    }
    public async Task<EligibleMemberListResult> GetEligibleMembersAsync(Guid id,CatalogListRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("FreelancePlans.ManageEligibleMembers",ct);Page(r);Freelance(await VisiblePlanAsync(id,user,ct));
        var q=db.MembershipPlanEligibleMembers.AsNoTracking().Where(e=>e.MembershipPlanId==id);
        if(r.IsActive.HasValue)q=q.Where(e=>e.Member.IsActive==r.IsActive);
        if(!string.IsNullOrWhiteSpace(r.Search))
        {
            var s=r.Search.Trim().ToLowerInvariant();q=q.Where(e=>e.Member.FullName.ToLower().Contains(s)||e.Member.PrimaryPhone.Contains(s)
                ||e.Member.PrimaryPhoneNormalized!=null&&e.Member.PrimaryPhoneNormalized.Contains(s)||e.Member.MemberCode.ToLower().Contains(s));
        }
        return new(await q.OrderBy(e=>e.Member.FullName).ThenBy(e=>e.MemberId).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)
            .Select(e=>new EligibleMemberDetails(e.MemberId,e.Member.FullName,e.Member.PrimaryPhone,e.Member.HomeBranchId,e.Member.ClientCode,e.Member.MemberCode,e.CreatedAt)).ToListAsync(ct),await q.CountAsync(ct),r.Page,r.PageSize);
    }
    public async Task<IReadOnlyList<EligibleMemberDetails>> FindEligibleMemberCandidatesAsync(Guid id,EligibleMemberCandidateRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("FreelancePlans.ManageEligibleMembers",ct);var plan=await VisiblePlanAsync(id,user,ct);Freelance(plan);
        if((r.MemberId.HasValue?1:0)+(!string.IsNullOrWhiteSpace(r.Phone)?1:0)+(!string.IsNullOrWhiteSpace(r.MemberCode)?1:0)!=1||r.MemberCode?.Length>256)
            throw new BusinessRuleException("Provide exactly one Member ID, exact phone or exact MemberCode.");
        var q=db.Members.AsNoTracking().Where(m=>m.IsActive);
        if(r.MemberId.HasValue)q=q.Where(m=>m.Id==r.MemberId);
        else if(!string.IsNullOrWhiteSpace(r.Phone))
        {
            var country=plan.BranchId.HasValue?await db.GymBranches.Where(b=>b.Id==plan.BranchId).Select(b=>b.CountryCallingCode).SingleAsync(ct):null;
            var normalized=PhoneNormalizer.Normalize(r.Phone,country);var display=r.Phone.Trim();
            q=q.Where(m=>m.PrimaryPhoneNormalized==normalized||m.PrimaryPhoneNormalized==null&&m.PrimaryPhone==display);
        }
        else{var code=r.MemberCode!.Trim();q=q.Where(m=>m.MemberCode==code);}
        return await q.OrderBy(m=>m.Id).Take(10).Select(m=>new EligibleMemberDetails(m.Id,m.FullName,m.PrimaryPhone,m.HomeBranchId,m.ClientCode,m.MemberCode,null)).ToListAsync(ct);
    }
    public Task<bool> AddEligibleMemberAsync(Guid id,EligibleMemberRequest r,CancellationToken ct=default)=>SetEligibilityAsync(id,r.MemberId,true,ct);
    public Task<bool> RemoveEligibleMemberAsync(Guid id,Guid memberId,CancellationToken ct=default)=>SetEligibilityAsync(id,memberId,false,ct);
    internal async Task<bool> SetEligibilityAsync(Guid id,Guid memberId,bool eligible,CancellationToken ct)
    {
        var user=await RequireAsync("FreelancePlans.ManageEligibleMembers",ct);
        return await TransactionAsync(async()=>{
            var plan=await LockPlanAsync(id,user,ct);Freelance(plan);
            var row=await db.MembershipPlanEligibleMembers.SingleOrDefaultAsync(e=>e.MembershipPlanId==id&&e.MemberId==memberId,ct);
            if(eligible)
            {
                if(!plan.IsActive)throw new BusinessRuleException("Select an active Freelance plan.");
                await ValidateCoachAsync(plan.FreelanceCoachEmployeeId!.Value,ct);
                var member=await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={memberId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
                if(member is null||!member.IsActive)throw new BusinessRuleException("Select an active Member.");
                if(row is not null)return false;
                row=new(){MembershipPlanId=id,MemberId=memberId,CreatedByUserId=user.UserId,CreatedAt=access.Now};db.Add(row);
            }
            else{if(row is null)return false;db.Remove(row);}
            access.Audit(user.UserId,plan.BranchId,eligible?"FreelancePlan.MemberEligible":"FreelancePlan.MemberEligibilityRemoved",nameof(MembershipPlanEligibleMember),row.Id,
                before:eligible?null:new{planId=id,memberId},after:eligible?new{planId=id,memberId}:null);
            await db.SaveChangesAsync(ct);return true;
        },ct);
    }
}
