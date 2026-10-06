using Microsoft.AspNetCore.Authorization;

namespace CampionsSystem.API.Authorization;

public sealed record PermissionRequirement(string Code) : IAuthorizationRequirement;
