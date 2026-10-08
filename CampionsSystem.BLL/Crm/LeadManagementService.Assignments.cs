using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Crm;
public sealed partial class LeadManagementService
{
    public async Task<LeadDetails> AssignLeadAsync(Guid id,LeadAssignmentRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.Assign",ct);
        if(r.SalesEmployeeId==Guid.Empty||r.Reason?.Length>2048)throw new BusinessRuleException("Invalid assignment.");
        return await TransactionAsync(async()=>{
            var lead=await LockedAsync(id,user,ct);
            if(lead.Status==LeadStatus.Converted)throw new BusinessRuleException("Use Member ownership management after conversion.");
            await ValidateOwnerAsync(r.SalesEmployeeId,lead.BranchId,ct);
            var old=await db.LeadAssignments.SingleOrDefaultAsync(a=>a.LeadId==id&&a.EndedAt==null,ct);
            if(old is not null)
            {
                await db.Entry(old).ReloadAsync(ct);
                if(old.SalesEmployeeId==r.SalesEmployeeId)throw new BusinessRuleException("Lead already belongs to this employee.");
                if(old.AssignedAt>access.Now)throw new BusinessRuleException("Reconcile future assignment first.");
                old.EndedAt=access.Now;await db.SaveChangesAsync(ct);
            }
            var next=new LeadAssignment{LeadId=id,SalesEmployeeId=r.SalesEmployeeId,AssignedByUserId=user.UserId,AssignedAt=access.Now,Reason=r.Reason?.Trim()};
            db.LeadAssignments.Add(next);
            access.Audit(user.UserId,lead.BranchId,old is null?"Lead.Assigned":"Lead.Reassigned",nameof(Lead),id,
                old is null?null:new{old.Id,old.SalesEmployeeId},new{next.Id,next.SalesEmployeeId,next.Reason});
            await db.SaveChangesAsync(ct);return await DetailsAsync(lead,ct);
        },ct);
    }
    public async Task<LeadAssignmentDetails?> GetCurrentLeadAssignmentAsync(Guid id,CancellationToken ct=default)
    {
        await GetLeadByIdAsync(id,ct);return await AssignmentQuery(id).SingleOrDefaultAsync(ct);
    }
    public async Task<IReadOnlyList<LeadAssignmentDetails>> GetLeadAssignmentHistoryAsync(Guid id,CancellationToken ct=default)
    {
        await GetLeadByIdAsync(id,ct);
        return await db.LeadAssignments.AsNoTracking().Where(a=>a.LeadId==id).OrderByDescending(a=>a.AssignedAt).ThenBy(a=>a.Id)
            .Select(a=>new LeadAssignmentDetails(a.Id,a.SalesEmployeeId,a.SalesEmployee.FullName,a.AssignedAt,a.EndedAt,a.AssignedByUserId,a.Reason)).ToListAsync(ct);
    }
    public async Task<LeadListResult> GetMyLeadsAsync(LeadListRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.View",ct);
        if(user.EmployeeId is null)throw new UnauthorizedAccessException();
        // Always restrict to authenticated employee, regardless of caller-supplied filters.
        return await ListAsync(user,r,true,ct);
    }
}
