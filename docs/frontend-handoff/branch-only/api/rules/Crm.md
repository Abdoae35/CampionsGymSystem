# Crm rule/validation evidence

Frozen working-tree source including pending Private drafts. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Crm/LeadManagementService.MemberFollowUps.cs

Lines 12–33

```csharp

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

```

Lines 36–54

```csharp

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

```

Lines 69–78

```csharp

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

```

Lines 96–107

```csharp

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

```

Lines 110–122

```csharp

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

```

## CampionsSystem.BLL/Crm/LeadManagementService.Create.cs

Lines 11–22

```csharp

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

```

Lines 27–34

```csharp

                Status=LeadStatus.New,CreatedByUserId=user.UserId,CreatedAt=access.Now};
            db.Leads.Add(lead);
            // Eligible Sales creating their own prospect do not need distribution permission.
            if(!user.Permissions.Contains("Leads.Assign"))
            {
                if(user.EmployeeId is null||!user.Permissions.Contains("Members.OwnSalesCustomers"))throw new UnauthorizedAccessException();
                await ValidateOwnerAsync(user.EmployeeId.Value,r.BranchId,ct);
                db.LeadAssignments.Add(new(){LeadId=lead.Id,SalesEmployeeId=user.EmployeeId.Value,AssignedByUserId=user.UserId,AssignedAt=access.Now,Reason="Registration"});

```

Lines 40–46

```csharp

    }
    public async Task<IReadOnlyList<LeadCreationResult>> CreateLeadsBulkAsync(IReadOnlyList<CreateLeadRequest> rows,CancellationToken ct=default)
    {
        await RequireAsync("Leads.Assign",ct);await RequireAsync("Leads.Manage",ct);
        if(rows.Count is <1 or >100)throw new BusinessRuleException("Batch must contain 1–100 leads.");
        var results=new List<LeadCreationResult>();
        foreach(var row in rows)

```

Lines 54–60

```csharp

    // JSON rows bypass recursive MVC row validation so one invalid row does not reject a valid batch.
    public async Task<IReadOnlyList<LeadCreationResult>> CreateLeadsBulkAsync(IReadOnlyList<System.Text.Json.JsonElement> rows,CancellationToken ct=default)
    {
        await RequireAsync("Leads.Assign",ct);await RequireAsync("Leads.Manage",ct);
        if(rows.Count is <1 or >100)throw new BusinessRuleException("Batch must contain 1–100 leads.");
        var results=new List<LeadCreationResult>();
        foreach(var row in rows)

```

Lines 72–83

```csharp

    }
    public async Task<LeadCreationResult> LookupPhoneAsync(Guid branchId,string phone,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.View",ct);BranchAccessService.EnsureScope(user,branchId);
        var branch=await db.GymBranches.AsNoTracking().SingleOrDefaultAsync(b=>b.Id==branchId&&b.IsActive,ct)
            ??throw new BusinessRuleException("An active operational branch is required.");
        var normalized=PhoneNormalizer.Normalize(phone,branch.CountryCallingCode);
        var result=await LookupAsync(user,normalized,ct)??new("NotFound");
        if(result.Result=="AlreadyLead" && result.Lead is null && user.EmployeeId.HasValue
            && user.Permissions.Contains("Leads.Convert") && user.Permissions.Contains("Members.Create"))
        {
            await members.RequireLeadConversionAccessAsync(branchId,false,ct);

```

Lines 91–97

```csharp

        var memberId=await PhoneIdentity.FindMemberAsync(db,phone,ct);
        if(memberId.HasValue)
        {
            var visible=user.Permissions.Contains("Members.View")&&await db.Members.AnyAsync(m=>m.Id==memberId &&
                (user.Permissions.Contains("Branches.AccessAll")||m.HomeBranchId==user.CurrentBranchId),ct);
            return new("AlreadyMember",ExistingMemberId:visible?memberId:null);
        }

```

Lines 115–120

```csharp

            ||!(await permissions.GetUserPermissionsAsync(uid,ct)).Contains("Members.OwnSalesCustomers")
            ||!await db.EmployeeBranchAssignments.AnyAsync(a=>a.EmployeeId==employeeId&&a.BranchId==branchId&&a.IsPrimary
                &&a.StartDate<=access.Today&&(a.EndDate==null||a.EndDate>=access.Today),ct))
            throw new BusinessRuleException("An eligible active Sales employee in the operational branch is required.");
    }
}

```

## CampionsSystem.BLL/Crm/LeadManagementService.Interactions.cs

Lines 7–24

```csharp

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

```

Lines 33–39

```csharp

    public async Task<IReadOnlyList<LeadInteractionDetails>> GetLeadInteractionsAsync(Guid id,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        Page(page,pageSize);await GetLeadByIdAsync(id,ct);
        return await db.LeadInteractions.AsNoTracking().Where(i=>i.LeadId==id).OrderByDescending(i=>i.ContactedAt??i.CreatedAt).ThenBy(i=>i.Id)
            .Skip((page-1)*pageSize).Take(pageSize).Select(i=>new LeadInteractionDetails(i.Id,i.EmployeeId,i.CreatedByUserId,i.InteractionType.ToString(),
                i.Notes,i.Outcome,i.ContactedAt??i.CreatedAt,i.NextFollowUpAt,i.CreatedAt)).ToListAsync(ct);
    }

```

## CampionsSystem.BLL/Crm/LeadManagementService.Lifecycle.cs

Lines 8–27

```csharp

{
    public async Task<LeadDetails> ChangeLeadStatusAsync(Guid id,LeadStatusRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.Manage",ct);
        if(!r.Status.HasValue||!Enum.IsDefined(r.Status.Value)||r.Status is LeadStatus.Converted or LeadStatus.Inactive||r.Reason?.Length>2048
            ||r.Status==LeadStatus.FollowUp&&!r.NextFollowUpAt.HasValue||r.NextFollowUpAt<=access.Now)
            throw new BusinessRuleException("Invalid lifecycle status or schedule. Conversion requires its dedicated workflow.");
        return await TransactionAsync(async()=>{
            var lead=await LockedAsync(id,user,ct);
            if(lead.Status==LeadStatus.Converted)throw new BusinessRuleException("Converted leads cannot change lifecycle.");
            if(lead.Status is LeadStatus.Lost or LeadStatus.Inactive)
            {
                if(!user.Permissions.Contains("Leads.Assign"))throw new UnauthorizedAccessException();
                CampionsSystem.BLL.Branches.BranchAccessService.EnsureScope(user,lead.BranchId);
            }
            var contacted=await db.LeadInteractions.AnyAsync(i=>i.LeadId==id,ct);
            if(r.Status==LeadStatus.New&&contacted||r.Status is LeadStatus.Contacted or LeadStatus.Interested&&!contacted)
                throw new BusinessRuleException("Status must reflect contact history; use FollowUp when retrying contacted leads.");
            var old=new{lead.Status,lead.NextFollowUpAt,lead.LostAt,lead.LostReason};
            var reactivated=lead.Status is LeadStatus.Lost or LeadStatus.Inactive && r.Status!=LeadStatus.Lost;

```

Lines 34–52

```csharp

    }
    public async Task<LeadDetails> UpdateLeadAsync(Guid id,UpdateLeadRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.Manage",ct);
        if(r.FullName?.Length>256||string.IsNullOrWhiteSpace(r.PrimaryPhone)||r.PrimaryPhone.Length>64
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
            lead.FullName=string.IsNullOrWhiteSpace(r.FullName)?null:r.FullName.Trim();lead.Phone=r.PrimaryPhone.Trim();lead.PhoneNormalized=normalized;lead.SecondaryPhone=r.SecondaryPhone?.Trim();

```

## CampionsSystem.BLL/Crm/LeadManagementService.Assignments.cs

Lines 7–25

```csharp

{
    public async Task<LeadDetails> AssignLeadAsync(Guid id,LeadAssignmentRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("Leads.Assign",ct);
        if(r.SalesEmployeeId==Guid.Empty||r.Reason?.Length>2048)throw new BusinessRuleException("Invalid assignment.");
        return await TransactionAsync(async()=>{
            var lead=await LockedAsync(id,user,ct);
            if(lead.Status==LeadStatus.Converted)throw new BusinessRuleException("Use Member ownership management after conversion.");
            // Ownership permits follow-up, not management distribution outside the current scope.
            CampionsSystem.BLL.Branches.BranchAccessService.EnsureScope(user,lead.BranchId);
            await ValidateOwnerAsync(r.SalesEmployeeId,lead.BranchId,ct);
            var old=await db.LeadAssignments.SingleOrDefaultAsync(a=>a.LeadId==id&&a.EndedAt==null,ct);
            if(old is not null)
            {
                await db.Entry(old).ReloadAsync(ct);
                if(old.SalesEmployeeId==r.SalesEmployeeId)throw new BusinessRuleException("Lead already belongs to this employee.");
                if(old.AssignedAt>access.Now)throw new BusinessRuleException("Reconcile future assignment first.");
                old.EndedAt=access.Now;await db.SaveChangesAsync(ct);
            }

```

Lines 37–48

```csharp

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

```

## CampionsSystem.BLL/Crm/LeadManagementService.cs

Lines 11–19

```csharp

public sealed partial class LeadManagementService(ApplicationDbContext db, ICurrentUserService current,
    OperationAccess access, IPermissionService permissions, MemberManagementService members)
{
    private async Task<CurrentUserContext> RequireAsync(string permission, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        return user;
    }

```

Lines 21–28

```csharp

    private IQueryable<Lead> Visible(CurrentUserContext user)
    {
        var q = db.Leads.AsNoTracking();
        if (user.Permissions.Contains("Leads.Assign"))
        {
            if (!user.Permissions.Contains("Branches.AccessAll")) q = q.Where(l => l.BranchId == user.CurrentBranchId
                || Assignments().Any(a => a.LeadId == l.Id && a.SalesEmployeeId == user.EmployeeId)
                || l.Status == LeadStatus.Converted && db.MemberSalesAssignments.Any(a => a.MemberId == l.ConvertedMemberId

```

Lines 57–66

```csharp

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

```

Lines 74–95

```csharp

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

```

Lines 112–117

```csharp

            q=q.Where(l=>l.Status!=LeadStatus.Lost && l.Status!=LeadStatus.Converted && l.Status!=LeadStatus.Inactive);
            q=r.FollowUp switch { FollowUpWindow.Overdue=>q.Where(l=>l.NextFollowUpAt<start), FollowUpWindow.Today=>q.Where(l=>l.NextFollowUpAt>=start&&l.NextFollowUpAt<end), _=>q.Where(l=>l.NextFollowUpAt>=end) };
        }
        var count=await q.CountAsync(ct); var result=await Project(q.OrderBy(l=>l.CreatedAt).ThenBy(l=>l.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)).ToListAsync(ct);
        return new(result,count,r.Page,r.PageSize);
    }

```

## CampionsSystem.BLL/Crm/LeadManagementService.Conversion.cs

Lines 12–22

```csharp

        return await TransactionAsync(async()=>{
            // Conversion alone may resolve another employee's Lead; normal list/detail visibility stays unchanged.
            var lead=await db.Leads.FromSqlInterpolated($"SELECT * FROM \"Leads\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct)
                ??throw new CampionsSystem.BLL.Branches.BranchNotFoundException("Lead not found.");
            await db.Entry(lead).ReloadAsync(ct);
            if(lead.Status is LeadStatus.Converted or LeadStatus.Lost or LeadStatus.Inactive||lead.ConvertedMemberId.HasValue)
                throw new BusinessRuleException("An active unconverted Lead is required.");
            var assignment=await db.LeadAssignments.SingleOrDefaultAsync(a=>a.LeadId==id&&a.EndedAt==null,ct);
            if(assignment is not null){await db.Entry(assignment).ReloadAsync(ct);if(assignment.AssignedAt>access.Now)throw new BusinessRuleException("Reconcile future assignment first.");}
            var member=await members.CreateMemberFromLeadAsync(id,new(){FullName=r.FullName,PrimaryPhone=r.PrimaryPhone,
                SecondaryPhone=r.SecondaryPhone,DateOfBirth=r.DateOfBirth,Gender=r.Gender,Notes=r.Notes,

```
