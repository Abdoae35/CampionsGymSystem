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
    [Fact] public async Task StatusPreservesOwnershipEmployeeInvitationAndCheckInHistory()
    {
        sales.MemberId = first.Id;
        var invitation = new GuestInvitation { InviteeMemberId = first.Id, InviteeName = first.FullName,
            InviteePhone = first.PrimaryPhone, InviteePhoneNormalized = "+201212345678", BranchId = a.Id,
            Source = GuestInvitationSource.Gym, CreatedByUserId = current.UserId };
        var checkIn = new CheckIn { MemberId = first.Id, BranchId = a.Id, CheckInType = CheckInType.Trial,
            CheckedInAt = access.Now, CreatedByUserId = current.UserId!.Value };
        db.AddRange(invitation, checkIn); await db.SaveChangesAsync();
        Scoped("Members.ChangeStatus");
        var inactive = await service.ChangeMemberStatusAsync(first.Id, new() { IsActive = false, Reason = "Inactive" });
        Assert.False(inactive.IsActive); Assert.False(inactive.IsHouse); Assert.Equal(sales.Id, inactive.Employee?.Id);
        Assert.Equal(invitation.Id, (await db.GuestInvitations.SingleAsync()).Id);
        Assert.Equal(checkIn.Id, (await db.CheckIns.SingleAsync()).Id);
        Assert.Single(await db.MemberSalesAssignments.ToListAsync());
        Assert.True((await service.ChangeMemberStatusAsync(first.Id, new() { IsActive = true })).IsActive);
        Assert.Equal(2, await db.AuditLogs.CountAsync(x => x.Action == "Member.StatusChanged"));
    }
    [Fact] public async Task StatusRejectsMissingStatusNoOpWrongBranchAndMissingPermission()
    {
        Scoped("Members.ChangeStatus");
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.ChangeMemberStatusAsync(first.Id, new()));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.ChangeMemberStatusAsync(first.Id, new() { IsActive = true }));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ChangeMemberStatusAsync(second.Id, new() { IsActive = false }));
        Scoped("Members.Edit");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ChangeMemberStatusAsync(first.Id, new() { IsActive = false }));
    }
    [Fact] public async Task HouseAssignmentReassignmentAndReturnPreserveHistory()
    {
        var otherUser = new ApplicationUser { UserName = "other-sales" };
        var other = new Employee { FullName = "Other Sales", User = otherUser, Phone = "other-sales", HireDate = access.Today };
        db.Add(other); await db.SaveChangesAsync();
        Scoped("Members.AssignSales", "Members.View", "Members.ViewSalesHistory", "Branches.AccessAll");
        var assigned = await service.AssignSalesAsync(second.Id, new() { SalesEmployeeId = sales.Id, Reason = "House distribution" });
        Assert.False(assigned.IsHouse); Assert.Equal(sales.Id, assigned.CurrentSalesOwner?.SalesEmployeeId);
        var reassigned = await service.AssignSalesAsync(second.Id, new() { SalesEmployeeId = other.Id, Reason = "Reassigned" });
        Assert.Equal(other.Id, reassigned.CurrentSalesOwner?.SalesEmployeeId);
        var history = await service.GetSalesAssignmentHistoryAsync(second.Id);
        Assert.Equal(2, history.Count); Assert.Single(history, x => x.EndedAt == null);
        Assert.NotNull(history.Single(x => x.SalesEmployeeId == sales.Id).EndedAt);
        Assert.Equal(other.Id, (await service.GetCurrentSalesAssignmentAsync(second.Id))?.SalesEmployeeId);
        Assert.True((await service.ReturnMemberToHouseAsync(second.Id, new() { Reason = "Return" })).IsHouse);
        Assert.Null(await service.GetCurrentSalesAssignmentAsync(second.Id));
        Assert.Equal(2, (await service.GetSalesAssignmentHistoryAsync(second.Id)).Count);
        Assert.False(await db.MemberSalesAssignments.AnyAsync(x => x.MemberId == second.Id && x.EndedAt == null));
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "Member.SalesReassigned");
        Assert.Contains(await db.AuditLogs.ToListAsync(), x => x.Action == "Member.ReturnedToHouse");
        Assert.Equal(a.Id, (await db.EmployeeBranchAssignments.SingleAsync()).BranchId);
    }
    [Fact] public async Task AssignmentRequiresPermissionScopeAndEligibleActiveSalesAccount()
    {
        Scoped("Members.OwnSalesCustomers");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AssignSalesAsync(first.Id, new() { SalesEmployeeId = sales.Id }));
        Scoped("Members.AssignSales");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AssignSalesAsync(second.Id, new() { SalesEmployeeId = sales.Id }));
        ownerPermissions.Enabled = false;
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.AssignSalesAsync(first.Id, new() { SalesEmployeeId = sales.Id }));
        ownerPermissions.Enabled = true;
        await db.Employees.Where(x => x.Id == sales.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.EmploymentStatus, EmploymentStatus.Suspended));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.AssignSalesAsync(first.Id, new() { SalesEmployeeId = sales.Id }));
        Assert.Single(await db.MemberSalesAssignments.ToListAsync()); Assert.Empty(await db.AuditLogs.ToListAsync());
    }
    [Fact] public async Task HistoryAndReturnRequireTheirPermissions()
    {
        Scoped("Members.View");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetSalesAssignmentHistoryAsync(first.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ReturnMemberToHouseAsync(first.Id, new()));
        Scoped("Members.ViewSalesHistory");
        await Assert.ThrowsAsync<BranchNotFoundException>(() => service.GetSalesAssignmentHistoryAsync(second.Id));
    }
    [Fact] public async Task LegacyHouseRowsArePreservedWhenAssigningSales()
    {
        db.Add(new MemberSalesAssignment { MemberId = second.Id, AssignmentType = AssignmentType.House,
            AssignedAt = access.Now, AssignedByUserId = current.UserId!.Value }); await db.SaveChangesAsync();
        Scoped("Members.AssignSales", "Branches.AccessAll");
        var result = await service.AssignSalesAsync(second.Id, new() { SalesEmployeeId = sales.Id });
        Assert.False(result.IsHouse);
        var rows = await db.MemberSalesAssignments.Where(x => x.MemberId == second.Id).ToListAsync();
        Assert.Equal(2, rows.Count); Assert.NotNull(rows.Single(x => x.AssignmentType == AssignmentType.House).EndedAt);
    }
    [Fact] public async Task DatabaseRejectsTwoActiveAssignments()
    {
        db.Add(new MemberSalesAssignment { MemberId = first.Id, SalesEmployeeId = sales.Id, AssignmentType = AssignmentType.Sales,
            AssignedAt = access.Now, AssignedByUserId = current.UserId!.Value });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    private sealed class OwnerPermissions : IPermissionService
    {
        public bool Enabled { get; set; } = true;
        public Task<HashSet<string>> GetUserPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Enabled ? new HashSet<string> { "Members.OwnSalesCustomers" } : []);
    }
}
