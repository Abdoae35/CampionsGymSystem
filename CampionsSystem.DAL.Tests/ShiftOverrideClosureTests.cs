using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Employees;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Subscriptions;

namespace CampionsSystem.DAL.Tests;

public sealed partial class EmployeeShiftTests
{
    private async Task MakeRoleAsync(string name = "BranchManager")
    {
        var role = new IdentityRole<Guid>(name) { Id = Guid.NewGuid(), NormalizedName = name.ToUpperInvariant() };
        db.Add(role);
        db.Add(new IdentityUserRole<Guid> { UserId = b.Id, RoleId = role.Id });
        await db.SaveChangesAsync();
    }

    [Theory]
    [InlineData(EmploymentStatus.Suspended)]
    [InlineData(EmploymentStatus.Resigned)]
    public async Task ManagerClosesInactiveEmployeesShiftWithoutInventingCountOrMoney(EmploymentStatus status)
    {
        var insta = new FinancialAccount { BranchId = branch.Id, Name = "InstaPay", AccountType = FinancialAccountType.InstaPay, OpeningBalance = 250 };
        db.Add(insta); await db.SaveChangesAsync();
        var first = await Service().OpenAsync(new(Guid.NewGuid(), 100000));
        await MakeRoleAsync();
        Actor(b);
        var second = await Service().OpenAsync(new(Guid.NewGuid(), 100000));
        var category = new ExpenseCategory { Name = "Operations" }; db.Add(category); await db.SaveChangesAsync();
        await new ExpenseService(db, user, access).RecordPaidAsync(new(Guid.NewGuid(), category.Id, insta.Id, 25, "InstaPay expense"));
        user.Context = user.Context! with { Permissions = [..user.Context.Permissions, "Employees.Manage"] };
        var employees = new EmployeeManagementService(db, user, access, new EmployeeBranchResolver(db),
            new EmployeeBranchAssignmentService(db, user, access), new SubscriptionService(db, user, access),
            OwnerLifecycleTestServices.Protection(db, user, clock));
        await employees.ChangeStatusAsync(ea.Id, new() { Status = status, Reason = "Employment status changed" });

        var review=new ShiftReportService(db,user,access);
        var preClosure=await review.ShiftAsync(first.Id);
        Assert.Equal(225,preClosure.CurrentSharedAccountBalances.Single(x=>x.Method==FinancialAccountType.InstaPay).BookBalance);
        Assert.Equal(100000,preClosure.CurrentSharedDrawer.ExpectedPhysicalCash);
        Assert.Empty((await review.TransactionsAsync(first.Id)).Items);
        Assert.Equal(25,(await review.TransactionsAsync(second.Id)).Items.Single().Amount);

        var beforeCounts = await db.CashCounts.CountAsync();
        var beforeLedger = await db.FinancialTransactions.CountAsync();
        var request = new OverrideCloseShiftRequest(Guid.NewGuid(), first.Version, "Employee is suspended");
        var closed = await Service().CloseOverrideAsync(first.Id, request);
        Assert.Equal("Override", closed.ClosureType);
        Assert.Equal("Unverified", closed.VerificationStatus);
        Assert.Equal(b.Id, closed.ClosedByUserId);
        Assert.Equal(ea.Id, closed.EmployeeId);
        Assert.Equal(100000, closed.ExpectedClosingCash);
        Assert.Null(closed.ActualClosingCash);
        Assert.Null(closed.Difference);
        Assert.Equal(225, closed.ClosingAccountBalances!.Single(x => x.Method == FinancialAccountType.InstaPay).BookBalance);
        Assert.Equal(100000, closed.ClosingAccountBalances!.Single(x => x.Method == FinancialAccountType.Cash).BookBalance);
        Assert.Equal(beforeCounts, await db.CashCounts.CountAsync());
        Assert.Equal(beforeLedger, await db.FinancialTransactions.CountAsync());
        Assert.Equal(ShiftStatus.Open, (await Service().GetAsync(second.Id)).Status);
        Assert.Equal(first.Id, (await Service().CloseOverrideAsync(first.Id, request)).Id);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().CloseOverrideAsync(first.Id, request with { Reason = "Different" }));
        Assert.Single(await db.AuditLogs.Where(x => x.Action == "Shift.OverrideClosed" && x.EntityId == first.Id).ToListAsync());
    }

    [Fact]
    public async Task OverrideRequiresSeparateCapabilityBranchScopeAndAnotherEmployee()
    {
        var shift = await Service().OpenAsync(new(Guid.NewGuid(), 100000));
        var request = new OverrideCloseShiftRequest(Guid.NewGuid(), shift.Version, "Absent employee");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().CloseOverrideAsync(shift.Id, request));
        Actor(b);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().CloseOverrideAsync(shift.Id, request));
        await MakeRoleAsync();
        user.Context = user.Context! with { Permissions = [ShiftPermissions.Manage, ShiftPermissions.View] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().CloseOverrideAsync(shift.Id, request));
        user.Context = user.Context! with { Permissions = [ShiftPermissions.CloseOthers, ShiftPermissions.View], CurrentBranch = new(other.Id, other.Name) };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().CloseOverrideAsync(shift.Id, request));
        user.Context = user.Context! with { CurrentBranch = new(branch.Id, branch.Name) };
        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().CloseOverrideAsync(shift.Id, request with { Reason = " " }));
        Assert.Equal(ShiftStatus.Open, (await Service().GetAsync(shift.Id)).Status);
    }

    [Fact]
    public async Task UnverifiedOverrideAllowsFinancialMonthClosureAfterLastOpenShiftEnds()
    {
        var month = await Month(today, 0);
        var shift = await Service().OpenAsync(new(Guid.NewGuid(), 100000));
        await MakeRoleAsync();
        Actor(b);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Months().CloseAsync(month.Id, new(Guid.NewGuid(), 0, "Still open")));
        await Service().CloseOverrideAsync(shift.Id, new(Guid.NewGuid(), shift.Version, "Absent employee"));
        var closed = await Months().CloseAsync(month.Id, new(Guid.NewGuid(), 0, "Close after unverified shift"));
        Assert.Equal("Closed", closed.Status);
        Assert.Null((await Service().GetAsync(shift.Id)).ActualClosingCash);
        Assert.Single(await db.CashCounts.ToListAsync()); // only the opening observation
    }

    [Fact]
    public async Task OwnerMembershipCanAuthorizeOverrideWithinBranchScope()
    {
        var shift = await Service().OpenAsync(new(Guid.NewGuid(), 100000));
        await MakeRoleAsync("Owner");
        Actor(b);
        var closed = await Service().CloseOverrideAsync(shift.Id,
            new(Guid.NewGuid(), shift.Version, "Owner closes absent employee shift"));
        Assert.Equal("Override", closed.ClosureType);
        Assert.Equal(b.Id, closed.ClosedByUserId);
    }
}
