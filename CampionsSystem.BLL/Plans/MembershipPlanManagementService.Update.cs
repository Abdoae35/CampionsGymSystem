using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
namespace CampionsSystem.BLL.Plans;
public sealed partial class MembershipPlanManagementService
{
    public Task<MembershipPlanDetails> UpdatePlanAsync(Guid id,UpdateMembershipPlanRequest r,CancellationToken ct=default)
        =>SavePlanAsync(id,r,"MembershipPlans.Manage",ct);

    public async Task<MembershipPlanDetails> ChangePlanStatusAsync(Guid id,ChangePlanStatusRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("MembershipPlans.Manage",ct);
        if(r.IsActive is null)throw new BusinessRuleException("Status is required.");
        return await TransactionAsync(async()=>{
            var p=await LockPlanAsync(id,user,ct);
            if(r.IsActive.Value)
            {
                var configuration=new UpdateMembershipPlanRequest{BranchId=p.BranchId??Guid.Empty,CityId=p.CityId,Name=p.Name,
                    PlanType=p.PlanCategory,AudienceCategoryId=p.MembershipPlanCategoryId,Mode=p.AccessMode,DurationDays=p.DurationDays,
                    Sessions=p.SessionCount,Price=p.Price,AllowFreeze=p.AllowFreeze,FreezeDays=p.IncludedFreezeDays,FreelanceCoachEmployeeId=p.FreelanceCoachEmployeeId};
                await BranchWriterAsync(user,p.BranchId,ct);await ValidatePlanAsync(configuration,p,ct);
            }
            var old=new{p.IsActive};p.IsActive=r.IsActive.Value;
            access.Audit(user.UserId,p.BranchId,"MembershipPlan.StatusChanged",nameof(MembershipPlan),id,old,new{p.IsActive});
            await db.SaveChangesAsync(ct);return await DetailsAsync(id,ct);
        },ct);
    }
}
