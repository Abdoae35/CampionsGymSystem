using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Plans;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class PlanManagementTests : IAsyncLifetime
{
    private AuthTestDatabase fixture=null!;
    private ApplicationDbContext db=null!;
    private TestBranchUser current=null!;
    private OperationAccess access=null!;
    private MembershipPlanManagementService service=null!;
    private GymBranch a=null!,b=null!;
    private ApplicationUser actor=null!;
    private void Global()=>current.Context=current.Context! with{Permissions=["MembershipPlans.View","MembershipPlans.Manage","Branches.AccessAll","FreelancePlans.ManageEligibleMembers"],CurrentBranch=new(a.Id,a.Name)};
    private void Branch(params string[] permissions)=>current.Context=current.Context! with{Permissions=permissions,CurrentBranch=new(a.Id,a.Name)};
    public async Task InitializeAsync()
    {
        fixture=await AuthTestDatabase.CreateAsync();db=fixture.CreateContext();actor=new(){UserName="plan-manager"};
        a=new(){Name="A",City=new(){Name="Same city"},CountryCallingCode="+20"};b=new(){Name="B",City=a.City,CountryCallingCode="+20"};db.AddRange(actor,a,b);await db.SaveChangesAsync();
        current=new(new(actor.Id,"manager",null,null,[],[],new(a.Id,a.Name)));Global();
        access=new(current,db,TimeProvider.System,Options.Create(new AuthContextOptions()));service=new(db,current,access);
    }
    public async Task DisposeAsync(){await db.DisposeAsync();await fixture.DisposeAsync();}
    [Fact] public async Task GlobalCategoryManagementPreservesReferencesAndSupportsSearch()
    {
        var c=await service.CreateCategoryAsync(new(){Name="Male"});await service.CreateCategoryAsync(new(){Name="Kids"});
        await service.UpdateCategoryAsync(c.Id,new(){Name="Adults",Description="Audience"});
        await service.ChangeCategoryStatusAsync(c.Id,new(){IsActive=false});
        Assert.Equal(c.Id,Assert.Single((await service.GetCategoriesAsync(new(){Search="adult",IsActive=false})).Items).Id);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CreateCategoryAsync(new(){Name="Adults"}));
        Assert.Single((await service.GetCategoriesAsync(new(){PageSize=1})).Items);
        Assert.Equal(3,await db.AuditLogs.CountAsync(x=>x.EntityId==c.Id));
    }
    [Fact] public async Task BranchManagerMayReadButCannotEditGlobalCatalog()
    {
        var c=await service.CreateCategoryAsync(new(){Name="Kids"});Branch("MembershipPlans.View","MembershipPlans.Manage");
        Assert.Single((await service.GetCategoriesAsync(new())).Items);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.CreateCategoryAsync(new(){Name="Other"}));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.UpdateCategoryAsync(c.Id,new(){Name="Spoof"}));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.ChangeCategoryStatusAsync(c.Id,new(){IsActive=false}));
        Branch("Branches.AccessAll");await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.GetCategoriesAsync(new()));
    }
}
