using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Plans;
public sealed partial class MembershipPlanManagementService
{
    private static CatalogDetails BenefitDetails(MembershipBenefit b)=>new(b.Id,b.Code,b.Name,b.Description,b.IsActive,b.CreatedAt);
    public async Task<CatalogListResult> GetBenefitsAsync(CatalogListRequest r,CancellationToken ct=default)
    {
        await RequireAsync("MembershipPlans.View",ct);Page(r);var q=db.MembershipBenefits.AsNoTracking();
        if(r.IsActive.HasValue)q=q.Where(b=>b.IsActive==r.IsActive);
        if(!string.IsNullOrWhiteSpace(r.Search)){var s=r.Search.Trim().ToLowerInvariant();q=q.Where(b=>b.Name.ToLower().Contains(s)||b.Code.ToLower().Contains(s));}
        return new(await q.OrderBy(b=>b.Name).ThenBy(b=>b.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)
            .Select(b=>new CatalogDetails(b.Id,b.Code,b.Name,b.Description,b.IsActive,b.CreatedAt)).ToListAsync(ct),await q.CountAsync(ct),r.Page,r.PageSize);
    }
    public async Task<CatalogDetails> GetBenefitByIdAsync(Guid id,CancellationToken ct=default)
    {
        await RequireAsync("MembershipPlans.View",ct);
        var b=await db.MembershipBenefits.AsNoTracking().SingleOrDefaultAsync(b=>b.Id==id,ct)??throw new BranchNotFoundException("Benefit not found.");return BenefitDetails(b);
    }
    private async Task<MembershipBenefit> LockBenefitAsync(Guid id,CancellationToken ct)
    {
        var b=await db.MembershipBenefits.FromSqlInterpolated($"SELECT * FROM \"MembershipBenefits\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)
            ??throw new BranchNotFoundException("Benefit not found.");await db.Entry(b).ReloadAsync(ct);return b;
    }
    private async Task ValidateBenefitAsync(SaveBenefitRequest r,Guid? except,CancellationToken ct)
    {
        CatalogProfile(r.Name,r.Description);
        if(string.IsNullOrWhiteSpace(r.Code)||r.Code.Length>128)throw new BusinessRuleException("Benefit code is required, up to 128 characters.");
        if(await db.MembershipBenefits.AnyAsync(b=>b.Id!=except&&(b.Code==r.Code.Trim()||b.Name==r.Name.Trim()),ct))
            throw new BusinessRuleException("Benefit code or name already exists.");
    }
    public async Task<CatalogDetails> CreateBenefitAsync(SaveBenefitRequest r,CancellationToken ct=default)
    {
        var user=await CatalogWriterAsync(ct);
        return await TransactionAsync(async()=>{await ValidateBenefitAsync(r,null,ct);var b=new MembershipBenefit{Code=r.Code.Trim(),Name=r.Name.Trim(),Description=r.Description?.Trim(),CreatedAt=access.Now};db.Add(b);
            access.Audit(user.UserId,null,"MembershipBenefit.Created",nameof(MembershipBenefit),b.Id,after:new{b.Code,b.Name,b.Description});
            await db.SaveChangesAsync(ct);return BenefitDetails(b);},ct);
    }
    public async Task<CatalogDetails> UpdateBenefitAsync(Guid id,SaveBenefitRequest r,CancellationToken ct=default)
    {
        var user=await CatalogWriterAsync(ct);
        return await TransactionAsync(async()=>{var b=await LockBenefitAsync(id,ct);await ValidateBenefitAsync(r,id,ct);
            var old=new{b.Code,b.Name,b.Description};b.Code=r.Code.Trim();b.Name=r.Name.Trim();b.Description=r.Description?.Trim();
            access.Audit(user.UserId,null,"MembershipBenefit.Updated",nameof(MembershipBenefit),id,old,new{b.Code,b.Name,b.Description});
            await db.SaveChangesAsync(ct);return BenefitDetails(b);},ct);
    }
    public async Task<CatalogDetails> ChangeBenefitStatusAsync(Guid id,ChangePlanStatusRequest r,CancellationToken ct=default)
    {
        var user=await CatalogWriterAsync(ct);if(r.IsActive is null)throw new BusinessRuleException("Status is required.");
        return await TransactionAsync(async()=>{var b=await LockBenefitAsync(id,ct);var old=new{b.IsActive};b.IsActive=r.IsActive.Value;
            access.Audit(user.UserId,null,"MembershipBenefit.StatusChanged",nameof(MembershipBenefit),id,old,new{b.IsActive});
            await db.SaveChangesAsync(ct);return BenefitDetails(b);},ct);
    }
}
