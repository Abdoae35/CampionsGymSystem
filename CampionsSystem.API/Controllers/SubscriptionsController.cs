using CampionsSystem.BLL.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;

[ApiController, Route("api/subscriptions")]
public sealed class SubscriptionsController(SubscriptionService subscriptions) : BranchControllerBase
{
    [HttpGet("/api/members/{memberId:guid}/subscriptions"), Authorize(Policy="Permission:Subscriptions.View")]
    public Task<IActionResult> History(Guid memberId, [FromQuery] SubscriptionHistoryRequest request, CancellationToken ct)
        => request.IncludePending
            ? Execute(()=>subscriptions.GetPurchaseHistoryAsync(memberId,request,ct))
            : Execute(()=>subscriptions.GetHistoryAsync(memberId,request,ct));
    [HttpGet("/api/members/{memberId:guid}/subscriptions/current"), Authorize(Policy="Permission:Subscriptions.View")]
    public Task<IActionResult> Current(Guid memberId, [FromQuery] Guid? branchId, CancellationToken ct)
        => Execute(()=>subscriptions.GetCurrentEntitlementAsync(memberId,branchId,ct));
    [HttpGet("{id:guid}"), Authorize(Policy="Permission:Subscriptions.View")]
    public Task<IActionResult> Details(Guid id, CancellationToken ct) => Execute(()=>subscriptions.GetDetailsAsync(id,ct));
    [HttpPost("{id:guid}/freeze"), Authorize(Policy="Permission:Subscriptions.Freeze")]
    public Task<IActionResult> Freeze(Guid id, FreezeRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.StartFreezeAsync(id,request,ct));
    [HttpPost("{id:guid}/freezes/{freezeId:guid}/complete"), Authorize(Policy="Permission:Subscriptions.Freeze")]
    public Task<IActionResult> Complete(Guid id, Guid freezeId, CompleteFreezeRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.CompleteFreezeAsync(id,freezeId,request,ct));
    [HttpPost("{id:guid}/custom-extension"), Authorize(Policy="Permission:Subscriptions.Extend")]
    public Task<IActionResult> Extend(Guid id, CustomExtensionRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.GrantCustomExtensionAsync(id,request,ct));
    [HttpPost("/api/employees/{id:guid}/membership-entitlement"), Authorize(Policy="Permission:Subscriptions.Manage"), Authorize(Policy="Permission:Employees.Manage")]
    public Task<IActionResult> Employee(Guid id, EmployeeEntitlementRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.StartEmployeeEntitlementAsync(id,request,ct));
    [HttpPost("official-holidays"), Authorize(Policy="Permission:Subscriptions.Extend"), Authorize(Policy="Permission:Branches.AccessAll")]
    public Task<IActionResult> Holiday(OfficialHolidayRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.CreateOfficialHolidayAsync(request,ct));
    [HttpPost("official-holidays/{id:guid}/apply"), Authorize(Policy="Permission:Subscriptions.Extend"), Authorize(Policy="Permission:Branches.AccessAll")]
    public Task<IActionResult> ApplyHoliday(Guid id, CancellationToken ct) => Execute(()=>subscriptions.ApplyOfficialHolidayAsync(id,ct));
}
