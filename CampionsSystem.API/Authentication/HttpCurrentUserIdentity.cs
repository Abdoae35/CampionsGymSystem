using System.Security.Claims;
using CampionsSystem.BLL.Authentication;

namespace CampionsSystem.API.Authentication;

public sealed class HttpCurrentUserIdentity(IHttpContextAccessor accessor) : ICurrentUserIdentity
{
    public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated == true && UserId.HasValue;
    public Guid? UserId => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue("sub"), out var id) ? id : null;
}
