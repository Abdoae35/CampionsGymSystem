using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.CheckIns;
public sealed partial class CheckInService
{
    private static void Pagination(int page,int size)
    {if(page<1 || size is <1 or >100 || (long)(page-1)*size>int.MaxValue)throw new BusinessRuleException("Invalid pagination.");}
    private async Task<CurrentUserContext> ViewAsync(Guid? branch,CancellationToken ct)
    {
        var user=await current.GetAsync(ct);if(user==null || !user.Permissions.Contains(CheckInPermissions.View))throw new UnauthorizedAccessException();
        if(branch!=null)BranchAccessService.EnsureScope(user,branch);return user;
    }
    private IQueryable<CheckIn> HistoryQuery(CurrentUserContext user,Guid? branch)
    {
        var q=db.CheckIns.AsNoTracking();
        if(!user.Permissions.Contains("Branches.AccessAll") || branch!=null)
        {var target=branch??user.CurrentBranchId??throw new UnauthorizedAccessException();q=q.Where(x=>x.BranchId==target);}
        return q;
    }
    private IQueryable<CheckInDetails> Project(IQueryable<CheckIn> q)=>q.Select(x=>new CheckInDetails(x.Id,x.OperationId,x.MemberId,
        x.Member!=null?x.Member.MemberCode:null,x.Member!=null?x.Member.FullName:db.GuestInvitations.Where(g=>g.RedeemedCheckInId==x.Id).Select(g=>g.InviteeName).FirstOrDefault(),
        x.BranchId,x.BusinessDate??(x.BusinessDay!=null?(DateOnly?)x.BusinessDay.BusinessDate:null),x.BusinessDayId,x.CheckedInAt,
        x.CheckInType.ToString(),x.CheckInMethod.ToString(),x.MemberSubscriptionId,x.SessionConsumed,x.SessionsBefore,x.SessionsAfter,x.CreatedByUserId));
    public async Task<CheckInHistory> GetHistoryAsync(CheckInHistoryRequest r,CancellationToken ct=default)
    {
        Pagination(r.Page,r.PageSize);if(r.From>r.To || r.Type.HasValue&&!Enum.IsDefined(r.Type.Value) || r.Method.HasValue&&!Enum.IsDefined(r.Method.Value))
            throw new BusinessRuleException("Invalid history filters.");
        var user=await ViewAsync(r.BranchId,ct);var q=HistoryQuery(user,r.BranchId);
        if(r.MemberId.HasValue)q=q.Where(x=>x.MemberId==r.MemberId);
        if(r.Type.HasValue)q=q.Where(x=>x.CheckInType==r.Type);if(r.Method.HasValue)q=q.Where(x=>x.CheckInMethod==r.Method);
        if(r.From.HasValue)q=q.Where(x=>(x.BusinessDate??(x.BusinessDay!=null?(DateOnly?)x.BusinessDay.BusinessDate:null))>=r.From);
        if(r.To.HasValue)q=q.Where(x=>(x.BusinessDate??(x.BusinessDay!=null?(DateOnly?)x.BusinessDay.BusinessDate:null))<=r.To);
        var count=await q.CountAsync(ct);var rows=await Project(q.OrderByDescending(x=>x.CheckedInAt).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)).ToListAsync(ct);
        return new(rows,count,r.Page,r.PageSize);
    }
    public async Task<CheckInHistory> GetCurrentBusinessDayAsync(Guid memberId,Guid? branchId=null,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        var context=await ContextAsync(CheckInPermissions.View,branchId,ct);var date=await BusinessDateAsync(context.Branch.Id,ct);
        return await GetHistoryAsync(new(){MemberId=memberId,BranchId=context.Branch.Id,From=date.Date,To=date.Date,Page=page,PageSize=pageSize},ct);
    }
    // Existence/identity of entrance only. Future Private Training must still validate its own entitlement and consumption.
    public async Task<CheckInDetails?> GetCurrentGymEntryAsync(Guid memberId,Guid? branchId=null,CancellationToken ct=default)
    {
        var context=await ContextAsync(CheckInPermissions.View,branchId,ct);var date=await BusinessDateAsync(context.Branch.Id,ct);
        return await FindGymEntryAsync(memberId,context.Branch.Id,date.Date,ct);
    }
    // Future BLL consumers must authorize their own workflow/member/branch before calling.
    internal async Task<CheckInDetails?> FindGymEntryAsync(Guid memberId,Guid branchId,DateOnly businessDate,CancellationToken ct)
    {
        var q=db.CheckIns.AsNoTracking().Where(x=>x.BranchId==branchId && x.MemberId==memberId
            && (x.CheckInType==CheckInType.Gym || x.CheckInType==CheckInType.BorrowedGym)
            && (x.BusinessDate??(x.BusinessDay!=null?(DateOnly?)x.BusinessDay.BusinessDate:null))==businessDate);
        return await Project(q.OrderByDescending(x=>x.CheckedInAt).ThenBy(x=>x.Id).Take(1)).SingleOrDefaultAsync(ct);
    }
    public async Task<CheckInMemberMatches> FindMembersAsync(CheckInMemberSearchRequest r,CancellationToken ct=default)
    {
        Pagination(r.Page,r.PageSize);if(string.IsNullOrWhiteSpace(r.Search)||r.Search.Length>256)throw new BusinessRuleException("Enter a MemberCode, phone, name or branch-local ClientCode.");
        var context=await ContextAsync(CheckInPermissions.Create,r.BranchId,ct);
        var q=db.Members.AsNoTracking();
        if(!context.Actor.Permissions.Contains("Branches.AccessAll"))
            q=q.Where(x=>x.HomeBranch.CityId==context.Branch.CityId || x.Subscriptions.Any(s=>s.CityId==context.Branch.CityId));
        q=MemberSearch.Apply(q,r.Search,context.Branch.Id);
        var count=await q.CountAsync(ct);var rows=await q.OrderBy(x=>x.FullName).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)
            .Select(x=>new CheckInMemberMatch(x.Id,x.MemberCode,x.FullName,x.IsActive,x.HomeBranchId,x.HomeBranchId==context.Branch.Id?x.ClientCode:null)).ToListAsync(ct);
        return new(rows,count,r.Page,r.PageSize);
    }
    public async Task<BorrowedDebtHistory> GetBorrowedDebtHistoryAsync(Guid memberId,BorrowedDebtHistoryRequest r,CancellationToken ct=default)
    {
        Pagination(r.Page,r.PageSize);var user=await ViewAsync(r.BranchId,ct);var q=db.BorrowedGymSessions.AsNoTracking().Where(x=>x.MemberId==memberId);
        if(!user.Permissions.Contains("Branches.AccessAll")||r.BranchId!=null)
        {var branch=r.BranchId??user.CurrentBranchId??throw new UnauthorizedAccessException();q=q.Where(x=>x.BranchId==branch);}
        if(r.ExpiredSubscriptionId!=null)q=q.Where(x=>x.ExpiredSubscriptionId==r.ExpiredSubscriptionId);
        var count=await q.CountAsync(ct);var items=await q.OrderByDescending(x=>x.BorrowedAt).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)
            .Select(x=>new BorrowedDebtDetails(x.Id,x.ExpiredSubscriptionId,x.CheckInId,x.BranchId,x.BorrowedAt,x.Status.ToString(),x.SettledBySubscriptionId,x.SettledByTransactionId,x.SettledAt)).ToListAsync(ct);
        return new(items,count,r.Page,r.PageSize);
    }
}
