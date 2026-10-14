using System.Net;
using System.Net.Http.Json;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampionsSystem.DAL.Tests;

public sealed partial class AuthenticationApiTests
{
    [Theory]
    [InlineData(EmploymentStatus.Suspended)]
    [InlineData(EmploymentStatus.Resigned)]
    public async Task OwnerLifecycleApiSelfLockoutReturnsConflictAndExistingJwtStillWorks(EmploymentStatus status)
    {
        var f = await BranchFixture(); await PrepareAccessOwner(); await GrantPermission("Owner", "Employees.Manage");
        using var response = await client.PatchAsJsonAsync($"/api/employees/{f.Employee}/status", new { status = status.ToString(), reason = "Accidental self change" });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
        await using var scope = factory.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(EmploymentStatus.Active, (await db.Employees.SingleAsync(x => x.Id == f.Employee)).EmploymentStatus);
        Assert.Empty(await db.AuditLogs.Where(x => x.Action == "EmployeeStatusChanged").ToListAsync());
    }

    [Fact]
    public async Task OwnerLifecycleApiManagerCannotDisableOwnerAndIndividualDenyStillWins()
    {
        var f = await BranchFixture(); await GrantPermission("BranchManager", "Employees.Manage"); await GrantPermission("BranchManager", "Branches.AccessAll");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var account = new ApplicationUser { UserName = "protected-owner" };
            var role = new IdentityRole<Guid>("Owner") { Id = Guid.NewGuid(), NormalizedName = "OWNER" };
            var employee = new Employee { User = account, FullName = "Protected Owner", Phone = "protected-owner", HireDate = BranchToday.AddDays(-30) };
            db.AddRange(account, role, new IdentityUserRole<Guid> { UserId = account.Id, RoleId = role.Id },
                new EmployeeBranchAssignment { Employee = employee, BranchId = f.Source, StartDate = employee.HireDate, AssignedByUserId = userId });
            await db.SaveChangesAsync();
            await Authenticate();
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PatchAsJsonAsync($"/api/employees/{employee.Id}/status", new { status = "Suspended" })).StatusCode);
            var permission = await db.Permissions.SingleAsync(x => x.Code == "Employees.Manage");
            db.Add(new UserPermissionOverride { UserId = userId, PermissionId = permission.Id, Effect = PermissionOverrideEffect.Deny }); await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PatchAsJsonAsync($"/api/employees/{f.Employee}/status", new { status = "Suspended" })).StatusCode);
    }
}
