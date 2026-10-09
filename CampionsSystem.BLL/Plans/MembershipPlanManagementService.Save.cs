using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Plans;
public sealed partial class MembershipPlanManagementService
{
    private async Task BranchWriterAsync(CurrentUserContext user,Guid? branchId,CancellationToken ct)
    {
        BranchAccessService.EnsureScope(user,branchId);
        if(branchId.HasValue)
        {
            var branch=await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={branchId.Value} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
            if(branch is null||!branch.IsActive)throw new BusinessRuleException("An active owning branch is required.");
        }
    }
    private async Task<MembershipPlan> LockPlanAsync(Guid id,CurrentUserContext user,CancellationToken ct)
    {
        await VisiblePlanAsync(id,user,ct);
        var p=await db.MembershipPlans.FromSqlInterpolated($"SELECT * FROM \"MembershipPlans\" WHERE \"Id\"={id} FOR UPDATE").SingleAsync(ct);
        await db.Entry(p).ReloadAsync(ct);BranchAccessService.EnsureScope(user,p.BranchId);
        if(!Global(user))await BranchWriterAsync(user,p.BranchId,ct);return p;
    }
    private async Task ValidatePlanAsync(UpdateMembershipPlanRequest r,MembershipPlan? old,CancellationToken ct)
    {
        if(r.BranchId==Guid.Empty||r.CityId==Guid.Empty||string.IsNullOrWhiteSpace(r.Name)||r.Name.Length>256
            ||r.PlanType is null||!Enum.IsDefined(r.PlanType.Value)||r.Mode is null||!Enum.IsDefined(r.Mode.Value)
            ||r.FreezeDays<0||!r.AllowFreeze&&r.FreezeDays!=0||r.Price>9999999999999999.99m)
            throw new BusinessRuleException("Invalid plan branch, city, name, type, mode, price or freeze configuration.");
        OperationAccess.Money(r.Price);
        if(!(r.Mode==AccessMode.Unlimited&&r.Sessions is null||r.Mode==AccessMode.SessionBased&&r.Sessions>0))throw new BusinessRuleException("Unlimited needs no sessions; SessionBased needs positive sessions.");
        if(r.PlanType==PlanCategory.Employee)
        {
            if(r.Price!=0||r.Mode!=AccessMode.Unlimited||r.DurationDays!=0||r.FreelanceCoachEmployeeId.HasValue)throw new BusinessRuleException("Employee plans are free, Unlimited, duration zero and have no Freelance Coach.");
        }
        else if(r.DurationDays<=0)throw new BusinessRuleException("Customer plans require positive duration.");
        if(old is not null&&old.PlanCategory!=r.PlanType)throw new BusinessRuleException("Behavioral type is immutable; create a new plan for another type.");
        if(!await db.Cities.AnyAsync(c=>c.Id==r.CityId&&c.IsActive,ct))throw new BusinessRuleException("An active access city is required.");
        if(r.AudienceCategoryId.HasValue)
        {
            // Keep existing inactive classifications readable/editable; newly selected classifications must be active.
            var category=await db.MembershipPlanCategories.FromSqlInterpolated($"SELECT * FROM \"MembershipPlanCategories\" WHERE \"Id\"={r.AudienceCategoryId.Value} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
            if(category is null||!category.IsActive&&old?.MembershipPlanCategoryId!=category.Id)throw new BusinessRuleException("An active audience category is required when selecting a category.");
        }
        if(r.PlanType==PlanCategory.Freelance)
        {
            if(!r.FreelanceCoachEmployeeId.HasValue)throw new BusinessRuleException("Freelance Coach is required.");
            await ValidateCoachAsync(r.FreelanceCoachEmployeeId.Value,ct);
        }
        else if(r.FreelanceCoachEmployeeId.HasValue)throw new BusinessRuleException("Only Freelance plans may reference a Freelance Coach.");
    }
    private async Task ValidateCoachAsync(Guid id,CancellationToken ct)
    {
        var coach=await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\"={id} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if(coach is null||coach.EmploymentType!=EmploymentType.Freelance||coach.EmploymentStatus!=EmploymentStatus.Active||coach.HireDate>access.Today||coach.TerminationDate<access.Today)
            throw new BusinessRuleException("An active Freelance Employee is required.");
    }
    private static object PlanValues(MembershipPlan p)=>new{p.Name,p.BranchId,p.CityId,p.PlanCategory,p.MembershipPlanCategoryId,p.Price,p.DurationDays,p.AccessMode,p.SessionCount,p.AllowFreeze,p.IncludedFreezeDays,p.FreelanceCoachEmployeeId,p.IsActive};
    private static void SetPlan(MembershipPlan p,UpdateMembershipPlanRequest r)
    {
        p.BranchId=r.BranchId;p.CityId=r.CityId;p.Name=r.Name.Trim();p.PlanCategory=r.PlanType!.Value;p.MembershipPlanCategoryId=r.AudienceCategoryId;
        p.Price=r.Price;p.DurationDays=r.DurationDays;p.AccessMode=r.Mode!.Value;p.SessionCount=r.Sessions;p.AllowFreeze=r.AllowFreeze;p.IncludedFreezeDays=r.FreezeDays;p.FreelanceCoachEmployeeId=r.FreelanceCoachEmployeeId;
    }
    private async Task<MembershipPlanDetails> SavePlanAsync(Guid? id,UpdateMembershipPlanRequest r,string permission,CancellationToken ct,bool? status=null,bool legacy=false)
    {
        var user=await RequireAsync(permission,ct);
        return await TransactionAsync(async()=>{
            var p=id.HasValue?await LockPlanAsync(id.Value,user,ct):new MembershipPlan{CreatedAt=access.Now};
            // Older Freelance API has no audience/freeze inputs; preserve these template fields under the same lock.
            if(legacy&&id.HasValue){r.AudienceCategoryId=p.MembershipPlanCategoryId;r.AllowFreeze=p.AllowFreeze;r.FreezeDays=p.IncludedFreezeDays;}
            await BranchWriterAsync(user,r.BranchId,ct);await ValidatePlanAsync(r,id.HasValue?p:null,ct);
            var old=id.HasValue?PlanValues(p):null;SetPlan(p,r);if(status.HasValue)p.IsActive=status.Value;if(!id.HasValue)db.Add(p);
            access.Audit(user.UserId,p.BranchId,id.HasValue?"MembershipPlan.Updated":"MembershipPlan.Created",nameof(MembershipPlan),p.Id,old,PlanValues(p));
            if(legacy)access.Audit(user.UserId,p.BranchId,id.HasValue?"FreelancePlan.Edited":"FreelancePlan.Created",nameof(MembershipPlan),p.Id,old,PlanValues(p));
            await db.SaveChangesAsync(ct);return await DetailsAsync(p.Id,ct);
        },ct);
    }
    public Task<MembershipPlanDetails> CreatePlanAsync(CreateMembershipPlanRequest r,CancellationToken ct=default)=>SavePlanAsync(null,r,"MembershipPlans.Manage",ct);
    internal async Task<Guid> SaveFreelancePlanAsync(Guid? id,SaveFreelancePlanRequest r,CancellationToken ct)
    {
        var details=await SavePlanAsync(id,new UpdateMembershipPlanRequest{BranchId=r.BranchId??Guid.Empty,CityId=r.CityId,Name=r.Name,Price=r.Price,
            DurationDays=r.DurationDays,Mode=r.AccessMode,Sessions=r.SessionCount,PlanType=PlanCategory.Freelance,FreelanceCoachEmployeeId=r.CoachEmployeeId},"FreelancePlans.Manage",ct,r.IsActive,true);
        return details.Plan.Id;
    }
}
