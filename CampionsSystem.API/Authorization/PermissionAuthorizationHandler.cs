using System.Security.Claims;
using CampionsSystem.BLL.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace CampionsSystem.API.Authorization;

public sealed class PermissionAuthorizationHandler(IPermissionService permissions, IHttpContextAccessor accessor)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true
            || !Guid.TryParse(context.User.FindFirstValue("sub"), out var userId)) return;
        var codes = await permissions.GetUserPermissionsAsync(userId,
            accessor.HttpContext?.RequestAborted ?? CancellationToken.None);
        if (codes.Contains(requirement.Code)) context.Succeed(requirement);
    }
}
