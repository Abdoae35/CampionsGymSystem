using CampionsSystem.BLL.Commissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController, Route("api/commissions")]
public sealed class CommissionsController(CommissionService commissions) : BranchControllerBase
{
    [HttpPost("rules"), Authorize(Policy = "Permission:Commission.ManageRules")]
    public Task<IActionResult> Configure(ConfigureCommissionRequest r, CancellationToken ct) => Execute(() => commissions.ConfigureAsync(r, ct));
    [HttpGet("rules"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> Rules([FromQuery] Guid? employeeId, [FromQuery] Guid? branchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => Execute(() => commissions.RulesAsync(employeeId, branchId, page, pageSize, ct));
    [HttpPost("periods"), Authorize(Policy = "Permission:Commission.ClosePeriod")]
    public Task<IActionResult> Open(OpenCommissionPeriodRequest r, CancellationToken ct) => Execute(() => commissions.OpenAsync(r, ct));
    [HttpGet("periods"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> History([FromQuery] Guid? branchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => Execute(() => commissions.PeriodsAsync(branchId, page, pageSize, ct));
    [HttpPost("periods/{id:guid}/finalize"), Authorize(Policy = "Permission:Commission.ClosePeriod")]
    public Task<IActionResult> Finalize(Guid id, FinalizeCommissionPeriodRequest r, CancellationToken ct) => Execute(() => commissions.FinalizeAsync(id, r, ct));
    [HttpGet("periods/{id:guid}"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> Progress(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => Execute(() => commissions.ProgressAsync(id, page, pageSize, ct));
    [HttpGet("periods/{id:guid}/employees/{employeeId:guid}"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> Employee(Guid id, Guid employeeId, CancellationToken ct) => Execute(() => commissions.EmployeeStatementAsync(id, employeeId, ct));
    [HttpGet("periods/{id:guid}/sources"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> Sources(Guid id, [FromQuery] Guid? employeeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => Execute(() => commissions.SourcesAsync(id, employeeId, page, pageSize, ct));
    [HttpGet("periods/{id:guid}/refund-allocations"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> RefundAllocations(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => Execute(() => commissions.RefundSourcesAsync(id, page, pageSize, ct));
}
