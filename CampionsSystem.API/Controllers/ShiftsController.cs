using CampionsSystem.BLL.Shifts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/shifts")]
public sealed class ShiftsController(ShiftService shifts,ShiftReportService reports):BranchControllerBase
{
    [HttpPost,Authorize(Policy="Permission:Shifts.Manage")]
    public Task<IActionResult> Open(OpenShiftRequest r,CancellationToken ct)=>Execute(()=>shifts.OpenAsync(r,ct));
    [HttpPost("{id:guid}/close"),Authorize(Policy="Permission:Shifts.Manage")]
    public Task<IActionResult> Close(Guid id,CloseShiftRequest r,CancellationToken ct)=>Execute(()=>shifts.CloseAsync(id,r,ct));
    [HttpPost("{id:guid}/close-override"),Authorize(Policy="Permission:Shifts.CloseOthers")]
    public Task<IActionResult> CloseOverride(Guid id,OverrideCloseShiftRequest r,CancellationToken ct)=>Execute(()=>shifts.CloseOverrideAsync(id,r,ct));
    [HttpGet("current"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Current(CancellationToken ct)=>Execute(()=>shifts.CurrentAsync(ct));
    [HttpGet,Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> History([FromQuery]ShiftHistoryRequest r,CancellationToken ct)=>Execute(()=>shifts.HistoryAsync(r,ct));
    [HttpGet("{id:guid}"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>shifts.GetAsync(id,ct));
    [HttpGet("{id:guid}/summary"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Summary(Guid id,CancellationToken ct)=>Execute(()=>reports.ShiftAsync(id,ct));
    [HttpGet("{id:guid}/transactions"),Authorize(Policy="Permission:Shifts.View"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Transactions(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        =>Execute(()=>reports.TransactionsAsync(id,page,pageSize,ct));
    [HttpGet("{id:guid}/branch-transactions"),Authorize(Policy="Permission:Shifts.View"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> BranchTransactions(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        =>Execute(()=>reports.BranchTransactionsAsync(id,page,pageSize,ct));
}
[ApiController,Route("api/business-days")]
public sealed class BusinessDaysController(ShiftService shifts):BranchControllerBase
{
    [HttpGet("current"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Current([FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>shifts.CurrentBusinessDayAsync(branchId,ct));
    [HttpPost,Authorize(Policy="Permission:Shifts.Manage")]
    public Task<IActionResult> Open(OpenBusinessDayRequest r,CancellationToken ct)=>Execute(async()=>await shifts.DayDetailsAsync(await shifts.OpenBusinessDayAsync(r,ct),ct));
    [HttpPost("{id:guid}/close"),Authorize(Policy="Permission:Shifts.Manage")]
    public Task<IActionResult> Close(Guid id,CancellationToken ct)=>Execute(async()=>await shifts.DayDetailsAsync(await shifts.CloseBusinessDayAsync(id,ct),ct));
}
