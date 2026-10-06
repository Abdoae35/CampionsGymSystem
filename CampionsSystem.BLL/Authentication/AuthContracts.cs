using System.ComponentModel.DataAnnotations;

namespace CampionsSystem.BLL.Authentication;

public sealed class LoginRequest
{
    [Required, MaxLength(256)]
    public string UserNameOrEmail { get; init; } = string.Empty;
    [Required, MaxLength(256)]
    public string Password { get; init; } = string.Empty;
}

public sealed record BranchContext(Guid Id, string Name);
public sealed record CurrentUserContext(Guid UserId, string? UserName, Guid? EmployeeId,
    string? EmployeeName, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions,
    BranchContext? CurrentBranch)
{
    public Guid? CurrentBranchId => CurrentBranch?.Id;
}
public sealed record LoginUser(Guid Id, string? UserName, Guid? EmployeeId, string? FullName,
    IReadOnlyList<string> Roles);
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, LoginUser User);

public interface IAuthenticationService
{
    Task<CurrentUserContext?> ValidateCredentialsAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
public interface IPermissionService
{
    Task<HashSet<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
// API provides claims access; BLL never depends on HttpContext.
public interface ICurrentUserIdentity
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
}
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    // DB-backed EmployeeId, CurrentBranchId, Roles and Permissions are asynchronous.
    Task<CurrentUserContext?> GetAsync(CancellationToken cancellationToken = default);
}

public sealed class AuthContextOptions
{
    public const string SectionName = "AuthContext";
    public string TimeZoneId { get; init; } = "Africa/Cairo";
}
