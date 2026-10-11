using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using Npgsql;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CheckInConcurrencyTests:IAsyncLifetime
{
    private readonly string schema="gym_checkin_concurrency_"+Guid.NewGuid().ToString("N");
    private string connection=null!;private Guid actor,member,branch,plan,subscription;
    private ApplicationDbContext Context()=>new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
    private sealed class Clock:TimeProvider {public override DateTimeOffset GetUtcNow()=>new(2026,10,10,10,0,0,TimeSpan.Zero);}
    private (CheckInService CheckIns,SubscriptionService Subscriptions) Services(ApplicationDbContext db)
    {
        var user=new TestBranchUser(new(actor,"concurrency",null,null,[],[..CheckInPermissions.Codes,..SubscriptionPermissions.Codes,"Branches.AccessAll"],new(branch,"Branch")));
        var access=new OperationAccess(user,db,new Clock(),Options.Create(new AuthContextOptions()));var subs=new SubscriptionService(db,user,access);return(new(db,user,access,subs),subs);
    }
    public async Task InitializeAsync()
    {
        var config=new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("GYM_SCHEMA_TEST_CONNECTION")??$"Host=localhost;Database=postgres;Username={Environment.UserName};Timeout=5");
        if(config.Host is not ("localhost" or "127.0.0.1" or "::1"))throw new InvalidOperationException("Concurrency tests require localhost.");
        await using var admin=new NpgsqlConnection(config.ConnectionString);await admin.OpenAsync();await using var command=new NpgsqlCommand($"CREATE SCHEMA {schema}",admin);await command.ExecuteNonQueryAsync();
        config.Pooling=false; // Dedicated disposable schemas must not leave one idle pool per test.
        config.SearchPath=schema;connection=config.ConnectionString;
        try
        {
            await using var db=Context();await db.Database.ExecuteSqlRawAsync(db.GetService<IMigrator>().GenerateScript());
            var user=new ApplicationUser{UserName="checkin-concurrent"};actor=user.Id;
            var b=new GymBranch{Name="Branch",City=new(){Name="City"}};branch=b.Id;
            var m=new Member{MemberCode="M-CONCURRENT",FullName="Member",PrimaryPhone="ci-con",HomeBranch=b,CreatedByUser=user};member=m.Id;
            var p=new MembershipPlan{Name="One session",Branch=b,City=b.City,DurationDays=30,AccessMode=AccessMode.SessionBased,SessionCount=1,Price=1000};plan=p.Id;
            db.AddRange(m,p);await db.SaveChangesAsync();subscription=(await Services(db).Subscriptions.CreateCustomerGrantAsync(new(member,plan,branch,Guid.NewGuid()))).SubscriptionId;
        }
        catch{await DisposeAsync();throw;}
    }
    public async Task DisposeAsync()
    {
        if(connection==null)return;await using var cn=new NpgsqlConnection(connection);await cn.OpenAsync();await using var command=new NpgsqlCommand($"DROP SCHEMA IF EXISTS {schema} CASCADE",cn);await command.ExecuteNonQueryAsync();
    }
    private async Task<CheckInResult> Entry(Guid op,bool borrow=false)
    {await using var db=Context();return await Services(db).CheckIns.CreateAsync(new(op,CheckInMethod.Manual,member,ConfirmBorrow:borrow));}
    [Fact] public async Task ConcurrentFinalSessionHasExactlyOneWinner()
    {
        var results=await Task.WhenAll(Entry(Guid.NewGuid()),Entry(Guid.NewGuid()));Assert.Single(results,x=>x.Decision.Allowed);
        Assert.Equal(AccessReasons.NoSessionsRemaining,Assert.Single(results,x=>!x.Decision.Allowed).Decision.ReasonCode);
        await using var db=Context();Assert.Equal(0,(await db.MemberSubscriptions.SingleAsync()).RemainingSessions);Assert.Single(await db.CheckIns.ToListAsync());
    }
    [Fact] public async Task ConcurrentSameOperationConsumesAndRecordsOnce()
    {
        var op=Guid.NewGuid();var results=await Task.WhenAll(Entry(op),Entry(op));Assert.All(results,x=>Assert.True(x.Decision.Allowed));Assert.Equal(results[0].CheckIn,results[1].CheckIn);
        await using var db=Context();Assert.Single(await db.CheckIns.ToListAsync());Assert.Equal(0,(await db.MemberSubscriptions.SingleAsync()).RemainingSessions);
    }
}
