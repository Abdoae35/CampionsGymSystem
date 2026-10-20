using CampionsSystem.BLL.Coaching;
using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Commissions;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;

public sealed partial class SalesTests
{
    private CoachingService Coaching() => new(db, user, access, subscriptions);
    private async Task<Employee> CoachingSetup()
    {
        user.Context = user.Context! with { Permissions = [..user.Context.Permissions, ..CheckInPermissions.Codes,
            CoachingPermissions.PrivateView, CoachingPermissions.PrivateManage, CoachingPermissions.RegisterPrivate, CoachingPermissions.Transfer, CoachingPermissions.Expiry,
            CoachingPermissions.ClassesView, CoachingPermissions.ClassesManage, CoachingPermissions.RegisterClass, "Classes.ViewAttendanceReport",
            CoachingPermissions.EarningsView, CoachingPermissions.Targets, CoachingPermissions.Recognize, CoachingPermissions.Pay, ..ShiftPermissions.Codes,
            CommissionPermissions.Close, CommissionPermissions.Manage, CommissionPermissions.View] };
        return await db.Employees.SingleAsync(e => e.UserId == actor.Id);
    }
    private async Task<CoachingPurchaseReceipt> PrivatePurchase(int sessions = 10, decimal price = 1000, int days = 30, decimal? tender = null)
    {
        var coach = await CoachingSetup(); var p = await Coaching().SavePackageAsync(null, new(b.Id, "Private " + Guid.NewGuid(), sessions, days, price), false);
        return await Finance().PurchaseCoachingAsync(new(Guid.NewGuid(), member.Id, p.Id, [new((await Account()).Id, tender ?? price)], coach.Id,
            tender == null ? PurchaseMode.FullPayment : PurchaseMode.Installment,
            tender == null ? null : [new(price - tender.Value, today.AddDays(29))]), false);
    }
    private async Task<Guid> CoachingReception()
    {
        if (!await db.MemberSubscriptions.AnyAsync()) await Finance().CreateAsync(Purchase(new PaymentPartInput((await Account()).Id, 1200)));
        var result = await new CheckInService(db, user, access, subscriptions).CreateAsync(new(Guid.NewGuid(), CheckInMethod.Manual, member.Id));
        Assert.True(result.Decision.Allowed, result.Decision.ReasonCode); return result.CheckIn!.Id;
    }
    private async Task<PrivateAttendanceDetails> DeliverPrivate(Guid sub)
        => await Coaching().RegisterPrivateAsync(sub, new(Guid.NewGuid(), await CoachingReception()));
    private async Task<Guid> CoachingPeriod(DateOnly? from = null, DateOnly? to = null)
        => (await new CommissionService(db, user, access).OpenAsync(new(Guid.NewGuid(), "Coaching", from ?? today, to ?? today))).Id;
    private async Task<CoachEarningsProgress> FinalizeCoaching(Guid period)
    {
        var end = (await db.CommissionPeriods.FindAsync(period))!.EndBusinessDate!.Value;
        clock.Now = new DateTimeOffset(end.AddDays(1).ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero);
        return await Coaching().RecognizeAsync(new(Guid.NewGuid(), period));
    }
    [Theory][InlineData("SessionValue")][InlineData("SessionNumber")]
    public async Task CoachingDatabaseRejectsIncompleteOperationalPrivateEvidence(string column)
    {
        var sale = await PrivatePurchase(); var session = await DeliverPrivate(sale.SubscriptionId);
        var exception = await Assert.ThrowsAsync<Npgsql.PostgresException>(() => column == "SessionValue"
            ? db.Database.ExecuteSqlRawAsync("UPDATE \"PrivateSessions\" SET \"SessionValue\"=NULL WHERE \"Id\"={0}", session.Id)
            : db.Database.ExecuteSqlRawAsync("UPDATE \"PrivateSessions\" SET \"SessionNumber\"=NULL WHERE \"Id\"={0}", session.Id));
        Assert.Equal("23514", exception.SqlState);
    }
    [Fact] public async Task CoachingDatabaseRejectsOriginalEarningWithoutTargetSnapshot()
    {
        var sale = await PrivatePurchase(); await DeliverPrivate(sale.SubscriptionId); var period = await CoachingPeriod();
        var coach = await db.Employees.SingleAsync(); await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, coach.Id, 100, null));
        await FinalizeCoaching(period);
        var exception = await Assert.ThrowsAsync<Npgsql.PostgresException>(() => db.Database.ExecuteSqlRawAsync("UPDATE \"CoachEarnings\" SET \"Target\"=NULL"));
        Assert.Equal("23514", exception.SqlState);
    }
    [Fact] public async Task CoachingPrivateRequiresReceptionAndNeverDeductsOnReceptionAlone()
    {
        var sale = await PrivatePurchase();
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().RegisterPrivateAsync(sale.SubscriptionId, new(Guid.NewGuid(), Guid.NewGuid())));
        var reception = await CoachingReception(); Assert.Equal(10, (await Coaching().PrivateDetailsAsync(sale.SubscriptionId)).RemainingSessions);
        var r = new PrivateAttendanceRequest(Guid.NewGuid(), reception); var first = await Coaching().RegisterPrivateAsync(sale.SubscriptionId, r);
        Assert.Equal(member.FullName,first.MemberName);Assert.Equal(member.MemberCode,first.MemberCode);
        Assert.Equal((await CoachingSetup()).FullName,first.CoachEmployeeName);
        var retry = await Coaching().RegisterPrivateAsync(sale.SubscriptionId, r); Assert.Equal(first.Id, retry.Id);
        var details=await Coaching().PrivateDetailsAsync(sale.SubscriptionId);
        Assert.Equal(member.FullName,details.MemberName);Assert.Equal(member.MemberCode,details.MemberCode);
        Assert.Equal(first.CoachEmployeeName,details.OriginalCoachEmployeeName);
        Assert.Equal(first.CoachEmployeeName,Assert.Single(details.Assignments).CoachEmployeeName);
        Assert.Equal(member.FullName,Assert.Single((await Coaching().PrivateHistoryAsync(sale.SubscriptionId)).Items).MemberName);
        Assert.Equal(9, (await Coaching().PrivateDetailsAsync(sale.SubscriptionId)).RemainingSessions); Assert.Single(await db.PrivateSessions.ToListAsync());
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().RegisterPrivateAsync(sale.SubscriptionId, r with { OperationId = Guid.NewGuid() }));
    }
    [Fact] public async Task CoachingPrivateRejectsExpiredRegularMembershipEvenWithPriorReception()
    {
        var sale = await PrivatePurchase(); var reception = await CoachingReception(); clock.Now = clock.Now.AddDays(31);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().RegisterPrivateAsync(sale.SubscriptionId, new(Guid.NewGuid(), reception)));
        Assert.Empty(await db.PrivateSessions.ToListAsync());
    }
    [Fact] public async Task CoachingPrivateRejectsMembershipExpiredAfterReceptionOnSameBusinessDate()
    {
        var sale = await PrivatePurchase(); var reception = await CoachingReception(); var membership = await db.MemberSubscriptions.SingleAsync();
        membership.StartDate = today.AddDays(-30); membership.EndDate = today.AddDays(-1); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().RegisterPrivateAsync(sale.SubscriptionId, new(Guid.NewGuid(), reception)));
        Assert.Equal(10, (await Coaching().PrivateDetailsAsync(sale.SubscriptionId)).RemainingSessions);
    }
    [Fact] public async Task CoachingPrivateCannotUseFrozenRegularMembership()
    {
        var sale = await PrivatePurchase(); var reception = await CoachingReception(); var membership = await db.MemberSubscriptions.SingleAsync();
        membership.Status = SubscriptionStatus.Frozen; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().RegisterPrivateAsync(sale.SubscriptionId, new(Guid.NewGuid(), reception)));
    }
    [Theory][InlineData(300, 50, 100)][InlineData(200, 60, 120)]
    public async Task CoachingPrivateNormalCoachUsesIndependentNetFundedTarget(int target, int rate, int earned)
    {
        var sale = await PrivatePurchase(); var period = await CoachingPeriod(to: today.AddDays(1));
        var coach = (await db.Employees.SingleAsync()).Id; await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, coach, target, null));
        await DeliverPrivate(sale.SubscriptionId); clock.Now = clock.Now.AddDays(1); await DeliverPrivate(sale.SubscriptionId);
        var progress = await FinalizeCoaching(period); Assert.Equal(earned, Assert.Single(progress.Coaches).ProjectedEarnings);
        Assert.All(await db.CoachEarnings.ToListAsync(), e => Assert.Equal(rate, e.Rate));
        Assert.Empty(await db.CoachPayouts.ToListAsync()); Assert.Equal(earned, (await Coaching().BalanceAsync(b.Id, coach)).Outstanding);
    }
    [Fact] public async Task CoachingPrivateFinancialTargetSettingsNeverBlockAttendance()
    {
        var sale = await PrivatePurchase(); await DeliverPrivate(sale.SubscriptionId); var period = await CoachingPeriod();
        Assert.Null(Assert.Single((await Coaching().ProgressAsync(period)).Coaches).ProjectedEarnings);
        clock.Now = clock.Now.AddDays(1); await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().RecognizeAsync(new(Guid.NewGuid(), period)));
        Assert.Empty(await db.CoachEarnings.ToListAsync()); Assert.Single(await db.PrivateSessions.ToListAsync());
    }
    private async Task<Employee> IncomingCoach()
    {
        var login = new ApplicationUser { UserName = "incoming-" + Guid.NewGuid() };
        var coach = new Employee { User = login, FullName = "Incoming", Phone = "incoming-" + Guid.NewGuid(), HireDate = today.AddYears(-1) };
        db.Add(new EmployeeBranchAssignment { Employee = coach, BranchId = b.Id, StartDate = today.AddYears(-1), AssignedByUserId = actor.Id });
        await db.SaveChangesAsync(); return coach;
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task CoachingTransferPreservesDeliveredAttributionAndIncomingFixedFifty(bool reached)
    {
        var sale = await PrivatePurchase(sessions: 10); var original = await db.Employees.SingleAsync(); var period = await CoachingPeriod(to: today.AddDays(9));
        await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, original.Id, reached ? 200 : 201, null));
        await DeliverPrivate(sale.SubscriptionId); clock.Now = clock.Now.AddDays(1); await DeliverPrivate(sale.SubscriptionId);
        var incoming = await IncomingCoach(); await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, incoming.Id, 1, null));
        await Coaching().TransferAsync(sale.SubscriptionId, new(Guid.NewGuid(), incoming.Id, 2, "Member request"));
        for (var i = 0; i < 8; i++)
        {
            clock.Now = clock.Now.AddDays(1); user.Context = user.Context! with { UserId = actor.Id }; var checkIn = await CoachingReception();
            user.Context = user.Context! with { UserId = incoming.UserId!.Value }; await Coaching().RegisterPrivateAsync(sale.SubscriptionId, new(Guid.NewGuid(), checkIn));
        }
        user.Context = user.Context! with { UserId = actor.Id }; await FinalizeCoaching(period);
        var earnings = await db.CoachEarnings.ToListAsync(); Assert.Equal(reached ? 120 : 100, earnings.Where(e => e.CoachEmployeeId == original.Id).Sum(e => e.Amount));
        Assert.Equal(400, earnings.Where(e => e.CoachEmployeeId == incoming.Id).Sum(e => e.Amount));
        Assert.All(earnings.Where(e => e.CoachEmployeeId == incoming.Id), e => { Assert.True(e.TransferredPackage); Assert.Equal(50, e.Rate); });
        Assert.Equal(2, await db.PrivateCoachAssignments.CountAsync()); Assert.Equal(10, await db.PrivateSessions.CountAsync());
    }
    [Fact] public async Task CoachingTransferRetryIsIdempotentAndCannotRewriteSessionCoach()
    {
        var sale = await PrivatePurchase(); var session = await DeliverPrivate(sale.SubscriptionId); var incoming = await IncomingCoach();
        var r = new PrivateTransferRequest(Guid.NewGuid(), incoming.Id, 1, "Transfer"); var first = await Coaching().TransferAsync(sale.SubscriptionId, r);
        Assert.Equal(incoming.FullName,first.CoachEmployeeName);
        Assert.Equal(first.Id, (await Coaching().TransferAsync(sale.SubscriptionId, r)).Id);
        Assert.Equal(session.CoachEmployeeId, (await db.PrivateSessions.SingleAsync()).CoachEmployeeId);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Coaching().RegisterPrivateAsync(sale.SubscriptionId, new(Guid.NewGuid(), session.GymCheckInId)));
    }
    [Fact] public async Task CoachingPrivateSnapshotsAndDeterministicRemainderSurviveCatalogEdits()
    {
        var sale = await PrivatePurchase(3, 100); var period = await CoachingPeriod(to: today.AddDays(2)); var coach = await db.Employees.SingleAsync();
        await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, coach.Id, 100, null));
        var p = await db.PrivatePackages.SingleAsync(); p.Price = 999; p.SessionCount = 20; await db.SaveChangesAsync();
        for (var i = 0; i < 3; i++) { if (i > 0) clock.Now = clock.Now.AddDays(1); await DeliverPrivate(sale.SubscriptionId); }
        Assert.Equal(new decimal[] { 33.33m, 33.33m, 33.34m }, await db.PrivateSessions.OrderBy(s => s.SessionNumber).Select(s => s.SessionValue!.Value).ToArrayAsync());
        await FinalizeCoaching(period); Assert.Equal(60, await db.CoachEarnings.SumAsync(e => e.Amount));
    }
    [Fact] public async Task CoachingPartialCollectionsDeferDeliveryAndLaterFundSameOriginalSourceOnce()
    {
        var sale = await PrivatePurchase(2, 1000, tender: 200); var coach = await db.Employees.SingleAsync(); var period = await CoachingPeriod();
        await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, coach.Id, 200, null)); await DeliverPrivate(sale.SubscriptionId);
        var first = await FinalizeCoaching(period); var progress = Assert.Single(first.Coaches); Assert.Equal(200, progress.EligibleValue); Assert.Equal(300, progress.DeferredValue); Assert.Equal(120, progress.ProjectedEarnings);
        await new CommissionService(db, user, access).FinalizeAsync(period, new(Guid.NewGuid(), 0));
        var next = await CoachingPeriod(today.AddDays(1), today.AddDays(1)); await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), next, coach.Id, 10000, null));
        var obligation = await db.Installments.SingleAsync(i => i.InstallmentAgreement.SaleTransactionId == sale.SaleId);
        await Finance().RecordPaymentAsync(sale.SaleId, new(Guid.NewGuid(), 800, [new((await Account()).Id, 800)], [new(obligation.Id, 800)]));
        await FinalizeCoaching(next); Assert.Equal(500, await db.CoachEarnings.SumAsync(e => e.EligibleValue)); Assert.Equal(270, await db.CoachEarnings.SumAsync(e => e.Amount));
        Assert.Single(await db.PrivateSessions.ToListAsync()); Assert.Equal(2, await db.CoachEarnings.CountAsync()); Assert.Single(await db.MemberSubscriptions.ToListAsync());
    }
    [Fact] public async Task CoachingExpiryRecognizesCurrentTransferredCoachWithoutFakeAttendanceOrDoubleSettlement()
    {
        var sale = await PrivatePurchase(10, 1000, 2); await DeliverPrivate(sale.SubscriptionId); var incoming = await IncomingCoach();
        await Coaching().TransferAsync(sale.SubscriptionId, new(Guid.NewGuid(), incoming.Id, 1, "Transfer")); clock.Now = clock.Now.AddDays(2);
        var period = await CoachingPeriod(today, today.AddDays(2)); var original = (await db.PrivateSubscriptionTransactions.SingleAsync()).OriginalCoachEmployeeId!.Value;
        await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, original, 100, null));
        var request = new PrivateExpiryRequest(Guid.NewGuid(), period, "Owner recognizes expired balance"); var settlement = await Coaching().SettleExpiryAsync(sale.SubscriptionId, request);
        Assert.Equal(incoming.FullName,settlement.CoachEmployeeName);
        Assert.Equal(incoming.Id, settlement.CoachEmployeeId); Assert.Equal(9, settlement.UnusedSessions); Assert.True(settlement.TransferredPackage);
        Assert.Equal(settlement.Id, (await Coaching().SettleExpiryAsync(sale.SubscriptionId, request)).Id);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Coaching().SettleExpiryAsync(sale.SubscriptionId, request with { OperationId = Guid.NewGuid() }));
        Assert.Single(await db.PrivateSessions.ToListAsync()); Assert.Single(await db.CheckIns.ToListAsync());
        await FinalizeCoaching(period); Assert.Equal(450, await db.CoachEarnings.Where(e => e.CoachEmployeeId == incoming.Id).SumAsync(e => e.Amount));
        Assert.Single(await db.PrivateSessions.ToListAsync());
    }
    [Theory][InlineData(1000, 60)][InlineData(1001, 50)]
    public async Task CoachingOriginalExpiryUsesConfiguredNormalRateWithoutAttendance(decimal target, decimal rate)
    {
        var sale = await PrivatePurchase(3, 1000, 1); var coach = await db.Employees.SingleAsync(); clock.Now = clock.Now.AddDays(1);
        var period = await CoachingPeriod(today, today.AddDays(1)); await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, coach.Id, target, null));
        var settlement = await Coaching().SettleExpiryAsync(sale.SubscriptionId, new(Guid.NewGuid(), period, "Owner entitlement recognition"));
        Assert.Equal(1000, settlement.EligibleValue); Assert.Equal(3, settlement.UnusedSessions); Assert.False(settlement.TransferredPackage);
        await FinalizeCoaching(period); var earning = await db.CoachEarnings.SingleAsync(); Assert.Equal(rate, earning.Rate); Assert.Equal(1000 * rate / 100, earning.Amount);
        Assert.Empty(await db.PrivateSessions.ToListAsync()); Assert.Empty(await db.CheckIns.ToListAsync());
        Assert.Equal(3, (await Coaching().PrivateDetailsAsync(sale.SubscriptionId)).RemainingSessions);
    }
    [Fact] public async Task CoachingNewPackageForTransferredCoachUsesNormalRulesIndependently()
    {
        var old = await PrivatePurchase(2, 200); var incoming = await IncomingCoach();
        await Coaching().TransferAsync(old.SubscriptionId, new(Guid.NewGuid(), incoming.Id, 0, "Transfer"));
        var package = await Coaching().SavePackageAsync(null, new(b.Id, "New independent purchase", 1, 30, 100), false);
        var fresh = await Finance().PurchaseCoachingAsync(new(Guid.NewGuid(), member.Id, package.Id, [new((await Account()).Id, 100)], incoming.Id), false);
        var period = await CoachingPeriod(to: today.AddDays(1)); await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, incoming.Id, 200, null));
        var firstReception = await CoachingReception(); user.Context = user.Context! with { UserId = incoming.UserId!.Value };
        await Coaching().RegisterPrivateAsync(old.SubscriptionId, new(Guid.NewGuid(), firstReception));
        clock.Now = clock.Now.AddDays(1); user.Context = user.Context! with { UserId = actor.Id }; var nextReception = await CoachingReception();
        user.Context = user.Context! with { UserId = incoming.UserId!.Value }; await Coaching().RegisterPrivateAsync(fresh.SubscriptionId, new(Guid.NewGuid(), nextReception));
        user.Context = user.Context! with { UserId = actor.Id }; await FinalizeCoaching(period);
        Assert.Equal(50, (await db.CoachEarnings.SingleAsync(e => e.SaleTransactionId == old.SaleId)).Rate);
        Assert.Equal(60, (await db.CoachEarnings.SingleAsync(e => e.SaleTransactionId == fresh.SaleId)).Rate);
        Assert.Equal(110, (await Coaching().BalanceAsync(b.Id, incoming.Id)).Earned);
    }
    [Fact] public async Task CoachingRefundBeforeRecognitionReducesEligibleCollectedValueAndRevokesUnusedGrant()
    {
        var sale = await PrivatePurchase(); await DeliverPrivate(sale.SubscriptionId); var period = await CoachingPeriod(); var coach = await db.Employees.SingleAsync();
        await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, coach.Id, 50, null));
        await Finance().RefundAsync(sale.SaleId, new(Guid.NewGuid(), 950, [new((await Account()).Id, 950)], "Partial refund"));
        await FinalizeCoaching(period); Assert.Equal(50, await db.CoachEarnings.SumAsync(e => e.EligibleValue)); Assert.Equal(30, await db.CoachEarnings.SumAsync(e => e.Amount));
        Assert.Single(await db.PrivateSessions.ToListAsync()); Assert.True((await Coaching().PrivateDetailsAsync(sale.SubscriptionId)).Cancelled);
    }
    [Fact] public async Task CoachingRefundAffectingFinalizedEarningsRejectsAtomically()
    {
        var sale = await PrivatePurchase(); await DeliverPrivate(sale.SubscriptionId); var period = await CoachingPeriod(); var coach = await db.Employees.SingleAsync();
        await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(), period, coach.Id, 100, null)); await FinalizeCoaching(period);
        var ledger = await db.FinancialTransactions.CountAsync(); var audits = await db.AuditLogs.CountAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(async () => await Finance().RefundAsync(sale.SaleId, new(Guid.NewGuid(), 950, [new((await Account()).Id, 950)], "Refund")));
        Assert.Empty(await db.Refunds.ToListAsync()); Assert.Equal(ledger, await db.FinancialTransactions.CountAsync()); Assert.Equal(audits, await db.AuditLogs.CountAsync());
        Assert.False((await Coaching().PrivateDetailsAsync(sale.SubscriptionId)).Cancelled);
    }
}
