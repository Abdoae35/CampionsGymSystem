using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CrmManagementTests : IAsyncLifetime
{
    private AuthTestDatabase fixture=null!;
    private ApplicationDbContext db=null!;
    private TestBranchUser current=null!;
    private OperationAccess access=null!;
    private LeadManagementService service=null!;
    private MemberManagementService members=null!;
    private GymBranch a=null!,b=null!;
    private Employee ahmed=null!,mohamed=null!;
    private readonly Capabilities capabilities=new();
    private sealed class Capabilities : IPermissionService
    {
        public bool Eligible=true;
        public Task<HashSet<string>> GetUserPermissionsAsync(Guid id,CancellationToken cancellationToken=default)=>Task.FromResult(
            Eligible?new HashSet<string>{"Members.OwnSalesCustomers"}:[]);
    }
    public async Task InitializeAsync()
    {
        fixture=await AuthTestDatabase.CreateAsync();db=fixture.CreateContext();
        a=new(){Name="A",CountryCallingCode="+20",City=new(){Name="CRM city"}};
        b=new(){Name="B",CountryCallingCode="+44",City=a.City,LastMemberClientCode=100};
        ahmed=new(){FullName="Ahmed",Phone="ahmed",HireDate=new(2020,1,1),User=new(){UserName="ahmed"}};
        mohamed=new(){FullName="Mohamed",Phone="mohamed",HireDate=new(2020,1,1),User=new(){UserName="mohamed"}};
        current=new(new(ahmed.User.Id,"ahmed",null,null,[],[],new(a.Id,a.Name)));
        access=new(current,db,TimeProvider.System,Options.Create(new AuthContextOptions()));
        db.AddRange(a,b,ahmed,mohamed,new EmployeeBranchAssignment{Employee=ahmed,Branch=a,StartDate=new(2020,1,1),IsPrimary=true,AssignedByUser=ahmed.User},
            new EmployeeBranchAssignment{Employee=mohamed,Branch=a,StartDate=new(2020,1,1),IsPrimary=true,AssignedByUser=ahmed.User});
        await db.SaveChangesAsync();members=new(db,current,access,capabilities);service=new(db,current,access,capabilities,members);Manager();
    }
    public async Task DisposeAsync(){await db.DisposeAsync();await fixture.DisposeAsync();}
    private void Manager()=>Set(ahmed,"Leads.View","Leads.Manage","Leads.Assign","Leads.Convert","Members.Create","Members.View","Members.AssignSales","Members.CreateCrossBranch","Branches.AccessAll","CRM.ViewMemberFollowUps","CRM.ManageMemberFollowUps","CRM.ManageHouseFollowUps");
    private void Set(Employee e,params string[] p)=>current.Context=current.Context! with{UserId=e.UserId!.Value,EmployeeId=e.Id,Permissions=p,CurrentBranch=new(a.Id,a.Name)};
    private CreateLeadRequest Request(string phone="01212345678")=>new(){BranchId=a.Id,FullName="Prospect",PrimaryPhone=phone};
    private async Task<LeadDetails> Lead(string phone="01212345678")=>(await service.CreateLeadAsync(Request(phone))).Lead!;
    [Fact] public async Task ManagerCreatesUnassignedLeadWithConfiguredPhone()
    {
        var l=await Lead();Assert.Equal("+201212345678",l.PrimaryPhoneNormalized);Assert.Null(l.CurrentAssignment);
        Assert.Equal("New",l.Status);Assert.Contains(await db.AuditLogs.ToListAsync(),x=>x.Action=="Lead.Created");
    }
    [Fact] public async Task MemberTakesLookupPriorityAndLeadDuplicateIsReported()
    {
        var l=await Lead();Assert.Equal("AlreadyLead",(await service.CreateLeadAsync(Request("+201212345678"))).Result);
        db.Members.Add(new(){FullName="Member",PrimaryPhone=l.PrimaryPhone,PrimaryPhoneNormalized=l.PrimaryPhoneNormalized,
            MemberCode="existing",HomeBranchId=b.Id,CreatedByUserId=ahmed.UserId!.Value});await db.SaveChangesAsync();
        Assert.Equal("AlreadyMember",(await service.CreateLeadAsync(Request())).Result);
        Assert.Equal("AlreadyMember",(await service.LookupPhoneAsync(a.Id,"01212345678")).Result);
    }
    [Fact] public async Task BulkReturnsPerRowResultsWithoutRollingBackValidRows()
    {
        var results=await service.CreateLeadsBulkAsync([Request(),Request("+201212345678"),Request("bad"),Request("01299999999")]);
        Assert.Equal(new[]{"Created","AlreadyLead","Invalid","Created"},results.Select(x=>x.Result));Assert.Equal(2,await db.Leads.CountAsync());
    }
    [Fact] public async Task ListSearchAndBranchIsolationWork()
    {
        var l=await Lead();await Lead("01299999999");
        Assert.Equal(2,(await service.GetLeadsAsync(new(){UnassignedOnly=true})).TotalCount);
        Assert.Single((await service.GetLeadsAsync(new(){Search="+20 12 1234 5678"})).Items);
        Assert.Single((await service.GetLeadsAsync(new(){PageSize=1,Page=2})).Items);
        Set(mohamed,"Leads.View");Assert.Empty((await service.GetLeadsAsync(new())).Items);
        await Assert.ThrowsAsync<BranchNotFoundException>(()=>service.GetLeadByIdAsync(l.Id));
    }
    [Fact] public async Task InaccessibleDuplicateReturnsNoPrivateDetails()
    {
        await Lead();Set(mohamed,"Leads.View","Leads.Manage","Members.OwnSalesCustomers");
        var result=await service.CreateLeadAsync(Request());Assert.Equal("AlreadyLead",result.Result);Assert.Null(result.Lead);Assert.Null(result.ExistingLeadId);
    }
}
