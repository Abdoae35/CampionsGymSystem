using CampionsSystem.BLL.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/offers")]
public sealed class OffersController(OfferService offers):BranchControllerBase
{
    [HttpGet,Authorize(Policy="Permission:Payments.Create")]
    public Task<IActionResult> Eligible([FromQuery]Guid memberId,[FromQuery]Guid membershipPlanId,[FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>offers.GetEligibleAsync(memberId,membershipPlanId,branchId,ct));
    [HttpGet("quote"),Authorize(Policy="Permission:Payments.Create")]
    public Task<IActionResult> Quote([FromQuery]Guid memberId,[FromQuery]Guid membershipPlanId,[FromQuery]Guid? offerId,[FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>offers.QuoteAsync(memberId,membershipPlanId,offerId,branchId,ct));
    [HttpGet("management"),Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Management([FromQuery]Guid? branchId,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>offers.GetManagementHistoryAsync(branchId,page,pageSize,ct));
    [HttpGet("{id:guid}"),Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Details(Guid id,CancellationToken ct)=>Execute(()=>offers.GetAsync(id,ct));
    [HttpPost,Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Create(OfferRequest r,CancellationToken ct)=>Execute(()=>offers.CreateAsync(r,ct));
    [HttpPut("{id:guid}"),Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Update(Guid id,OfferRequest r,CancellationToken ct)=>Execute(()=>offers.UpdateAsync(id,r,ct));
    [HttpPost("{id:guid}/member-entitlements"),Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Grant(Guid id,GrantMemberOfferRequest r,CancellationToken ct)=>Execute(()=>offers.GrantMemberAsync(id,r,ct));
    [HttpDelete("{id:guid}/member-entitlements/{grantId:guid}"),Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Revoke(Guid id,Guid grantId,CancellationToken ct)=>Execute(()=>offers.RevokeMemberAsync(id,grantId,ct));
    [HttpPut("/api/members/{memberId:guid}/audience-categories"),Authorize(Policy="Permission:Members.Manage")]
    public Task<IActionResult> Audiences(Guid memberId,SetMemberAudienceRequest r,CancellationToken ct)=>Execute(()=>offers.SetMemberAudiencesAsync(memberId,r,ct));
}
