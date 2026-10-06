using CampionsSystem.API.Authentication;
using CampionsSystem.BLL.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampionsSystem.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthenticationService authentication, JwtTokenIssuer tokens,
    ICurrentUserService currentUser) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await authentication.ValidateCredentialsAsync(request, cancellationToken);
        if (user is null) return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid credentials.");
        return Ok(tokens.Issue(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserContext>> Me(CancellationToken cancellationToken)
    {
        var user = await currentUser.GetAsync(cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }
}
