using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/crm")]
public sealed class CrmController(LeadManagementService crm) : BranchControllerBase
{
    [HttpGet("my-follow-ups"),Authorize(Policy="Permission:CRM.ViewMemberFollowUps")]
    public Task<IActionResult> Mine([FromQuery] FollowUpListRequest r,CancellationToken ct)=>Execute(()=>crm.GetMyFollowUpsAsync(r,ct));
    [HttpGet("house-follow-ups"),Authorize(Policy="Permission:CRM.ManageHouseFollowUps")]
    public Task<IActionResult> House([FromQuery] FollowUpListRequest r,CancellationToken ct)=>Execute(()=>crm.GetHouseFollowUpsAsync(r,ct));
    [HttpPost("members/{id:guid}/follow-ups"),Authorize(Policy="Permission:CRM.ManageMemberFollowUps")]
    public Task<IActionResult> Open(Guid id,MemberFollowUpRequest r,CancellationToken ct)=>Execute(()=>crm.OpenMemberFollowUpAsync(id,r,ct));
    [HttpPost("member-follow-ups/{id:guid}/complete"),Authorize(Policy="Permission:CRM.ManageMemberFollowUps")]
    public Task<IActionResult> Complete(Guid id,CancellationToken ct)=>Execute(()=>crm.CompleteMemberFollowUpAsync(id,ct));
    [HttpPost("house-follow-ups/{id:guid}/assign"),Authorize(Policy="Permission:CRM.ManageHouseFollowUps")]
    public Task<IActionResult> Assign(Guid id,SalesAssignmentRequest r,CancellationToken ct)=>Execute(()=>crm.AssignHouseFollowUpAsync(id,r,ct));
}
