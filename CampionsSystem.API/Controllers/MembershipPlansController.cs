using CampionsSystem.BLL.Plans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/membership-plans")]
public sealed class MembershipPlansController(MembershipPlanManagementService plans):BranchControllerBase
{
    [HttpGet,Authorize(Policy="Permission:MembershipPlans.View")]
    public Task<IActionResult> List([FromQuery]MembershipPlanListRequest r,CancellationToken ct)=>Execute(()=>plans.GetPlansAsync(r,ct));
    [HttpGet("{id:guid}"),Authorize(Policy="Permission:MembershipPlans.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>plans.GetPlanByIdAsync(id,ct));
    [HttpPost,Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Create(CreateMembershipPlanRequest r,CancellationToken ct)=>Execute(()=>plans.CreatePlanAsync(r,ct));
    [HttpPut("{id:guid}"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Update(Guid id,UpdateMembershipPlanRequest r,CancellationToken ct)=>Execute(()=>plans.UpdatePlanAsync(id,r,ct));
    [HttpPatch("{id:guid}/status"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Status(Guid id,ChangePlanStatusRequest r,CancellationToken ct)=>Execute(()=>plans.ChangePlanStatusAsync(id,r,ct));
    [HttpGet("{id:guid}/benefits"),Authorize(Policy="Permission:MembershipPlans.View")]
    public Task<IActionResult> Benefits(Guid id,CancellationToken ct)=>Execute(()=>plans.GetPlanBenefitsAsync(id,ct));
    [HttpPut("{id:guid}/benefits"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> SetBenefits(Guid id,SetPlanBenefitsRequest r,CancellationToken ct)=>Execute(()=>plans.SetPlanBenefitsAsync(id,r,ct));
    [HttpGet("{id:guid}/eligible-members"),Authorize(Policy="Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> Eligible(Guid id,[FromQuery]CatalogListRequest r,CancellationToken ct)=>Execute(()=>plans.GetEligibleMembersAsync(id,r,ct));
    [HttpGet("{id:guid}/eligible-members/candidates"),Authorize(Policy="Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> Candidates(Guid id,[FromQuery]EligibleMemberCandidateRequest r,CancellationToken ct)=>Execute(()=>plans.FindEligibleMemberCandidatesAsync(id,r,ct));
    [HttpPost("{id:guid}/eligible-members"),Authorize(Policy="Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> AddEligible(Guid id,EligibleMemberRequest r,CancellationToken ct)=>Execute(()=>plans.AddEligibleMemberAsync(id,r,ct));
    [HttpDelete("{id:guid}/eligible-members/{memberId:guid}"),Authorize(Policy="Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> RemoveEligible(Guid id,Guid memberId,CancellationToken ct)=>Execute(()=>plans.RemoveEligibleMemberAsync(id,memberId,ct));
}
