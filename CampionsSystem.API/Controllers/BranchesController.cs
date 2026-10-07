using CampionsSystem.BLL.Branches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampionsSystem.API.Controllers;

[ApiController]
[Route("api/branches")]
public sealed class BranchesController(BranchManagementService branches) : BranchControllerBase
{
    [HttpGet, Authorize(Policy = "Permission:Branches.View")]
    public Task<IActionResult> List(CancellationToken ct) => Execute(() => branches.ListAsync(ct));

    [HttpGet("{id:guid}"), Authorize(Policy = "Permission:Branches.View")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) => Execute(() => branches.GetAsync(id, ct));

    [HttpPost, Authorize(Policy = "Permission:Branches.Manage")]
    public Task<IActionResult> Create(SaveBranchRequest request, CancellationToken ct) => Execute(() => branches.SaveAsync(null, request, ct));

    [HttpPut("{id:guid}"), Authorize(Policy = "Permission:Branches.Manage")]
    public Task<IActionResult> Update(Guid id, SaveBranchRequest request, CancellationToken ct) => Execute(() => branches.SaveAsync(id, request, ct));
}
