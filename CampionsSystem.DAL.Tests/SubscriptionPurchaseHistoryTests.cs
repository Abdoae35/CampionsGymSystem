using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.DAL.Tests;

public sealed partial class SalesTests
{
    [Fact] public async Task PaidFirstEntryPurchaseAppearsWithoutCreatingEntitlement()
    {
        var cash = await Account(); plan.AccessMode = AccessMode.SessionBased; plan.SessionCount = 12;
        await db.SaveChangesAsync();
        var sale = await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id, 1200))
            with { ActivationMode = SubscriptionActivationMode.FirstCheckIn });

        Assert.Empty((await subscriptions.GetHistoryAsync(member.Id, new())).Items);
        var history = await subscriptions.GetPurchaseHistoryAsync(member.Id, new() { IncludePending = true });
        var item = Assert.Single(history.Items);
        Assert.Equal(1, history.TotalCount);
        Assert.Equal(sale.SaleId, item.SaleId);
        Assert.Null(item.SubscriptionId); Assert.Null(item.StartDate); Assert.Null(item.EndDate);
        Assert.Equal("PendingFirstCheckIn", item.ActivationStatus); Assert.Equal("Paid", item.PaymentStatus);
        Assert.Equal("Monthly", item.PlanName); Assert.Equal(plan.Id, item.PlanId);
        Assert.Equal("SessionBased", item.Mode); Assert.Equal(30, item.PurchasedDurationDays);
        Assert.Equal(b.Name, item.OriginBranchName); Assert.Equal(b.City.Name, item.AccessCityName);
        Assert.Equal(12, item.TotalSessions); Assert.Equal(12, item.RemainingSessions);
        Assert.Equal(1200, item.FinalPrice); Assert.Equal(1200, item.Collected);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());
        Assert.Empty(await db.MembershipSubscriptionTransactions.ToListAsync());
        Assert.False((await subscriptions.GetCurrentEntitlementAsync(member.Id, b.Id)).HasAccess);
    }

    [Fact] public async Task FirstEntryReplacesPendingHistoryItemWithoutDuplication()
    {
        Reception(); var cash = await Account(); plan.AccessMode = AccessMode.SessionBased; plan.SessionCount = 12;
        await db.SaveChangesAsync();
        var sale = await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id, 1200))
            with { ActivationMode = SubscriptionActivationMode.FirstCheckIn });
        clock.Now = clock.Now.AddDays(4);
        var entry = await Entrance().CreateAsync(new(Guid.NewGuid(), CheckInMethod.Manual, member.Id));
        Assert.True(entry.Decision.Allowed);

        var history = await subscriptions.GetPurchaseHistoryAsync(member.Id, new() { IncludePending = true });
        var item = Assert.Single(history.Items);
        Assert.Equal(1, history.TotalCount); Assert.NotNull(item.SubscriptionId);
        Assert.Null(item.SaleId); Assert.Equal("Active", item.ActivationStatus);
        Assert.Equal(today.AddDays(4), item.StartDate); Assert.Equal(11, item.RemainingSessions);
        Assert.Equal(entry.Decision.SubscriptionId, item.SubscriptionId);
        Assert.Equal(b.Name, item.OriginBranchName); Assert.Equal(b.City.Name, item.AccessCityName);
        Assert.Equal(1, (await subscriptions.GetHistoryAsync(member.Id, new())).TotalCount);
        Assert.Equal(sale.SaleId, (await db.MembershipSubscriptionTransactions.SingleAsync()).SaleTransactionId);
    }

    [Fact] public async Task ImmediateSaleAndEarlyFirstEntryRenewalKeepExistingPeriodHistory()
    {
        var cash = await Account(); plan.AccessMode = AccessMode.SessionBased; plan.SessionCount = 12;
        await db.SaveChangesAsync();
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id, 1200)));
        var initial = Assert.Single((await subscriptions.GetPurchaseHistoryAsync(member.Id, new())).Items);
        Assert.NotNull(initial.SubscriptionId); Assert.Equal("Active", initial.ActivationStatus);
        Assert.Null(initial.SaleId);

        clock.Now = clock.Now.AddDays(15);
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id, 1200))
            with { ActivationMode = SubscriptionActivationMode.FirstCheckIn });
        var history = await subscriptions.GetPurchaseHistoryAsync(member.Id, new());
        var item = Assert.Single(history.Items);
        Assert.Equal(initial.SubscriptionId, item.SubscriptionId); Assert.Equal(1, history.TotalCount);
        Assert.Equal(24, item.RemainingSessions);
        Assert.Equal(2, await db.MembershipSubscriptionTransactions.CountAsync());
    }

    [Fact] public async Task CancelledPendingSaleAndUnpaidReservationAreNotInPurchaseHistory()
    {
        var cash = await Account();
        var reservation = await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id, 200))
            with { PurchaseMode = PurchaseMode.Reservation, ActivationMode = SubscriptionActivationMode.FirstCheckIn });
        Assert.Empty((await subscriptions.GetPurchaseHistoryAsync(member.Id, new())).Items);
        await Finance().RefundAsync(reservation.SaleId, new(Guid.NewGuid(), 200, [new(cash.Id, 200)], "Cancel reservation"));
        Assert.Empty((await subscriptions.GetPurchaseHistoryAsync(member.Id, new())).Items);

        var paid = await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id, 1200))
            with { ActivationMode = SubscriptionActivationMode.FirstCheckIn });
        Assert.Single((await subscriptions.GetPurchaseHistoryAsync(member.Id, new())).Items);
        await Finance().RefundAsync(paid.SaleId, new(Guid.NewGuid(), 1200, [new(cash.Id, 1200)], "Cancel pending sale"));
        Assert.Empty((await subscriptions.GetPurchaseHistoryAsync(member.Id, new())).Items);
    }

    [Fact] public async Task QualifiedPendingInstallmentShowsActualCollectionState()
    {
        var cash = await Account();
        var sale = await Finance().CreateAsync(InstallmentPurchase(cash.Id)
            with { ActivationMode = SubscriptionActivationMode.FirstCheckIn });
        var partial = Assert.Single((await subscriptions.GetPurchaseHistoryAsync(member.Id, new())).Items);
        Assert.Equal(sale.SaleId, partial.SaleId);
        Assert.Equal("InstallmentActive", partial.PaymentStatus);
        Assert.Equal(200, partial.Collected); Assert.Equal(1200, partial.FinalPrice);

        var lines = await db.Installments.OrderBy(x => x.InstallmentNumber).ToListAsync();
        await Finance().RecordPaymentAsync(sale.SaleId,
            new(Guid.NewGuid(), 1000, [new(cash.Id, 1000)], [new(lines[0].Id, 500), new(lines[1].Id, 500)]));
        var paid = Assert.Single((await subscriptions.GetPurchaseHistoryAsync(member.Id, new())).Items);
        Assert.Equal("Paid", paid.PaymentStatus); Assert.Equal(1200, paid.Collected);
        Assert.Null(paid.SubscriptionId);
    }

    [Fact] public async Task PendingHistoryPreservesBranchPermissionFiltersAndGlobalPagination()
    {
        var cash = await Account();
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id, 1200)));
        clock.Now = clock.Now.AddDays(31);
        var sale = await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id, 1200))
            with { ActivationMode = SubscriptionActivationMode.FirstCheckIn });
        var first = await subscriptions.GetPurchaseHistoryAsync(member.Id, new() { PageSize = 1 });
        var second = await subscriptions.GetPurchaseHistoryAsync(member.Id, new() { Page = 2, PageSize = 1 });
        Assert.Equal(2, first.TotalCount); Assert.Equal(2, second.TotalCount);
        Assert.Equal(sale.SaleId, Assert.Single(first.Items).SaleId);
        Assert.NotNull(Assert.Single(second.Items).SubscriptionId);
        Assert.Empty((await subscriptions.GetPurchaseHistoryAsync(member.Id,
            new() { From = today.AddDays(32) })).Items);
        Assert.Single((await subscriptions.GetPurchaseHistoryAsync(member.Id,
            new() { From = today.AddDays(31) })).Items);
        Assert.Single((await subscriptions.GetPurchaseHistoryAsync(member.Id,
            new() { Status = SubscriptionStatus.Expired })).Items);
        Assert.Single((await subscriptions.GetPurchaseHistoryAsync(member.Id,
            new() { PlanType = PlanCategory.Normal, From = today.AddDays(31) })).Items);
        Assert.Empty((await subscriptions.GetPurchaseHistoryAsync(member.Id,
            new() { PlanType = PlanCategory.Freelance })).Items);

        user.Context = user.Context! with { Permissions = [SubscriptionPermissions.View], CurrentBranch = new(a.Id, a.Name) };
        Assert.Empty((await subscriptions.GetPurchaseHistoryAsync(member.Id, new())).Items);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => subscriptions.GetPurchaseHistoryAsync(member.Id,
            new() { BranchId = b.Id }));
        user.Context = user.Context! with { Permissions = [], CurrentBranch = new(b.Id, b.Name) };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => subscriptions.GetPurchaseHistoryAsync(member.Id, new()));
    }
}
