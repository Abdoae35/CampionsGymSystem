using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Operations;

public sealed record CreateActivityCoachRequest(ActivityCoachType CoachType, Guid? EmployeeId,
    string? ExternalName, string? ExternalPhone, decimal Percentage);

public sealed class ActivityManagementService(ApplicationDbContext db, OperationAccess access)
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
            Percentage = request.Percentage, CreatedAt = access.Now };
        db.Add(coach);
        access.Audit(actor, null, "ActivityCoach.Created", nameof(ActivityCoach), coach.Id,
            after: new { coach.CoachType, coach.EmployeeId, coach.Percentage });
        await db.SaveChangesAsync(ct);
        return coach.Id;
    }

    public async Task SetPercentageAsync(Guid coachId, decimal percentage, CancellationToken ct = default)
    {
        var actor = await access.RequireAsync("Activities.ManageCoaches", null, ct);
        OperationAccess.Percentage(percentage);
        var coach = await db.ActivityCoaches.SingleOrDefaultAsync(x => x.Id == coachId, ct)
            ?? throw new BusinessRuleException("Activity coach not found.");
        var before = coach.Percentage;
        coach.Percentage = percentage;
        access.Audit(actor, null, "ActivityCoach.PercentageChanged", nameof(ActivityCoach), coach.Id,
            new { Percentage = before }, new { coach.Percentage });
        await db.SaveChangesAsync(ct);
    }

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
        db.Add(assignment);
        access.Audit(actor, activity.BranchId, "ActivityCoach.Assigned", nameof(ActivityCoachAssignment), assignment.Id,
            after: new { activityId, coachId });
        await db.SaveChangesAsync(ct);
        return assignment.Id;
    }

    public async Task UnassignAsync(Guid assignmentId, CancellationToken ct = default)
    {
        var row = await db.ActivityCoachAssignments.Include(x => x.SportsActivity)
            .SingleOrDefaultAsync(x => x.Id == assignmentId, ct) ?? throw new BusinessRuleException("Assignment not found.");
        var actor = await access.RequireAsync("Activities.ManageCoaches", row.SportsActivity.BranchId, ct);
        if (row.StartDate > access.Today) throw new BusinessRuleException("Future assignments require explicit date correction.");
        row.IsActive = false;
        row.EndDate = access.Today;
        access.Audit(actor, row.SportsActivity.BranchId, "ActivityCoach.Unassigned", nameof(ActivityCoachAssignment), row.Id);
        await db.SaveChangesAsync(ct);
    }
}
