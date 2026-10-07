using System.Text.Json;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.Extensions.Options;

namespace CampionsSystem.BLL.Operations;

public sealed class BusinessRuleException(string message) : Exception(message);

// Business services use this as well as API policies, including branch checks.
public sealed class OperationAccess(ICurrentUserService current, ApplicationDbContext database,
    TimeProvider clock, IOptions<AuthContextOptions> options)
{
    public DateTimeOffset Now => clock.GetUtcNow();
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(Now,
        TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId)).DateTime);

    public async Task<Guid> RequireAsync(string permission, Guid? branchId, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        if (branchId.HasValue) BranchAccessService.EnsureScope(user, branchId);
        return user.UserId;
    }

    public void Audit(Guid actor, Guid? branch, string action, string entityType, Guid id,
        object? before = null, object? after = null) => database.AuditLogs.Add(new AuditLog
        {
            UserId = actor, BranchId = branch, Action = action, EntityType = entityType, EntityId = id,
            OldValues = before is null ? null : JsonSerializer.Serialize(before),
            NewValues = after is null ? null : JsonSerializer.Serialize(after), CreatedAt = Now
        });

    public static void Percentage(decimal value)
    {
        if (value < 0 || value > 100 || decimal.Round(value, 2) != value)
            throw new BusinessRuleException("Percentage must be between 0 and 100 with at most two decimal places.");
    }
    public static void Money(decimal value)
    {
        if (value < 0 || decimal.Round(value, 2) != value)
            throw new BusinessRuleException("Amount must be nonnegative with at most two decimal places.");
    }
}
