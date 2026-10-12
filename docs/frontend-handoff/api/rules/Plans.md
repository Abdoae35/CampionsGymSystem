# Plans rule/validation evidence

Baseline source, not new Private drafts. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Plans/MembershipPlanManagementService.Save.cs

Lines 9–18

```csharp

{
    private async Task BranchWriterAsync(CurrentUserContext user,Guid? branchId,CancellationToken ct)
    {
        BranchAccessService.EnsureScope(user,branchId);
        if(branchId.HasValue)
        {
            var branch=await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={branchId.Value} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
            if(branch is null||!branch.IsActive)throw new BusinessRuleException("An active owning branch is required.");
        }
    }

```

Lines 20–25

```csharp

    {
        await VisiblePlanAsync(id,user,ct);
        var p=await db.MembershipPlans.FromSqlInterpolated($"SELECT * FROM \"MembershipPlans\" WHERE \"Id\"={id} FOR UPDATE").SingleAsync(ct);
        await db.Entry(p).ReloadAsync(ct);BranchAccessService.EnsureScope(user,p.BranchId);
        if(!Global(user))await BranchWriterAsync(user,p.BranchId,ct);return p;
    }

```

Lines 28–60

```csharp

        if(r.BranchId==Guid.Empty||r.CityId==Guid.Empty||string.IsNullOrWhiteSpace(r.Name)||r.Name.Length>256
            ||r.PlanType is null||!Enum.IsDefined(r.PlanType.Value)||r.Mode is null||!Enum.IsDefined(r.Mode.Value)
            ||r.FreezeDays<0||!r.AllowFreeze&&r.FreezeDays!=0||r.Price>9999999999999999.99m)
            throw new BusinessRuleException("Invalid plan branch, city, name, type, mode, price or freeze configuration.");
        OperationAccess.Money(r.Price);
        if(!(r.Mode==AccessMode.Unlimited&&r.Sessions is null||r.Mode==AccessMode.SessionBased&&r.Sessions>0))throw new BusinessRuleException("Unlimited needs no sessions; SessionBased needs positive sessions.");
        if(r.PlanType==PlanCategory.Employee)
        {
            if(r.Price!=0||r.Mode!=AccessMode.Unlimited||r.DurationDays!=0||r.FreelanceCoachEmployeeId.HasValue)throw new BusinessRuleException("Employee plans are free, Unlimited, duration zero and have no Freelance Coach.");
        }
        else if(r.DurationDays<=0)throw new BusinessRuleException("Customer plans require positive duration.");
        if(old is not null&&old.PlanCategory!=r.PlanType)throw new BusinessRuleException("Behavioral type is immutable; create a new plan for another type.");
        if(!await db.Cities.AnyAsync(c=>c.Id==r.CityId&&c.IsActive,ct))throw new BusinessRuleException("An active access city is required.");
        if(r.AudienceCategoryId.HasValue)
        {
            // Keep existing inactive classifications readable/editable; newly selected classifications must be active.
            var category=await db.MembershipPlanCategories.FromSqlInterpolated($"SELECT * FROM \"MembershipPlanCategories\" WHERE \"Id\"={r.AudienceCategoryId.Value} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
            if(category is null||!category.IsActive&&old?.MembershipPlanCategoryId!=category.Id)throw new BusinessRuleException("An active audience category is required when selecting a category.");
        }
        if(r.PlanType==PlanCategory.Freelance)
        {
            if(!r.FreelanceCoachEmployeeId.HasValue)throw new BusinessRuleException("Freelance Coach is required.");
            await ValidateCoachAsync(r.FreelanceCoachEmployeeId.Value,ct);
        }
        else if(r.FreelanceCoachEmployeeId.HasValue)throw new BusinessRuleException("Only Freelance plans may reference a Freelance Coach.");
    }
    private async Task ValidateCoachAsync(Guid id,CancellationToken ct)
    {
        var coach=await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\"={id} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if(coach is null||coach.EmploymentType!=EmploymentType.Freelance||coach.EmploymentStatus!=EmploymentStatus.Active||coach.HireDate>access.Today||coach.TerminationDate<access.Today)
            throw new BusinessRuleException("An active Freelance Employee is required.");
    }
    private static object PlanValues(MembershipPlan p)=>new{p.Name,p.BranchId,p.CityId,p.PlanCategory,p.MembershipPlanCategoryId,p.Price,p.DurationDays,p.AccessMode,p.SessionCount,p.AllowFreeze,p.IncludedFreezeDays,p.FreelanceCoachEmployeeId,p.IsActive};

```

Lines 65–70

```csharp

    }
    private async Task<MembershipPlanDetails> SavePlanAsync(Guid? id,UpdateMembershipPlanRequest r,string permission,CancellationToken ct,bool? status=null,bool legacy=false)
    {
        var user=await RequireAsync(permission,ct);
        return await TransactionAsync(async()=>{
            var p=id.HasValue?await LockPlanAsync(id.Value,user,ct):new MembershipPlan{CreatedAt=access.Now};

```

## CampionsSystem.BLL/Plans/MembershipPlanManagementService.cs

Lines 8–32

```csharp


public sealed partial class MembershipPlanManagementService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private async Task<CurrentUserContext> RequireAsync(string permission,CancellationToken ct)
    {
        var user=await current.GetAsync(ct);
        if(user is null||!user.Permissions.Contains(permission))throw new UnauthorizedAccessException();
        return user;
    }
    private async Task<CurrentUserContext> CatalogWriterAsync(CancellationToken ct)
    {
        var user=await RequireAsync("MembershipPlans.Manage",ct);
        if(!user.Permissions.Contains("Branches.AccessAll"))throw new UnauthorizedAccessException();
        return user;
    }
    private static void Page(CatalogListRequest r)
    {
        if(r.Page<1||r.PageSize is <1 or >100||(long)(r.Page-1)*r.PageSize>int.MaxValue||r.Search?.Length>256)
            throw new BusinessRuleException("Invalid search or pagination.");
    }
    private static void CatalogProfile(string name,string? description)
    {
        if(string.IsNullOrWhiteSpace(name)||name.Length>256||description?.Length>4096)throw new BusinessRuleException("Invalid catalog name or description.");
    }
    private async Task<T> TransactionAsync<T>(Func<Task<T>> change,CancellationToken ct)

```

Lines 40–48

```csharp

    private static CatalogDetails CategoryDetails(MembershipPlanCategory c)=>new(c.Id,null,c.Name,c.Description,c.IsActive,c.CreatedAt);
    public async Task<CatalogListResult> GetCategoriesAsync(CatalogListRequest r,CancellationToken ct=default)
    {
        await RequireAsync("MembershipPlans.View",ct);Page(r);var q=db.MembershipPlanCategories.AsNoTracking();
        if(r.IsActive.HasValue)q=q.Where(c=>c.IsActive==r.IsActive);
        if(!string.IsNullOrWhiteSpace(r.Search)){var s=r.Search.Trim().ToLowerInvariant();q=q.Where(c=>c.Name.ToLower().Contains(s));}
        return new(await q.OrderBy(c=>c.Name).ThenBy(c=>c.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)
            .Select(c=>new CatalogDetails(c.Id,null,c.Name,c.Description,c.IsActive,c.CreatedAt)).ToListAsync(ct),await q.CountAsync(ct),r.Page,r.PageSize);
    }

```

Lines 50–55

```csharp

    {
        var user=await CatalogWriterAsync(ct);CatalogProfile(r.Name,r.Description);
        return await TransactionAsync(async()=>{
            var name=r.Name.Trim();if(await db.MembershipPlanCategories.AnyAsync(c=>c.Name==name,ct))throw new BusinessRuleException("Category name already exists.");
            var c=new MembershipPlanCategory{Name=name,Description=r.Description?.Trim(),CreatedAt=access.Now};db.Add(c);
            access.Audit(user.UserId,null,"MembershipPlanCategory.Created",nameof(MembershipPlanCategory),c.Id,after:new{c.Name,c.Description});

```

Lines 59–64

```csharp

    private async Task<MembershipPlanCategory> LockCategoryAsync(Guid id,CancellationToken ct)
    {
        var c=await db.MembershipPlanCategories.FromSqlInterpolated($"SELECT * FROM \"MembershipPlanCategories\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)
            ??throw new BranchNotFoundException("Category not found.");await db.Entry(c).ReloadAsync(ct);return c;
    }
    public async Task<CatalogDetails> UpdateCategoryAsync(Guid id,SaveCategoryRequest r,CancellationToken ct=default)

```

Lines 66–71

```csharp

        var user=await CatalogWriterAsync(ct);CatalogProfile(r.Name,r.Description);
        return await TransactionAsync(async()=>{
            var c=await LockCategoryAsync(id,ct);var name=r.Name.Trim();
            if(await db.MembershipPlanCategories.AnyAsync(x=>x.Id!=id&&x.Name==name,ct))throw new BusinessRuleException("Category name already exists.");
            var old=new{c.Name,c.Description};c.Name=name;c.Description=r.Description?.Trim();
            access.Audit(user.UserId,null,"MembershipPlanCategory.Updated",nameof(MembershipPlanCategory),id,old,new{c.Name,c.Description});

```

Lines 74–79

```csharp

    }
    public async Task<CatalogDetails> ChangeCategoryStatusAsync(Guid id,ChangePlanStatusRequest r,CancellationToken ct=default)
    {
        var user=await CatalogWriterAsync(ct);if(r.IsActive is null)throw new BusinessRuleException("Status is required.");
        return await TransactionAsync(async()=>{var c=await LockCategoryAsync(id,ct);var old=new{c.IsActive};c.IsActive=r.IsActive.Value;
            access.Audit(user.UserId,null,"MembershipPlanCategory.StatusChanged",nameof(MembershipPlanCategory),id,old,new{c.IsActive});

```

## CampionsSystem.BLL/Plans/MembershipPlanManagementService.Eligibility.cs

Lines 8–17

```csharp

{
    private static void Freelance(MembershipPlan p)
    {
        if(p.PlanCategory!=PlanCategory.Freelance)throw new BusinessRuleException("Eligibility applies only to Freelance plans.");
    }
    public async Task<EligibleMemberListResult> GetEligibleMembersAsync(Guid id,CatalogListRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("FreelancePlans.ManageEligibleMembers",ct);Page(r);Freelance(await VisiblePlanAsync(id,user,ct));
        var q=db.MembershipPlanEligibleMembers.AsNoTracking().Where(e=>e.MembershipPlanId==id);
        if(r.IsActive.HasValue)q=q.Where(e=>e.Member.IsActive==r.IsActive);

```

Lines 20–32

```csharp

            var s=r.Search.Trim().ToLowerInvariant();q=q.Where(e=>e.Member.FullName.ToLower().Contains(s)||e.Member.PrimaryPhone.Contains(s)
                ||e.Member.PrimaryPhoneNormalized!=null&&e.Member.PrimaryPhoneNormalized.Contains(s)||e.Member.MemberCode.ToLower().Contains(s));
        }
        return new(await q.OrderBy(e=>e.Member.FullName).ThenBy(e=>e.MemberId).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)
            .Select(e=>new EligibleMemberDetails(e.MemberId,e.Member.FullName,e.Member.PrimaryPhone,e.Member.HomeBranchId,e.Member.ClientCode,e.Member.MemberCode,e.CreatedAt)).ToListAsync(ct),await q.CountAsync(ct),r.Page,r.PageSize);
    }
    public async Task<IReadOnlyList<EligibleMemberDetails>> FindEligibleMemberCandidatesAsync(Guid id,EligibleMemberCandidateRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("FreelancePlans.ManageEligibleMembers",ct);var plan=await VisiblePlanAsync(id,user,ct);Freelance(plan);
        if((r.MemberId.HasValue?1:0)+(!string.IsNullOrWhiteSpace(r.Phone)?1:0)+(!string.IsNullOrWhiteSpace(r.MemberCode)?1:0)!=1||r.MemberCode?.Length>256)
            throw new BusinessRuleException("Provide exactly one Member ID, exact phone or exact MemberCode.");
        var q=db.Members.AsNoTracking().Where(m=>m.IsActive);
        if(r.MemberId.HasValue)q=q.Where(m=>m.Id==r.MemberId);

```

Lines 37–57

```csharp

            q=q.Where(m=>m.PrimaryPhoneNormalized==normalized||m.PrimaryPhoneNormalized==null&&m.PrimaryPhone==display);
        }
        else{var code=r.MemberCode!.Trim();q=q.Where(m=>m.MemberCode==code);}
        return await q.OrderBy(m=>m.Id).Take(10).Select(m=>new EligibleMemberDetails(m.Id,m.FullName,m.PrimaryPhone,m.HomeBranchId,m.ClientCode,m.MemberCode,null)).ToListAsync(ct);
    }
    public Task<bool> AddEligibleMemberAsync(Guid id,EligibleMemberRequest r,CancellationToken ct=default)=>SetEligibilityAsync(id,r.MemberId,true,ct);
    public Task<bool> RemoveEligibleMemberAsync(Guid id,Guid memberId,CancellationToken ct=default)=>SetEligibilityAsync(id,memberId,false,ct);
    internal async Task<bool> SetEligibilityAsync(Guid id,Guid memberId,bool eligible,CancellationToken ct)
    {
        var user=await RequireAsync("FreelancePlans.ManageEligibleMembers",ct);
        return await TransactionAsync(async()=>{
            var plan=await LockPlanAsync(id,user,ct);Freelance(plan);
            var row=await db.MembershipPlanEligibleMembers.SingleOrDefaultAsync(e=>e.MembershipPlanId==id&&e.MemberId==memberId,ct);
            if(eligible)
            {
                if(!plan.IsActive)throw new BusinessRuleException("Select an active Freelance plan.");
                await ValidateCoachAsync(plan.FreelanceCoachEmployeeId!.Value,ct);
                var member=await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={memberId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
                if(member is null||!member.IsActive)throw new BusinessRuleException("Select an active Member.");
                if(row is not null)return false;
                row=new(){MembershipPlanId=id,MemberId=memberId,CreatedByUserId=user.UserId,CreatedAt=access.Now};db.Add(row);

```

## CampionsSystem.BLL/Plans/MembershipPlanManagementService.Update.cs

Lines 8–14

```csharp


    public async Task<MembershipPlanDetails> ChangePlanStatusAsync(Guid id,ChangePlanStatusRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("MembershipPlans.Manage",ct);
        if(r.IsActive is null)throw new BusinessRuleException("Status is required.");
        return await TransactionAsync(async()=>{
            var p=await LockPlanAsync(id,user,ct);

```

## CampionsSystem.BLL/Plans/MembershipPlanManagementService.PlanBenefits.cs

Lines 11–31

```csharp

    }
    public async Task<IReadOnlyList<PlanBenefitDetails>> SetPlanBenefitsAsync(Guid id,SetPlanBenefitsRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("MembershipPlans.Manage",ct);
        if(r.Benefits is null||r.Benefits.Count>100||r.Benefits.Any(b=>b is null)||r.Benefits.Select(b=>b.BenefitId).Distinct().Count()!=r.Benefits.Count)
            throw new BusinessRuleException("Supply at most 100 distinct benefits.");
        foreach(var b in r.Benefits)
            if(b.BenefitId==Guid.Empty||!(b.GrantType==MembershipBenefitGrantType.Quantity&&b.Quantity>0||b.GrantType==MembershipBenefitGrantType.Access&&b.Quantity is null))
                throw new BusinessRuleException("Quantity grants need positive quantity; Access grants need null quantity.");
        return await TransactionAsync<IReadOnlyList<PlanBenefitDetails>>(async()=>{
            var plan=await LockPlanAsync(id,user,ct);
            var existing=await db.MembershipPlanBenefits.Where(b=>b.MembershipPlanId==id).ToListAsync(ct);
            // Lock catalog entries in a stable order; additions and catalog deactivation cannot race.
            foreach(var b in r.Benefits.OrderBy(b=>b.BenefitId))
            {
                var catalog=await db.MembershipBenefits.FromSqlInterpolated($"SELECT * FROM \"MembershipBenefits\" WHERE \"Id\"={b.BenefitId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
                var old=existing.SingleOrDefault(x=>x.MembershipBenefitId==b.BenefitId);
                if(catalog is null||!catalog.IsActive&&(old is null||old.GrantType!=b.GrantType||old.Quantity!=b.Quantity))
                    throw new BusinessRuleException("Select active benefits; unchanged inactive configurations may be retained or removed.");
            }
            foreach(var old in existing.Where(x=>!r.Benefits.Any(b=>b.BenefitId==x.MembershipBenefitId)))

```

## CampionsSystem.BLL/Plans/MembershipPlanManagementService.BenefitCatalog.cs

Lines 8–34

```csharp

    private static CatalogDetails BenefitDetails(MembershipBenefit b)=>new(b.Id,b.Code,b.Name,b.Description,b.IsActive,b.CreatedAt);
    public async Task<CatalogListResult> GetBenefitsAsync(CatalogListRequest r,CancellationToken ct=default)
    {
        await RequireAsync("MembershipPlans.View",ct);Page(r);var q=db.MembershipBenefits.AsNoTracking();
        if(r.IsActive.HasValue)q=q.Where(b=>b.IsActive==r.IsActive);
        if(!string.IsNullOrWhiteSpace(r.Search)){var s=r.Search.Trim().ToLowerInvariant();q=q.Where(b=>b.Name.ToLower().Contains(s)||b.Code.ToLower().Contains(s));}
        return new(await q.OrderBy(b=>b.Name).ThenBy(b=>b.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)
            .Select(b=>new CatalogDetails(b.Id,b.Code,b.Name,b.Description,b.IsActive,b.CreatedAt)).ToListAsync(ct),await q.CountAsync(ct),r.Page,r.PageSize);
    }
    public async Task<CatalogDetails> GetBenefitByIdAsync(Guid id,CancellationToken ct=default)
    {
        await RequireAsync("MembershipPlans.View",ct);
        var b=await db.MembershipBenefits.AsNoTracking().SingleOrDefaultAsync(b=>b.Id==id,ct)??throw new BranchNotFoundException("Benefit not found.");return BenefitDetails(b);
    }
    private async Task<MembershipBenefit> LockBenefitAsync(Guid id,CancellationToken ct)
    {
        var b=await db.MembershipBenefits.FromSqlInterpolated($"SELECT * FROM \"MembershipBenefits\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)
            ??throw new BranchNotFoundException("Benefit not found.");await db.Entry(b).ReloadAsync(ct);return b;
    }
    private async Task ValidateBenefitAsync(SaveBenefitRequest r,Guid? except,CancellationToken ct)
    {
        CatalogProfile(r.Name,r.Description);
        if(string.IsNullOrWhiteSpace(r.Code)||r.Code.Length>128)throw new BusinessRuleException("Benefit code is required, up to 128 characters.");
        if(await db.MembershipBenefits.AnyAsync(b=>b.Id!=except&&(b.Code==r.Code.Trim()||b.Name==r.Name.Trim()),ct))
            throw new BusinessRuleException("Benefit code or name already exists.");
    }
    public async Task<CatalogDetails> CreateBenefitAsync(SaveBenefitRequest r,CancellationToken ct=default)

```

Lines 48–53

```csharp

    }
    public async Task<CatalogDetails> ChangeBenefitStatusAsync(Guid id,ChangePlanStatusRequest r,CancellationToken ct=default)
    {
        var user=await CatalogWriterAsync(ct);if(r.IsActive is null)throw new BusinessRuleException("Status is required.");
        return await TransactionAsync(async()=>{var b=await LockBenefitAsync(id,ct);var old=new{b.IsActive};b.IsActive=r.IsActive.Value;
            access.Audit(user.UserId,null,"MembershipBenefit.StatusChanged",nameof(MembershipBenefit),id,old,new{b.IsActive});

```

## CampionsSystem.BLL/Plans/MembershipPlanManagementService.Queries.cs

Lines 6–11

```csharp

namespace CampionsSystem.BLL.Plans;
public sealed partial class MembershipPlanManagementService
{
    private bool Global(CurrentUserContext u)=>u.Permissions.Contains("Branches.AccessAll");
    private IQueryable<MembershipPlan> Scoped(CurrentUserContext u)
    {

```

Lines 19–36

```csharp

        p.AccessMode.ToString(),p.Price,p.DurationDays,p.SessionCount,p.AllowFreeze,p.IncludedFreezeDays,p.IsActive,
        p.FreelanceCoachEmployeeId==null?null:new PlanReference(p.FreelanceCoachEmployeeId.Value,p.FreelanceCoachEmployee!.FullName)));
    private async Task<MembershipPlan> VisiblePlanAsync(Guid id,CurrentUserContext user,CancellationToken ct)
        =>await Scoped(user).SingleOrDefaultAsync(p=>p.Id==id,ct)??throw new BranchNotFoundException("Plan not found in accessible scope.");
    private Task<List<PlanBenefitDetails>> BenefitsAsync(Guid id,CancellationToken ct)=>db.MembershipPlanBenefits.AsNoTracking().Where(b=>b.MembershipPlanId==id)
        .OrderBy(b=>b.MembershipBenefit.Name).ThenBy(b=>b.Id).Select(b=>new PlanBenefitDetails(b.Id,b.MembershipBenefitId,b.MembershipBenefit.Code,b.MembershipBenefit.Name,b.MembershipBenefit.IsActive,b.GrantType.ToString(),b.Quantity)).ToListAsync(ct);
    private async Task<MembershipPlanDetails> DetailsAsync(Guid id,CancellationToken ct)=>new(await Summaries(db.MembershipPlans.AsNoTracking().Where(p=>p.Id==id)).SingleAsync(ct),
        await db.MembershipPlans.Where(p=>p.Id==id).Select(p=>p.CreatedAt).SingleAsync(ct),await BenefitsAsync(id,ct),await db.MembershipPlanEligibleMembers.CountAsync(e=>e.MembershipPlanId==id,ct));
    public async Task<MembershipPlanListResult> GetPlansAsync(MembershipPlanListRequest r,CancellationToken ct=default)
    {
        var user=await RequireAsync("MembershipPlans.View",ct);Page(r);
        if(r.PlanType.HasValue&&!Enum.IsDefined(r.PlanType.Value)||r.Mode.HasValue&&!Enum.IsDefined(r.Mode.Value))throw new CampionsSystem.BLL.Operations.BusinessRuleException("Invalid plan filters.");
        if((r.IncludeInternal||r.PlanType==PlanCategory.Employee)&&!user.Permissions.Contains("MembershipPlans.Manage"))throw new UnauthorizedAccessException();
        var q=Scoped(user);
        if(!r.IncludeInternal&&r.PlanType!=PlanCategory.Employee)q=q.Where(p=>p.PlanCategory!=PlanCategory.Employee);
        if(r.BranchId.HasValue){BranchAccessService.EnsureScope(user,r.BranchId);q=q.Where(p=>p.BranchId==r.BranchId);}
        if(r.CityId.HasValue)q=q.Where(p=>p.CityId==r.CityId);
        if(r.PlanType.HasValue)q=q.Where(p=>p.PlanCategory==r.PlanType);

```

Lines 39–49

```csharp

        if(r.FreelanceCoachEmployeeId.HasValue)q=q.Where(p=>p.FreelanceCoachEmployeeId==r.FreelanceCoachEmployeeId);
        if(r.IsActive.HasValue)q=q.Where(p=>p.IsActive==r.IsActive);
        if(!string.IsNullOrWhiteSpace(r.Search)){var s=r.Search.Trim().ToLowerInvariant();q=q.Where(p=>p.Name.ToLower().Contains(s));}
        return new(await Summaries(q.OrderBy(p=>p.Name).ThenBy(p=>p.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)).ToListAsync(ct),await q.CountAsync(ct),r.Page,r.PageSize);
    }
    public async Task<MembershipPlanDetails> GetPlanByIdAsync(Guid id,CancellationToken ct=default)
    {
        var user=await RequireAsync("MembershipPlans.View",ct);var p=await VisiblePlanAsync(id,user,ct);
        if(p.PlanCategory==PlanCategory.Employee&&!user.Permissions.Contains("MembershipPlans.Manage"))throw new BranchNotFoundException("Customer plan not found.");
        return await DetailsAsync(id,ct);
    }

```
