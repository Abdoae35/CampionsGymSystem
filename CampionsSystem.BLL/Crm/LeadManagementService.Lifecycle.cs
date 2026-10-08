using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Crm;
public sealed partial class LeadManagementService
{
    public async Task<LeadDetails> ChangeLeadStatusAsync(Guid id,LeadStatusRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.Manage",ct);
        if(!Enum.IsDefined(r.Status)||r.Status is LeadStatus.Converted or LeadStatus.Inactive||r.Reason?.Length>2048
            ||r.Status==LeadStatus.FollowUp&&!r.NextFollowUpAt.HasValue||r.NextFollowUpAt<=access.Now)
            throw new BusinessRuleException("Invalid lifecycle status or schedule. Conversion requires its dedicated workflow.");
        return await TransactionAsync(async()=>{
            var lead=await LockedAsync(id,user,ct);
            if(lead.Status==LeadStatus.Converted)throw new BusinessRuleException("Converted leads cannot change lifecycle.");
            if(lead.Status is LeadStatus.Lost or LeadStatus.Inactive&&!user.Permissions.Contains("Leads.Assign"))throw new UnauthorizedAccessException();
            var contacted=await db.LeadInteractions.AnyAsync(i=>i.LeadId==id,ct);
            if(r.Status==LeadStatus.New&&contacted||r.Status is LeadStatus.Contacted or LeadStatus.Interested&&!contacted)
                throw new BusinessRuleException("Status must reflect contact history; use FollowUp when retrying contacted leads.");
            var old=new{lead.Status,lead.NextFollowUpAt,lead.LostAt,lead.LostReason};
            var reactivated=lead.Status is LeadStatus.Lost or LeadStatus.Inactive && r.Status!=LeadStatus.Lost;
            lead.Status=r.Status;lead.NextFollowUpAt=r.Status==LeadStatus.Lost?null:r.NextFollowUpAt?.ToUniversalTime();
            lead.LostAt=r.Status==LeadStatus.Lost?access.Now:null;lead.LostReason=r.Status==LeadStatus.Lost?r.Reason?.Trim():null;
            access.Audit(user.UserId,lead.BranchId,r.Status==LeadStatus.Lost?"Lead.MarkedLost":reactivated?"Lead.Reactivated":"Lead.StatusChanged",
                nameof(Lead),id,old,new{lead.Status,lead.NextFollowUpAt,lead.LostAt,lead.LostReason,r.Reason});
            await db.SaveChangesAsync(ct);return await DetailsAsync(lead,ct);
        },ct);
    }
    public async Task<LeadDetails> UpdateLeadAsync(Guid id,UpdateLeadRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.Manage",ct);
        if(string.IsNullOrWhiteSpace(r.FullName)||r.FullName.Length>256||string.IsNullOrWhiteSpace(r.PrimaryPhone)||r.PrimaryPhone.Length>64
            ||r.SecondaryPhone?.Length>64||r.Email?.Length>320||r.Source?.Length>256||r.Notes?.Length>4096
            ||!string.IsNullOrWhiteSpace(r.Email)&&!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(r.Email))
            throw new BusinessRuleException("Invalid lead profile.");
        return await TransactionAsync(async()=>{
            var lead=await LockedAsync(id,user,ct);
            if(lead.Status==LeadStatus.Converted)throw new BusinessRuleException("Update the Member profile after conversion.");
            var code=await db.GymBranches.Where(b=>b.Id==lead.BranchId).Select(b=>b.CountryCallingCode).SingleAsync(ct);
            var normalized=lead.Phone==r.PrimaryPhone.Trim()&&lead.PhoneNormalized is not null?lead.PhoneNormalized:PhoneNormalizer.Normalize(r.PrimaryPhone,code);
            await PhoneIdentity.LockAsync(db,normalized,ct);
            if(await PhoneIdentity.FindMemberAsync(db,normalized,ct) is not null)throw new BusinessRuleException("An existing Member uses this phone.");
            var duplicate=await PhoneIdentity.FindLeadAsync(db,normalized,ct);
            if(duplicate.HasValue&&duplicate!=id)throw new BusinessRuleException("An existing Lead uses this phone.");
            var old=new{lead.FullName,lead.Phone,lead.PhoneNormalized,lead.SecondaryPhone,lead.Email,lead.Source,lead.Notes};
            lead.FullName=r.FullName.Trim();lead.Phone=r.PrimaryPhone.Trim();lead.PhoneNormalized=normalized;lead.SecondaryPhone=r.SecondaryPhone?.Trim();
            lead.Email=r.Email?.Trim();lead.Source=r.Source?.Trim();lead.Notes=r.Notes?.Trim();
            access.Audit(user.UserId,lead.BranchId,"Lead.Updated",nameof(Lead),id,old,new{lead.FullName,lead.Phone,lead.PhoneNormalized,lead.SecondaryPhone,lead.Email,lead.Source,lead.Notes});
            await db.SaveChangesAsync(ct);return await DetailsAsync(lead,ct);
        },ct);
    }
}
