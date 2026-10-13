using CampionsSystem.BLL.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace CampionsSystem.API.Authorization;

public sealed class OwnerAdministrationRequirement : IAuthorizationRequirement;
public sealed class OwnerAdministrationHandler(OwnerAdministrationAccess access)
    : AuthorizationHandler<OwnerAdministrationRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnerAdministrationRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true) return;
        try { await access.RequireAsync(); context.Succeed(requirement); }
        catch (UnauthorizedAccessException) { }
    }
}
