using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

// Each test uses an isolated schema inside a rolled-back PostgreSQL transaction.
// No existing gym database/schema is migrated or changed.
public sealed class PrivateAndFreelanceSchemaTests : IAsyncLifetime
{
    private NpgsqlConnection connection = null!;
    private NpgsqlTransaction transaction = null!;
    private ApplicationDbContext db = null!;
    private readonly DateTimeOffset now = new(2026, 10, 6, 12, 0, 0, TimeSpan.Zero);
    private ApplicationUser user = null!;
    private City city = null!;
    private GymBranch branch = null!;
    private Employee coachA = null!, coachB = null!;
    private Member member = null!;
    private PrivateSubscription subscription = null!;
    private PrivateSubscriptionTransaction batch = null!;
    private PrivateCoachAssignment assignment = null!;
    private CommissionPeriod oldPeriod = null!, openPeriod = null!;
    private CheckIn checkIn = null!;
    private MembershipPlan normalPlan = null!;
    private MemberSubscription gymSubscription = null!;
    private SaleTransaction sale = null!;

    public async Task InitializeAsync()
    {
        var cs = Environment.GetEnvironmentVariable("GYM_SCHEMA_TEST_CONNECTION")
            ?? $"Host=localhost;Database=postgres;Username={Environment.UserName};Timeout=5";
        connection = new NpgsqlConnection(cs);
        await connection.OpenAsync();
        transaction = await connection.BeginTransactionAsync();
        var schema = "gym_rules_test_" + Guid.NewGuid().ToString("N");
        await using (var command = new NpgsqlCommand($"CREATE SCHEMA {schema}; SET LOCAL search_path TO {schema};", connection, transaction))
            await command.ExecuteNonQueryAsync();
        db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
        await db.Database.UseTransactionAsync(transaction);
        // Exercise the actual InitialCreate + new migration, not EnsureCreated.
        var sql = db.GetService<IMigrator>().GenerateScript()
            .Replace("START TRANSACTION;", "").Replace("BEGIN TRANSACTION;", "").Replace("COMMIT;", "");
        await db.Database.ExecuteSqlRawAsync(sql);
        user = new ApplicationUser();
        city = new City { Name = "Test" };
        branch = new GymBranch { City = city, Name = "Test" };
        coachA = new Employee { FullName = "A", Phone = "A", EmploymentType = EmploymentType.Freelance, HireDate = new(2026, 1, 1) };
        coachB = new Employee { FullName = "B", Phone = "B", EmploymentType = EmploymentType.Freelance, HireDate = new(2026, 1, 1) };
        member = new Member { MemberCode = "TEST", FullName = "Test", PrimaryPhone = "TEST", HomeBranch = branch, CreatedByUser = user };
        normalPlan = Plan(PlanCategory.Normal, null, 500m);
        gymSubscription = new MemberSubscription { Member = member, City = city, PurchasedAtBranch = branch, MembershipPlan = normalPlan, StartDate = new(2026, 1, 1), EndDate = new(2026, 12, 31), AccessMode = AccessMode.Unlimited, Status = SubscriptionStatus.Active };
        sale = new SaleTransaction { Member = member, Branch = branch, CreatedByUser = user, SaleType = SaleType.Private, GrossAmount = 2000, NetAmount = 2000 };
        subscription = new PrivateSubscription { Member = member, Branch = branch, StartDate = new(2026, 1, 1), EndDate = new(2026, 12, 31), TotalSessions = 20, RemainingSessions = 20, Status = PrivateSubscriptionStatus.Active };
        batch = new PrivateSubscriptionTransaction { PrivateSubscription = subscription, SaleTransaction = sale, PrivatePackage = new PrivatePackage { Branch = branch, Name = "Private", SessionCount = 20, DurationDays = 30, Price = 2000 }, SessionsAdded = 20, RemainingSessions = 20, DurationDaysAdded = 30, NetAmount = 2000, UnitSessionValue = 100, NewEndDate = subscription.EndDate };
        assignment = Assignment(coachA);
        oldPeriod = new CommissionPeriod { Branch = branch, Name = "Closed", StartAt = now.AddMonths(-1), EndAt = now.AddDays(-1), Status = CommissionPeriodStatus.Closed, ClosedAt = now.AddDays(-1) };
        openPeriod = new CommissionPeriod { Branch = branch, Name = "Open", StartAt = now, Status = CommissionPeriodStatus.Open };
        checkIn = new CheckIn { Member = member, Branch = branch, MemberSubscription = gymSubscription, CheckInType = CheckInType.Gym, CheckedInAt = now, CreatedByUser = user };
        db.AddRange(batch, assignment, oldPeriod, openPeriod, checkIn, coachB);
        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        if (transaction is not null) await transaction.RollbackAsync();
        if (db is not null) await db.DisposeAsync();
        if (transaction is not null) await transaction.DisposeAsync();
        if (connection is not null) await connection.DisposeAsync();
    }

    private MembershipPlan Plan(PlanCategory category, Employee? coach, decimal price) =>
        new() { City = city, Name = "Plan", PlanCategory = category, FreelanceCoachEmployee = coach, Price = price, AccessMode = AccessMode.Unlimited, DurationDays = 30 };
    private PrivateCoachAssignment Assignment(Employee coach) =>
        new() { PrivateSubscription = subscription, CoachEmployee = coach, AssignedAt = now, CreatedByUser = user };
    private PrivateSession Session(Employee coach, CommissionPeriod period) =>
        new() { PrivateSubscription = subscription, PrivateSubscriptionTransaction = batch, CoachEmployee = coach, GymCheckIn = checkIn, CommissionPeriod = period, SessionAt = period.StartAt.AddHours(1), CreatedByUser = user };
    private PrivateExpirySettlement Settlement(Employee coach) =>
        new() { PrivateSubscription = subscription, PrivateSubscriptionTransaction = batch, CoachEmployee = coach, CommissionPeriod = openPeriod, RemainingSessions = 4, UnitSessionValue = 100, EligibleAmount = 400, SettledAt = now };
    private async Task Transfer()
    {
        assignment.EndedAt = now;
        await db.SaveChangesAsync(); // Close before insert; caller's transaction is atomic.
        db.Add(Assignment(coachB));
        await db.SaveChangesAsync();
    }
    private async Task ConstraintFailure(string code)
    {
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(code, Assert.IsType<PostgresException>(error.InnerException).SqlState);
    }

    [Fact] public async Task NormalPlanAllowsNullCoach()
    {
        Assert.Null((await db.MembershipPlans.SingleAsync()).FreelanceCoachEmployeeId);
    }
    [Fact] public async Task FreelancePlanRequiresCoach()
    {
        db.Add(Plan(PlanCategory.Freelance, null, 300));
        await ConstraintFailure(PostgresErrorCodes.CheckViolation);
    }
    [Fact] public async Task NormalPlanRejectsCoach()
    {
        db.Add(Plan(PlanCategory.Normal, coachA, 300));
        await ConstraintFailure(PostgresErrorCodes.CheckViolation);
    }
    [Fact] public async Task CoachesHaveIndependentPlanPrices()
    {
        db.AddRange(Plan(PlanCategory.Freelance, coachA, 300), Plan(PlanCategory.Freelance, coachB, 350));
        await db.SaveChangesAsync();
        Assert.Equal(new[] { 300m, 350m }, await db.MembershipPlans.Where(x => x.PlanCategory == PlanCategory.Freelance).OrderBy(x => x.Price).Select(x => x.Price).ToArrayAsync());
    }
    [Fact] public async Task PurchaseSnapshotSurvivesPlanEdit()
    {
        var plan = Plan(PlanCategory.Freelance, coachA, 300);
        var purchase = new MembershipSubscriptionTransaction { MemberSubscription = gymSubscription, SaleTransaction = sale, MembershipPlan = plan, PlanCategorySnapshot = PlanCategory.Freelance, FreelanceCoachEmployeeIdSnapshot = coachA.Id, PlanPriceSnapshot = 300, NetPriceSnapshot = 300, PurchasedDurationDays = 30, NewEndDate = new(2026, 12, 31) };
        db.Add(purchase);
        await db.SaveChangesAsync();
        plan.FreelanceCoachEmployee = coachB;
        plan.Price = 350;
        plan.IsActive = false;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var historical = await db.MembershipSubscriptionTransactions.SingleAsync();
        Assert.Equal(coachA.Id, historical.FreelanceCoachEmployeeIdSnapshot);
        Assert.Equal(PlanCategory.Freelance, historical.PlanCategorySnapshot);
        Assert.Equal(300, historical.PlanPriceSnapshot);
    }
    [Fact] public async Task OnlyOneCurrentAssignment()
    {
        db.Add(Assignment(coachB));
        await ConstraintFailure(PostgresErrorCodes.UniqueViolation);
    }
    [Fact] public async Task TransferAllowedAfterPeriodClose()
    {
        await Transfer();
        Assert.Equal(coachB.Id, (await db.PrivateCoachAssignments.SingleAsync(x => x.EndedAt == null)).CoachEmployeeId);
        Assert.Equal(2, await db.PrivateCoachAssignments.CountAsync());
    }
    [Fact] public async Task OldSessionsRetainDeliveringCoachAfterTransfer()
    {
        db.Add(Session(coachA, oldPeriod));
        await db.SaveChangesAsync();
        await Transfer();
        db.ChangeTracker.Clear();
        Assert.Equal(coachA.Id, (await db.PrivateSessions.SingleAsync()).CoachEmployeeId);
    }
    [Fact] public async Task NewSessionsUseNewCoach()
    {
        await Transfer();
        db.Add(Session(coachB, openPeriod));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var session = await db.PrivateSessions.SingleAsync();
        Assert.Equal(coachB.Id, session.CoachEmployeeId);
        Assert.Equal(openPeriod.Id, session.CommissionPeriodId);
    }
    [Fact] public async Task SessionPreservesPurchasedBatchAndPrice()
    {
        db.Add(Session(coachA, oldPeriod));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var session = await db.PrivateSessions.Include(x => x.PrivateSubscriptionTransaction).SingleAsync();
        Assert.Equal(batch.Id, session.PrivateSubscriptionTransactionId);
        Assert.Equal(100, session.PrivateSubscriptionTransaction.UnitSessionValue);
    }
    [Fact] public async Task ExpiryCannotSettleBatchTwice()
    {
        db.Add(Settlement(coachA));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.Add(new PrivateExpirySettlement { PrivateSubscriptionId = subscription.Id, PrivateSubscriptionTransactionId = batch.Id, CoachEmployeeId = coachB.Id, CommissionPeriodId = openPeriod.Id, RemainingSessions = 4, UnitSessionValue = 100, EligibleAmount = 400, SettledAt = now });
        await ConstraintFailure(PostgresErrorCodes.UniqueViolation);
    }
    [Fact] public async Task ExpiryRecordsCurrentCoachAndOpenPeriod()
    {
        await Transfer();
        var current = await db.PrivateCoachAssignments.Include(x => x.CoachEmployee).SingleAsync(x => x.EndedAt == null);
        db.Add(Settlement(current.CoachEmployee));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var settlement = await db.PrivateExpirySettlements.SingleAsync();
        Assert.Equal(coachB.Id, settlement.CoachEmployeeId);
        Assert.Equal(openPeriod.Id, settlement.CommissionPeriodId);
        Assert.Equal(400, settlement.EligibleAmount);
    }
    [Fact] public async Task TransferDoesNotModifyClosedPeriodOrItsSessions()
    {
        db.Add(Session(coachA, oldPeriod));
        await db.SaveChangesAsync();
        var closedAt = oldPeriod.ClosedAt;
        await Transfer();
        db.ChangeTracker.Clear();
        var period = await db.CommissionPeriods.SingleAsync(x => x.Id == oldPeriod.Id);
        Assert.Equal(CommissionPeriodStatus.Closed, period.Status);
        Assert.Equal(closedAt, period.ClosedAt);
        Assert.Equal(oldPeriod.Id, (await db.PrivateSessions.SingleAsync()).CommissionPeriodId);
    }
    [Fact] public async Task ExpiryRejectsIncorrectEligibleAmount()
    {
        var settlement = Settlement(coachA);
        settlement.EligibleAmount = 401;
        db.Add(settlement);
        await ConstraintFailure(PostgresErrorCodes.CheckViolation);
    }
    [Fact] public async Task DeliveredCoachCannotBeDeleted()
    {
        db.Add(Session(coachA, oldPeriod));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.Remove(new Employee { Id = coachA.Id });
        await ConstraintFailure(PostgresErrorCodes.ForeignKeyViolation);
    }
    [Fact] public async Task BatchPriceIsImmutableButRemainingCounterCanChange()
    {
        batch.RemainingSessions--;
        await db.SaveChangesAsync();
        batch.UnitSessionValue = 120;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
    private string Script(string from, string to) => db.GetService<IMigrator>().GenerateScript(from, to)
        .Replace("START TRANSACTION;", "").Replace("BEGIN TRANSACTION;", "").Replace("COMMIT;", "");

    [Fact] public async Task MigrationBackfillsUnusedLegacyBatchWithoutLosingAssignmentData()
    {
        await db.Database.ExecuteSqlRawAsync("UPDATE \"PrivateCoachAssignments\" SET \"SessionsAllocated\" = 20, \"CommissionBasisAmount\" = 2000");
        await db.Database.ExecuteSqlRawAsync(Script("UpdatePrivateAndFreelanceBusinessRules", "InitialCreate"));
        await db.Database.ExecuteSqlRawAsync(Script("InitialCreate", "UpdatePrivateAndFreelanceBusinessRules"));
        db.ChangeTracker.Clear();
        Assert.Equal(20, (await db.PrivateSubscriptionTransactions.SingleAsync()).RemainingSessions);
        Assert.Equal(PlanCategory.Normal, (await db.MembershipPlans.SingleAsync()).PlanCategory);
        Assert.Equal(2000m, await db.PrivateCoachAssignments.Select(x => EF.Property<decimal>(x, "CommissionBasisAmount")).SingleAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MigrationRejectsUnreconciledHistoryWithoutChangingIt(bool hasLegacySession)
    {
        if (hasLegacySession) db.Add(Session(coachA, oldPeriod));
        else subscription.RemainingSessions = 19;
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync("SAVEPOINT legacy_check");
        await db.Database.ExecuteSqlRawAsync(Script("UpdatePrivateAndFreelanceBusinessRules", "InitialCreate"));
        var error = await Assert.ThrowsAsync<PostgresException>(() =>
            db.Database.ExecuteSqlRawAsync(Script("InitialCreate", "UpdatePrivateAndFreelanceBusinessRules")));
        Assert.Contains("requires explicit batch/period/counter reconciliation", error.MessageText);
        await db.Database.ExecuteSqlRawAsync("ROLLBACK TO SAVEPOINT legacy_check");
        db.ChangeTracker.Clear();
        Assert.Equal(hasLegacySession ? 1 : 0, await db.PrivateSessions.CountAsync());
        Assert.Equal(hasLegacySession ? 20 : 19, (await db.PrivateSubscriptions.SingleAsync()).RemainingSessions);
    }

    [Fact] public void BusinessHistoryHasRestrictedForeignKeysAndNoShadowProperties()
    {
        foreach (var type in new[] { typeof(PrivateSubscriptionTransaction), typeof(PrivateSession), typeof(PrivateCoachAssignment), typeof(PrivateExpirySettlement), typeof(MembershipSubscriptionTransaction), typeof(SaleTransaction), typeof(Payment) })
        {
            var entity = db.Model.FindEntityType(type)!;
            Assert.All(entity.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
            Assert.DoesNotContain(entity.GetProperties(), p => p.IsShadowProperty());
        }
        var unit = db.Model.FindEntityType(typeof(PrivateSubscriptionTransaction))!.FindProperty(nameof(PrivateSubscriptionTransaction.UnitSessionValue))!;
        Assert.Equal(PropertySaveBehavior.Throw, unit.GetAfterSaveBehavior());
    }
}
