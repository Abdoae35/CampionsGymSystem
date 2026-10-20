using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampionsSystem.DAL.Tests;

public sealed partial class AuthenticationApiTests
{
    private const string AdminRoot = "/api/access-administration";
    private async Task PrepareAccessOwner()
    {
        foreach (var code in OwnerAdministrationAccess.RecoveryPermissions) await GrantPermission("Owner", code);
        await Authenticate();
    }
    private async Task<(Guid User, Guid Role, Guid Permission)> AccessTarget(bool activePermission = true)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var role = new IdentityRole<Guid>("BranchManager") { Id = Guid.NewGuid() };
        Assert.True((await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>().CreateAsync(role)).Succeeded);
        var user = new ApplicationUser { UserName = "access-target" };
        Assert.True((await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().CreateAsync(user, password)).Succeeded);
        var permission = new Permission { Code = "Members.View", Name = "View members", Module = "Members", Description = "View scoped member details", IsActive = activePermission };
        db.Add(new RolePermission { RoleId = role.Id, Permission = permission });
        await db.SaveChangesAsync(); return (user.Id, role.Id, permission.Id);
    }
    private async Task<UserAccessDetails> AccessUser(Guid id)
    {
        using var response = await client.GetAsync($"{AdminRoot}/users/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<UserAccessDetails>())!;
    }
    private async Task<RoleAccessDetails> AccessRole(Guid id)
    {
        using var response = await client.GetAsync($"{AdminRoot}/roles/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<RoleAccessDetails>())!;
    }
    private async Task<AccessChangeResult> AssignAccessRole(Guid user, Guid role, bool assigned = true)
    {
        var snapshot = await AccessUser(user);
        using var response = await client.PutAsJsonAsync($"{AdminRoot}/users/{user}/roles/{role}", new RoleMembershipRequest
            { OperationId = Guid.NewGuid(), ExpectedVersion = snapshot.Version, Reason = "Authorized test membership", Assigned = assigned });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AccessChangeResult>())!;
    }
    private async Task<AccessChangeResult> SetAccessOverride(Guid user, Guid permission, string? effect)
    {
        var snapshot = await AccessUser(user);
        using var response = await client.PutAsJsonAsync($"{AdminRoot}/users/{user}/permissions/{permission}", new PermissionOverrideRequest
            { OperationId = Guid.NewGuid(), ExpectedVersion = snapshot.Version, Reason = "Authorized test override", Effect = effect });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AccessChangeResult>())!;
    }
    [Fact] public async Task OwnerAdministrationCatalogUsesCanonicalTitlesAndExistingRoles()
    {
        await PrepareAccessOwner(); var target = await AccessTarget();
        using var catalog = await client.GetAsync(AdminRoot + "/permissions");
        var modules = (await catalog.Content.ReadFromJsonAsync<List<PermissionModule>>())!;
        var member = Assert.Single(modules.Single(x => x.Module == "Members").Permissions);
        Assert.Equal("View members", member.Title); Assert.Equal("View scoped member details", member.Description);
        using var roles = await client.GetAsync(AdminRoot + "/roles");
        Assert.Equal(2, (await roles.Content.ReadFromJsonAsync<List<AccessRole>>())!.Count);
        var detail = await AccessRole(target.Role); Assert.Equal(target.Permission, Assert.Single(detail.Grants).Id);
        using var users = await client.GetAsync(AdminRoot + "/users?search=access-target&pageSize=1");
        Assert.Equal(target.User, Assert.Single((await users.Content.ReadFromJsonAsync<AccessPage<AccessUserItem>>())!.Items).UserId);
    }
    [Theory]
    [InlineData("BranchManager")]
    [InlineData("Sales")]
    [InlineData("PrivateCoach")]
    [InlineData("Receptionist")]
    public async Task NonOwnersWithAllAdministrationCapabilitiesCannotReadOrMutate(string role)
    {
        foreach (var code in OwnerAdministrationAccess.RecoveryPermissions) await GrantPermission(role, code);
        await GrantPermission(role, "Branches.AccessAll"); await Authenticate();
        foreach (var path in new[] { "/permissions", "/roles", "/users", "/history", $"/users/{userId}" })
        { using var denied = await client.GetAsync(AdminRoot + path); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode); }
        foreach (var path in new[] { $"/users/{userId}/roles/{Guid.NewGuid()}", $"/users/{userId}/permissions/{Guid.NewGuid()}", $"/roles/{Guid.NewGuid()}/permissions/{Guid.NewGuid()}" })
        { using var denied = await client.PutAsJsonAsync(AdminRoot + path, new { operationId = Guid.NewGuid(), expectedVersion = new string('A', 64), reason = "Escalation", assigned = true }); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode); }
    }
    [Fact] public async Task OwnerAdministrationStillRequiresEffectiveCapabilities()
    {
        await GrantPermission("Owner", "Users.View"); await Authenticate();
        using var read = await client.GetAsync(AdminRoot + "/users"); Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        using var catalog = await client.GetAsync(AdminRoot + "/permissions"); Assert.Equal(HttpStatusCode.Forbidden, catalog.StatusCode);
        using var write = await client.PutAsJsonAsync($"{AdminRoot}/users/{userId}/roles/{Guid.NewGuid()}", new { assigned = true });
        Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
    }
    [Fact] public async Task AccessAdministrationMultiRoleUnionDenyClearAndAllowUseRuntimeEvaluator()
    {
        await PrepareAccessOwner(); var t = await AccessTarget(); await AssignAccessRole(t.User, t.Role);
        Guid salesRole, editPermission;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var r = new IdentityRole<Guid>("Sales") { Id = Guid.NewGuid() }; salesRole = r.Id;
            Assert.True((await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>().CreateAsync(r)).Succeeded);
            var p = new Permission { Code = "Members.Edit", Name = "Edit", Module = "Members" }; editPermission = p.Id;
            db.Add(new RolePermission { RoleId = r.Id, Permission = p }); await db.SaveChangesAsync();
        }
        await AssignAccessRole(t.User, salesRole);
        var union = await AccessUser(t.User); Assert.Equal(2, union.Roles.Count);
        Assert.True(union.Permissions.Single(x => x.Code == "Members.View").Effective);
        Assert.True(union.Permissions.Single(x => x.Code == "Members.Edit").Effective);
        await SetAccessOverride(t.User, t.Permission, "Deny");
        var denied = (await AccessUser(t.User)).Permissions.Single(x => x.PermissionId == t.Permission);
        Assert.False(denied.Effective); Assert.Equal("Deny", denied.Override); Assert.Single(denied.InheritedFromRoles);
        await SetAccessOverride(t.User, t.Permission, null);
        Assert.True((await AccessUser(t.User)).Permissions.Single(x => x.PermissionId == t.Permission).Effective);
        await AssignAccessRole(t.User, salesRole, false); await SetAccessOverride(t.User, editPermission, "Allow");
        var allowed = (await AccessUser(t.User)).Permissions.Single(x => x.PermissionId == editPermission);
        Assert.True(allowed.Effective); Assert.Empty(allowed.InheritedFromRoles);
    }
    [Fact] public async Task AccessOverrideRevocationAndRoleRemovalAffectExistingJwtImmediately()
    {
        await PrepareAccessOwner(); var ownerToken = client.DefaultRequestHeaders.Authorization;
        var t = await AccessTarget(); await AssignAccessRole(t.User, t.Role);
        using var login = await Login(identifier: "access-target");
        var targetToken = new AuthenticationHeaderValue("Bearer", (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken);
        client.DefaultRequestHeaders.Authorization = targetToken;
        using (var allowed = await client.GetAsync("/_tests/auth/permission")) Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        client.DefaultRequestHeaders.Authorization = ownerToken; await SetAccessOverride(t.User, t.Permission, "Deny");
        client.DefaultRequestHeaders.Authorization = targetToken;
        using (var denied = await client.GetAsync("/_tests/auth/permission")) Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        client.DefaultRequestHeaders.Authorization = ownerToken; await SetAccessOverride(t.User, t.Permission, null);
        await AssignAccessRole(t.User, t.Role, false); client.DefaultRequestHeaders.Authorization = targetToken;
        using var me = await client.GetAsync("/api/auth/me");
        var context = (await me.Content.ReadFromJsonAsync<CurrentUserContext>())!;
        Assert.Empty(context.Roles); Assert.DoesNotContain("Members.View", context.Permissions);
    }
    [Fact] public async Task RoleGrantRevocationChangesExistingJwtAndInvalidatesUserVersion()
    {
        await PrepareAccessOwner(); var token = client.DefaultRequestHeaders.Authorization;
        var t = await AccessTarget(); await AssignAccessRole(t.User, t.Role); var before = await AccessUser(t.User);
        using var login = await Login(identifier: "access-target"); var targetToken = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
        var role = await AccessRole(t.Role);
        using var revoke = await client.PutAsJsonAsync($"{AdminRoot}/roles/{t.Role}/permissions/{t.Permission}", new RoleGrantRequest
            { OperationId = Guid.NewGuid(), ExpectedVersion = role.Version, Reason = "Remove inherited access", Granted = false });
        Assert.Equal(HttpStatusCode.OK, revoke.StatusCode);
        using var stale = await client.PutAsJsonAsync($"{AdminRoot}/users/{t.User}/permissions/{t.Permission}", new PermissionOverrideRequest
            { OperationId = Guid.NewGuid(), ExpectedVersion = before.Version, Reason = "Old inherited state", Effect = "Allow" });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", targetToken);
        using var denied = await client.GetAsync("/_tests/auth/permission"); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        client.DefaultRequestHeaders.Authorization = token;
    }
    [Fact] public async Task AccessChangesRetryOnceRejectOperationReuseAndPreserveAudit()
    {
        await PrepareAccessOwner(); var t = await AccessTarget(); var before = await AccessUser(t.User); var op = Guid.NewGuid();
        var body = new RoleMembershipRequest { OperationId = op, ExpectedVersion = before.Version, Reason = "Approved role", Assigned = true };
        var path = $"{AdminRoot}/users/{t.User}/roles/{t.Role}";
        using var first = await client.PutAsJsonAsync(path, body); Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        using var retry = await client.PutAsJsonAsync(path, body); Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Equal(await first.Content.ReadFromJsonAsync<AccessChangeResult>(), await retry.Content.ReadFromJsonAsync<AccessChangeResult>());
        using var reused = await client.PutAsJsonAsync(path, new RoleMembershipRequest { OperationId = op, ExpectedVersion = before.Version, Reason = "Different request", Assigned = false });
        Assert.Equal(HttpStatusCode.Conflict, reused.StatusCode);
        using var history = await client.GetAsync($"{AdminRoot}/history?targetUserId={t.User}&roleId={t.Role}&operationId={op}&actorUserId={userId}");
        var page = (await history.Content.ReadFromJsonAsync<AccessPage<AccessAuditDetails>>())!;
        var fact = Assert.Single(page.Items); Assert.Equal(1, page.TotalCount); Assert.Equal("Approved role", fact.Reason);
        Assert.Equal(userId, fact.ActorUserId); Assert.Equal(t.User, fact.TargetUserId); Assert.True(fact.Result.Changed);
        Assert.Null(fact.ActorName); Assert.Null(fact.TargetUserName);
        Assert.Equal("BranchManager", fact.RoleName); Assert.Null(fact.PermissionCode);
        Assert.Contains("BranchManager", fact.NewValues);
        await using var scope = factory.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var audit = await db.AuditLogs.SingleAsync(x => x.EntityId == op); audit.Description = "Tamper";
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task AccessHistoryUsesCurrentLinkedNamesAndPermissionCodes()
    {
        await PrepareAccessOwner(); var target = await AccessTarget();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.AddRange(
                new Employee { UserId = userId, FullName = "Owner operator", Phone = "access-actor-name", HireDate = BranchToday.AddDays(-5) },
                new Employee { UserId = target.User, FullName = "Target operator", Phone = "access-target-name", HireDate = BranchToday.AddDays(-5), EmploymentStatus = CampionsSystem.DAL.Enums.EmploymentStatus.Resigned });
            await db.SaveChangesAsync();
        }
        await SetAccessOverride(target.User, target.Permission, "Deny");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var employee = await db.Employees.SingleAsync(x => x.UserId == target.User);
            employee.FullName = "Updated target";
            await db.SaveChangesAsync();
        }
        using var response = await client.GetAsync($"{AdminRoot}/history?targetUserId={target.User}&page=1&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = (await response.Content.ReadFromJsonAsync<AccessPage<AccessAuditDetails>>())!;
        var item = Assert.Single(page.Items);
        Assert.Equal("Owner operator", item.ActorName);
        Assert.Equal("Updated target", item.TargetUserName);
        Assert.Equal("Members.View", item.PermissionCode);
        Assert.Null(item.RoleName);
        Assert.Equal(1, page.TotalCount);
    }
    [Fact] public async Task DesiredStateNoopIsAuditedWithoutDuplicateMembership()
    {
        await PrepareAccessOwner(); var t = await AccessTarget(); await AssignAccessRole(t.User, t.Role);
        Assert.False((await AssignAccessRole(t.User, t.Role)).Changed);
        await using var scope = factory.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Single(await db.UserRoles.Where(x => x.UserId == t.User).ToListAsync());
        Assert.Equal(2, await db.AuditLogs.CountAsync(x => x.Action == AccessAdministrationService.AuditAction));
    }
    [Fact] public async Task OwnerSelfRemovalProvisioningAndRecoveryCapabilityDenialAreRejectedAtomically()
    {
        await PrepareAccessOwner(); var t = await AccessTarget();
        Guid role, manage;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            role = await db.Roles.Where(x => x.NormalizedName == "OWNER").Select(x => x.Id).SingleAsync();
            manage = await db.Permissions.Where(x => x.Code == "Users.Manage").Select(x => x.Id).SingleAsync();
        }
        var snapshot = await AccessUser(userId);
        using var self = await client.PutAsJsonAsync($"{AdminRoot}/users/{userId}/roles/{role}", new RoleMembershipRequest
            { OperationId = Guid.NewGuid(), ExpectedVersion = snapshot.Version, Reason = "Self demotion", Assigned = false });
        Assert.Equal(HttpStatusCode.Conflict, self.StatusCode);
        using var deny = await client.PutAsJsonAsync($"{AdminRoot}/users/{userId}/permissions/{manage}", new PermissionOverrideRequest
            { OperationId = Guid.NewGuid(), ExpectedVersion = snapshot.Version, Reason = "Self lockout", Effect = "Deny" });
        Assert.Equal(HttpStatusCode.Conflict, deny.StatusCode);
        using var provision = await client.PutAsJsonAsync($"{AdminRoot}/users/{t.User}/roles/{role}", new RoleMembershipRequest
            { OperationId = Guid.NewGuid(), ExpectedVersion = (await AccessUser(t.User)).Version, Reason = "Owner creation", Assigned = true });
        Assert.Equal(HttpStatusCode.BadRequest, provision.StatusCode);
        Assert.Equal(snapshot.Version, (await AccessUser(userId)).Version);
        await using var scope2 = factory.Services.CreateAsyncScope(); var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Empty(await db2.UserPermissionOverrides.ToListAsync()); Assert.Empty(await db2.AuditLogs.ToListAsync());
    }
    [Fact] public async Task OwnerRoleCoreGrantCannotRemoveLastUsableAdministrator()
    {
        await PrepareAccessOwner(); Guid role, permission;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            role = await db.Roles.Where(x => x.NormalizedName == "OWNER").Select(x => x.Id).SingleAsync();
            permission = await db.Permissions.Where(x => x.Code == "Roles.Manage").Select(x => x.Id).SingleAsync();
        }
        using var remove = await client.PutAsJsonAsync($"{AdminRoot}/roles/{role}/permissions/{permission}", new RoleGrantRequest
            { OperationId = Guid.NewGuid(), ExpectedVersion = (await AccessRole(role)).Version, Reason = "Remove recovery", Granted = false });
        Assert.Equal(HttpStatusCode.Conflict, remove.StatusCode);
        Assert.Contains((await AccessRole(role)).Grants, x => x.Id == permission);
    }
    [Fact] public async Task OwnerCanDemoteAnotherOwnerWithoutChangingEmployeeOrSalesHistory()
    {
        await PrepareAccessOwner(); var t = await AccessTarget(); Guid ownerRole;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            ownerRole = await db.Roles.Where(x => x.NormalizedName == "OWNER").Select(x => x.Id).SingleAsync();
            Assert.True((await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().AddToRoleAsync((await db.Users.SingleAsync(x => x.Id == t.User)), "Owner")).Succeeded);
        }
        await AssignAccessRole(t.User, ownerRole, false);
        Assert.DoesNotContain((await AccessUser(t.User)).Roles, x => x.IsOwner);
        Assert.Contains((await AccessUser(userId)).Roles, x => x.IsOwner);
    }
    [Fact] public async Task AccessConfigurationPreservesBranchAndCommercialOwnership()
    {
        await PrepareAccessOwner(); var t = await AccessTarget(); Guid branch, employee, member, lead;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var b = new GymBranch { Name = "Target branch", City = new() { Name = "City" } }; branch = b.Id;
            var e = new Employee { UserId = t.User, FullName = "Manager Sales", Phone = "access-person", HireDate = BranchToday.AddDays(-5) }; employee = e.Id;
            var m = new Member { MemberCode = "ACCESS-M", FullName = "Assigned member", PrimaryPhone = "access-member", HomeBranch = b, CreatedByUserId = userId }; member = m.Id;
            var l = new Lead { Branch = b, FullName = "Assigned lead", Phone = "access-lead", CreatedByUserId = userId }; lead = l.Id;
            db.AddRange(m, new EmployeeBranchAssignment { Employee = e, Branch = b, StartDate = e.HireDate, IsPrimary = true, AssignedByUserId = userId },
                new MemberSalesAssignment { Member = m, SalesEmployee = e, AssignedByUserId = userId, AssignedAt = DateTimeOffset.UtcNow },
                new LeadAssignment { Lead = l, SalesEmployee = e, AssignedByUserId = userId, AssignedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
        }
        await AssignAccessRole(t.User, t.Role); await SetAccessOverride(t.User, t.Permission, "Deny"); await AssignAccessRole(t.User, t.Role, false);
        var snapshot = await AccessUser(t.User); Assert.Equal(branch, snapshot.CurrentBranch?.Id); Assert.False(snapshot.HasGlobalBranchScope);
        Assert.Equal(employee, snapshot.Employee?.Id); Assert.Single(snapshot.BranchAssignments);
        await using var scope2 = factory.Services.CreateAsyncScope(); var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(employee, (await db2.MemberSalesAssignments.SingleAsync(x => x.MemberId == member)).SalesEmployeeId);
        var leadAssignment = await db2.LeadAssignments.SingleAsync(x => x.LeadId == lead);
        Assert.Equal(employee, leadAssignment.SalesEmployeeId); Assert.Null(leadAssignment.EndedAt);
        Assert.Equal(2, await db2.Users.CountAsync()); Assert.Empty(await db2.FinancialTransactions.ToListAsync());
    }
    [Theory]
    [InlineData("bad-effect")]
    [InlineData("unknown-field")]
    [InlineData("empty-operation")]
    [InlineData("empty-reason")]
    [InlineData("omitted-effect")]
    [InlineData("numeric-effect")]
    public async Task AccessAdministrationMalformedRequestsDoNotMutate(string input)
    {
        await PrepareAccessOwner(); var t = await AccessTarget();
        var body = new Dictionary<string, object?> { ["operationId"] = input == "empty-operation" ? Guid.Empty : Guid.NewGuid(),
            ["expectedVersion"] = (await AccessUser(t.User)).Version, ["reason"] = input == "empty-reason" ? " " : "Test", ["effect"] = input == "bad-effect" ? "Admin" : "Allow" };
        if (input == "unknown-field") body["isOwner"] = true;
        if (input == "omitted-effect") body.Remove("effect");
        if (input == "numeric-effect") body["effect"] = 1;
        using var response = await client.PutAsJsonAsync($"{AdminRoot}/users/{t.User}/permissions/{t.Permission}", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Empty(await db.UserPermissionOverrides.ToListAsync()); Assert.Empty(await db.AuditLogs.ToListAsync());
    }
    [Fact] public async Task AccessAdministrationMissingResourcesInactivePermissionsAndPaginationAreSafe()
    {
        await PrepareAccessOwner(); var t = await AccessTarget(false);
        using var missing = await client.GetAsync($"{AdminRoot}/users/{Guid.NewGuid()}"); Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var grant = await client.PutAsJsonAsync($"{AdminRoot}/roles/{t.Role}/permissions/{t.Permission}", new RoleGrantRequest
            { OperationId = Guid.NewGuid(), ExpectedVersion = (await AccessRole(t.Role)).Version, Reason = "Inactive grant", Granted = true });
        Assert.Equal(HttpStatusCode.BadRequest, grant.StatusCode);
        foreach (var query in new[] { "page=0", "pageSize=101", "page=2147483647&pageSize=100" })
        { using var invalid = await client.GetAsync(AdminRoot + "/users?" + query); Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode); }
        using var missingRole = await client.PutAsJsonAsync($"{AdminRoot}/users/{t.User}/roles/{Guid.NewGuid()}", new RoleMembershipRequest
            { OperationId = Guid.NewGuid(), ExpectedVersion = (await AccessUser(t.User)).Version, Reason = "Unknown role", Assigned = true });
        Assert.Equal(HttpStatusCode.NotFound, missingRole.StatusCode);
    }
    [Fact] public async Task OwnerAccessGateRejectsAnonymousLockedAndInactiveAccounts()
    {
        using (var anonymous = await client.GetAsync(AdminRoot + "/users")) Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        await PrepareAccessOwner(); await UpdateUser(x => x.LockoutEnd = DateTimeOffset.UtcNow.AddHours(1));
        using (var locked = await client.GetAsync(AdminRoot + "/users")) Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
        await UpdateUser(x => { x.LockoutEnd = null; x.IsActive = false; });
        using var inactive = await client.GetAsync(AdminRoot + "/users"); Assert.Equal(HttpStatusCode.Unauthorized, inactive.StatusCode);
    }
    [Fact] public async Task OwnerCanInspectUnavailableAccountsWithoutActivatingThem()
    {
        await PrepareAccessOwner(); var t = await AccessTarget(); await AssignAccessRole(t.User, t.Role);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var target = await db.Users.SingleAsync(x => x.Id == t.User); target.LockoutEnabled = true; target.LockoutEnd = DateTimeOffset.UtcNow.AddHours(1); await db.SaveChangesAsync();
        }
        var locked = await AccessUser(t.User); Assert.True(locked.IsLockedOut); Assert.False(locked.RuntimeAccountEligible);
        Assert.True(locked.Permissions.Single(x => x.PermissionId == t.Permission).Effective);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.Users.SingleAsync(x => x.Id == t.User)).IsActive = false; await db.SaveChangesAsync();
        }
        var inactive = await AccessUser(t.User); Assert.False(inactive.IsActive); Assert.False(inactive.RuntimeAccountEligible);
        Assert.False(inactive.Permissions.Single(x => x.PermissionId == t.Permission).Effective);
    }
    [Fact] public async Task OwnerAccessAdministrationRejectsSuspendedLinkedEmployee()
    {
        await PrepareAccessOwner(); var fixture = await BranchFixture();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.Employees.SingleAsync(x => x.Id == fixture.Employee)).EmploymentStatus = CampionsSystem.DAL.Enums.EmploymentStatus.Suspended;
            await db.SaveChangesAsync();
        }
        using var rejected = await client.GetAsync(AdminRoot + "/users"); Assert.Equal(HttpStatusCode.Unauthorized, rejected.StatusCode);
    }
}
