using CampionsSystem.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CampionsSystem.API.Authentication;

public static class AuthenticationRegistration
{
    public static IServiceCollection AddGymAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations().Validate(settings => settings.HasValidSigningKey(),
                "Jwt:Key must be a base64-encoded secret of at least 32 random bytes.")
            .ValidateOnStart();
        services.AddOptions<AuthContextOptions>().Bind(configuration.GetSection(AuthContextOptions.SectionName))
            .Validate(settings => IsValidTimeZone(settings.TimeZoneId), "AuthContext:TimeZoneId must be a valid time zone.")
            .ValidateOnStart();
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserIdentity, HttpCurrentUserIdentity>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<UserContextService>();
        services.AddScoped<EmployeeBranchResolver>();
        services.AddScoped<ICurrentBranchContext, CurrentBranchContext>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<JwtTokenIssuer>();
        services.AddScoped<ActiveUserJwtEvents>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, settings) =>
            {
                bearer.MapInboundClaims = false;
                bearer.IncludeErrorDetails = false;
                bearer.EventsType = typeof(ActiveUserJwtEvents);
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Value.Audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = settings.Value.GetSigningKey(),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        return services;
    }

    private static bool IsValidTimeZone(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        try { TimeZoneInfo.FindSystemTimeZoneById(id); return true; }
        catch (TimeZoneNotFoundException) { return false; }
        catch (InvalidTimeZoneException) { return false; }
    }
}
