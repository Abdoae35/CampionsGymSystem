using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.DAL.Tests;

public sealed class BranchSecurityTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private ApplicationUser user = null!;
    private GymBranch branch = null!;
    private GymBranch other = null!;
    private TestBranchUser current = null!;
    private BranchAccessService access = null!;
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        user = new ApplicationUser { UserName = "scope-test" };
        branch = new GymBranch { Name = "A", City = new City { Name = "Scope" } };
        other = new GymBranch { Name = "B", City = branch.City };
        db.AddRange(user, branch, other); await db.SaveChangesAsync();
        current = new TestBranchUser(new(user.Id, user.UserName, null, null, [], [], new(branch.Id, branch.Name)));
        access = new BranchAccessService(new CurrentBranchContext(current), db);
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    [Fact] public async Task NormalEmployeeCannotAccessOrSelectAnotherBranch()
    {
        Assert.True(await access.CanAccessBranchAsync(branch.Id)); Assert.False(await access.CanAccessBranchAsync(other.Id));
        Assert.Equal(branch.Id, await access.ResolveOperationBranchAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => access.ResolveOperationBranchAsync(other.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => access.EnsureCanAccessBranchAsync(other.Id));
    }
    [Fact] public async Task GlobalPermissionAllowsExplicitValidatedSelectionWithoutInventingCurrentBranch()
    {
        current.Context = current.Context! with { CurrentBranch = null, Permissions = ["Branches.AccessAll"] };
        Assert.True(await access.CanAccessBranchAsync(other.Id)); Assert.Equal(other.Id, await access.ResolveOperationBranchAsync(other.Id));
        await Assert.ThrowsAsync<CampionsSystem.BLL.Operations.BusinessRuleException>(() => access.ResolveOperationBranchAsync());
        await Assert.ThrowsAsync<BranchNotFoundException>(() => access.EnsureCanAccessBranchAsync(Guid.NewGuid()));
    }
    [Fact] public async Task OwnerRoleAloneDoesNotBypassBranchScope()
    {
        current.Context = current.Context! with { Roles = ["Owner"] };
        Assert.False(await access.CanAccessBranchAsync(other.Id));
    }
    [Fact] public async Task InactiveBranchesAreRejectedForOperationsEvenWithGlobalAccess()
    {
        other.IsActive = false; await db.SaveChangesAsync(); current.Context = current.Context! with { Permissions = ["Branches.AccessAll"] };
        Assert.False(await access.CanAccessBranchAsync(other.Id));
        await Assert.ThrowsAsync<CampionsSystem.BLL.Operations.BusinessRuleException>(() => access.EnsureCanAccessBranchAsync(other.Id));
    }
    [Fact] public async Task AnonymousAndBranchlessNormalUsersFailClosed()
    {
        current.Context = current.Context! with { CurrentBranch = null };
        Assert.False(await access.CanAccessBranchAsync(branch.Id)); current.Context = null;
        Assert.False(await access.CanAccessBranchAsync(branch.Id));
    }
    [Fact] public async Task DirectDenyOverridesRoleGrantAndDirectAllowWorksWithoutRole()
    {
        var role = new IdentityRole<Guid>("ScopeRole") { Id = Guid.NewGuid() };
        var global = new Permission { Code = "Branches.AccessAll", Name = "All", Module = "Branches" };
        var view = new Permission { Code = "Branches.View", Name = "View", Module = "Branches" };
        var inactive = new Permission { Code = "Branches.Hidden", Name = "Hidden", Module = "Branches", IsActive = false };
        db.AddRange(role, new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id },
            new RolePermission { Role = role, Permission = global },
            new UserPermissionOverride { User = user, Permission = global, Effect = PermissionOverrideEffect.Deny },
            new UserPermissionOverride { User = user, Permission = view, Effect = PermissionOverrideEffect.Allow },
            new UserPermissionOverride { User = user, Permission = inactive, Effect = PermissionOverrideEffect.Allow });
        await db.SaveChangesAsync();
        var permissions = await new PermissionService(db).GetUserPermissionsAsync(user.Id);
        Assert.Equal(new[] { "Branches.View" }, permissions.Order());
        current.Context = current.Context! with { Permissions = permissions.ToArray() };
        Assert.False(await access.CanAccessBranchAsync(other.Id));
    }
    [Fact] public async Task ExplicitPermissionSetupIsIdempotentAndCreatesNoAccounts()
    {
        var owner = new IdentityRole<Guid>("Owner") { Id = Guid.NewGuid(), NormalizedName = "OWNER" }; db.Add(owner); await db.SaveChangesAsync();
        var setup = new BranchPermissionSetup(db); await setup.ConfigureAsync(); await setup.ConfigureAsync();
        Assert.Single(await db.Permissions.Where(x => x.Code == "Branches.AccessAll").ToListAsync());
        Assert.Single(await db.RolePermissions.ToListAsync()); Assert.Single(await db.Users.ToListAsync()); Assert.Empty(await db.Employees.ToListAsync());
    }
}
