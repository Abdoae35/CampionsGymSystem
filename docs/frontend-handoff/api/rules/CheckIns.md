# CheckIns rule/validation evidence

Completed source; deployment/configuration prerequisites are separate. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/CheckIns/CheckInService.Queries.cs

Lines 9–70

```csharp

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

```

Lines 72–82

```csharp

    public async Task<BorrowedDebtHistory> GetBorrowedDebtHistoryAsync(Guid memberId,BorrowedDebtHistoryRequest r,CancellationToken ct=default)
    {
        Pagination(r.Page,r.PageSize);var user=await ViewAsync(r.BranchId,ct);var q=db.BorrowedGymSessions.AsNoTracking().Where(x=>x.MemberId==memberId);
        if(!user.Permissions.Contains("Branches.AccessAll")||r.BranchId!=null)
        {var branch=r.BranchId??user.CurrentBranchId??throw new UnauthorizedAccessException();q=q.Where(x=>x.BranchId==branch);}
        if(r.ExpiredSubscriptionId!=null)q=q.Where(x=>x.ExpiredSubscriptionId==r.ExpiredSubscriptionId);
        var count=await q.CountAsync(ct);var debts=await q.OrderByDescending(x=>x.BorrowedAt).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize).ToListAsync(ct);
        var ids=debts.Select(d=>d.Id).ToArray();var events=await db.BorrowedDebtSettlementEvents.AsNoTracking().Include(e=>e.Grant)
            .Where(e=>ids.Contains(e.BorrowedGymSessionId)).OrderBy(e=>e.Sequence).ToListAsync(ct);
        var items=debts.Select(d=>
        {

```

## CampionsSystem.BLL/CheckIns/CheckInPermissionSetup.cs

Lines 11–16

```csharp

        await using var tx=db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        if(managementRoleId.HasValue && !await db.RolePermissions.AnyAsync(x=>x.RoleId==managementRoleId
            && x.Permission.Code=="Branches.AccessAll" && x.Permission.IsActive,ct))
            throw new InvalidOperationException("Select an existing role with active global branch capability.");
        var permissions=await db.Permissions.Where(x=>CheckInPermissions.Codes.Contains(x.Code)).ToListAsync(ct);
        foreach(var code in CheckInPermissions.Codes.Where(x=>permissions.All(p=>p.Code!=x)))

```

## CampionsSystem.BLL/CheckIns/CheckInService.Trials.cs

Lines 9–14

```csharp

    // Existing issued invitation identity + canonical phone; no new token, issuance or Member-registration workflow.
    public async Task<TrialCheckInResult> RedeemTrialAsync(RedeemTrialRequest r,CancellationToken ct=default)
    {
        if(r.OperationId==Guid.Empty || r.InvitationId==Guid.Empty)throw new BusinessRuleException("Invitation and operation identities are required.");
        var context=await ContextAsync(CheckInPermissions.Create,r.BranchId,ct);
        return await TransactionAsync(async ()=>

```

Lines 17–22

```csharp

                .AsNoTracking().Include(x=>x.City).SingleAsync(ct);
            var day=await db.BusinessDays.FromSqlInterpolated($"SELECT * FROM \"BusinessDays\" WHERE \"BranchId\"={branch.Id} AND \"Status\"='Open' FOR SHARE")
                .AsNoTracking().SingleOrDefaultAsync(ct);
            if(day!=null && (day.OpenedAt>access.Now || day.BusinessDate>access.Today))throw new BusinessRuleException("Invalid open BusinessDay.");
            var phone=PhoneNormalizer.Normalize(r.Phone,branch.CountryCallingCode);
            await PhoneIdentity.LockAsync(db,phone,ct);

```

Lines 29–34

```csharp

            TrialCheckInResult Denied(string reason)=>new(false,reason,r.InvitationId,null);
            if(invitation==null)return Denied(TrialReasons.InvitationNotFound);
            await db.Entry(invitation).ReloadAsync(ct);
            if(invitation.InvitedByMemberId!=issued?.InvitedByMemberId)throw new BusinessRuleException("Invitation changed; reload before redemption.");
            if(invitation.BranchId!=branch.Id)return Denied(TrialReasons.InvitationBranchMismatch);
            if(invitation.InviteePhoneNormalized!=phone || PhoneNormalizer.Normalize(invitation.InviteePhone,branch.CountryCallingCode)!=phone

```

Lines 38–47

```csharp

            if(retry!=null)
            {
                if(invitation.RedeemedCheckInId!=retry.Id || retry.CheckInType!=CheckInType.Trial || retry.BranchId!=branch.Id || retry.CreatedByUserId!=context.Actor.UserId)
                    throw new BusinessRuleException("Operation identity belongs to a different Check-In request.");
                return new(true,AccessReasons.Allowed,invitation.Id,Details(retry,member) with{MemberName=invitation.InviteeName});
            }
            if(!branch.IsActive || !branch.City.IsActive)return Denied(AccessReasons.BranchInactive);
            var date=day?.BusinessDate??access.Today;
            if(await db.BranchClosures.AnyAsync(x=>x.BranchId==branch.Id && x.AppliesToGym && x.StartDate<=date && x.EndDate>=date,ct))return Denied(AccessReasons.BranchClosed);
            if(member is {IsActive:false})return Denied(AccessReasons.MemberInactive);

```

Lines 63–72

```csharp

                usage=new(){MemberSubscriptionBenefitId=benefitId,QuantityUsed=1,BranchId=branch.Id,UsedAt=access.Now,CreatedAt=access.Now,CreatedByUserId=context.Actor.UserId,
                    Notes=$"Guest invitation {invitation.Id} redeemed"};db.Add(usage);
            }
            var row=new CheckIn{OperationId=r.OperationId,MemberId=memberId,BranchId=branch.Id,BusinessDate=date,BusinessDayId=day?.Id,
                CheckInType=CheckInType.Trial,CheckInMethod=CheckInMethod.Manual,CheckedInAt=access.Now,CreatedByUserId=context.Actor.UserId};
            db.Add(row);invitation.InviteeMemberId=memberId;invitation.Status=GuestInvitationStatus.Redeemed;invitation.RedeemedAt=access.Now;invitation.RedeemedCheckIn=row;
            access.Audit(context.Actor.UserId,branch.Id,"TrialCheckIn.Created",nameof(CheckIn),row.Id,null,
                new{InvitationId=invitation.Id,BenefitUsageId=usage?.Id,row.MemberId,row.BusinessDate,row.BusinessDayId,row.CheckInType,row.CheckInMethod,row.OperationId});
            await db.SaveChangesAsync(ct);
            return new(true,AccessReasons.Allowed,invitation.Id,Details(row,member) with{MemberName=invitation.InviteeName});

```

## CampionsSystem.BLL/CheckIns/CheckInContracts.cs

Lines 4–35

```csharp


public static class CheckInPermissions
{
    public const string View = "CheckIns.View";
    public const string Create = "CheckIns.Create";
    public static readonly string[] Codes = [View, Create];
}
public static class AccessReasons
{
    public const string InstallmentOverdue = "InstallmentOverdue";
    public const string PendingFirstCheckIn = "PendingFirstCheckIn";
    public const string Allowed = "Allowed";
    public const string MemberNotFound = "MemberNotFound";
    public const string MemberInactive = "MemberInactive";
    public const string NoGymEntitlement = "NoGymEntitlement";
    public const string SubscriptionNotStarted = "SubscriptionNotStarted";
    public const string SubscriptionExpired = "SubscriptionExpired";
    public const string SubscriptionFrozen = "SubscriptionFrozen";
    public const string SubscriptionCancelled = "SubscriptionCancelled";
    public const string SubscriptionSuspended = "SubscriptionSuspended";
    public const string NoSessionsRemaining = "NoSessionsRemaining";
    public const string AccessCityMismatch = "AccessCityMismatch";
    public const string BranchClosed = "BranchClosed";
    public const string BranchInactive = "BranchInactive";
    public const string BorrowLimitReached = "BorrowLimitReached";
    public const string BorrowConfirmationRequired = "BorrowConfirmationRequired";
    public const string BorrowCycleRenewed = "BorrowCycleRenewed";
    public const string BorrowNotEligible = "BorrowNotEligible";
    public const string EmployeeEntitlementInactive = "EmployeeEntitlementInactive";
    public const string AmbiguousEntitlement = "AmbiguousEntitlement";
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]

```

Lines 38–47

```csharp

public sealed record CreateCheckInRequest(Guid OperationId, CheckInMethod Method, Guid? MemberId = null,
    string? MemberCode = null, bool ConfirmBorrow = false, Guid? BranchId = null);
public sealed record AccessDecision(bool Allowed, string ReasonCode, Guid? MemberId, string? MemberCode,
    string? MemberName, Guid BranchId, DateOnly BusinessDate, Guid? BusinessDayId,
    Guid? SubscriptionId, string? SubscriptionMode, int? RemainingSessions, bool BorrowAvailable,
    int BorrowedUsed, int BorrowedRemaining, bool CanActivateOnCheckIn = false);
public sealed record CheckInDetails(Guid Id, Guid? OperationId, Guid? MemberId, string? MemberCode,
    string? MemberName, Guid BranchId, DateOnly? BusinessDate, Guid? BusinessDayId,
    DateTimeOffset OccurredAt, string Type, string Method, Guid? SubscriptionId, bool SessionConsumed,
    int? SessionsBefore, int? SessionsAfter, Guid ActorId);

```

Lines 50–56

```csharp

{
    public Guid? BranchId { get; set; }
    public Guid? MemberId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public CheckInType? Type { get; set; }
    public CheckInMethod? Method { get; set; }

```

Lines 85–95

```csharp

public sealed record TrialCheckInResult(bool Allowed,string ReasonCode,Guid InvitationId,CheckInDetails? CheckIn);
public static class TrialReasons
{
    public const string InvitationNotFound="InvitationNotFound";
    public const string InvitationUnavailable="InvitationUnavailable";
    public const string InvitationBranchMismatch="InvitationBranchMismatch";
    public const string InvitationIdentityMismatch="InvitationIdentityMismatch";
    public const string TrialAlreadyUsed="TrialAlreadyUsed";
    public const string InvitationBenefitInvalid="InvitationBenefitInvalid";
    public const string InvitationBenefitExhausted="InvitationBenefitExhausted";
}

```

## CampionsSystem.BLL/CheckIns/CheckInService.cs

Lines 14–35

```csharp

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

```

Lines 37–45

```csharp

    public async Task<AccessDecision> EvaluateAsync(AccessRequest r,CancellationToken ct=default)
    {
        var context=await ContextAsync(CheckInPermissions.Create,r.BranchId,ct);
        var date=await BusinessDateAsync(context.Branch.Id,ct);
        return await DecideAsync(await IdentifyAsync(r.MemberId,r.MemberCode,ct),context.Branch,date.Date,date.DayId,ct);
    }
    private async Task<AccessDecision> DecideAsync(Member? member,GymBranch branch,DateOnly date,Guid? day,CancellationToken ct)
    {
        AccessDecision Result(string reason,SubscriptionSummary? s=null,bool borrow=false,int used=0)

```

Lines 57–62

```csharp

        if(currentPeriods.Length>1)
        {
            var usable=currentPeriods.Where(x=>!(x.Status=="Expired" && x.Mode==nameof(AccessMode.SessionBased) && x.RemainingSessions<=0)).ToArray();
            currentPeriods=usable.Length>0?usable:[currentPeriods.OrderByDescending(x=>x.StartDate).First()];
        }
        if(currentPeriods.Length>1)return Result(AccessReasons.AmbiguousEntitlement);

```

Lines 94–99

```csharp

            var renewed=await db.MembershipSubscriptionTransactions.AnyAsync(x=>x.RenewedFromSubscriptionId==s.Id && x.MemberSubscriptionId!=s.Id
                    && !db.SubscriptionGrantCancellations.Any(c=>c.GrantId==x.Id),ct)
                || await db.BorrowedGymSessions.AnyAsync(x=>x.ExpiredSubscriptionId==s.Id
                    && !(db.BorrowedDebtSettlementEvents.Where(e=>e.BorrowedGymSessionId==x.Id).OrderByDescending(e=>e.Sequence)
                        .Select(e=>(bool?)e.IsReversal).FirstOrDefault()??x.Status==BorrowedSessionStatus.Outstanding),ct);
            if(renewed)return Result(AccessReasons.BorrowCycleRenewed,s,false,used);

```

Lines 102–107

```csharp

        var stopped=facts.FirstOrDefault(x=>x.StartDate<=date && x.Status is "Cancelled" or "SuspendedForInstallment");
        if(stopped!=null)return Result(stopped.Status=="Cancelled"?AccessReasons.SubscriptionCancelled
            :await subscriptions.IsSubscriptionOverdueAsync(stopped.Id,date,ct)?AccessReasons.InstallmentOverdue:AccessReasons.SubscriptionSuspended,stopped);
        var future=facts.Where(x=>x.StartDate>date).OrderBy(x=>x.StartDate).FirstOrDefault();
        return Result(future!=null?AccessReasons.SubscriptionNotStarted:AccessReasons.NoGymEntitlement,future);
    }

```

## CampionsSystem.BLL/CheckIns/CheckInService.Entry.cs

Lines 24–35

```csharp

        await db.Entry(sub).ReloadAsync(ct);return sub;
    }
    private static CheckInDetails Details(CheckIn row,Member? member)=>new(row.Id,row.OperationId,row.MemberId,row.MemberId==member?.Id?member?.MemberCode:null,
        row.MemberId==member?.Id?member?.FullName:null,row.BranchId,row.BusinessDate,row.BusinessDayId,row.CheckedInAt,row.CheckInType.ToString(),row.CheckInMethod.ToString(),
        row.MemberSubscriptionId,row.SessionConsumed,row.SessionsBefore,row.SessionsAfter,row.CreatedByUserId);
    private async Task<CheckInResult> RecordedResultAsync(CheckIn row,Member member,CancellationToken ct,bool borrowAvailable=false)
    {
        var sub=await db.MemberSubscriptions.AsNoTracking().SingleAsync(x=>x.Id==row.MemberSubscriptionId,ct);
        var used=row.CheckInType==CheckInType.BorrowedGym?await db.BorrowedGymSessions.CountAsync(x=>x.ExpiredSubscriptionId==sub.Id,ct):0;
        return new(new(true,AccessReasons.Allowed,member.Id,member.MemberCode,member.FullName,row.BranchId,row.BusinessDate!.Value,row.BusinessDayId,
            sub.Id,sub.AccessMode.ToString(),row.CheckInType==CheckInType.BorrowedGym?sub.RemainingSessions:row.SessionsAfter,borrowAvailable,used,borrowAvailable?Math.Max(0,2-used):0),Details(row,member));
    }

```

Lines 37–42

```csharp

    {
        if(r.OperationId==Guid.Empty || r.Method is not (CheckInMethod.Manual or CheckInMethod.Scanner)
            || r.Method==CheckInMethod.Scanner && string.IsNullOrWhiteSpace(r.MemberCode))
            throw new BusinessRuleException("A nonempty operation identity and Manual/Scanner method with matching identification are required.");
        var context=await ContextAsync(CheckInPermissions.Create,r.BranchId,ct);
        return await TransactionAsync(async ()=>

```

Lines 45–52

```csharp

                .AsNoTracking().Include(x=>x.City).SingleAsync(ct);
            var day=await db.BusinessDays.FromSqlInterpolated($"SELECT * FROM \"BusinessDays\" WHERE \"BranchId\"={branch.Id} AND \"Status\"='Open' FOR SHARE")
                .AsNoTracking().SingleOrDefaultAsync(ct);
            if(day!=null && (day.OpenedAt>access.Now || day.BusinessDate>access.Today))throw new BusinessRuleException("Invalid open BusinessDay.");
            var member=await IdentifyAsync(r.MemberId,r.MemberCode,ct);
            if(member==null)return new CheckInResult(await DecideAsync(null,branch,day?.BusinessDate??access.Today,day?.Id,ct),null);
            member=await LockMemberAsync(member.Id,ct);
            var retry=await db.CheckIns.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);

```

Lines 54–71

```csharp

            {
                if(retry.MemberId!=member.Id || retry.BranchId!=branch.Id || retry.CheckInMethod!=r.Method
                    || retry.CreatedByUserId!=context.Actor.UserId || (retry.CheckInType==CheckInType.BorrowedGym)!=r.ConfirmBorrow)
                    throw new BusinessRuleException("Operation identity belongs to a different Check-In request.");
                var borrowing=retry.CheckInType==CheckInType.BorrowedGym
                    ?await DecideAsync(member,branch,day?.BusinessDate??access.Today,day?.Id,ct):null;
                var borrowAvailable=borrowing is {BorrowAvailable:true} && borrowing.SubscriptionId==retry.MemberSubscriptionId;
                return await RecordedResultAsync(retry,member,ct,borrowAvailable);
            }
            var decision=await DecideAsync(member,branch,day?.BusinessDate??access.Today,day?.Id,ct);
            if (!r.ConfirmBorrow && decision.CanActivateOnCheckIn)
            {
                var pending = await subscriptions.PendingFirstEntrySaleAsync(member.Id,branch.CityId,decision.BusinessDate,ct);
                await subscriptions.ActivatePaidSaleAsync(pending!.Value,context.Actor.UserId,true,decision.BusinessDate,ct,day?.Id);
                decision = await DecideAsync(member,branch,decision.BusinessDate,day?.Id,ct);
                if (!decision.Allowed) throw new BusinessRuleException("Paid first-entry activation cannot complete without a valid gym entry: " + decision.ReasonCode);
            }
            if(r.ConfirmBorrow && decision.BorrowAvailable)

```

Lines 75–84

```csharp

            var sub=await LockSubscriptionAsync(decision.SubscriptionId!.Value,ct);
            var before=sub.RemainingSessions;
            if(sub.AccessMode==AccessMode.SessionBased){sub.RemainingSessions--;sub.Version++;}
            var row=new CheckIn{OperationId=r.OperationId,MemberId=member.Id,BranchId=branch.Id,BusinessDate=decision.BusinessDate,BusinessDayId=day?.Id,
                MemberSubscriptionId=sub.Id,CheckInType=CheckInType.Gym,CheckInMethod=r.Method,SessionConsumed=sub.AccessMode==AccessMode.SessionBased,
                SessionsBefore=before,SessionsAfter=sub.RemainingSessions,CheckedInAt=access.Now,CreatedByUserId=context.Actor.UserId};
            db.Add(row);access.Audit(context.Actor.UserId,branch.Id,"CheckIn.Created",nameof(CheckIn),row.Id,
                new{RemainingSessions=before},new{row.MemberId,row.MemberSubscriptionId,row.BusinessDate,row.BusinessDayId,row.CheckInType,row.CheckInMethod,row.OperationId,RemainingSessions=sub.RemainingSessions});
            await db.SaveChangesAsync(ct);return await RecordedResultAsync(row,member,ct);
        },ct);

```

## CampionsSystem.BLL/CheckIns/CheckInService.Borrowing.cs

Lines 10–21

```csharp

        var source=await LockSubscriptionAsync(decision.SubscriptionId!.Value,ct);
        source.Version++;
        var entry=new CheckIn{OperationId=request.OperationId,MemberId=member.Id,BranchId=decision.BranchId,
            BusinessDate=decision.BusinessDate,BusinessDayId=decision.BusinessDayId,MemberSubscriptionId=source.Id,
            CheckInType=CheckInType.BorrowedGym,CheckInMethod=request.Method,CheckedInAt=access.Now,CreatedByUserId=actor};
        var debt=new BorrowedGymSession{MemberId=member.Id,ExpiredSubscriptionId=source.Id,CheckIn=entry,
            BranchId=decision.BranchId,BorrowedAt=entry.CheckedInAt,Status=BorrowedSessionStatus.Outstanding};
        db.Add(debt);
        access.Audit(actor,decision.BranchId,"CheckIn.Borrowed",nameof(CheckIn),entry.Id,
            after:new{entry.MemberId,entry.MemberSubscriptionId,entry.OperationId,entry.BusinessDate,entry.BusinessDayId,entry.CheckInMethod,DebtId=debt.Id});
        access.Audit(actor,decision.BranchId,"BorrowedSessionDebt.Created",nameof(BorrowedGymSession),debt.Id,
            after:new{debt.MemberId,debt.ExpiredSubscriptionId,CheckInId=entry.Id,BorrowedUsed=decision.BorrowedUsed+1});

```
