using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CampionsSystem.BLL.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CampionsSystem.API.Authentication;

public sealed class JwtTokenIssuer(IOptions<JwtOptions> options, TimeProvider clock)
{
    public LoginResponse Issue(CurrentUserContext user)
    {
        var settings = options.Value;
        var now = clock.GetUtcNow();
        var expiresAt = now.AddMinutes(settings.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture), ClaimValueTypes.Integer64)
        };
        if (!string.IsNullOrEmpty(user.UserName)) claims.Add(new Claim("name", user.UserName));
        claims.AddRange(user.Roles.Select(role => new Claim("role", role)));
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims,
            now.UtcDateTime, expiresAt.UtcDateTime,
            new SigningCredentials(settings.GetSigningKey(), SecurityAlgorithms.HmacSha256));
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt,
            new LoginUser(user.UserId, user.UserName, user.EmployeeId, user.EmployeeName ?? user.UserName, user.Roles));
    }
}
