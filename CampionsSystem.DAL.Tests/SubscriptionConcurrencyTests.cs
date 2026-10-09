using CampionsSystem.BLL.Authentication;
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
public sealed partial class SubscriptionConcurrencyTests : IAsyncLifetime
{
    private readonly string schema="gym_subscription_concurrency_"+Guid.NewGuid().ToString("N");
    private string connection=null!;
    private Guid actor,member,branch,plan,otherPlan,subscription;
    private ApplicationDbContext Context()=>new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow()=>new(2026,10,10,10,0,0,TimeSpan.Zero); }
    private SubscriptionService Service(ApplicationDbContext db)
    {
        var user=new TestBranchUser(new(actor,"concurrency",null,null,[],[..SubscriptionPermissions.Codes,"Branches.AccessAll"],new(branch,"Branch")));
        return new(db,user,new(user,db,new Clock(),Options.Create(new AuthContextOptions())));
    }
    public async Task InitializeAsync()
    {
        var config=new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("GYM_SCHEMA_TEST_CONNECTION")??$"Host=localhost;Database=postgres;Username={Environment.UserName};Timeout=5");
        if(config.Host is not ("localhost" or "127.0.0.1" or "::1"))throw new InvalidOperationException("Concurrency tests require local PostgreSQL.");
        await using var admin=new NpgsqlConnection(config.ConnectionString);await admin.OpenAsync();
        await using var command=new NpgsqlCommand($"CREATE SCHEMA {schema}",admin);await command.ExecuteNonQueryAsync();
        config.SearchPath=schema;connection=config.ConnectionString;
        try
        {
            await using var db=Context();await db.Database.ExecuteSqlRawAsync(db.GetService<IMigrator>().GenerateScript());
            var user=new ApplicationUser{UserName="subscription-concurrent"};actor=user.Id;
            var b=new GymBranch{Name="Branch",City=new(){Name="City"}};branch=b.Id;
            var m=new Member{MemberCode="SUB-CON",FullName="Member",PrimaryPhone="sub-con",HomeBranch=b,CreatedByUser=user};member=m.Id;
            var p=new MembershipPlan{Name="Unlimited",Branch=b,City=b.City,DurationDays=30,Price=1000,AllowFreeze=true,IncludedFreezeDays=14};plan=p.Id;
            var other=new MembershipPlan{Name="Sessions",Branch=b,City=b.City,DurationDays=30,Price=1000,AccessMode=AccessMode.SessionBased,SessionCount=12};otherPlan=other.Id;
            db.AddRange(m,p,other);await db.SaveChangesAsync();subscription=(await Service(db).CreateCustomerGrantAsync(new(member,plan,branch,Guid.NewGuid()))).SubscriptionId;
        }
        catch{await DisposeAsync();throw;}
    }
    public async Task DisposeAsync()
    {
        if(connection is null)return;
        await using var cn=new NpgsqlConnection(connection);await cn.OpenAsync();
        await using var command=new NpgsqlCommand($"DROP SCHEMA IF EXISTS {schema} CASCADE",cn);await command.ExecuteNonQueryAsync();
    }
    private async Task<bool> AttemptRenew(Guid selectedPlan, Guid operation)
    {
        await using var db=Context();
        try{await Service(db).RenewCustomerAsync(new(subscription,selectedPlan,branch,operation,0));return true;}
        catch(DbUpdateConcurrencyException){return false;}
        catch(BusinessRuleException){return false;}
    }
    [Fact] public async Task IndependentRenewalsCannotDoubleApplyOneObservedVersion()
    {
        Assert.Single(await Task.WhenAll(AttemptRenew(plan,Guid.NewGuid()),AttemptRenew(plan,Guid.NewGuid())),x=>x);
        await using var db=Context();Assert.Equal(2,await db.MembershipSubscriptionTransactions.CountAsync());
        Assert.Equal(new DateOnly(2026,12,8),(await db.MemberSubscriptions.SingleAsync()).EndDate);
    }
    [Fact] public async Task IndependentRequestsCannotCreateConflictingFutureSubscriptions()
    {
        Assert.Single(await Task.WhenAll(AttemptRenew(otherPlan,Guid.NewGuid()),AttemptRenew(otherPlan,Guid.NewGuid())),x=>x);
        await using var db=Context();var rows=await db.MemberSubscriptions.OrderBy(x=>x.StartDate).ToListAsync();Assert.Equal(2,rows.Count);
        Assert.Equal(rows[0].EndDate!.Value.AddDays(1),rows[1].StartDate);Assert.Equal(12,rows[1].RemainingSessions);
    }
    [Fact] public async Task ConcurrentSameOperationIsIdempotent()
    {
        var op=Guid.NewGuid();Assert.All(await Task.WhenAll(AttemptRenew(plan,op),AttemptRenew(plan,op)),Assert.True);
        await using var db=Context();Assert.Equal(2,await db.MembershipSubscriptionTransactions.CountAsync());Assert.Single(await db.MemberSubscriptions.ToListAsync());
    }
}
