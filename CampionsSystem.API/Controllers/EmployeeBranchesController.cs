using CampionsSystem.BLL.Branches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampionsSystem.API.Controllers;

[ApiController]
[Route("api/employees/{employeeId:guid}")]
public sealed class EmployeeBranchesController(EmployeeBranchAssignmentService assignments) : BranchControllerBase
{
    [HttpGet("branch-assignment"), Authorize(Policy = "Permission:Employees.View")]
    public Task<IActionResult> Current(Guid employeeId, CancellationToken ct) => Execute(() => assignments.GetCurrentAsync(employeeId, ct));

    [HttpGet("branch-history"), Authorize(Policy = "Permission:Employees.View")]
    public Task<IActionResult> History(Guid employeeId, CancellationToken ct) => Execute(() => assignments.HistoryAsync(employeeId, ct));

    [HttpPost("assign-branch"), Authorize(Policy = "Permission:Employees.Manage")]
    public Task<IActionResult> Assign(Guid employeeId, AssignBranchRequest request, CancellationToken ct) => Execute(() => assignments.AssignAsync(employeeId, request, ct));
}
