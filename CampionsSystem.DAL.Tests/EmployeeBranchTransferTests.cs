using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace CampionsSystem.DAL.Tests;

public sealed class EmployeeBranchTransferTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private TestBranchUser current = null!;
    private EmployeeBranchAssignmentService service = null!;
    private Employee employee = null!;
    private GymBranch source = null!;
    private GymBranch target = null!;
    private EmployeeBranchAssignment original = null!;
    private OperationAccess access = null!;
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        var user = new ApplicationUser { UserName = "transfer-admin" };
        source = new GymBranch { Name = "A", City = new City { Name = "Transfer" } };
        target = new GymBranch { Name = "B", City = source.City };
        employee = new Employee { FullName = "Transfer Employee", Phone = "transfer", HireDate = new(2026, 1, 1) };
        current = new TestBranchUser(new(user.Id, user.UserName, null, null, [], ["Employees.Transfer", "Employees.View", "Branches.AccessAll"], null));
        access = new OperationAccess(current, db, TimeProvider.System, Options.Create(new AuthContextOptions()));
        original = new EmployeeBranchAssignment { Employee = employee, Branch = source, AssignedByUser = user,
            StartDate = access.Today.AddDays(-30), IsPrimary = true };
        db.AddRange(original, target); await db.SaveChangesAsync();
        service = new EmployeeBranchAssignmentService(db, current, access);
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    [Fact] public async Task TransferClosesOriginalCreatesNewAndAuditsWithoutMovingHistory()
    {
        var member = new Member { MemberCode = "TRANSFER-M", FullName = "Member", PrimaryPhone = "transfer-m", HomeBranch = source, CreatedByUserId = current.UserId!.Value };
        var sale = new SaleTransaction { Member = member, Branch = source, CreatedByUserId = current.UserId.Value };
        var shift = new Shift { Branch = source, OpenedByUserId = current.UserId.Value, OpenedAt = access.Now.AddDays(-1) };
        db.AddRange(sale, shift); await db.SaveChangesAsync();
        var result = await service.TransferAsync(employee.Id, new(target.Id, access.Today));
        db.ChangeTracker.Clear();
        Assert.Equal(target.Id, (await service.GetCurrentAsync(employee.Id))!.BranchId);
        Assert.Equal(access.Today.AddDays(-1), (await db.EmployeeBranchAssignments.SingleAsync(x => x.Id == original.Id)).EndDate);
        Assert.Equal(access.Today, result.StartDate); Assert.Null(result.EndDate); Assert.Equal(2, (await service.HistoryAsync(employee.Id)).Count);
        Assert.Equal(source.Id, (await db.SaleTransactions.SingleAsync()).BranchId);
        Assert.Equal(source.Id, (await db.Shifts.SingleAsync()).BranchId);
        var audit = await db.AuditLogs.SingleAsync(x => x.Action == "Employee.BranchTransferred");
        Assert.Equal(current.UserId, audit.UserId); Assert.Contains(target.Id.ToString(), audit.NewValues!);
    }
    [Fact] public async Task FutureTransferPreservesCurrentBranchUntilEffectiveDate()
    {
        var effective = access.Today.AddDays(3); await service.TransferAsync(employee.Id, new(target.Id, effective));
        var resolver = new EmployeeBranchResolver(db);
        Assert.Equal(source.Id, (await resolver.ResolveAsync(employee.Id, effective.AddDays(-1)))!.Id);
        Assert.Equal(target.Id, (await resolver.ResolveAsync(employee.Id, effective))!.Id);
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.TransferAsync(employee.Id, new(target.Id, effective.AddDays(1))));
    }
    [Theory] [InlineData("inactive")] [InlineData("same")] [InlineData("past")] [InlineData("missing")]
    public async Task InvalidTransferLeavesOriginalUnchanged(string reason)
    {
        if (reason == "inactive") { target.IsActive = false; await db.SaveChangesAsync(); }
        var request = new TransferBranchRequest(reason == "same" ? source.Id : reason == "missing" ? Guid.NewGuid() : target.Id,
            reason == "past" ? access.Today.AddDays(-1) : access.Today);
        if (reason == "missing") await Assert.ThrowsAsync<BranchNotFoundException>(() => service.TransferAsync(employee.Id, request));
        else await Assert.ThrowsAsync<BusinessRuleException>(() => service.TransferAsync(employee.Id, request));
        Assert.Null((await db.EmployeeBranchAssignments.AsNoTracking().SingleAsync()).EndDate);
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }
    [Fact] public async Task TransferRequiresPermissionAndBothBranchScopes()
    {
        current.Context = current.Context! with { Permissions = ["Employees.Transfer"], CurrentBranch = new(source.Id, source.Name) };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.TransferAsync(employee.Id, new(target.Id, access.Today)));
        current.Context = current.Context! with { CurrentBranch = new(target.Id, target.Name) };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.TransferAsync(employee.Id, new(target.Id, access.Today)));
        current.Context = current.Context! with { Permissions = ["Branches.AccessAll"] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.TransferAsync(employee.Id, new(target.Id, access.Today)));
    }
    [Fact] public async Task MissingAssignmentFailsSafely()
    {
        original.EndDate = access.Today.AddDays(-1); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.TransferAsync(employee.Id, new(target.Id, access.Today)));
    }
    [Fact] public async Task FailureAfterClosingOldAssignmentRollsBackBothSaves()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(fixture.Connection)
            .AddInterceptors(new RejectTransferAudit()).Options;
        await using var failing = new ApplicationDbContext(options); await failing.Database.UseTransactionAsync(fixture.Transaction);
        var operations = new OperationAccess(current, failing, TimeProvider.System, Options.Create(new AuthContextOptions()));
        var transfers = new EmployeeBranchAssignmentService(failing, current, operations);
        await Assert.ThrowsAsync<InvalidOperationException>(() => transfers.TransferAsync(employee.Id, new(target.Id, access.Today)));
        db.ChangeTracker.Clear(); Assert.Null((await db.EmployeeBranchAssignments.SingleAsync()).EndDate);
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }
    private sealed class RejectTransferAudit : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<AuditLog>().Any(x => x.Entity.Action == "Employee.BranchTransferred"))
                throw new InvalidOperationException("Injected failure after old assignment save.");
            return ValueTask.FromResult(result);
        }
    }
}
