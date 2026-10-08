using System.Text.Json;
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
    public async Task<LeadCreationResult> CreateLeadAsync(CreateLeadRequest r, CancellationToken ct = default)
    {
        var user=await RequireAsync("Leads.Manage",ct); BranchAccessService.EnsureScope(user,r.BranchId);
        if (r.FullName?.Length>256||string.IsNullOrWhiteSpace(r.PrimaryPhone)||r.PrimaryPhone.Length>64
            ||r.SecondaryPhone?.Length>64||r.Email?.Length>320||r.Source?.Length>256||r.Notes?.Length>4096
            ||!string.IsNullOrWhiteSpace(r.Email)&&!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(r.Email))
            throw new BusinessRuleException("Invalid lead profile.");
        var branch=await db.GymBranches.AsNoTracking().SingleOrDefaultAsync(b=>b.Id==r.BranchId&&b.IsActive,ct)
            ?? throw new BusinessRuleException("An active operational branch is required.");
        var phone=PhoneNormalizer.Normalize(r.PrimaryPhone,branch.CountryCallingCode);
        return await TransactionAsync(async()=>{
            await PhoneIdentity.LockAsync(db,phone,ct);
            var duplicate=await LookupAsync(user,phone,ct); if(duplicate is not null)return duplicate;
            var lead=new Lead{BranchId=r.BranchId,FullName=string.IsNullOrWhiteSpace(r.FullName)?null:r.FullName.Trim(),Phone=r.PrimaryPhone.Trim(),PhoneNormalized=phone,
                SecondaryPhone=r.SecondaryPhone?.Trim(),Email=r.Email?.Trim(),Source=r.Source?.Trim(),Notes=r.Notes?.Trim(),
                Status=LeadStatus.New,CreatedByUserId=user.UserId,CreatedAt=access.Now};
            db.Leads.Add(lead);
            // Eligible Sales creating their own prospect do not need distribution permission.
            if(!user.Permissions.Contains("Leads.Assign"))
            {
                if(user.EmployeeId is null||!user.Permissions.Contains("Members.OwnSalesCustomers"))throw new UnauthorizedAccessException();
                await ValidateOwnerAsync(user.EmployeeId.Value,r.BranchId,ct);
                db.LeadAssignments.Add(new(){LeadId=lead.Id,SalesEmployeeId=user.EmployeeId.Value,AssignedByUserId=user.UserId,AssignedAt=access.Now,Reason="Registration"});
                access.Audit(user.UserId,r.BranchId,"Lead.Assigned",nameof(Lead),lead.Id,after:new{SalesEmployeeId=user.EmployeeId});
            }
            access.Audit(user.UserId,r.BranchId,"Lead.Created",nameof(Lead),lead.Id,after:new{lead.FullName,lead.Phone,lead.PhoneNormalized});
            await db.SaveChangesAsync(ct);return new LeadCreationResult("Created",await DetailsAsync(lead,ct));
        },ct);
    }
    public async Task<IReadOnlyList<LeadCreationResult>> CreateLeadsBulkAsync(IReadOnlyList<CreateLeadRequest> rows,CancellationToken ct=default)
    {
        await RequireAsync("Leads.Assign",ct);await RequireAsync("Leads.Manage",ct);
        if(rows.Count is <1 or >100)throw new BusinessRuleException("Batch must contain 1–100 leads.");
        var results=new List<LeadCreationResult>();
        foreach(var row in rows)
        {
            try { results.Add(row is null?new("Invalid",Error:"Lead row is required."):await CreateLeadAsync(row,ct)); }
            catch(BusinessRuleException e){results.Add(new("Invalid",Error:e.Message));}
            catch(UnauthorizedAccessException){results.Add(new("Invalid",Error:"Branch or action is not permitted."));}
        }
        return results;
    }
    // JSON rows bypass recursive MVC row validation so one invalid row does not reject a valid batch.
    public async Task<IReadOnlyList<LeadCreationResult>> CreateLeadsBulkAsync(IReadOnlyList<System.Text.Json.JsonElement> rows,CancellationToken ct=default)
    {
        await RequireAsync("Leads.Assign",ct);await RequireAsync("Leads.Manage",ct);
        if(rows.Count is <1 or >100)throw new BusinessRuleException("Batch must contain 1–100 leads.");
        var results=new List<LeadCreationResult>();
        foreach(var row in rows)
        {
            try
            {
                var request=row.Deserialize<CreateLeadRequest>(new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
                results.Add(request is null?new("Invalid",Error:"Lead row is required."):await CreateLeadAsync(request,ct));
            }
            catch(System.Text.Json.JsonException){results.Add(new("Invalid",Error:"Invalid lead JSON fields."));}
            catch(BusinessRuleException e){results.Add(new("Invalid",Error:e.Message));}
            catch(UnauthorizedAccessException){results.Add(new("Invalid",Error:"Branch or action is not permitted."));}
        }
        return results;
    }
    public async Task<LeadCreationResult> LookupPhoneAsync(Guid branchId,string phone,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.View",ct);BranchAccessService.EnsureScope(user,branchId);
        var branch=await db.GymBranches.AsNoTracking().SingleOrDefaultAsync(b=>b.Id==branchId&&b.IsActive,ct)
            ??throw new BusinessRuleException("An active operational branch is required.");
        return await LookupAsync(user,PhoneNormalizer.Normalize(phone,branch.CountryCallingCode),ct)??new("NotFound");
    }
    private async Task<LeadCreationResult?> LookupAsync(CurrentUserContext user,string phone,CancellationToken ct)
    {
        var memberId=await PhoneIdentity.FindMemberAsync(db,phone,ct);
        if(memberId.HasValue)
        {
            var visible=user.Permissions.Contains("Members.View")&&await db.Members.AnyAsync(m=>m.Id==memberId &&
                (user.Permissions.Contains("Branches.AccessAll")||m.HomeBranchId==user.CurrentBranchId),ct);
            return new("AlreadyMember",ExistingMemberId:visible?memberId:null);
        }
        var leadId=await PhoneIdentity.FindLeadAsync(db,phone,ct);
        if(leadId.HasValue)
        {
            var lead=await Visible(user).SingleOrDefaultAsync(l=>l.Id==leadId,ct);
            return new("AlreadyLead",lead is null?null:await DetailsAsync(lead,ct));
        }
        return null;
    }
    // Same effective capability/account rules as Member ownership. Lead distribution stays operationally local.
    private async Task ValidateOwnerAsync(Guid employeeId,Guid branchId,CancellationToken ct)
    {
        // Serialize eligibility with employment status changes; transfers retain existing ownership.
        await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\" = {employeeId} FOR SHARE")
            .AsNoTracking().ToListAsync(ct);
        var e=await db.Employees.AsNoTracking().Include(e=>e.User).SingleOrDefaultAsync(e=>e.Id==employeeId,ct);
        if(e?.UserId is not Guid uid||e.User is not{IsActive:true}||e.User.LockoutEnabled&&e.User.LockoutEnd>access.Now
            ||e.EmploymentStatus!=EmploymentStatus.Active||e.HireDate>access.Today||e.TerminationDate<access.Today
            ||!(await permissions.GetUserPermissionsAsync(uid,ct)).Contains("Members.OwnSalesCustomers")
            ||!await db.EmployeeBranchAssignments.AnyAsync(a=>a.EmployeeId==employeeId&&a.BranchId==branchId&&a.IsPrimary
                &&a.StartDate<=access.Today&&(a.EndDate==null||a.EndDate>=access.Today),ct))
            throw new BusinessRuleException("An eligible active Sales employee in the operational branch is required.");
    }
}
