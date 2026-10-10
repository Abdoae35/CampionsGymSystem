using CampionsSystem.BLL.Operations;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
public sealed partial class OfferService
{
    public async Task<OfferHistory> GetManagementHistoryAsync(Guid? branchId=null,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        if(page is <1 or >1000000 || pageSize is <1 or >100)throw new BusinessRuleException("Invalid Offer pagination.");
        var actor=await current.GetAsync(ct)??throw new UnauthorizedAccessException();var branch=branchId??(actor.Permissions.Contains("Branches.AccessAll")?null:actor.CurrentBranchId);
        await RequireAsync(SalesPermissions.Offers,branch,ct);var q=db.Offers.AsNoTracking();if(branch!=null)q=q.Where(o=>o.BranchId==branch);
        var count=await q.CountAsync(ct);var offers=await q.OrderByDescending(o=>o.CreatedAt).ThenBy(o=>o.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        var rows=new List<OfferDetails>();foreach(var o in offers)rows.Add(await DetailsAsync(o,ct));return new(rows,count,page,pageSize);
    }
}
