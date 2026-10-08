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
        a = new GymBranch { Name = "A", City = new City { Name = "Member city" } };
        b = new GymBranch { Name = "B", City = a.City };
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
        service = new(db, current);
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
    private sealed class OwnerPermissions : IPermissionService
    {
        public Task<HashSet<string>> GetUserPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(new HashSet<string> { "Members.OwnSalesCustomers" });
    }
}
