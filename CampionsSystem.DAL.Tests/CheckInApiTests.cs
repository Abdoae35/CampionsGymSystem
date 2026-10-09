using System.Net;
using System.Net.Http.Json;
using CampionsSystem.BLL.CheckIns;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace CampionsSystem.DAL.Tests;
public sealed partial class AuthenticationApiTests
{
    [Fact] public async Task CheckInHttpRequiresCapabilityAndCannotSpoofOperationalBranch()
    {
        var f=await SubscriptionHttpFixture(true);var r=new CreateCheckInRequest(Guid.NewGuid(),CheckInMethod.Manual,f.Member);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync("/api/check-ins",r)).StatusCode);await Authenticate();
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/check-ins",r)).StatusCode);
        await GrantPermission("Entrance",CheckInPermissions.Create);await GrantPermission("Entrance","Members.OwnSalesCustomers");
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/check-ins",r with{BranchId=f.Target})).StatusCode);
        var result=(await (await client.PostAsJsonAsync("/api/check-ins",r)).Content.ReadFromJsonAsync<CheckInResult>())!;
        Assert.True(result.Decision.Allowed);Assert.Equal(f.Source,result.CheckIn!.BranchId); // plan ownership in other branch does not prohibit city access
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(f.Source,(await db.Members.SingleAsync()).HomeBranchId);Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());
    }
    [Fact] public async Task CheckInScannerManualRetriesAndDistinctOperationsUseSameAccessRules()
    {
        var f=await SubscriptionHttpFixture();await GrantPermission("Entrance",CheckInPermissions.Create);await Authenticate();
        var scanner=new CreateCheckInRequest(Guid.NewGuid(),CheckInMethod.Scanner,MemberCode:"HTTP-SUB");
        var first=(await (await client.PostAsJsonAsync("/api/check-ins",scanner)).Content.ReadFromJsonAsync<CheckInResult>())!;
        var retry=(await (await client.PostAsJsonAsync("/api/check-ins",scanner)).Content.ReadFromJsonAsync<CheckInResult>())!;Assert.Equal(first.CheckIn,retry.CheckIn);
        Assert.True((await (await client.PostAsJsonAsync("/api/check-ins",new CreateCheckInRequest(Guid.NewGuid(),CheckInMethod.Manual,f.Member))).Content.ReadFromJsonAsync<CheckInResult>())!.Decision.Allowed);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/check-ins",scanner with{Method=CheckInMethod.Manual})).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Assert.Equal(2,await db.CheckIns.CountAsync());Assert.Null((await db.MemberSubscriptions.SingleAsync()).RemainingSessions);
    }
    [Theory][InlineData("remainingSessions")][InlineData("businessDate")][InlineData("subscriptionId")][InlineData("allowed")]
    public async Task CheckInHttpRejectsClientAuthoritativeFacts(string field)
    {
        var f=await SubscriptionHttpFixture();await GrantPermission("Entrance",CheckInPermissions.Create);await Authenticate();
        var body=new Dictionary<string,object?>{{"operationId",Guid.NewGuid()},{"method",0},{"memberId",f.Member},{field,null}};
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/check-ins",body)).StatusCode);
    }
    [Fact] public async Task CheckInHttpBorrowingIsExplicitAndHistoryHasIndependentViewCapability()
    {
        var f=await SubscriptionHttpFixture();await GrantPermission("Entrance",CheckInPermissions.Create);await Authenticate();
        await using(var scope=factory.Services.CreateAsyncScope())
        {var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();await db.MemberSubscriptions.Where(x=>x.Id==f.Subscription).ExecuteUpdateAsync(x=>x.SetProperty(s=>s.StartDate,BranchToday.AddDays(-30)).SetProperty(s=>s.EndDate,BranchToday.AddDays(-1)));}
        var evaluation=(await (await client.PostAsJsonAsync("/api/check-ins/evaluate",new AccessRequest(f.Member,null))).Content.ReadFromJsonAsync<AccessDecision>())!;
        Assert.False(evaluation.Allowed);Assert.True(evaluation.BorrowAvailable);Assert.Equal(2,evaluation.BorrowedRemaining);
        var r=new CreateCheckInRequest(Guid.NewGuid(),CheckInMethod.Manual,f.Member);
        var denied=(await (await client.PostAsJsonAsync("/api/check-ins",r)).Content.ReadFromJsonAsync<CheckInResult>())!;Assert.Null(denied.CheckIn);
        var borrowed=(await (await client.PostAsJsonAsync("/api/check-ins",r with{ConfirmBorrow=true})).Content.ReadFromJsonAsync<CheckInResult>())!;Assert.Equal("BorrowedGym",borrowed.CheckIn!.Type);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/check-ins")).StatusCode);await GrantPermission("Entrance",CheckInPermissions.View);
        Assert.Single((await client.GetFromJsonAsync<CheckInHistory>($"/api/check-ins?memberId={f.Member}"))!.Items);
        Assert.Single((await client.GetFromJsonAsync<CheckInHistory>($"/api/members/{f.Member}/check-ins/current-business-day"))!.Items);
        Assert.Single((await client.GetFromJsonAsync<BorrowedDebtHistory>($"/api/members/{f.Member}/borrowed-gym-sessions"))!.Items);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync($"/api/check-ins?branchId={f.Target}")).StatusCode);
    }
    [Fact] public async Task ReceptionLookupReturnsMinimalIdentityAndTrialRedemptionUsesExistingInvitation()
    {
        var f=await SubscriptionHttpFixture();await GrantPermission("Entrance",CheckInPermissions.Create);await Authenticate();Guid invitation;
        await using(var scope=factory.Services.CreateAsyncScope())
        {var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var i=new GuestInvitation{BranchId=f.Source,InviteeName="Guest",InviteePhone="+201000000099",InviteePhoneNormalized="+201000000099",CreatedAt=DateTimeOffset.UtcNow.AddMinutes(-1)};invitation=i.Id;db.Add(i);await db.SaveChangesAsync();}
        var matches=(await client.GetFromJsonAsync<CheckInMemberMatches>("/api/check-ins/members?search=HTTP-SUB"))!;Assert.Equal(f.Member,matches.Items.Single().Id);
        var request=new RedeemTrialRequest(Guid.NewGuid(),invitation,"+201000000099");
        var first=(await (await client.PostAsJsonAsync("/api/check-ins/trials",request)).Content.ReadFromJsonAsync<TrialCheckInResult>())!;Assert.True(first.Allowed);Assert.Null(first.CheckIn!.MemberId);
        Assert.Equal(first.CheckIn,(await (await client.PostAsJsonAsync("/api/check-ins/trials",request)).Content.ReadFromJsonAsync<TrialCheckInResult>())!.CheckIn);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/check-ins/trials",new{operationId=Guid.NewGuid(),invitationId=invitation,phone="+201000000099",quantityUsed=0})).StatusCode);
    }
}
