using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Plans;
public sealed partial class MembershipPlanManagementService
{
    private bool Global(CurrentUserContext u)=>u.Permissions.Contains("Branches.AccessAll");
    private IQueryable<MembershipPlan> Scoped(CurrentUserContext u)
    {
        var q=db.MembershipPlans.AsNoTracking();
        if(!Global(u))q=q.Where(p=>p.BranchId==u.CurrentBranchId&&p.BranchId!=null&&p.Branch!.IsActive);
        return q;
    }
    private IQueryable<MembershipPlanSummary> Summaries(IQueryable<MembershipPlan> q)=>q.Select(p=>new MembershipPlanSummary(p.Id,p.Name,p.PlanCategory.ToString(),
        p.BranchId==null?null:new PlanReference(p.BranchId.Value,p.Branch!.Name),new(p.CityId,p.City.Name),
        p.MembershipPlanCategoryId==null?null:new PlanReference(p.MembershipPlanCategoryId.Value,p.MembershipPlanCategory!.Name),
        p.AccessMode.ToString(),p.Price,p.DurationDays,p.SessionCount,p.AllowFreeze,p.IncludedFreezeDays,p.IsActive,
        p.FreelanceCoachEmployeeId==null?null:new PlanReference(p.FreelanceCoachEmployeeId.Value,p.FreelanceCoachEmployee!.FullName)));
    private async Task<MembershipPlan> VisiblePlanAsync(Guid id,CurrentUserContext user,CancellationToken ct)
        =>await Scoped(user).SingleOrDefaultAsync(p=>p.Id==id,ct)??throw new BranchNotFoundException("Plan not found in accessible scope.");
    private Task<List<PlanBenefitDetails>> BenefitsAsync(Guid id,CancellationToken ct)=>db.MembershipPlanBenefits.AsNoTracking().Where(b=>b.MembershipPlanId==id)
        .OrderBy(b=>b.MembershipBenefit.Name).ThenBy(b=>b.Id).Select(b=>new PlanBenefitDetails(b.Id,b.MembershipBenefitId,b.MembershipBenefit.Code,b.MembershipBenefit.Name,b.MembershipBenefit.IsActive,b.GrantType.ToString(),b.Quantity)).ToListAsync(ct);
    private async Task<MembershipPlanDetails> DetailsAsync(Guid id,CancellationToken ct)=>new(await Summaries(db.MembershipPlans.AsNoTracking().Where(p=>p.Id==id)).SingleAsync(ct),
        await db.MembershipPlans.Where(p=>p.Id==id).Select(p=>p.CreatedAt).SingleAsync(ct),await BenefitsAsync(id,ct),await db.MembershipPlanEligibleMembers.CountAsync(e=>e.MembershipPlanId==id,ct));
    public async Task<MembershipPlanListResult> GetPlansAsync(MembershipPlanListRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("MembershipPlans.View",ct);Page(r);
        if(r.PlanType.HasValue&&!Enum.IsDefined(r.PlanType.Value)||r.Mode.HasValue&&!Enum.IsDefined(r.Mode.Value))throw new CampionsSystem.BLL.Operations.BusinessRuleException("Invalid plan filters.");
        if((r.IncludeInternal||r.PlanType==PlanCategory.Employee)&&!user.Permissions.Contains("MembershipPlans.Manage"))throw new UnauthorizedAccessException();
        var q=Scoped(user);
        if(!r.IncludeInternal&&r.PlanType!=PlanCategory.Employee)q=q.Where(p=>p.PlanCategory!=PlanCategory.Employee);
        if(r.BranchId.HasValue){BranchAccessService.EnsureScope(user,r.BranchId);q=q.Where(p=>p.BranchId==r.BranchId);}
        if(r.CityId.HasValue)q=q.Where(p=>p.CityId==r.CityId);
        if(r.PlanType.HasValue)q=q.Where(p=>p.PlanCategory==r.PlanType);
        if(r.AudienceCategoryId.HasValue)q=q.Where(p=>p.MembershipPlanCategoryId==r.AudienceCategoryId);
        if(r.Mode.HasValue)q=q.Where(p=>p.AccessMode==r.Mode);
        if(r.FreelanceCoachEmployeeId.HasValue)q=q.Where(p=>p.FreelanceCoachEmployeeId==r.FreelanceCoachEmployeeId);
        if(r.IsActive.HasValue)q=q.Where(p=>p.IsActive==r.IsActive);
        if(!string.IsNullOrWhiteSpace(r.Search)){var s=r.Search.Trim().ToLowerInvariant();q=q.Where(p=>p.Name.ToLower().Contains(s));}
        return new(await Summaries(q.OrderBy(p=>p.Name).ThenBy(p=>p.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)).ToListAsync(ct),await q.CountAsync(ct),r.Page,r.PageSize);
    }
    public async Task<MembershipPlanDetails> GetPlanByIdAsync(Guid id,CancellationToken ct=default)
    {
        var user=await RequireAsync("MembershipPlans.View",ct);var p=await VisiblePlanAsync(id,user,ct);
        if(p.PlanCategory==PlanCategory.Employee&&!user.Permissions.Contains("MembershipPlans.Manage"))throw new BranchNotFoundException("Customer plan not found.");
        return await DetailsAsync(id,ct);
    }
}
