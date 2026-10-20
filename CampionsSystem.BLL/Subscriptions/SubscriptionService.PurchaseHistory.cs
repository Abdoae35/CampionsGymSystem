using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Subscriptions;

public sealed partial class SubscriptionService
{
    private static SubscriptionHistoryItem HistoryItem(SubscriptionSummary s) => new(
        s.Id, null, s.MemberId, s.SourcePlanId, s.PlanName, s.PlanType, s.OriginBranchId,
        s.AccessCityId, s.Mode, s.StartDate, s.EndDate, s.Status, s.TotalSessions,
        s.RemainingSessions, null, null, null, null, null, null,
        s.OriginBranchName, s.AccessCityName);

    // The default history contract remains unchanged. This opt-in view inserts the one
    // possible pending paid sale into the same ordered, filtered page of period history.
    public async Task<SubscriptionPurchaseHistory> GetPurchaseHistoryAsync(Guid memberId,
        SubscriptionHistoryRequest r, CancellationToken ct = default)
    {
        var user = await RequireAsync(SubscriptionPermissions.View, r.BranchId, false, ct);
        ValidateHistoryRequest(r);
        var periods = HistoryQuery(memberId, r, user);
        var periodCount = await periods.CountAsync(ct);

        SubscriptionHistoryItem? pending = null;
        DateOnly pendingOrderDate = default;
        if (!r.Status.HasValue)
        {
            var sales = PendingSales(memberId).Where(s => s.MembershipSnapshot != null);
            if (!user.Permissions.Contains("Branches.AccessAll") || r.BranchId != null)
            {
                var branch = r.BranchId ?? user.CurrentBranchId ?? throw new UnauthorizedAccessException();
                sales = sales.Where(s => s.BranchId == branch);
            }
            if (r.PlanType.HasValue) sales = sales.Where(s => s.MembershipSnapshot!.PlanCategory == r.PlanType);
            if (r.From.HasValue) sales = sales.Where(s => s.BusinessDate >= r.From);
            if (r.To.HasValue) sales = sales.Where(s => s.BusinessDate <= r.To);
            var matches = await sales.Select(s => new
            {
                s.Id, s.MemberId, s.BranchId, s.BusinessDate, s.CreatedAt, s.PurchaseMode,
                s.NetAmount, s.ExtraDaysSnapshot,
                PlanId = s.MembershipSnapshot!.MembershipPlanId,
                PlanName = s.MembershipSnapshot.PlanName,
                PlanType = s.MembershipSnapshot.PlanCategory,
                CityId = s.MembershipSnapshot.CityId,
                Mode = s.MembershipSnapshot.AccessMode,
                DurationDays = s.MembershipSnapshot.DurationDays,
                Sessions = s.MembershipSnapshot.SessionCount,
                Collected = db.Payments.Where(p => p.SaleTransactionId == s.Id && p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount)
            }).Take(2).ToListAsync(ct);
            if (matches.Count > 1) throw new BusinessRuleException("Ambiguous paid entitlements pending first entry.");
            if (matches.Count == 1)
            {
                var s = matches[0];
                pendingOrderDate = s.BusinessDate ?? DateOnly.FromDateTime(s.CreatedAt.UtcDateTime);
                pending = new(null, s.Id, s.MemberId, s.PlanId, s.PlanName, s.PlanType.ToString(),
                    s.BranchId, s.CityId, s.Mode.ToString(), null, null, "PendingFirstCheckIn",
                    s.Sessions, s.Sessions, s.BusinessDate, s.PurchaseMode?.ToString(),
                    s.DurationDays + s.ExtraDaysSnapshot, s.NetAmount, s.Collected,
                    s.Collected >= s.NetAmount ? "Paid" : "InstallmentActive");
            }
        }

        // Pending sorts by sale business date, before periods beginning on the same date.
        // All existing period-to-period ordering remains StartDate DESC, Id ASC.
        var insertion = pending is null ? -1 : await periods.CountAsync(x => x.Subscription.StartDate > pendingOrderDate, ct);
        var offset = (r.Page - 1) * r.PageSize;
        var periodOffset = pending is not null && offset > insertion ? offset - 1 : offset;
        var periodRows = await periods.OrderByDescending(x => x.Subscription.StartDate).ThenBy(x => x.Subscription.Id)
            .Skip(periodOffset).Take(r.PageSize).ToListAsync(ct);
        var items = periodRows.Select(x => HistoryItem(Summary(x))).ToList();
        if (pending is not null && insertion >= offset && insertion < (long)offset + r.PageSize)
            items.Insert(insertion - offset, pending);
        if (items.Count > r.PageSize) items.RemoveAt(items.Count - 1);
        var names = await DisplayNamesAsync(items.Select(x => x.OriginBranchId), items.Select(x => x.AccessCityId), ct);
        items = items.Select(x => x with
        {
            OriginBranchName = names.Branches.GetValueOrDefault(x.OriginBranchId),
            AccessCityName = names.Cities.GetValueOrDefault(x.AccessCityId)
        }).ToList();
        return new(items, periodCount + (pending is null ? 0 : 1), r.Page, r.PageSize);
    }
}
