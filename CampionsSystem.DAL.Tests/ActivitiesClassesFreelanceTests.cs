using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CampionsSystem.DAL.Tests;

public sealed class ActivitiesClassesFreelanceTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private readonly FixedClock clock = new();
    private ApplicationUser actor = null!;
    private GymBranch branch = null!;
    private Employee employee = null!;
    private Employee substitute = null!;
    private Member member = null!;
    private SportsActivity activity = null!;
    private ActivityPackage package = null!;
    private GroupClass group = null!;
    private CommissionPeriod period = null!;
    private Shift shift = null!;
    private FinancialAccount account = null!;
    private OperationAccess access = null!;
    private ActivityManagementService coaches = null!;
    private FreelancePlanService plans = null!;
    private SubscriptionPurchaseService purchases = null!;
    private ClassSessionService classes = null!;
    private CoachReportService reports = null!;

    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        actor = new ApplicationUser { UserName = "operations-test" };
        var city = new City { Name = "Test City" };
        branch = new GymBranch { Name = "Test Branch", City = city };
        employee = new Employee { FullName = "Internal Coach", Phone = "test-1", HireDate = new(2026, 1, 1),
            EmploymentStatus = EmploymentStatus.Active, EmploymentType = EmploymentType.Freelance };
        substitute = new Employee { FullName = "Substitute", Phone = "test-2", HireDate = new(2026, 1, 1), EmploymentStatus = EmploymentStatus.Active };
        member = new Member { ClientCode = 1, MemberCode = "M-TEST", FullName = "Test Member", PrimaryPhone = "test-member", HomeBranch = branch, CreatedByUser = actor };
        activity = new SportsActivity { Name = "Boxing", Branch = branch };
        package = new ActivityPackage { SportsActivity = activity, Name = "Monthly Boxing", Price = 500,
            DurationDays = 30, AccessMode = AccessMode.Unlimited };
        group = new GroupClass { Branch = branch, CoachEmployee = employee, Name = "Fitness" };
        period = new CommissionPeriod { Branch = branch, Name = "October", StartAt = clock.GetUtcNow().AddDays(-10), Status = CommissionPeriodStatus.Open };
        shift = new Shift { Branch = branch, OpenedByUser = actor, OpenedAt = period.StartAt, Status = ShiftStatus.Open };
        account = new FinancialAccount { Name = "Cash", Branch = branch, AccountType = FinancialAccountType.Cash };
        db.AddRange(actor, member, package, employee, substitute, group, period, shift, account);
        await db.SaveChangesAsync();
        access = new OperationAccess(new TestCurrentUser(actor.Id, branch.Id), db, clock, Options.Create(new AuthContextOptions()));
        coaches = new(db, access); plans = new(db, access, new(db, new TestCurrentUser(actor.Id, branch.Id), access)); purchases = new(db, access); classes = new(db, access); reports = new(db, access);
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private async Task<Guid> Coach(bool external = false, decimal percentage = 30)
    {
        var id = await coaches.CreateCoachAsync(new(external ? ActivityCoachType.External : ActivityCoachType.Internal,
            external ? null : employee.Id, external ? "External Coach" : null, null, percentage));
        await coaches.AssignAsync(activity.Id, id); return id;
    }
    private async Task<Payment> Pay(PurchaseResult purchase, decimal amount, bool split = false)
    {
        var payment = new Payment { SaleTransactionId = purchase.SaleTransactionId, Branch = branch, Shift = shift,
            CommissionPeriod = period, Amount = amount, Status = PaymentStatus.Completed,
            PaidAt = clock.GetUtcNow().AddHours(-1), ReceivedByUser = actor };
        payment.Parts.Add(new PaymentPart { FinancialAccount = account, Amount = split ? amount / 2 : amount });
        if (split) payment.Parts.Add(new PaymentPart { FinancialAccount = account, Amount = amount / 2 });
        db.Add(payment); await db.SaveChangesAsync(); return payment;
    }
    private async Task<Guid> Plan(bool eligible = true)
    {
        var id = await plans.SaveAsync(null, new(branch.CityId, employee.Id, "Coach Monthly", 300, 30, AccessMode.Unlimited, null, BranchId: branch.Id));
        if (eligible) await plans.SetEligibilityAsync(id, member.Id, true); return id;
    }
    private Task<Guid> Session(ClassSessionStatus status = ClassSessionStatus.Completed, Guid? coach = null) => classes.SaveAsync(null,
        new(group.Id, null, coach ?? employee.Id, clock.GetUtcNow().AddHours(-2),
            status == ClassSessionStatus.Completed ? clock.GetUtcNow().AddHours(-1) : null, status));
    private Task<IReadOnlyList<ClassCoachReport>> Stats() => reports.ClassesAsync(branch.Id, period.StartAt, clock.GetUtcNow());

    [Fact] public async Task InternalCoachLinksToEmployee()
    {
        var id = await Coach(); var row = await db.ActivityCoaches.FindAsync(id);
        Assert.Equal(employee.Id, row!.EmployeeId); Assert.Equal(ActivityCoachType.Internal, row.CoachType);
    }
    [Fact] public async Task ExternalCoachNeedsNeitherEmployeeNorIdentityAccount()
    {
        var id = await Coach(true); var row = await db.ActivityCoaches.FindAsync(id);
        Assert.Null(row!.EmployeeId); Assert.Equal(ActivityCoachType.External, row.CoachType);
        Assert.Single(await db.Users.ToListAsync());
    }
    [Fact] public async Task ActivityPurchaseRequiresSelectedAssignedCoach()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() => purchases.PurchaseActivityAsync(new(member.Id, package.Id, Guid.Empty)));
        var coach = await Coach(); var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, coach));
        Assert.Equal(coach, (await db.ActivitySubscriptions.FindAsync(purchase.SubscriptionId))!.ActivityCoachId);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync()); // Activity does not grant Gym membership.
    }
    [Fact] public async Task PercentageChangePreservesPurchaseSnapshot()
    {
        var coach = await Coach(); var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, coach));
        await coaches.SetPercentageAsync(coach, 25); await Pay(purchase, 500);
        Assert.Equal(30m, Assert.Single(await reports.ActivitiesAsync(period.Id)).Percentage);
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "ActivityCoach.PercentageChanged");
    }
    [Theory] [InlineData(-1)] [InlineData(101)] public async Task PercentageOutOfRangeRejected(decimal rate)
        => await Assert.ThrowsAsync<BusinessRuleException>(async () => await coaches.SetPercentageAsync(await Coach(), rate));
    [Fact] public async Task ActivityEntitlementUsesCollectedInstallmentNotPackagePrice()
    {
        var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, await Coach())); await Pay(purchase, 200);
        var row = Assert.Single(await reports.ActivitiesAsync(period.Id)); Assert.Equal(200, row.EligibleRevenue); Assert.Equal(60, row.Entitlement);
    }
    [Fact] public async Task DiscountedSplitPaymentCountedOnlyOnce()
    {
        var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, await Coach()));
        var sale = await db.SaleTransactions.FindAsync(purchase.SaleTransactionId); sale!.DiscountAmount = 100; sale.NetAmount = 400;
        await db.SaveChangesAsync(); await Pay(purchase, 400, true);
        var row = Assert.Single(await reports.ActivitiesAsync(period.Id)); Assert.Equal(400, row.EligibleRevenue); Assert.Equal(120, row.Entitlement);
    }
    [Fact] public async Task OpenPeriodRefundReducesRevenueAndEntitlement()
    {
        var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, await Coach())); var payment = await Pay(purchase, 500);
        db.Add(new Refund { SaleTransactionId = purchase.SaleTransactionId, OriginalPayment = payment, Branch = branch,
            ProcessedByUser = actor, Amount = 100, Reason = "Test refund", CreatedAt = clock.GetUtcNow().AddMinutes(-1) });
        await db.SaveChangesAsync(); var row = Assert.Single(await reports.ActivitiesAsync(period.Id));
        Assert.Equal(400, row.EligibleRevenue); Assert.Equal(120, row.Entitlement);
    }
    [Fact] public async Task InternalFinalizationBridgesEmployeeSummaryExactlyOnce()
    {
        var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, await Coach())); await Pay(purchase, 500);
        period.Status = CommissionPeriodStatus.Closed; period.EndAt = clock.GetUtcNow(); await db.SaveChangesAsync();
        await reports.FinalizeActivitiesAsync(period.Id);
        var summary = Assert.Single(await db.EmployeeCommissionSummaries.ToListAsync());
        Assert.Equal(employee.Id, summary.EmployeeId); Assert.Equal(150, summary.CommissionAmount);
        await Assert.ThrowsAsync<BusinessRuleException>(() => reports.FinalizeActivitiesAsync(period.Id));
    }
    [Fact] public async Task ExternalEntitlementNeedsNoEmployeePayroll()
    {
        var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, await Coach(true))); await Pay(purchase, 500);
        period.Status = CommissionPeriodStatus.Closed; period.EndAt = clock.GetUtcNow(); await db.SaveChangesAsync();
        await reports.FinalizeActivitiesAsync(period.Id);
        Assert.Equal(150, Assert.Single(await reports.ActivitiesAsync(period.Id)).Entitlement);
        Assert.Null(Assert.Single(await db.ActivityCoachSettlements.ToListAsync()).EmployeeId);
        Assert.Empty(await db.EmployeeCommissionSummaries.ToListAsync()); Assert.Empty(await db.Payrolls.ToListAsync());
    }
    [Fact] public async Task ClosedSnapshotIgnoresLaterRefundAndRateChange()
    {
        var coach = await Coach(); var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, coach)); var payment = await Pay(purchase, 500);
        period.Status = CommissionPeriodStatus.Closed; period.EndAt = clock.GetUtcNow(); await db.SaveChangesAsync();
        await reports.FinalizeActivitiesAsync(period.Id); await coaches.SetPercentageAsync(coach, 90);
        db.Add(new Refund { SaleTransactionId = purchase.SaleTransactionId, OriginalPayment = payment, Branch = branch,
            ProcessedByUser = actor, Amount = 100, Reason = "Later refund", CreatedAt = clock.GetUtcNow().AddDays(1) });
        await db.SaveChangesAsync(); Assert.Equal(150, Assert.Single(await reports.ActivitiesAsync(period.Id)).Entitlement);
    }
    [Fact] public async Task CompletedClassCounts()
    { await Session(); Assert.Equal(1, Assert.Single(await Stats()).CompletedClassesCount); }
    [Fact] public async Task ZeroAttendanceIncludedInDenominator()
    { var id = await Session(); await classes.AttendAsync(id, member.Id); await Session(); Assert.Equal(0.5m, Assert.Single(await Stats()).AverageAttendance); }
    [Fact] public async Task CancelledAndScheduledClassesExcluded()
    { await Session(ClassSessionStatus.Cancelled); await Session(ClassSessionStatus.Scheduled); Assert.Empty(await Stats()); }
    [Fact] public async Task ActualSubstituteReceivesStatistics()
    { await Session(coach: substitute.Id); Assert.Equal(substitute.Id, Assert.Single(await Stats()).CoachEmployeeId); }
    [Fact] public async Task AttendanceAverageUsesActualUniqueCheckIns()
    {
        var first = await Session(); var second = await Session(); await classes.AttendAsync(first, member.Id); await classes.AttendAsync(second, member.Id);
        var another = new Member { ClientCode = 2, MemberCode = "M-2", FullName = "Second", PrimaryPhone = "second", HomeBranch = branch, CreatedByUser = actor };
        db.Add(another); await db.SaveChangesAsync(); await classes.AttendAsync(second, another.Id);
        var row = Assert.Single(await Stats()); Assert.Equal(3, row.TotalAttendances); Assert.Equal(1.5m, row.AverageAttendance);
        await Assert.ThrowsAsync<BusinessRuleException>(() => classes.AttendAsync(first, member.Id));
    }
    [Fact] public async Task ClassStatisticsNeverCreateFinancialPayout()
    { await Session(); await Stats(); Assert.Empty(await db.EmployeeCommissionSummaries.ToListAsync()); Assert.Empty(await db.Payrolls.ToListAsync()); }
    [Fact] public async Task IneligibleMemberCannotPurchaseFreelancePlan()
    { var plan = await Plan(false); await Assert.ThrowsAsync<BusinessRuleException>(() => purchases.PurchaseFreelanceAsync(new(member.Id, plan, branch.Id))); Assert.Empty(await db.SaleTransactions.ToListAsync()); }
    [Fact] public async Task EligibleMemberCanPurchaseAndSnapshotCoach()
    {
        var plan = await Plan(); var purchase = await purchases.PurchaseFreelanceAsync(new(member.Id, plan, branch.Id));
        var batch = await db.MembershipSubscriptionTransactions.FindAsync(purchase.TransactionId);
        Assert.Equal(employee.Id, batch!.FreelanceCoachEmployeeIdSnapshot); Assert.Equal(300, purchase.GymAmountDue);
    }
    [Fact] public async Task FreelanceRenewalCannotConvertEmployeeEntitlementIntoPurchase()
    {
        var employeePlan = new MembershipPlan { Name = "Staff access", CityId = branch.CityId,
            PlanCategory = PlanCategory.Employee, AccessMode = AccessMode.Unlimited };
        var entitlement = new MemberSubscription { EmployeeId = employee.Id, MemberId = member.Id,
            MembershipPlan = employeePlan, CityId = branch.CityId, PurchasedAtBranchId = branch.Id,
            StartDate = access.Today, Status = SubscriptionStatus.Active };
        db.Add(entitlement); await db.SaveChangesAsync();
        var freelancePlan = await Plan();
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            purchases.PurchaseFreelanceAsync(new(member.Id, freelancePlan, branch.Id, entitlement.Id)));
        Assert.Empty(await db.SaleTransactions.ToListAsync()); Assert.Null(entitlement.EndDate);
        Assert.Equal(employeePlan.Id, entitlement.MembershipPlanId);
    }
    [Fact] public async Task FreelancePlanRequiresActiveFreelanceCoach()
    { await Assert.ThrowsAsync<BusinessRuleException>(() => plans.SaveAsync(null, new(branch.CityId, substitute.Id, "Invalid", 300, 30, AccessMode.Unlimited, null, BranchId: branch.Id))); }
    [Fact] public async Task FreelancePurchaseHasNoSalesCommissionAndPreservesOldOwnership()
    {
        var old = new MemberSalesAssignment { Member = member, SalesEmployee = substitute, AssignmentType = AssignmentType.Sales,
            AssignedByUser = actor, AssignedAt = clock.GetUtcNow().AddDays(-3) }; db.Add(old); await db.SaveChangesAsync();
        var purchase = await purchases.PurchaseFreelanceAsync(new(member.Id, await Plan(), branch.Id));
        var sale = await db.SaleTransactions.FindAsync(purchase.SaleTransactionId);
        Assert.Null(sale!.SalesEmployeeId); Assert.Null(sale.SaleCategory); Assert.Equal(AssignmentType.House, sale.SalesAttributionSnapshot);
        Assert.NotNull(old.EndedAt); Assert.Equal(substitute.Id, old.SalesEmployeeId);
        Assert.Equal(AssignmentType.House, (await db.MemberSalesAssignments.SingleAsync(x => x.EndedAt == null)).AssignmentType);
        Assert.Empty(await db.EmployeeCommissionSummaries.ToListAsync());
    }
    [Fact] public async Task FreelanceReportShowsActualGymMoneyAndHouse()
    {
        var plan = await Plan(); var purchase = await purchases.PurchaseFreelanceAsync(new(member.Id, plan, branch.Id)); await Pay(purchase, 200, true);
        var row = Assert.Single(await reports.FreelanceAsync(plan, branch.Id, period.StartAt, clock.GetUtcNow().AddSeconds(1)));
        Assert.Equal("House", row.SalesAttribution); Assert.Equal(200, row.ActualGymRevenue); Assert.Equal(1, row.PurchasedMemberCount);
    }
    [Fact] public async Task DuplicateEligibilityPreventedByDatabase()
    {
        var plan = await Plan(); db.Add(new MembershipPlanEligibleMember { MembershipPlanId = plan, MemberId = member.Id, CreatedByUserId = actor.Id });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task RemovingEligibilityPreventsFuturePurchaseAndPreservesHistory()
    {
        var plan = await Plan(); var old = await purchases.PurchaseFreelanceAsync(new(member.Id, plan, branch.Id));
        await plans.SetEligibilityAsync(plan, member.Id, false);
        await Assert.ThrowsAsync<BusinessRuleException>(() => purchases.PurchaseFreelanceAsync(new(member.Id, plan, branch.Id)));
        Assert.NotNull(await db.MembershipSubscriptionTransactions.FindAsync(old.TransactionId)); Assert.NotNull(await db.SaleTransactions.FindAsync(old.SaleTransactionId));
    }
    [Fact] public async Task MemberMayBeEligibleForMoreThanOneFreelanceCoach()
    {
        await Plan(); substitute.EmploymentType = EmploymentType.Freelance; await db.SaveChangesAsync();
        var other = await plans.SaveAsync(null, new(branch.CityId, substitute.Id, "Other Coach", 350, 30, AccessMode.Unlimited, null, BranchId: branch.Id));
        await plans.SetEligibilityAsync(other, member.Id, true); Assert.Equal(2, await db.MembershipPlanEligibleMembers.CountAsync());
    }
    [Fact] public async Task ActivityBatchAttributionCannotBeRewritten()
    {
        var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, await Coach()));
        var batch = await db.ActivitySubscriptionTransactions.FindAsync(purchase.TransactionId); batch!.CoachPercentageSnapshot = 70;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task BranchScopedReportRejectsOtherBranch()
    { await Assert.ThrowsAsync<UnauthorizedAccessException>(() => reports.ClassesAsync(Guid.NewGuid(), period.StartAt, clock.GetUtcNow())); }


    [Fact] public async Task RenewalCanSelectAnotherCoachWithoutMovingOldPaidRevenue()
    {
        var firstCoach = await Coach(); var first = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, firstCoach));
        await Pay(first, 500); var secondCoach = await Coach(true, 25);
        var second = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, secondCoach, first.SubscriptionId));
        await Pay(second, 200); var rows = await reports.ActivitiesAsync(period.Id);
        Assert.Equal(150, rows.Single(x => x.CoachId == firstCoach).Entitlement);
        Assert.Equal(50, rows.Single(x => x.CoachId == secondCoach).Entitlement);
        Assert.Equal(firstCoach, (await db.ActivitySubscriptionTransactions.FindAsync(first.TransactionId))!.ActivityCoachIdSnapshot);
    }
    [Fact] public async Task ReopenedPeriodDoesNotSilentlyReplaceActivitySnapshot()
    {
        var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, await Coach())); var payment = await Pay(purchase, 500);
        period.Status = CommissionPeriodStatus.Closed; period.EndAt = clock.GetUtcNow(); await db.SaveChangesAsync();
        await reports.FinalizeActivitiesAsync(period.Id); period.Status = CommissionPeriodStatus.Open;
        payment.Amount = 200; payment.Parts.Single().Amount = 200; await db.SaveChangesAsync();
        Assert.Equal(150, Assert.Single(await reports.ActivitiesAsync(period.Id)).Entitlement);
    }
    [Fact] public async Task PlanReassignmentPreservesHistoricalPurchaseCoachAndPrice()
    {
        var plan = await Plan(); var purchase = await purchases.PurchaseFreelanceAsync(new(member.Id, plan, branch.Id)); await Pay(purchase, 300);
        substitute.EmploymentType = EmploymentType.Freelance; await db.SaveChangesAsync();
        await plans.SaveAsync(plan, new(branch.CityId, substitute.Id, "Edited plan", 350, 30, AccessMode.Unlimited, null, BranchId: branch.Id));
        var batch = await db.MembershipSubscriptionTransactions.FindAsync(purchase.TransactionId);
        Assert.Equal(employee.Id, batch!.FreelanceCoachEmployeeIdSnapshot); Assert.Equal(300, batch.PlanPriceSnapshot);
        var rows = await reports.FreelanceAsync(plan, branch.Id, period.StartAt, clock.GetUtcNow().AddSeconds(1));
        Assert.Equal(300, rows.Single(x => x.CoachEmployeeId == employee.Id).ActualGymRevenue);
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "FreelancePlan.Edited");
    }
    [Fact] public async Task AmbiguousActivityRefundIsRejectedInsteadOfGuessed()
    {
        var purchase = await purchases.PurchaseActivityAsync(new(member.Id, package.Id, await Coach())); await Pay(purchase, 500);
        db.Add(new Refund { SaleTransactionId = purchase.SaleTransactionId, Branch = branch, ProcessedByUser = actor,
            Amount = 100, Reason = "Legacy refund", CreatedAt = clock.GetUtcNow().AddMinutes(-1) }); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(() => reports.ActivitiesAsync(period.Id));
    }
    [Fact] public async Task DuplicateActualSessionAttendanceIsRejectedByDatabase()
    {
        var session = await Session(); await classes.AttendAsync(session, member.Id);
        db.Add(new CheckIn { ClassSessionId = session, GroupClassId = group.Id, MemberId = member.Id, BranchId = branch.Id,
            CoachEmployeeId = employee.Id, CheckInType = CheckInType.Class, CreatedByUserId = actor.Id, CheckedInAt = clock.GetUtcNow() });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    private sealed class FixedClock : TimeProvider
    { public override DateTimeOffset GetUtcNow() => new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero); }
    private sealed class TestCurrentUser(Guid userId, Guid branchId) : ICurrentUserService
    {
        public bool IsAuthenticated => true;
        public Guid? UserId => userId;
        public Task<CurrentUserContext?> GetAsync(CancellationToken cancellationToken = default) => Task.FromResult<CurrentUserContext?>(
            new(userId, "test", null, null, [], DevelopmentAuthSeeder.PermissionCodes.Where(code => code != "Branches.AccessAll").ToArray(), new(branchId, "Test Branch")));
    }
}
