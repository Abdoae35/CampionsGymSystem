using CampionsSystem.DAL.Enums;

namespace CampionsSystem.DAL.Entities;

public sealed class UserPermissionOverride
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
    public PermissionOverrideEffect Effect { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
