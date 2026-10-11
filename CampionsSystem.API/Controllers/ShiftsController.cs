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
    [HttpGet("current"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Current(CancellationToken ct)=>Execute(()=>shifts.CurrentAsync(ct));
    [HttpGet,Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> History([FromQuery]ShiftHistoryRequest r,CancellationToken ct)=>Execute(()=>shifts.HistoryAsync(r,ct));
    [HttpGet("{id:guid}"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>shifts.GetAsync(id,ct));
    [HttpGet("{id:guid}/summary"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Summary(Guid id,CancellationToken ct)=>Execute(()=>reports.ShiftAsync(id,ct));
}
[ApiController,Route("api/business-days")]
public sealed class BusinessDaysController(ShiftService shifts):BranchControllerBase
{
    [HttpGet("current"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Current([FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>shifts.CurrentBusinessDayAsync(branchId,ct));
    [HttpPost,Authorize(Policy="Permission:Shifts.Manage")]
    public Task<IActionResult> Open(OpenBusinessDayRequest r,CancellationToken ct)=>Execute(async()=>ShiftService.DayDetails(await shifts.OpenBusinessDayAsync(r,ct)));
    [HttpPost("{id:guid}/close"),Authorize(Policy="Permission:Shifts.Manage")]
    public Task<IActionResult> Close(Guid id,CancellationToken ct)=>Execute(async()=>ShiftService.DayDetails(await shifts.CloseBusinessDayAsync(id,ct)));
}
