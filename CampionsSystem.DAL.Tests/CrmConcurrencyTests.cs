using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using Microsoft.Extensions.Options;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

// Committed disposable schema is necessary: independent sessions cannot see transactional fixtures.
public sealed class CrmConcurrencyTests : IAsyncLifetime
{
    private string connection = null!;
    private string schema = "gym_crm_concurrent_" + Guid.NewGuid().ToString("N");
    private Guid a, b, actor, owner;
    private ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
    public async Task InitializeAsync()
    {
        var config = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("GYM_SCHEMA_TEST_CONNECTION")
            ?? $"Host=localhost;Database=postgres;Username={Environment.UserName};Timeout=5");
        await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE SCHEMA {schema}", admin); await command.ExecuteNonQueryAsync();
        config.SearchPath = schema; connection = config.ConnectionString;
        try
        {
            await using var db = Context();
            await db.Database.ExecuteSqlRawAsync(db.GetService<IMigrator>().GenerateScript());
            var user = new ApplicationUser { UserName = "concurrent-member" }; actor = user.Id;
            var first = new GymBranch { Name = "A", City = new City { Name = "Concurrent city" } };
            var second = new GymBranch { Name = "B", City = first.City };
            var sales = new Employee { FullName = "Concurrent Sales", Phone = "sales", UserId = actor, HireDate = new(2026,1,1) };
            owner = sales.Id; a = first.Id; b = second.Id; db.AddRange(user, first, second, sales); await db.SaveChangesAsync();
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }
    public async Task DisposeAsync()
    {
        if (connection is null) return;
        await using var cn = new NpgsqlConnection(connection); await cn.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS {schema} CASCADE", cn); await command.ExecuteNonQueryAsync();
    }
    private sealed class Capabilities : IPermissionService
    {
        public Task<HashSet<string>> GetUserPermissionsAsync(Guid id,CancellationToken cancellationToken=default)
            =>Task.FromResult(new HashSet<string>{"Members.OwnSalesCustomers"});
    }
    private LeadManagementService Service(ApplicationDbContext db)
    {
        var user=new TestBranchUser(new(actor,"concurrent",null,null,[],
            ["Leads.View","Leads.Manage","Leads.Assign","Leads.Convert","Members.Create","Members.AssignSales","Members.OwnSalesCustomers","Branches.AccessAll","CRM.ManageMemberFollowUps"],null));
        var access=new OperationAccess(user,db,TimeProvider.System,Options.Create(new AuthContextOptions()));
        var capability=new Capabilities();var members=new MemberManagementService(db,user,access,capability);
        return new(db,user,access,capability,members);
    }
    private async Task<Guid> PrepareLead()
    {
        await using var db=Context();
        db.EmployeeBranchAssignments.Add(new(){EmployeeId=owner,BranchId=a,StartDate=new(2020,1,1),IsPrimary=true,AssignedByUserId=actor});await db.SaveChangesAsync();
        var service=Service(db);var l=(await service.CreateLeadAsync(new(){BranchId=a,FullName="Concurrent Lead",PrimaryPhone="+201212345678"})).Lead!;
        await service.AssignLeadAsync(l.Id,new(){SalesEmployeeId=owner});return l.Id;
    }
    [Fact] public async Task IndependentSessionsCannotConvertOneLeadTwice()
    {
        var lead=await PrepareLead();
        async Task<bool> Attempt()
        {
            await using var db=Context();
            try{await Service(db).ConvertLeadAsync(lead,new(){TargetRegistrationBranchId=a,FullName="Converted",PrimaryPhone="+201212345678"});return true;}
            catch(BusinessRuleException){return false;}
        }
        var attempts=await Task.WhenAll(Attempt(),Attempt());Assert.Single(attempts,x=>x);
        await using var verify=Context();Assert.Single(await verify.Members.ToListAsync());Assert.Single(await verify.MemberSalesAssignments.ToListAsync());
        Assert.Equal(1,await verify.GymBranches.Where(x=>x.Id==a).Select(x=>x.LastMemberClientCode).SingleAsync());
    }
    [Fact] public async Task ConcurrentLeadCreationReturnsOneCreatedAndOneDuplicate()
    {
        async Task<string> Attempt()
        {
            await using var db=Context();return (await Service(db).CreateLeadAsync(new(){BranchId=a,FullName="Duplicate",PrimaryPhone="+201212345678"})).Result;
        }
        var results=await Task.WhenAll(Attempt(),Attempt());Assert.Contains("Created",results);Assert.Contains("AlreadyLead",results);
        await using var verify=Context();Assert.Single(await verify.Leads.ToListAsync());
    }
    [Fact] public async Task IndependentReassignmentsPreserveOneCurrentAssignment()
    {
        var lead=await PrepareLead();Guid one,two;
        await using(var db=Context())
        {
            var e1=new Employee{FullName="One",Phone="one",User=new(){UserName="one"},HireDate=new(2020,1,1)};
            var e2=new Employee{FullName="Two",Phone="two",User=new(){UserName="two"},HireDate=new(2020,1,1)};
            db.AddRange(e1,e2,new EmployeeBranchAssignment{Employee=e1,BranchId=a,StartDate=new(2020,1,1),IsPrimary=true,AssignedByUserId=actor},
                new EmployeeBranchAssignment{Employee=e2,BranchId=a,StartDate=new(2020,1,1),IsPrimary=true,AssignedByUserId=actor});await db.SaveChangesAsync();one=e1.Id;two=e2.Id;
        }
        async Task Assign(Guid employee){await using var db=Context();await Service(db).AssignLeadAsync(lead,new(){SalesEmployeeId=employee});}
        await Task.WhenAll(Assign(one),Assign(two));
        await using var verify=Context();Assert.Equal(3,await verify.LeadAssignments.CountAsync());Assert.Equal(1,await verify.LeadAssignments.CountAsync(x=>x.EndedAt==null));
    }
    [Fact] public async Task IndependentSessionsCannotDuplicateOpenMemberWork()
    {
        var lead=await PrepareLead();Guid member;
        await using(var db=Context())member=(await Service(db).ConvertLeadAsync(lead,new(){TargetRegistrationBranchId=a,FullName="Member",PrimaryPhone="+201212345678"})).Id;
        async Task<Guid> Open(){await using var db=Context();return await Service(db).OpenMemberFollowUpAsync(member,new(){Reason="Reactivation",DueAt=DateTimeOffset.UtcNow});}
        var ids=await Task.WhenAll(Open(),Open());Assert.Equal(ids[0],ids[1]);
        await using var verify=Context();Assert.Single(await verify.MemberFollowUpTasks.ToListAsync());
    }

}
