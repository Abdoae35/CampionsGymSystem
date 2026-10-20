using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Coaching;

public sealed partial class CoachingService
{
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
            access.Audit(actor, row.BranchId, "Classes.TypeConfigured", nameof(GroupClass), row.Id, after: new { row.Name, row.CoachEmployeeId, row.IsActive });
            var coachName=await db.Employees.AsNoTracking().Where(x=>x.Id==row.CoachEmployeeId).Select(x=>x.FullName).SingleOrDefaultAsync(ct);
            return new ClassTypeDetails(row.Id, row.BranchId, row.Name, row.CoachEmployeeId, row.IsActive, row.Description,coachName);
        }, ct);
    public async Task<CoachingPage<ClassTypeDetails>> ClassTypesAsync(Guid branch, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
        var q = db.GroupClasses.AsNoTracking().Where(x => x.BranchId == branch); var total = await q.CountAsync(ct);
        return new(await q.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new ClassTypeDetails(x.Id, x.BranchId, x.Name, x.CoachEmployeeId, x.IsActive, x.Description,x.CoachEmployee.FullName)).ToListAsync(ct), total, page, pageSize);
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
            if (r.StartsAt == default || r.StartsAt < access.Now || access.LocalDate(r.StartsAt) != access.Today)
                throw new BusinessRuleException("Occurrence must use the current operational BusinessDate; future scheduling is outside this workflow.");
            var row = new ClassSession { GroupClassId = group.Id, BranchId = group.BranchId, CoachEmployeeId = r.CoachEmployeeId,
                StartedAt = r.StartsAt.ToUniversalTime(), Status = ClassSessionStatus.Scheduled, BusinessDate = date, BusinessDayId = day,
                OperationId = r.OperationId, RequestHash = hash, CreatedByUserId = actor, CreatedAt = access.Now };
            db.Add(row); access.Audit(actor, group.BranchId, "Classes.OccurrenceCreated", nameof(ClassSession), row.Id, after: r);
            var coachName=await db.Employees.AsNoTracking().Where(x=>x.Id==row.CoachEmployeeId).Select(x=>x.FullName).SingleOrDefaultAsync(ct);
            return new ClassOccurrenceDetails(row.Id, group.Id, row.BranchId, row.CoachEmployeeId, date, row.StartedAt, null, "Scheduled", 0, 0,group.Name,coachName);
        }, ct);
    public Task<ClassOccurrenceDetails> CompleteOccurrenceAsync(Guid id, CompleteClassRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId);
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"class-completion:" + r.OperationId}, 0))", ct);
            var info = await db.ClassSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Occurrence not found.");
            var actor = await access.RequireAsync(CoachingPermissions.ClassesManage, info.BranchId, ct); await coordination.LockBranchAsync(info.BranchId, ct);
            var row = await db.ClassSessions.SingleAsync(x => x.Id == id, ct); await db.Entry(row).ReloadAsync(ct);
            var hash = FinancialOperations.Hash(new { id, r });
            var marker = await db.AuditLogs.AsNoTracking().SingleOrDefaultAsync(x => x.Action == "Classes.OccurrenceEnded" && x.EntityId == r.OperationId, ct);
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
            return await OccurrenceDetailsAsync(row, ct);
        }, ct);
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
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.CreatedByUserId, actor); return await ClassAttendanceAsync(prior,member,ct); }
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
            access.Audit(actor, row.BranchId, "Classes.AttendanceRegistered", nameof(CheckIn), entry.Id, after: new { MemberId = member.Id, occurrence, SubscriptionId = sub.Id, sub.RemainingSessions, date });
            return await ClassAttendanceAsync(entry,member,ct);
        }, ct);
    private static ClassAttendanceDetails ClassAttendance(CheckIn c) => new(c.Id, c.MemberId!.Value, c.ClassesSubscriptionId!.Value,
        c.ClassSessionId!.Value, c.CoachEmployeeId!.Value, c.BusinessDate!.Value, c.SessionsAfter!.Value, c.CheckedInAt);
    private async Task<ClassAttendanceDetails> ClassAttendanceAsync(CheckIn row,Member member,CancellationToken ct)
    {
        var coach=await db.Employees.AsNoTracking().Where(x=>x.Id==row.CoachEmployeeId).Select(x=>x.FullName).SingleOrDefaultAsync(ct);
        var type=await db.GroupClasses.AsNoTracking().Where(x=>x.Id==row.GroupClassId).Select(x=>x.Name).SingleOrDefaultAsync(ct);
        return ClassAttendance(row) with { MemberName=member.FullName,MemberCode=member.MemberCode,CoachEmployeeName=coach,ClassTypeName=type };
    }
    private async Task<ClassOccurrenceDetails> OccurrenceDetailsAsync(ClassSession c, CancellationToken ct) => (await OccurrenceDetailsManyAsync([c],ct))[0];
    private async Task<ClassOccurrenceDetails[]> OccurrenceDetailsManyAsync(IReadOnlyList<ClassSession> rows,CancellationToken ct)
    {
        if(rows.Count==0)return [];
        var ids=rows.Select(x=>x.Id).ToArray();
        var counts=await db.CheckIns.AsNoTracking().Where(x=>x.ClassSessionId.HasValue&&ids.Contains(x.ClassSessionId.Value))
            .GroupBy(x=>x.ClassSessionId!.Value).Select(x=>new{Id=x.Key,Count=x.Count()}).ToDictionaryAsync(x=>x.Id,x=>x.Count,ct);
        var typeIds=rows.Select(x=>x.GroupClassId).Distinct().ToArray();
        var types=await db.GroupClasses.AsNoTracking().Where(x=>typeIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.Name}).ToDictionaryAsync(x=>x.Id,x=>x.Name,ct);
        var coachIds=rows.Select(x=>x.CoachEmployeeId).Distinct().ToArray();
        var coaches=await db.Employees.AsNoTracking().Where(x=>coachIds.Contains(x.Id))
            .Select(x=>new{x.Id,x.FullName}).ToDictionaryAsync(x=>x.Id,x=>x.FullName,ct);
        return rows.Select(c=>new ClassOccurrenceDetails(c.Id,c.GroupClassId,c.BranchId,c.CoachEmployeeId,c.BusinessDate,
            c.StartedAt,c.EndedAt,c.Status.ToString(),counts.GetValueOrDefault(c.Id),c.Version,
            types.GetValueOrDefault(c.GroupClassId),coaches.GetValueOrDefault(c.CoachEmployeeId))).ToArray();
    }
    public async Task<CoachingPage<ClassOccurrenceDetails>> OccurrencesAsync(Guid branch, DateOnly from, DateOnly to, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); if (from > to) throw new BusinessRuleException("Invalid date range."); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
        var q = db.ClassSessions.AsNoTracking().Where(x => x.BranchId == branch && x.BusinessDate >= from && x.BusinessDate <= to);
        var total = await q.CountAsync(ct); var rows = await q.OrderByDescending(x => x.StartedAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new(await OccurrenceDetailsManyAsync(rows,ct), total, page, pageSize);
    }
    public async Task<CoachingPage<ClassAttendanceDetails>> ClassAttendanceHistoryAsync(Guid occurrence, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); var branch = await db.ClassSessions.Where(x => x.Id == occurrence).Select(x => (Guid?)x.BranchId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Occurrence not found.");
        await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
        var q = db.CheckIns.AsNoTracking().Where(x => x.ClassSessionId == occurrence && x.ClassesSubscriptionId != null); var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.CheckedInAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x=>new ClassAttendanceDetails(x.Id,x.MemberId!.Value,x.ClassesSubscriptionId!.Value,x.ClassSessionId!.Value,
                x.CoachEmployeeId!.Value,x.BusinessDate!.Value,x.SessionsAfter!.Value,x.CheckedInAt,
                x.Member!.FullName,x.Member.MemberCode,x.CoachEmployee!.FullName,x.GroupClass!.Name)).ToListAsync(ct);
        return new(rows,total,page,pageSize);
    }
    public async Task<CoachingPage<ClassAttendanceDetails>> ClassMemberHistoryAsync(Guid branch, Guid member, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
        var q = db.CheckIns.AsNoTracking().Where(x => x.BranchId == branch && x.MemberId == member && x.ClassesSubscriptionId != null);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.CheckedInAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x=>new ClassAttendanceDetails(x.Id,x.MemberId!.Value,x.ClassesSubscriptionId!.Value,x.ClassSessionId!.Value,
                x.CoachEmployeeId!.Value,x.BusinessDate!.Value,x.SessionsAfter!.Value,x.CheckedInAt,
                x.Member!.FullName,x.Member.MemberCode,x.CoachEmployee!.FullName,x.GroupClass!.Name)).ToListAsync(ct);
        return new(rows,total,page,pageSize);
    }
    public async Task<CoachingPage<ClassesSubscriptionDetails>> ClassesSubscriptionsAsync(Guid branch, Guid member, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
        var q = db.ClassesSubscriptions.AsNoTracking().Where(s => s.BranchId == branch && s.MemberId == member); var total = await q.CountAsync(ct);
        return new(await q.OrderByDescending(s => s.CreatedAt).ThenBy(s => s.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new ClassesSubscriptionDetails(s.Id, s.MemberId, s.BranchId, s.SaleTransactionId, s.PackageName, s.StartDate, s.EndDate,
                s.PurchasedSessions, s.RemainingSessions, s.Price, s.SaleTransaction.CancelledAt != null, s.Version,
                s.Member.FullName,s.Member.MemberCode)).ToListAsync(ct), total, page, pageSize);
    }
    public async Task<ClassCoachStatistics> ClassStatisticsAsync(Guid branch, Guid coach, DateOnly from, DateOnly to, Guid? periodId = null, CancellationToken ct = default)
    {
        await access.RequireAsync("Classes.ViewAttendanceReport", branch, ct);
        if (from > to || to.DayNumber - from.DayNumber > 366) throw new BusinessRuleException("Select a report range of at most 367 days.");
        var rows = await db.ClassSessions.AsNoTracking().Where(c => c.BranchId == branch && c.CoachEmployeeId == coach && c.Status == ClassSessionStatus.Completed
            && c.BusinessDate >= from && c.BusinessDate <= to).OrderBy(c => c.StartedAt).ThenBy(c => c.Id).ToListAsync(ct);
        var stats = await OccurrenceDetailsManyAsync(rows,ct);
        decimal? target = null;
        if (periodId != null)
        {
            var period = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.Id == periodId && p.BranchId == branch && p.StartBusinessDate == from && p.EndBusinessDate == to, ct)
                ?? throw new BusinessRuleException("Target report dates must exactly match the branch period.");
            target = await db.CoachTargetRules.Where(t => t.CommissionPeriodId == period.Id && t.CoachEmployeeId == coach).Select(t => t.ClassesAverageTarget).SingleOrDefaultAsync(ct);
        }
        var total = stats.Sum(s => s.Attendance); var average = rows.Count == 0 ? 0m : (decimal)total / rows.Count;
        var coachName=await db.Employees.AsNoTracking().Where(x=>x.Id==coach).Select(x=>x.FullName).SingleOrDefaultAsync(ct);
        return new(coach, rows.Count, total, average, target, target == null ? null : average >= target, stats,"NotConfigured",coachName);
    }
}
