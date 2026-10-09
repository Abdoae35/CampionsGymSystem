using System.Net;
using System.Net.Http.Json;
using CampionsSystem.BLL.Members;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampionsSystem.DAL.Tests;

public sealed partial class AuthenticationApiTests
{
    private async Task GrantMembers(params string[] codes)
    {
        foreach (var code in codes) await GrantPermission("ConfigurableMemberOperator", code);
    }
    private static CreateMemberRequest Registration(Guid branch, string phone = "+201212345678") => new()
        { FullName = "HTTP Member", PrimaryPhone = phone, TargetRegistrationBranchId = branch, CreationType = MemberCreationType.NormalSales };
    private async Task<MemberDetails> Register(Guid branch, string phone = "+201212345678")
    {
        using var response = await client.PostAsJsonAsync("/api/members", Registration(branch, phone));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<MemberDetails>())!;
    }
    [Fact] public async Task MemberEndpointsRequireAuthenticationAndActionPermissions()
    {
        using var anonymous = await client.GetAsync("/api/members"); Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        await Authenticate();
        using var list = await client.GetAsync("/api/members"); Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        using var create = await client.PostAsJsonAsync("/api/members", Registration(Guid.NewGuid())); Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
        using var assign = await client.PostAsJsonAsync($"/api/members/{Guid.NewGuid()}/assign-sales", new SalesAssignmentRequest { SalesEmployeeId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.Forbidden, assign.StatusCode);
        using var status = await client.PatchAsJsonAsync($"/api/members/{Guid.NewGuid()}/status", new { isActive = false }); Assert.Equal(HttpStatusCode.Forbidden, status.StatusCode);
        using var history = await client.GetAsync($"/api/members/{Guid.NewGuid()}/sales-history"); Assert.Equal(HttpStatusCode.Forbidden, history.StatusCode);
        using var update = await client.PutAsJsonAsync($"/api/members/{Guid.NewGuid()}", new UpdateMemberRequest()); Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
    }
    [Fact] public async Task MemberCrossBranchReferralRequiresSpecificPermissionAndDoesNotGrantOtherScope()
    {
        var data = await BranchFixture(); await GrantMembers("Members.Create", "Members.View", "Members.OwnSalesCustomers"); await Authenticate();
        using var denied = await client.PostAsJsonAsync("/api/members", Registration(data.Target)); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        await GrantMembers("Members.CreateCrossBranch");
        var created = await Register(data.Target);
        Assert.Equal(data.Target, created.RegistrationBranch.Id); Assert.Equal(1, created.ClientCode);
        Assert.Equal(data.Employee, created.CurrentSalesOwner?.SalesEmployeeId); Assert.False(created.IsHouse);
        using var other = await client.GetAsync($"/api/members/{created.Id}"); Assert.Equal(HttpStatusCode.NotFound, other.StatusCode);
        using var list = await client.GetAsync($"/api/members?registrationBranchId={data.Target}"); Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        using var branch = await client.GetAsync($"/api/branches/{data.Target}/employees"); Assert.Equal(HttpStatusCode.Forbidden, branch.StatusCode);
        using var report = await client.GetAsync($"/api/activities/coach-revenue/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Forbidden, report.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(data.Source, (await db.EmployeeBranchAssignments.SingleAsync()).BranchId);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync()); Assert.Empty(await db.SaleTransactions.ToListAsync());
        Assert.Empty(await db.CheckIns.ToListAsync());
    }
    [Fact] public async Task MemberHttpWorkflowUpdatesStatusAndOwnershipWithoutDeletingHistory()
    {
        var data = await BranchFixture();
        await GrantMembers("Members.Create", "Members.View", "Members.OwnSalesCustomers", "Members.Edit", "Members.ChangeStatus",
            "Members.AssignSales", "Members.ViewSalesHistory", "Members.ChangePrimaryPhone"); await Authenticate();
        var created = await Register(data.Source);
        using var update = await client.PutAsJsonAsync($"/api/members/{created.Id}", new UpdateMemberRequest { FullName = "Edited", PrimaryPhone = " +20 (12) 2222-2222 ", Notes = "HTTP" });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var edited = (await update.Content.ReadFromJsonAsync<MemberDetails>())!; Assert.Equal("+201222222222", edited.PrimaryPhoneNormalized);
        Assert.Equal(created.ClientCode, edited.ClientCode); Assert.Equal(created.MemberCode, edited.MemberCode);
        using var status = await client.PatchAsJsonAsync($"/api/members/{created.Id}/status", new { isActive = false, reason = "HTTP" }); Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        using var house = await client.PostAsJsonAsync($"/api/members/{created.Id}/return-to-house", new { reason = "Distribution" }); Assert.Equal(HttpStatusCode.OK, house.StatusCode);
        Assert.True((await house.Content.ReadFromJsonAsync<MemberDetails>())!.IsHouse);
        using var assign = await client.PostAsJsonAsync($"/api/members/{created.Id}/assign-sales", new { salesEmployeeId = data.Employee }); Assert.Equal(HttpStatusCode.OK, assign.StatusCode);
        using var owner = await client.GetAsync($"/api/members/{created.Id}/sales-assignment"); Assert.Equal(HttpStatusCode.OK, owner.StatusCode);
        using var history = await client.GetAsync($"/api/members/{created.Id}/sales-history"); Assert.Equal(HttpStatusCode.OK, history.StatusCode);
        Assert.Equal(2, (await history.Content.ReadFromJsonAsync<SalesAssignmentHistoryItem[]>())!.Length);
        using var search = await client.GetAsync("/api/members?search=%2B20%20(12)%202222-2222&isActive=false&houseOnly=false&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, search.StatusCode); Assert.Equal(created.Id, Assert.Single((await search.Content.ReadFromJsonAsync<MemberListResult>())!.Items).Id);
        using var delete = await client.DeleteAsync($"/api/members/{created.Id}"); Assert.Equal(HttpStatusCode.MethodNotAllowed, delete.StatusCode);
    }
    [Fact] public async Task MemberApiRejectsProtectedFieldsAndSensitivePhoneWithoutPermission()
    {
        var data = await BranchFixture(); await GrantMembers("Members.Create", "Members.OwnSalesCustomers", "Members.Edit"); await Authenticate();
        var created = await Register(data.Source);
        foreach (var field in new[] { "id", "clientCode", "registrationBranchId", "homeBranchId", "salesEmployeeId", "isActive", "memberCode" })
        {
            using var response = await client.PutAsJsonAsync($"/api/members/{created.Id}",
                new Dictionary<string, object?> { ["fullName"] = "Spoof", ["primaryPhone"] = created.PrimaryPhone, [field] = null });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        using var phone = await client.PutAsJsonAsync($"/api/members/{created.Id}", new UpdateMemberRequest { FullName = "Phone", PrimaryPhone = "+201255555555" });
        Assert.Equal(HttpStatusCode.Forbidden, phone.StatusCode);
        var request = Registration(data.Source); request.SalesEmployeeId = data.Employee;
        using var spoof = await client.PostAsJsonAsync("/api/members", request); Assert.Equal(HttpStatusCode.BadRequest, spoof.StatusCode);
    }
    [Fact] public async Task MemberApiRejectsDuplicatePhonesInactiveBranchesAndInvalidRequests()
    {
        var data = await BranchFixture(); await GrantMembers("Members.Create", "Members.OwnSalesCustomers", "Members.View", "Members.CreateCrossBranch"); await Authenticate();
        await Register(data.Source);
        using var duplicate = await client.PostAsJsonAsync("/api/members", Registration(data.Source, "+20 (12) 1234-5678")); Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        using var missingType = await client.PostAsJsonAsync("/api/members", new { fullName = "Missing", primaryPhone = "+441234567890", targetRegistrationBranchId = data.Source }); Assert.Equal(HttpStatusCode.BadRequest, missingType.StatusCode);
        using var invalidPage = await client.GetAsync("/api/members?page=2147483647&pageSize=100"); Assert.Equal(HttpStatusCode.BadRequest, invalidPage.StatusCode);
        using var missingBranch = await client.PostAsJsonAsync("/api/members", Registration(Guid.NewGuid(), "+441234567890")); Assert.Equal(HttpStatusCode.BadRequest, missingBranch.StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.GymBranches.Where(b => b.Id == data.Target).ExecuteUpdateAsync(x => x.SetProperty(b => b.IsActive, false));
        }
        using var inactive = await client.PostAsJsonAsync("/api/members", Registration(data.Target, "+441234567890")); Assert.Equal(HttpStatusCode.BadRequest, inactive.StatusCode);
    }
    [Fact] public async Task MemberSalesCapabilityCanBeRevokedAndCannotAssignWithoutManagementPermission()
    {
        var data = await BranchFixture(); await GrantMembers("Members.Create", "Members.OwnSalesCustomers"); await Authenticate();
        var created = await Register(data.Source);
        using var assign = await client.PostAsJsonAsync($"/api/members/{created.Id}/assign-sales", new { salesEmployeeId = data.Employee }); Assert.Equal(HttpStatusCode.Forbidden, assign.StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var permission = await db.Permissions.SingleAsync(x => x.Code == "Members.OwnSalesCustomers");
            db.UserPermissionOverrides.Add(new() { UserId = userId, PermissionId = permission.Id, Effect = PermissionOverrideEffect.Deny }); await db.SaveChangesAsync();
        }
        using var revoked = await client.PostAsJsonAsync("/api/members", Registration(data.Source, "+441234567890")); Assert.Equal(HttpStatusCode.Forbidden, revoked.StatusCode);
    }
    [Fact] public async Task MemberFreelanceHouseHttpFlowRequiresEligibilityPermission()
    {
        var data = await BranchFixture(); await GrantMembers("Members.Create", "Members.View"); await Authenticate();
        Guid planId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var city = await db.GymBranches.Where(b => b.Id == data.Source).Select(b => b.CityId).SingleAsync();
            var coach = new Employee { FullName = "Freelance HTTP", Phone = "coach", HireDate = BranchToday, EmploymentType = EmploymentType.Freelance };
            var plan = new MembershipPlan { Name = "HTTP Special", BranchId = data.Source, CityId = city, DurationDays = 30, PlanCategory = PlanCategory.Freelance, FreelanceCoachEmployee = coach };
            db.Add(plan); await db.SaveChangesAsync(); planId = plan.Id;
        }
        var request = Registration(data.Source); request.CreationType = MemberCreationType.FreelanceHouse; request.FreelancePlanId = planId;
        using var denied = await client.PostAsJsonAsync("/api/members", request); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        await GrantMembers("FreelancePlans.ManageEligibleMembers");
        using var response = await client.PostAsJsonAsync("/api/members", request); Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = (await response.Content.ReadFromJsonAsync<MemberDetails>())!;
        Assert.True(member.IsHouse); Assert.Equal(planId, Assert.Single(member.EligibleFreelancePlans).PlanId);
        using var eligibility = await client.PutAsJsonAsync($"/api/freelance-plans/{planId}/eligible-members/{member.Id}", new { });
        Assert.Equal(HttpStatusCode.OK, eligibility.StatusCode);
        await using var checkScope = factory.Services.CreateAsyncScope(); var check = checkScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Empty(await check.MemberSalesAssignments.ToListAsync()); Assert.Empty(await check.MemberSubscriptions.ToListAsync());
        Assert.Single(await check.MembershipPlanEligibleMembers.ToListAsync());
    }
    [Fact] public async Task MemberManagerWithoutOwnCapabilityCanAssignEligibleEmployeeFromOtherBranch()
    {
        var data = await BranchFixture(); await GrantMembers("Members.Create", "Members.AssignSales"); await Authenticate();
        Guid eligible;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = new ApplicationUser { UserName = "eligible-http-sales" };
            var users = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            Assert.True((await users.CreateAsync(user, password)).Succeeded);
            var permission = new Permission { Code = "Members.OwnSalesCustomers", Name = "Own", Module = "Members" };
            var employee = new Employee { UserId = user.Id, FullName = "Other branch Sales", Phone = "sales", HireDate = BranchToday };
            db.AddRange(permission, employee, new EmployeeBranchAssignment { Employee = employee, BranchId = data.Target,
                StartDate = BranchToday, IsPrimary = true, AssignedByUserId = userId },
                new UserPermissionOverride { UserId = user.Id, Permission = permission, Effect = PermissionOverrideEffect.Allow });
            await db.SaveChangesAsync(); eligible = employee.Id;
        }
        var request = Registration(data.Source); request.CreationType = MemberCreationType.ManagementCreated; request.SalesEmployeeId = eligible;
        using var created = await client.PostAsJsonAsync("/api/members", request); Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var house = (await created.Content.ReadFromJsonAsync<MemberDetails>())!; Assert.False(house.IsHouse);
        using var returned = await client.PostAsJsonAsync($"/api/members/{house.Id}/return-to-house", new { }); Assert.Equal(HttpStatusCode.OK, returned.StatusCode);
        using var assigned = await client.PostAsJsonAsync($"/api/members/{house.Id}/assign-sales", new { salesEmployeeId = eligible });
        Assert.Equal(HttpStatusCode.OK, assigned.StatusCode); Assert.Equal(eligible, (await assigned.Content.ReadFromJsonAsync<MemberDetails>())!.CurrentSalesOwner?.SalesEmployeeId);
    }

}
