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
        ValidateDate(asOf);
        // The due date is an expectation, not evidence of payment. It remains overdue
        // until an Owner explicitly advances it in a new plan revision.
        var next = p.NextExpectedDueDate ?? p.FirstDueDate;
        var days = next.DayNumber-asOf.DayNumber;
        var dueStatus = !p.IsActive ? "Inactive" : days<0 ? "Overdue" : days==0 ? "DueToday" : "Upcoming";
        return new(p.Id,p.PlanId,p.BranchId,p.Revision,p.Name,p.ExpectedCost,p.FrequencyMonths,p.FirstDueDate,next,
            decimal.Round(p.ExpectedCost/p.FrequencyMonths,2,MidpointRounding.AwayFromZero),p.IsActive,
            Description:p.Description,DueStatus:dueStatus,DaysUntilDue:days);
    }
    private static void ValidateDate(DateOnly date)
    {
        if(date==default||date.Year>9900)throw new BusinessRuleException("Maintenance date must be between year 1 and 9900.");
    }
    public Task<MaintenanceForecast> ConfigureAsync(MaintenancePlanRequest r,CancellationToken ct=default)=>coordination.TransactionAsync(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Money(r.ExpectedCost,true);FinancialOperations.Reason(r.Reason);
        if(string.IsNullOrWhiteSpace(r.Name)||r.Name.Length>256||r.Description?.Length>2000||r.FrequencyMonths is <1 or >120)
            throw new BusinessRuleException("Name, positive expected cost, frequency 1..120 months and valid first expected date are required.");
        ValidateDate(r.FirstDueDate);
        if(r.NextExpectedDueDate is { } due)
        {
            ValidateDate(due);
            if(due<r.FirstDueDate)throw new BusinessRuleException("Next expected due date cannot precede the first due date.");
        }
        var actor=await access.RequireAsync(MonthlyFinancePermissions.MaintenanceManage,r.BranchId,ct);await access.RequireAsync("Branches.AccessAll",r.BranchId,ct);
        if(!await new OwnerAdministrationAccess(db,current).IsOwnerAsync(actor,ct))throw new UnauthorizedAccessException();
        // Plans have no drawer movement. Serialize revisions without requiring a
        // configured cash drawer or locking financial postings.
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"maintenance-plan:" + r.BranchId}, 0))",ct);
        var branch=await db.GymBranches.AsNoTracking().Where(x=>x.Id==r.BranchId).Select(x=>new{x.IsActive}).SingleOrDefaultAsync(ct);
        if(branch is null)throw new BranchNotFoundException("Branch not found.");
        if(!branch.IsActive)throw new BusinessRuleException("Branch is inactive.");
        // Preserve hashes of pre-extension requests so historical OperationIds
        // remain retryable when the two new optional fields are omitted.
        var hash=r.Description is null&&r.NextExpectedDueDate is null
            ?FinancialOperations.Hash(new{r.OperationId,r.BranchId,r.Name,r.ExpectedCost,r.FrequencyMonths,
                r.FirstDueDate,r.Reason,r.PlanId,r.ExpectedRevision,r.IsActive})
            :FinancialOperations.Hash(r);
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
        var nextDue=r.NextExpectedDueDate??(last?.FirstDueDate==r.FirstDueDate?last?.NextExpectedDueDate:null)??r.FirstDueDate;
        var row=new MaintenancePlanRevision{PlanId=last?.PlanId??Guid.NewGuid(),BranchId=r.BranchId,Revision=(last?.Revision??0)+1,Name=r.Name.Trim(),
            Description=r.Description?.Trim(),ExpectedCost=r.ExpectedCost,FrequencyMonths=r.FrequencyMonths,FirstDueDate=r.FirstDueDate,
            NextExpectedDueDate=nextDue,IsActive=r.IsActive,OperationId=r.OperationId,RequestHash=hash,ActorUserId=actor,RecordedAt=access.Now};
        db.Add(row);access.Audit(actor,r.BranchId,"Maintenance.PlanConfigured",nameof(MaintenancePlanRevision),row.Id,
            before:last==null?null:new{last.Revision,last.Name,last.Description,last.ExpectedCost,last.FrequencyMonths,last.FirstDueDate,last.NextExpectedDueDate,last.IsActive},
            after:new{row.PlanId,row.Revision,row.Name,row.Description,row.ExpectedCost,row.FrequencyMonths,row.FirstDueDate,row.NextExpectedDueDate,row.IsActive,r.Reason,InformationalOnly=true});
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
    private IQueryable<MaintenancePlanRevision> Latest(Guid branch) =>
        db.MaintenancePlanRevisions.AsNoTracking().Where(x=>x.BranchId==branch&&x.IsActive&&
            !db.MaintenancePlanRevisions.Any(y=>y.PlanId==x.PlanId&&y.Revision>x.Revision));

    public async Task<MaintenanceBranchSummary> SummaryAsync(Guid? branchId=null,DateOnly? asOf=null,int horizonDays=90,CancellationToken ct=default)
    {
        if(horizonDays is <1 or >366)throw new BusinessRuleException("Reminder horizon must be 1..366 days.");
        var day=asOf??access.Today;ValidateDate(day);
        var(branch,_)=await coordination.ResolveAsync(MonthlyFinancePermissions.MaintenanceView,branchId,ct);
        var plans=await Latest(branch).Select(x=>new{x.ExpectedCost,x.FrequencyMonths,x.FirstDueDate,x.NextExpectedDueDate}).ToListAsync(ct);
        var due=plans.Select(x=>x.NextExpectedDueDate??x.FirstDueDate).ToArray();
        return new(branch,day,horizonDays,plans.Sum(x=>decimal.Round(x.ExpectedCost/x.FrequencyMonths,2,MidpointRounding.AwayFromZero)),
            plans.Count,due.Count(x=>x>day&&x.DayNumber-day.DayNumber<=horizonDays),due.Count(x=>x==day),due.Count(x=>x<day));
    }

    public async Task<ShiftPage<MaintenanceForecast>> RemindersAsync(Guid? branchId=null,DateOnly? asOf=null,int horizonDays=90,
        int page=1,int pageSize=25,CancellationToken ct=default)
    {
        MonthlyFinanceService.Page(page,pageSize);
        if(horizonDays is <1 or >366)throw new BusinessRuleException("Reminder horizon must be 1..366 days.");
        var day=asOf??access.Today;ValidateDate(day);
        var(branch,_)=await coordination.ResolveAsync(MonthlyFinancePermissions.MaintenanceView,branchId,ct);
        var until=day.AddDays(horizonDays);
        var q=Latest(branch).Where(x=>(x.NextExpectedDueDate??x.FirstDueDate)<=until);
        var total=await q.CountAsync(ct);
        var rows=await q.OrderBy(x=>x.NextExpectedDueDate??x.FirstDueDate).ThenBy(x=>x.PlanId)
            .Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(x=>Forecast(x,day)).ToArray(),total,page,pageSize);
    }
}
