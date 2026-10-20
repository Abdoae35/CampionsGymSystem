using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CampionsSystem.BLL.Authentication;

public abstract class AccessChangeRequest
{
    public Guid OperationId { get; init; }
    [Required, StringLength(64, MinimumLength = 64)] public string ExpectedVersion { get; init; } = "";
    [Required, MaxLength(2000)] public string Reason { get; init; } = "";
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RoleMembershipRequest : AccessChangeRequest
{
    [Required] public bool? Assigned { get; init; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RoleGrantRequest : AccessChangeRequest
{
    [Required] public bool? Granted { get; init; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PermissionOverrideRequest : AccessChangeRequest
{
    // null clears the override; strings intentionally exclude numeric enum inputs.
    [JsonRequired, RegularExpression("^(Allow|Deny)$")] public string? Effect { get; init; }
}
public sealed class AccessInventoryQuery
{
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 100)] public int PageSize { get; init; } = 25;
    [MaxLength(256)] public string? Search { get; init; }
    public Guid? RoleId { get; init; }
    public Guid? BranchId { get; init; }
    public bool? IsActive { get; init; }
}
public sealed class AccessAuditQuery
{
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 100)] public int PageSize { get; init; } = 25;
    public Guid? TargetUserId { get; init; }
    public Guid? RoleId { get; init; }
    public Guid? ActorUserId { get; init; }
    public Guid? OperationId { get; init; }
}
public sealed record AccessPage<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
public sealed record PermissionCatalogItem(Guid Id, string Code, string Title, string? Description, bool IsActive);
public sealed record PermissionModule(string Module, IReadOnlyList<PermissionCatalogItem> Permissions);
public sealed record AccessRole(Guid Id, string Name, bool IsOwner);
public sealed record RoleAccessDetails(AccessRole Role, string Version, IReadOnlyList<PermissionCatalogItem> Grants);
public sealed record PermissionExplanation(Guid PermissionId, string Code, string Title, string Module,
    bool IsActive, IReadOnlyList<AccessRole> InheritedFromRoles, string? Override, bool Effective);
public sealed record AccessBranchAssignment(Guid Id, Guid BranchId, string BranchName, bool BranchIsActive,
    DateOnly StartDate, DateOnly? EndDate, bool IsPrimary);
public sealed record AccessEmployee(Guid Id, string FullName, string Status, DateOnly HireDate, DateOnly? TerminationDate);
public sealed record UserAccessDetails(Guid UserId, string? UserName, bool IsActive, bool IsLockedOut,
    bool RuntimeAccountEligible, AccessEmployee? Employee, BranchContext? CurrentBranch,
    bool HasGlobalBranchScope, IReadOnlyList<AccessBranchAssignment> BranchAssignments,
    IReadOnlyList<AccessRole> Roles, string Version, IReadOnlyList<PermissionExplanation> Permissions);
public sealed record AccessUserItem(Guid UserId, string? UserName, bool IsActive,
    Guid? EmployeeId, string? EmployeeName);
public sealed record AccessChangeResult(Guid OperationId, bool Changed, string Version);
public sealed record AccessAuditDetails(Guid AuditId, Guid OperationId, Guid? ActorUserId,
    DateTimeOffset Timestamp, string Kind, Guid? TargetUserId, Guid? RoleId, Guid? PermissionId,
    string Reason, string OldValues, string NewValues, AccessChangeResult Result,
    string? ActorName = null, string? TargetUserName = null, string? RoleName = null, string? PermissionCode = null);
public sealed class AccessAdministrationException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}
