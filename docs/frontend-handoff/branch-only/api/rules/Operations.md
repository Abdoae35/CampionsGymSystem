# Operations rule/validation evidence

Frozen working-tree source including pending Private drafts. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Operations/OperationAccess.cs

Lines 14–38

```csharp

    TimeProvider clock, IOptions<AuthContextOptions> options)
{
    public DateTimeOffset Now => clock.GetUtcNow();
    public DateOnly Today => LocalDate(Now);
    public DateOnly LocalDate(DateTimeOffset instant) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant,
        TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId)).DateTime);

    public (DateTimeOffset Start, DateTimeOffset End) DayBounds()
        => BusinessDateBounds(Today);

    public (DateTimeOffset Start, DateTimeOffset End) BusinessDateBounds(DateOnly businessDate)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);
        var date = businessDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return (new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(date, zone)),
            new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(date.AddDays(1), zone)));
    }

    public async Task<Guid> RequireAsync(string permission, Guid? branchId, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        if (branchId.HasValue) BranchAccessService.EnsureScope(user, branchId);
        return user.UserId;
    }

```

Lines 48–58

```csharp

    public static void Percentage(decimal value)
    {
        if (value < 0 || value > 100 || decimal.Round(value, 2) != value)
            throw new BusinessRuleException("Percentage must be between 0 and 100 with at most two decimal places.");
    }
    public static void Money(decimal value)
    {
        if (value < 0 || decimal.Round(value, 2) != value)
            throw new BusinessRuleException("Amount must be nonnegative with at most two decimal places.");
    }
}

```

## CampionsSystem.BLL/Operations/ActivityManagementService.cs

Lines 12–32

```csharp

{
    public async Task<Guid> CreateCoachAsync(CreateActivityCoachRequest request, CancellationToken ct = default)
    {
        var actor = await access.RequireAsync("Activities.ManageCoaches", null, ct);
        OperationAccess.Percentage(request.Percentage);
        if (request.CoachType == ActivityCoachType.Internal)
        {
            if (request.EmployeeId is null || request.ExternalName is not null || request.ExternalPhone is not null
                || !await db.Employees.AnyAsync(x => x.Id == request.EmployeeId && x.EmploymentStatus == EmploymentStatus.Active
                    && x.HireDate <= access.Today && (x.TerminationDate == null || x.TerminationDate >= access.Today), ct))
                throw new BusinessRuleException("An internal coach requires an active Employee and no external details.");
            if (await db.ActivityCoaches.AnyAsync(x => x.EmployeeId == request.EmployeeId, ct))
                throw new BusinessRuleException("This Employee already has an ActivityCoach profile.");
        }
        else if (request.CoachType != ActivityCoachType.External || request.EmployeeId is not null
                 || string.IsNullOrWhiteSpace(request.ExternalName))
            throw new BusinessRuleException("An external coach requires a name and no Employee.");
        if (request.ExternalName?.Length > 256 || request.ExternalPhone?.Length > 64)
            throw new BusinessRuleException("Coach contact details are too long.");
        var coach = new ActivityCoach { CoachType = request.CoachType, EmployeeId = request.EmployeeId,
            ExternalName = request.ExternalName?.Trim(), ExternalPhone = request.ExternalPhone?.Trim(),

```

Lines 40–48

```csharp


    public async Task SetPercentageAsync(Guid coachId, decimal percentage, CancellationToken ct = default)
    {
        var actor = await access.RequireAsync("Activities.ManageCoaches", null, ct);
        OperationAccess.Percentage(percentage);
        var coach = await db.ActivityCoaches.SingleOrDefaultAsync(x => x.Id == coachId, ct)
            ?? throw new BusinessRuleException("Activity coach not found.");
        var before = coach.Percentage;
        coach.Percentage = percentage;

```

Lines 54–65

```csharp

    public async Task<Guid> AssignAsync(Guid activityId, Guid coachId, CancellationToken ct = default)
    {
        var activity = await db.SportsActivities.SingleOrDefaultAsync(x => x.Id == activityId && x.IsActive, ct)
            ?? throw new BusinessRuleException("Active activity not found.");
        var actor = await access.RequireAsync("Activities.ManageCoaches", activity.BranchId, ct);
        if (!await db.ActivityCoaches.AnyAsync(x => x.Id == coachId && x.IsActive, ct))
            throw new BusinessRuleException("Active activity coach not found.");
        if (await db.ActivityCoachAssignments.AnyAsync(x => x.SportsActivityId == activityId
            && x.ActivityCoachId == coachId && x.IsActive && (x.EndDate == null || x.EndDate >= access.Today), ct))
            throw new BusinessRuleException("Coach already has an active assignment to this activity.");
        var assignment = new ActivityCoachAssignment { SportsActivityId = activityId, ActivityCoachId = coachId,
            StartDate = access.Today, CreatedAt = access.Now };

```

Lines 73–80

```csharp

    public async Task UnassignAsync(Guid assignmentId, CancellationToken ct = default)
    {
        var row = await db.ActivityCoachAssignments.Include(x => x.SportsActivity)
            .SingleOrDefaultAsync(x => x.Id == assignmentId, ct) ?? throw new BusinessRuleException("Assignment not found.");
        var actor = await access.RequireAsync("Activities.ManageCoaches", row.SportsActivity.BranchId, ct);
        if (row.StartDate > access.Today) throw new BusinessRuleException("Future assignments require explicit date correction.");
        row.IsActive = false;
        row.EndDate = access.Today;

```

## CampionsSystem.BLL/Operations/SubscriptionPurchaseService.cs

Lines 21–47

```csharp

            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        var package = await db.ActivityPackages.Include(x => x.SportsActivity)
            .SingleOrDefaultAsync(x => x.Id == request.ActivityPackageId && x.IsActive && x.SportsActivity.IsActive, ct)
            ?? throw new BusinessRuleException("Active activity package not found.");
        var actor = await access.RequireAsync("Activities.Manage", package.SportsActivity.BranchId, ct);
        if (!await db.Members.AnyAsync(x => x.Id == request.MemberId && x.IsActive, ct)) throw new BusinessRuleException("Active member required.");
        var coach = await db.ActivityCoaches.Include(x => x.Employee)
            .SingleOrDefaultAsync(x => x.Id == request.ActivityCoachId && x.IsActive, ct)
            ?? throw new BusinessRuleException("An active activity coach must be selected.");
        if (coach.CoachType == ActivityCoachType.Internal && (coach.Employee is null
            || coach.Employee.EmploymentStatus != EmploymentStatus.Active || coach.Employee.HireDate > access.Today
            || coach.Employee.TerminationDate < access.Today)) throw new BusinessRuleException("Coach Employee is not active.");
        var assignment = await db.ActivityCoachAssignments.Where(x => x.SportsActivityId == package.SportsActivityId
            && x.ActivityCoachId == coach.Id && x.IsActive && x.StartDate <= access.Today
            && (x.EndDate == null || x.EndDate >= access.Today)).OrderByDescending(x => x.StartDate).FirstOrDefaultAsync(ct)
            ?? throw new BusinessRuleException("Coach is not assigned to this activity.");
        var subscription = request.ExistingSubscriptionId.HasValue
            ? await db.ActivitySubscriptions.Include(x => x.ActivityPackage).SingleOrDefaultAsync(x => x.Id == request.ExistingSubscriptionId, ct)
                ?? throw new BusinessRuleException("Subscription not found.")
            : new ActivitySubscription { MemberId = request.MemberId, BranchId = package.SportsActivity.BranchId,
                StartDate = access.Today, AccessMode = package.AccessMode, Status = ActivitySubscriptionStatus.Scheduled };
        if (subscription.MemberId != request.MemberId || subscription.BranchId != package.SportsActivity.BranchId
            || (request.ExistingSubscriptionId.HasValue && (subscription.ActivityPackage.SportsActivityId != package.SportsActivityId
                || subscription.AccessMode != package.AccessMode))) throw new BusinessRuleException("Renewal context does not match.");
        var previousEnd = request.ExistingSubscriptionId.HasValue ? subscription.EndDate : (DateOnly?)null;
        var previousCoach = subscription.ActivityCoachId;
        subscription.ActivityCoachId = coach.Id; subscription.ActivityCoachAssignmentId = assignment.Id;

```

## CampionsSystem.BLL/Operations/CoachReportService.cs

Lines 18–24

```csharp

    public async Task<IReadOnlyList<ActivityCoachReport>> ActivitiesAsync(Guid periodId, CancellationToken ct = default)
    {
        var period = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(x => x.Id == periodId, ct)
            ?? throw new BusinessRuleException("Commission period not found.");
        await access.RequireAsync("Activities.ViewCoachRevenue", period.BranchId, ct);
        if (period.Status == CommissionPeriodStatus.Closed || await db.AuditLogs.AnyAsync(
            x => x.Action == "ActivityCoach.PeriodFinalized" && x.EntityId == periodId, ct))

```

Lines 28–33

```csharp

                    x.CoachTypeSnapshot, x.EmployeeId, x.SubscribersCount, x.EligibleRevenue, x.Percentage, x.Entitlement)).ToListAsync(ct);
            // A marker in AuditLog also records a finalized zero-revenue period.
            if (rows.Count == 0 && !await db.AuditLogs.AnyAsync(x => x.Action == "ActivityCoach.PeriodFinalized" && x.EntityId == periodId, ct))
                throw new BusinessRuleException("Closed period has no activity snapshot; explicitly finalize/reconcile it first.");
            return rows;
        }

```

Lines 40–51

```csharp

        await using var transaction = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct) : null;
        var period = await db.CommissionPeriods.SingleOrDefaultAsync(x => x.Id == periodId, ct)
            ?? throw new BusinessRuleException("Commission period not found.");
        var actor = await access.RequireAsync("Commission.ClosePeriod", period.BranchId, ct);
        if (period.Status != CommissionPeriodStatus.Closed || period.EndAt is null || period.EndAt > access.Now)
            throw new BusinessRuleException("Close the period with a final end time before taking an activity snapshot.");
        if (await db.ActivityCoachSettlements.AnyAsync(x => x.CommissionPeriodId == periodId, ct)
            || await db.AuditLogs.AnyAsync(x => x.Action == "ActivityCoach.PeriodFinalized" && x.EntityId == periodId, ct))
            throw new BusinessRuleException("Activity period is already finalized; explicit correction required.");
        var rows = await CalculateActivitiesAsync(period, ct);
        foreach (var row in rows)

```

Lines 59–64

```csharp

        {
            if (await db.EmployeeCommissionSummaries.AnyAsync(x => x.CommissionPeriodId == periodId && x.EmployeeId == group.Key.EmployeeId
                && x.CommissionType == CommissionType.Activity && x.SportsActivityId == group.Key.ActivityId, ct))
                throw new BusinessRuleException("An employee activity summary already exists; reconcile instead of double paying.");
            db.Add(new EmployeeCommissionSummary { CommissionPeriodId = periodId, EmployeeId = group.Key.EmployeeId!.Value,
                BranchId = period.BranchId, CommissionType = CommissionType.Activity, SportsActivityId = group.Key.ActivityId,

```

Lines 81–86

```csharp

                PartsAmount = x.Parts.Sum(p => (decimal?)p.Amount) ?? 0m }).ToListAsync(ct);
        if (payments.Count == 0) return [];
        if (payments.Any(x => x.BranchId != period.BranchId || x.PaidAt < period.StartAt || x.PaidAt >= until || x.Amount != x.PartsAmount))
            throw new BusinessRuleException("Activity payment branch/time or PaymentParts total is inconsistent; reconcile first.");
        var saleIds = payments.Select(x => x.SaleTransactionId).Distinct().ToArray();
        var batches = await db.ActivitySubscriptionTransactions.AsNoTracking().Where(x => saleIds.Contains(x.SaleTransactionId))

```

Lines 92–108

```csharp

                CoachType = x.ActivityCoachSnapshot == null ? (ActivityCoachType?)null : x.ActivityCoachSnapshot.CoachType }).ToListAsync(ct);
        if (saleIds.Any(id => batches.Count(x => x.SaleTransactionId == id) != 1)
            || batches.Any(x => x.ActivityCoachIdSnapshot is null || x.CoachPercentageSnapshot is null || x.BranchId != period.BranchId))
            throw new BusinessRuleException("Activity purchase has missing/ambiguous coach, rate or sale attribution; reconcile legacy data first.");
        var refunds = await db.Refunds.AsNoTracking().Where(x => saleIds.Contains(x.SaleTransactionId) && x.CreatedAt < until)
            .Select(x => new { x.OriginalPaymentId, x.SaleTransactionId, x.Amount }).ToListAsync(ct);
        if (refunds.Any(x => x.OriginalPaymentId is null))
            throw new BusinessRuleException("Activity refunds require an explicit OriginalPaymentId for commission attribution.");
        if (refunds.Any(x => payments.Any(p => p.Id == x.OriginalPaymentId && p.SaleTransactionId != x.SaleTransactionId)))
            throw new BusinessRuleException("Refund payment and sale disagree.");
        var values = payments.Select(payment =>
        {
            var batch = batches.Single(x => x.SaleTransactionId == payment.SaleTransactionId);
            var refunded = refunds.Where(x => x.OriginalPaymentId == payment.Id).Sum(x => x.Amount);
            if (refunded > payment.Amount) throw new BusinessRuleException("Activity refunds exceed received payment.");
            return new { Batch = batch, Revenue = payment.Amount - refunded };
        });

```

Lines 111–123

```csharp

            .Select(g => new ActivityCoachReport(g.Key.ActivityCoachIdSnapshot!.Value, g.Key.CoachName,
                g.Key.ActivityId, g.Key.ActivityName, g.Key.CoachType!.Value, g.Key.EmployeeId,
                g.Select(x => x.Batch.MemberId).Distinct().Count(), g.Sum(x => x.Revenue), g.Key.CoachPercentageSnapshot!.Value,
                decimal.Round(g.Sum(x => x.Revenue) * g.Key.CoachPercentageSnapshot.Value / 100m, 2, MidpointRounding.AwayFromZero))).ToArray();
    }

    public async Task<IReadOnlyList<ClassCoachReport>> ClassesAsync(Guid branchId, DateTimeOffset from,
        DateTimeOffset until, CancellationToken ct = default)
    {
        await access.RequireAsync("Classes.ViewAttendanceReport", branchId, ct);
        if (until <= from) throw new BusinessRuleException("Report end must be after start.");
        from = from.ToUniversalTime(); until = until.ToUniversalTime();
        var rows = await db.ClassSessions.AsNoTracking().Where(x => x.BranchId == branchId && x.Status == ClassSessionStatus.Completed

```

Lines 132–143

```csharp

    public async Task<IReadOnlyList<FreelancePlanReport>> FreelanceAsync(Guid planId, Guid branchId,
        DateTimeOffset from, DateTimeOffset until, CancellationToken ct = default)
    {
        await access.RequireAsync("FreelancePlans.View", branchId, ct);
        if (until <= from) throw new BusinessRuleException("Report end must be after start.");
        from = from.ToUniversalTime(); until = until.ToUniversalTime();
        var plan = await db.MembershipPlans.AsNoTracking().Include(x => x.FreelanceCoachEmployee)
            .SingleOrDefaultAsync(x => x.Id == planId && x.PlanCategory == PlanCategory.Freelance, ct)
            ?? throw new BusinessRuleException("Freelance plan not found.");
        if(plan.BranchId!=branchId)throw new BusinessRuleException("Report branch must own the Freelance plan.");
        var eligible = await db.MembershipPlanEligibleMembers.CountAsync(x => x.MembershipPlanId == planId && x.Member.IsActive, ct);
        var purchases = await db.MembershipSubscriptionTransactions.AsNoTracking().Where(x => x.MembershipPlanId == planId

```

Lines 148–163

```csharp

                x.SaleTransaction!.OperationId,x.SaleTransaction!.CommissionEligible,x.SaleTransaction!.TargetEligible,x.CreatedAt }).ToListAsync(ct);
        var ids = purchases.Select(x => x.SaleTransactionId).ToArray();
        if (purchases.GroupBy(x => x.SaleTransactionId).Any(g => g.Count() != 1))
            throw new BusinessRuleException("Ambiguous membership sale; reconcile before reporting.");
        var payments = await db.Payments.AsNoTracking().Where(x => ids.Contains(x.SaleTransactionId) && x.BranchId == branchId
            && x.Status == PaymentStatus.Completed && x.PaidAt >= from && x.PaidAt < until)
            .Select(x => new { x.SaleTransactionId, x.Amount, Parts = x.Parts.Sum(p => (decimal?)p.Amount) ?? 0m }).ToListAsync(ct);
        var refunds = await db.Refunds.AsNoTracking().Where(x => ids.Contains(x.SaleTransactionId) && x.BranchId == branchId
            && x.CreatedAt >= from && x.CreatedAt < until).Select(x => new { x.SaleTransactionId, x.Amount }).ToListAsync(ct);
        if (payments.Any(x => x.Parts != x.Amount)) throw new BusinessRuleException("PaymentParts do not match received gym money.");
        if (purchases.Any(x => x.OperationId==null && (x.SalesAttributionSnapshot != AssignmentType.House || x.SalesEmployeeId != null || x.SaleCategory != null)))
            throw new BusinessRuleException("Legacy Freelance sale requires explicit House attribution reconciliation.");
        if(purchases.Any(x=>x.OperationId!=null && (x.CommissionEligible!=false || x.TargetEligible!=false)))
            throw new BusinessRuleException("Managed Freelance revenue must be commission/target ineligible.");
        var result = purchases.GroupBy(x => new { x.FreelanceCoachEmployeeIdSnapshot, x.Coach }).Select(g =>
        {

```

## CampionsSystem.BLL/Operations/ClassSessionService.cs

Lines 13–33

```csharp

    public async Task<Guid> SaveAsync(Guid? id, SaveClassSessionRequest request, CancellationToken ct = default)
    {
        var group = await db.GroupClasses.SingleOrDefaultAsync(x => x.Id == request.GroupClassId && x.IsActive, ct)
            ?? throw new BusinessRuleException("Active class not found.");
        var actor = await access.RequireAsync("Classes.Manage", group.BranchId, ct);
        if (!Enum.IsDefined(request.Status) || request.EndedAt < request.StartedAt
            || (request.Status == ClassSessionStatus.Completed && (request.EndedAt is null || request.EndedAt > access.Now)))
            throw new BusinessRuleException("Invalid actual session status/times.");
        if (!await db.Employees.AnyAsync(x => x.Id == request.CoachEmployeeId && x.EmploymentStatus == EmploymentStatus.Active, ct))
            throw new BusinessRuleException("Active delivering coach required.");
        if (request.ClassScheduleId.HasValue && !await db.ClassSchedules.AnyAsync(x => x.Id == request.ClassScheduleId
            && x.GroupClassId == group.Id, ct)) throw new BusinessRuleException("Schedule belongs to another class.");
        var session = id.HasValue ? await db.ClassSessions.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new BusinessRuleException("Session not found.") : new ClassSession { GroupClassId = group.Id, BranchId = group.BranchId };
        if (session.OperationId != null || session.GroupClassId != group.Id || (id.HasValue && session.Status != ClassSessionStatus.Scheduled))
            throw new BusinessRuleException("Executed/cancelled sessions cannot be rewritten; use an explicit correction workflow.");
        if (id.HasValue && await db.CheckIns.AnyAsync(x => x.ClassSessionId == id, ct)
            && (session.CoachEmployeeId != request.CoachEmployeeId || session.StartedAt != request.StartedAt))
            throw new BusinessRuleException("Cannot change coach/start time after recording attendance.");
        session.ClassScheduleId = request.ClassScheduleId; session.CoachEmployeeId = request.CoachEmployeeId;
        session.StartedAt = request.StartedAt.ToUniversalTime(); session.EndedAt = request.EndedAt?.ToUniversalTime(); session.Status = request.Status;

```

Lines 39–43

```csharp

    }

    public Task<Guid> AttendAsync(Guid sessionId, Guid memberId, CancellationToken ct = default)
        => throw new BusinessRuleException("Legacy package-free class attendance is disabled; use the Classes package attendance workflow.");
}

```
