using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Crm;
public sealed partial class LeadManagementService
{
    private IQueryable<MemberSalesAssignment> MemberOwners()=>db.MemberSalesAssignments.AsNoTracking().Where(a=>a.EndedAt==null&&a.AssignmentType==AssignmentType.Sales);
    private async Task<Member> LockMemberAsync(Guid id,CancellationToken ct)
    {
        var m=await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)
            ??throw new BranchNotFoundException("Member not found.");
        await db.Entry(m).ReloadAsync(ct);return m;
    }
    private async Task MemberWorkAccessAsync(Member m,CurrentUserContext user,CancellationToken ct)
    {
        if(user.EmployeeId.HasValue&&await MemberOwners().AnyAsync(a=>a.MemberId==m.Id&&a.SalesEmployeeId==user.EmployeeId,ct))return;
        if(!user.Permissions.Contains("Members.AssignSales"))throw new UnauthorizedAccessException();
        BranchAccessService.EnsureScope(user,m.HomeBranchId);
    }
    public async Task<FollowUpListResult> GetMyFollowUpsAsync(FollowUpListRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("CRM.ViewMemberFollowUps",ct);if(user.EmployeeId is null)throw new UnauthorizedAccessException();
        var memberTasks=db.MemberFollowUpTasks.AsNoTracking().Where(t=>t.CompletedAt==null&&MemberOwners().Any(a=>a.MemberId==t.MemberId&&a.SalesEmployeeId==user.EmployeeId))
            .Select(t=>new FollowUpRow{Id=t.Id,SubjectType="Member",LeadId=null,MemberId=t.MemberId,Name=t.Member.FullName,Phone=t.Member.PrimaryPhone,
                BranchId=t.Member.HomeBranchId,BranchName=t.Member.HomeBranch.Name,Reason=t.Reason,DueAt=t.DueAt,
                CrossBranch=user.CurrentBranchId.HasValue&&t.Member.HomeBranchId!=user.CurrentBranchId});
        var leads=db.Leads.AsNoTracking().Where(l=>user.Permissions.Contains("Leads.View")&&l.NextFollowUpAt!=null&&l.Status!=LeadStatus.Lost&&l.Status!=LeadStatus.Converted&&l.Status!=LeadStatus.Inactive
            &&Assignments().Any(a=>a.LeadId==l.Id&&a.SalesEmployeeId==user.EmployeeId))
            .Select(l=>new FollowUpRow{Id=l.Id,SubjectType="Lead",LeadId=l.Id,MemberId=null,Name=l.FullName??l.Phone,Phone=l.Phone,BranchId=l.BranchId,
                BranchName=l.Branch.Name,Reason="LeadFollowUp",DueAt=l.NextFollowUpAt!.Value,CrossBranch=user.CurrentBranchId.HasValue&&l.BranchId!=user.CurrentBranchId});
        return await WorkListAsync(memberTasks.Concat(leads),r,ct);
    }
    public async Task<FollowUpListResult> GetHouseFollowUpsAsync(FollowUpListRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("CRM.ManageHouseFollowUps",ct);
        if(r.BranchId.HasValue)BranchAccessService.EnsureScope(user,r.BranchId);
        var q=db.MemberFollowUpTasks.AsNoTracking().Where(t=>t.CompletedAt==null&&!MemberOwners().Any(a=>a.MemberId==t.MemberId));
        if(!user.Permissions.Contains("Branches.AccessAll"))q=q.Where(t=>t.Member.HomeBranchId==user.CurrentBranchId);
        return await WorkListAsync(q.Select(t=>new FollowUpRow{Id=t.Id,SubjectType="Member",LeadId=null,MemberId=t.MemberId,Name=t.Member.FullName,
            Phone=t.Member.PrimaryPhone,BranchId=t.Member.HomeBranchId,BranchName=t.Member.HomeBranch.Name,Reason=t.Reason,DueAt=t.DueAt,CrossBranch=false}),r,ct);
    }
    private async Task<FollowUpListResult> WorkListAsync(IQueryable<FollowUpRow> q,FollowUpListRequest r,CancellationToken ct)
    {
        Page(r.Page,r.PageSize);if(r.Window.HasValue&&!Enum.IsDefined(r.Window.Value))throw new BusinessRuleException("Invalid follow-up filter.");
        if(r.BranchId.HasValue)q=q.Where(x=>x.BranchId==r.BranchId);
        var(start,end)=DayBounds();q=r.Window switch{FollowUpWindow.Overdue=>q.Where(x=>x.DueAt<start),FollowUpWindow.Today=>q.Where(x=>x.DueAt>=start&&x.DueAt<end),
            FollowUpWindow.Upcoming=>q.Where(x=>x.DueAt>=end),_=>q};
        return new(await q.OrderBy(x=>x.DueAt).ThenBy(x=>x.SubjectType).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize).Select(x=>new FollowUpItem(x.Id,x.SubjectType,x.LeadId,x.MemberId,x.Name,x.Phone,x.BranchId,x.BranchName,x.Reason,x.DueAt,x.CrossBranch)).ToListAsync(ct),await q.CountAsync(ct),r.Page,r.PageSize);
    }
    // Member-initialized projection allows EF to translate UNION/filter/order before constructing API records.
    private sealed class FollowUpRow
    {
        public Guid Id { get; set; }
        public string SubjectType { get; set; } = "";
        public Guid? LeadId { get; set; }
        public Guid? MemberId { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = "";
        public string Reason { get; set; } = "";
        public DateTimeOffset DueAt { get; set; }
        public bool CrossBranch { get; set; }
    }
    private static void ValidateFollowUp(MemberFollowUpRequest r)
    {
        if(r.Reason is not("RenewalDue" or "ExpiredMembership" or "InactiveMember" or "Reactivation" or "ManagerAssigned" or "Other")
            ||r.DueAt==default||r.Notes?.Length>4096)throw new BusinessRuleException("Invalid follow-up reason, date or notes.");
    }
    public async Task<Guid> OpenMemberFollowUpAsync(Guid memberId,MemberFollowUpRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("CRM.ManageMemberFollowUps",ct);ValidateFollowUp(r);
        return await TransactionAsync(async()=>{
            var member=await LockMemberAsync(memberId,ct);await MemberWorkAccessAsync(member,user,ct);
            return await OpenWorkAsync(member,r,user.UserId,ct);
        },ct);
    }
    // Future Subscription/CheckIn BLL integration; deliberately assembly-internal and no HTTP system-actor bypass.
    internal async Task<Guid> OpenMemberFollowUpFromWorkflowAsync(Guid memberId,MemberFollowUpRequest r,Guid? actor,CancellationToken ct=default)
    {
        ValidateFollowUp(r);return await TransactionAsync(async()=>await OpenWorkAsync(await LockMemberAsync(memberId,ct),r,actor,ct),ct);
    }
    private async Task<Guid> OpenWorkAsync(Member member,MemberFollowUpRequest r,Guid? actor,CancellationToken ct)
    {
        var task=await db.MemberFollowUpTasks.SingleOrDefaultAsync(t=>t.MemberId==member.Id&&t.Reason==r.Reason&&t.CompletedAt==null,ct);
        var created=task is null;
        if(task is null){task=new(){MemberId=member.Id,Reason=r.Reason,CreatedAt=access.Now,CreatedByUserId=actor};db.MemberFollowUpTasks.Add(task);}
        else await db.Entry(task).ReloadAsync(ct);
        var before=created?null:new{task.DueAt,task.Notes};task.DueAt=r.DueAt.ToUniversalTime();task.Notes=r.Notes?.Trim();
        access.Audit(actor,member.HomeBranchId,created?"FollowUp.Created":"FollowUp.Updated",nameof(MemberFollowUpTask),task.Id,before,new{task.MemberId,task.Reason,task.DueAt,task.Notes});
        await db.SaveChangesAsync(ct);return task.Id;
    }
    public async Task<Guid> CompleteMemberFollowUpAsync(Guid id,CancellationToken ct=default)
    {
        var user=await RequireAsync("CRM.ManageMemberFollowUps",ct);
        return await TransactionAsync(async()=>{
            var memberId=await db.MemberFollowUpTasks.Where(t=>t.Id==id).Select(t=>(Guid?)t.MemberId).SingleOrDefaultAsync(ct)
                ??throw new BranchNotFoundException("Follow-up not found.");
            var member=await LockMemberAsync(memberId,ct);await MemberWorkAccessAsync(member,user,ct);
            var task=await db.MemberFollowUpTasks.SingleAsync(t=>t.Id==id,ct);await db.Entry(task).ReloadAsync(ct);
            if(task.CompletedAt.HasValue)throw new BusinessRuleException("Follow-up is already complete.");
            task.CompletedAt=access.Now;task.CompletedByUserId=user.UserId;
            access.Audit(user.UserId,member.HomeBranchId,"FollowUp.Completed",nameof(MemberFollowUpTask),id,after:new{task.MemberId,task.CompletedAt});
            await db.SaveChangesAsync(ct);return id;
        },ct);
    }
    public async Task<MemberDetails> AssignHouseFollowUpAsync(Guid id,SalesAssignmentRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("CRM.ManageHouseFollowUps",ct);
        return await TransactionAsync(async()=>{
            var memberId=await db.MemberFollowUpTasks.Where(t=>t.Id==id&&t.CompletedAt==null).Select(t=>(Guid?)t.MemberId).SingleOrDefaultAsync(ct)
                ??throw new BranchNotFoundException("Open follow-up not found.");
            var member=await LockMemberAsync(memberId,ct);BranchAccessService.EnsureScope(user,member.HomeBranchId);
            if(!await db.MemberFollowUpTasks.AnyAsync(t=>t.Id==id&&t.CompletedAt==null,ct))
                throw new BusinessRuleException("Follow-up was completed; reload before distribution.");
            if(await MemberOwners().AnyAsync(a=>a.MemberId==memberId,ct))throw new BusinessRuleException("Member already has an owner; use Member reassignment.");
            var result=await members.AssignSalesAsync(memberId,r,ct);
            access.Audit(user.UserId,member.HomeBranchId,"HouseMember.AssignedForFollowUp",nameof(Member),memberId,after:new{FollowUpId=id,r.SalesEmployeeId});
            await db.SaveChangesAsync(ct);return result;
        },ct);
    }
}
