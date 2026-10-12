# Coaching rule/validation evidence

Frozen working-tree source including pending Private drafts. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Coaching/CoachingService.Classes.cs

Lines 11–22

```csharp

    public Task<ClassTypeDetails> SaveClassTypeAsync(Guid? id, ClassTypeRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            var actor = await access.RequireAsync(CoachingPermissions.ClassesManage, r.BranchId, ct);
            await ActiveBranchAsync(r.BranchId, ct);
            if (string.IsNullOrWhiteSpace(r.Name) || r.Name.Trim().Length > 256 || r.Description?.Length > 2000) throw new BusinessRuleException("Invalid class type name/description.");
            var (date, _) = await coordination.BusinessDateAsync(r.BranchId, ct);
            await ValidateCoachAsync(r.CoachEmployeeId, r.BranchId, date, ct);
            var row = id == null ? new GroupClass { BranchId = r.BranchId } : await db.GroupClasses.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Class type not found.");
            if (row.BranchId != r.BranchId) throw new UnauthorizedAccessException();
            row.Name = r.Name.Trim(); row.CoachEmployeeId = r.CoachEmployeeId; row.Description = r.Description; row.IsActive = r.IsActive;
            if (id == null) db.Add(row);

```

Lines 25–50

```csharp

        }, ct);
    public async Task<CoachingPage<ClassTypeDetails>> ClassTypesAsync(Guid branch, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
        var q = db.GroupClasses.AsNoTracking().Where(x => x.BranchId == branch); var total = await q.CountAsync(ct);
        return new(await q.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new ClassTypeDetails(x.Id, x.BranchId, x.Name, x.CoachEmployeeId, x.IsActive, x.Description)).ToListAsync(ct), total, page, pageSize);
    }
    public Task<ClassOccurrenceDetails> CreateOccurrenceAsync(ClassOccurrenceRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId);
            var group = await db.GroupClasses.AsNoTracking().SingleOrDefaultAsync(g => g.Id == r.ClassTypeId && g.IsActive, ct) ?? throw new BranchNotFoundException("Active class type not found.");
            var actor = await access.RequireAsync(CoachingPermissions.ClassesManage, group.BranchId, ct);
            await ActiveBranchAsync(group.BranchId, ct);
            await coordination.LockBranchAsync(group.BranchId, ct); var hash = FinancialOperations.Hash(r);
            var prior = await db.ClassSessions.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId!.Value, actor); return await OccurrenceDetailsAsync(prior, ct); }
            var (date, day) = await coordination.BusinessDateAsync(group.BranchId, ct);
            await ValidateCoachAsync(r.CoachEmployeeId, group.BranchId, date, ct);
            if (r.StartsAt == default || r.StartsAt < access.Now)
                throw new BusinessRuleException("Occurrence must use the current operational BusinessDate; future scheduling is outside this workflow.");
            var row = new ClassSession { GroupClassId = group.Id, BranchId = group.BranchId, CoachEmployeeId = r.CoachEmployeeId,
                StartedAt = r.StartsAt.ToUniversalTime(), Status = ClassSessionStatus.Scheduled, BusinessDate = date, BusinessDayId = day,
                OperationId = r.OperationId, RequestHash = hash, CreatedByUserId = actor, CreatedAt = access.Now };
            db.Add(row); access.Audit(actor, group.BranchId, "Classes.OccurrenceCreated", nameof(ClassSession), row.Id, after: r);

```

Lines 53–61

```csharp

    public Task<ClassOccurrenceDetails> CompleteOccurrenceAsync(Guid id, CompleteClassRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId);
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"class-completion:" + r.OperationId}, 0))", ct);
            var info = await db.ClassSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Occurrence not found.");
            var actor = await access.RequireAsync(CoachingPermissions.ClassesManage, info.BranchId, ct); await coordination.LockBranchAsync(info.BranchId, ct);
            var row = await db.ClassSessions.SingleAsync(x => x.Id == id, ct); await db.Entry(row).ReloadAsync(ct);
            var hash = FinancialOperations.Hash(new { id, r });

```

Lines 63–75

```csharp

            if (marker != null)
            {
                using var evidence = System.Text.Json.JsonDocument.Parse(marker.NewValues!);
                if (evidence.RootElement.GetProperty("Id").GetGuid() != id || evidence.RootElement.GetProperty("Hash").GetString() != hash || marker.UserId != actor) throw new BusinessRuleException("Operation belongs to another occurrence completion.");
                return await OccurrenceDetailsAsync(row, ct);
            }
            if (row.OperationId == null || row.Status != ClassSessionStatus.Scheduled || row.Version != r.ExpectedVersion) throw new BusinessRuleException("Occurrence is legacy, finalized or has a stale version.");
            var (date, _) = await coordination.BusinessDateAsync(row.BranchId, ct);
            await CoachAsync(row.CoachEmployeeId, actor, row.BranchId, date, ct);
            if (row.BusinessDate != date || row.StartedAt > access.Now || r.Cancelled && await db.CheckIns.AnyAsync(x => x.ClassSessionId == id, ct))
                throw new BusinessRuleException("Cannot complete before start or cancel attended occurrences; explicit correction is required.");
            row.Status = r.Cancelled ? ClassSessionStatus.Cancelled : ClassSessionStatus.Completed; row.EndedAt = access.Now; row.Version++;
            access.Audit(actor, row.BranchId, "Classes.OccurrenceEnded", nameof(ClassSession), r.OperationId, after: new { Id = id, Hash = hash });

```

Lines 78–106

```csharp

    public Task<ClassAttendanceDetails> RegisterClassAsync(Guid occurrence, ClassAttendanceRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId);
            var info = await db.ClassSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == occurrence, ct) ?? throw new BranchNotFoundException("Occurrence not found.");
            var actor = await access.RequireAsync(CoachingPermissions.RegisterClass, info.BranchId, ct); await coordination.LockBranchAsync(info.BranchId, ct);
            await ActiveBranchAsync(info.BranchId, ct);
            var member = await LockMemberAsync(r.MemberId, ct);
            var row = await db.ClassSessions.SingleAsync(x => x.Id == occurrence, ct); await db.Entry(row).ReloadAsync(ct);
            var hash = FinancialOperations.Hash(new { occurrence, r });
            var prior = await db.CheckIns.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return ClassAttendance(prior); }
            var (date, day) = await coordination.BusinessDateAsync(row.BranchId, ct);
            await CoachAsync(row.CoachEmployeeId, actor, row.BranchId, date, ct);
            if (!member.IsActive || row.OperationId == null || row.Status != ClassSessionStatus.Scheduled || row.BusinessDate != date || access.Now >= row.StartedAt)
                throw new BusinessRuleException("Record an active member individually before the scheduled class starts on its BusinessDate.");
            if (await db.CheckIns.AnyAsync(x => x.ClassSessionId == occurrence && x.MemberId == r.MemberId, ct)) throw new BusinessRuleException("Member is already registered for this occurrence.");
            var eligible = db.ClassesSubscriptions.Where(s => s.BranchId == row.BranchId && s.MemberId == r.MemberId && s.StartDate <= date && s.EndDate >= date
                && s.RemainingSessions > 0 && s.SaleTransaction.CancelledAt == null);
            if (r.ClassesSubscriptionId != null) eligible = eligible.Where(s => s.Id == r.ClassesSubscriptionId);
            var sub = await eligible.OrderBy(s => s.EndDate).ThenBy(s => s.CreatedAt).ThenBy(s => s.Id).FirstOrDefaultAsync(ct)
                ?? throw new BusinessRuleException("No eligible Classes package session remains.");
            await db.Entry(sub).ReloadAsync(ct);
            if (await memberships.IsPaidSaleOverdueAsync(sub.SaleTransactionId, date, ct)) throw new BusinessRuleException("Classes installment is overdue.");
            var entry = new CheckIn { MemberId = member.Id, BranchId = row.BranchId, ClassesSubscriptionId = sub.Id, ClassSessionId = row.Id,
                GroupClassId = row.GroupClassId, CoachEmployeeId = row.CoachEmployeeId, CheckInType = CheckInType.Class, CheckInMethod = CheckInMethod.Coach,
                OperationId = r.OperationId, RequestHash = hash, BusinessDate = date, BusinessDayId = day, SessionConsumed = true,
                SessionsBefore = sub.RemainingSessions, SessionsAfter = sub.RemainingSessions - 1, CheckedInAt = access.Now, CreatedByUserId = actor };
            sub.RemainingSessions--; sub.Version++; db.Add(entry);

```

Lines 108–149

```csharp

            return ClassAttendance(entry);
        }, ct);
    private static ClassAttendanceDetails ClassAttendance(CheckIn c) => new(c.Id, c.MemberId!.Value, c.ClassesSubscriptionId!.Value,
        c.ClassSessionId!.Value, c.CoachEmployeeId!.Value, c.BusinessDate!.Value, c.SessionsAfter!.Value, c.CheckedInAt);
    private async Task<ClassOccurrenceDetails> OccurrenceDetailsAsync(ClassSession c, CancellationToken ct) => new(c.Id, c.GroupClassId, c.BranchId, c.CoachEmployeeId,
        c.BusinessDate, c.StartedAt, c.EndedAt, c.Status.ToString(), await db.CheckIns.CountAsync(x => x.ClassSessionId == c.Id, ct), c.Version);
    public async Task<CoachingPage<ClassOccurrenceDetails>> OccurrencesAsync(Guid branch, DateOnly from, DateOnly to, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); if (from > to) throw new BusinessRuleException("Invalid date range."); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
        var q = db.ClassSessions.AsNoTracking().Where(x => x.BranchId == branch && x.BusinessDate >= from && x.BusinessDate <= to);
        var total = await q.CountAsync(ct); var rows = await q.OrderByDescending(x => x.StartedAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var result = new List<ClassOccurrenceDetails>(); foreach (var c in rows) result.Add(await OccurrenceDetailsAsync(c, ct)); return new(result, total, page, pageSize);
    }
    public async Task<CoachingPage<ClassAttendanceDetails>> ClassAttendanceHistoryAsync(Guid occurrence, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); var branch = await db.ClassSessions.Where(x => x.Id == occurrence).Select(x => (Guid?)x.BranchId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Occurrence not found.");
        await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
        var q = db.CheckIns.AsNoTracking().Where(x => x.ClassSessionId == occurrence && x.ClassesSubscriptionId != null); var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.CheckedInAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct); return new(rows.Select(ClassAttendance).ToArray(), total, page, pageSize);
    }
    public async Task<CoachingPage<ClassesSubscriptionDetails>> ClassesSubscriptionsAsync(Guid branch, Guid member, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
        var q = db.ClassesSubscriptions.AsNoTracking().Where(s => s.BranchId == branch && s.MemberId == member); var total = await q.CountAsync(ct);
        return new(await q.OrderByDescending(s => s.CreatedAt).ThenBy(s => s.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new ClassesSubscriptionDetails(s.Id, s.MemberId, s.BranchId, s.SaleTransactionId, s.PackageName, s.StartDate, s.EndDate,
                s.PurchasedSessions, s.RemainingSessions, s.Price, s.SaleTransaction.CancelledAt != null, s.Version)).ToListAsync(ct), total, page, pageSize);
    }
    public async Task<ClassCoachStatistics> ClassStatisticsAsync(Guid branch, Guid coach, DateOnly from, DateOnly to, Guid? periodId = null, CancellationToken ct = default)
    {
        await access.RequireAsync("Classes.ViewAttendanceReport", branch, ct);
        if (from > to || to.DayNumber - from.DayNumber > 366) throw new BusinessRuleException("Select a report range of at most 367 days.");
        var rows = await db.ClassSessions.AsNoTracking().Where(c => c.BranchId == branch && c.CoachEmployeeId == coach && c.Status == ClassSessionStatus.Completed
            && c.BusinessDate >= from && c.BusinessDate <= to).OrderBy(c => c.StartedAt).ThenBy(c => c.Id).ToListAsync(ct);
        var stats = new List<ClassOccurrenceDetails>(); foreach (var row in rows) stats.Add(await OccurrenceDetailsAsync(row, ct));
        decimal? target = null;
        if (periodId != null)
        {
            var period = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.Id == periodId && p.BranchId == branch && p.StartBusinessDate == from && p.EndBusinessDate == to, ct)
                ?? throw new BusinessRuleException("Target report dates must exactly match the branch period.");
            target = await db.CoachTargetRules.Where(t => t.CommissionPeriodId == period.Id && t.CoachEmployeeId == coach).Select(t => t.ClassesAverageTarget).SingleOrDefaultAsync(ct);
        }

```

## CampionsSystem.BLL/Coaching/CoachingService.cs

Lines 17–66

```csharp

    internal static decimal UnitValue(decimal price, int sessions) => decimal.Floor(price / sessions * 100m) / 100m;
    internal static decimal SessionValue(decimal unit, decimal remainder, int number, int total) => unit + (number == total ? remainder : 0m);
    private static void Page(int page, int size)
    { if (page < 1 || size is < 1 or > 100 || (long)(page - 1) * size > int.MaxValue) throw new BusinessRuleException("Invalid pagination."); }
    private async Task<Guid> OwnerAsync(string permission, Guid branch, CancellationToken ct)
    {
        var actor = await access.RequireAsync(permission, branch, ct);
        await access.RequireAsync("Branches.AccessAll", branch, ct); return actor;
    }
    private async Task CoachAsync(Guid coach, Guid actor, Guid branch, DateOnly date, CancellationToken ct)
    {
        var e = await coordination.EmployeeAsync(actor, branch, date, ct);
        if (e.Id != coach) throw new UnauthorizedAccessException();
    }
    private async Task ActiveBranchAsync(Guid branch, CancellationToken ct)
    {
        if (!await db.GymBranches.AnyAsync(b => b.Id == branch && b.IsActive && b.City.IsActive, ct)) throw new BusinessRuleException("Active branch and city required.");
    }
    internal async Task ValidateCoachAsync(Guid coach, Guid branch, DateOnly date, CancellationToken ct)
    {
        if (!await db.Employees.AnyAsync(e => e.Id == coach && e.EmploymentStatus == EmploymentStatus.Active
            && e.HireDate <= date && (e.TerminationDate == null || e.TerminationDate >= date), ct)
            || !await db.EmployeeBranchAssignments.AnyAsync(a => a.EmployeeId == coach && a.BranchId == branch
                && a.StartDate <= date && (a.EndDate == null || a.EndDate >= date), ct))
            throw new BusinessRuleException("Select an active coach assigned to this branch.");
    }
    private async Task<Member> LockMemberAsync(Guid id, CancellationToken ct)
    {
        var member = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Member not found.");
        await db.Entry(member).ReloadAsync(ct); return member;
    }
    private async Task<(PrivateSubscription Sub, PrivateSubscriptionTransaction Batch, Guid Actor)> PrivateAsync(Guid id, string permission, CancellationToken ct)
    {
        var info = await db.PrivateSubscriptions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Private subscription not found.");
        var actor = await access.RequireAsync(permission, info.BranchId, ct);
        await ActiveBranchAsync(info.BranchId, ct);
        await coordination.LockBranchAsync(info.BranchId, ct); await LockMemberAsync(info.MemberId, ct);
        var sub = await db.PrivateSubscriptions.FromSqlInterpolated($"SELECT * FROM \"PrivateSubscriptions\" WHERE \"Id\"={id} FOR UPDATE").SingleAsync(ct);
        await db.Entry(sub).ReloadAsync(ct);
        var batches = await db.PrivateSubscriptionTransactions.Where(x => x.PrivateSubscriptionId == id).ToListAsync(ct);
        if (batches.Count != 1 || batches[0].OriginalAssignmentId == null) throw new BusinessRuleException("Legacy Private batches require explicit reconciliation.");
        await db.Entry(batches[0]).ReloadAsync(ct); return (sub, batches[0], actor);
    }
    public async Task<CoachingPage<CoachingMemberResult>> SearchAsync(Guid branch, bool classes, string search, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(classes ? CoachingPermissions.ClassesView : CoachingPermissions.PrivateView, branch, ct);
        if (string.IsNullOrWhiteSpace(search) || search.Length > 200) throw new BusinessRuleException("Supply a search term up to 200 characters.");
        // Operational discovery follows purchased entitlement, not home branch. Phone
        // and codes share the existing MemberSearch normalization conventions.

```

Lines 68–107

```csharp

            ? db.ClassesSubscriptions.Any(s => s.MemberId == m.Id && s.BranchId == branch)
            : db.PrivateSubscriptions.Any(s => s.MemberId == m.Id && s.BranchId == branch))), search, branch);
        var total = await q.CountAsync(ct);
        return new(await q.OrderBy(m => m.FullName).ThenBy(m => m.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new CoachingMemberResult(m.Id, m.MemberCode, m.ClientCode, m.FullName, m.PrimaryPhone)).ToListAsync(ct), total, page, pageSize);
    }
    public Task<PrivateAttendanceDetails> RegisterPrivateAsync(Guid id, PrivateAttendanceRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId);
            if (r.Notes?.Length > 2000) throw new BusinessRuleException("Notes must be at most 2000 characters.");
            var (sub, batch, actor) = await PrivateAsync(id, CoachingPermissions.RegisterPrivate, ct);
            var hash = FinancialOperations.Hash(new { id, r });
            var prior = await db.PrivateSessions.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return SessionDetails(prior); }
            var (date, _) = await coordination.BusinessDateAsync(sub.BranchId, ct);
            var assignment = await db.PrivateCoachAssignments.AsNoTracking().SingleOrDefaultAsync(x => x.PrivateSubscriptionId == id && x.EndedAt == null && x.AssignedAt <= access.Now, ct)
                ?? throw new BusinessRuleException("Package has no current coach assignment.");
            await CoachAsync(assignment.CoachEmployeeId, actor, sub.BranchId, date, ct);
            if (!await db.Members.AnyAsync(x => x.Id == sub.MemberId && x.IsActive, ct) || sub.Status == PrivateSubscriptionStatus.Cancelled
                || sub.StartDate > date || sub.EndDate < date || batch.RemainingSessions <= 0
                || await db.SaleTransactions.AnyAsync(x => x.Id == batch.SaleTransactionId && x.CancelledAt != null, ct))
                throw new BusinessRuleException("No eligible unexpired Private session remains.");
            if (await memberships.IsPaidSaleOverdueAsync(batch.SaleTransactionId, date, ct)) throw new BusinessRuleException("Private installment is overdue.");
            var checkIn = await db.CheckIns.AsNoTracking().SingleOrDefaultAsync(x => x.Id == r.GymCheckInId && x.MemberId == sub.MemberId
                && x.BranchId == sub.BranchId && x.BusinessDate == date && x.CheckInType == CheckInType.Gym && x.CheckedInAt <= access.Now, ct)
                ?? throw new BusinessRuleException("Reception Gym Check-In for this branch and BusinessDate is required.");
            var city = await db.GymBranches.Where(x => x.Id == sub.BranchId).Select(x => x.CityId).SingleAsync(ct);
            var facts = await memberships.GetGymAccessFactsAsync(sub.MemberId, date, ct);
            var membership = facts.SingleOrDefault(x => x.Id == checkIn.MemberSubscriptionId && !x.IsStandaloneVisit && x.EmployeeId == null
                && x.StartDate <= date && x.EndDate >= date && x.AccessCityId == city && (x.Status == "Active"
                    || x.Status == "Expired" && x.Mode == "SessionBased" && x.RemainingSessions == 0));
            if (membership == null) throw new BusinessRuleException("A valid regular Gym membership is required; borrowed/standalone/expired membership is insufficient.");
            if (await db.PrivateSessions.AnyAsync(x => x.GymCheckInId == checkIn.Id, ct)) throw new BusinessRuleException("This reception Check-In already has a Private session.");
            var number = batch.SessionsAdded - batch.RemainingSessions + 1;
            var session = new PrivateSession { PrivateSubscriptionId = id, PrivateSubscriptionTransactionId = batch.Id,
                CoachEmployeeId = assignment.CoachEmployeeId, PrivateCoachAssignmentId = assignment.Id,
                GymCheckInId = checkIn.Id, OperationId = r.OperationId, RequestHash = hash, BusinessDate = date,
                SessionNumber = number, SessionValue = SessionValue(batch.UnitSessionValue, batch.RemainderValue, number, batch.SessionsAdded),
                TransferredPackage = assignment.Id != batch.OriginalAssignmentId, CreatedByUserId = actor, SessionAt = access.Now, Notes = r.Notes };

```

Lines 109–128

```csharp

            access.Audit(actor, sub.BranchId, "Private.SessionRegistered", nameof(PrivateSession), session.Id, after: new { SubscriptionId = id, GymCheckInId = checkIn.Id, session.CoachEmployeeId, session.SessionValue, sub.RemainingSessions, date });
            return SessionDetails(session);
        }, ct);
    private static PrivateAttendanceDetails SessionDetails(PrivateSession s) => new(s.Id, s.PrivateSubscriptionId, s.CoachEmployeeId, s.GymCheckInId, s.BusinessDate, s.SessionAt, s.SessionNumber, s.SessionValue, s.TransferredPackage);
    public Task<PrivateAssignmentDetails> TransferAsync(Guid id, PrivateTransferRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId); FinancialOperations.Reason(r.Reason);
            var (sub, _, actor) = await PrivateAsync(id, CoachingPermissions.Transfer, ct);
            var hash = FinancialOperations.Hash(new { id, r });
            var prior = await db.PrivateCoachAssignments.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return AssignmentDetails(prior); }
            if (sub.Version != r.ExpectedVersion) throw new DbUpdateConcurrencyException();
            var (date, _) = await coordination.BusinessDateAsync(sub.BranchId, ct);
            if (sub.Status == PrivateSubscriptionStatus.Cancelled || sub.EndDate < date || sub.RemainingSessions <= 0) throw new BusinessRuleException("No transferable Private balance remains.");
            await ValidateCoachAsync(r.CoachEmployeeId, sub.BranchId, date, ct);
            var old = await db.PrivateCoachAssignments.SingleAsync(x => x.PrivateSubscriptionId == id && x.EndedAt == null, ct);
            if (old.CoachEmployeeId == r.CoachEmployeeId) throw new BusinessRuleException("Coach is already assigned.");
            old.EndedAt = access.Now; await db.SaveChangesAsync(ct);
            var row = new PrivateCoachAssignment { PrivateSubscriptionId = id, CoachEmployeeId = r.CoachEmployeeId,

```

Lines 135–172

```csharp

    public Task<PrivateExpiryDetails> SettleExpiryAsync(Guid id, PrivateExpiryRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId); FinancialOperations.Reason(r.Reason);
            var (sub, batch, _) = await PrivateAsync(id, CoachingPermissions.Expiry, ct);
            var actor = await OwnerAsync(CoachingPermissions.Expiry, sub.BranchId, ct); var hash = FinancialOperations.Hash(new { id, r });
            var prior = await db.PrivateExpirySettlements.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId!.Value, actor); return ExpiryDetails(prior); }
            var (date, _) = await coordination.BusinessDateAsync(sub.BranchId, ct);
            var period = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.Id == r.CommissionPeriodId && p.BranchId == sub.BranchId
                && p.StartBusinessDate <= date && p.EndBusinessDate >= date && p.Status == CommissionPeriodStatus.Open, ct)
                ?? throw new BusinessRuleException("Select the open branch commission period containing the settlement BusinessDate.");
            if (await IsRecognizedAsync(period.Id, ct)) throw new BusinessRuleException("Coach earnings period is finalized.");
            if (sub.EndDate >= date || sub.Status == PrivateSubscriptionStatus.Cancelled || batch.RemainingSessions <= 0
                || await db.SaleTransactions.AnyAsync(s => s.Id == batch.SaleTransactionId && s.CancelledAt != null, ct)
                || await db.PrivateExpirySettlements.AnyAsync(s => s.PrivateSubscriptionTransactionId == batch.Id, ct))
                throw new BusinessRuleException("An expired eligible unused batch may be settled exactly once.");
            var assignment = await db.PrivateCoachAssignments.AsNoTracking().SingleAsync(a => a.PrivateSubscriptionId == id && a.EndedAt == null, ct);
            // These are unused-session entitlement facts, not attendance or yet-paid earnings.
            var row = new PrivateExpirySettlement { PrivateSubscriptionId = id, PrivateSubscriptionTransactionId = batch.Id,
                CoachEmployeeId = assignment.CoachEmployeeId, PrivateCoachAssignmentId = assignment.Id, TransferredPackage = assignment.Id != batch.OriginalAssignmentId,
                CommissionPeriodId = period.Id, RemainingSessions = batch.RemainingSessions, UnitSessionValue = batch.UnitSessionValue, RemainderValue = batch.RemainderValue,
                EligibleAmount = batch.RemainingSessions * batch.UnitSessionValue + batch.RemainderValue, BusinessDate = date,
                OperationId = r.OperationId, RequestHash = hash, Reason = r.Reason.Trim(), CreatedByUserId = actor, CreatedAt = access.Now, SettledAt = access.Now };
            db.Add(row); sub.Status = PrivateSubscriptionStatus.Expired; sub.Version++;
            access.Audit(actor, sub.BranchId, "Private.ExpiryAuthorized", nameof(PrivateExpirySettlement), row.Id, after: ExpiryDetails(row));
            return ExpiryDetails(row);
        }, ct);
    private static PrivateExpiryDetails ExpiryDetails(PrivateExpirySettlement x) => new(x.Id, x.PrivateSubscriptionId, x.CoachEmployeeId, x.RemainingSessions, x.EligibleAmount, x.TransferredPackage, x.BusinessDate, x.SettledAt, x.Reason);
    public async Task<PrivateDetails> PrivateDetailsAsync(Guid id, CancellationToken ct = default)
    {
        var s = await db.PrivateSubscriptions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Private subscription not found.");
        await access.RequireAsync(CoachingPermissions.PrivateView, s.BranchId, ct);
        var batch = await db.PrivateSubscriptionTransactions.AsNoTracking().Where(x => x.PrivateSubscriptionId == id).ToListAsync(ct);
        if (batch.Count != 1) throw new BusinessRuleException("Legacy multiple batches require reconciliation."); var b = batch[0];
        var assignments = await db.PrivateCoachAssignments.AsNoTracking().Where(x => x.PrivateSubscriptionId == id).OrderBy(x => x.AssignedAt).ThenBy(x => x.Id).ToListAsync(ct);
        return new(s.Id, s.MemberId, s.BranchId, s.StartDate, s.EndDate, s.TotalSessions, s.TotalSessions - s.RemainingSessions,
            s.RemainingSessions, s.Version, s.Status == PrivateSubscriptionStatus.Cancelled, b.PackageNameSnapshot, b.NetAmount,

```

Lines 174–181

```csharp

    }
    public async Task<CoachingPage<PrivateAttendanceDetails>> PrivateHistoryAsync(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); var branch = await db.PrivateSubscriptions.Where(x => x.Id == id).Select(x => (Guid?)x.BranchId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Private subscription not found.");
        await access.RequireAsync(CoachingPermissions.PrivateView, branch, ct); var q = db.PrivateSessions.AsNoTracking().Where(x => x.PrivateSubscriptionId == id);
        var total = await q.CountAsync(ct); var rows = await q.OrderByDescending(x => x.SessionAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(SessionDetails).ToArray(), total, page, pageSize);
    }

```

## CampionsSystem.BLL/Coaching/CoachingContracts.cs

Lines 5–10

```csharp


public static class CoachingPermissions
{
    public const string PrivateView = "Private.View", PrivateManage = "Private.Manage",
        RegisterPrivate = "Private.RegisterSession", Transfer = "Private.Transfer", Expiry = "Private.SettleExpiry",
        ClassesView = "Classes.View", ClassesManage = "Classes.Manage", RegisterClass = "Classes.RegisterAttendance",

```

Lines 22–42

```csharp

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PrivateAttendanceRequest(Guid OperationId, Guid GymCheckInId, string? Notes = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PrivateTransferRequest(Guid OperationId, Guid CoachEmployeeId, int ExpectedVersion, string Reason);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PrivateExpiryRequest(Guid OperationId, Guid CommissionPeriodId, string Reason);
public sealed record PrivateAssignmentDetails(Guid Id, Guid CoachEmployeeId, DateTimeOffset AssignedAt, DateTimeOffset? EndedAt, string? Reason);
public sealed record PrivateAttendanceDetails(Guid Id, Guid SubscriptionId, Guid CoachEmployeeId, Guid GymCheckInId,
    DateOnly? BusinessDate, DateTimeOffset SessionAt, int? SessionNumber, decimal? SessionValue, bool? TransferredPackage);
public sealed record PrivateDetails(Guid Id, Guid MemberId, Guid BranchId, DateOnly StartDate, DateOnly EndDate,
    int PurchasedSessions, int UsedSessions, int RemainingSessions, int Version, bool Cancelled,
    string? PackageName, decimal Price, decimal UnitSessionValue, decimal RemainderValue,
    Guid? OriginalCoachEmployeeId, IReadOnlyList<PrivateAssignmentDetails> Assignments);
public sealed record PrivateExpiryDetails(Guid Id, Guid SubscriptionId, Guid CoachEmployeeId, int UnusedSessions,
    decimal EligibleValue, bool? TransferredPackage, DateOnly? BusinessDate, DateTimeOffset SettledAt, string? Reason);
public sealed record CoachingMemberResult(Guid Id, string MemberCode, long? ClientCode, string Name, string Phone);
public sealed record CoachingPage<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
public sealed record ClassesSubscriptionDetails(Guid Id, Guid MemberId, Guid BranchId, Guid SaleId, string PackageName,
    DateOnly StartDate, DateOnly EndDate, int PurchasedSessions, int RemainingSessions, decimal Price, bool Cancelled, int Version);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ClassTypeRequest(Guid BranchId, string Name, Guid CoachEmployeeId, bool IsActive = true, string? Description = null);

```

Lines 44–55

```csharp

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ClassOccurrenceRequest(Guid OperationId, Guid ClassTypeId, Guid CoachEmployeeId, DateTimeOffset StartsAt);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CompleteClassRequest(Guid OperationId, int ExpectedVersion, bool Cancelled = false);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ClassAttendanceRequest(Guid OperationId, Guid MemberId, Guid? ClassesSubscriptionId = null);
public sealed record ClassOccurrenceDetails(Guid Id, Guid ClassTypeId, Guid BranchId, Guid CoachEmployeeId,
    DateOnly? BusinessDate, DateTimeOffset StartsAt, DateTimeOffset? EndedAt, string Status, int Attendance, int Version);
public sealed record ClassAttendanceDetails(Guid Id, Guid MemberId, Guid SubscriptionId, Guid ClassSessionId,
    Guid CoachEmployeeId, DateOnly BusinessDate, int RemainingSessions, DateTimeOffset RegisteredAt);
public sealed record ClassCoachStatistics(Guid CoachEmployeeId, int CompletedClasses, int TotalAttendance, decimal AverageAttendance,
    decimal? Target, bool? TargetReached, IReadOnlyList<ClassOccurrenceDetails> Occurrences, string CompensationStatus = "NotConfigured");

```

Lines 63–68

```csharp

    decimal? NormalRate, decimal? ProjectedEarnings, decimal DeferredValue);
public sealed record CoachEarningsProgress(Guid PeriodId, bool Finalized, IReadOnlyList<CoachProgress> Coaches);
public sealed record CoachEarningDetails(Guid Id, Guid CoachEmployeeId, Guid PeriodId, Guid SaleId, Guid? SessionId,
    Guid? ExpirySettlementId, decimal EligibleValue, decimal Rate, decimal Earned, bool TransferredPackage, DateOnly BusinessDate);
public sealed record CoachBalance(Guid BranchId, Guid CoachEmployeeId, decimal Earned, decimal Paid, decimal Outstanding);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]

```

Lines 70–73

```csharp

    Guid FinancialAccountId, Guid ExpenseCategoryId, string Reason, string? ReceiptReference = null);
public sealed record CoachPayoutDetails(Guid Id, Guid BranchId, Guid CoachEmployeeId, Guid ExpenseId,
    Guid AccountId, string Method, decimal Amount, decimal BalanceBefore, decimal BalanceAfter,
    DateOnly BusinessDate, DateTimeOffset PaidAt, Guid AuthorizedByUserId);

```

## CampionsSystem.BLL/Coaching/CoachingService.Payouts.cs

Lines 10–37

```csharp

    public Task<CoachPayoutDetails> PayAsync(CoachPayoutRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.Amount, true); FinancialOperations.Reason(r.Reason);
            if (r.ReceiptReference?.Length > 512) throw new BusinessRuleException("Receipt reference is too long.");
            var actor = await OwnerAsync(CoachingPermissions.Pay, r.BranchId, ct); await coordination.LockBranchAsync(r.BranchId, ct);
            var hash = FinancialOperations.Hash(r);
            var prior = await db.CoachPayouts.AsNoTracking().SingleOrDefaultAsync(p => p.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.AuthorizedByUserId, actor); return await PayoutDetailsAsync(prior, ct); }
            var balance = await BalanceCoreAsync(r.BranchId, r.CoachEmployeeId, ct);
            if (balance.Outstanding < r.Amount) throw new BusinessRuleException("Payout exceeds this coach's recognized outstanding branch earnings.");
            var shift = await coordination.RequireShiftAsync(r.BranchId, actor, ct);
            var account = await coordination.AccountAsync(r.BranchId, r.FinancialAccountId, ct);
            if (account.AccountType != FinancialAccountType.Cash && await coordination.BookAsync(account.Id, ct) < r.Amount) throw new BusinessRuleException("Insufficient financial account balance.");
            if (!await db.ExpenseCategories.AnyAsync(c => c.Id == r.ExpenseCategoryId && c.IsActive, ct)) throw new BusinessRuleException("Select an active expense category.");
            var (date, day) = await coordination.BusinessDateAsync(r.BranchId, ct);
            var expense = new Expense { BranchId = r.BranchId, ShiftId = shift.Id, RecordingEmployeeId = shift.EmployeeId,
                ExpenseCategoryId = r.ExpenseCategoryId, FinancialAccountId = account.Id, OperationId = r.OperationId, RequestHash = hash,
                Amount = r.Amount, Description = r.Reason.Trim(), ReceiptReference = r.ReceiptReference, ExpenseDate = access.Now, PaidAt = access.Now,
                CreatedAt = access.Now, CreatedByUserId = actor, ApprovedByUserId = actor, BusinessDate = date, BusinessDayId = day, Status = ExpenseStatus.Recorded };
            var payout = new CoachPayout { BranchId = r.BranchId, CoachEmployeeId = r.CoachEmployeeId, Expense = expense,
                OperationId = r.OperationId, RequestHash = hash, Amount = r.Amount, BalanceBefore = balance.Outstanding,
                BalanceAfter = balance.Outstanding - r.Amount, AuthorizedByUserId = actor, PaidAt = access.Now, BusinessDate = date };
            db.Add(payout);
            var earnings = await db.CoachEarnings.AsNoTracking().Where(e => e.BranchId == r.BranchId && e.CoachEmployeeId == r.CoachEmployeeId)
                .OrderBy(e => e.BusinessDate).ThenBy(e => e.CreatedAt).ThenBy(e => e.Id).ToListAsync(ct);
            var remaining = r.Amount;
            foreach (var earning in earnings)

```

Lines 42–50

```csharp

                db.Add(new CoachPayoutAllocation { CoachPayout = payout, CoachEarningId = earning.Id, Amount = take }); remaining -= take;
                if (remaining == 0) break;
            }
            if (remaining != 0) throw new BusinessRuleException("Earning allocation history does not reconcile.");
            await coordination.PostAsync(new FinancialTransaction { BranchId = r.BranchId, FinancialAccountId = account.Id, ShiftId = shift.Id,
                Expense = expense, SourceType = FinancialSourceType.Expense, TransactionType = FinancialTransactionType.Debit, Amount = r.Amount,
                BusinessDate = date, BusinessDayId = day, OccurredAt = access.Now, CreatedByUserId = actor, Notes = r.Reason.Trim() }, ct);
            access.Audit(actor, r.BranchId, "CoachEarnings.Paid", nameof(CoachPayout), payout.Id, after: new { r.CoachEmployeeId, r.Amount, ExpenseId = expense.Id, payout.BalanceBefore, payout.BalanceAfter, AccountId = account.Id, account.AccountType, date });
            return await PayoutDetailsAsync(payout, ct);

```

Lines 53–64

```csharp

    {
        var e = p.Expense ?? await db.Expenses.AsNoTracking().SingleAsync(e => e.Id == p.ExpenseId, ct);
        var method = await db.FinancialAccounts.Where(a => a.Id == e.FinancialAccountId).Select(a => a.AccountType).SingleAsync(ct);
        return new(p.Id, p.BranchId, p.CoachEmployeeId, p.ExpenseId, e.FinancialAccountId, method.ToString(), p.Amount, p.BalanceBefore, p.BalanceAfter, p.BusinessDate, p.PaidAt, p.AuthorizedByUserId);
    }
    public async Task<CoachingPage<CoachPayoutDetails>> PayoutHistoryAsync(Guid branch, Guid coach, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.EarningsView, branch, ct);
        var q = db.CoachPayouts.AsNoTracking().Where(p => p.BranchId == branch && p.CoachEmployeeId == coach); var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(p => p.PaidAt).ThenBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var details = new List<CoachPayoutDetails>(); foreach (var p in rows) details.Add(await PayoutDetailsAsync(p, ct)); return new(details, total, page, pageSize);
    }

```

## CampionsSystem.BLL/Coaching/CoachingService.Packages.cs

Lines 10–31

```csharp

    public Task<CoachingPackageDetails> SavePackageAsync(Guid? id, CoachingPackageRequest r, bool classes, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            var actor = await access.RequireAsync(classes ? CoachingPermissions.ClassesManage : CoachingPermissions.PrivateManage, r.BranchId, ct);
            FinancialOperations.Money(r.Price, true);
            if (string.IsNullOrWhiteSpace(r.Name) || r.Name.Trim().Length > 256 || r.SessionCount is < 1 or > 100000 || r.DurationDays is < 1 or > 36500)
                throw new BusinessRuleException("Invalid package name, session count or duration.");
            if (!await db.GymBranches.AnyAsync(b => b.Id == r.BranchId && b.IsActive && b.City.IsActive, ct)) throw new BusinessRuleException("Active branch/city required.");
            Guid key;
            if (classes)
            {
                var p = id == null ? new ClassesPackage { BranchId = r.BranchId } : await db.ClassesPackages.SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new BranchNotFoundException("Package not found.");
                if (p.BranchId != r.BranchId) throw new UnauthorizedAccessException();
                p.Name = r.Name.Trim(); p.Price = r.Price; p.SessionCount = r.SessionCount; p.DurationDays = r.DurationDays; p.IsActive = r.IsActive;
                if (id == null) db.Add(p); key = p.Id;
            }
            else
            {
                var p = id == null ? new PrivatePackage { BranchId = r.BranchId } : await db.PrivatePackages.SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new BranchNotFoundException("Package not found.");
                if (p.BranchId != r.BranchId) throw new UnauthorizedAccessException();
                p.Name = r.Name.Trim(); p.Price = r.Price; p.SessionCount = r.SessionCount; p.DurationDays = r.DurationDays; p.IsActive = r.IsActive;
                if (id == null) db.Add(p); key = p.Id;

```

Lines 35–58

```csharp

        }, ct);
    public async Task<CoachingPage<CoachingPackageDetails>> PackagesAsync(Guid branch, bool classes, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(classes ? CoachingPermissions.ClassesView : CoachingPermissions.PrivateView, branch, ct);
        IQueryable<CoachingPackageDetails> q = classes
            ? db.ClassesPackages.AsNoTracking().Where(p => p.BranchId == branch).Select(p => new CoachingPackageDetails(p.Id, p.BranchId, p.Name, p.SessionCount, p.DurationDays, p.Price, p.IsActive))
            : db.PrivatePackages.AsNoTracking().Where(p => p.BranchId == branch).Select(p => new CoachingPackageDetails(p.Id, p.BranchId, p.Name, p.SessionCount, p.DurationDays, p.Price, p.IsActive));
        var total = await q.CountAsync(ct); return new(await q.OrderBy(p => p.Name).ThenBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct), total, page, pageSize);
    }
    public async Task<CoachingPage<PrivateDetails>> PrivateSubscriptionsAsync(Guid branch, Guid member, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.PrivateView, branch, ct);
        var q = db.PrivateSubscriptions.AsNoTracking().Where(s => s.BranchId == branch && s.MemberId == member); var total = await q.CountAsync(ct);
        var ids = await q.OrderByDescending(s => s.CreatedAt).ThenBy(s => s.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(s => s.Id).ToListAsync(ct);
        var rows = new List<PrivateDetails>(); foreach (var id in ids) rows.Add(await PrivateDetailsAsync(id, ct)); return new(rows, total, page, pageSize);
    }
    public async Task<CoachingPage<PrivateExpiryDetails>> ExpiryHistoryAsync(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); var branch = await db.PrivateSubscriptions.Where(s => s.Id == id).Select(s => (Guid?)s.BranchId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Subscription not found.");
        await access.RequireAsync(CoachingPermissions.PrivateView, branch, ct);
        var q = db.PrivateExpirySettlements.AsNoTracking().Where(s => s.PrivateSubscriptionId == id); var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(s => s.SettledAt).ThenBy(s => s.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct); return new(rows.Select(ExpiryDetails).ToArray(), total, page, pageSize);
    }
}

```

## CampionsSystem.BLL/Coaching/CoachingService.Earnings.cs

Lines 12–30

```csharp

    public Task<CoachTargetDetails> ConfigureTargetsAsync(CoachTargetRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId);
            var period = await PeriodAsync(r.CommissionPeriodId, ct); var actor = await OwnerAsync(CoachingPermissions.Targets, period.BranchId, ct);
            await coordination.LockBranchAsync(period.BranchId, ct); var hash = FinancialOperations.Hash(r);
            var prior = await db.CoachTargetRules.AsNoTracking().SingleOrDefaultAsync(t => t.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return TargetDetails(prior); }
            if (r.PrivateTarget == null && r.ClassesAverageTarget == null || r.PrivateTarget <= 0 || r.ClassesAverageTarget <= 0
                || r.ClassesAverageTarget != null && (decimal.Round(r.ClassesAverageTarget.Value, 4) != r.ClassesAverageTarget || r.ClassesAverageTarget > 99999999999999.9999m))
                throw new BusinessRuleException("Supply an explicit positive Private target and/or Classes average target (up to four decimals).");
            if (r.PrivateTarget != null) FinancialOperations.Money(r.PrivateTarget.Value, true);
            if (await IsRecognizedAsync(period.Id, ct)) throw new BusinessRuleException("Coach period has finalized earnings.");
            if (!await db.Employees.AnyAsync(e => e.Id == r.CoachEmployeeId, ct)) throw new BranchNotFoundException("Coach not found.");
            if (!await db.EmployeeBranchAssignments.AnyAsync(a => a.EmployeeId == r.CoachEmployeeId && a.BranchId == period.BranchId
                && a.StartDate <= period.EndBusinessDate && (a.EndDate == null || a.EndDate >= period.StartBusinessDate), ct)) throw new BusinessRuleException("Coach has no assignment overlapping this branch period.");
            if (await db.CoachTargetRules.AnyAsync(t => t.CommissionPeriodId == period.Id && t.CoachEmployeeId == r.CoachEmployeeId, ct)) throw new BusinessRuleException("Targets are already configured for this coach/period; immutable configuration preserves history.");
            var row = new CoachTargetRule { CommissionPeriodId = period.Id, CoachEmployeeId = r.CoachEmployeeId,
                PrivateTarget = r.PrivateTarget, ClassesAverageTarget = r.ClassesAverageTarget, OperationId = r.OperationId, RequestHash = hash, CreatedByUserId = actor, CreatedAt = access.Now };

```

Lines 33–76

```csharp

    private static CoachTargetDetails TargetDetails(CoachTargetRule t) => new(t.Id, t.CommissionPeriodId, t.CoachEmployeeId, t.PrivateTarget, t.ClassesAverageTarget);
    private async Task<CommissionPeriod> PeriodAsync(Guid id, CancellationToken ct)
    {
        var p = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new BranchNotFoundException("Period not found.");
        if (p.StartBusinessDate == null || p.EndBusinessDate == null) throw new BusinessRuleException("Legacy period needs explicit BusinessDate reconciliation."); return p;
    }
    public async Task<IReadOnlyList<CoachTargetDetails>> TargetsAsync(Guid periodId, CancellationToken ct = default)
    {
        var p = await PeriodAsync(periodId, ct); await access.RequireAsync(CoachingPermissions.EarningsView, p.BranchId, ct);
        return await db.CoachTargetRules.AsNoTracking().Where(t => t.CommissionPeriodId == p.Id).OrderBy(t => t.CoachEmployeeId)
            .Select(t => new CoachTargetDetails(t.Id, t.CommissionPeriodId, t.CoachEmployeeId, t.PrivateTarget, t.ClassesAverageTarget)).ToListAsync(ct);
    }
    private sealed record EarningSource(Guid Sale, Guid Coach, Guid? Session, Guid? Expiry, decimal Value, bool Transferred, int Order, decimal FundedBefore = 0, decimal FundedNow = 0, decimal PriorExactCommission = 0, decimal PriorRoundedCommission = 0);
    private async Task<List<EarningSource>> FundingAsync(CommissionPeriod period, CancellationToken ct)
    {
        var end = period.EndBusinessDate!.Value;
        var sessions = await db.PrivateSessions.AsNoTracking().Where(s => s.OperationId != null && s.BusinessDate <= end
            && s.PrivateSubscription.BranchId == period.BranchId)
            .Select(s => new EarningSource(s.PrivateSubscriptionTransaction.SaleTransactionId, s.CoachEmployeeId, s.Id, null,
                s.SessionValue!.Value, s.TransferredPackage!.Value, s.SessionNumber!.Value, 0, 0)).ToListAsync(ct);
        var expiries = await db.PrivateExpirySettlements.AsNoTracking().Where(s => s.OperationId != null && s.BusinessDate <= end && s.PrivateSubscription.BranchId == period.BranchId)
            .Select(s => new EarningSource(s.PrivateSubscriptionTransaction.SaleTransactionId, s.CoachEmployeeId, null, s.Id, s.EligibleAmount,
                s.TransferredPackage!.Value, int.MaxValue, 0, 0)).ToListAsync(ct);
        var sources = sessions.Concat(expiries).ToArray(); var sales = sources.Select(s => s.Sale).Distinct().ToArray();
        var payments = await db.Payments.AsNoTracking().Include(p => p.Parts).Where(p => sales.Contains(p.SaleTransactionId)
            && p.Status == PaymentStatus.Completed && p.PaidAt <= access.Now).ToListAsync(ct);
        if (payments.Any(p => p.Amount != p.Parts.Sum(part => part.Amount))) throw new BusinessRuleException("Collections and PaymentParts do not reconcile.");
        // Reuse actual newest-first refund allocations, including refunds received
        // after the evaluation end but before recognition. Refunded money is never funded.
        var refunded = await new Commissions.CommissionSources(db, access).RefundedAsync(payments, ct);
        var previous = await db.CoachEarnings.AsNoTracking().Where(e => sales.Contains(e.SaleTransactionId)).ToListAsync(ct);
        if (previous.Any(e => e.CommissionPeriodId != period.Id && e.BusinessDate > end)) throw new BusinessRuleException("Recognize coach periods in chronological order; future funded evidence exists.");
        var result = new List<EarningSource>();
        foreach (var group in sources.GroupBy(s => s.Sale).OrderBy(g => g.Key))
        {
            var funding = payments.Where(p => p.SaleTransactionId == group.Key && p.BusinessDate <= end).Sum(p => p.Amount - refunded.GetValueOrDefault(p.Id));
            if (funding < 0) throw new BusinessRuleException("Refunded value exceeds collected value.");
            foreach (var source in group.OrderBy(s => s.Order).ThenBy(s => s.Session).ThenBy(s => s.Expiry))
            {
                var allowed = Math.Min(source.Value, funding); funding -= allowed;
                var old = previous.Where(e => source.Session != null ? e.PrivateSessionId == source.Session : e.PrivateExpirySettlementId == source.Expiry).Sum(e => e.EligibleValue);
                if (old > allowed) throw new BusinessRuleException("Historical funding no longer reconciles; Owner adjustment policy required.");
                var priorSlices = previous.Where(e => source.Session != null ? e.PrivateSessionId == source.Session : e.PrivateExpirySettlementId == source.Expiry).ToArray();
                result.Add(source with { FundedBefore = old, FundedNow = allowed - old, PriorExactCommission = priorSlices.Sum(e => e.EligibleValue * e.Rate / 100m), PriorRoundedCommission = priorSlices.Sum(e => e.Amount) });

```

Lines 78–83

```csharp

        }
        return result;
    }
    private static decimal SliceCommission(EarningSource source, decimal rate) => decimal.Round(source.PriorExactCommission + source.FundedNow * rate / 100m, 2, MidpointRounding.AwayFromZero) - source.PriorRoundedCommission;
    private async Task<CoachEarningsProgress> ProgressCoreAsync(CommissionPeriod period, CancellationToken ct)
    {

```

Lines 88–93

```csharp

        }
        var sources = await FundingAsync(period, ct);
        var targets = await db.CoachTargetRules.AsNoTracking().Where(t => t.CommissionPeriodId == period.Id).ToDictionaryAsync(t => t.CoachEmployeeId, ct);
        return new(period.Id, false, sources.GroupBy(s => s.Coach).OrderBy(g => g.Key).Select(g =>
        {
            var total = g.Sum(s => s.FundedNow); var target = targets.GetValueOrDefault(g.Key)?.PrivateTarget;

```

Lines 99–104

```csharp

    }
    public async Task<CoachEarningsProgress> ProgressAsync(Guid periodId, CancellationToken ct = default)
    {
        var period = await PeriodAsync(periodId, ct); await access.RequireAsync(CoachingPermissions.EarningsView, period.BranchId, ct);
        if (db.Database.CurrentTransaction != null) return await ProgressCoreAsync(period, ct);
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);

```

Lines 107–112

```csharp

    public Task<CoachEarningsProgress> RecognizeAsync(RecognizeCoachEarningsRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId);
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"coach-recognition:" + r.OperationId}, 0))", ct);
            var p = await PeriodAsync(r.CommissionPeriodId, ct);

```

Lines 116–130

```csharp

            if (operation != null)
            {
                using var evidence = JsonDocument.Parse(operation.NewValues!);
                if (operation.UserId != actor || evidence.RootElement.GetProperty("PeriodId").GetGuid() != p.Id || evidence.RootElement.GetProperty("Hash").GetString() != hash) throw new BusinessRuleException("Operation belongs to another recognition request.");
                return await ProgressCoreAsync(p, ct);
            }
            if (await IsRecognizedAsync(p.Id, ct)) throw new BusinessRuleException("Coach earnings period is already finalized.");
            var (date, _) = await coordination.BusinessDateAsync(p.BranchId, ct);
            if (date <= p.EndBusinessDate) throw new BusinessRuleException("Recognize after the period's last BusinessDate ends, when its target result is final.");
            if (await db.CommissionPeriods.AnyAsync(x => x.BranchId == p.BranchId && x.EndBusinessDate < p.StartBusinessDate
                && !db.AuditLogs.Any(a => a.EntityId == x.Id && a.Action == "CoachEarnings.PeriodRecognized"), ct)) throw new BusinessRuleException("Recognize preceding branch coach periods first (including zero earnings).");
            var sources = await FundingAsync(p, ct); var progress = await ProgressCoreAsync(p, ct);
            if (progress.Coaches.Any(c => c.ProjectedEarnings == null)) throw new BusinessRuleException("Configure an independent Private target for every original coach with funded value.");
            var rules = await db.CoachTargetRules.AsNoTracking().Where(t => t.CommissionPeriodId == p.Id).ToDictionaryAsync(t => t.CoachEmployeeId, ct);
            foreach (var source in sources.Where(s => s.FundedNow > 0))

```

Lines 135–140

```csharp

                    CoachTargetRuleId = rule?.Id, SaleTransactionId = source.Sale, PrivateSessionId = source.Session, PrivateExpirySettlementId = source.Expiry,
                    EligibleValue = source.FundedNow, Rate = rate, Amount = SliceCommission(source, rate),
                    TransferredPackage = source.Transferred, TargetProgress = coach.EligibleValue, Target = coach.Target,
                    RecognitionOperationId = r.OperationId, RequestHash = hash, BusinessDate = p.EndBusinessDate!.Value, CreatedAt = access.Now, CreatedByUserId = actor };
                db.Add(earning); access.Audit(actor, p.BranchId, "CoachEarnings.Recognized", nameof(CoachEarning), earning.Id, after: new { earning.CoachEmployeeId, earning.EligibleValue, earning.Rate, earning.Amount, source.Session, source.Expiry });
            }

```

Lines 150–155

```csharp

    }
    public async Task<CoachBalance> BalanceAsync(Guid branch, Guid coach, CancellationToken ct = default)
    {
        await access.RequireAsync(CoachingPermissions.EarningsView, branch, ct);
        if (db.Database.CurrentTransaction != null) return await BalanceCoreAsync(branch, coach, ct);
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);

```

Lines 157–166

```csharp

    }
    public async Task<CoachingPage<CoachEarningDetails>> EarningsHistoryAsync(Guid branch, Guid coach, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.EarningsView, branch, ct);
        var q = db.CoachEarnings.AsNoTracking().Where(e => e.BranchId == branch && e.CoachEmployeeId == coach); var count = await q.CountAsync(ct);
        return new(await q.OrderByDescending(e => e.BusinessDate).ThenBy(e => e.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(e =>
            new CoachEarningDetails(e.Id, e.CoachEmployeeId, e.CommissionPeriodId, e.SaleTransactionId, e.PrivateSessionId, e.PrivateExpirySettlementId,
                e.EligibleValue, e.Rate, e.Amount, e.TransferredPackage, e.BusinessDate)).ToListAsync(ct), count, page, pageSize);
    }
}

```
