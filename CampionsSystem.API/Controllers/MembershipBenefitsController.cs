using CampionsSystem.BLL.Plans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/membership-benefits")]
public sealed class MembershipBenefitsController(MembershipPlanManagementService plans):BranchControllerBase
{
    [HttpGet,Authorize(Policy="Permission:MembershipPlans.View")]
    public Task<IActionResult> List([FromQuery]CatalogListRequest r,CancellationToken ct)=>Execute(()=>plans.GetBenefitsAsync(r,ct));
    [HttpGet("{id:guid}"),Authorize(Policy="Permission:MembershipPlans.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>plans.GetBenefitByIdAsync(id,ct));
    [HttpPost,Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Create(SaveBenefitRequest r,CancellationToken ct)=>Execute(()=>plans.CreateBenefitAsync(r,ct));
    [HttpPut("{id:guid}"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Update(Guid id,SaveBenefitRequest r,CancellationToken ct)=>Execute(()=>plans.UpdateBenefitAsync(id,r,ct));
    [HttpPatch("{id:guid}/status"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Status(Guid id,ChangePlanStatusRequest r,CancellationToken ct)=>Execute(()=>plans.ChangeBenefitStatusAsync(id,r,ct));
}
