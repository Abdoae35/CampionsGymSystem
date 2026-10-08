using CampionsSystem.BLL.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampionsSystem.API.Controllers;

[ApiController]
[Route("api/members")]
public sealed class MembersController(MemberManagementService members) : BranchControllerBase
{
    [HttpGet, Authorize(Policy = "Permission:Members.View")]
    public Task<IActionResult> List([FromQuery] MemberListRequest request, CancellationToken ct) => Execute(() => members.GetMembersAsync(request, ct));
    [HttpGet("{id:guid}"), Authorize(Policy = "Permission:Members.View")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) => Execute(() => members.GetMemberByIdAsync(id, ct));
    [HttpPost, Authorize(Policy = "Permission:Members.Create")]
    public Task<IActionResult> Create(CreateMemberRequest request, CancellationToken ct) => Execute(() => members.CreateMemberAsync(request, ct));
    [HttpPut("{id:guid}"), Authorize(Policy = "Permission:Members.Edit")]
    public Task<IActionResult> Update(Guid id, UpdateMemberRequest request, CancellationToken ct) => Execute(() => members.UpdateMemberAsync(id, request, ct));
    [HttpPatch("{id:guid}/status"), Authorize(Policy = "Permission:Members.ChangeStatus")]
    public Task<IActionResult> Status(Guid id, ChangeMemberStatusRequest request, CancellationToken ct) => Execute(() => members.ChangeMemberStatusAsync(id, request, ct));
    [HttpGet("{id:guid}/sales-assignment"), Authorize(Policy = "Permission:Members.View")]
    public Task<IActionResult> CurrentSales(Guid id, CancellationToken ct) => Execute(() => members.GetCurrentSalesAssignmentAsync(id, ct));
    [HttpGet("{id:guid}/sales-history"), Authorize(Policy = "Permission:Members.ViewSalesHistory")]
    public Task<IActionResult> SalesHistory(Guid id, CancellationToken ct) => Execute(() => members.GetSalesAssignmentHistoryAsync(id, ct));
    [HttpPost("{id:guid}/assign-sales"), Authorize(Policy = "Permission:Members.AssignSales")]
    public Task<IActionResult> AssignSales(Guid id, SalesAssignmentRequest request, CancellationToken ct) => Execute(() => members.AssignSalesAsync(id, request, ct));
    [HttpPost("{id:guid}/return-to-house"), Authorize(Policy = "Permission:Members.AssignSales")]
    public Task<IActionResult> ReturnToHouse(Guid id, ReturnToHouseRequest request, CancellationToken ct) => Execute(() => members.ReturnMemberToHouseAsync(id, request, ct));
}
