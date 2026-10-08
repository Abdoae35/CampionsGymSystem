using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

public sealed class EmployeeGymMembershipTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private GymBranch branch = null!;
    private ApplicationUser user = null!;
    private Employee employee = null!;
    private Member member = null!;
    private MembershipPlan plan = null!;
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        user = new ApplicationUser { UserName = "employee-gym-tests" };
        branch = new GymBranch { Name = "Employee gym", City = new City { Name = "Employee city" } };
        member = new Member { MemberCode = "EMP-GYM", FullName = "Employee member", PrimaryPhone = "employee-gym",
            HomeBranch = branch, CreatedByUser = user };
        employee = new Employee { FullName = "Employee", Phone = "employee", HireDate = new(2026,1,1) };
        plan = new MembershipPlan { Name = "Employee gym", City = branch.City, PlanCategory = PlanCategory.Employee,
            AccessMode = AccessMode.Unlimited, Price = 0, DurationDays = 0 };
        db.AddRange(employee, member, plan); await db.SaveChangesAsync();
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private MemberSubscription Period() => new() { EmployeeId = employee.Id, MemberId = member.Id, MembershipPlanId = plan.Id,
        CityId = branch.CityId, PurchasedAtBranchId = branch.Id, StartDate = new(2026,1,1),
        AccessMode = AccessMode.Unlimited, Status = SubscriptionStatus.Active };
    [Fact] public async Task EmployeeMayExistWithoutMemberThenLinkOneProfile()
    {
        Assert.Null((await db.Employees.SingleAsync()).MemberId);
        employee.MemberId = member.Id; await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        Assert.Equal(member.Id, (await db.Employees.Include(x => x.Member).SingleAsync()).Member!.Id);
    }
    [Fact] public async Task SameMemberCannotLinkMultipleEmployees()
    {
        employee.MemberId = member.Id; await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        db.Add(new Employee { MemberId = member.Id, FullName = "Other", Phone = "other", HireDate = employee.HireDate });
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
    }
    [Fact] public async Task UnlimitedOpenPeriodNeedsNoPurchaseOrFinancialRecordAndUsesNormalCheckIn()
    {
        employee.MemberId = member.Id; var period = Period(); db.Add(period); await db.SaveChangesAsync();
        db.Add(new CheckIn { MemberId = member.Id, BranchId = branch.Id, MemberSubscriptionId = period.Id,
            CheckInType = CheckInType.Gym, CheckInMethod = CheckInMethod.Scanner, CreatedByUserId = user.Id,
            CheckedInAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        var saved = await db.MemberSubscriptions.SingleAsync();
        Assert.Null(saved.EndDate); Assert.Null(saved.TotalSessions); Assert.Null(saved.RemainingSessions);
        Assert.Equal(employee.Id, saved.EmployeeId);
        Assert.Empty(await db.MembershipSubscriptionTransactions.ToListAsync());
        Assert.Empty(await db.SaleTransactions.ToListAsync()); Assert.Empty(await db.Payments.ToListAsync());
        Assert.Empty(await db.PaymentParts.ToListAsync()); Assert.Empty(await db.FinancialTransactions.ToListAsync());
        Assert.False((await db.CheckIns.SingleAsync()).SessionConsumed);
    }
    [Theory] [InlineData("Employee")] [InlineData("Member")]
    public async Task OnlyOneOpenPeriodPerEmployeeAndMember(string duplicate)
    {
        db.Add(Period()); await db.SaveChangesAsync(); var second = Period();
        if (duplicate == "Member")
        {
            var other = new Employee { FullName = "Other", Phone = "other", HireDate = employee.HireDate };
            db.Add(other); await db.SaveChangesAsync(); second.EmployeeId = other.Id;
        }
        else
        {
            var other = new Member { MemberCode = "OTHER-EMP", FullName = "Other member", PrimaryPhone = "other-member",
                HomeBranchId = branch.Id, CreatedByUserId = user.Id };
            db.Add(other); await db.SaveChangesAsync(); second.MemberId = other.Id;
        }
        db.Add(second); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task LeavingClosesOldPeriodAndRehireCreatesIndependentHistory()
    {
        var old = Period(); db.Add(old); await db.SaveChangesAsync();
        employee.EmploymentStatus = EmploymentStatus.Resigned; employee.TerminationDate = new(2026,6,1);
        old.EndDate = new(2026,6,1); old.Status = SubscriptionStatus.Expired; await db.SaveChangesAsync();
        employee.EmploymentStatus = EmploymentStatus.Active; employee.TerminationDate = null;
        var next = Period(); next.StartDate = new(2027,3,1); db.Add(next); await db.SaveChangesAsync();
        db.ChangeTracker.Clear(); var history = await db.MemberSubscriptions.OrderBy(x => x.StartDate).ToListAsync();
        Assert.Equal(2, history.Count); Assert.Equal(old.Id, history[0].Id); Assert.Equal(new DateOnly(2026,6,1), history[0].EndDate);
        Assert.Equal(SubscriptionStatus.Expired, history[0].Status); Assert.Null(history[1].EndDate); Assert.NotEqual(old.Id, next.Id);
    }
    [Theory] [InlineData(true)] [InlineData(false)]
    public async Task HistoricalEmployeePeriodCannotBeDeletedOrReopenedThroughEf(bool delete)
    {
        var period = Period(); period.EndDate = new(2026,6,1); period.Status = SubscriptionStatus.Expired;
        db.Add(period); await db.SaveChangesAsync();
        if (delete) db.Remove(period); else { period.EndDate = null; period.Status = SubscriptionStatus.Active; }
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
    [Theory] [InlineData("Price")] [InlineData("Session")] [InlineData("Duration")]
    public async Task InvalidEmployeePlanRejected(string invalid)
    {
        if (invalid == "Price") plan.Price = 1;
        if (invalid == "Session") { plan.AccessMode = AccessMode.SessionBased; plan.SessionCount = 5; }
        if (invalid == "Duration") plan.DurationDays = 30;
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task NormalSubscriptionStillRequiresEndDate()
    {
        var normal = new MembershipPlan { Name = "Normal", CityId = branch.CityId, DurationDays = 30 };
        var period = Period(); period.EmployeeId = null; period.MembershipPlan = normal; db.Add(period);
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Theory] [InlineData("Session")] [InlineData("ClosedWithoutEnd")] [InlineData("ActiveWithEnd")]
    public async Task InvalidEmploymentPeriodRejected(string invalid)
    {
        var period = Period();
        if (invalid == "Session") { period.AccessMode = AccessMode.SessionBased; period.TotalSessions = period.RemainingSessions = 5; }
        if (invalid == "ClosedWithoutEnd") period.Status = SubscriptionStatus.Expired;
        if (invalid == "ActiveWithEnd") period.EndDate = new(2026,6,1);
        db.Add(period); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public void EmployeeLinksAreRestrictedAndPurchaseSnapshotsStayPaidOnly()
    {
        var employeeModel = db.Model.FindEntityType(typeof(Employee))!;
        Assert.True(employeeModel.GetForeignKeys().Single(x => x.Properties.Any(p => p.Name == "MemberId")).IsUnique);
        Assert.Equal(DeleteBehavior.Restrict, db.Model.FindEntityType(typeof(MemberSubscription))!.GetForeignKeys()
            .Single(x => x.Properties.Any(p => p.Name == "EmployeeId")).DeleteBehavior);
        Assert.False(db.Model.FindEntityType(typeof(MembershipSubscriptionTransaction))!.FindProperty("SaleTransactionId")!.IsNullable);
    }
}
