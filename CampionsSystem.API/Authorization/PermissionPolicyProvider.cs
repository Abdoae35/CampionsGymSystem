using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CampionsSystem.API.Authorization;

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public const string Prefix = "Permission:";

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(Prefix, StringComparison.Ordinal) || policyName.Length == Prefix.Length)
            return base.GetPolicyAsync(policyName);

        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName[Prefix.Length..])).Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
