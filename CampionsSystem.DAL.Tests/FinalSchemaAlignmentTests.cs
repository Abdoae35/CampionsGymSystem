using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Tests;

public sealed class FinalSchemaAlignmentTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private ApplicationUser user = null!;
    private GymBranch branch = null!;
    private Employee employee = null!;
    private Employee replacement = null!;
    private readonly DateTimeOffset opened = new(2026, 10, 30, 8, 0, 0, TimeSpan.Zero);
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        user = new ApplicationUser { UserName = "alignment-test" };
        branch = new GymBranch { Name = "Alignment Branch", City = new City { Name = "Alignment City" } };
        employee = new Employee { FullName = "Original", Phone = "align-original", HireDate = new(2026, 1, 1) };
        replacement = new Employee { FullName = "Replacement", Phone = "align-replacement", HireDate = new(2026, 1, 1) };
        db.AddRange(user, branch, employee, replacement); await db.SaveChangesAsync();
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private BusinessDay Day(DateOnly? date = null) => new() { BranchId = branch.Id, BusinessDate = date ?? new(2026, 10, 30),
        OpenedAt = opened, OpenedByUserId = user.Id, CreatedAt = opened, Status = BusinessDayStatus.Open };
    private Permission Permission() => new() { Code = "Alignment.Test", Name = "Test", Module = "Alignment" };
    private EmployeeLeaveRequest Leave() => new() { EmployeeId = employee.Id, FromDate = new(2026, 11, 1),
        ToDate = new(2026, 11, 3), Status = EmployeeLeaveRequestStatus.Pending, CreatedAt = opened };

    [Theory] [InlineData(PermissionOverrideEffect.Allow)] [InlineData(PermissionOverrideEffect.Deny)]
    public async Task DirectOverridePersistsWithNullableCreator(PermissionOverrideEffect effect)
    {
        db.Add(new UserPermissionOverride { User = user, Permission = Permission(), Effect = effect }); await db.SaveChangesAsync();
        var row = await db.UserPermissionOverrides.AsNoTracking().SingleAsync(); Assert.Equal(effect, row.Effect); Assert.Null(row.CreatedByUserId);
    }
    [Fact] public async Task OverridePairIsUniqueInDatabase()
    {
        var permission = Permission(); db.Add(new UserPermissionOverride { User = user, Permission = permission, Effect = PermissionOverrideEffect.Allow });
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        db.Add(new UserPermissionOverride { UserId = user.Id, PermissionId = permission.Id, Effect = PermissionOverrideEffect.Deny });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task InvalidOverrideEffectRejected()
    {
        db.Add(new UserPermissionOverride { User = user, Permission = Permission(), Effect = (PermissionOverrideEffect)99 });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task InheritedRoleGrantAndDirectDenyCanCoexistAsFacts()
    {
        var permission = Permission(); var role = new IdentityRole<Guid>("AlignmentRole") { Id = Guid.NewGuid() };
        db.AddRange(role, new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id }, new RolePermission { Role = role, Permission = permission },
            new UserPermissionOverride { User = user, Permission = permission, Effect = PermissionOverrideEffect.Deny, CreatedByUser = user });
        await db.SaveChangesAsync(); Assert.Equal(PermissionOverrideEffect.Deny, (await db.UserPermissionOverrides.SingleAsync()).Effect);
        Assert.Single(await db.RolePermissions.ToListAsync()); // Resolution is deliberately not implemented here.
    }
    [Fact] public async Task OnlyOneOpenDayPerBranch()
    {
        db.Add(Day()); await db.SaveChangesAsync(); db.Add(Day(new(2026, 10, 31)));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task BranchBusinessDateUniqueEvenWhenClosed()
    {
        var day = Day(); day.Status = BusinessDayStatus.Closed; day.ClosedAt = opened.AddHours(20); db.Add(day); await db.SaveChangesAsync();
        db.Add(Day()); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task DifferentBranchesCanHaveOpenDayOnSameDate()
    {
        db.Add(Day()); db.Add(new BusinessDay { Branch = new GymBranch { Name = "Other", CityId = branch.CityId },
            BusinessDate = new(2026, 10, 30), OpenedAt = opened }); await db.SaveChangesAsync(); Assert.Equal(2, await db.BusinessDays.CountAsync());
    }
    [Fact] public async Task ClosedDayAllowsNextOpenDay()
    {
        var day = Day(); day.Status = BusinessDayStatus.Closed; day.ClosedAt = opened.AddHours(20); db.Add(day); db.Add(Day(new(2026, 10, 31)));
        await db.SaveChangesAsync(); Assert.Equal(2, await db.BusinessDays.CountAsync());
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task InvalidBusinessDayLifecycleRejected(bool reversed)
    {
        var day = Day(); day.Status = BusinessDayStatus.Closed; day.ClosedAt = reversed ? opened.AddMinutes(-1) : null;
        db.Add(day); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task AfterMidnightPurchaseKeepsPriorOperationalDateAndActualTimestamp()
    {
        var day = Day(); var actual = new DateTimeOffset(2026, 10, 31, 1, 30, 0, TimeSpan.Zero);
        var member = new Member { ClientCode = 1, MemberCode = "ALIGN", FullName = "Member", PrimaryPhone = "align-member", HomeBranch = branch, CreatedByUser = user };
        var plan = new MembershipPlan { Name = "Normal", CityId = branch.CityId, Price = 500, DurationDays = 30 };
        var sale = new SaleTransaction { Member = member, Branch = branch, CreatedByUser = user, BusinessDay = day,
            SaleType = SaleType.GymMembership, GrossAmount = 500, NetAmount = 500, CreatedAt = actual };
        var subscription = new MemberSubscription { Member = member, MembershipPlan = plan, CityId = branch.CityId,
            PurchasedAtBranch = branch, StartDate = day.BusinessDate, EndDate = day.BusinessDate.AddDays(30) };
        db.Add(new MembershipSubscriptionTransaction { MemberSubscription = subscription, SaleTransaction = sale, MembershipPlan = plan,
            PurchasedDurationDays = 30, PlanPriceSnapshot = 500, NetPriceSnapshot = 500, NewEndDate = subscription.EndDate!.Value, CreatedAt = actual });
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        var batch = await db.MembershipSubscriptionTransactions.Include(x => x.SaleTransaction).ThenInclude(x => x.BusinessDay).SingleAsync();
        Assert.Equal(new DateOnly(2026, 10, 30), batch.SaleTransaction.BusinessDay!.BusinessDate);
        Assert.Equal(actual, batch.CreatedAt); Assert.Equal(actual, batch.SaleTransaction.CreatedAt);
    }
    [Fact] public async Task AudienceIsDynamicAndIndependentOfNormalFreelanceCategory()
    {
        var category = new MembershipPlanCategory { Name = "New audience chosen by management" };
        db.Add(new MembershipPlan { CityId = branch.CityId, Name = "Normal", DurationDays = 30,
            MembershipPlanCategory = category, PlanCategory = PlanCategory.Normal }); await db.SaveChangesAsync();
        var plan = await db.MembershipPlans.Include(x => x.MembershipPlanCategory).SingleAsync();
        Assert.Equal(PlanCategory.Normal, plan.PlanCategory); Assert.Equal(category.Name, plan.MembershipPlanCategory!.Name);
    }
    [Fact] public async Task LegacyPlanMayRemainUnclassified()
    { db.Add(new MembershipPlan { CityId = branch.CityId, Name = "Legacy", DurationDays = 30 }); await db.SaveChangesAsync(); Assert.Null((await db.MembershipPlans.SingleAsync()).MembershipPlanCategoryId); }
    [Fact] public async Task DuplicateAudienceNameRejected()
    {
        db.Add(new MembershipPlanCategory { Name = "Kids" }); await db.SaveChangesAsync(); db.Add(new MembershipPlanCategory { Name = "Kids" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Theory] [InlineData(EmployeeLeaveRequestStatus.Pending)] [InlineData(EmployeeLeaveRequestStatus.Approved)] [InlineData(EmployeeLeaveRequestStatus.Rejected)]
    public async Task LeaveRequestAndReviewPersist(EmployeeLeaveRequestStatus status)
    {
        var leave = Leave(); leave.Status = status;
        if (status != EmployeeLeaveRequestStatus.Pending) { leave.ReviewedByUser = user; leave.ReviewedAt = opened.AddHours(1); leave.ManagerNotes = "Reviewed"; }
        db.Add(leave); await db.SaveChangesAsync(); Assert.Equal(status, (await db.EmployeeLeaveRequests.SingleAsync()).Status);
    }
    [Fact] public async Task ReversedLeaveDatesRejected()
    { var leave = Leave(); leave.ToDate = leave.FromDate.AddDays(-1); db.Add(leave); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync()); }
    [Fact] public async Task ReviewedLeaveRequiresReviewTime()
    { var leave = Leave(); leave.Status = EmployeeLeaveRequestStatus.Approved; db.Add(leave); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync()); }
    [Fact] public async Task ShiftReplacementPreservesOriginalAssignmentAndRepeatedChanges()
    {
        var shift = new Shift { Branch = branch, OpenedByUser = user, OpenedAt = opened };
        db.Add(new ShiftEmployee { Shift = shift, Employee = employee, JoinedAt = opened, LeftAt = opened.AddHours(2) });
        db.Add(new ShiftEmployee { Shift = shift, Employee = replacement, JoinedAt = opened.AddHours(2) });
        db.Add(new ShiftEmployeeChange { Shift = shift, OriginalEmployee = employee, ReplacementEmployee = replacement,
            ChangedByUser = user, ChangedAt = opened.AddHours(2), Reason = "Cover leave" });
        db.Add(new ShiftEmployeeChange { Shift = shift, OriginalEmployee = replacement, ReplacementEmployee = employee,
            ChangedByUser = user, ChangedAt = opened.AddHours(4), Reason = "Returned" });
        await db.SaveChangesAsync(); Assert.Equal(2, await db.ShiftEmployeeChanges.CountAsync());
        Assert.Equal(opened, (await db.ShiftEmployees.SingleAsync(x => x.EmployeeId == employee.Id)).JoinedAt);
    }
    [Fact] public async Task ReplacementMustBeDifferentEmployee()
    {
        db.Add(new ShiftEmployeeChange { Shift = new Shift { Branch = branch, OpenedByUser = user, OpenedAt = opened },
            OriginalEmployeeId = employee.Id, ReplacementEmployeeId = employee.Id, ChangedByUserId = user.Id });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task ReferencedBusinessDayCannotBeDeleted()
    {
        var day = Day(); db.Add(new Shift { Branch = branch, OpenedByUser = user, OpenedAt = opened, BusinessDay = day });
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        db.Remove(await db.BusinessDays.SingleAsync()); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public void ModelHasOnlyRestrictedNewFksNoShadowPropertiesAndNullableTransitions()
    {
        foreach (var type in new[] { typeof(UserPermissionOverride), typeof(BusinessDay), typeof(MembershipPlanCategory),
            typeof(EmployeeLeaveRequest), typeof(ShiftEmployeeChange) })
            Assert.All(db.Model.FindEntityType(type)!.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        foreach (var type in new[] { typeof(Shift), typeof(SaleTransaction), typeof(Payment), typeof(Refund), typeof(Expense),
            typeof(CheckIn), typeof(FinancialTransaction), typeof(ClassSession) })
        {
            var entity = db.Model.FindEntityType(type)!; Assert.True(entity.FindProperty("BusinessDayId")!.IsNullable);
            Assert.Equal(DeleteBehavior.Restrict, entity.GetForeignKeys().Single(x => x.PrincipalEntityType.ClrType == typeof(BusinessDay)).DeleteBehavior);
        }
        Assert.DoesNotContain(db.Model.GetEntityTypes().SelectMany(x => x.GetProperties()), x => x.IsShadowProperty());
        Assert.Null(db.Model.FindEntityType(typeof(MembershipSubscriptionTransaction))!.FindProperty("BusinessDayId"));
        Assert.Equal("date", db.Model.FindEntityType(typeof(BusinessDay))!.FindProperty("BusinessDate")!.GetColumnType());
    }
}
