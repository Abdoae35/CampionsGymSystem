using CampionsSystem.BLL.Commissions;
using CampionsSystem.BLL.Operations;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    private CommissionService Commissions() => new(db, user, access);
    private void CommissionAccess() => user.Context = user.Context! with { Permissions = [..user.Context!.Permissions, CommissionPermissions.View, CommissionPermissions.Manage, CommissionPermissions.Close] };
    private async Task<ConfigureCommissionRequest> Settings(DateOnly? effective = null)
    { CommissionAccess(); var employee = await db.Employees.SingleAsync(e => e.UserId == actor.Id); return new(Guid.NewGuid(), employee.Id, effective ?? today, new(30000, 5, 7), new(30000, 3, 5)); }
    [Fact] public async Task CommissionSettingsAreIndependentFutureOnlyAndPreserveRuleHistory()
    {
        var r = await Settings(); var initial = await Commissions().ConfigureAsync(r); Assert.Equal(2, initial.Count);
        var p = await Commissions().OpenAsync(new(Guid.NewGuid(), "Custom boundary", today, today.AddDays(13)));
        await Assert.ThrowsAsync<BusinessRuleException>(() => Commissions().ConfigureAsync(r with { OperationId = Guid.NewGuid(), NewClients = new(10000, 1, 2) }));
        var changed = await Commissions().ConfigureAsync(r with { OperationId = Guid.NewGuid(), EffectiveFrom = today.AddDays(14), NewClients = new(10000, 1, 2) });
        Assert.Equal(4, await db.CommissionRules.CountAsync()); Assert.Equal(30000, (await Commissions().ProgressAsync(p.Id)).Employees.Single().NewClients.Target);
        Assert.Equal(10000, changed.Single(x => x.Category == "SalesNew").Target); Assert.Equal(30000, changed.Single(x => x.Category == "SalesRenewal").Target);
        Assert.Equal(2, (await Commissions().ConfigureAsync(r)).Count); Assert.Equal(4, await db.CommissionRules.CountAsync());
    }
    [Fact] public async Task CommissionPeriodsRequireContiguousBoundsAndManualNarrowFinalization()
    {
        CommissionAccess(); var r = new OpenCommissionPeriodRequest(Guid.NewGuid(), "Oct 10-23", today, today.AddDays(13));
        var p = await Commissions().OpenAsync(r); Assert.Equal(p.Id, (await Commissions().OpenAsync(r)).Id);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Commissions().OpenAsync(r with { OperationId = Guid.NewGuid() }));
        await Assert.ThrowsAsync<BusinessRuleException>(() => Commissions().FinalizeAsync(p.Id, new(Guid.NewGuid(), 0)));
        clock.Now = clock.Now.AddDays(14); var close = new FinalizeCommissionPeriodRequest(Guid.NewGuid(), 0);
        Assert.Equal("Closed", (await Commissions().FinalizeAsync(p.Id, close)).Status); Assert.Equal(1, (await Commissions().FinalizeAsync(p.Id, close)).Version);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Commissions().OpenAsync(r with { OperationId = Guid.NewGuid(), StartDate = today.AddDays(15), EndDate = today.AddDays(25) }));
        Assert.Equal(today.AddDays(14), (await Commissions().OpenAsync(r with { OperationId = Guid.NewGuid(), StartDate = today.AddDays(14), EndDate = today.AddDays(25) })).StartDate);
        Assert.Empty(await db.Payrolls.ToListAsync());
    }
    [Fact] public async Task CommissionBranchesHaveIndependentBoundariesAndScope()
    {
        CommissionAccess(); var p = await Commissions().OpenAsync(new(Guid.NewGuid(), "B", today, today.AddDays(13)));
        var other = await Commissions().OpenAsync(new(Guid.NewGuid(), "A", today, today.AddDays(20), a.Id));
        Assert.NotEqual(p.EndDate, other.EndDate); user.Context = user.Context! with { Permissions = [CommissionPermissions.View] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Commissions().ProgressAsync(other.Id));
        Assert.Single((await Commissions().PeriodsAsync()).Items);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Commissions().PeriodsAsync(a.Id));
    }
    [Fact] public async Task CommissionRuleAdministrationRequiresCapabilityAndGlobalScope()
    {
        var r = await Settings(); user.Context = user.Context! with { Permissions = [CommissionPermissions.Manage] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Commissions().ConfigureAsync(r));
        user.Context = user.Context! with { Permissions = ["Branches.AccessAll"] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Commissions().ConfigureAsync(r));
        CommissionAccess(); await Assert.ThrowsAsync<BusinessRuleException>(() => Commissions().ConfigureAsync(r with { NewClients = new(0, 5, 7) }));
        await Assert.ThrowsAsync<BusinessRuleException>(() => Commissions().ConfigureAsync(r with { Renewals = new(30000, 101, 5) }));
        Assert.Empty(await db.CommissionRules.ToListAsync());
    }
}
