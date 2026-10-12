using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Coaching;
using CampionsSystem.BLL.Commissions;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;

public sealed partial class SalesConcurrencyTests
{
    private (CoachingService Coaching, SalesService Sales, CommissionService Periods) CoachingServices(ApplicationDbContext db, TimeProvider? clock = null)
    {
        var user = new TestBranchUser(new(actor, "coach", null, null, [], [..SalesPermissions.Codes, ..SubscriptionPermissions.Codes,
            CoachingPermissions.PrivateView, CoachingPermissions.PrivateManage, CoachingPermissions.RegisterPrivate, CoachingPermissions.Transfer,
            CoachingPermissions.ClassesView, CoachingPermissions.ClassesManage, CoachingPermissions.RegisterClass, CoachingPermissions.EarningsView,
            CoachingPermissions.Targets, CoachingPermissions.Recognize, CoachingPermissions.Pay, CommissionPermissions.Close, CommissionPermissions.View, "Branches.AccessAll"], new(branch, "Branch")));
        var access = new OperationAccess(user, db, clock ?? new Clock(), Options.Create(new AuthContextOptions())); var subs = new SubscriptionService(db, user, access);
        return (new(db, user, access, subs), new(db, user, access, new(db, user, access, subs), subs), new(db, user, access));
    }
    private async Task<(CoachingPurchaseReceipt Receipt, Guid Reception, Guid Coach)> ConcurrentPrivate(int sessions = 10, decimal price = 1000)
    {
        await Buy(Guid.NewGuid()); var checkIn = (await Entry(Guid.NewGuid())).CheckIn!.Id;
        await using var db = Context(); var services = CoachingServices(db); var coach = await db.Employees.SingleAsync(e => e.UserId == actor);
        var p = await services.Coaching.SavePackageAsync(null, new(branch, "Private", sessions, 30, price), false);
        return (await services.Sales.PurchaseCoachingAsync(new(Guid.NewGuid(), member, p.Id, [new(account, price)], coach.Id), false), checkIn, coach.Id);
    }
    [Fact] public async Task CoachingConcurrentPrivateRetryConsumesExactlyOneSession()
    {
        var f = await ConcurrentPrivate(); var r = new PrivateAttendanceRequest(Guid.NewGuid(), f.Reception);
        async Task<PrivateAttendanceDetails> Register() { await using var db = Context(); return await CoachingServices(db).Coaching.RegisterPrivateAsync(f.Receipt.SubscriptionId, r); }
        var results = await Task.WhenAll(Register(), Register()); Assert.Equal(results[0].Id, results[1].Id);
        await using var verify = Context(); Assert.Single(await verify.PrivateSessions.ToListAsync()); Assert.Equal(9, (await verify.PrivateSubscriptions.SingleAsync()).RemainingSessions);
    }
    [Fact] public async Task CoachingConcurrentDifferentPrivateOperationsCannotConsumeSameReceptionTwice()
    {
        var f = await ConcurrentPrivate();
        async Task<bool> Register() { await using var db = Context(); try { await CoachingServices(db).Coaching.RegisterPrivateAsync(f.Receipt.SubscriptionId, new(Guid.NewGuid(), f.Reception)); return true; } catch (BusinessRuleException) { return false; } }
        Assert.Single(await Task.WhenAll(Register(), Register()), x => x); await using var verify = Context(); Assert.Single(await verify.PrivateSessions.ToListAsync());
    }
    [Fact] public async Task CoachingConcurrentClassesCannotOveruseLastSharedSessionAcrossDifferentTypes()
    {
        Guid sub; var occurrences = new List<Guid>();
        await using (var db = Context())
        {
            var services = CoachingServices(db); var coach = await db.Employees.SingleAsync(e => e.UserId == actor);
            var p = await services.Coaching.SavePackageAsync(null, new(branch, "Shared", 1, 30, 100), true);
            sub = (await services.Sales.PurchaseCoachingAsync(new(Guid.NewGuid(), member, p.Id, [new(account, 100)]), true)).SubscriptionId;
            foreach (var name in new[] { "Yoga", "Zumba" })
            {
                var type = await services.Coaching.SaveClassTypeAsync(null, new(branch, name, coach.Id));
                occurrences.Add((await services.Coaching.CreateOccurrenceAsync(new(Guid.NewGuid(), type.Id, coach.Id, new Clock().GetUtcNow().AddHours(1)))).Id);
            }
        }
        async Task<bool> Register(Guid occurrence) { await using var db = Context(); try { await CoachingServices(db).Coaching.RegisterClassAsync(occurrence, new(Guid.NewGuid(), member, sub)); return true; } catch (BusinessRuleException) { return false; } }
        Assert.Single(await Task.WhenAll(Register(occurrences[0]), Register(occurrences[1])), x => x);
        await using var verify = Context(); Assert.Single(await verify.CheckIns.ToListAsync()); Assert.Equal(0, (await verify.ClassesSubscriptions.SingleAsync()).RemainingSessions);
    }
    private async Task<(Guid Coach, Guid Category)> ConcurrentEarnings()
    {
        var f = await ConcurrentPrivate(1, 8000); await using var db = Context(); var services = CoachingServices(db);
        await services.Coaching.RegisterPrivateAsync(f.Receipt.SubscriptionId, new(Guid.NewGuid(), f.Reception));
        var period = await services.Periods.OpenAsync(new(Guid.NewGuid(), "Period", new(2026, 10, 10), new(2026, 10, 10)));
        await services.Coaching.ConfigureTargetsAsync(new(Guid.NewGuid(), period.Id, f.Coach, 10000, null));
        await CoachingServices(db, new NextDayClock()).Coaching.RecognizeAsync(new(Guid.NewGuid(), period.Id));
        var category = new ExpenseCategory { Name = "Coach payout" }; db.Add(category); await db.SaveChangesAsync(); return (f.Coach, category.Id);
    }
    [Fact] public async Task CoachingConcurrentPartialPayoutsCannotOverpayOutstandingBalance()
    {
        var (coach, category) = await ConcurrentEarnings();
        async Task<bool> Pay() { await using var db = Context(); try { await CoachingServices(db, new NextDayClock()).Coaching.PayAsync(new(Guid.NewGuid(), branch, coach, 3000, account, category, "Part")); return true; } catch (BusinessRuleException) { return false; } }
        Assert.Single(await Task.WhenAll(Pay(), Pay()), x => x); await using var verify = Context();
        Assert.Single(await verify.CoachPayouts.ToListAsync()); Assert.Equal(1000, (await CoachingServices(verify).Coaching.BalanceAsync(branch, coach)).Outstanding);
        Assert.Single(await verify.Expenses.ToListAsync()); Assert.Equal(3000, await verify.CoachPayoutAllocations.SumAsync(a => a.Amount));
    }
    [Fact] public async Task CoachingConcurrentPayoutRetryDebitsOneExpenseExactlyOnce()
    {
        var (coach, category) = await ConcurrentEarnings(); var r = new CoachPayoutRequest(Guid.NewGuid(), branch, coach, 2000, account, category, "Part");
        async Task<CoachPayoutDetails> Pay() { await using var db = Context(); return await CoachingServices(db, new NextDayClock()).Coaching.PayAsync(r); }
        var results = await Task.WhenAll(Pay(), Pay()); Assert.Equal(results[0].Id, results[1].Id); await using var verify = Context();
        Assert.Single(await verify.CoachPayouts.ToListAsync()); Assert.Single(await verify.Expenses.ToListAsync());
        Assert.Single(await verify.FinancialTransactions.Where(t => t.ExpenseId != null).ToListAsync());
    }
    [Fact] public async Task CoachingConcurrentRefundAndRecognitionNeverRecognizeRefundedMoney()
    {
        var f = await ConcurrentPrivate(); Guid period;
        await using (var db = Context())
        {
            var service = CoachingServices(db); await service.Coaching.RegisterPrivateAsync(f.Receipt.SubscriptionId, new(Guid.NewGuid(), f.Reception));
            period = (await service.Periods.OpenAsync(new(Guid.NewGuid(), "Period", new(2026, 10, 10), new(2026, 10, 10)))).Id;
            await service.Coaching.ConfigureTargetsAsync(new(Guid.NewGuid(), period, f.Coach, 50, null));
        }
        async Task<bool> Refund() { await using var db = Context(); try { await CoachingServices(db, new NextDayClock()).Sales.RefundAsync(f.Receipt.SaleId, new(Guid.NewGuid(), 950, [new(account, 950)], "Refund")); return true; } catch (BusinessRuleException) { return false; } }
        async Task Recognize() { await using var db = Context(); await CoachingServices(db, new NextDayClock()).Coaching.RecognizeAsync(new(Guid.NewGuid(), period)); }
        var refund = Refund(); await Task.WhenAll(refund, Recognize()); await using var verify = Context();
        Assert.Equal(await refund ? 50 : 100, await verify.CoachEarnings.SumAsync(e => e.EligibleValue));
        Assert.Equal(await refund ? 30 : 60, await verify.CoachEarnings.SumAsync(e => e.Amount));
    }
}
