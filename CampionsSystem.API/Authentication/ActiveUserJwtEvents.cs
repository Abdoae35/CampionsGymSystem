using System.Security.Claims;
using CampionsSystem.BLL.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CampionsSystem.API.Authentication;

public sealed class ActiveUserJwtEvents(UserContextService contexts) : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        if (!Guid.TryParse(context.Principal?.FindFirstValue("sub"), out var userId))
        {
            context.Fail("Invalid credentials.");
            return;
        }
        var user = await contexts.ResolveAsync(userId, context.HttpContext.RequestAborted);
        if (user is null)
        {
            context.Fail("Invalid credentials.");
            return;
        }
        // Even rare role-only checks reflect current Identity membership, not stale JWT roles.
        if (context.Principal?.Identity is ClaimsIdentity identity)
        {
            foreach (var claim in identity.FindAll(identity.RoleClaimType).ToArray()) identity.RemoveClaim(claim);
            identity.AddClaims(user.Roles.Select(role => new Claim(identity.RoleClaimType, role)));
        }
    }
}
