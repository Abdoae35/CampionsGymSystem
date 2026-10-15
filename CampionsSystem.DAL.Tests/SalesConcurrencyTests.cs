using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Npgsql;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesConcurrencyTests : IAsyncLifetime
{
    private readonly string schema="gym_sales_concurrency_"+Guid.NewGuid().ToString("N");
    private string connection=null!;private Guid actor,member,branch,plan,account;
    private ApplicationDbContext Context()=>new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
    private sealed class Clock:TimeProvider {public override DateTimeOffset GetUtcNow()=>new(2026,10,10,10,0,0,TimeSpan.Zero);}
    private (SalesService Sales,CheckInService CheckIns) Services(ApplicationDbContext db)
    {
        var user=new TestBranchUser(new(actor,"concurrency",null,null,[],[..SalesPermissions.Codes,..CheckInPermissions.Codes,"Branches.AccessAll"],new(branch,"Branch")));
        var access=new OperationAccess(user,db,new Clock(),Options.Create(new AuthContextOptions()));var subs=new SubscriptionService(db,user,access);var offers=new OfferService(db,user,access,subs);
        return(new(db,user,access,offers,subs),new(db,user,access,subs));
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
            var user=new ApplicationUser{UserName="sales-concurrent"};actor=user.Id;
            var b=new GymBranch{Name="Branch",City=new(){Name="City"}};branch=b.Id;
            var m=new Member{MemberCode="M-SALES-CON",FullName="Member",PrimaryPhone="sales-con",HomeBranch=b,CreatedByUser=user};member=m.Id;
            var p=new MembershipPlan{Name="Monthly",Branch=b,City=b.City,DurationDays=30,Price=1200};plan=p.Id;
            var cash=new FinancialAccount{Branch=b,Name="Cash",AccountType=FinancialAccountType.Cash};account=cash.Id;
            db.AddRange(m,p,cash);await db.SaveChangesAsync();
            foreach(var name in new[]{"BranchManager","Reception"})
            {
                var role=new IdentityRole<Guid>(name){Id=Guid.NewGuid(),NormalizedName=name.ToUpperInvariant()};
                db.Add(role);db.Add(new IdentityUserRole<Guid>{UserId=actor,RoleId=role.Id});
            }
            await db.SaveChangesAsync();
            await OperationalShiftFixture.OpenAsync(db,actor,branch,new Clock(),cash.Id);
        }
        catch{await DisposeAsync();throw;}
    }
    public async Task DisposeAsync()
    { if(connection==null)return;await using var cn=new NpgsqlConnection(connection);await cn.OpenAsync();await using var command=new NpgsqlCommand($"DROP SCHEMA IF EXISTS {schema} CASCADE",cn);await command.ExecuteNonQueryAsync(); }
    private async Task<SaleReceipt> Buy(Guid op,SubscriptionActivationMode activation=SubscriptionActivationMode.Now)
    {await using var db=Context();return await Services(db).Sales.CreateAsync(new(op,member,plan,PurchaseMode.FullPayment,activation,[new(account,1200)]));}
    private async Task<CheckInResult> Entry(Guid op)
    {await using var db=Context();return await Services(db).CheckIns.CreateAsync(new(op,CheckInMethod.Manual,member));}
    [Fact] public async Task ConcurrentDuplicateSaleHasOnePaymentLedgerAndEntitlement()
    {
        var op=Guid.NewGuid();var results=await Task.WhenAll(Buy(op),Buy(op));Assert.Equal(results[0].SaleId,results[1].SaleId);
        await using var db=Context();Assert.Single(await db.SaleTransactions.ToListAsync());Assert.Single(await db.Payments.ToListAsync());Assert.Single(await db.FinancialTransactions.ToListAsync());
        Assert.Single(await db.MemberSubscriptions.ToListAsync());Assert.Single(await db.MembershipSubscriptionTransactions.ToListAsync());
    }
    [Fact] public async Task ConcurrentFirstEntryRetryCreatesOnlyOneActivationAndEntry()
    {
        await Buy(Guid.NewGuid(),SubscriptionActivationMode.FirstCheckIn);var op=Guid.NewGuid();var results=await Task.WhenAll(Entry(op),Entry(op));Assert.All(results,x=>Assert.True(x.Decision.Allowed));Assert.Equal(results[0].CheckIn,results[1].CheckIn);
        await using var db=Context();Assert.Single(await db.MemberSubscriptions.ToListAsync());Assert.Single(await db.MembershipSubscriptionTransactions.ToListAsync());Assert.Single(await db.CheckIns.ToListAsync());
    }
    [Fact] public async Task ConcurrentPaidPendingRequestsCannotCreateConflictingEntitlements()
    {
        async Task<bool> TryBuy(){try{await Buy(Guid.NewGuid(),SubscriptionActivationMode.FirstCheckIn);return true;}catch(BusinessRuleException){return false;}}
        var results=await Task.WhenAll(TryBuy(),TryBuy());Assert.Single(results,x=>x);
        await using var db=Context();Assert.Single(await db.SaleTransactions.ToListAsync());Assert.Single(await db.Payments.ToListAsync());Assert.Empty(await db.MemberSubscriptions.ToListAsync());
    }
    [Fact] public async Task ConcurrentVisitorPurchasesAndEntriesCannotDuplicatePaidAllowance()
    {
        await using(var db=Context()){var p=await db.MembershipPlans.SingleAsync();p.AccessMode=AccessMode.SessionBased;p.SessionCount=1;await db.SaveChangesAsync();}
        async Task<bool> Visit(){await using var db=Context();try{await Services(db).Sales.CreateAsync(new(Guid.NewGuid(),member,plan,PurchaseMode.StandaloneSingleSession,SubscriptionActivationMode.Now,[new(account,1200)]));return true;}catch(BusinessRuleException){return false;}}
        Assert.Single(await Task.WhenAll(Visit(),Visit()),x=>x);
        var entries=await Task.WhenAll(Entry(Guid.NewGuid()),Entry(Guid.NewGuid()));Assert.Single(entries,x=>x.Decision.Allowed);
        await using var verify=Context();Assert.Single(await verify.SaleTransactions.ToListAsync());Assert.Single(await verify.CheckIns.ToListAsync());Assert.Equal(0,(await verify.MemberSubscriptions.SingleAsync()).RemainingSessions);
    }
    [Fact] public async Task ConcurrentRefundsCannotExceedCollectedMoneyOrRepeatGrantReversal()
    {
        var sale=await Buy(Guid.NewGuid());
        async Task<bool> Refund(){await using var db=Context();try{await Services(db).Sales.RefundAsync(sale.SaleId,new(Guid.NewGuid(),700,[new(account,700)],"Decision"));return true;}catch(BusinessRuleException){return false;}}
        Assert.Single(await Task.WhenAll(Refund(),Refund()),x=>x);await using var verify=Context();
        Assert.Equal(700,await verify.Refunds.SumAsync(r=>r.Amount));Assert.Single(await verify.SubscriptionGrantCancellations.ToListAsync());
    }
    [Fact] public async Task ConcurrentDuplicateRefundProducesOneHistoricalReversal()
    {
        var sale=await Buy(Guid.NewGuid());var op=Guid.NewGuid();
        async Task<RefundDetails> Refund(){await using var db=Context();return await Services(db).Sales.RefundAsync(sale.SaleId,new(op,1200,[new(account,1200)],"Decision"));}
        var result=await Task.WhenAll(Refund(),Refund());Assert.Equal(result[0].Id,result[1].Id);await using var verify=Context();Assert.Single(await verify.Refunds.ToListAsync());Assert.Single(await verify.SubscriptionGrantCancellations.ToListAsync());
    }
    [Fact] public async Task QuickCancelAndCheckInSerializeWithoutLosingHistoricalUse()
    {
        var sale=await Buy(Guid.NewGuid());
        async Task<bool> Cancel(){await using var db=Context();try{await Services(db).Sales.QuickCancelAsync(sale.SaleId,new(Guid.NewGuid(),"Mistake",1200,[new(account,1200)]));return true;}catch(BusinessRuleException){return false;}}
        var cancel=Cancel();var entry=Entry(Guid.NewGuid());await Task.WhenAll(cancel,entry);
        Assert.True(await cancel);await using var verify=Context();
        Assert.Single(await verify.Refunds.ToListAsync());Assert.Equal(1200,await verify.Refunds.SumAsync(r=>r.Amount));
        Assert.Equal((await entry).Decision.Allowed?1:0,await verify.CheckIns.CountAsync());
        Assert.NotNull((await verify.SaleTransactions.SingleAsync()).CancelledAt);
    }
}
