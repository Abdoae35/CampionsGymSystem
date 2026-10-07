using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CampionsSystem.DAL.Tests;

public sealed class BranchManagementTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private TestBranchUser current = null!;
    private BranchManagementService service = null!;
    private City city = null!;
    private GymBranch branch = null!;
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        var user = new ApplicationUser { UserName = "branch-manager" };
        city = new City { Name = "Branch City" }; branch = new GymBranch { Name = "Branch A", City = city };
        db.AddRange(user, branch); await db.SaveChangesAsync();
        current = new TestBranchUser(new(user.Id, user.UserName, null, null, [],
            ["Branches.View", "Branches.Manage", BranchManagementService.AccessAllPermission], null));
        service = new BranchManagementService(db, current, new OperationAccess(current, db, TimeProvider.System, Options.Create(new AuthContextOptions())));
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    [Fact] public async Task GlobalAccountWithoutBranchCanCreateUpdateAndAudit()
    {
        var created = await service.SaveAsync(null, new("New branch", city.Id));
        var updated = await service.SaveAsync(created.Id, new("Renamed", city.Id, "Address", "Phone", false));
        Assert.False(updated.IsActive); Assert.Equal("Renamed", updated.Name);
        Assert.Equal(2, (await service.ListAsync()).Count);
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "Branch.Created");
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "Branch.Deactivated");
        Assert.Equal(2, await db.GymBranches.CountAsync());
    }
    [Fact] public async Task ScopedUserListsOnlyCurrentBranchAndCannotSpoofAnother()
    {
        var other = new GymBranch { Name = "B", City = city }; db.Add(other); await db.SaveChangesAsync();
        current.Context = current.Context! with { Permissions = ["Branches.View", "Branches.Manage"], CurrentBranch = new(branch.Id, branch.Name) };
        Assert.Equal(branch.Id, Assert.Single(await service.ListAsync()).Id);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAsync(other.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SaveAsync(other.Id, new("Spoofed", city.Id)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SaveAsync(null, new("New", city.Id)));
    }
    [Fact] public async Task MissingPermissionCannotManageEvenWithGlobalScope()
    {
        current.Context = current.Context! with { Permissions = [BranchManagementService.AccessAllPermission] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SaveAsync(null, new("New", city.Id)));
    }
    [Fact] public async Task UnknownBranchReturnsNotFoundForGlobalUser() =>
        await Assert.ThrowsAsync<BranchNotFoundException>(() => service.GetAsync(Guid.NewGuid()));
    [Theory] [InlineData("")] [InlineData("   ")]
    public async Task InvalidNameRejected(string name) =>
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.SaveAsync(null, new(name, city.Id)));
    [Fact] public async Task InactiveCityCannotReceiveNewBranch()
    {
        city.IsActive = false; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.SaveAsync(null, new("New", city.Id)));
    }
}

internal sealed class TestBranchUser(CurrentUserContext? context) : ICurrentUserService
{
    public CurrentUserContext? Context { get; set; } = context;
    public bool IsAuthenticated => Context is not null;
    public Guid? UserId => Context?.UserId;
    public Task<CurrentUserContext?> GetAsync(CancellationToken cancellationToken = default) => Task.FromResult(Context);
}
