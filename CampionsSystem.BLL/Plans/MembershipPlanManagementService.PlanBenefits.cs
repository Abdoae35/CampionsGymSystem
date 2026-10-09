using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Plans;
public sealed partial class MembershipPlanManagementService
{
    public async Task<IReadOnlyList<PlanBenefitDetails>> GetPlanBenefitsAsync(Guid id,CancellationToken ct=default)
    {
        await GetPlanByIdAsync(id,ct);return await BenefitsAsync(id,ct);
    }
    public async Task<IReadOnlyList<PlanBenefitDetails>> SetPlanBenefitsAsync(Guid id,SetPlanBenefitsRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("MembershipPlans.Manage",ct);
        if(r.Benefits is null||r.Benefits.Count>100||r.Benefits.Any(b=>b is null)||r.Benefits.Select(b=>b.BenefitId).Distinct().Count()!=r.Benefits.Count)
            throw new BusinessRuleException("Supply at most 100 distinct benefits.");
        foreach(var b in r.Benefits)
            if(b.BenefitId==Guid.Empty||!(b.GrantType==MembershipBenefitGrantType.Quantity&&b.Quantity>0||b.GrantType==MembershipBenefitGrantType.Access&&b.Quantity is null))
                throw new BusinessRuleException("Quantity grants need positive quantity; Access grants need null quantity.");
        return await TransactionAsync<IReadOnlyList<PlanBenefitDetails>>(async()=>{
            var plan=await LockPlanAsync(id,user,ct);
            var existing=await db.MembershipPlanBenefits.Where(b=>b.MembershipPlanId==id).ToListAsync(ct);
            // Lock catalog entries in a stable order; additions and catalog deactivation cannot race.
            foreach(var b in r.Benefits.OrderBy(b=>b.BenefitId))
            {
                var catalog=await db.MembershipBenefits.FromSqlInterpolated($"SELECT * FROM \"MembershipBenefits\" WHERE \"Id\"={b.BenefitId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
                var old=existing.SingleOrDefault(x=>x.MembershipBenefitId==b.BenefitId);
                if(catalog is null||!catalog.IsActive&&(old is null||old.GrantType!=b.GrantType||old.Quantity!=b.Quantity))
                    throw new BusinessRuleException("Select active benefits; unchanged inactive configurations may be retained or removed.");
            }
            foreach(var old in existing.Where(x=>!r.Benefits.Any(b=>b.BenefitId==x.MembershipBenefitId)))
            {
                db.Remove(old);access.Audit(user.UserId,plan.BranchId,"MembershipPlan.BenefitRemoved",nameof(MembershipPlanBenefit),old.Id,
                    new{old.MembershipPlanId,old.MembershipBenefitId,old.GrantType,old.Quantity});
            }
            foreach(var b in r.Benefits)
            {
                var row=existing.SingleOrDefault(x=>x.MembershipBenefitId==b.BenefitId);var before=row is null?null:new{row.MembershipPlanId,row.MembershipBenefitId,row.GrantType,row.Quantity};
                if(row is not null&&row.GrantType==b.GrantType&&row.Quantity==b.Quantity)continue;
                var added=row is null;
                if(added){row=new(){MembershipPlanId=id,MembershipBenefitId=b.BenefitId,CreatedAt=access.Now};db.Add(row);}
                row!.GrantType=b.GrantType!.Value;row.Quantity=b.Quantity;
                access.Audit(user.UserId,plan.BranchId,added?"MembershipPlan.BenefitAdded":"MembershipPlan.BenefitUpdated",nameof(MembershipPlanBenefit),row.Id,before,
                    new{row.MembershipPlanId,row.MembershipBenefitId,row.GrantType,row.Quantity});
            }
            await db.SaveChangesAsync(ct);return await BenefitsAsync(id,ct);
        },ct);
    }
}
