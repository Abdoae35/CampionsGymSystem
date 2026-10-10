using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.CheckIns;

public sealed partial class CheckInService(ApplicationDbContext db, ICurrentUserService current,
    OperationAccess access, SubscriptionService subscriptions)
{
    private async Task<(CurrentUserContext Actor,GymBranch Branch)> ContextAsync(string permission,Guid? selected,CancellationToken ct)
    {
        var actor=await current.GetAsync(ct);
        if(actor is null || !actor.Permissions.Contains(permission))throw new UnauthorizedAccessException();
        var branchId=selected??actor.CurrentBranchId??throw new BusinessRuleException("Select an operational branch.");
        BranchAccessService.EnsureScope(actor,branchId);
        var branch=await db.GymBranches.AsNoTracking().Include(x=>x.City).SingleOrDefaultAsync(x=>x.Id==branchId,ct)
            ?? throw new BranchNotFoundException("Visited branch not found.");
        return(actor,branch);
    }
    private async Task<(DateOnly Date,Guid? DayId)> BusinessDateAsync(Guid branch,CancellationToken ct)
    {
        var day=await db.BusinessDays.AsNoTracking().SingleOrDefaultAsync(x=>x.BranchId==branch && x.Status==BusinessDayStatus.Open,ct);
        if(day!=null && (day.OpenedAt>access.Now || day.BusinessDate>access.Today))throw new BusinessRuleException("Invalid open BusinessDay.");
        return(day?.BusinessDate??access.Today,day?.Id);
    }
    private async Task<Member?> IdentifyAsync(Guid? id,string? code,CancellationToken ct)
    {
        if(id==Guid.Empty || id.HasValue==!string.IsNullOrWhiteSpace(code) || code?.Length>80)
            throw new BusinessRuleException("Supply exactly one MemberId or global MemberCode.");
        var value=code?.Trim();
        return await db.Members.AsNoTracking().SingleOrDefaultAsync(x=>id.HasValue?x.Id==id:x.MemberCode==value,ct);
    }
    public async Task<AccessDecision> EvaluateAsync(AccessRequest r,CancellationToken ct=default)
    {
        var context=await ContextAsync(CheckInPermissions.Create,r.BranchId,ct);
        var date=await BusinessDateAsync(context.Branch.Id,ct);
        return await DecideAsync(await IdentifyAsync(r.MemberId,r.MemberCode,ct),context.Branch,date.Date,date.DayId,ct);
    }
    private async Task<AccessDecision> DecideAsync(Member? member,GymBranch branch,DateOnly date,Guid? day,CancellationToken ct)
    {
        AccessDecision Result(string reason,SubscriptionSummary? s=null,bool borrow=false,int used=0)
            => new(reason==AccessReasons.Allowed,reason,member?.Id,member?.MemberCode,member?.FullName,branch.Id,date,day,
                s?.Id,s?.Mode,s?.RemainingSessions,borrow,used,borrow?Math.Max(0,2-used):0);
        if(member==null)return Result(AccessReasons.MemberNotFound);
        if(!member.IsActive)return Result(AccessReasons.MemberInactive);
        if(!branch.IsActive || !branch.City.IsActive)return Result(AccessReasons.BranchInactive);
        if(await db.BranchClosures.AnyAsync(x=>x.BranchId==branch.Id && x.AppliesToGym && x.StartDate<=date && x.EndDate>=date,ct))
            return Result(AccessReasons.BranchClosed);
        var facts=await subscriptions.GetGymAccessFactsAsync(member.Id,date,ct);
        if(facts.Count>100)return Result(AccessReasons.AmbiguousEntitlement);
        var currentPeriods=facts.Where(x=>x.StartDate<=date && (x.EndDate==null || x.EndDate>=date)
            && x.Status is not ("Cancelled" or "SuspendedForInstallment") && (x.EmployeeId==null || x.EndDate==null)).ToArray();
        if(currentPeriods.Length>1)
        {
            var usable=currentPeriods.Where(x=>!(x.Status=="Expired" && x.Mode==nameof(AccessMode.SessionBased) && x.RemainingSessions<=0)).ToArray();
            currentPeriods=usable.Length>0?usable:[currentPeriods.OrderByDescending(x=>x.StartDate).First()];
        }
        if(currentPeriods.Length>1)return Result(AccessReasons.AmbiguousEntitlement);
        if(currentPeriods.Length==1)
        {
            var s=currentPeriods[0];
            if(s.EmployeeId!=null && s.Status=="Expired")return Result(AccessReasons.EmployeeEntitlementInactive,s);
            if(s.AccessCityId!=branch.CityId)return Result(AccessReasons.AccessCityMismatch,s);
            if(s.Status=="Frozen")return Result(AccessReasons.SubscriptionFrozen,s);
            if(s.Mode==nameof(AccessMode.SessionBased) && s.RemainingSessions<=0)
            {
                if(await subscriptions.PendingFirstEntrySaleAsync(member.Id,branch.CityId,date,ct) is Guid pending)
                    return await subscriptions.IsPaidSaleOverdueAsync(pending,date,ct) ? Result(AccessReasons.InstallmentOverdue)
                        : Result(AccessReasons.PendingFirstCheckIn) with{CanActivateOnCheckIn=true};
                return Result(AccessReasons.NoSessionsRemaining,s);
            }
            return Result(s.Status=="Active"?AccessReasons.Allowed:AccessReasons.NoGymEntitlement,s);
        }
        if (await subscriptions.PendingFirstEntrySaleAsync(member.Id,branch.CityId,date,ct) is Guid pendingSale)
            return await subscriptions.IsPaidSaleOverdueAsync(pendingSale,date,ct) ? Result(AccessReasons.InstallmentOverdue)
                : Result(AccessReasons.PendingFirstCheckIn) with { CanActivateOnCheckIn = true };
        var expired=facts.Where(x=>x.StartDate<=date && x.EndDate<date).ToArray();
        if(expired.Length>1)return Result(AccessReasons.AmbiguousEntitlement);
        if(expired.Length==1)
        {
            var s=expired[0];
            if(s.EmployeeId!=null)return Result(AccessReasons.EmployeeEntitlementInactive,s);
            if(s.Status=="Cancelled")return Result(AccessReasons.SubscriptionCancelled,s);
            if(s.Status=="SuspendedForInstallment")return Result(await subscriptions.IsSubscriptionOverdueAsync(s.Id,date,ct)?AccessReasons.InstallmentOverdue:AccessReasons.SubscriptionSuspended,s);
            if(s.AccessCityId!=branch.CityId)return Result(AccessReasons.AccessCityMismatch,s);
            if(await db.SubscriptionFreezes.AnyAsync(x=>x.MemberSubscriptionId==s.Id && x.FreezeType==FreezeType.Normal && x.StartDate<=date && (x.EndDate==null || x.EndDate>=date),ct)
                || await db.MemberSubscriptions.AnyAsync(x=>x.Id==s.Id && x.Status==SubscriptionStatus.Frozen,ct))
                return Result(AccessReasons.SubscriptionFrozen,s);
            var used=await db.BorrowedGymSessions.CountAsync(x=>x.ExpiredSubscriptionId==s.Id,ct);
            var renewed=await db.MembershipSubscriptionTransactions.AnyAsync(x=>x.RenewedFromSubscriptionId==s.Id && x.MemberSubscriptionId!=s.Id
                    && !db.SubscriptionGrantCancellations.Any(c=>c.GrantId==x.Id),ct)
                || await db.BorrowedGymSessions.AnyAsync(x=>x.ExpiredSubscriptionId==s.Id
                    && !(db.BorrowedDebtSettlementEvents.Where(e=>e.BorrowedGymSessionId==x.Id).OrderByDescending(e=>e.Sequence)
                        .Select(e=>(bool?)e.IsReversal).FirstOrDefault()??x.Status==BorrowedSessionStatus.Outstanding),ct);
            if(renewed)return Result(AccessReasons.BorrowCycleRenewed,s,false,used);
            return Result(used>=2?AccessReasons.BorrowLimitReached:AccessReasons.SubscriptionExpired,s,used<2,used);
        }
        var stopped=facts.FirstOrDefault(x=>x.StartDate<=date && x.Status is "Cancelled" or "SuspendedForInstallment");
        if(stopped!=null)return Result(stopped.Status=="Cancelled"?AccessReasons.SubscriptionCancelled
            :await subscriptions.IsSubscriptionOverdueAsync(stopped.Id,date,ct)?AccessReasons.InstallmentOverdue:AccessReasons.SubscriptionSuspended,stopped);
        var future=facts.Where(x=>x.StartDate>date).OrderBy(x=>x.StartDate).FirstOrDefault();
        return Result(future!=null?AccessReasons.SubscriptionNotStarted:AccessReasons.NoGymEntitlement,future);
    }
}
