# Coaching rule/validation evidence

Completed source at 476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80. Source excerpts preserve exact input, scope, ordering, financial and retry guards. Use the endpoint catalog and workflow prose for integration; this is evidence, not runtime execution.

## CampionsSystem.BLL/Coaching/CoachingContracts.cs
```csharp
// ...
5:
6: public static class CoachingPermissions
7: {
8:     public const string PrivateView = "Private.View", PrivateManage = "Private.Manage",
9:         RegisterPrivate = "Private.RegisterSession", Transfer = "Private.Transfer", Expiry = "Private.SettleExpiry",
10:         ClassesView = "Classes.View", ClassesManage = "Classes.Manage", RegisterClass = "Classes.RegisterAttendance",
// ...
22: [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
23: public sealed record PrivateAttendanceRequest(Guid OperationId, Guid GymCheckInId, string? Notes = null);
24: [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
25: public sealed record PrivateTransferRequest(Guid OperationId, Guid CoachEmployeeId, int ExpectedVersion, string Reason);
26: [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
27: public sealed record PrivateExpiryRequest(Guid OperationId, Guid CommissionPeriodId, string Reason);
28: public sealed record PrivateAssignmentDetails(Guid Id, Guid CoachEmployeeId, DateTimeOffset AssignedAt, DateTimeOffset? EndedAt, string? Reason);
29: public sealed record PrivateAttendanceDetails(Guid Id, Guid SubscriptionId, Guid CoachEmployeeId, Guid GymCheckInId,
30:     DateOnly? BusinessDate, DateTimeOffset SessionAt, int? SessionNumber, decimal? SessionValue, bool? TransferredPackage);
31: public sealed record PrivateDetails(Guid Id, Guid MemberId, Guid BranchId, DateOnly StartDate, DateOnly EndDate,
32:     int PurchasedSessions, int UsedSessions, int RemainingSessions, int Version, bool Cancelled,
33:     string? PackageName, decimal Price, decimal UnitSessionValue, decimal RemainderValue,
34:     Guid? OriginalCoachEmployeeId, IReadOnlyList<PrivateAssignmentDetails> Assignments);
35: public sealed record PrivateExpiryDetails(Guid Id, Guid SubscriptionId, Guid CoachEmployeeId, int UnusedSessions,
36:     decimal EligibleValue, bool? TransferredPackage, DateOnly? BusinessDate, DateTimeOffset SettledAt, string? Reason);
37: public sealed record CoachingMemberResult(Guid Id, string MemberCode, long? ClientCode, string Name, string Phone);
38: public sealed record CoachingPage<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
39: public sealed record ClassesSubscriptionDetails(Guid Id, Guid MemberId, Guid BranchId, Guid SaleId, string PackageName,
40:     DateOnly StartDate, DateOnly EndDate, int PurchasedSessions, int RemainingSessions, decimal Price, bool Cancelled, int Version);
41: [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
42: public sealed record ClassTypeRequest(Guid BranchId, string Name, Guid CoachEmployeeId, bool IsActive = true, string? Description = null);
// ...
44: [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
45: public sealed record ClassOccurrenceRequest(Guid OperationId, Guid ClassTypeId, Guid CoachEmployeeId, DateTimeOffset StartsAt);
46: [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
47: public sealed record CompleteClassRequest(Guid OperationId, int ExpectedVersion, bool Cancelled = false);
48: [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
49: public sealed record ClassAttendanceRequest(Guid OperationId, Guid MemberId, Guid? ClassesSubscriptionId = null);
50: public sealed record ClassOccurrenceDetails(Guid Id, Guid ClassTypeId, Guid BranchId, Guid CoachEmployeeId,
51:     DateOnly? BusinessDate, DateTimeOffset StartsAt, DateTimeOffset? EndedAt, string Status, int Attendance, int Version);
52: public sealed record ClassAttendanceDetails(Guid Id, Guid MemberId, Guid SubscriptionId, Guid ClassSessionId,
53:     Guid CoachEmployeeId, DateOnly BusinessDate, int RemainingSessions, DateTimeOffset RegisteredAt);
54: public sealed record ClassCoachStatistics(Guid CoachEmployeeId, int CompletedClasses, int TotalAttendance, decimal AverageAttendance,
55:     decimal? Target, bool? TargetReached, IReadOnlyList<ClassOccurrenceDetails> Occurrences, string CompensationStatus = "NotConfigured");
// ...
63:     decimal? NormalRate, decimal? ProjectedEarnings, decimal DeferredValue);
64: public sealed record CoachEarningsProgress(Guid PeriodId, bool Finalized, IReadOnlyList<CoachProgress> Coaches);
65: public sealed record CoachEarningDetails(Guid Id, Guid CoachEmployeeId, Guid PeriodId, Guid SaleId, Guid? SessionId,
66:     Guid? ExpirySettlementId, decimal EligibleValue, decimal Rate, decimal Earned, bool TransferredPackage, DateOnly BusinessDate);
67: public sealed record CoachBalance(Guid BranchId, Guid CoachEmployeeId, decimal Earned, decimal Paid, decimal Outstanding);
68: [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
// ...
70:     Guid FinancialAccountId, Guid ExpenseCategoryId, string Reason, string? ReceiptReference = null);
71: public sealed record CoachPayoutDetails(Guid Id, Guid BranchId, Guid CoachEmployeeId, Guid ExpenseId,
72:     Guid AccountId, string Method, decimal Amount, decimal BalanceBefore, decimal BalanceAfter,
73:     DateOnly BusinessDate, DateTimeOffset PaidAt, Guid AuthorizedByUserId);
```
## CampionsSystem.BLL/Coaching/CoachingService.Classes.cs
```csharp
// ...
11:     public Task<ClassTypeDetails> SaveClassTypeAsync(Guid? id, ClassTypeRequest r, CancellationToken ct = default)
12:         => coordination.TransactionAsync(async () =>
13:         {
14:             var actor = await access.RequireAsync(CoachingPermissions.ClassesManage, r.BranchId, ct);
15:             await ActiveBranchAsync(r.BranchId, ct);
16:             if (string.IsNullOrWhiteSpace(r.Name) || r.Name.Trim().Length > 256 || r.Description?.Length > 2000) throw new BusinessRuleException("Invalid class type name/description.");
17:             var (date, _) = await coordination.BusinessDateAsync(r.BranchId, ct);
18:             await ValidateCoachAsync(r.CoachEmployeeId, r.BranchId, date, ct);
19:             var row = id == null ? new GroupClass { BranchId = r.BranchId } : await db.GroupClasses.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Class type not found.");
20:             if (row.BranchId != r.BranchId) throw new UnauthorizedAccessException();
21:             row.Name = r.Name.Trim(); row.CoachEmployeeId = r.CoachEmployeeId; row.Description = r.Description; row.IsActive = r.IsActive;
22:             if (id == null) db.Add(row);
// ...
25:         }, ct);
26:     public async Task<CoachingPage<ClassTypeDetails>> ClassTypesAsync(Guid branch, int page = 1, int pageSize = 25, CancellationToken ct = default)
27:     {
28:         Page(page, pageSize); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
29:         var q = db.GroupClasses.AsNoTracking().Where(x => x.BranchId == branch); var total = await q.CountAsync(ct);
30:         return new(await q.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
31:             .Select(x => new ClassTypeDetails(x.Id, x.BranchId, x.Name, x.CoachEmployeeId, x.IsActive, x.Description)).ToListAsync(ct), total, page, pageSize);
32:     }
33:     public Task<ClassOccurrenceDetails> CreateOccurrenceAsync(ClassOccurrenceRequest r, CancellationToken ct = default)
34:         => coordination.TransactionAsync(async () =>
35:         {
36:             FinancialOperations.Operation(r.OperationId);
37:             var group = await db.GroupClasses.AsNoTracking().SingleOrDefaultAsync(g => g.Id == r.ClassTypeId && g.IsActive, ct) ?? throw new BranchNotFoundException("Active class type not found.");
38:             var actor = await access.RequireAsync(CoachingPermissions.ClassesManage, group.BranchId, ct);
39:             await ActiveBranchAsync(group.BranchId, ct);
40:             await coordination.LockBranchAsync(group.BranchId, ct); var hash = FinancialOperations.Hash(r);
41:             var prior = await db.ClassSessions.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
42:             if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId!.Value, actor); return await OccurrenceDetailsAsync(prior, ct); }
43:             var (date, day) = await coordination.BusinessDateAsync(group.BranchId, ct);
44:             await ValidateCoachAsync(r.CoachEmployeeId, group.BranchId, date, ct);
45:             if (r.StartsAt == default || r.StartsAt < access.Now || access.LocalDate(r.StartsAt) != access.Today)
46:                 throw new BusinessRuleException("Occurrence must use the current operational BusinessDate; future scheduling is outside this workflow.");
47:             var row = new ClassSession { GroupClassId = group.Id, BranchId = group.BranchId, CoachEmployeeId = r.CoachEmployeeId,
48:                 StartedAt = r.StartsAt.ToUniversalTime(), Status = ClassSessionStatus.Scheduled, BusinessDate = date, BusinessDayId = day,
49:                 OperationId = r.OperationId, RequestHash = hash, CreatedByUserId = actor, CreatedAt = access.Now };
50:             db.Add(row); access.Audit(actor, group.BranchId, "Classes.OccurrenceCreated", nameof(ClassSession), row.Id, after: r);
// ...
53:     public Task<ClassOccurrenceDetails> CompleteOccurrenceAsync(Guid id, CompleteClassRequest r, CancellationToken ct = default)
54:         => coordination.TransactionAsync(async () =>
55:         {
56:             FinancialOperations.Operation(r.OperationId);
57:             await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"class-completion:" + r.OperationId}, 0))", ct);
58:             var info = await db.ClassSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Occurrence not found.");
59:             var actor = await access.RequireAsync(CoachingPermissions.ClassesManage, info.BranchId, ct); await coordination.LockBranchAsync(info.BranchId, ct);
60:             var row = await db.ClassSessions.SingleAsync(x => x.Id == id, ct); await db.Entry(row).ReloadAsync(ct);
61:             var hash = FinancialOperations.Hash(new { id, r });
// ...
63:             if (marker != null)
64:             {
65:                 using var evidence = System.Text.Json.JsonDocument.Parse(marker.NewValues!);
66:                 if (evidence.RootElement.GetProperty("Id").GetGuid() != id || evidence.RootElement.GetProperty("Hash").GetString() != hash || marker.UserId != actor) throw new BusinessRuleException("Operation belongs to another occurrence completion.");
67:                 return await OccurrenceDetailsAsync(row, ct);
68:             }
69:             if (row.OperationId == null || row.Status != ClassSessionStatus.Scheduled || row.Version != r.ExpectedVersion) throw new BusinessRuleException("Occurrence is legacy, finalized or has a stale version.");
70:             var (date, _) = await coordination.BusinessDateAsync(row.BranchId, ct);
71:             await CoachAsync(row.CoachEmployeeId, actor, row.BranchId, date, ct);
72:             if (row.BusinessDate != date || row.StartedAt > access.Now || r.Cancelled && await db.CheckIns.AnyAsync(x => x.ClassSessionId == id, ct))
73:                 throw new BusinessRuleException("Cannot complete before start or cancel attended occurrences; explicit correction is required.");
74:             row.Status = r.Cancelled ? ClassSessionStatus.Cancelled : ClassSessionStatus.Completed; row.EndedAt = access.Now; row.Version++;
75:             access.Audit(actor, row.BranchId, "Classes.OccurrenceEnded", nameof(ClassSession), r.OperationId, after: new { Id = id, Hash = hash });
// ...
78:     public Task<ClassAttendanceDetails> RegisterClassAsync(Guid occurrence, ClassAttendanceRequest r, CancellationToken ct = default)
79:         => coordination.TransactionAsync(async () =>
80:         {
81:             FinancialOperations.Operation(r.OperationId);
82:             var info = await db.ClassSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == occurrence, ct) ?? throw new BranchNotFoundException("Occurrence not found.");
83:             var actor = await access.RequireAsync(CoachingPermissions.RegisterClass, info.BranchId, ct); await coordination.LockBranchAsync(info.BranchId, ct);
84:             await ActiveBranchAsync(info.BranchId, ct);
85:             var member = await LockMemberAsync(r.MemberId, ct);
86:             var row = await db.ClassSessions.SingleAsync(x => x.Id == occurrence, ct); await db.Entry(row).ReloadAsync(ct);
87:             var hash = FinancialOperations.Hash(new { occurrence, r });
88:             var prior = await db.CheckIns.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
89:             if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return ClassAttendance(prior); }
90:             var (date, day) = await coordination.BusinessDateAsync(row.BranchId, ct);
91:             await CoachAsync(row.CoachEmployeeId, actor, row.BranchId, date, ct);
92:             if (!member.IsActive || row.OperationId == null || row.Status != ClassSessionStatus.Scheduled || row.BusinessDate != date || access.Now >= row.StartedAt)
93:                 throw new BusinessRuleException("Record an active member individually before the scheduled class starts on its BusinessDate.");
94:             if (await db.CheckIns.AnyAsync(x => x.ClassSessionId == occurrence && x.MemberId == r.MemberId, ct)) throw new BusinessRuleException("Member is already registered for this occurrence.");
95:             var eligible = db.ClassesSubscriptions.Where(s => s.BranchId == row.BranchId && s.MemberId == r.MemberId && s.StartDate <= date && s.EndDate >= date
96:                 && s.RemainingSessions > 0 && s.SaleTransaction.CancelledAt == null);
97:             if (r.ClassesSubscriptionId != null) eligible = eligible.Where(s => s.Id == r.ClassesSubscriptionId);
98:             var sub = await eligible.OrderBy(s => s.EndDate).ThenBy(s => s.CreatedAt).ThenBy(s => s.Id).FirstOrDefaultAsync(ct)
99:                 ?? throw new BusinessRuleException("No eligible Classes package session remains.");
100:             await db.Entry(sub).ReloadAsync(ct);
101:             if (await memberships.IsPaidSaleOverdueAsync(sub.SaleTransactionId, date, ct)) throw new BusinessRuleException("Classes installment is overdue.");
102:             var entry = new CheckIn { MemberId = member.Id, BranchId = row.BranchId, ClassesSubscriptionId = sub.Id, ClassSessionId = row.Id,
103:                 GroupClassId = row.GroupClassId, CoachEmployeeId = row.CoachEmployeeId, CheckInType = CheckInType.Class, CheckInMethod = CheckInMethod.Coach,
104:                 OperationId = r.OperationId, RequestHash = hash, BusinessDate = date, BusinessDayId = day, SessionConsumed = true,
105:                 SessionsBefore = sub.RemainingSessions, SessionsAfter = sub.RemainingSessions - 1, CheckedInAt = access.Now, CreatedByUserId = actor };
106:             sub.RemainingSessions--; sub.Version++; db.Add(entry);
// ...
108:             return ClassAttendance(entry);
109:         }, ct);
110:     private static ClassAttendanceDetails ClassAttendance(CheckIn c) => new(c.Id, c.MemberId!.Value, c.ClassesSubscriptionId!.Value,
111:         c.ClassSessionId!.Value, c.CoachEmployeeId!.Value, c.BusinessDate!.Value, c.SessionsAfter!.Value, c.CheckedInAt);
112:     private async Task<ClassOccurrenceDetails> OccurrenceDetailsAsync(ClassSession c, CancellationToken ct) => new(c.Id, c.GroupClassId, c.BranchId, c.CoachEmployeeId,
113:         c.BusinessDate, c.StartedAt, c.EndedAt, c.Status.ToString(), await db.CheckIns.CountAsync(x => x.ClassSessionId == c.Id, ct), c.Version);
114:     public async Task<CoachingPage<ClassOccurrenceDetails>> OccurrencesAsync(Guid branch, DateOnly from, DateOnly to, int page = 1, int pageSize = 25, CancellationToken ct = default)
115:     {
116:         Page(page, pageSize); if (from > to) throw new BusinessRuleException("Invalid date range."); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
117:         var q = db.ClassSessions.AsNoTracking().Where(x => x.BranchId == branch && x.BusinessDate >= from && x.BusinessDate <= to);
118:         var total = await q.CountAsync(ct); var rows = await q.OrderByDescending(x => x.StartedAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
119:         var result = new List<ClassOccurrenceDetails>(); foreach (var c in rows) result.Add(await OccurrenceDetailsAsync(c, ct)); return new(result, total, page, pageSize);
120:     }
121:     public async Task<CoachingPage<ClassAttendanceDetails>> ClassAttendanceHistoryAsync(Guid occurrence, int page = 1, int pageSize = 25, CancellationToken ct = default)
122:     {
123:         Page(page, pageSize); var branch = await db.ClassSessions.Where(x => x.Id == occurrence).Select(x => (Guid?)x.BranchId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Occurrence not found.");
124:         await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
125:         var q = db.CheckIns.AsNoTracking().Where(x => x.ClassSessionId == occurrence && x.ClassesSubscriptionId != null); var total = await q.CountAsync(ct);
126:         var rows = await q.OrderBy(x => x.CheckedInAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct); return new(rows.Select(ClassAttendance).ToArray(), total, page, pageSize);
127:     }
128:     public async Task<CoachingPage<ClassAttendanceDetails>> ClassMemberHistoryAsync(Guid branch, Guid member, int page = 1, int pageSize = 25, CancellationToken ct = default)
129:     {
130:         Page(page, pageSize); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
131:         var q = db.CheckIns.AsNoTracking().Where(x => x.BranchId == branch && x.MemberId == member && x.ClassesSubscriptionId != null);
132:         var total = await q.CountAsync(ct);
133:         var rows = await q.OrderByDescending(x => x.CheckedInAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
134:         return new(rows.Select(ClassAttendance).ToArray(), total, page, pageSize);
135:     }
136:     public async Task<CoachingPage<ClassesSubscriptionDetails>> ClassesSubscriptionsAsync(Guid branch, Guid member, int page = 1, int pageSize = 25, CancellationToken ct = default)
137:     {
138:         Page(page, pageSize); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
139:         var q = db.ClassesSubscriptions.AsNoTracking().Where(s => s.BranchId == branch && s.MemberId == member); var total = await q.CountAsync(ct);
140:         return new(await q.OrderByDescending(s => s.CreatedAt).ThenBy(s => s.Id).Skip((page - 1) * pageSize).Take(pageSize)
141:             .Select(s => new ClassesSubscriptionDetails(s.Id, s.MemberId, s.BranchId, s.SaleTransactionId, s.PackageName, s.StartDate, s.EndDate,
142:                 s.PurchasedSessions, s.RemainingSessions, s.Price, s.SaleTransaction.CancelledAt != null, s.Version)).ToListAsync(ct), total, page, pageSize);
143:     }
144:     public async Task<ClassCoachStatistics> ClassStatisticsAsync(Guid branch, Guid coach, DateOnly from, DateOnly to, Guid? periodId = null, CancellationToken ct = default)
145:     {
146:         await access.RequireAsync("Classes.ViewAttendanceReport", branch, ct);
147:         if (from > to || to.DayNumber - from.DayNumber > 366) throw new BusinessRuleException("Select a report range of at most 367 days.");
148:         var rows = await db.ClassSessions.AsNoTracking().Where(c => c.BranchId == branch && c.CoachEmployeeId == coach && c.Status == ClassSessionStatus.Completed
149:             && c.BusinessDate >= from && c.BusinessDate <= to).OrderBy(c => c.StartedAt).ThenBy(c => c.Id).ToListAsync(ct);
150:         var stats = new List<ClassOccurrenceDetails>(); foreach (var row in rows) stats.Add(await OccurrenceDetailsAsync(row, ct));
151:         decimal? target = null;
152:         if (periodId != null)
153:         {
154:             var period = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.Id == periodId && p.BranchId == branch && p.StartBusinessDate == from && p.EndBusinessDate == to, ct)
155:                 ?? throw new BusinessRuleException("Target report dates must exactly match the branch period.");
156:             target = await db.CoachTargetRules.Where(t => t.CommissionPeriodId == period.Id && t.CoachEmployeeId == coach).Select(t => t.ClassesAverageTarget).SingleOrDefaultAsync(ct);
157:         }
```
## CampionsSystem.BLL/Coaching/CoachingService.Earnings.cs
```csharp
// ...
12:     public Task<CoachTargetDetails> ConfigureTargetsAsync(CoachTargetRequest r, CancellationToken ct = default)
13:         => coordination.TransactionAsync(async () =>
14:         {
15:             FinancialOperations.Operation(r.OperationId);
16:             var period = await PeriodAsync(r.CommissionPeriodId, ct); var actor = await OwnerAsync(CoachingPermissions.Targets, period.BranchId, ct);
17:             await coordination.LockBranchAsync(period.BranchId, ct); var hash = FinancialOperations.Hash(r);
18:             var prior = await db.CoachTargetRules.AsNoTracking().SingleOrDefaultAsync(t => t.OperationId == r.OperationId, ct);
19:             if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return TargetDetails(prior); }
20:             if (r.PrivateTarget == null && r.ClassesAverageTarget == null || r.PrivateTarget <= 0 || r.ClassesAverageTarget <= 0
21:                 || r.ClassesAverageTarget != null && (decimal.Round(r.ClassesAverageTarget.Value, 4) != r.ClassesAverageTarget || r.ClassesAverageTarget > 99999999999999.9999m))
22:                 throw new BusinessRuleException("Supply an explicit positive Private target and/or Classes average target (up to four decimals).");
23:             if (r.PrivateTarget != null) FinancialOperations.Money(r.PrivateTarget.Value, true);
24:             if (await IsRecognizedAsync(period.Id, ct)) throw new BusinessRuleException("Coach period has finalized earnings.");
25:             if (!await db.Employees.AnyAsync(e => e.Id == r.CoachEmployeeId, ct)) throw new BranchNotFoundException("Coach not found.");
26:             if (!await db.EmployeeBranchAssignments.AnyAsync(a => a.EmployeeId == r.CoachEmployeeId && a.BranchId == period.BranchId
27:                 && a.StartDate <= period.EndBusinessDate && (a.EndDate == null || a.EndDate >= period.StartBusinessDate), ct)) throw new BusinessRuleException("Coach has no assignment overlapping this branch period.");
28:             if (await db.CoachTargetRules.AnyAsync(t => t.CommissionPeriodId == period.Id && t.CoachEmployeeId == r.CoachEmployeeId, ct)) throw new BusinessRuleException("Targets are already configured for this coach/period; immutable configuration preserves history.");
29:             var row = new CoachTargetRule { CommissionPeriodId = period.Id, CoachEmployeeId = r.CoachEmployeeId,
30:                 PrivateTarget = r.PrivateTarget, ClassesAverageTarget = r.ClassesAverageTarget, OperationId = r.OperationId, RequestHash = hash, CreatedByUserId = actor, CreatedAt = access.Now };
// ...
33:     private static CoachTargetDetails TargetDetails(CoachTargetRule t) => new(t.Id, t.CommissionPeriodId, t.CoachEmployeeId, t.PrivateTarget, t.ClassesAverageTarget);
34:     private async Task<CommissionPeriod> PeriodAsync(Guid id, CancellationToken ct)
35:     {
36:         var p = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new BranchNotFoundException("Period not found.");
37:         if (p.StartBusinessDate == null || p.EndBusinessDate == null) throw new BusinessRuleException("Legacy period needs explicit BusinessDate reconciliation."); return p;
38:     }
39:     public async Task<IReadOnlyList<CoachTargetDetails>> TargetsAsync(Guid periodId, CancellationToken ct = default)
40:     {
41:         var p = await PeriodAsync(periodId, ct); await access.RequireAsync(CoachingPermissions.EarningsView, p.BranchId, ct);
42:         return await db.CoachTargetRules.AsNoTracking().Where(t => t.CommissionPeriodId == p.Id).OrderBy(t => t.CoachEmployeeId)
43:             .Select(t => new CoachTargetDetails(t.Id, t.CommissionPeriodId, t.CoachEmployeeId, t.PrivateTarget, t.ClassesAverageTarget)).ToListAsync(ct);
44:     }
45:     private sealed record EarningSource(Guid Sale, Guid Coach, Guid? Session, Guid? Expiry, decimal Value, bool Transferred, int Order, decimal FundedBefore = 0, decimal FundedNow = 0, decimal PriorExactCommission = 0, decimal PriorRoundedCommission = 0);
46:     private async Task<List<EarningSource>> FundingAsync(CommissionPeriod period, CancellationToken ct)
47:     {
48:         var end = period.EndBusinessDate!.Value;
49:         var sessions = await db.PrivateSessions.AsNoTracking().Where(s => s.OperationId != null && s.BusinessDate <= end
50:             && s.PrivateSubscription.BranchId == period.BranchId)
51:             .Select(s => new EarningSource(s.PrivateSubscriptionTransaction.SaleTransactionId, s.CoachEmployeeId, s.Id, null,
52:                 s.SessionValue!.Value, s.TransferredPackage!.Value, s.SessionNumber!.Value, 0, 0)).ToListAsync(ct);
53:         var expiries = await db.PrivateExpirySettlements.AsNoTracking().Where(s => s.OperationId != null && s.BusinessDate <= end && s.PrivateSubscription.BranchId == period.BranchId)
54:             .Select(s => new EarningSource(s.PrivateSubscriptionTransaction.SaleTransactionId, s.CoachEmployeeId, null, s.Id, s.EligibleAmount,
55:                 s.TransferredPackage!.Value, int.MaxValue, 0, 0)).ToListAsync(ct);
56:         var sources = sessions.Concat(expiries).ToArray(); var sales = sources.Select(s => s.Sale).Distinct().ToArray();
57:         var payments = await db.Payments.AsNoTracking().Include(p => p.Parts).Where(p => sales.Contains(p.SaleTransactionId)
58:             && p.Status == PaymentStatus.Completed && p.PaidAt <= access.Now).ToListAsync(ct);
59:         if (payments.Any(p => p.Amount != p.Parts.Sum(part => part.Amount))) throw new BusinessRuleException("Collections and PaymentParts do not reconcile.");
60:         // Reuse actual newest-first refund allocations, including refunds received
61:         // after the evaluation end but before recognition. Refunded money is never funded.
62:         var refunded = await new Commissions.CommissionSources(db, access).RefundedAsync(payments, ct);
63:         var previous = await db.CoachEarnings.AsNoTracking().Where(e => sales.Contains(e.SaleTransactionId)).ToListAsync(ct);
64:         if (previous.Any(e => e.CommissionPeriodId != period.Id && e.BusinessDate > end)) throw new BusinessRuleException("Recognize coach periods in chronological order; future funded evidence exists.");
65:         var result = new List<EarningSource>();
66:         foreach (var group in sources.GroupBy(s => s.Sale).OrderBy(g => g.Key))
67:         {
68:             var funding = payments.Where(p => p.SaleTransactionId == group.Key && p.BusinessDate <= end).Sum(p => p.Amount - refunded.GetValueOrDefault(p.Id));
69:             if (funding < 0) throw new BusinessRuleException("Refunded value exceeds collected value.");
70:             foreach (var source in group.OrderBy(s => s.Order).ThenBy(s => s.Session).ThenBy(s => s.Expiry))
71:             {
72:                 var allowed = Math.Min(source.Value, funding); funding -= allowed;
73:                 var old = previous.Where(e => source.Session != null ? e.PrivateSessionId == source.Session : e.PrivateExpirySettlementId == source.Expiry).Sum(e => e.EligibleValue);
74:                 if (old > allowed) throw new BusinessRuleException("Historical funding no longer reconciles; Owner adjustment policy required.");
75:                 var priorSlices = previous.Where(e => source.Session != null ? e.PrivateSessionId == source.Session : e.PrivateExpirySettlementId == source.Expiry).ToArray();
76:                 result.Add(source with { FundedBefore = old, FundedNow = allowed - old, PriorExactCommission = priorSlices.Sum(e => e.EligibleValue * e.Rate / 100m), PriorRoundedCommission = priorSlices.Sum(e => e.Amount) });
// ...
78:         }
79:         return result;
80:     }
81:     private static decimal SliceCommission(EarningSource source, decimal rate) => decimal.Round(source.PriorExactCommission + source.FundedNow * rate / 100m, 2, MidpointRounding.AwayFromZero) - source.PriorRoundedCommission;
82:     private async Task<CoachEarningsProgress> ProgressCoreAsync(CommissionPeriod period, CancellationToken ct)
83:     {
// ...
88:         }
89:         var sources = await FundingAsync(period, ct);
90:         var targets = await db.CoachTargetRules.AsNoTracking().Where(t => t.CommissionPeriodId == period.Id).ToDictionaryAsync(t => t.CoachEmployeeId, ct);
91:         return new(period.Id, false, sources.GroupBy(s => s.Coach).OrderBy(g => g.Key).Select(g =>
92:         {
93:             var total = g.Sum(s => s.FundedNow); var target = targets.GetValueOrDefault(g.Key)?.PrivateTarget;
// ...
99:     }
100:     public async Task<CoachEarningsProgress> ProgressAsync(Guid periodId, CancellationToken ct = default)
101:     {
102:         var period = await PeriodAsync(periodId, ct); await access.RequireAsync(CoachingPermissions.EarningsView, period.BranchId, ct);
103:         if (db.Database.CurrentTransaction != null) return await ProgressCoreAsync(period, ct);
104:         await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
// ...
107:     public Task<CoachEarningsProgress> RecognizeAsync(RecognizeCoachEarningsRequest r, CancellationToken ct = default)
108:         => coordination.TransactionAsync(async () =>
109:         {
110:             FinancialOperations.Operation(r.OperationId);
111:             await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"coach-recognition:" + r.OperationId}, 0))", ct);
112:             var p = await PeriodAsync(r.CommissionPeriodId, ct);
113:             var actor = await OwnerAsync(CoachingPermissions.Recognize, p.BranchId, ct); await coordination.LockBranchAsync(p.BranchId, ct);
114:             var hash = FinancialOperations.Hash(r);
115:             var operation = await db.AuditLogs.AsNoTracking().SingleOrDefaultAsync(a => a.Action == "CoachEarnings.RecognitionOperation" && a.EntityId == r.OperationId, ct);
116:             if (operation != null)
117:             {
118:                 using var evidence = JsonDocument.Parse(operation.NewValues!);
119:                 if (operation.UserId != actor || evidence.RootElement.GetProperty("PeriodId").GetGuid() != p.Id || evidence.RootElement.GetProperty("Hash").GetString() != hash) throw new BusinessRuleException("Operation belongs to another recognition request.");
120:                 return await ProgressCoreAsync(p, ct);
121:             }
122:             if (await IsRecognizedAsync(p.Id, ct)) throw new BusinessRuleException("Coach earnings period is already finalized.");
123:             var (date, _) = await coordination.BusinessDateAsync(p.BranchId, ct);
124:             if (date <= p.EndBusinessDate) throw new BusinessRuleException("Recognize after the period's last BusinessDate ends, when its target result is final.");
125:             if (await db.CommissionPeriods.AnyAsync(x => x.BranchId == p.BranchId && x.EndBusinessDate < p.StartBusinessDate
126:                 && !db.AuditLogs.Any(a => a.EntityId == x.Id && a.Action == "CoachEarnings.PeriodRecognized"), ct)) throw new BusinessRuleException("Recognize preceding branch coach periods first (including zero earnings).");
127:             var sources = await FundingAsync(p, ct); var progress = await ProgressCoreAsync(p, ct);
128:             if (progress.Coaches.Any(c => c.ProjectedEarnings == null)) throw new BusinessRuleException("Configure an independent Private target for every original coach with funded value.");
129:             var rules = await db.CoachTargetRules.AsNoTracking().Where(t => t.CommissionPeriodId == p.Id).ToDictionaryAsync(t => t.CoachEmployeeId, ct);
130:             foreach (var source in sources.Where(s => s.FundedNow > 0))
// ...
135:                     CoachTargetRuleId = rule?.Id, SaleTransactionId = source.Sale, PrivateSessionId = source.Session, PrivateExpirySettlementId = source.Expiry,
136:                     EligibleValue = source.FundedNow, Rate = rate, Amount = SliceCommission(source, rate),
137:                     TransferredPackage = source.Transferred, TargetProgress = coach.EligibleValue, Target = coach.Target,
138:                     RecognitionOperationId = r.OperationId, RequestHash = hash, BusinessDate = p.EndBusinessDate!.Value, CreatedAt = access.Now, CreatedByUserId = actor };
139:                 db.Add(earning); access.Audit(actor, p.BranchId, "CoachEarnings.Recognized", nameof(CoachEarning), earning.Id, after: new { earning.CoachEmployeeId, earning.EligibleValue, earning.Rate, earning.Amount, source.Session, source.Expiry });
140:             }
// ...
150:     }
151:     public async Task<CoachBalance> BalanceAsync(Guid branch, Guid coach, CancellationToken ct = default)
152:     {
153:         await access.RequireAsync(CoachingPermissions.EarningsView, branch, ct);
154:         if (db.Database.CurrentTransaction != null) return await BalanceCoreAsync(branch, coach, ct);
155:         await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
// ...
157:     }
158:     public async Task<CoachingPage<CoachEarningDetails>> EarningsHistoryAsync(Guid branch, Guid coach, int page = 1, int pageSize = 25, CancellationToken ct = default)
159:     {
160:         Page(page, pageSize); await access.RequireAsync(CoachingPermissions.EarningsView, branch, ct);
161:         var q = db.CoachEarnings.AsNoTracking().Where(e => e.BranchId == branch && e.CoachEmployeeId == coach); var count = await q.CountAsync(ct);
162:         return new(await q.OrderByDescending(e => e.BusinessDate).ThenBy(e => e.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(e =>
163:             new CoachEarningDetails(e.Id, e.CoachEmployeeId, e.CommissionPeriodId, e.SaleTransactionId, e.PrivateSessionId, e.PrivateExpirySettlementId,
164:                 e.EligibleValue, e.Rate, e.Amount, e.TransferredPackage, e.BusinessDate)).ToListAsync(ct), count, page, pageSize);
165:     }
166: }
```
## CampionsSystem.BLL/Coaching/CoachingService.Packages.cs
```csharp
// ...
10:     public Task<CoachingPackageDetails> SavePackageAsync(Guid? id, CoachingPackageRequest r, bool classes, CancellationToken ct = default)
11:         => coordination.TransactionAsync(async () =>
12:         {
13:             var actor = await access.RequireAsync(classes ? CoachingPermissions.ClassesManage : CoachingPermissions.PrivateManage, r.BranchId, ct);
14:             FinancialOperations.Money(r.Price, true);
15:             if (string.IsNullOrWhiteSpace(r.Name) || r.Name.Trim().Length > 256 || r.SessionCount is < 1 or > 100000 || r.DurationDays is < 1 or > 36500)
16:                 throw new BusinessRuleException("Invalid package name, session count or duration.");
17:             if (!await db.GymBranches.AnyAsync(b => b.Id == r.BranchId && b.IsActive && b.City.IsActive, ct)) throw new BusinessRuleException("Active branch/city required.");
18:             Guid key;
19:             if (classes)
20:             {
21:                 var p = id == null ? new ClassesPackage { BranchId = r.BranchId } : await db.ClassesPackages.SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new BranchNotFoundException("Package not found.");
22:                 if (p.BranchId != r.BranchId) throw new UnauthorizedAccessException();
23:                 p.Name = r.Name.Trim(); p.Price = r.Price; p.SessionCount = r.SessionCount; p.DurationDays = r.DurationDays; p.IsActive = r.IsActive;
24:                 if (id == null) db.Add(p); key = p.Id;
25:             }
26:             else
27:             {
28:                 var p = id == null ? new PrivatePackage { BranchId = r.BranchId } : await db.PrivatePackages.SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new BranchNotFoundException("Package not found.");
29:                 if (p.BranchId != r.BranchId) throw new UnauthorizedAccessException();
30:                 p.Name = r.Name.Trim(); p.Price = r.Price; p.SessionCount = r.SessionCount; p.DurationDays = r.DurationDays; p.IsActive = r.IsActive;
31:                 if (id == null) db.Add(p); key = p.Id;
// ...
35:         }, ct);
36:     public async Task<CoachingPage<CoachingPackageDetails>> PackagesAsync(Guid branch, bool classes, int page = 1, int pageSize = 25, CancellationToken ct = default)
37:     {
38:         Page(page, pageSize); await access.RequireAsync(classes ? CoachingPermissions.ClassesView : CoachingPermissions.PrivateView, branch, ct);
39:         IQueryable<CoachingPackageDetails> q = classes
40:             ? db.ClassesPackages.AsNoTracking().Where(p => p.BranchId == branch).OrderBy(p => p.Name).ThenBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(p => new CoachingPackageDetails(p.Id, p.BranchId, p.Name, p.SessionCount, p.DurationDays, p.Price, p.IsActive))
41:             : db.PrivatePackages.AsNoTracking().Where(p => p.BranchId == branch).OrderBy(p => p.Name).ThenBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(p => new CoachingPackageDetails(p.Id, p.BranchId, p.Name, p.SessionCount, p.DurationDays, p.Price, p.IsActive));
42:         var total = classes ? await db.ClassesPackages.CountAsync(p => p.BranchId == branch, ct) : await db.PrivatePackages.CountAsync(p => p.BranchId == branch, ct);
43:         return new(await q.ToListAsync(ct), total, page, pageSize);
44:     }
45:     public async Task<CoachingPage<PrivateDetails>> PrivateSubscriptionsAsync(Guid branch, Guid member, int page = 1, int pageSize = 25, CancellationToken ct = default)
46:     {
47:         Page(page, pageSize); await access.RequireAsync(CoachingPermissions.PrivateView, branch, ct);
48:         var q = db.PrivateSubscriptions.AsNoTracking().Where(s => s.BranchId == branch && s.MemberId == member); var total = await q.CountAsync(ct);
49:         var ids = await q.OrderByDescending(s => s.CreatedAt).ThenBy(s => s.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(s => s.Id).ToListAsync(ct);
50:         var rows = new List<PrivateDetails>(); foreach (var id in ids) rows.Add(await PrivateDetailsAsync(id, ct)); return new(rows, total, page, pageSize);
51:     }
52:     public async Task<CoachingPage<PrivateExpiryDetails>> ExpiryHistoryAsync(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default)
53:     {
54:         Page(page, pageSize); var branch = await db.PrivateSubscriptions.Where(s => s.Id == id).Select(s => (Guid?)s.BranchId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Subscription not found.");
55:         await access.RequireAsync(CoachingPermissions.PrivateView, branch, ct);
56:         var q = db.PrivateExpirySettlements.AsNoTracking().Where(s => s.PrivateSubscriptionId == id); var total = await q.CountAsync(ct);
57:         var rows = await q.OrderByDescending(s => s.SettledAt).ThenBy(s => s.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct); return new(rows.Select(ExpiryDetails).ToArray(), total, page, pageSize);
58:     }
59: }
```
## CampionsSystem.BLL/Coaching/CoachingService.Payouts.cs
```csharp
// ...
10:     public Task<CoachPayoutDetails> PayAsync(CoachPayoutRequest r, CancellationToken ct = default)
11:         => coordination.TransactionAsync(async () =>
12:         {
13:             FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.Amount, true); FinancialOperations.Reason(r.Reason);
14:             if (r.ReceiptReference?.Length > 512) throw new BusinessRuleException("Receipt reference is too long.");
15:             var actor = await OwnerAsync(CoachingPermissions.Pay, r.BranchId, ct); await coordination.LockBranchAsync(r.BranchId, ct);
16:             var hash = FinancialOperations.Hash(r);
17:             var prior = await db.CoachPayouts.AsNoTracking().SingleOrDefaultAsync(p => p.OperationId == r.OperationId, ct);
18:             if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.AuthorizedByUserId, actor); return await PayoutDetailsAsync(prior, ct); }
19:             var balance = await BalanceCoreAsync(r.BranchId, r.CoachEmployeeId, ct);
20:             if (balance.Outstanding < r.Amount) throw new BusinessRuleException("Payout exceeds this coach's recognized outstanding branch earnings.");
21:             var shift = await coordination.RequireShiftAsync(r.BranchId, actor, ct);
22:             var account = await coordination.AccountAsync(r.BranchId, r.FinancialAccountId, ct);
23:             if (account.AccountType != FinancialAccountType.Cash && await coordination.BookAsync(account.Id, ct) < r.Amount) throw new BusinessRuleException("Insufficient financial account balance.");
24:             if (!await db.ExpenseCategories.AnyAsync(c => c.Id == r.ExpenseCategoryId && c.IsActive, ct)) throw new BusinessRuleException("Select an active expense category.");
25:             var (date, day) = await coordination.BusinessDateAsync(r.BranchId, ct);
26:             var expense = new Expense { BranchId = r.BranchId, ShiftId = shift.Id, RecordingEmployeeId = shift.EmployeeId,
27:                 ExpenseCategoryId = r.ExpenseCategoryId, FinancialAccountId = account.Id, OperationId = r.OperationId, RequestHash = hash,
28:                 Amount = r.Amount, Description = r.Reason.Trim(), ReceiptReference = r.ReceiptReference, ExpenseDate = access.Now, PaidAt = access.Now,
29:                 CreatedAt = access.Now, CreatedByUserId = actor, ApprovedByUserId = actor, BusinessDate = date, BusinessDayId = day, Status = ExpenseStatus.Recorded };
30:             var payout = new CoachPayout { BranchId = r.BranchId, CoachEmployeeId = r.CoachEmployeeId, Expense = expense,
31:                 OperationId = r.OperationId, RequestHash = hash, Amount = r.Amount, BalanceBefore = balance.Outstanding,
32:                 BalanceAfter = balance.Outstanding - r.Amount, AuthorizedByUserId = actor, PaidAt = access.Now, BusinessDate = date };
33:             db.Add(payout);
34:             var earnings = await db.CoachEarnings.AsNoTracking().Where(e => e.BranchId == r.BranchId && e.CoachEmployeeId == r.CoachEmployeeId)
35:                 .OrderBy(e => e.BusinessDate).ThenBy(e => e.CreatedAt).ThenBy(e => e.Id).ToListAsync(ct);
36:             var remaining = r.Amount;
37:             foreach (var earning in earnings)
// ...
42:                 db.Add(new CoachPayoutAllocation { CoachPayout = payout, CoachEarningId = earning.Id, Amount = take }); remaining -= take;
43:                 if (remaining == 0) break;
44:             }
45:             if (remaining != 0) throw new BusinessRuleException("Earning allocation history does not reconcile.");
46:             await coordination.PostAsync(new FinancialTransaction { BranchId = r.BranchId, FinancialAccountId = account.Id, ShiftId = shift.Id,
47:                 Expense = expense, SourceType = FinancialSourceType.Expense, TransactionType = FinancialTransactionType.Debit, Amount = r.Amount,
48:                 BusinessDate = date, BusinessDayId = day, OccurredAt = access.Now, CreatedByUserId = actor, Notes = r.Reason.Trim() }, ct);
49:             access.Audit(actor, r.BranchId, "CoachEarnings.Paid", nameof(CoachPayout), payout.Id, after: new { r.CoachEmployeeId, r.Amount, ExpenseId = expense.Id, payout.BalanceBefore, payout.BalanceAfter, AccountId = account.Id, account.AccountType, date });
50:             return await PayoutDetailsAsync(payout, ct);
// ...
53:     {
54:         var e = p.Expense ?? await db.Expenses.AsNoTracking().SingleAsync(e => e.Id == p.ExpenseId, ct);
55:         var method = await db.FinancialAccounts.Where(a => a.Id == e.FinancialAccountId).Select(a => a.AccountType).SingleAsync(ct);
56:         return new(p.Id, p.BranchId, p.CoachEmployeeId, p.ExpenseId, e.FinancialAccountId, method.ToString(), p.Amount, p.BalanceBefore, p.BalanceAfter, p.BusinessDate, p.PaidAt, p.AuthorizedByUserId);
57:     }
58:     public async Task<CoachingPage<CoachPayoutDetails>> PayoutHistoryAsync(Guid branch, Guid coach, int page = 1, int pageSize = 25, CancellationToken ct = default)
59:     {
60:         Page(page, pageSize); await access.RequireAsync(CoachingPermissions.EarningsView, branch, ct);
61:         var q = db.CoachPayouts.AsNoTracking().Where(p => p.BranchId == branch && p.CoachEmployeeId == coach); var total = await q.CountAsync(ct);
62:         var rows = await q.OrderByDescending(p => p.PaidAt).ThenBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
63:         var details = new List<CoachPayoutDetails>(); foreach (var p in rows) details.Add(await PayoutDetailsAsync(p, ct)); return new(details, total, page, pageSize);
64:     }
```
## CampionsSystem.BLL/Coaching/CoachingService.cs
```csharp
// ...
17:     internal static decimal UnitValue(decimal price, int sessions) => decimal.Floor(price / sessions * 100m) / 100m;
18:     internal static decimal SessionValue(decimal unit, decimal remainder, int number, int total) => unit + (number == total ? remainder : 0m);
19:     private static void Page(int page, int size)
20:     { if (page < 1 || size is < 1 or > 100 || (long)(page - 1) * size > int.MaxValue) throw new BusinessRuleException("Invalid pagination."); }
21:     private async Task<Guid> OwnerAsync(string permission, Guid branch, CancellationToken ct)
22:     {
23:         var actor = await access.RequireAsync(permission, branch, ct);
24:         await access.RequireAsync("Branches.AccessAll", branch, ct); return actor;
25:     }
26:     private async Task CoachAsync(Guid coach, Guid actor, Guid branch, DateOnly date, CancellationToken ct)
27:     {
28:         var e = await coordination.EmployeeAsync(actor, branch, date, ct);
29:         if (e.Id != coach) throw new UnauthorizedAccessException();
30:     }
31:     private async Task ActiveBranchAsync(Guid branch, CancellationToken ct)
32:     {
33:         if (!await db.GymBranches.AnyAsync(b => b.Id == branch && b.IsActive && b.City.IsActive, ct)) throw new BusinessRuleException("Active branch and city required.");
34:     }
35:     internal async Task ValidateCoachAsync(Guid coach, Guid branch, DateOnly date, CancellationToken ct)
36:     {
37:         if (!await db.Employees.AnyAsync(e => e.Id == coach && e.EmploymentStatus == EmploymentStatus.Active
38:             && e.HireDate <= date && (e.TerminationDate == null || e.TerminationDate >= date), ct)
39:             || !await db.EmployeeBranchAssignments.AnyAsync(a => a.EmployeeId == coach && a.BranchId == branch
40:                 && a.StartDate <= date && (a.EndDate == null || a.EndDate >= date), ct))
41:             throw new BusinessRuleException("Select an active coach assigned to this branch.");
42:     }
43:     private async Task<Member> LockMemberAsync(Guid id, CancellationToken ct)
44:     {
45:         var member = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)
46:             ?? throw new BranchNotFoundException("Member not found.");
47:         await db.Entry(member).ReloadAsync(ct); return member;
48:     }
49:     private async Task<(PrivateSubscription Sub, PrivateSubscriptionTransaction Batch, Guid Actor)> PrivateAsync(Guid id, string permission, CancellationToken ct)
50:     {
51:         var info = await db.PrivateSubscriptions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Private subscription not found.");
52:         var actor = await access.RequireAsync(permission, info.BranchId, ct);
53:         await ActiveBranchAsync(info.BranchId, ct);
54:         await coordination.LockBranchAsync(info.BranchId, ct); await LockMemberAsync(info.MemberId, ct);
55:         var sub = await db.PrivateSubscriptions.FromSqlInterpolated($"SELECT * FROM \"PrivateSubscriptions\" WHERE \"Id\"={id} FOR UPDATE").SingleAsync(ct);
56:         await db.Entry(sub).ReloadAsync(ct);
57:         var batches = await db.PrivateSubscriptionTransactions.Where(x => x.PrivateSubscriptionId == id).ToListAsync(ct);
58:         if (batches.Count != 1 || batches[0].OriginalAssignmentId == null) throw new BusinessRuleException("Legacy Private batches require explicit reconciliation.");
59:         await db.Entry(batches[0]).ReloadAsync(ct); return (sub, batches[0], actor);
60:     }
61:     public async Task<CoachingPage<CoachingMemberResult>> SearchAsync(Guid branch, bool classes, string search, int page = 1, int pageSize = 25, CancellationToken ct = default)
62:     {
63:         Page(page, pageSize); await access.RequireAsync(classes ? CoachingPermissions.ClassesView : CoachingPermissions.PrivateView, branch, ct);
64:         if (string.IsNullOrWhiteSpace(search) || search.Length > 200) throw new BusinessRuleException("Supply a search term up to 200 characters.");
65:         // Operational discovery follows purchased entitlement, not home branch. Phone
66:         // and codes share the existing MemberSearch normalization conventions.
// ...
68:             ? db.ClassesSubscriptions.Any(s => s.MemberId == m.Id && s.BranchId == branch)
69:             : db.PrivateSubscriptions.Any(s => s.MemberId == m.Id && s.BranchId == branch))), search, branch);
70:         var total = await q.CountAsync(ct);
71:         return new(await q.OrderBy(m => m.FullName).ThenBy(m => m.Id).Skip((page - 1) * pageSize).Take(pageSize)
72:             .Select(m => new CoachingMemberResult(m.Id, m.MemberCode, m.ClientCode, m.FullName, m.PrimaryPhone)).ToListAsync(ct), total, page, pageSize);
73:     }
74:     public Task<PrivateAttendanceDetails> RegisterPrivateAsync(Guid id, PrivateAttendanceRequest r, CancellationToken ct = default)
75:         => coordination.TransactionAsync(async () =>
76:         {
77:             FinancialOperations.Operation(r.OperationId);
78:             if (r.Notes?.Length > 2000) throw new BusinessRuleException("Notes must be at most 2000 characters.");
79:             var (sub, batch, actor) = await PrivateAsync(id, CoachingPermissions.RegisterPrivate, ct);
80:             var hash = FinancialOperations.Hash(new { id, r });
81:             var prior = await db.PrivateSessions.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
82:             if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return SessionDetails(prior); }
83:             var (date, _) = await coordination.BusinessDateAsync(sub.BranchId, ct);
84:             var assignment = await db.PrivateCoachAssignments.AsNoTracking().SingleOrDefaultAsync(x => x.PrivateSubscriptionId == id && x.EndedAt == null && x.AssignedAt <= access.Now, ct)
85:                 ?? throw new BusinessRuleException("Package has no current coach assignment.");
86:             await CoachAsync(assignment.CoachEmployeeId, actor, sub.BranchId, date, ct);
87:             if (!await db.Members.AnyAsync(x => x.Id == sub.MemberId && x.IsActive, ct) || sub.Status == PrivateSubscriptionStatus.Cancelled
88:                 || sub.StartDate > date || sub.EndDate < date || batch.RemainingSessions <= 0
89:                 || await db.SaleTransactions.AnyAsync(x => x.Id == batch.SaleTransactionId && x.CancelledAt != null, ct))
90:                 throw new BusinessRuleException("No eligible unexpired Private session remains.");
91:             if (await memberships.IsPaidSaleOverdueAsync(batch.SaleTransactionId, date, ct)) throw new BusinessRuleException("Private installment is overdue.");
92:             var checkIn = await db.CheckIns.AsNoTracking().SingleOrDefaultAsync(x => x.Id == r.GymCheckInId && x.MemberId == sub.MemberId
93:                 && x.BranchId == sub.BranchId && x.BusinessDate == date && x.CheckInType == CheckInType.Gym && x.CheckedInAt <= access.Now, ct)
94:                 ?? throw new BusinessRuleException("Reception Gym Check-In for this branch and BusinessDate is required.");
95:             var city = await db.GymBranches.Where(x => x.Id == sub.BranchId).Select(x => x.CityId).SingleAsync(ct);
96:             var facts = await memberships.GetGymAccessFactsAsync(sub.MemberId, date, ct);
97:             var membership = facts.SingleOrDefault(x => x.Id == checkIn.MemberSubscriptionId && !x.IsStandaloneVisit && x.EmployeeId == null
98:                 && x.StartDate <= date && x.EndDate >= date && x.AccessCityId == city && (x.Status == "Active"
99:                     || x.Status == "Expired" && x.Mode == "SessionBased" && x.RemainingSessions == 0));
100:             if (membership == null) throw new BusinessRuleException("A valid regular Gym membership is required; borrowed/standalone/expired membership is insufficient.");
101:             if (await db.PrivateSessions.AnyAsync(x => x.GymCheckInId == checkIn.Id, ct)) throw new BusinessRuleException("This reception Check-In already has a Private session.");
102:             var number = batch.SessionsAdded - batch.RemainingSessions + 1;
103:             var session = new PrivateSession { PrivateSubscriptionId = id, PrivateSubscriptionTransactionId = batch.Id,
104:                 CoachEmployeeId = assignment.CoachEmployeeId, PrivateCoachAssignmentId = assignment.Id,
105:                 GymCheckInId = checkIn.Id, OperationId = r.OperationId, RequestHash = hash, BusinessDate = date,
106:                 SessionNumber = number, SessionValue = SessionValue(batch.UnitSessionValue, batch.RemainderValue, number, batch.SessionsAdded),
107:                 TransferredPackage = assignment.Id != batch.OriginalAssignmentId, CreatedByUserId = actor, SessionAt = access.Now, Notes = r.Notes };
// ...
109:             access.Audit(actor, sub.BranchId, "Private.SessionRegistered", nameof(PrivateSession), session.Id, after: new { SubscriptionId = id, GymCheckInId = checkIn.Id, session.CoachEmployeeId, session.SessionValue, sub.RemainingSessions, date });
110:             return SessionDetails(session);
111:         }, ct);
112:     private static PrivateAttendanceDetails SessionDetails(PrivateSession s) => new(s.Id, s.PrivateSubscriptionId, s.CoachEmployeeId, s.GymCheckInId, s.BusinessDate, s.SessionAt, s.SessionNumber, s.SessionValue, s.TransferredPackage);
113:     public Task<PrivateAssignmentDetails> TransferAsync(Guid id, PrivateTransferRequest r, CancellationToken ct = default)
114:         => coordination.TransactionAsync(async () =>
115:         {
116:             FinancialOperations.Operation(r.OperationId); FinancialOperations.Reason(r.Reason);
117:             var (sub, _, actor) = await PrivateAsync(id, CoachingPermissions.Transfer, ct);
118:             var hash = FinancialOperations.Hash(new { id, r });
119:             var prior = await db.PrivateCoachAssignments.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
120:             if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return AssignmentDetails(prior); }
121:             if (sub.Version != r.ExpectedVersion) throw new DbUpdateConcurrencyException();
122:             var (date, _) = await coordination.BusinessDateAsync(sub.BranchId, ct);
123:             if (sub.Status == PrivateSubscriptionStatus.Cancelled || sub.EndDate < date || sub.RemainingSessions <= 0) throw new BusinessRuleException("No transferable Private balance remains.");
124:             await ValidateCoachAsync(r.CoachEmployeeId, sub.BranchId, date, ct);
125:             var old = await db.PrivateCoachAssignments.SingleAsync(x => x.PrivateSubscriptionId == id && x.EndedAt == null, ct);
126:             if (old.CoachEmployeeId == r.CoachEmployeeId) throw new BusinessRuleException("Coach is already assigned.");
127:             old.EndedAt = access.Now; await db.SaveChangesAsync(ct);
128:             var row = new PrivateCoachAssignment { PrivateSubscriptionId = id, CoachEmployeeId = r.CoachEmployeeId,
// ...
135:     public Task<PrivateExpiryDetails> SettleExpiryAsync(Guid id, PrivateExpiryRequest r, CancellationToken ct = default)
136:         => coordination.TransactionAsync(async () =>
137:         {
138:             FinancialOperations.Operation(r.OperationId); FinancialOperations.Reason(r.Reason);
139:             var (sub, batch, _) = await PrivateAsync(id, CoachingPermissions.Expiry, ct);
140:             var actor = await OwnerAsync(CoachingPermissions.Expiry, sub.BranchId, ct); var hash = FinancialOperations.Hash(new { id, r });
141:             var prior = await db.PrivateExpirySettlements.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
142:             if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId!.Value, actor); return ExpiryDetails(prior); }
143:             var (date, _) = await coordination.BusinessDateAsync(sub.BranchId, ct);
144:             var period = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.Id == r.CommissionPeriodId && p.BranchId == sub.BranchId
145:                 && p.StartBusinessDate <= date && p.EndBusinessDate >= date && p.Status == CommissionPeriodStatus.Open, ct)
146:                 ?? throw new BusinessRuleException("Select the open branch commission period containing the settlement BusinessDate.");
147:             if (await IsRecognizedAsync(period.Id, ct)) throw new BusinessRuleException("Coach earnings period is finalized.");
148:             if (sub.EndDate >= date || sub.Status == PrivateSubscriptionStatus.Cancelled || batch.RemainingSessions <= 0
149:                 || await db.SaleTransactions.AnyAsync(s => s.Id == batch.SaleTransactionId && s.CancelledAt != null, ct)
150:                 || await db.PrivateExpirySettlements.AnyAsync(s => s.PrivateSubscriptionTransactionId == batch.Id, ct))
151:                 throw new BusinessRuleException("An expired eligible unused batch may be settled exactly once.");
152:             var assignment = await db.PrivateCoachAssignments.AsNoTracking().SingleAsync(a => a.PrivateSubscriptionId == id && a.EndedAt == null, ct);
153:             // These are unused-session entitlement facts, not attendance or yet-paid earnings.
154:             var row = new PrivateExpirySettlement { PrivateSubscriptionId = id, PrivateSubscriptionTransactionId = batch.Id,
155:                 CoachEmployeeId = assignment.CoachEmployeeId, PrivateCoachAssignmentId = assignment.Id, TransferredPackage = assignment.Id != batch.OriginalAssignmentId,
156:                 CommissionPeriodId = period.Id, RemainingSessions = batch.RemainingSessions, UnitSessionValue = batch.UnitSessionValue, RemainderValue = batch.RemainderValue,
157:                 EligibleAmount = batch.RemainingSessions * batch.UnitSessionValue + batch.RemainderValue, BusinessDate = date,
158:                 OperationId = r.OperationId, RequestHash = hash, Reason = r.Reason.Trim(), CreatedByUserId = actor, CreatedAt = access.Now, SettledAt = access.Now };
159:             db.Add(row); sub.Status = PrivateSubscriptionStatus.Expired; sub.Version++;
160:             access.Audit(actor, sub.BranchId, "Private.ExpiryAuthorized", nameof(PrivateExpirySettlement), row.Id, after: ExpiryDetails(row));
161:             return ExpiryDetails(row);
162:         }, ct);
163:     private static PrivateExpiryDetails ExpiryDetails(PrivateExpirySettlement x) => new(x.Id, x.PrivateSubscriptionId, x.CoachEmployeeId, x.RemainingSessions, x.EligibleAmount, x.TransferredPackage, x.BusinessDate, x.SettledAt, x.Reason);
164:     public async Task<PrivateDetails> PrivateDetailsAsync(Guid id, CancellationToken ct = default)
165:     {
166:         var s = await db.PrivateSubscriptions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Private subscription not found.");
167:         await access.RequireAsync(CoachingPermissions.PrivateView, s.BranchId, ct);
168:         var batch = await db.PrivateSubscriptionTransactions.AsNoTracking().Where(x => x.PrivateSubscriptionId == id).ToListAsync(ct);
169:         if (batch.Count != 1) throw new BusinessRuleException("Legacy multiple batches require reconciliation."); var b = batch[0];
170:         var assignments = await db.PrivateCoachAssignments.AsNoTracking().Where(x => x.PrivateSubscriptionId == id).OrderBy(x => x.AssignedAt).ThenBy(x => x.Id).ToListAsync(ct);
171:         return new(s.Id, s.MemberId, s.BranchId, s.StartDate, s.EndDate, s.TotalSessions, s.TotalSessions - s.RemainingSessions,
172:             s.RemainingSessions, s.Version, s.Status == PrivateSubscriptionStatus.Cancelled, b.PackageNameSnapshot, b.NetAmount,
// ...
174:     }
175:     public async Task<CoachingPage<PrivateAttendanceDetails>> PrivateHistoryAsync(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default)
176:     {
177:         Page(page, pageSize); var branch = await db.PrivateSubscriptions.Where(x => x.Id == id).Select(x => (Guid?)x.BranchId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Private subscription not found.");
178:         await access.RequireAsync(CoachingPermissions.PrivateView, branch, ct); var q = db.PrivateSessions.AsNoTracking().Where(x => x.PrivateSubscriptionId == id);
179:         var total = await q.CountAsync(ct); var rows = await q.OrderByDescending(x => x.SessionAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
180:         return new(rows.Select(SessionDetails).ToArray(), total, page, pageSize);
181:     }
```
