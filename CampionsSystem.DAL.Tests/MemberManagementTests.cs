using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CampionsSystem.DAL.Tests;

public sealed class MemberManagementTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private TestBranchUser current = null!;
    private MemberManagementService service = null!;
    private OperationAccess access = null!;
    private GymBranch a = null!, b = null!;
    private Member first = null!, second = null!;
    private Employee sales = null!;
    private readonly OwnerPermissions ownerPermissions = new();
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        var user = new ApplicationUser { UserName = "member-actor" };
        a = new GymBranch { Name = "A", CountryCallingCode = "+20", LastMemberClientCode = 100, City = new City { Name = "Member city" } };
        b = new GymBranch { Name = "B", CountryCallingCode = "+44", LastMemberClientCode = 100, City = a.City };
        current = new TestBranchUser(new(user.Id, user.UserName, null, null, [], ["Members.View", "Branches.AccessAll"], null));
        access = new(current, db, TimeProvider.System, Options.Create(new AuthContextOptions()));
        sales = new Employee { FullName = "Ahmed", Phone = "sales", User = user, HireDate = access.Today.AddDays(-1) };
        first = new Member { FullName = "Alpha", PrimaryPhone = "01212345678", MemberCode = "ALPHA-QR", ClientCode = 100,
            HomeBranch = a, CreatedByUser = user };
        second = new Member { FullName = "Beta", PrimaryPhone = "+441234567890", MemberCode = "BETA-QR", ClientCode = 100,
            HomeBranch = b, CreatedByUser = user };
        db.AddRange(first, second, sales, new EmployeeBranchAssignment { Employee = sales, Branch = a,
            StartDate = access.Today, IsPrimary = true, AssignedByUser = user },
            new MemberSalesAssignment { Member = first, SalesEmployee = sales, AssignmentType = AssignmentType.Sales,
                AssignedAt = access.Now, AssignedByUser = user });
        await db.SaveChangesAsync();
        service = new(db, current, access, ownerPermissions);
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private void Scoped(params string[] permissions) => current.Context = current.Context! with
        { Permissions = permissions, CurrentBranch = new(a.Id, a.Name), EmployeeId = sales.Id };
    [Fact] public async Task NamePhoneCodeAndMemberCodeSearchWork()
    {
        Assert.Equal(first.Id, Assert.Single((await service.GetMembersAsync(new() { Search = "aLpHa" })).Items).Id);
        Assert.Equal(first.Id, Assert.Single((await service.GetMembersAsync(new() { Search = "012123" })).Items).Id);
        Assert.Equal(second.Id, Assert.Single((await service.GetMembersAsync(new() { Search = "BETA-QR" })).Items).Id);
        Assert.Equal(2, (await service.GetMembersAsync(new() { Search = "100" })).TotalCount);
        Assert.Equal(second.Id, Assert.Single((await service.GetMembersAsync(new() { ClientCode = 100, RegistrationBranchId = b.Id })).Items).Id);
    }
    [Fact] public async Task FiltersAndPaginationWork()
    {
        Assert.Equal(second.Id, Assert.Single((await service.GetMembersAsync(new() { HouseOnly = true })).Items).Id);
        Assert.Equal(first.Id, Assert.Single((await service.GetMembersAsync(new() { SalesEmployeeId = sales.Id })).Items).Id);
        Assert.Empty((await service.GetMembersAsync(new() { IsActive = false })).Items);
        var page = await service.GetMembersAsync(new() { Page = 2, PageSize = 1 });
        Assert.Equal(2, page.TotalCount); Assert.Equal(second.Id, Assert.Single(page.Items).Id);
    }
    [Fact] public async Task BranchReadsDoNotExpandWithSalesOwnership()
    {
        Scoped("Members.View", "Members.OwnSalesCustomers", "Members.CreateCrossBranch");
        Assert.Equal(first.Id, Assert.Single((await service.GetMembersAsync(new())).Items).Id);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetMembersAsync(new() { RegistrationBranchId = b.Id }));
        await Assert.ThrowsAsync<BranchNotFoundException>(() => service.GetMemberByIdAsync(second.Id));
    }
    [Fact] public async Task DetailsDeriveHouseAndRetainEmployeeLink()
    {
        sales.MemberId = first.Id; await db.SaveChangesAsync();
        var detail = await service.GetMemberByIdAsync(first.Id);
        Assert.False(detail.IsHouse); Assert.Equal(sales.Id, detail.Employee?.Id);
        Assert.True((await service.GetMemberByIdAsync(second.Id)).IsHouse);
    }
    [Fact] public async Task MissingPermissionAndInvalidPaginationFail()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.GetMembersAsync(new() { Page = int.MaxValue }));
        Scoped(); await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetMemberByIdAsync(first.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetMembersAsync(new()));
    }
    private CreateMemberRequest NewMember(Guid? branch = null, string phone = "01299999999") => new()
    {
        FullName = "New Member", PrimaryPhone = phone, TargetRegistrationBranchId = branch ?? a.Id,
        CreationType = MemberCreationType.NormalSales
    };
    [Fact] public async Task NormalSalesCreationOwnsMemberAndCreatesNoPurchase()
    {
        Scoped("Members.Create", "Members.OwnSalesCustomers");
        var result = await service.CreateMemberAsync(NewMember());
        Assert.False(result.IsHouse); Assert.Equal(sales.Id, result.CurrentSalesOwner?.SalesEmployeeId);
        Assert.Equal(101, result.ClientCode); Assert.Equal("01299999999", result.PrimaryPhone);
        Assert.Equal("+201299999999", result.PrimaryPhoneNormalized);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync()); Assert.Empty(await db.SaleTransactions.ToListAsync());
        Assert.Empty(await db.PaymentParts.ToListAsync()); Assert.Empty(await db.CheckIns.ToListAsync());
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "Member.Created");
    }
    [Fact] public async Task CrossBranchReferralKeepsSalesEmployeeAndDoesNotExpandScope()
    {
        Scoped("Members.Create", "Members.View", "Members.OwnSalesCustomers", "Members.CreateCrossBranch");
        var result = await service.CreateMemberAsync(NewMember(b.Id));
        Assert.Equal(b.Id, result.RegistrationBranch.Id); Assert.Equal(101, result.ClientCode);
        Assert.Equal("+441299999999", result.PrimaryPhoneNormalized);
        Assert.Equal(sales.Id, result.CurrentSalesOwner?.SalesEmployeeId);
        Assert.Equal(a.Id, (await db.EmployeeBranchAssignments.SingleAsync()).BranchId);
        await Assert.ThrowsAsync<BranchNotFoundException>(() => service.GetMemberByIdAsync(result.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => access.RequireAsync("Reports.ViewRevenue", b.Id, default));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => access.RequireAsync("Expenses.View", b.Id, default));
    }
    [Fact] public async Task CreationCannotSpoofOwnerOrBypassCrossBranchPermission()
    {
        Scoped("Members.Create", "Members.OwnSalesCustomers");
        var request = NewMember(); request.SalesEmployeeId = sales.Id;
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateMemberAsync(request));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateMemberAsync(NewMember(b.Id)));
        Scoped("Members.Create"); await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateMemberAsync(NewMember()));
    }
    [Fact] public async Task DuplicateAndInactiveBranchRollbackMemberCounterAndAudit()
    {
        Scoped("Members.Create", "Members.OwnSalesCustomers");
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateMemberAsync(NewMember(phone: "+201212345678")));
        Assert.Equal(100, await db.GymBranches.Where(x => x.Id == a.Id).Select(x => x.LastMemberClientCode).SingleAsync());
        Assert.Empty(await db.AuditLogs.ToListAsync());
        await db.GymBranches.Where(x => x.Id == a.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.IsActive, false));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateMemberAsync(NewMember()));
        Assert.Equal(2, await db.Members.CountAsync());
    }
    [Fact] public async Task FreelanceHouseCreatesEligibilityWithoutSalesAssignment()
    {
        var coach = new Employee { FullName = "Freelance Coach", Phone = "coach", EmploymentType = EmploymentType.Freelance, HireDate = access.Today };
        var plan = new MembershipPlan { Name = "Special", CityId = a.CityId, PlanCategory = PlanCategory.Freelance,
            FreelanceCoachEmployee = coach, DurationDays = 30 };
        db.Add(plan); await db.SaveChangesAsync();
        Scoped("Members.Create", "FreelancePlans.ManageEligibleMembers");
        var request = NewMember(); request.CreationType = MemberCreationType.FreelanceHouse; request.FreelancePlanId = plan.Id;
        var result = await service.CreateMemberAsync(request);
        Assert.True(result.IsHouse); Assert.Null(result.CurrentSalesOwner);
        Assert.False(await db.MemberSalesAssignments.AnyAsync(x => x.MemberId == result.Id));
        Assert.Equal(plan.Id, Assert.Single(result.EligibleFreelancePlans).PlanId);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());
    }
    [Fact] public async Task ManagementExplicitlyAssignsOrCreatesHouseWithPermission()
    {
        Scoped("Members.Create", "Members.AssignSales");
        var request = NewMember(); request.CreationType = MemberCreationType.ManagementCreated;
        var house = await service.CreateMemberAsync(request); Assert.True(house.IsHouse);
        request.PrimaryPhone = "01288888888"; request.SalesEmployeeId = sales.Id;
        var owned = await service.CreateMemberAsync(request); Assert.False(owned.IsHouse);
    }
    private UpdateMemberRequest UpdatedProfile(string? phone = null) => new()
        { FullName = "Updated Alpha", PrimaryPhone = phone ?? first.PrimaryPhone, SecondaryPhone = "01222222222", Notes = "Profile note" };
    [Fact] public async Task ProfileUpdatePreservesIdentifiersOwnershipAndLegacyPhone()
    {
        Scoped("Members.Edit"); var result = await service.UpdateMemberAsync(first.Id, UpdatedProfile());
        Assert.Equal(first.Id, result.Id); Assert.Equal(100, result.ClientCode); Assert.Equal(a.Id, result.RegistrationBranch.Id);
        Assert.Equal(first.MemberCode, result.MemberCode); Assert.Equal(sales.Id, result.CurrentSalesOwner?.SalesEmployeeId);
        Assert.Null(result.PrimaryPhoneNormalized); Assert.Equal("Updated Alpha", result.FullName);
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "Member.Updated");
    }
    [Fact] public async Task PhoneChangeRequiresDedicatedPermissionAndResetsVerification()
    {
        first.IsPrimaryPhoneVerified = true; first.PrimaryPhoneVerifiedAt = access.Now; await db.SaveChangesAsync();
        Scoped("Members.Edit");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateMemberAsync(first.Id, UpdatedProfile("01277777777")));
        Scoped("Members.Edit", "Members.ChangePrimaryPhone");
        var result = await service.UpdateMemberAsync(first.Id, UpdatedProfile("01277777777"));
        Assert.Equal("+201277777777", result.PrimaryPhoneNormalized); Assert.False(result.IsPrimaryPhoneVerified);
        Assert.Null((await db.Members.SingleAsync(x => x.Id == first.Id)).PrimaryPhoneVerifiedAt);
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "Member.PrimaryPhoneChanged");
    }
    [Fact] public async Task BranchCountryChangeDoesNotRewriteExistingNormalizedPhone()
    {
        first.PrimaryPhoneNormalized = "+201212345678"; await db.SaveChangesAsync();
        await db.GymBranches.Where(x => x.Id == a.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.CountryCallingCode, "+44"));
        Scoped("Members.Edit"); var result = await service.UpdateMemberAsync(first.Id, UpdatedProfile());
        Assert.Equal("+201212345678", result.PrimaryPhoneNormalized);
    }
    [Fact] public async Task ProfileUpdateDeniesOtherBranchDuplicatesAndMissingPermission()
    {
        Scoped("Members.Edit", "Members.ChangePrimaryPhone");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateMemberAsync(second.Id, UpdatedProfile()));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateMemberAsync(first.Id, UpdatedProfile(second.PrimaryPhone)));
        Scoped("Members.View"); await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateMemberAsync(first.Id, UpdatedProfile()));
    }
    [Theory] [InlineData("Id")] [InlineData("ClientCode")] [InlineData("RegistrationBranchId")] [InlineData("SalesEmployeeId")]
    public void UpdateContractRejectsProtectedFields(string field)
    {
        var json = "{\"FullName\":\"Name\",\"PrimaryPhone\":\"01212345678\",\"" + field + "\":null}";
        Assert.Throws<System.Text.Json.JsonException>(() => System.Text.Json.JsonSerializer.Deserialize<UpdateMemberRequest>(json));
    }
    private sealed class OwnerPermissions : IPermissionService
    {
        public Task<HashSet<string>> GetUserPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(new HashSet<string> { "Members.OwnSalesCustomers" });
    }
}
