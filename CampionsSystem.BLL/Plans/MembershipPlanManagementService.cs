using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Plans;

public sealed partial class MembershipPlanManagementService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private async Task<CurrentUserContext> RequireAsync(string permission,CancellationToken ct)
    {
        var user=await current.GetAsync(ct);
        if(user is null||!user.Permissions.Contains(permission))throw new UnauthorizedAccessException();
        return user;
    }
    private async Task<CurrentUserContext> CatalogWriterAsync(CancellationToken ct)
    {
        var user=await RequireAsync("MembershipPlans.Manage",ct);
        if(!user.Permissions.Contains("Branches.AccessAll"))throw new UnauthorizedAccessException();
        return user;
    }
    private static void Page(CatalogListRequest r)
    {
        if(r.Page<1||r.PageSize is <1 or >100||(long)(r.Page-1)*r.PageSize>int.MaxValue||r.Search?.Length>256)
            throw new BusinessRuleException("Invalid search or pagination.");
    }
    private static void CatalogProfile(string name,string? description)
    {
        if(string.IsNullOrWhiteSpace(name)||name.Length>256||description?.Length>4096)throw new BusinessRuleException("Invalid catalog name or description.");
    }
    private async Task<T> TransactionAsync<T>(Func<Task<T>> change,CancellationToken ct)
    {
        var owned=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        var tx=db.Database.CurrentTransaction!;var point="plan_"+Guid.NewGuid().ToString("N");
        try{await tx.CreateSavepointAsync(point,ct);var result=await change();await tx.ReleaseSavepointAsync(point,ct);if(owned is not null)await owned.CommitAsync(ct);return result;}
        catch{if(owned is not null)await owned.RollbackAsync(CancellationToken.None);else await tx.RollbackToSavepointAsync(point,CancellationToken.None);db.ChangeTracker.Clear();throw;}
        finally{if(owned is not null)await owned.DisposeAsync();}
    }
    private static CatalogDetails CategoryDetails(MembershipPlanCategory c)=>new(c.Id,null,c.Name,c.Description,c.IsActive,c.CreatedAt);
    public async Task<CatalogListResult> GetCategoriesAsync(CatalogListRequest r,CancellationToken ct=default)
    {
        await RequireAsync("MembershipPlans.View",ct);Page(r);var q=db.MembershipPlanCategories.AsNoTracking();
        if(r.IsActive.HasValue)q=q.Where(c=>c.IsActive==r.IsActive);
        if(!string.IsNullOrWhiteSpace(r.Search)){var s=r.Search.Trim().ToLowerInvariant();q=q.Where(c=>c.Name.ToLower().Contains(s));}
        return new(await q.OrderBy(c=>c.Name).ThenBy(c=>c.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)
            .Select(c=>new CatalogDetails(c.Id,null,c.Name,c.Description,c.IsActive,c.CreatedAt)).ToListAsync(ct),await q.CountAsync(ct),r.Page,r.PageSize);
    }
    public async Task<CatalogDetails> CreateCategoryAsync(SaveCategoryRequest r,CancellationToken ct=default)
    {
        var user=await CatalogWriterAsync(ct);CatalogProfile(r.Name,r.Description);
        return await TransactionAsync(async()=>{
            var name=r.Name.Trim();if(await db.MembershipPlanCategories.AnyAsync(c=>c.Name==name,ct))throw new BusinessRuleException("Category name already exists.");
            var c=new MembershipPlanCategory{Name=name,Description=r.Description?.Trim(),CreatedAt=access.Now};db.Add(c);
            access.Audit(user.UserId,null,"MembershipPlanCategory.Created",nameof(MembershipPlanCategory),c.Id,after:new{c.Name,c.Description});
            await db.SaveChangesAsync(ct);return CategoryDetails(c);
        },ct);
    }
    private async Task<MembershipPlanCategory> LockCategoryAsync(Guid id,CancellationToken ct)
    {
        var c=await db.MembershipPlanCategories.FromSqlInterpolated($"SELECT * FROM \"MembershipPlanCategories\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)
            ??throw new BranchNotFoundException("Category not found.");await db.Entry(c).ReloadAsync(ct);return c;
    }
    public async Task<CatalogDetails> UpdateCategoryAsync(Guid id,SaveCategoryRequest r,CancellationToken ct=default)
    {
        var user=await CatalogWriterAsync(ct);CatalogProfile(r.Name,r.Description);
        return await TransactionAsync(async()=>{
            var c=await LockCategoryAsync(id,ct);var name=r.Name.Trim();
            if(await db.MembershipPlanCategories.AnyAsync(x=>x.Id!=id&&x.Name==name,ct))throw new BusinessRuleException("Category name already exists.");
            var old=new{c.Name,c.Description};c.Name=name;c.Description=r.Description?.Trim();
            access.Audit(user.UserId,null,"MembershipPlanCategory.Updated",nameof(MembershipPlanCategory),id,old,new{c.Name,c.Description});
            await db.SaveChangesAsync(ct);return CategoryDetails(c);
        },ct);
    }
    public async Task<CatalogDetails> ChangeCategoryStatusAsync(Guid id,ChangePlanStatusRequest r,CancellationToken ct=default)
    {
        var user=await CatalogWriterAsync(ct);if(r.IsActive is null)throw new BusinessRuleException("Status is required.");
        return await TransactionAsync(async()=>{var c=await LockCategoryAsync(id,ct);var old=new{c.IsActive};c.IsActive=r.IsActive.Value;
            access.Audit(user.UserId,null,"MembershipPlanCategory.StatusChanged",nameof(MembershipPlanCategory),id,old,new{c.IsActive});
            await db.SaveChangesAsync(ct);return CategoryDetails(c);},ct);
    }
}
