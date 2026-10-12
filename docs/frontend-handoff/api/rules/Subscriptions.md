# Subscriptions rule/validation evidence

Baseline source, not new Private drafts. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Subscriptions/SubscriptionService.Cancellations.cs

Lines 7–12

```csharp

{
    internal async Task CancelSaleGrantAsync(SaleTransaction sale,Refund refund,Guid actor,CancellationToken ct)
    {
        if(db.Database.CurrentTransaction is null)throw new InvalidOperationException("Grant reversal requires the Finance transaction.");
        var grant=await db.MembershipSubscriptionTransactions.SingleOrDefaultAsync(g=>g.SaleTransactionId==sale.Id && g.OperationId!=null,ct);
        if(grant==null || await db.SubscriptionGrantCancellations.AnyAsync(c=>c.GrantId==grant.Id,ct))return;

```

Lines 17–40

```csharp

        if(!other)sub.Status=SubscriptionStatus.Cancelled;
        else
        {
            if(sub.EndDate==null || grant.PurchasedDurationDays<=0)throw new BusinessRuleException("Grant reversal requires reconciled customer duration history.");
            var date=await EffectiveBusinessDateAsync(sale.BranchId,ct);
            var priorRemoved=await db.SubscriptionGrantCancellations.Where(c=>c.Grant.MemberSubscriptionId==sub.Id
                && c.Grant.StartDateSnapshot<grant.StartDateSnapshot).SumAsync(c=>c.DaysRemoved,ct);
            var grantStart=PlusDays(grant.StartDateSnapshot??throw new BusinessRuleException("Grant date history is missing."),-priorRemoved);
            daysRemoved=Math.Clamp(grant.PurchasedDurationDays-Math.Max(0,date.DayNumber-grantStart.DayNumber),0,grant.PurchasedDurationDays);
            sub.EndDate=PlusDays(sub.EndDate.Value,-daysRemoved);
            if(sub.EndDate<sub.StartDate)throw new BusinessRuleException("Grant reversal conflicts with existing entitlement history.");
            if(sub.AccessMode==AccessMode.SessionBased)
            {
                // Remaining pooled sessions are assigned newest grant first, equivalent to consuming older grants first.
                var live=await db.MembershipSubscriptionTransactions.Where(g=>g.MemberSubscriptionId==sub.Id
                    && !db.SubscriptionGrantCancellations.Any(c=>c.GrantId==g.Id)).OrderByDescending(g=>g.StartDateSnapshot).ThenByDescending(g=>g.CreatedAt).ThenByDescending(g=>g.Id).ToListAsync(ct);
                var remaining=sub.RemainingSessions!.Value;var remove=0;
                foreach(var g in live)
                {
                    var settled=await db.BorrowedGymSessions.CountAsync(d=>d.SettledByTransactionId==g.Id
                        || db.BorrowedDebtSettlementEvents.Any(e=>e.BorrowedGymSessionId==d.Id && e.GrantId==g.Id && !e.IsReversal),ct);
                    var available=Math.Min(remaining,(g.PurchasedSessions??throw new BusinessRuleException("Session grant history is missing."))-settled);
                    if(g.Id==grant.Id){remove=available;break;}remaining-=available;
                }

```

Lines 44–49

```csharp

            var next=await db.MemberSubscriptions.SingleOrDefaultAsync(s=>s.FollowsSubscriptionId==sub.Id && s.Status!=SubscriptionStatus.Cancelled,ct);
            if(next!=null)
            {
                var nextDate=await EffectiveBusinessDateAsync(next.PurchasedAtBranchId,ct);
                if(next.StartDate>nextDate)
                {

```

Lines 59–64

```csharp

        var debts=await db.BorrowedGymSessions.Where(d=>d.MemberId==sale.MemberId).ToListAsync(ct);
        foreach(var debt in debts)
        {
            var last=await db.BorrowedDebtSettlementEvents.Where(e=>e.BorrowedGymSessionId==debt.Id).OrderByDescending(e=>e.Sequence).FirstOrDefaultAsync(ct);
            if(last!=null ? last.IsReversal || last.GrantId!=grant.Id : debt.Status!=BorrowedSessionStatus.Settled || debt.SettledByTransactionId!=grant.Id)continue;
            db.Add(new BorrowedDebtSettlementEvent{BorrowedGymSessionId=debt.Id,GrantId=grant.Id,IsReversal=true,Refund=refund,

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.Employees.cs

Lines 8–19

```csharp

{
    public async Task<SubscriptionGrantResult> StartEmployeeEntitlementAsync(Guid employeeId, EmployeeEntitlementRequest r, CancellationToken ct = default)
    {
        var actor = await RequireAsync(SubscriptionPermissions.Manage, null, false, ct);
        await RequireAsync("Employees.Manage", null, false, ct);
        return await EmployeeAssignmentTransaction.RunAsync(db, employeeId, async locked =>
        {
            var branch = await new EmployeeBranchResolver(db).ResolveAsync(employeeId, access.Today, ct)
                ?? throw new BusinessRuleException("Employee requires one current active branch assignment.");
            BranchAccessService.EnsureScope(actor, branch.Id);
            var plan = await LockPlanAsync(r.PlanId, branch.Id, ct);
            var member = await LockMemberAsync(r.MemberId, ct);

```

Lines 21–37

```csharp

            if (retry is not null)
            {
                if (!await db.MemberSubscriptions.AnyAsync(x => x.Id == retry.SubscriptionId && x.EmployeeId == employeeId, ct))
                    throw new BusinessRuleException("Operation identity belongs to another Employee.");
                return retry;
            }
            if (locked.EmploymentStatus != EmploymentStatus.Active || locked.HireDate > access.Today || locked.TerminationDate < access.Today
                || !member.IsActive || plan.PlanCategory != PlanCategory.Employee || plan.Price != 0 || plan.AccessMode != AccessMode.Unlimited
                || plan.DurationDays != 0 || plan.SessionCount != null || locked.MemberId != null && locked.MemberId != member.Id)
                throw new BusinessRuleException("Active linked Employee/Member and free Unlimited Employee plan required.");
            if (await db.Employees.AnyAsync(x => x.Id != employeeId && x.MemberId == member.Id, ct))
                throw new BusinessRuleException("Member is linked to another Employee.");
            if (await db.MemberSubscriptions.AnyAsync(x => (x.EmployeeId == employeeId || x.MemberId == member.Id)
                && x.Status != SubscriptionStatus.Cancelled && (x.EndDate == null || x.EndDate >= access.Today), ct))
                throw new BusinessRuleException("An existing entitlement period must finish before a new Employee period can start.");
            var employee = await db.Employees.SingleAsync(x => x.Id == employeeId, ct); await db.Entry(employee).ReloadAsync(ct);
            if (employee.MemberId == null)

```

Lines 53–58

```csharp

    internal async Task CloseEmployeeEntitlementsAsync(Guid employeeId, Guid actorId, string? reason, CancellationToken ct)
    {
        var memberIds = await db.MemberSubscriptions.Where(x => x.EmployeeId == employeeId && x.EndDate == null)
            .Select(x => x.MemberId).Distinct().OrderBy(x => x).ToListAsync(ct);
        foreach (var memberId in memberIds)
        {

```

Lines 65–70

```csharp

                sub.Status = access.Today < sub.StartDate ? SubscriptionStatus.Cancelled : SubscriptionStatus.Expired;
                sub.Version++;
                access.Audit(actorId,sub.PurchasedAtBranchId,"Subscription.EmployeeEntitlementEnded",nameof(MemberSubscription),sub.Id,
                    new { EndDate=(DateOnly?)null },new { sub.MemberId,employeeId,sub.EndDate,sub.Status,Reason=reason });
            }
        }

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.Grants.cs

Lines 27–67

```csharp

    private async Task<Member> LockMemberAsync(Guid id, CancellationToken ct)
    {
        var member = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={id} FOR UPDATE")
            .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Member not found.");
        await db.Entry(member).ReloadAsync(ct); return member;
    }
    private async Task<MembershipPlan> LockPlanAsync(Guid id, Guid branch, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FromSqlInterpolated($"SELECT * FROM \"MembershipPlans\" WHERE \"Id\"={id} FOR UPDATE")
            .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Plan not found.");
        await db.Entry(plan).ReloadAsync(ct);
        if (!plan.IsActive || plan.BranchId != branch || !await db.GymBranches.AnyAsync(x => x.Id == branch && x.IsActive, ct)
            || !await db.Cities.AnyAsync(x => x.Id == plan.CityId && x.IsActive, ct))
            throw new BusinessRuleException("Select an active plan from the originating branch with active access city.");
        return plan;
    }
    private async Task<(DateOnly Date, Guid? DayId)> GrantDateAsync(Guid branch, CancellationToken ct)
    {
        var day = await db.BusinessDays.FromSqlInterpolated($"SELECT * FROM \"BusinessDays\" WHERE \"BranchId\"={branch} AND \"Status\"='Open' FOR SHARE")
            .AsNoTracking().SingleOrDefaultAsync(ct);
        if (day is not null && (day.OpenedAt > access.Now || day.BusinessDate > access.Today))
            throw new BusinessRuleException("Invalid open business day.");
        return (day?.BusinessDate ?? access.Today, day?.Id);
    }
    private async Task<SubscriptionGrantResult?> RetryGrantAsync(Guid operation, Guid member, Guid plan, Guid branch,
        Guid? source, CancellationToken ct)
    {
        if (operation == Guid.Empty) throw new BusinessRuleException("A nonempty operation identity is required.");
        var prior = await db.MembershipSubscriptionTransactions.AsNoTracking().Include(x => x.MemberSubscription)
            .SingleOrDefaultAsync(x => x.OperationId == operation, ct);
        if (prior is null) return null;
        if (prior.MemberSubscription.MemberId != member || prior.MembershipPlanId != plan
            || prior.MemberSubscription.PurchasedAtBranchId != branch || prior.RenewedFromSubscriptionId != source)
            throw new BusinessRuleException("Operation identity already belongs to another request.");
        return new(prior.MemberSubscriptionId, prior.Id, prior.MemberSubscription.StartDate > await EffectiveBusinessDateAsync(branch, ct));
    }
    private async Task<MembershipSubscriptionTransaction> SnapshotAsync(MemberSubscription subscription, MembershipPlan plan,
        Guid operation, Guid actor, Guid? businessDay, DateOnly grantStart, SubscriptionTransactionType type,
        MemberSubscription? source, DateOnly? previousEnd, int? previousSessions, CancellationToken ct, SaleTransaction? sale = null)
    {
        var grant = new MembershipSubscriptionTransaction

```

Lines 87–97

```csharp

        else
        {
            var configs = await db.MembershipPlanBenefits.AsNoTracking().Where(x => x.MembershipPlanId == plan.Id)
                .OrderBy(x => x.MembershipBenefitId).ToListAsync(ct);
            foreach (var config in configs)
            {
                var benefit = await db.MembershipBenefits.FromSqlInterpolated($"SELECT * FROM \"MembershipBenefits\" WHERE \"Id\"={config.MembershipBenefitId} FOR SHARE")
                    .AsNoTracking().SingleAsync(ct);
                if (!benefit.IsActive) throw new BusinessRuleException("Plan contains an inactive benefit; update its configuration before granting.");
                db.Add(new MemberSubscriptionBenefit { MembershipSubscriptionTransaction = grant,
                    MembershipBenefitId = benefit.Id, BenefitCodeSnapshot = benefit.Code, BenefitNameSnapshot = benefit.Name,

```

Lines 103–112

```csharp

    // Protected administration boundary; Finance uses the internal verified-paid boundary. No raw customer grant HTTP route.
    public async Task<SubscriptionGrantResult> CreateCustomerGrantAsync(SubscriptionGrantCommand r, CancellationToken ct = default)
    {
        var actor = await RequireAsync(SubscriptionPermissions.Manage, r.BranchId, true, ct);
        return await CreateCustomerGrantCoreAsync(r, actor.UserId, null, null, ct);
    }
    private async Task<SubscriptionGrantResult> CreateCustomerGrantCoreAsync(SubscriptionGrantCommand r, Guid actor,
        SaleTransaction? sale, DateOnly? requestedStart, CancellationToken ct, (DateOnly Date, Guid? Day)? activationDate = null)
    {
        return await TransactionAsync(async () =>

```

Lines 116–126

```csharp

            var retry = await RetryGrantAsync(r.OperationId, r.MemberId, r.PlanId, r.BranchId, null, ct);
            if (retry is not null) return retry;
            if (sale == null) await ValidateCustomerAsync(member, plan, ct);
            else if (!member.IsActive) throw new BusinessRuleException("Member must be active to activate purchased entitlement.");
            var borrowedDebt=await OutstandingBorrowedDebtAsync(member.Id,plan,ct);
            var (date, day) = activationDate ?? await GrantDateAsync(r.BranchId, ct);
            if (await db.MemberSubscriptions.AnyAsync(x => x.MemberId == member.Id && x.Status != SubscriptionStatus.Cancelled && (!x.IsStandaloneVisit || x.RemainingSessions > 0)
                && (x.EndDate == null || x.EndDate >= date), ct))
                throw new BusinessRuleException("Member already has an open or future entitlement; use the renewal integration.");
            var start = requestedStart ?? date;
            var sub = new MemberSubscription { MemberId = member.Id, MembershipPlanId = plan.Id, PurchasedAtBranchId = r.BranchId,

```

## CampionsSystem.BLL/Subscriptions/SubscriptionPermissionSetup.cs

Lines 11–16

```csharp

        await using var tx=db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        if(managementRoleId.HasValue && !await db.RolePermissions.AnyAsync(x=>x.RoleId==managementRoleId
            && x.Permission.Code=="Branches.AccessAll" && x.Permission.IsActive,ct))
            throw new InvalidOperationException("Select an existing role with active global branch capability.");
        var permissions=await db.Permissions.Where(x=>SubscriptionPermissions.Codes.Contains(x.Code)).ToListAsync(ct);
        foreach(var code in SubscriptionPermissions.Codes.Where(x=>permissions.All(p=>p.Code!=x)))

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.StandaloneSales.cs

Lines 6–18

```csharp

public sealed partial class SubscriptionService
{
    private async Task<SubscriptionGrantResult?> ActivateStandaloneSaleAsync(SaleTransaction sale,Guid actor,bool firstEntry,
        DateOnly? entranceDate,Guid? entranceDay,DateOnly date,CancellationToken ct)
    {
        var plan=SoldPlan(sale);
        if(plan.AccessMode!=AccessMode.SessionBased || plan.SessionCount!=1)throw new BusinessRuleException("A standalone visit grants exactly one paid Gym entry.");
        if(await Evaluated(date).AnyAsync(x=>x.Subscription.MemberId==sale.MemberId && x.Status!=SubscriptionStatus.Cancelled
            && (x.Subscription.EndDate==null || x.Subscription.EndDate>=date)
            && !(x.Status==SubscriptionStatus.Expired && x.Subscription.AccessMode==AccessMode.SessionBased && x.Subscription.RemainingSessions<=0),ct))
            throw new BusinessRuleException("Standalone visits cannot overlap another current/future gym entitlement.");
        if(sale.ActivationMode==SubscriptionActivationMode.FirstCheckIn && !firstEntry)
        {

```

Lines 20–25

```csharp

                access.Audit(actor,sale.BranchId,"Subscription.PendingFirstCheckIn",nameof(SaleTransaction),sale.Id,after:new{sale.MemberId,Standalone=true});
            await db.SaveChangesAsync(ct);return null;
        }
        var member=await db.Members.SingleAsync(x=>x.Id==sale.MemberId,ct);if(!member.IsActive)throw new BusinessRuleException("Member must be active.");
        var start=sale.ActivationMode==SubscriptionActivationMode.SpecificDate && sale.RequestedStartDate>date?sale.RequestedStartDate.Value:date;
        var day=entranceDate.HasValue?entranceDay:(await GrantDateAsync(sale.BranchId,ct)).DayId;

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.PaidSales.cs

Lines 7–14

```csharp

{
    private static MembershipPlan SoldPlan(SaleTransaction sale)
    {
        var s = sale.MembershipSnapshot ?? throw new BusinessRuleException("Sale entitlement snapshot is missing.");
        if (s.PlanCategory == PlanCategory.Employee || s.DurationDays <= 0 || (long)s.DurationDays + sale.ExtraDaysSnapshot > int.MaxValue)
            throw new BusinessRuleException("Sale contains invalid paid entitlement terms.");
        return new MembershipPlan { Id = s.MembershipPlanId, BranchId = sale.BranchId, CityId = s.CityId, Name = s.PlanName,
            PlanCategory = s.PlanCategory, AccessMode = s.AccessMode, DurationDays = s.DurationDays + sale.ExtraDaysSnapshot,

```

Lines 28–54

```csharp

            : db.Payments.Where(p => p.SaleTransactionId == s.Id && p.Status == PaymentStatus.Completed).Sum(p => p.Amount) >= s.NetAmount));
    private IQueryable<SaleTransaction> PendingSales(Guid member) => QualifiedSales().Where(s => s.MemberId == member
        && s.ActivationMode == SubscriptionActivationMode.FirstCheckIn && !db.MembershipSubscriptionTransactions.Any(g => g.SaleTransactionId == s.Id));
    internal async Task<SubscriptionGrantResult?> ActivatePaidSaleAsync(Guid saleId, Guid actor, bool firstEntry, DateOnly? entranceDate,
        CancellationToken ct, Guid? entranceDay = null)
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("Paid activation requires the financial/entrance transaction.");
        var memberId = await db.SaleTransactions.Where(x => x.Id == saleId).Select(x => x.MemberId).SingleAsync(ct);
        await LockMemberAsync(memberId, ct);
        var sale = await db.SaleTransactions.FromSqlInterpolated($"SELECT * FROM \"SaleTransactions\" WHERE \"Id\"={saleId} FOR UPDATE").SingleAsync(ct);
        await db.Entry(sale).ReloadAsync(ct);
        var prior = await db.MembershipSubscriptionTransactions.AsNoTracking().Include(x => x.MemberSubscription).SingleOrDefaultAsync(x => x.SaleTransactionId == saleId && x.OperationId != null, ct);
        if (prior != null) return new(prior.MemberSubscriptionId, prior.Id, prior.MemberSubscription.StartDate > (entranceDate ?? await EffectiveBusinessDateAsync(sale.BranchId,ct)));
        if (!await QualifiedSales().AnyAsync(x => x.Id == sale.Id, ct)) return null;
        if (await PendingSales(memberId).AnyAsync(x => x.Id != sale.Id,ct))
            throw new BusinessRuleException("Member already has a paid entitlement pending first Check-In.");
        sale.MembershipSnapshot = await db.SaleMembershipSnapshots.Include(x => x.Benefits).SingleOrDefaultAsync(x => x.SaleTransactionId == sale.Id,ct);
        var date = entranceDate ?? await EffectiveBusinessDateAsync(sale.BranchId,ct);
        if (sale.PurchaseMode == PurchaseMode.StandaloneSingleSession)
            return await ActivateStandaloneSaleAsync(sale,actor,firstEntry,entranceDate,entranceDay,date,ct);
        if (await db.MemberSubscriptions.AnyAsync(x=>x.MemberId==memberId && x.IsStandaloneVisit && x.Status!=SubscriptionStatus.Cancelled
            && x.EndDate>=date && x.RemainingSessions>0,ct)) throw new BusinessRuleException("Use or cancel the existing paid visit before activating a membership.");
        var periods = await Evaluated(date).Where(x => x.Subscription.MemberId == memberId && !x.Subscription.IsStandaloneVisit
            && x.Status != SubscriptionStatus.Cancelled && (x.Subscription.EndDate == null || x.Subscription.EndDate >= date)
            && !(x.Status == SubscriptionStatus.Expired && x.Subscription.AccessMode == AccessMode.SessionBased && x.Subscription.RemainingSessions <= 0)).Take(3).ToListAsync(ct);
        var currentPeriods = periods.Where(x => x.Subscription.StartDate <= date).ToList();
        var futurePeriods = periods.Where(x => x.Subscription.StartDate > date).ToList();

```

Lines 62–69

```csharp

            && current.Subscription.PurchasedAtBranchId == sale.BranchId && current.PlanType == soldPlan.PlanCategory;
        if (currentPeriods.Count > 1 || periods.Any(x => x.Subscription.EmployeeId != null)
            || (futurePeriods.Count > 0 && !canShiftSuccessor))
            throw new BusinessRuleException("Conflicting current/future entitlement must be resolved before paid activation.");
        if (current != null && current.Status is not (SubscriptionStatus.Active or SubscriptionStatus.Frozen or SubscriptionStatus.Expired))
            throw new BusinessRuleException("Current entitlement cannot be renewed in its present state.");
        var ongoing = current?.Status is SubscriptionStatus.Active or SubscriptionStatus.Frozen ? current : null;
        if (ongoing == null && sale.ActivationMode == SubscriptionActivationMode.FirstCheckIn && !firstEntry)

```

Lines 74–81

```csharp

        }
        var exhausted = current == null ? await Evaluated(date).Where(x=>x.Subscription.MemberId==memberId && !x.Subscription.IsStandaloneVisit
            && x.Subscription.EmployeeId==null && x.Status==SubscriptionStatus.Expired && x.Subscription.StartDate<=date && x.Subscription.EndDate>=date)
            .OrderByDescending(x=>x.Subscription.StartDate).ThenByDescending(x=>x.Subscription.CreatedAt).FirstOrDefaultAsync(ct) : null;
        var source = current?.Subscription ?? exhausted?.Subscription ?? await db.MemberSubscriptions.AsNoTracking().Where(x => x.MemberId == memberId && x.EmployeeId == null
            && !x.IsStandaloneVisit && x.Status != SubscriptionStatus.Cancelled && x.EndDate < date).OrderByDescending(x => x.EndDate).ThenByDescending(x=>x.CreatedAt).FirstOrDefaultAsync(ct);
        // Early renewals apply immediately. Only new/expired periods honor a chosen activation date.
        var start = ongoing == null && sale.ActivationMode == SubscriptionActivationMode.SpecificDate

```

Lines 87–102

```csharp

            after:new{sale.MemberId,result.SubscriptionId,result.GrantId,result.Scheduled});
        await db.SaveChangesAsync(ct); return result;
    }
    internal async Task<Guid?> PendingFirstEntrySaleAsync(Guid memberId, Guid cityId, DateOnly date, CancellationToken ct)
    {
        var pending = await PendingSales(memberId).Where(x => x.MembershipSnapshot!.CityId == cityId).Select(x => x.Id).Take(2).ToListAsync(ct);
        if (pending.Count > 1) throw new BusinessRuleException("Ambiguous paid entitlements pending first entry.");
        return pending.Count == 1 ? pending[0] : null;
    }
    internal Task<bool> IsPaidSaleOverdueAsync(Guid saleId,DateOnly date,CancellationToken ct)
        => db.Installments.AnyAsync(i=>i.InstallmentAgreement.SaleTransactionId==saleId && i.InstallmentAgreement.SaleTransaction.CancelledAt==null
            && i.InstallmentAgreement.SaleTransaction.PurchaseMode==PurchaseMode.Installment && i.DueDate<date
            && i.AmountDue>db.InstallmentPaymentAllocations.Where(a=>a.InstallmentId==i.Id && a.Payment.Status==PaymentStatus.Completed).Sum(a=>a.Amount),ct);
    internal Task<bool> IsSubscriptionOverdueAsync(Guid subscriptionId,DateOnly date,CancellationToken ct)
        => db.Installments.AnyAsync(i=>i.InstallmentAgreement.SaleTransaction.OperationId!=null && i.InstallmentAgreement.SaleTransaction.CancelledAt==null
            && i.InstallmentAgreement.SaleTransaction.PurchaseMode==PurchaseMode.Installment && i.DueDate<date

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.BorrowedDebt.cs

Lines 12–23

```csharp

    {
        var debts=await db.BorrowedGymSessions.Include(x=>x.ExpiredSubscription)
            .Where(x=>x.MemberId==memberId && (db.BorrowedDebtSettlementEvents.Where(e=>e.BorrowedGymSessionId==x.Id)
                .OrderByDescending(e=>e.Sequence).Select(e=>(bool?)e.IsReversal).FirstOrDefault() ?? x.Status==BorrowedSessionStatus.Outstanding))
            .OrderBy(x=>x.BorrowedAt).ThenBy(x=>x.Id).ToListAsync(ct);
        if(debts.Any(x=>x.ExpiredSubscription.MemberId!=memberId || x.ExpiredSubscription.EmployeeId!=null
            || x.ExpiredSubscription.EndDate==null || x.BorrowedAt>access.Now))
            throw new BusinessRuleException("Outstanding borrowed-session source history requires reconciliation.");
        if(plan.AccessMode==AccessMode.SessionBased && plan.SessionCount<debts.Count)
            throw new BusinessRuleException("The new SessionBased grant must cover all outstanding borrowed-session debt; partial settlement is not allowed.");
        return debts;
    }

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.Holidays.cs

Lines 8–22

```csharp

{
    public async Task<HolidayApplicationResult> CreateOfficialHolidayAsync(OfficialHolidayRequest r, CancellationToken ct = default)
    {
        var actor=await RequireAsync(SubscriptionPermissions.Extend,r.BranchId,true,ct);
        if (r.StartDate == default || r.EndDate < r.StartDate || r.EndDate.DayNumber-r.StartDate.DayNumber>=366
            || string.IsNullOrWhiteSpace(r.Reason) || r.Reason.Length>2048)
            throw new BusinessRuleException("Supply an inclusive holiday range of at most 366 days and a reason.");
        if (!await db.GymBranches.AnyAsync(x=>x.Id==r.BranchId && x.IsActive,ct)) throw new BranchNotFoundException("Active holiday branch not found.");
        return await TransactionAsync(async () =>
        {
            // Coordinate operational closure declarations with entrance branch locks without blocking FK key-share readers.
            var branch=await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={r.BranchId} FOR NO KEY UPDATE").AsNoTracking().SingleAsync(ct);
            if(!branch.IsActive)throw new BusinessRuleException("Holiday branch is inactive.");
            // Preserve module applicability. ExtensionDays=0 explicitly disables subscription compensation.
            var closure=new BranchClosure { BranchId=r.BranchId,StartDate=r.StartDate,EndDate=r.EndDate,

```

Lines 30–40

```csharp

    }
    public async Task<HolidayApplicationResult> ApplyOfficialHolidayAsync(Guid id, CancellationToken ct = default)
    {
        var actor=await RequireAsync(SubscriptionPermissions.Extend,null,true,ct);
        return await TransactionAsync(async () =>
        {
            // Shared closure lock prevents configuration edits; Member locks serialize compensation writers.
            var closure=await db.BranchClosures.FromSqlInterpolated($"SELECT * FROM \"BranchClosures\" WHERE \"Id\"={id} FOR SHARE")
                .AsNoTracking().SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Official holiday not found.");
            return await ApplyClosureAsync(closure,actor.UserId,null,ct);
        },ct);

```

Lines 44–49

```csharp

        var scopes=await db.MemberSubscriptions.Where(x=>x.MemberId==memberId && x.EmployeeId==null)
            .Select(x=>x.PurchasedAtBranchId).Distinct().ToListAsync(ct);
        var closures=await db.BranchClosures.AsNoTracking().Where(x=>scopes.Contains(x.BranchId) && x.AppliesToGym && x.ExtensionDays>0)
            .OrderBy(x=>x.StartDate).ThenBy(x=>x.Id).ToListAsync(ct);
        foreach(var closure in closures) await ApplyClosureAsync(closure,actor,memberId,ct);
    }

```

Lines 52–68

```csharp

        if (!closure.AppliesToGym || closure.ExtensionDays==0) return new(closure.Id,0,0);
        var length=closure.EndDate.DayNumber-closure.StartDate.DayNumber+1;
        if (length is <1 or >366 || closure.ExtensionDays!=length)
            throw new BusinessRuleException("Holiday compensation requires one protected day per declared closure day; reconcile legacy closure configuration.");
        var cutoff=await EffectiveBusinessDateAsync(closure.BranchId,ct);var last=closure.EndDate<cutoff?closure.EndDate:cutoff;
        if(last<closure.StartDate)return new(closure.Id,0,0);
        var members=await db.MemberSubscriptions.Where(x=>x.PurchasedAtBranchId==closure.BranchId && x.EmployeeId==null
            && x.EndDate>=closure.StartDate && x.StartDate<=last && (!onlyMember.HasValue || x.MemberId==onlyMember))
            .Select(x=>x.MemberId).Distinct().OrderBy(x=>x).ToListAsync(ct);
        var protectedDays=0;var extendedDays=0;
        foreach(var memberId in members)
        {
            var member=await LockMemberAsync(memberId,ct);if(!member.IsActive)continue;
            var periods=await db.MemberSubscriptions.Where(x=>x.MemberId==memberId && x.PurchasedAtBranchId==closure.BranchId
                && x.EmployeeId==null && x.EndDate!=null).OrderBy(x=>x.StartDate).ToListAsync(ct);
            foreach(var sub in periods)await db.Entry(sub).ReloadAsync(ct);
            for(var day=closure.StartDate;day<=last;day=PlusDays(day,1))

```

Lines 75–80

```csharp

                    var frozen=await db.SubscriptionFreezes.AnyAsync(x=>x.MemberSubscriptionId==sub.Id && x.FreezeType==FreezeType.Normal
                        && x.StartDate<=day && (x.EndDate==null || x.EndDate>=day),ct);
                    if(sub.Status==SubscriptionStatus.Frozen && !await db.SubscriptionFreezes.AnyAsync(x=>x.MemberSubscriptionId==sub.Id && x.FreezeType==FreezeType.Normal,ct))
                        throw new BusinessRuleException("Legacy Frozen state has no dated history; reconcile it before compensation.");
                    var old=sub.EndDate!.Value;var days=frozen?0:1;
                    if(days>0)

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.CustomerRules.cs

Lines 13–28

```csharp

    private async Task ValidateCustomerAsync(Member member, MembershipPlan plan, CancellationToken ct)
    {
        if (!member.IsActive || plan.PlanCategory == PlanCategory.Employee)
            throw new BusinessRuleException("Active customer Member and Normal/Freelance plan required.");
        if (!await MatchesAudienceAsync(member.Id, plan.MembershipPlanCategoryId, ct))
            throw new BusinessRuleException("Member does not match the Plan audience.");
        if (plan.PlanCategory != PlanCategory.Freelance) return;
        var coach = await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\"={plan.FreelanceCoachEmployeeId} FOR SHARE")
            .AsNoTracking().SingleOrDefaultAsync(ct);
        if (coach is not { EmploymentType: EmploymentType.Freelance, EmploymentStatus: EmploymentStatus.Active }
            || coach.HireDate > access.Today || coach.TerminationDate < access.Today)
            throw new BusinessRuleException("Freelance plan requires an active Freelance Coach.");
        if (!await db.MembershipPlanEligibleMembers.AnyAsync(x => x.MembershipPlanId == plan.Id && x.MemberId == member.Id, ct))
            throw new BusinessRuleException("Member is not eligible for this Freelance plan.");
        // Member origin and Sales assignment are deliberately not changed by this operation.
    }

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.Freezes.cs

Lines 12–22

```csharp

    {
        var identity = await db.MemberSubscriptions.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.MemberId,x.PurchasedAtBranchId }).SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Subscription not found.");
        await RequireAsync(permission,identity.PurchasedAtBranchId,false,ct);
        return await TransactionAsync(async () =>
        {
            await LockMemberAsync(identity.MemberId,ct);var sub = await LockedSubscriptionAsync(id,ct);
            var actor = await RequireAsync(permission,sub.PurchasedAtBranchId,false,ct);
            return await change(sub,actor.UserId);
        },ct);

```

Lines 24–51

```csharp

    public Task<FreezeDetails> StartFreezeAsync(Guid id, FreezeRequest r, CancellationToken ct = default)
        => MutateSubscriptionAsync(id,SubscriptionPermissions.Freeze,async (sub,actor) =>
        {
            if (r.OperationId == Guid.Empty || r.Days <= 0) throw new BusinessRuleException("Positive freeze days and operation identity are required.");
            var prior = await db.SubscriptionFreezes.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId,ct);
            if (prior != null)
            {
                if (prior.MemberSubscriptionId != id || prior.FreezeType != FreezeType.Normal || prior.StartDate != r.StartDate || prior.DaysApplied != r.Days)
                    throw new BusinessRuleException("Operation identity belongs to another freeze request.");
                return FreezeView(prior);
            }
            Version(sub,r.ExpectedVersion);
            var date = await EffectiveBusinessDateAsync(sub.PurchasedAtBranchId,ct);
            var status = (await Evaluated().SingleAsync(x => x.Subscription.Id == id,ct)).Status;
            if (sub.EmployeeId != null || sub.EndDate == null || status is not (SubscriptionStatus.Active or SubscriptionStatus.Frozen)
                || r.StartDate < date || r.StartDate > sub.EndDate)
                throw new BusinessRuleException("Normal freeze requires a current customer entitlement and a start within its remaining period.");
            var end = PlusDays(r.StartDate,r.Days-1);
            var grants = await db.MembershipSubscriptionTransactions.AsNoTracking().Where(x => x.MemberSubscriptionId == id
                && !db.SubscriptionGrantCancellations.Any(c=>c.GrantId==x.Id)).ToListAsync(ct);
            var freezes = await db.SubscriptionFreezes.AsNoTracking().Where(x => x.MemberSubscriptionId == id && x.FreezeType == FreezeType.Normal).ToListAsync(ct);
            if (grants.Count == 0 || grants.Any(x => x.FreezeDaysSnapshot == null) || freezes.Any(x => x.DaysApplied == null || x.EndDate == null))
                throw new BusinessRuleException("Legacy freeze allowance/history requires explicit reconciliation.");
            if (freezes.Any(x => x.StartDate <= end && x.EndDate >= r.StartDate)) throw new BusinessRuleException("Freeze periods cannot overlap.");
            var remaining = grants.Sum(x => (long)x.FreezeDaysSnapshot!.Value) - freezes.Sum(x => (long)x.DaysApplied!.Value);
            if (r.Days > remaining) throw new BusinessRuleException("Freeze exceeds remaining snapshot allowance.");
            var already = await db.SubscriptionHolidayExtensions.CountAsync(x => x.MemberSubscriptionId == id && x.DaysApplied == 1
                && x.ProtectedDate >= r.StartDate && x.ProtectedDate <= end,ct);

```

Lines 64–73

```csharp

        => MutateSubscriptionAsync(id,SubscriptionPermissions.Freeze,async (sub,actor) =>
        {
            var freeze = await db.SubscriptionFreezes.SingleOrDefaultAsync(x => x.Id == freezeId && x.MemberSubscriptionId == id && x.FreezeType == FreezeType.Normal,ct)
                ?? throw new BranchNotFoundException("Normal freeze not found for this subscription.");
            await db.Entry(freeze).ReloadAsync(ct);if (freeze.CompletedAt != null) return FreezeView(freeze);
            Version(sub,r.ExpectedVersion);
            if (freeze.EndDate == null || await EffectiveBusinessDateAsync(sub.PurchasedAtBranchId,ct) <= freeze.EndDate)
                throw new BusinessRuleException("Completion is recorded after the approved freeze period ends.");
            freeze.CompletedAt=access.Now;freeze.CompletedByUserId=actor;sub.Version++;
            access.Audit(actor,sub.PurchasedAtBranchId,"Subscription.FreezeCompleted",nameof(SubscriptionFreeze),freeze.Id,

```

## CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs

Lines 4–12

```csharp


public static class SubscriptionPermissions
{
    public const string View = "Subscriptions.View";
    public const string Manage = "Subscriptions.Manage";
    public const string Freeze = "Subscriptions.Freeze";
    public const string Extend = "Subscriptions.Extend";
    public static readonly string[] Codes = [View, Manage, Freeze, Extend];
}

```

Lines 17–27

```csharp

    public Guid? BranchId { get; set; }
    public SubscriptionStatus? Status { get; set; }
    public PlanCategory? PlanType { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}
public sealed record SubscriptionSummary(Guid Id, Guid MemberId, Guid SourcePlanId, string? PlanName,
    string? PlanType, Guid OriginBranchId, Guid AccessCityId, string Mode, DateOnly StartDate,
    DateOnly? EndDate, string Status, int? TotalSessions, int? RemainingSessions, int Version,
    Guid? EmployeeId, Guid? FollowsSubscriptionId, bool IsStandaloneVisit = false);
public sealed record SubscriptionHistory(IReadOnlyList<SubscriptionSummary> Items, int TotalCount, int Page, int PageSize);

```

Lines 29–39

```csharp

public sealed record SubscriptionGrantDetails(Guid Id, Guid PlanId, string? PlanName, string PlanType,
    Guid? PlanBranchId, Guid? AccessCityId, string? Mode, int DurationDays, int? Sessions, int? FreezeDays,
    decimal PlanPrice, decimal NetPrice, Guid? FreelanceCoachId, string TransactionType,
    DateOnly? StartDate, DateOnly? PreviousEndDate, DateOnly? NewEndDate, int? PreviousRemainingSessions,
    Guid? RenewedFromSubscriptionId, Guid? BusinessDayId, Guid? ActorId, DateTimeOffset CreatedAt, Guid? CancellationRefundId = null);
public sealed record FreezeDetails(Guid Id, string Type, DateOnly StartDate, DateOnly? EndDate, int? DaysApplied,
    string? Reason, Guid ApprovedBy, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt, int DaysAlreadyCompensated,
    DateOnly? PreviousSubscriptionEndDate, DateOnly? NewSubscriptionEndDate);
public sealed record HolidayExtensionDetails(Guid ClosureId, DateOnly ProtectedDate, int DaysApplied);
public sealed record SubscriptionDetails(SubscriptionSummary Subscription, string MemberName,
    int? FreezeAllowance, int ConsumedFreezeDays, int? RemainingFreezeDays,

```

Lines 41–57

```csharp

    IReadOnlyList<FreezeDetails> Freezes, IReadOnlyList<HolidayExtensionDetails> Holidays);
// Integration commands are BLL-only: no public paid activation/renewal route before Finance exists.
public sealed record SubscriptionGrantCommand(Guid MemberId, Guid PlanId, Guid BranchId, Guid OperationId);
public sealed record SubscriptionRenewalCommand(Guid SubscriptionId, Guid PlanId, Guid BranchId, Guid OperationId, int ExpectedVersion);
public sealed record SubscriptionGrantResult(Guid SubscriptionId, Guid GrantId, bool Scheduled);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record EmployeeEntitlementRequest(Guid MemberId, Guid PlanId, Guid OperationId);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record FreezeRequest(Guid OperationId, int ExpectedVersion, DateOnly StartDate, int Days);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CompleteFreezeRequest(int ExpectedVersion);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CustomExtensionRequest(Guid OperationId, int ExpectedVersion, int Days, string Reason);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OfficialHolidayRequest(Guid BranchId, DateOnly StartDate, DateOnly EndDate, bool ExtendSubscriptions, string Reason);
public sealed record HolidayApplicationResult(Guid ClosureId, int ProtectedDays, int ExtendedDays);
public sealed record MemberEntitlement(bool HasAccess, SubscriptionSummary? Subscription);

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.Renewals.cs

Lines 6–23

```csharp

namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    private static DateOnly PlusDays(DateOnly date, int days)
    {
        try { return date.AddDays(days); }
        catch (ArgumentOutOfRangeException) { throw new BusinessRuleException("Entitlement date is outside the supported date range."); }
    }
    private static void Version(MemberSubscription sub, int expected)
    {
        if (expected != sub.Version) throw new DbUpdateConcurrencyException("Subscription changed; reload before submitting a new operation.");
    }
    private async Task<MemberSubscription> LockedSubscriptionAsync(Guid id, CancellationToken ct)
    {
        var sub = await db.MemberSubscriptions.FromSqlInterpolated($"SELECT * FROM \"MemberSubscriptions\" WHERE \"Id\"={id} FOR UPDATE")
            .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Subscription not found.");
        await db.Entry(sub).ReloadAsync(ct); return sub;
    }

```

Lines 28–34

```csharp

        if (next is null) return;
        await db.Entry(next).ReloadAsync(ct);
        if (next.MemberId != source.MemberId || next.EmployeeId != null || next.EndDate == null
            || next.Status == SubscriptionStatus.Cancelled || next.StartDate <= await EffectiveBusinessDateAsync(next.PurchasedAtBranchId, ct))
            throw new BusinessRuleException("A successor already started or has conflicting history; this extension cannot rewrite it.");
        var old = new { next.StartDate, next.EndDate };
        next.StartDate = PlusDays(next.StartDate, days); next.EndDate = PlusDays(next.EndDate.Value, days); next.Version++;

```

Lines 37–73

```csharp

    }
    public async Task<SubscriptionGrantResult> RenewCustomerAsync(SubscriptionRenewalCommand r, CancellationToken ct = default)
    {
        var actor = await RequireAsync(SubscriptionPermissions.Manage,r.BranchId,true,ct);
        return await RenewCustomerCoreAsync(r, actor.UserId, null, null, ct);
    }
    private async Task<SubscriptionGrantResult> RenewCustomerCoreAsync(SubscriptionRenewalCommand r, Guid actor,
        SaleTransaction? sale, DateOnly? requestedStart, CancellationToken ct, (DateOnly Date, Guid? Day)? activationDate = null)
    {
        var sourceMember = await db.MemberSubscriptions.AsNoTracking().Where(x => x.Id == r.SubscriptionId)
            .Select(x => (Guid?)x.MemberId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Subscription not found.");
        return await TransactionAsync(async () =>
        {
            var plan = sale == null ? await LockPlanAsync(r.PlanId,r.BranchId,ct) : SoldPlan(sale);
            var member = await LockMemberAsync(sourceMember,ct);
            var source = await LockedSubscriptionAsync(r.SubscriptionId,ct);
            if (sale == null) await RequireAsync(SubscriptionPermissions.Manage,source.PurchasedAtBranchId,true,ct);
            if (source.EmployeeId != null) throw new BusinessRuleException("Employee periods cannot be renewed as customer memberships.");
            if (source.IsStandaloneVisit) throw new BusinessRuleException("Standalone visits are not membership renewal cycles.");
            var retry = await RetryGrantAsync(r.OperationId,member.Id,plan.Id,r.BranchId,source.Id,ct);
            if (retry is not null) return retry;
            Version(source,r.ExpectedVersion);
            if (sale == null) await ValidateCustomerAsync(member,plan,ct);
            else if (!member.IsActive) throw new BusinessRuleException("Member must be active to activate purchased entitlement.");
            var borrowedDebt=await OutstandingBorrowedDebtAsync(member.Id,plan,ct);
            var (date,day) = activationDate ?? await GrantDateAsync(r.BranchId,ct);
            var state = await Evaluated(sale == null ? null : date).SingleAsync(x => x.Subscription.Id == source.Id,ct);
            if (state.Status is not (SubscriptionStatus.Active or SubscriptionStatus.Frozen or SubscriptionStatus.Expired))
                throw new BusinessRuleException("Renewal requires a current or expired customer period.");
            if (state.PlanType is null) throw new BusinessRuleException("Legacy grant history must be reconciled before renewal.");
            var previousEnd = source.EndDate; var previousSessions = source.RemainingSessions;
            var ongoing = state.Status is SubscriptionStatus.Active or SubscriptionStatus.Frozen;
            var compatible = source.AccessMode == plan.AccessMode && source.CityId == plan.CityId
                && source.PurchasedAtBranchId == r.BranchId && state.PlanType == plan.PlanCategory;
            MemberSubscription target; DateOnly grantStart;
            if (ongoing && compatible)
            {

```

Lines 77–82

```csharp

                {
                    if ((long)source.TotalSessions!.Value + plan.SessionCount!.Value > int.MaxValue
                        || (long)source.RemainingSessions!.Value + plan.SessionCount.Value > int.MaxValue)
                        throw new BusinessRuleException("Session entitlement exceeds supported quantity.");
                    source.TotalSessions += plan.SessionCount; source.RemainingSessions += plan.SessionCount;
                }

```

Lines 86–91

```csharp

            {
                if (await db.MemberSubscriptions.AnyAsync(x => x.MemberId == member.Id && x.Id != source.Id
                    && x.Status != SubscriptionStatus.Cancelled && (!x.IsStandaloneVisit || x.RemainingSessions > 0) && (x.EndDate == null || x.EndDate >= date),ct))
                    throw new BusinessRuleException("Member already has another open/future period. Only one pending successor is supported.");
                grantStart = ongoing ? PlusDays(source.EndDate!.Value,1) : requestedStart ?? date;
                target = new MemberSubscription { MemberId=member.Id,MembershipPlanId=plan.Id,PurchasedAtBranchId=r.BranchId,

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.cs

Lines 9–24

```csharp


public sealed partial class SubscriptionService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private async Task<CurrentUserContext> RequireAsync(string permission, Guid? branch, bool global, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)
            || global && !user.Permissions.Contains("Branches.AccessAll")) throw new UnauthorizedAccessException();
        if (branch.HasValue) BranchAccessService.EnsureScope(user, branch);
        return user;
    }
    public async Task<DateOnly> EffectiveBusinessDateAsync(Guid branchId, CancellationToken ct = default)
        => await db.BusinessDays.Where(x => x.BranchId == branchId && x.Status == BusinessDayStatus.Open)
            .Select(x => (DateOnly?)x.BusinessDate).SingleOrDefaultAsync(ct) ?? access.Today;

    private sealed class Evaluation

```

Lines 28–33

```csharp

        public string? PlanName { get; init; }
        public PlanCategory? PlanType { get; init; }
    }
    private IQueryable<Evaluation> Evaluated(DateOnly? at = null)
    {
        var today = access.Today;

```

Lines 35–40

```csharp

        {
            S = s,
            Day = s.EmployeeId != null ? today : at ?? db.BusinessDays.Where(d => d.BranchId == s.PurchasedAtBranchId && d.Status == BusinessDayStatus.Open)
                .Select(d => (DateOnly?)d.BusinessDate).SingleOrDefault() ?? today,
            EmploymentValid = s.EmployeeId == null || s.Employee != null && s.Employee.MemberId == s.MemberId
                && s.Employee.EmploymentStatus == EmploymentStatus.Active && s.Employee.HireDate <= today

```

Lines 43–59

```csharp

            UnreconciledFreeze = s.Status == SubscriptionStatus.Frozen && !db.SubscriptionFreezes.Any(f => f.MemberSubscriptionId == s.Id && f.FreezeType == FreezeType.Normal),
            Frozen = db.SubscriptionFreezes.Any(f => f.MemberSubscriptionId == s.Id && f.FreezeType == FreezeType.Normal
                && f.StartDate <= (at ?? db.BusinessDays.Where(d => d.BranchId == s.PurchasedAtBranchId && d.Status == BusinessDayStatus.Open)
                    .Select(d => (DateOnly?)d.BusinessDate).SingleOrDefault() ?? today)
                && (f.EndDate == null || f.EndDate >= (at ?? db.BusinessDays.Where(d => d.BranchId == s.PurchasedAtBranchId && d.Status == BusinessDayStatus.Open)
                    .Select(d => (DateOnly?)d.BusinessDate).SingleOrDefault() ?? today))),
            Overdue = db.Installments.Any(i => i.InstallmentAgreement.SaleTransaction.OperationId != null
                && i.InstallmentAgreement.SaleTransaction.PurchaseMode == PurchaseMode.Installment && i.InstallmentAgreement.SaleTransaction.CancelledAt == null
                && i.DueDate < (at ?? db.BusinessDays.Where(d=>d.BranchId==s.PurchasedAtBranchId && d.Status==BusinessDayStatus.Open).Select(d=>(DateOnly?)d.BusinessDate).SingleOrDefault() ?? today)
                && i.AmountDue > db.InstallmentPaymentAllocations.Where(a=>a.InstallmentId==i.Id && a.Payment.Status==PaymentStatus.Completed).Sum(a=>a.Amount)
                && db.MembershipSubscriptionTransactions.Any(g=>g.SaleTransactionId==i.InstallmentAgreement.SaleTransactionId && g.MemberSubscriptionId==s.Id)),
            Name = db.MembershipSubscriptionTransactions.Where(t => t.MemberSubscriptionId == s.Id)
                .OrderBy(t => t.CreatedAt).ThenBy(t => t.Id).Select(t => t.PlanNameSnapshot).FirstOrDefault(),
            Type = db.MembershipSubscriptionTransactions.Where(t => t.MemberSubscriptionId == s.Id)
                .OrderBy(t => t.CreatedAt).ThenBy(t => t.Id).Select(t => (PlanCategory?)t.PlanCategorySnapshot).FirstOrDefault()
        });
        return facts.Select(x => new Evaluation

```

Lines 76–100

```csharp


    // Assembly-internal operational projection. Check-In authorizes the visited branch first;
    // it must not inherit originating-branch administration permissions.
    internal async Task<IReadOnlyList<SubscriptionSummary>> GetGymAccessFactsAsync(Guid memberId, DateOnly date, CancellationToken ct)
    {
        var lastEnd=await db.MemberSubscriptions.Where(s=>s.MemberId==memberId && !s.IsStandaloneVisit && s.EndDate<date).MaxAsync(s=>s.EndDate,ct);
        var rows=await Evaluated(date).Where(x=>x.Subscription.MemberId==memberId
            && (!x.Subscription.IsStandaloneVisit || x.Subscription.EndDate>=date && x.Subscription.RemainingSessions>0)
            && (x.Subscription.EndDate==null || x.Subscription.EndDate>=date || !x.Subscription.IsStandaloneVisit && x.Subscription.EndDate==lastEnd))
            .OrderByDescending(x=>x.Subscription.StartDate).Take(101).ToListAsync(ct);
        return rows.Select(Summary).ToArray();
    }

    public async Task<SubscriptionHistory> GetHistoryAsync(Guid memberId, SubscriptionHistoryRequest r, CancellationToken ct = default)
    {
        var user = await RequireAsync(SubscriptionPermissions.View, r.BranchId, false, ct);
        if (r.Page < 1 || r.PageSize is < 1 or > 100 || (long)(r.Page - 1) * r.PageSize > int.MaxValue
            || r.From > r.To || r.Status.HasValue && !Enum.IsDefined(r.Status.Value)
            || r.PlanType.HasValue && !Enum.IsDefined(r.PlanType.Value)) throw new BusinessRuleException("Invalid subscription filters or pagination.");
        var q = Evaluated().Where(x => x.Subscription.MemberId == memberId);
        if (!user.Permissions.Contains("Branches.AccessAll") || r.BranchId != null)
        {
            var branch = r.BranchId ?? user.CurrentBranchId ?? throw new UnauthorizedAccessException();
            q = q.Where(x => x.Subscription.PurchasedAtBranchId == branch);
        }

```

Lines 103–119

```csharp

        if (r.From.HasValue) q = q.Where(x => x.Subscription.StartDate >= r.From);
        if (r.To.HasValue) q = q.Where(x => x.Subscription.StartDate <= r.To);
        var count = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.Subscription.StartDate).ThenBy(x => x.Subscription.Id)
            .Skip((r.Page - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
        return new(rows.Select(Summary).ToArray(), count, r.Page, r.PageSize);
    }
    public async Task<SubscriptionDetails> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        var e = await Evaluated().SingleOrDefaultAsync(x => x.Subscription.Id == id, ct)
            ?? throw new BranchNotFoundException("Subscription not found.");
        await RequireAsync(SubscriptionPermissions.View, e.Subscription.PurchasedAtBranchId, false, ct);
        var grants = await db.MembershipSubscriptionTransactions.AsNoTracking().Where(x => x.MemberSubscriptionId == id)
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToListAsync(ct);
        var freezes = await db.SubscriptionFreezes.AsNoTracking().Where(x => x.MemberSubscriptionId == id).OrderBy(x => x.StartDate).ToListAsync(ct);
        var cancellations=await db.SubscriptionGrantCancellations.AsNoTracking().Where(c=>c.Grant.MemberSubscriptionId==id).ToListAsync(ct);
        var liveGrants=grants.Where(g=>!cancellations.Any(c=>c.GrantId==g.Id)).ToArray();

```

Lines 122–127

```csharp

        return new(Summary(e), await db.Members.Where(x => x.Id == e.Subscription.MemberId).Select(x => x.FullName).SingleAsync(ct),
            allowance, used, allowance.HasValue?Math.Max(0,allowance.Value-used):null,
            await db.MemberSubscriptionBenefits.AsNoTracking().Where(x => x.MembershipSubscriptionTransaction.MemberSubscriptionId == id)
                .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Select(x => new BenefitSnapshot(x.Id, x.MembershipSubscriptionTransactionId,
                    x.BenefitCodeSnapshot, x.BenefitNameSnapshot, x.GrantTypeSnapshot.ToString(), x.GrantedQuantity,
                    db.SubscriptionGrantCancellations.Any(c=>c.GrantId==x.MembershipSubscriptionTransactionId))).ToListAsync(ct),

```

Lines 132–151

```csharp

                cancellations.FirstOrDefault(c=>c.GrantId==x.Id)?.RefundId)).ToArray(),
            freezes.Select(FreezeView).ToArray(),
            await db.SubscriptionHolidayExtensions.AsNoTracking().Where(x => x.MemberSubscriptionId == id)
                .OrderBy(x => x.ProtectedDate).Select(x => new HolidayExtensionDetails(x.BranchClosureId, x.ProtectedDate, x.DaysApplied)).ToListAsync(ct));
    }
    // Cross-branch access reads are permitted only in the caller's operational branch and snapshot city.
    // Administration/history continue to use the subscription's originating branch.
    public async Task<MemberEntitlement> GetCurrentEntitlementAsync(Guid memberId, Guid? branchId = null, CancellationToken ct = default)
    {
        var user=await RequireAsync(SubscriptionPermissions.View, branchId, false, ct);
        branchId ??= user.CurrentBranchId ?? throw new BusinessRuleException("Select an operational branch for access lookup.");
        BranchAccessService.EnsureScope(user,branchId);
        var branch = await db.GymBranches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == branchId && x.IsActive && x.City.IsActive, ct)
            ?? throw new BranchNotFoundException("Active access branch not found.");
        var date = await EffectiveBusinessDateAsync(branchId.Value, ct);
        var candidates = await Evaluated(date).Where(x => x.Subscription.MemberId == memberId && x.Subscription.CityId == branch.CityId
            && (x.Status == SubscriptionStatus.Active || x.Status == SubscriptionStatus.Frozen)).Take(2).ToListAsync(ct);
        if (candidates.Count > 1) throw new BusinessRuleException("Conflicting entitlements require management reconciliation.");
        var closed = await db.BranchClosures.AnyAsync(x => x.BranchId == branchId && x.AppliesToGym && x.StartDate <= date && x.EndDate >= date, ct);
        return new(candidates.Count == 1 && candidates[0].Status == SubscriptionStatus.Active && !closed,

```

## CampionsSystem.BLL/Subscriptions/SubscriptionService.Extensions.cs

Lines 13–37

```csharp

            && x.StartDate <= sub.EndDate).ToListAsync(ct);
        if (others.Any(x => (x.AccessMode == AccessMode.Unlimited || x.RemainingSessions > 0)
                && x.StartDate <= sub.EndDate && (x.EndDate == null || x.EndDate >= sub.StartDate)))
            throw new BusinessRuleException("Extension would overlap another entitlement or rewrite an already-started period.");
    }
    public Task<FreezeDetails> GrantCustomExtensionAsync(Guid id, CustomExtensionRequest r, CancellationToken ct = default)
        => MutateSubscriptionAsync(id,SubscriptionPermissions.Extend,async (sub,actor) =>
        {
            if (r.OperationId == Guid.Empty || r.Days <= 0 || string.IsNullOrWhiteSpace(r.Reason) || r.Reason.Length > 2048)
                throw new BusinessRuleException("Positive extension days, operation identity and a reason up to 2048 characters are required.");
            var reason = r.Reason.Trim();
            var prior = await db.SubscriptionFreezes.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId,ct);
            if (prior is not null)
            {
                if (prior.MemberSubscriptionId != id || prior.FreezeType != FreezeType.Medical || prior.DaysApplied != r.Days || prior.Reason != reason)
                    throw new BusinessRuleException("Operation identity belongs to another extension request.");
                return FreezeView(prior);
            }
            Version(sub,r.ExpectedVersion);
            var status = (await Evaluated().SingleAsync(x => x.Subscription.Id == id,ct)).Status;
            if (sub.EmployeeId != null || sub.EndDate == null || status is not (SubscriptionStatus.Active or SubscriptionStatus.Frozen))
                throw new BusinessRuleException("Custom extension requires a current customer entitlement; expired revival is not supported.");
            var date = await EffectiveBusinessDateAsync(sub.PurchasedAtBranchId,ct);var old = sub.EndDate.Value;
            sub.EndDate=PlusDays(old,r.Days);sub.Version++;
            await ShiftSuccessorAsync(sub,r.Days,actor,ct);await EnsureNoOverlapAsync(sub,ct);

```
