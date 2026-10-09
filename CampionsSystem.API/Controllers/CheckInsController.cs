using CampionsSystem.BLL.CheckIns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/check-ins")]
public sealed class CheckInsController(CheckInService checkIns):BranchControllerBase
{
    [HttpPost,Authorize(Policy="Permission:CheckIns.Create")]
    public Task<IActionResult> Create(CreateCheckInRequest request,CancellationToken ct)=>Execute(()=>checkIns.CreateAsync(request,ct));
    [HttpPost("evaluate"),Authorize(Policy="Permission:CheckIns.Create")]
    public Task<IActionResult> Evaluate(AccessRequest request,CancellationToken ct)=>Execute(()=>checkIns.EvaluateAsync(request,ct));
    [HttpGet,Authorize(Policy="Permission:CheckIns.View")]
    public Task<IActionResult> History([FromQuery]CheckInHistoryRequest request,CancellationToken ct)=>Execute(()=>checkIns.GetHistoryAsync(request,ct));
    [HttpGet("members"),Authorize(Policy="Permission:CheckIns.Create")]
    public Task<IActionResult> Lookup([FromQuery]CheckInMemberSearchRequest request,CancellationToken ct)=>Execute(()=>checkIns.FindMembersAsync(request,ct));
    [HttpGet("/api/members/{memberId:guid}/check-ins/current-business-day"),Authorize(Policy="Permission:CheckIns.View")]
    public Task<IActionResult> Today(Guid memberId,[FromQuery]Guid? branchId,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        =>Execute(()=>checkIns.GetCurrentBusinessDayAsync(memberId,branchId,page,pageSize,ct));
    [HttpGet("/api/members/{memberId:guid}/borrowed-gym-sessions"),Authorize(Policy="Permission:CheckIns.View")]
    public Task<IActionResult> Debts(Guid memberId,[FromQuery]BorrowedDebtHistoryRequest request,CancellationToken ct)=>Execute(()=>checkIns.GetBorrowedDebtHistoryAsync(memberId,request,ct));
    [HttpPost("trials"),Authorize(Policy="Permission:CheckIns.Create")]
    public Task<IActionResult> Trial(RedeemTrialRequest request,CancellationToken ct)=>Execute(()=>checkIns.RedeemTrialAsync(request,ct));
}
