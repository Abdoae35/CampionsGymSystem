using CampionsSystem.BLL.Coaching;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;

public sealed partial class SalesTests
{
    private async Task<CoachingPurchaseReceipt> ClassesPurchase(int sessions = 10)
    {
        await CoachingSetup(); var p = await Coaching().SavePackageAsync(null, new(b.Id, "Shared classes " + Guid.NewGuid(), sessions, 30, 1000), true);
        return await Finance().PurchaseCoachingAsync(new(Guid.NewGuid(), member.Id, p.Id, [new((await Account()).Id, 1000)]), true);
    }
    private async Task<ClassOccurrenceDetails> ClassOccurrence(string name = "Yoga")
    {
        var coach = await db.Employees.SingleAsync(e => e.UserId == actor.Id);
        var type = await Coaching().SaveClassTypeAsync(null, new(b.Id, name, coach.Id));
        return await Coaching().CreateOccurrenceAsync(new(Guid.NewGuid(), type.Id, coach.Id, clock.Now.AddHours(1)));
    }
    [Fact] public async Task CoachingClassesNeedNoReceptionOrRegularMembershipAndShareOneBalanceAcrossTypes()
    {
        var sale = await ClassesPurchase(2); var yoga = await ClassOccurrence(); var zumba = await ClassOccurrence("Zumba");
        Assert.Equal("Yoga",yoga.ClassTypeName);Assert.NotNull(yoga.CoachEmployeeName);
        Assert.Equal(yoga.CoachEmployeeName,Assert.Single((await Coaching().ClassTypesAsync(b.Id)).Items,x=>x.Name=="Yoga").CoachEmployeeName);
        Assert.Equal(member.FullName,Assert.Single((await Coaching().ClassesSubscriptionsAsync(b.Id,member.Id)).Items).MemberName);
        var r = new ClassAttendanceRequest(Guid.NewGuid(), member.Id, sale.SubscriptionId);
        var first = await Coaching().RegisterClassAsync(yoga.Id, r); Assert.Equal(1, first.RemainingSessions);
        Assert.Equal(member.FullName,first.MemberName);Assert.Equal(member.MemberCode,first.MemberCode);
        Assert.Equal("Yoga",first.ClassTypeName);Assert.Equal(yoga.CoachEmployeeName,first.CoachEmployeeName);
        Assert.Equal(first.Id, (await Coaching().RegisterClassAsync(yoga.Id, r)).Id);
        var second = await Coaching().RegisterClassAsync(zumba.Id, r with { OperationId = Guid.NewGuid() }); Assert.Equal(0, second.RemainingSessions);
        Assert.Equal(2, (await Coaching().ClassMemberHistoryAsync(b.Id, member.Id)).TotalCount);
        Assert.Equal("Yoga",Assert.Single((await Coaching().ClassAttendanceHistoryAsync(yoga.Id)).Items).ClassTypeName);
        Assert.Equal("Zumba",Assert.Single((await Coaching().OccurrencesAsync(b.Id,today,today)).Items,x=>x.Id==zumba.Id).ClassTypeName);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync()); Assert.Empty(await db.CheckIns.Where(c => c.CheckInType == CheckInType.Gym).ToListAsync());
        Assert.All(await db.CheckIns.ToListAsync(), c => { Assert.Equal(CheckInType.Class, c.CheckInType); Assert.True(c.SessionConsumed); });
        await Assert.ThrowsAsync<BusinessRuleException>(async () => await Coaching().RegisterClassAsync((await ClassOccurrence("CrossFit")).Id, new(Guid.NewGuid(), member.Id)));
    }
    [Fact] public async Task CoachingClassesPreventDuplicateMemberOccurrenceEvenWithDifferentOperationsOrPackages()
    {
        var first = await ClassesPurchase(); var second = await ClassesPurchase(); var occurrence = await ClassOccurrence();
        await Coaching().RegisterClassAsync(occurrence.Id, new(Guid.NewGuid(), member.Id, first.SubscriptionId));
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().RegisterClassAsync(occurrence.Id, new(Guid.NewGuid(), member.Id, second.SubscriptionId)));
        Assert.Equal(19, await db.ClassesSubscriptions.SumAsync(s => s.RemainingSessions)); Assert.Single(await db.CheckIns.ToListAsync());
    }
    [Fact] public async Task CoachingClassesRequireAuthorizedAssignedCoachAndPreClassRegistration()
    {
        await ClassesPurchase(); var occurrence = await ClassOccurrence(); var incoming = await IncomingCoach();
        user.Context = user.Context! with { UserId = incoming.UserId!.Value };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Coaching().RegisterClassAsync(occurrence.Id, new(Guid.NewGuid(), member.Id)));
        user.Context = user.Context! with { UserId = actor.Id }; clock.Now = clock.Now.AddHours(2);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().RegisterClassAsync(occurrence.Id, new(Guid.NewGuid(), member.Id)));
    }
    [Fact] public async Task CoachingClassesRejectExpiredAndRefundCancelledPackage()
    {
        var sale = await ClassesPurchase(); var occurrence = await ClassOccurrence();
        await Finance().RefundAsync(sale.SaleId, new(Guid.NewGuid(), 1000, [new((await Account()).Id, 1000)], "Refund"));
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().RegisterClassAsync(occurrence.Id, new(Guid.NewGuid(), member.Id, sale.SubscriptionId)));
        Assert.Empty(await db.CheckIns.ToListAsync()); Assert.Equal(10, (await db.ClassesSubscriptions.SingleAsync()).RemainingSessions);
    }
    [Fact] public async Task CoachingClassesAttendanceAverageIncludesZeroAttendanceCompletedOccurrencesAndConfiguredTarget()
    {
        await ClassesPurchase(); var period = await CoachingPeriod(); var coach = await db.Employees.SingleAsync();
        await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, coach.Id, null, 0.5m));
        var first = await ClassOccurrence(); var second = await ClassOccurrence("Zumba");
        await Coaching().RegisterClassAsync(first.Id, new(Guid.NewGuid(), member.Id));
        clock.Now = clock.Now.AddHours(2);
        await Coaching().CompleteOccurrenceAsync(first.Id, new(Guid.NewGuid(), 0)); await Coaching().CompleteOccurrenceAsync(second.Id, new(Guid.NewGuid(), 0));
        var stats = await Coaching().ClassStatisticsAsync(b.Id, coach.Id, today, today, period);
        Assert.Equal(2, stats.CompletedClasses); Assert.Equal(1, stats.TotalAttendance); Assert.Equal(0.5m, stats.AverageAttendance); Assert.True(stats.TargetReached);
        Assert.Equal(coach.FullName,stats.CoachEmployeeName);
        Assert.Equal("NotConfigured", stats.CompensationStatus); Assert.Empty(await db.CoachEarnings.ToListAsync());
    }
    [Fact] public async Task CoachingClassesTenEightTwelveAttendeesAverageTen()
    {
        await CoachingSetup(); var coach = await db.Employees.SingleAsync(); var occurrences = new List<ClassOccurrenceDetails>();
        var package = await Coaching().SavePackageAsync(null, new(b.Id, "Shared", 3, 30, 300), true);
        var members = new List<Guid>();
        for (var i = 0; i < 12; i++)
        {
            var m = new Member { HomeBranchId = b.Id, CreatedByUserId = actor.Id, MemberCode = "CLASS-" + i, FullName = "Attendee " + i, PrimaryPhone = "class-" + i };
            db.Add(m); await db.SaveChangesAsync(); members.Add(m.Id);
            await Finance().PurchaseCoachingAsync(new(Guid.NewGuid(), m.Id, package.Id, [new((await Account()).Id, 300)]), true);
        }
        foreach (var count in new[] { 10, 8, 12 })
        {
            var c = await ClassOccurrence("Class " + count); occurrences.Add(c);
            foreach (var m in members.Take(count)) await Coaching().RegisterClassAsync(c.Id, new(Guid.NewGuid(), m));
        }
        clock.Now = clock.Now.AddHours(2); foreach (var c in occurrences) await Coaching().CompleteOccurrenceAsync(c.Id, new(Guid.NewGuid(), 0));
        var stats = await Coaching().ClassStatisticsAsync(b.Id, coach.Id, today, today);
        Assert.Equal(30, stats.TotalAttendance); Assert.Equal(10, stats.AverageAttendance); Assert.Null(stats.Target); Assert.Null(stats.TargetReached);
        Assert.Equal(new[] { 10, 8, 12 }.Order(), stats.Occurrences.Select(c => c.Attendance).Order());
    }
    [Fact] public async Task CoachingOccurrenceCompletionIsIdempotentAndCannotCancelAttendance()
    {
        await ClassesPurchase(); var c = await ClassOccurrence(); await Coaching().RegisterClassAsync(c.Id, new(Guid.NewGuid(), member.Id));
        clock.Now = clock.Now.AddHours(2);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().CompleteOccurrenceAsync(c.Id, new(Guid.NewGuid(), 0, true)));
        var r = new CompleteClassRequest(Guid.NewGuid(), 0); var ended = await Coaching().CompleteOccurrenceAsync(c.Id, r);
        Assert.Equal(ended.Id, (await Coaching().CompleteOccurrenceAsync(c.Id, r)).Id); Assert.Equal("Completed", ended.Status);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().CompleteOccurrenceAsync(c.Id, r with { OperationId = Guid.NewGuid() }));
    }
    [Fact] public async Task CoachingBranchScopeAndMemberSearchPreservePurchaseOriginRatherThanHomeBranch()
    {
        await PrivatePurchase(); var results = await Coaching().SearchAsync(b.Id, false, member.MemberCode); Assert.Single(results.Items);
        user.Context = user.Context! with { Permissions = [CoachingPermissions.PrivateView, CoachingPermissions.RegisterPrivate, CoachingPermissions.ClassesView] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Coaching().SearchAsync(a.Id, false, member.FullName));
        Assert.Empty((await Coaching().SearchAsync(b.Id, true, member.MemberCode)).Items);
    }
    private async Task<(Guid Coach, Guid Category)> CoachingEarned(decimal price = 8000)
    {
        var sale = await PrivatePurchase(1, price); var period = await CoachingPeriod(); var coach = await db.Employees.SingleAsync();
        await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, coach.Id, price + 1, null)); await DeliverPrivate(sale.SubscriptionId); await FinalizeCoaching(period);
        var category = new ExpenseCategory { Name = "Coach earnings" }; db.Add(category); await db.SaveChangesAsync(); return (coach.Id, category.Id);
    }
    [Fact] public async Task CoachingOwnerPartialPayoutsReduceFourThousandToTwoThousandThenZeroExactlyOnce()
    {
        var (coach, category) = await CoachingEarned(); var account = (await Account()).Id;
        var coachName=(await db.Employees.SingleAsync(x=>x.Id==coach)).FullName;
        Assert.Equal(coachName,Assert.Single((await Coaching().EarningsHistoryAsync(b.Id,coach)).Items).CoachEmployeeName);
        var initial = await new VaultService(db, user, access).PositionAsync(b.Id); var r = new CoachPayoutRequest(Guid.NewGuid(), b.Id, coach, 2000, account, category, "Part payment");
        var first = await Coaching().PayAsync(r); Assert.Equal(4000, first.BalanceBefore); Assert.Equal(2000, first.BalanceAfter);
        Assert.Equal(coachName,first.CoachEmployeeName);Assert.Equal(coachName,first.AuthorizedByName);
        Assert.Equal(coachName,Assert.Single((await Coaching().PayoutHistoryAsync(b.Id,coach)).Items).CoachEmployeeName);
        Assert.Equal(first.Id, (await Coaching().PayAsync(r)).Id); Assert.Equal(2000, (await Coaching().BalanceAsync(b.Id, coach)).Outstanding);
        Assert.Equal(coachName,(await Coaching().BalanceAsync(b.Id,coach)).CoachEmployeeName);
        clock.Now = clock.Now.AddMonths(1); var second = await Coaching().PayAsync(r with { OperationId = Guid.NewGuid(), Reason = "Remainder" });
        Assert.Equal(0, second.BalanceAfter); Assert.Equal(0, (await Coaching().BalanceAsync(b.Id, coach)).Outstanding);
        Assert.Equal(2, await db.CoachPayouts.CountAsync()); Assert.Equal(2, await db.Expenses.CountAsync());
        Assert.Equal(2, await db.FinancialTransactions.CountAsync(x => x.SourceType == FinancialSourceType.Expense));
        Assert.Equal(initial.ExpectedPhysicalCash - 4000, (await new VaultService(db, user, access).PositionAsync(b.Id)).ExpectedPhysicalCash);
        Assert.Single(await db.CoachEarnings.ToListAsync()); Assert.Equal(4000, await db.CoachPayoutAllocations.SumAsync(x => x.Amount));
    }
    [Fact] public async Task CoachingPayoutRejectsOverpaymentAndKeepsLedgerUnchanged()
    {
        var (coach, category) = await CoachingEarned(); var ledger = await db.FinancialTransactions.CountAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(async () => await Coaching().PayAsync(new(Guid.NewGuid(), b.Id, coach, 4000.01m, (await Account()).Id, category, "Too much")));
        Assert.Empty(await db.CoachPayouts.ToListAsync()); Assert.Empty(await db.Expenses.ToListAsync()); Assert.Equal(ledger, await db.FinancialTransactions.CountAsync());
    }
    [Fact] public async Task CoachingPayoutRequiresOwnerCapabilitiesAndGlobalPermissionEvenWhenBranchManagerHasPay()
    {
        var (coach, category) = await CoachingEarned(); var cash = (await Account()).Id;
        user.Context = user.Context! with { Permissions = [CoachingPermissions.Pay, CoachingPermissions.EarningsView] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await Coaching().PayAsync(new(Guid.NewGuid(), b.Id, coach, 100, cash, category, "Part")));
        Assert.Empty(await db.CoachPayouts.ToListAsync());
    }
    [Fact] public async Task CoachingPayoutChecksPhysicalCashAndElectronicAccountFundsWithoutInventingMoney()
    {
        var (coach, category) = await CoachingEarned(); var insta = await Account(FinancialAccountType.InstaPay);
        await Assert.ThrowsAsync<BusinessRuleException>(async () => await Coaching().PayAsync(new(Guid.NewGuid(), b.Id, coach, 100, insta.Id, category, "Unfunded")));
        await new VaultService(db, user, access).CountAsync(new(Guid.NewGuid(), 0, "Physical shortage", b.Id));
        await Assert.ThrowsAsync<BusinessRuleException>(async () => await Coaching().PayAsync(new(Guid.NewGuid(), b.Id, coach, 100, (await Account()).Id, category, "Cash unavailable")));
        Assert.Empty(await db.CoachPayouts.ToListAsync()); Assert.Empty(await db.Expenses.ToListAsync());
    }
    [Fact] public async Task CoachingPayoutCannotBypassBalanceThroughGenericExpenseReversal()
    {
        var (coach, category) = await CoachingEarned(); var payout = await Coaching().PayAsync(new(Guid.NewGuid(), b.Id, coach, 100, (await Account()).Id, category, "Part"));
        await Assert.ThrowsAsync<BusinessRuleException>(() => new ExpenseService(db, user, access).ReverseAsync(payout.ExpenseId, new(Guid.NewGuid(), 100, true, "Returned")));
        Assert.Empty(await db.ExpenseReversals.ToListAsync()); Assert.Equal(3900, (await Coaching().BalanceAsync(b.Id, coach)).Outstanding);
    }
    [Fact] public async Task CoachingRecognitionRetryPreservesFinalEvidenceAndDoesNotWriteMoney()
    {
        var sale = await PrivatePurchase(); await DeliverPrivate(sale.SubscriptionId); var coach = await db.Employees.SingleAsync(); var period = await CoachingPeriod();
        var target=await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, coach.Id, 100, null));
        Assert.Equal(coach.FullName,target.CoachEmployeeName);
        Assert.Equal(coach.FullName,Assert.Single(await Coaching().TargetsAsync(period)).CoachEmployeeName);
        clock.Now = clock.Now.AddDays(1);
        var ledger = await db.FinancialTransactions.CountAsync(); var r = new RecognizeCoachEarningsRequest(Guid.NewGuid(), period);
        Assert.Equal(coach.FullName,Assert.Single((await Coaching().RecognizeAsync(r)).Coaches).CoachEmployeeName);
        Assert.Equal(coach.FullName,Assert.Single((await Coaching().ProgressAsync(period)).Coaches).CoachEmployeeName);
        await Coaching().RecognizeAsync(r); Assert.Single(await db.CoachEarnings.ToListAsync());
        Assert.Equal(ledger, await db.FinancialTransactions.CountAsync()); Assert.Empty(await db.CoachPayouts.ToListAsync());
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().RecognizeAsync(r with { OperationId = Guid.NewGuid() }));
    }
}
