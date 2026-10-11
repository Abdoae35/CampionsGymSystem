using System.Security.Cryptography;
using System.Text;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CampionsSystem.BLL.Authentication;

// Explicit local-development command only; never registered as an automatic startup seed.
public sealed class DevelopmentAuthSeeder(ApplicationDbContext database, UserManager<ApplicationUser> users,
    RoleManager<IdentityRole<Guid>> roles, TimeProvider clock, IOptions<AuthContextOptions> options)
{
    public static readonly string[] PermissionCodes =
    [
        "Users.View", "Users.Manage", "Roles.View", "Roles.Manage",
        "Employees.View", "Employees.Manage", "Employees.Transfer", "Branches.View", "Branches.Manage", "Branches.AccessAll",
        .. CampionsSystem.BLL.Subscriptions.SubscriptionPermissions.Codes,
        .. CampionsSystem.BLL.CheckIns.CheckInPermissions.Codes,
        .. CampionsSystem.BLL.Members.MemberPermissions.Codes, "MembershipPlans.View", "MembershipPlans.Manage",
        .. CampionsSystem.BLL.Sales.SalesPermissions.Codes, .. CampionsSystem.BLL.Shifts.ShiftPermissions.Codes, "Expenses.Approve",
        "Private.View", "Private.Manage", "Activities.View", "Activities.Manage",
        .. CampionsSystem.BLL.Crm.CrmPermissions.Codes, "Commission.View", "Commission.ClosePeriod", "Commission.ReopenPeriod",
        "Commission.ManageRules", "Payroll.View", "Payroll.Manage", "Reports.View", "Reports.ViewRevenue", "AuditLogs.View",
        "Activities.ManageCoaches", "Activities.ViewCoachRevenue", "Classes.View", "Classes.Manage",
        "Classes.ViewAttendanceReport", "FreelancePlans.View", "FreelancePlans.Manage", "FreelancePlans.ManageEligibleMembers"
    ];

    public async Task SeedAsync(string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Configure DemoAuth:Password securely.");
        await using var transaction = database.Database.CurrentTransaction is null
            ? await database.Database.BeginTransactionAsync(cancellationToken) : null;
        var permissions = await database.Permissions.Where(x => PermissionCodes.Contains(x.Code)).ToListAsync(cancellationToken);
        foreach (var code in PermissionCodes.Where(code => permissions.All(p => p.Code != code)))
        {
            var permission = new Permission { Code = code, Name = code, Module = code.Split('.')[0] };
            database.Add(permission);
            permissions.Add(permission);
        }
        await database.SaveChangesAsync(cancellationToken);
        var grants = new Dictionary<string, string[]>
        {
            ["Owner"] = PermissionCodes,
            ["BranchManager"] = ["Vault.View", "Vault.Manage", "Expenses.Manage", "Expenses.Reverse", "Advances.View", "Advances.Create", "Payments.Refund", "Offers.Manage", "Installments.Manage", "Sales.QuickCancel", ..CampionsSystem.BLL.CheckIns.CheckInPermissions.Codes,"Branches.View", "Employees.View", "Members.View", "Members.Create", "Members.Edit", "Members.AssignSales", "Members.ChangeStatus", "Members.ViewSalesHistory", "MembershipPlans.View", "Payments.View", "Payments.Create", "Expenses.View", "Expenses.Create", "Shifts.View", "Shifts.Manage", "Private.View", "Activities.View", "Leads.View", "Leads.Manage", "Leads.Assign", "Leads.Convert", "CRM.ViewMemberFollowUps", "CRM.ManageMemberFollowUps", "CRM.ManageHouseFollowUps", "Commission.View", "Reports.View"],
            ["Sales"] = ["Shifts.View", "Shifts.Manage", "Vault.View", "Expenses.View", "Expenses.Create", "Advances.View", "Advances.Create", "Payments.View", "Payments.Create", "Members.View", "Members.Create", "Members.OwnSalesCustomers", "Members.CreateCrossBranch", "MembershipPlans.View", "Leads.View", "Leads.Manage", "Leads.Convert", "CRM.ViewMemberFollowUps", "CRM.ManageMemberFollowUps"],
            ["Reception"] = ["Shifts.Manage", "Vault.View", "Expenses.View", "Expenses.Create", "Advances.View", "Advances.Create", ..CampionsSystem.BLL.CheckIns.CheckInPermissions.Codes,"Members.View", "Members.Create", "MembershipPlans.View", "Payments.View", "Payments.Create", "Shifts.View"],
            ["PrivateCoach"] = ["Private.View"],
            ["ClassesCoach"] = []
        };
        foreach (var (name, codes) in grants)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await roles.FindByNameAsync(name) is not null) continue; // Preserve customized existing grants.
            var role = new IdentityRole<Guid>(name) { Id = Guid.NewGuid() };
            Ensure(await roles.CreateAsync(role));
            foreach (var permission in permissions.Where(p => p.IsActive && codes.Contains(p.Code)))
                database.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        }
        await database.SaveChangesAsync(cancellationToken);

        var owner = await AccountAsync("dev.owner", true, "Owner", password);
        var reception = await AccountAsync("dev.reception", true, "Reception", password);
        await AccountAsync("dev.no-permissions", true, null, password);
        await AccountAsync("dev.inactive", false, null, password);
        var suspended = await AccountAsync("dev.suspended", true, "Reception", password);

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(),
            TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId)).DateTime);
        var city = await database.Cities.FindAsync([Id("city")], cancellationToken);
        if (city is null) { city = new City { Id = Id("city"), Name = "Auth Demo City" }; database.Add(city); }
        var branch = await database.GymBranches.FindAsync([Id("branch")], cancellationToken);
        if (branch is null)
        {
            branch = new GymBranch { Id = Id("branch"), City = city, Name = "Auth Demo Branch" };
            database.Add(branch);
        }
        await EmployeeAsync(reception, branch, owner, today, EmploymentStatus.Active, cancellationToken);
        await EmployeeAsync(suspended, branch, owner, today, EmploymentStatus.Suspended, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
    }

    private async Task<ApplicationUser> AccountAsync(string name, bool active, string? role, string password)
    {
        var user = await users.FindByNameAsync(name);
        if (user is not null)
        {
            if (user.Id != Id(name)) throw new InvalidOperationException("A demo username is occupied by another account; no data was changed.");
            return user; // Do not reset password, activation, lockout or customized role memberships.
        }
        user = new ApplicationUser { Id = Id(name), UserName = name, Email = name + "@auth-demo.invalid", IsActive = active, LockoutEnabled = true };
        Ensure(await users.CreateAsync(user, password));
        if (role is not null) Ensure(await users.AddToRoleAsync(user, role));
        return user;
    }

    private async Task EmployeeAsync(ApplicationUser user, GymBranch branch, ApplicationUser creator,
        DateOnly today, EmploymentStatus status, CancellationToken cancellationToken)
    {
        var employee = await database.Employees.SingleOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);
        if (employee is not null) return; // Preserve existing employment/assignment changes.
        employee = new Employee { Id = Id(user.UserName! + ":employee"), UserId = user.Id,
            FullName = user.UserName == "dev.reception" ? "Demo Reception" : "Demo Suspended Employee",
            Phone = user.UserName!, EmploymentStatus = status, HireDate = today };
        database.Add(employee);
        database.Add(new EmployeeBranchAssignment { Id = Id(user.UserName! + ":assignment"), Employee = employee,
            Branch = branch, StartDate = today, IsPrimary = true, AssignedByUserId = creator.Id });
    }

    // Hash is for stable fixture IDs only. Passwords are handled exclusively by Identity.
    private static Guid Id(string value) => new(SHA256.HashData(Encoding.UTF8.GetBytes("Campions:AuthDemo:" + value)).AsSpan(0, 16));
    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException("Demo Identity setup failed; transaction will roll back. Check password policy or account conflicts.");
    }
}
