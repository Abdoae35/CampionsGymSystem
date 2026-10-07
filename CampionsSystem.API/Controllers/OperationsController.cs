using CampionsSystem.BLL.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampionsSystem.API.Controllers;

[ApiController]
[Route("api")]
public sealed class OperationsController(ActivityManagementService activities, FreelancePlanService freelance,
    SubscriptionPurchaseService purchases, ClassSessionService classes, CoachReportService reports) : ControllerBase
{
    public sealed record PercentageRequest(decimal Percentage);
    public sealed record AssignmentRequest(Guid ActivityCoachId);

    [HttpPost("activities/coaches")]
    [Authorize(Policy = "Permission:Activities.ManageCoaches")]
    public Task<IActionResult> CreateCoach(CreateActivityCoachRequest request, CancellationToken ct) =>
        Execute(async () => new { Id = await activities.CreateCoachAsync(request, ct) });

    [HttpPut("activities/coaches/{id:guid}/percentage")]
    [Authorize(Policy = "Permission:Activities.ManageCoaches")]
    public Task<IActionResult> Percentage(Guid id, PercentageRequest request, CancellationToken ct) =>
        Execute(async () => { await activities.SetPercentageAsync(id, request.Percentage, ct); return new { Updated = true }; });

    [HttpPost("activities/{id:guid}/coach-assignments")]
    [Authorize(Policy = "Permission:Activities.ManageCoaches")]
    public Task<IActionResult> Assign(Guid id, AssignmentRequest request, CancellationToken ct) =>
        Execute(async () => new { Id = await activities.AssignAsync(id, request.ActivityCoachId, ct) });

    [HttpDelete("activities/coach-assignments/{id:guid}")]
    [Authorize(Policy = "Permission:Activities.ManageCoaches")]
    public Task<IActionResult> Unassign(Guid id, CancellationToken ct) =>
        Execute(async () => { await activities.UnassignAsync(id, ct); return new { Updated = true }; });

    [HttpPost("activities/purchases")]
    [Authorize(Policy = "Permission:Activities.Manage")]
    public Task<IActionResult> PurchaseActivity(ActivityPurchaseRequest request, CancellationToken ct) =>
        Execute(() => purchases.PurchaseActivityAsync(request, ct));

    [HttpGet("activities/coach-revenue/{periodId:guid}")]
    [Authorize(Policy = "Permission:Activities.ViewCoachRevenue")]
    public Task<IActionResult> ActivityRevenue(Guid periodId, CancellationToken ct) => Execute(() => reports.ActivitiesAsync(periodId, ct));

    [HttpPost("activities/coach-revenue/{periodId:guid}/finalize")]
    [Authorize(Policy = "Permission:Commission.ClosePeriod")]
    public Task<IActionResult> FinalizeActivity(Guid periodId, CancellationToken ct) =>
        Execute(async () => { await reports.FinalizeActivitiesAsync(periodId, ct); return new { Finalized = true }; });

    [HttpPost("classes/sessions")]
    [Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> CreateSession(SaveClassSessionRequest request, CancellationToken ct) =>
        Execute(async () => new { Id = await classes.SaveAsync(null, request, ct) });

    [HttpPut("classes/sessions/{id:guid}")]
    [Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> UpdateSession(Guid id, SaveClassSessionRequest request, CancellationToken ct) =>
        Execute(async () => new { Id = await classes.SaveAsync(id, request, ct) });

    [HttpPost("classes/sessions/{id:guid}/attendance/{memberId:guid}")]
    [Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> Attendance(Guid id, Guid memberId, CancellationToken ct) =>
        Execute(async () => new { Id = await classes.AttendAsync(id, memberId, ct) });

    [HttpGet("classes/attendance-report")]
    [Authorize(Policy = "Permission:Classes.ViewAttendanceReport")]
    public Task<IActionResult> ClassReport(Guid branchId, DateTimeOffset from, DateTimeOffset until, CancellationToken ct) =>
        Execute(() => reports.ClassesAsync(branchId, from, until, ct));

    [HttpPost("freelance-plans")]
    [Authorize(Policy = "Permission:FreelancePlans.Manage")]
    public Task<IActionResult> CreatePlan(SaveFreelancePlanRequest request, CancellationToken ct) =>
        Execute(async () => new { Id = await freelance.SaveAsync(null, request, ct) });

    [HttpPut("freelance-plans/{id:guid}")]
    [Authorize(Policy = "Permission:FreelancePlans.Manage")]
    public Task<IActionResult> UpdatePlan(Guid id, SaveFreelancePlanRequest request, CancellationToken ct) =>
        Execute(async () => new { Id = await freelance.SaveAsync(id, request, ct) });

    [HttpPut("freelance-plans/{id:guid}/eligible-members/{memberId:guid}")]
    [Authorize(Policy = "Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> AddEligible(Guid id, Guid memberId, CancellationToken ct) =>
        Execute(async () => { await freelance.SetEligibilityAsync(id, memberId, true, ct); return new { Eligible = true }; });

    [HttpDelete("freelance-plans/{id:guid}/eligible-members/{memberId:guid}")]
    [Authorize(Policy = "Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> RemoveEligible(Guid id, Guid memberId, CancellationToken ct) =>
        Execute(async () => { await freelance.SetEligibilityAsync(id, memberId, false, ct); return new { Eligible = false }; });

    [HttpPost("freelance-plans/purchases")]
    [Authorize(Policy = "Permission:FreelancePlans.Manage")]
    public Task<IActionResult> PurchaseFreelance(FreelancePurchaseRequest request, CancellationToken ct) =>
        Execute(() => purchases.PurchaseFreelanceAsync(request, ct));

    [HttpGet("freelance-plans/{id:guid}/report")]
    [Authorize(Policy = "Permission:FreelancePlans.View")]
    public Task<IActionResult> FreelanceReport(Guid id, Guid branchId, DateTimeOffset from, DateTimeOffset until, CancellationToken ct) =>
        Execute(() => reports.FreelanceAsync(id, branchId, from, until, ct));

    private async Task<IActionResult> Execute<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (BusinessRuleException error) { return Problem(statusCode: 400, title: error.Message); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException error) when (error.InnerException is Npgsql.PostgresException
            { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation or Npgsql.PostgresErrorCodes.SerializationFailure })
        { return Problem(statusCode: 409, title: "Concurrent or duplicate change; reload and retry."); }
        catch (Npgsql.PostgresException error) when (error.SqlState == Npgsql.PostgresErrorCodes.SerializationFailure)
        { return Problem(statusCode: 409, title: "Concurrent change; reload and retry."); }
    }
}
