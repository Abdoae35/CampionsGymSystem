using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Crm;
public sealed partial class LeadManagementService
{
    public async Task<LeadDetails> AddLeadInteractionAsync(Guid id,LeadInteractionRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.Manage",ct);
        if(user.EmployeeId is null)throw new BusinessRuleException("A linked employee is required to record contact.");
        var contacted=(r.ContactedAt??access.Now).ToUniversalTime();
        if(!Enum.IsDefined(r.InteractionType)||r.Status is not(LeadStatus.Contacted or LeadStatus.FollowUp or LeadStatus.Interested)
            ||string.IsNullOrWhiteSpace(r.Notes)||r.Notes.Length>4096||r.Outcome?.Length>256||contacted>access.Now
            ||r.NextFollowUpAt<=contacted||r.Status==LeadStatus.FollowUp&&!r.NextFollowUpAt.HasValue)
            throw new BusinessRuleException("Invalid contact, status or next follow-up time.");
        return await TransactionAsync(async()=>{
            var lead=await LockedAsync(id,user,ct);
            if(lead.Status is LeadStatus.Lost or LeadStatus.Converted or LeadStatus.Inactive)throw new BusinessRuleException("Reactivate the lead before contact; converted leads use Member follow-ups.");
            var employee=await db.Employees.SingleOrDefaultAsync(e=>e.Id==user.EmployeeId&&e.UserId==user.UserId&&e.EmploymentStatus==EmploymentStatus.Active
                &&e.HireDate<=access.Today&&(e.TerminationDate==null||e.TerminationDate>=access.Today),ct)
                ??throw new BusinessRuleException("An active linked employee is required.");
            var latest=await db.LeadInteractions.Where(i=>i.LeadId==id).MaxAsync(i=>(DateTimeOffset?)(i.ContactedAt??i.CreatedAt),ct);
            var interaction=new LeadInteraction{LeadId=id,EmployeeId=employee.Id,CreatedByUserId=user.UserId,CreatedAt=access.Now,
                ContactedAt=contacted,InteractionType=r.InteractionType,Notes=r.Notes.Trim(),Outcome=r.Outcome?.Trim(),NextFollowUpAt=r.NextFollowUpAt?.ToUniversalTime()};
            db.LeadInteractions.Add(interaction);
            // Backdated history must not replace a newer contact's schedule/status.
            if(!latest.HasValue||contacted>=latest){lead.NextFollowUpAt=interaction.NextFollowUpAt;lead.Status=r.Status;}
            access.Audit(user.UserId,lead.BranchId,"Lead.InteractionAdded",nameof(Lead),id,after:new{interaction.Id,interaction.ContactedAt,interaction.NextFollowUpAt,r.Status});
            await db.SaveChangesAsync(ct);return await DetailsAsync(lead,ct);
        },ct);
    }
    public async Task<IReadOnlyList<LeadInteractionDetails>> GetLeadInteractionsAsync(Guid id,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        Page(page,pageSize);await GetLeadByIdAsync(id,ct);
        return await db.LeadInteractions.AsNoTracking().Where(i=>i.LeadId==id).OrderByDescending(i=>i.ContactedAt??i.CreatedAt).ThenBy(i=>i.Id)
            .Skip((page-1)*pageSize).Take(pageSize).Select(i=>new LeadInteractionDetails(i.Id,i.EmployeeId,i.CreatedByUserId,i.InteractionType.ToString(),
                i.Notes,i.Outcome,i.ContactedAt??i.CreatedAt,i.NextFollowUpAt,i.CreatedAt,i.Employee.FullName)).ToListAsync(ct);
    }
}
