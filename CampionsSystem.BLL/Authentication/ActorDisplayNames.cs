using CampionsSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Authentication;

/// <summary>Resolves Identity User IDs to current Employee names for read contracts.</summary>
public static class ActorDisplayNames
{
    public static async Task<IReadOnlyDictionary<Guid, string>> LoadAsync(
        ApplicationDbContext db, IEnumerable<Guid?> userIds, CancellationToken ct = default)
    {
        var ids = userIds.Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value).Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<Guid, string>();

        var rows = await db.Employees.AsNoTracking()
            .Where(employee => employee.UserId.HasValue && ids.Contains(employee.UserId.Value))
            .Select(employee => new { UserId = employee.UserId!.Value, employee.FullName })
            .ToListAsync(ct);

        return rows.Where(row => !string.IsNullOrWhiteSpace(row.FullName))
            .ToDictionary(row => row.UserId, row => row.FullName.Trim());
    }

    public static Task<IReadOnlyDictionary<Guid, string>> LoadAsync(
        ApplicationDbContext db, IEnumerable<Guid> userIds, CancellationToken ct = default) =>
        LoadAsync(db, userIds.Select(id => (Guid?)id), ct);

    public static string? Get(IReadOnlyDictionary<Guid, string> names, Guid? userId) =>
        userId.HasValue && names.TryGetValue(userId.Value, out var name) ? name : null;
}
