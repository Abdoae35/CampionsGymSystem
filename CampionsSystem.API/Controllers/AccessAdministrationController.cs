using CampionsSystem.BLL.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampionsSystem.API.Controllers;

[ApiController, Route("api/access-administration"), Authorize(Policy = OwnerAdministrationAccess.Policy)]
[ProducesResponseType(401), ProducesResponseType(403)]
[ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 404), ProducesResponseType(typeof(ProblemDetails), 409)]
public sealed class AccessAdministrationController(AccessAdministrationService service) : BranchControllerBase
{
    private async Task<IActionResult> Admin<T>(Func<Task<T>> action)
    {
        try { return await Execute(action); }
        catch (AccessAdministrationException e) { return Problem(statusCode: e.Status, title: e.Message); }
    }
    [HttpGet("permissions"), Authorize(Policy = "Permission:Roles.View")]
    [ProducesResponseType(typeof(PermissionModule[]), 200)]
    public Task<IActionResult> Catalog(CancellationToken ct) => Admin(() => service.CatalogAsync(ct));
    [HttpGet("roles"), Authorize(Policy = "Permission:Roles.View")]
    [ProducesResponseType(typeof(AccessRole[]), 200)]
    public Task<IActionResult> Roles(CancellationToken ct) => Admin(() => service.RolesAsync(ct));
    [HttpGet("roles/{roleId:guid}"), Authorize(Policy = "Permission:Roles.View")]
    [ProducesResponseType(typeof(RoleAccessDetails), 200)]
    public Task<IActionResult> Role(Guid roleId, CancellationToken ct) => Admin(() => service.RoleAsync(roleId, ct));
    [HttpGet("users"), Authorize(Policy = "Permission:Users.View")]
    [ProducesResponseType(typeof(AccessPage<AccessUserItem>), 200)]
    public Task<IActionResult> Users([FromQuery] AccessInventoryQuery query, CancellationToken ct) => Admin(() => service.UsersAsync(query, ct));
    [HttpGet("users/{userId:guid}"), Authorize(Policy = "Permission:Users.View")]
    [ProducesResponseType(typeof(UserAccessDetails), 200)]
    public Task<IActionResult> UserAccess(Guid userId, CancellationToken ct) => Admin(() => service.UserAsync(userId, ct));
    [HttpGet("history"), Authorize(Policy = "Permission:Users.View"), Authorize(Policy = "Permission:Roles.View")]
    [ProducesResponseType(typeof(AccessPage<AccessAuditDetails>), 200)]
    public Task<IActionResult> History([FromQuery] AccessAuditQuery query, CancellationToken ct) => Admin(() => service.HistoryAsync(query, ct));
    [HttpPut("users/{userId:guid}/roles/{roleId:guid}"), Authorize(Policy = "Permission:Users.Manage")]
    [ProducesResponseType(typeof(AccessChangeResult), 200)]
    public Task<IActionResult> Membership(Guid userId, Guid roleId, RoleMembershipRequest request, CancellationToken ct)
        => Admin(() => service.MembershipAsync(userId, roleId, request, ct));
    [HttpPut("roles/{roleId:guid}/permissions/{permissionId:guid}"), Authorize(Policy = "Permission:Roles.Manage")]
    [ProducesResponseType(typeof(AccessChangeResult), 200)]
    public Task<IActionResult> Grant(Guid roleId, Guid permissionId, RoleGrantRequest request, CancellationToken ct)
        => Admin(() => service.GrantAsync(roleId, permissionId, request, ct));
    [HttpPut("users/{userId:guid}/permissions/{permissionId:guid}"), Authorize(Policy = "Permission:Users.Manage")]
    [ProducesResponseType(typeof(AccessChangeResult), 200)]
    public Task<IActionResult> Override(Guid userId, Guid permissionId, PermissionOverrideRequest request, CancellationToken ct)
        => Admin(() => service.OverrideAsync(userId, permissionId, request, ct));
}
