using CampionsSystem.BLL.Plans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/membership-plan-categories")]
public sealed class MembershipPlanCategoriesController(MembershipPlanManagementService plans):BranchControllerBase
{
    [HttpGet,Authorize(Policy="Permission:MembershipPlans.View")]
    public Task<IActionResult> List([FromQuery]CatalogListRequest r,CancellationToken ct)=>Execute(()=>plans.GetCategoriesAsync(r,ct));
    [HttpPost,Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Create(SaveCategoryRequest r,CancellationToken ct)=>Execute(()=>plans.CreateCategoryAsync(r,ct));
    [HttpPut("{id:guid}"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Update(Guid id,SaveCategoryRequest r,CancellationToken ct)=>Execute(()=>plans.UpdateCategoryAsync(id,r,ct));
    [HttpPatch("{id:guid}/status"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Status(Guid id,ChangePlanStatusRequest r,CancellationToken ct)=>Execute(()=>plans.ChangeCategoryStatusAsync(id,r,ct));
}
