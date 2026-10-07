using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Operations;

public sealed record SaveClassSessionRequest(Guid GroupClassId, Guid? ClassScheduleId, Guid CoachEmployeeId,
    DateTimeOffset StartedAt, DateTimeOffset? EndedAt, ClassSessionStatus Status);

public sealed class ClassSessionService(ApplicationDbContext db, OperationAccess access)
{
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
        if (session.GroupClassId != group.Id || (id.HasValue && session.Status != ClassSessionStatus.Scheduled))
            throw new BusinessRuleException("Executed/cancelled sessions cannot be rewritten; use an explicit correction workflow.");
        if (id.HasValue && await db.CheckIns.AnyAsync(x => x.ClassSessionId == id, ct)
            && (session.CoachEmployeeId != request.CoachEmployeeId || session.StartedAt != request.StartedAt))
            throw new BusinessRuleException("Cannot change coach/start time after recording attendance.");
        session.ClassScheduleId = request.ClassScheduleId; session.CoachEmployeeId = request.CoachEmployeeId;
        session.StartedAt = request.StartedAt.ToUniversalTime(); session.EndedAt = request.EndedAt?.ToUniversalTime(); session.Status = request.Status;
        if (!id.HasValue) db.Add(session);
        access.Audit(actor, group.BranchId, "ClassSession.Recorded", nameof(ClassSession), session.Id,
            after: new { session.Status, session.CoachEmployeeId, session.StartedAt, session.EndedAt });
        await db.SaveChangesAsync(ct);
        return session.Id;
    }

    public async Task<Guid> AttendAsync(Guid sessionId, Guid memberId, CancellationToken ct = default)
    {
        var session = await db.ClassSessions.SingleOrDefaultAsync(x => x.Id == sessionId, ct)
            ?? throw new BusinessRuleException("Session not found.");
        var actor = await access.RequireAsync("Classes.Manage", session.BranchId, ct);
        if (session.Status == ClassSessionStatus.Cancelled) throw new BusinessRuleException("Cancelled class cannot record attendance.");
        if (!await db.Members.AnyAsync(x => x.Id == memberId && x.IsActive, ct)) throw new BusinessRuleException("Active member required.");
        if (await db.CheckIns.AnyAsync(x => x.ClassSessionId == sessionId && x.MemberId == memberId, ct))
            throw new BusinessRuleException("Member is already attending this session.");
        var row = new CheckIn { ClassSessionId = session.Id, GroupClassId = session.GroupClassId, BranchId = session.BranchId,
            CoachEmployeeId = session.CoachEmployeeId, MemberId = memberId, CheckInType = CheckInType.Class,
            CheckedInAt = access.Now, CreatedByUserId = actor, CheckInMethod = CheckInMethod.Manual };
        db.Add(row);
        access.Audit(actor, session.BranchId, "ClassSession.AttendanceRecorded", nameof(CheckIn), row.Id, after: new { sessionId, memberId });
        await db.SaveChangesAsync(ct);
        return row.Id;
    }
}
