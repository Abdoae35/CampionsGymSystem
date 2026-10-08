using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Crm;

public sealed partial class LeadManagementService(ApplicationDbContext db, ICurrentUserService current,
    OperationAccess access, IPermissionService permissions, MemberManagementService members)
{
    private async Task<CurrentUserContext> RequireAsync(string permission, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        return user;
    }
    private IQueryable<LeadAssignment> Assignments() => db.LeadAssignments.AsNoTracking().Where(a => a.EndedAt == null);
    private IQueryable<Lead> Visible(CurrentUserContext user)
    {
        var q = db.Leads.AsNoTracking();
        if (user.Permissions.Contains("Leads.Assign"))
        {
            if (!user.Permissions.Contains("Branches.AccessAll")) q = q.Where(l => l.BranchId == user.CurrentBranchId);
        }
        else q = Owned(q,user.EmployeeId);
        return q;
    }
    private IQueryable<Lead> Owned(IQueryable<Lead> q,Guid? employeeId)
    {
        q=q.Where(l=>Assignments().Any(a=>a.LeadId==l.Id&&a.SalesEmployeeId==employeeId)
            ||l.Status==LeadStatus.Converted&&db.MemberSalesAssignments.Any(a=>a.MemberId==l.ConvertedMemberId
                &&a.EndedAt==null&&a.AssignmentType==AssignmentType.Sales&&a.SalesEmployeeId==employeeId));
        return q;
    }
    private async Task<T> TransactionAsync<T>(Func<Task<T>> change, CancellationToken ct)
    {
        var owned = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        var tx = db.Database.CurrentTransaction!; var point = "crm_" + Guid.NewGuid().ToString("N");
        try
        {
            await tx.CreateSavepointAsync(point, ct); var result = await change();
            await tx.ReleaseSavepointAsync(point, ct); if (owned is not null) await owned.CommitAsync(ct); return result;
        }
        catch
        {
            if (owned is not null) await owned.RollbackAsync(CancellationToken.None);
            else await tx.RollbackToSavepointAsync(point, CancellationToken.None);
            db.ChangeTracker.Clear(); throw;
        }
        finally { if (owned is not null) await owned.DisposeAsync(); }
    }
    private async Task<Lead> LockedAsync(Guid id, CurrentUserContext user, CancellationToken ct)
    {
        if (!await Visible(user).AnyAsync(l => l.Id == id, ct)) throw new BranchNotFoundException("Lead not found in accessible scope.");
        var lead = await db.Leads.FromSqlInterpolated($"SELECT * FROM \"Leads\" WHERE \"Id\" = {id} FOR UPDATE").SingleAsync(ct);
        await db.Entry(lead).ReloadAsync(ct);
        // Recheck after lock: owner may have changed while waiting.
        if (!await Visible(user).AnyAsync(l => l.Id == id, ct)) throw new BranchNotFoundException("Lead not found in accessible scope.");
        return lead;
    }
    private Task<LeadDetails> DetailsAsync(Lead l, CancellationToken ct) => Project(db.Leads.AsNoTracking().Where(x=>x.Id==l.Id)).SingleAsync(ct);
    private IQueryable<LeadDetails> Project(IQueryable<Lead> q) => q.Select(l=>new LeadDetails(l.Id,l.BranchId,l.Branch.Name,l.FullName,l.Phone,
        l.PhoneNormalized,l.SecondaryPhone,l.Email,l.Source,l.Notes,l.Status.ToString(),l.CreatedAt,l.NextFollowUpAt,
        db.LeadAssignments.Where(a=>a.LeadId==l.Id&&a.EndedAt==null).Select(a=>new LeadAssignmentDetails(a.Id,a.SalesEmployeeId,
            a.SalesEmployee.FullName,a.AssignedAt,a.EndedAt,a.AssignedByUserId,a.Reason)).SingleOrDefault(),
        l.ConvertedMemberId,l.ConvertedAt,l.LostAt,l.LostReason));
    private IQueryable<LeadAssignmentDetails> AssignmentQuery(Guid id) => Assignments().Where(a => a.LeadId == id)
        .Select(a => new LeadAssignmentDetails(a.Id,a.SalesEmployeeId,a.SalesEmployee.FullName,a.AssignedAt,a.EndedAt,a.AssignedByUserId,a.Reason));
    private static void Page(int page, int size)
    {
        if (page < 1 || size is < 1 or > 100 || (long)(page-1)*size > int.MaxValue) throw new BusinessRuleException("Invalid pagination.");
    }
    public async Task<LeadDetails> GetLeadByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await RequireAsync("Leads.View",ct);
        var lead = await Visible(user).SingleOrDefaultAsync(l => l.Id == id,ct) ?? throw new BranchNotFoundException("Lead not found in accessible scope.");
        return await DetailsAsync(lead,ct);
    }
    public async Task<LeadListResult> GetLeadsAsync(LeadListRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Leads.View",ct); return await ListAsync(user,request,false,ct);
    }
    private async Task<LeadListResult> ListAsync(CurrentUserContext user, LeadListRequest r, bool mine, CancellationToken ct)
    {
        Page(r.Page,r.PageSize);
        if (r.Search?.Length > 256 || r.Status.HasValue && !Enum.IsDefined(r.Status.Value) || r.FollowUp.HasValue && !Enum.IsDefined(r.FollowUp.Value))
            throw new BusinessRuleException("Invalid lead filters.");
        var q = Visible(user);
        if (mine)
        {
            q=Owned(q,user.EmployeeId);
            if(!r.Status.HasValue)q=q.Where(l=>l.Status!=LeadStatus.Converted&&l.Status!=LeadStatus.Lost&&l.Status!=LeadStatus.Inactive);
        }
        if (r.BranchId.HasValue) q=q.Where(l=>l.BranchId==r.BranchId);
        if (r.Status.HasValue) q=q.Where(l=>l.Status==r.Status);
        if (r.SalesEmployeeId.HasValue) q=q.Where(l=>Assignments().Any(a=>a.LeadId==l.Id && a.SalesEmployeeId==r.SalesEmployeeId));
        if (r.UnassignedOnly.HasValue) q=q.Where(l=>!Assignments().Any(a=>a.LeadId==l.Id)==r.UnassignedOnly);
        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s=r.Search.Trim().ToLowerInvariant(); var compact=System.Text.RegularExpressions.Regex.Replace(s,@"[\s().-]","").TrimStart('0');
            q=q.Where(l=>l.FullName.ToLower().Contains(s)||l.Phone.Contains(s)||l.PhoneNormalized!=null && l.PhoneNormalized.Contains(compact));
        }
        if (r.FollowUp.HasValue)
        {
            var (start,end)=DayBounds();
            q=q.Where(l=>l.Status!=LeadStatus.Lost && l.Status!=LeadStatus.Converted && l.Status!=LeadStatus.Inactive);
            q=r.FollowUp switch { FollowUpWindow.Overdue=>q.Where(l=>l.NextFollowUpAt<start), FollowUpWindow.Today=>q.Where(l=>l.NextFollowUpAt>=start&&l.NextFollowUpAt<end), _=>q.Where(l=>l.NextFollowUpAt>=end) };
        }
        var count=await q.CountAsync(ct); var result=await Project(q.OrderBy(l=>l.CreatedAt).ThenBy(l=>l.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)).ToListAsync(ct);
        return new(result,count,r.Page,r.PageSize);
    }
    // Gym calendar boundaries, not server-local or UTC midnight.
    private (DateTimeOffset Start,DateTimeOffset End) DayBounds() => access.DayBounds();
}
