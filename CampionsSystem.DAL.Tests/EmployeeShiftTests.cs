using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;

public sealed partial class EmployeeShiftTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!; private ApplicationDbContext db = null!;
    private ApplicationUser a = null!, b = null!; private GymBranch branch = null!, other = null!;
    private Employee ea = null!, eb = null!; private FinancialAccount cash = null!;
    private TestBranchUser user = null!; private OperationAccess access = null!; private Clock clock = new();
    private readonly DateOnly today = new(2026,10,11);
    private sealed class Clock : TimeProvider { public DateTimeOffset Now = new(2026,10,11,10,0,0,TimeSpan.Zero); public override DateTimeOffset GetUtcNow() => Now; }
    private ShiftService Service() => new(db, user, access);
    private void Actor(ApplicationUser actor) => user.Context = user.Context! with { UserId = actor.Id };
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        a = new() { UserName = "shift-a" }; b = new() { UserName = "shift-b" };
        branch = new() { Name = "A", City = new() { Name = "City" } }; other = new() { Name = "B", City = branch.City };
        ea = new() { User = a, FullName = "Ahmed", Phone = "shift-a", HireDate = today.AddYears(-1) };
        eb = new() { User = b, FullName = "Mohamed", Phone = "shift-b", HireDate = ea.HireDate };
        cash = new() { Branch = branch, Name = "Shared drawer", OpeningBalance = 100000 };
        db.AddRange(cash, other, ea, eb);
        db.AddRange(new EmployeeBranchAssignment { Employee = ea, Branch = branch, StartDate = ea.HireDate, AssignedByUser = a }, new EmployeeBranchAssignment { Employee = eb, Branch = branch, StartDate = eb.HireDate, AssignedByUser = a });
        db.Add(new CashDrawer { Branch = branch, FinancialAccount = cash, CreatedByUser = a, CreatedAt = clock.Now, OperationId = Guid.NewGuid(), RequestHash = new('A',64) });
        await db.SaveChangesAsync();
        user = new(new(a.Id, "employee", null, null, [], [..ShiftPermissions.Codes, "Branches.AccessAll"], new(branch.Id,branch.Name)));
        access = new(user,db,clock,Options.Create(new AuthContextOptions()));
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }

    [Fact] public async Task OverlappingEmployeesOwnIndependentShiftsAndObserveOneDrawer()
    {
        var service = Service(); var one = await service.OpenAsync(new(Guid.NewGuid(),100000));
        Actor(b); var two = await service.OpenAsync(new(Guid.NewGuid(),100000));
        Assert.NotEqual(one.Id,two.Id); Assert.Equal(ea.Id,one.EmployeeId); Assert.Equal(eb.Id,two.EmployeeId);
        Assert.Equal(2, await db.Shifts.CountAsync()); Assert.Single(await db.FinancialAccounts.ToListAsync()); Assert.Empty(await db.FinancialTransactions.ToListAsync());
        Assert.Equal(100000,(await new FinancialOperations(db,user,access).PositionAsync(branch.Id,default)).ExpectedPhysicalCash);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.CloseAsync(one.Id,new(Guid.NewGuid(),one.Version,100000)));
        var closed = await service.CloseAsync(two.Id,new(Guid.NewGuid(),two.Version,99800,"Shared drawer shortage"));
        Assert.Equal(-200,closed.Difference); Assert.Equal(ShiftStatus.Open,(await service.GetAsync(one.Id)).Status);
        Assert.Equal(99800,(await new FinancialOperations(db,user,access).PositionAsync(branch.Id,default)).ExpectedPhysicalCash);
        Assert.Equal(100000,(await new FinancialOperations(db,user,access).PositionAsync(branch.Id,default)).BookCash);
    }
    [Fact] public async Task ForgottenShiftIsReturnedAndCannotBeDuplicatedOrAutomaticallyClosed()
    {
        var service = Service(); var one = await service.OpenAsync(new(Guid.NewGuid(),100000)); clock.Now = clock.Now.AddDays(3);
        Assert.Equal(one.Id,(await service.CurrentAsync())!.Id);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.OpenAsync(new(Guid.NewGuid(),100000)));
        Actor(b); Assert.NotEqual(one.Id,(await service.OpenAsync(new(Guid.NewGuid(),100000))).Id);
    }
    [Fact] public async Task OpeningAndClosingRetriesSurviveFinalClosureAndRejectChangedPayloads()
    {
        var service = Service(); var open = new OpenShiftRequest(Guid.NewGuid(),100000); var one = await service.OpenAsync(open);
        var close = new CloseShiftRequest(Guid.NewGuid(),one.Version,100000); await service.CloseAsync(one.Id,close);
        Assert.Equal(one.Id,(await service.OpenAsync(open)).Id); Assert.Equal(ShiftStatus.Closed,(await service.CloseAsync(one.Id,close)).Status);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.OpenAsync(open with { OpeningCash = 1 }));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CloseAsync(one.Id,close with { CountedCash = 1 }));
        Assert.Equal(2,await db.CashCounts.CountAsync()); Assert.Single(await db.Shifts.ToListAsync());
        var row = await db.Shifts.SingleAsync(); row.ActualClosingCash = 42; await Assert.ThrowsAsync<InvalidOperationException>(()=>db.SaveChangesAsync());
    }
    [Fact] public async Task StaleCloseAndBranchSpoofingRejectedWithoutCountSideEffects()
    {
        var one = await Service().OpenAsync(new(Guid.NewGuid(),100000));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(()=>Service().CloseAsync(one.Id,new(Guid.NewGuid(),0,100000)));
        Assert.Single(await db.CashCounts.ToListAsync());
        user.Context = user.Context! with { Permissions = [ShiftPermissions.Manage,ShiftPermissions.View] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Service().OpenAsync(new(Guid.NewGuid(),100000,other.Id)));
        user.Context = user.Context! with { Permissions = [] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Service().CloseAsync(one.Id,new(Guid.NewGuid(),one.Version,100000)));
    }
    [Fact] public async Task OvernightShiftPreservesActualTimestampsAndExplicitBusinessDate()
    {
        var service = Service(); var day = await service.OpenBusinessDayAsync(new(Guid.NewGuid(),today));
        var one = await service.OpenAsync(new(Guid.NewGuid(),100000)); clock.Now = new(2026,10,11,22,0,0,TimeSpan.Zero);
        Assert.Equal(today.AddDays(1),access.Today);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CloseBusinessDayAsync(day.Id));
        var closed = await service.CloseAsync(one.Id,new(Guid.NewGuid(),one.Version,100000));
        Assert.Equal(today,closed.OpeningBusinessDate); Assert.Equal(today,closed.ClosingBusinessDate); Assert.Equal(clock.Now,closed.ClosedAt);
        Assert.Equal(BusinessDayStatus.Open,(await db.BusinessDays.SingleAsync()).Status); await service.CloseBusinessDayAsync(day.Id);
        Assert.Equal(BusinessDayStatus.Closed,(await db.BusinessDays.SingleAsync()).Status);
    }
    [Fact] public async Task TiedTimestampCountsUsePersistedObservationOrderForCarryForward()
    {
        var service = Service(); await service.OpenAsync(new(Guid.NewGuid(),100000)); Actor(b); await service.OpenAsync(new(Guid.NewGuid(),99800));
        var pos = await new FinancialOperations(db,user,access).PositionAsync(branch.Id,default);
        Assert.Equal(99800,pos.ExpectedPhysicalCash); Assert.Equal(-200,pos.ObservedDiscrepancies);
        Assert.Equal(2,(await db.CashCounts.OrderBy(x=>x.ObservationOrder).LastAsync()).ObservationOrder);
    }
    [Fact] public async Task ActiveLinkedEmploymentAndBranchAssignmentRequired()
    {
        ea.EmploymentStatus = EmploymentStatus.Suspended; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Service().OpenAsync(new(Guid.NewGuid(),100000)));
        ea = await db.Employees.SingleAsync(x=>x.Id == ea.Id); ea.EmploymentStatus = EmploymentStatus.Active; await db.SaveChangesAsync();
        var assignment = await db.EmployeeBranchAssignments.SingleAsync(x=>x.EmployeeId==ea.Id); assignment.EndDate=today.AddDays(-1); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Service().OpenAsync(new(Guid.NewGuid(),100000)));
    }
}
