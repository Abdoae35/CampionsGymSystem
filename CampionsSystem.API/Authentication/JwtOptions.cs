using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;

namespace CampionsSystem.API.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required] public string Issuer { get; init; } = string.Empty;
    [Required] public string Audience { get; init; } = string.Empty;
    [Range(1, 60)] public int AccessTokenMinutes { get; init; } = 30;
    [Required] public string Key { get; init; } = string.Empty;

    public bool HasValidSigningKey()
    {
        try { return Convert.FromBase64String(Key).Length >= 32; }
        catch (FormatException) { return false; }
    }
    public SymmetricSecurityKey GetSigningKey() => new(Convert.FromBase64String(Key));
}
