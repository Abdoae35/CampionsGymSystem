using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Coaching;

public sealed partial class CoachingService(ApplicationDbContext db, ICurrentUserService current,
    OperationAccess access, SubscriptionService memberships)
{
    private readonly FinancialOperations coordination = new(db, current, access);
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
        var q = MemberSearch.Apply(db.Members.AsNoTracking().Where(m => m.IsActive && (classes
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
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return await SessionDetailsAsync(prior,sub.MemberId,ct); }
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
            batch.RemainingSessions--; sub.RemainingSessions--; sub.Version++; db.Add(session);
            access.Audit(actor, sub.BranchId, "Private.SessionRegistered", nameof(PrivateSession), session.Id, after: new { SubscriptionId = id, GymCheckInId = checkIn.Id, session.CoachEmployeeId, session.SessionValue, sub.RemainingSessions, date });
            return await SessionDetailsAsync(session,sub.MemberId,ct);
        }, ct);
    private static PrivateAttendanceDetails SessionDetails(PrivateSession s) => new(s.Id, s.PrivateSubscriptionId, s.CoachEmployeeId, s.GymCheckInId, s.BusinessDate, s.SessionAt, s.SessionNumber, s.SessionValue, s.TransferredPackage);
    private async Task<PrivateAttendanceDetails> SessionDetailsAsync(PrivateSession session,Guid memberId,CancellationToken ct)
    {
        var member=await db.Members.AsNoTracking().Where(x=>x.Id==memberId)
            .Select(x=>new{x.FullName,x.MemberCode}).SingleOrDefaultAsync(ct);
        var coach=await db.Employees.AsNoTracking().Where(x=>x.Id==session.CoachEmployeeId)
            .Select(x=>x.FullName).SingleOrDefaultAsync(ct);
        return SessionDetails(session) with { MemberName=member?.FullName,MemberCode=member?.MemberCode,CoachEmployeeName=coach };
    }
    public Task<PrivateAssignmentDetails> TransferAsync(Guid id, PrivateTransferRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId); FinancialOperations.Reason(r.Reason);
            var (sub, _, actor) = await PrivateAsync(id, CoachingPermissions.Transfer, ct);
            var hash = FinancialOperations.Hash(new { id, r });
            var prior = await db.PrivateCoachAssignments.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return await AssignmentDetailsAsync(prior,ct); }
            if (sub.Version != r.ExpectedVersion) throw new DbUpdateConcurrencyException();
            var (date, _) = await coordination.BusinessDateAsync(sub.BranchId, ct);
            if (sub.Status == PrivateSubscriptionStatus.Cancelled || sub.EndDate < date || sub.RemainingSessions <= 0) throw new BusinessRuleException("No transferable Private balance remains.");
            await ValidateCoachAsync(r.CoachEmployeeId, sub.BranchId, date, ct);
            var old = await db.PrivateCoachAssignments.SingleAsync(x => x.PrivateSubscriptionId == id && x.EndedAt == null, ct);
            if (old.CoachEmployeeId == r.CoachEmployeeId) throw new BusinessRuleException("Coach is already assigned.");
            old.EndedAt = access.Now; await db.SaveChangesAsync(ct);
            var row = new PrivateCoachAssignment { PrivateSubscriptionId = id, CoachEmployeeId = r.CoachEmployeeId,
                AssignedAt = access.Now, CreatedAt = access.Now, CreatedByUserId = actor, OperationId = r.OperationId, RequestHash = hash, TransferReason = r.Reason.Trim() };
            db.Add(row); sub.Version++;
            access.Audit(actor, sub.BranchId, "Private.CoachTransferred", nameof(PrivateCoachAssignment), row.Id, new { old.CoachEmployeeId }, new { r.CoachEmployeeId, id, r.Reason });
            return await AssignmentDetailsAsync(row,ct);
        }, ct);
    private static PrivateAssignmentDetails AssignmentDetails(PrivateCoachAssignment a) => new(a.Id, a.CoachEmployeeId, a.AssignedAt, a.EndedAt, a.TransferReason);
    private async Task<PrivateAssignmentDetails> AssignmentDetailsAsync(PrivateCoachAssignment assignment,CancellationToken ct)
    {
        var name=await db.Employees.AsNoTracking().Where(x=>x.Id==assignment.CoachEmployeeId)
            .Select(x=>x.FullName).SingleOrDefaultAsync(ct);
        return AssignmentDetails(assignment) with { CoachEmployeeName=name };
    }
    public Task<PrivateExpiryDetails> SettleExpiryAsync(Guid id, PrivateExpiryRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId); FinancialOperations.Reason(r.Reason);
            var (sub, batch, _) = await PrivateAsync(id, CoachingPermissions.Expiry, ct);
            var actor = await OwnerAsync(CoachingPermissions.Expiry, sub.BranchId, ct); var hash = FinancialOperations.Hash(new { id, r });
            var prior = await db.PrivateExpirySettlements.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId!.Value, actor); return await ExpiryDetailsAsync(prior,ct); }
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
            return await ExpiryDetailsAsync(row,ct);
        }, ct);
    private static PrivateExpiryDetails ExpiryDetails(PrivateExpirySettlement x) => new(x.Id, x.PrivateSubscriptionId, x.CoachEmployeeId, x.RemainingSessions, x.EligibleAmount, x.TransferredPackage, x.BusinessDate, x.SettledAt, x.Reason);
    private async Task<PrivateExpiryDetails> ExpiryDetailsAsync(PrivateExpirySettlement settlement,CancellationToken ct)
    {
        var name=await db.Employees.AsNoTracking().Where(x=>x.Id==settlement.CoachEmployeeId)
            .Select(x=>x.FullName).SingleOrDefaultAsync(ct);
        return ExpiryDetails(settlement) with { CoachEmployeeName=name };
    }
    public async Task<PrivateDetails> PrivateDetailsAsync(Guid id, CancellationToken ct = default)
    {
        var s = await db.PrivateSubscriptions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Private subscription not found.");
        await access.RequireAsync(CoachingPermissions.PrivateView, s.BranchId, ct);
        var batch = await db.PrivateSubscriptionTransactions.AsNoTracking().Where(x => x.PrivateSubscriptionId == id).ToListAsync(ct);
        if (batch.Count != 1) throw new BusinessRuleException("Legacy multiple batches require reconciliation."); var b = batch[0];
        var assignments = await db.PrivateCoachAssignments.AsNoTracking().Where(x => x.PrivateSubscriptionId == id).OrderBy(x => x.AssignedAt).ThenBy(x => x.Id).ToListAsync(ct);
        var member=await db.Members.AsNoTracking().Where(x=>x.Id==s.MemberId)
            .Select(x=>new{x.FullName,x.MemberCode}).SingleOrDefaultAsync(ct);
        var coachIds=assignments.Select(x=>x.CoachEmployeeId)
            .Concat(b.OriginalCoachEmployeeId.HasValue?[b.OriginalCoachEmployeeId.Value]:[]).Distinct().ToArray();
        var coaches=await db.Employees.AsNoTracking().Where(x=>coachIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.FullName}).ToDictionaryAsync(x=>x.Id,x=>x.FullName,ct);
        return new(s.Id, s.MemberId, s.BranchId, s.StartDate, s.EndDate, s.TotalSessions, s.TotalSessions - s.RemainingSessions,
            s.RemainingSessions, s.Version, s.Status == PrivateSubscriptionStatus.Cancelled, b.PackageNameSnapshot, b.NetAmount,
            b.UnitSessionValue, b.RemainderValue, b.OriginalCoachEmployeeId,
            assignments.Select(x=>AssignmentDetails(x) with { CoachEmployeeName=coaches.GetValueOrDefault(x.CoachEmployeeId) }).ToArray(),
            member?.FullName,member?.MemberCode,b.OriginalCoachEmployeeId.HasValue?coaches.GetValueOrDefault(b.OriginalCoachEmployeeId.Value):null);
    }
    public async Task<CoachingPage<PrivateAttendanceDetails>> PrivateHistoryAsync(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); var branch = await db.PrivateSubscriptions.Where(x => x.Id == id).Select(x => (Guid?)x.BranchId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Private subscription not found.");
        await access.RequireAsync(CoachingPermissions.PrivateView, branch, ct); var q = db.PrivateSessions.AsNoTracking().Where(x => x.PrivateSubscriptionId == id);
        var total = await q.CountAsync(ct); var rows = await q.OrderByDescending(x => x.SessionAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var memberId=await db.PrivateSubscriptions.AsNoTracking().Where(x=>x.Id==id).Select(x=>x.MemberId).SingleAsync(ct);
        var member=await db.Members.AsNoTracking().Where(x=>x.Id==memberId).Select(x=>new{x.FullName,x.MemberCode}).SingleOrDefaultAsync(ct);
        var coachIds=rows.Select(x=>x.CoachEmployeeId).Distinct().ToArray();
        var coaches=await db.Employees.AsNoTracking().Where(x=>coachIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.FullName}).ToDictionaryAsync(x=>x.Id,x=>x.FullName,ct);
        return new(rows.Select(x=>SessionDetails(x) with { MemberName=member?.FullName,MemberCode=member?.MemberCode,
            CoachEmployeeName=coaches.GetValueOrDefault(x.CoachEmployeeId) }).ToArray(),total,page,pageSize);
    }
    private Task<bool> IsRecognizedAsync(Guid period, CancellationToken ct) => db.AuditLogs.AnyAsync(a => a.Action == "CoachEarnings.PeriodRecognized" && a.EntityId == period, ct);
}
