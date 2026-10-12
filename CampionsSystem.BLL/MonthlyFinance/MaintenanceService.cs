using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.MonthlyFinance;

public sealed class MaintenanceService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private readonly FinancialOperations coordination = new(db,current,access);
    internal static MaintenanceForecast Forecast(MaintenancePlanRevision p, DateOnly asOf)
    {
        if(asOf.Year>9900)throw new BusinessRuleException("Maintenance forecast date must be before year 9901.");
        var elapsed = (asOf.Year-p.FirstDueDate.Year)*12+asOf.Month-p.FirstDueDate.Month;
        var cycles = Math.Max(0,elapsed/p.FrequencyMonths);
        var next = p.FirstDueDate.AddMonths(cycles*p.FrequencyMonths);
        if(next<asOf)next=p.FirstDueDate.AddMonths((cycles+1)*p.FrequencyMonths);
        return new(p.Id,p.PlanId,p.BranchId,p.Revision,p.Name,p.ExpectedCost,p.FrequencyMonths,p.FirstDueDate,next,
            decimal.Round(p.ExpectedCost/p.FrequencyMonths,2,MidpointRounding.AwayFromZero),p.IsActive);
    }
    public Task<MaintenanceForecast> ConfigureAsync(MaintenancePlanRequest r,CancellationToken ct=default)=>coordination.TransactionAsync(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Money(r.ExpectedCost,true);FinancialOperations.Reason(r.Reason);
        if(string.IsNullOrWhiteSpace(r.Name)||r.Name.Length>256||r.FrequencyMonths is <1 or >120||r.FirstDueDate==default||r.FirstDueDate.Year>9900)
            throw new BusinessRuleException("Name, positive expected cost, frequency 1..120 months and valid first expected date are required.");
        var actor=await access.RequireAsync(MonthlyFinancePermissions.MaintenanceManage,r.BranchId,ct);await access.RequireAsync("Branches.AccessAll",r.BranchId,ct);
        await coordination.LockBranchAsync(r.BranchId,ct);var hash=FinancialOperations.Hash(r);
        var prior=await db.MaintenancePlanRevisions.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null){FinancialOperations.Retry(prior.RequestHash,hash,prior.ActorUserId,actor);return Forecast(prior,access.Today);}
        MaintenancePlanRevision? last=null;
        if(r.PlanId!=null)
        {
            last=await db.MaintenancePlanRevisions.AsNoTracking().Where(x=>x.PlanId==r.PlanId).OrderByDescending(x=>x.Revision).FirstOrDefaultAsync(ct)
                ??throw new BranchNotFoundException("Maintenance plan not found.");
            if(last.BranchId!=r.BranchId)throw new UnauthorizedAccessException();
        }
        if(r.ExpectedRevision!=(last?.Revision??0))throw new DbUpdateConcurrencyException();
        var row=new MaintenancePlanRevision{PlanId=last?.PlanId??Guid.NewGuid(),BranchId=r.BranchId,Revision=(last?.Revision??0)+1,Name=r.Name.Trim(),
            ExpectedCost=r.ExpectedCost,FrequencyMonths=r.FrequencyMonths,FirstDueDate=r.FirstDueDate,IsActive=r.IsActive,OperationId=r.OperationId,RequestHash=hash,ActorUserId=actor,RecordedAt=access.Now};
        db.Add(row);access.Audit(actor,r.BranchId,"Maintenance.PlanConfigured",nameof(MaintenancePlanRevision),row.Id,after:new{row.PlanId,row.Revision,r.ExpectedCost,r.FrequencyMonths,r.FirstDueDate,r.IsActive,r.Reason,InformationalOnly=true});
        return Forecast(row,access.Today);
    },ct);
    public async Task<ShiftPage<MaintenanceForecast>> PlansAsync(Guid? branchId=null,DateOnly? asOf=null,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        MonthlyFinanceService.Page(page,pageSize);var(branch,_)=await coordination.ResolveAsync(MonthlyFinancePermissions.MaintenanceView,branchId,ct);
        var q=db.MaintenancePlanRevisions.AsNoTracking().Where(x=>x.BranchId==branch&&!db.MaintenancePlanRevisions.Any(y=>y.PlanId==x.PlanId&&y.Revision>x.Revision));
        var total=await q.CountAsync(ct);var rows=await q.OrderBy(x=>x.Name).ThenBy(x=>x.PlanId).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(x=>Forecast(x,asOf??access.Today)).ToArray(),total,page,pageSize);
    }
    public async Task<ShiftPage<MaintenanceForecast>> HistoryAsync(Guid planId,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        MonthlyFinanceService.Page(page,pageSize);var branch=await db.MaintenancePlanRevisions.Where(x=>x.PlanId==planId).Select(x=>(Guid?)x.BranchId).FirstOrDefaultAsync(ct)
            ??throw new BranchNotFoundException("Maintenance plan not found.");
        await access.RequireAsync(MonthlyFinancePermissions.MaintenanceView,branch,ct);var q=db.MaintenancePlanRevisions.AsNoTracking().Where(x=>x.PlanId==planId);
        var total=await q.CountAsync(ct);var rows=await q.OrderBy(x=>x.Revision).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(x=>Forecast(x,access.Today)).ToArray(),total,page,pageSize);
    }
}
