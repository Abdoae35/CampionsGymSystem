using CampionsSystem.BLL.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampionsSystem.API.Controllers;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController(EmployeeManagementService employees) : BranchControllerBase
{
    [HttpGet, Authorize(Policy = "Permission:Employees.View")]
    public Task<IActionResult> List([FromQuery] EmployeeListRequest request, CancellationToken ct) => Execute(() => employees.ListAsync(request, ct));

    [HttpGet("{id:guid}"), Authorize(Policy = "Permission:Employees.View")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) => Execute(() => employees.GetAsync(id, ct));

    [HttpGet("/api/branches/{branchId:guid}/employees"), Authorize(Policy = "Permission:Employees.View")]
    public Task<IActionResult> BranchEmployees(Guid branchId, [FromQuery] EmployeeListRequest request, CancellationToken ct)
    {
        request.BranchId = branchId;
        return Execute(() => employees.ListAsync(request, ct));
    }
}
