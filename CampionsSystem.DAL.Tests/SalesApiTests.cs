using System.Net;
using System.Net.Http.Json;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.CheckIns;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace CampionsSystem.DAL.Tests;
public sealed partial class AuthenticationApiTests
{
    private async Task<(Guid Member,Guid Plan,Guid Account,Guid Branch,Guid Other)> SalesHttpFixture()
    {
        var f=await BranchFixture();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var city=await db.GymBranches.Where(b=>b.Id==f.Source).Select(b=>b.CityId).SingleAsync();
        var member=new Member{MemberCode="M-HTTP-SALE",FullName="Buyer",PrimaryPhone="http-buyer",HomeBranchId=f.Target,CreatedByUserId=userId};
        var plan=new MembershipPlan{Name="Monthly",BranchId=f.Source,CityId=city,DurationDays=30,Price=1200};
        var account=new FinancialAccount{BranchId=f.Source,Name="Cash",AccountType=FinancialAccountType.Cash};db.AddRange(member,plan,account);await db.SaveChangesAsync();
        await OperationalShiftFixture.OpenAsync(db,userId,f.Source,accountId:account.Id);
        return(member.Id,plan.Id,account.Id,f.Source,f.Target);
    }
    [Fact] public async Task SalesHttpRequiresCapabilitiesAndRejectsCrossBranchSpoofing()
    {
        var f=await SalesHttpFixture();var r=new CreateSaleRequest(Guid.NewGuid(),f.Member,f.Plan,PurchaseMode.FullPayment,SubscriptionActivationMode.Now,[new(f.Account,1200)]);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync("/api/sales",r)).StatusCode);await Authenticate();Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/sales",r)).StatusCode);
        await GrantPermission("Finance",SalesPermissions.Create);await GrantPermission("Finance","Members.OwnSalesCustomers");
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/sales",r with{BranchId=f.Other})).StatusCode);
        var response=await client.PostAsJsonAsync("/api/sales",r);Assert.Equal(HttpStatusCode.OK,response.StatusCode);var sale=(await response.Content.ReadFromJsonAsync<SaleReceipt>())!;
        Assert.Equal(sale.SaleId,(await (await client.PostAsJsonAsync("/api/sales",r)).Content.ReadFromJsonAsync<SaleReceipt>())!.SaleId);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync($"/api/sales/{sale.SaleId}")).StatusCode);
        await GrantPermission("Finance",SalesPermissions.View);var details=(await client.GetFromJsonAsync<SaleDetails>($"/api/sales/{sale.SaleId}"))!;Assert.Equal(f.Branch,details.Sale.BranchId);Assert.Equal("Active",details.ActivationStatus);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync($"/api/sales?branchId={f.Other}")).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Assert.Equal(f.Other,(await db.Members.SingleAsync()).HomeBranchId);
        Assert.Single(await db.Payments.ToListAsync());Assert.Single(await db.MemberSubscriptions.ToListAsync());Assert.Empty(await db.MemberSalesAssignments.ToListAsync());
    }
    [Theory][InlineData("finalPrice")][InlineData("salesEmployeeId")][InlineData("commissionEligible")][InlineData("remainingSessions")]
    public async Task SalesHttpRejectsAuthoritativePriceOwnerAndEntitlementFacts(string field)
    {
        var f=await SalesHttpFixture();await GrantPermission("Finance",SalesPermissions.Create);await Authenticate();
        var body=new Dictionary<string,object?>{{"operationId",Guid.NewGuid()},{"memberId",f.Member},{"membershipPlanId",f.Plan},{"purchaseMode",0},{"activationMode",0},{"paymentParts",new[]{new{financialAccountId=f.Account,amount=1200}}},{field,null}};
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/sales",body)).StatusCode);
    }
    [Fact] public async Task SaleHttpPaymentCancellationAndQueriesHaveIndependentCapabilities()
    {
        var f=await SalesHttpFixture();await GrantPermission("Finance",SalesPermissions.Create);await Authenticate();
        var sale=(await (await client.PostAsJsonAsync("/api/sales",new CreateSaleRequest(Guid.NewGuid(),f.Member,f.Plan,PurchaseMode.Reservation,SubscriptionActivationMode.Now,[new(f.Account,200)]))).Content.ReadFromJsonAsync<SaleReceipt>())!;
        var payment=new RecordPaymentRequest(Guid.NewGuid(),1000,[new(f.Account,1000)]);Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync($"/api/sales/{sale.SaleId}/payments",payment)).StatusCode);
        var refund=new RefundRequest(Guid.NewGuid(),100,[new(f.Account,100)],"Manual decision");Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync($"/api/sales/{sale.SaleId}/refunds",refund)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync($"/api/sales/{sale.SaleId}/quick-cancel",new QuickCancelRequest(Guid.NewGuid(),"Mistake"))).StatusCode);
        await GrantPermission("Finance",SalesPermissions.Refund);Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync($"/api/sales/{sale.SaleId}/refunds",refund)).StatusCode);
        await GrantPermission("Finance",SalesPermissions.View);
        Assert.Equal(2,(await client.GetFromJsonAsync<PaymentHistory>($"/api/sales/{sale.SaleId}/payments"))!.TotalCount);
        Assert.Single((await client.GetFromJsonAsync<RefundHistory>($"/api/sales/{sale.SaleId}/refunds"))!.Items);
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync($"/api/sales/{sale.SaleId}/installments")).StatusCode);
        Assert.Equal(1100,(await client.GetFromJsonAsync<BranchFinancialTotals>("/api/sales/branch-totals"))!.NetCollected);
    }
    [Fact] public async Task OfferHttpConfigurationAndApplicationRequireExplicitEligibility()
    {
        var f=await SalesHttpFixture();await GrantPermission("Finance",SalesPermissions.Create);await Authenticate();
        var r=new OfferRequest("Summer",OfferType.FixedDiscount,200,[f.Plan],OfferEligibilityMode.MemberOnly,StartDate:BranchToday,EndDate:BranchToday.AddDays(1));
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/offers",r)).StatusCode);await GrantPermission("Offers",SalesPermissions.Offers);
        var offer=(await (await client.PostAsJsonAsync("/api/offers",r)).Content.ReadFromJsonAsync<OfferDetails>())!;
        var quote=$"/api/offers/quote?memberId={f.Member}&membershipPlanId={f.Plan}&offerId={offer.Id}";Assert.Equal(HttpStatusCode.BadRequest,(await client.GetAsync(quote)).StatusCode);
        var grant=(await (await client.PostAsJsonAsync($"/api/offers/{offer.Id}/member-entitlements",new GrantMemberOfferRequest(f.Member))).Content.ReadFromJsonAsync<MemberOfferGrantDetails>())!;
        Assert.Equal(1000,(await client.GetFromJsonAsync<PricingResult>(quote))!.FinalPrice);
        Assert.Single((await client.GetFromJsonAsync<OfferHistory>("/api/offers/management"))!.Items);
        Assert.Equal(HttpStatusCode.OK,(await client.DeleteAsync($"/api/offers/{offer.Id}/member-entitlements/{grant.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync($"/api/offers/{offer.Id}",r with{EligibilityMode=OfferEligibilityMode.General})).StatusCode);
        Assert.Single((await client.GetFromJsonAsync<List<OfferDetails>>($"/api/offers?memberId={f.Member}&membershipPlanId={f.Plan}"))!);
    }
    [Fact] public async Task AccountProvisioningRequiresGlobalManagementAndCannotInjectOpeningMoney()
    {
        var f=await SalesHttpFixture();await GrantPermission("Accounts","Branches.Manage");await Authenticate();var r=new CreateFinancialAccountRequest("InstaPay",FinancialAccountType.InstaPay);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/financial-accounts",r)).StatusCode);await GrantPermission("Accounts","Branches.AccessAll");
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/financial-accounts",new{name="InstaPay",method=1,openingBalance=1000})).StatusCode);
        var account=(await (await client.PostAsJsonAsync("/api/financial-accounts",r)).Content.ReadFromJsonAsync<FinancialAccountDetails>())!;Assert.Equal(f.Branch,account.BranchId);
        Assert.Equal(account.Id,(await (await client.PostAsJsonAsync("/api/financial-accounts",r)).Content.ReadFromJsonAsync<FinancialAccountDetails>())!.Id);
        await GrantPermission("Accounts",SalesPermissions.View);Assert.Equal(2,(await client.GetFromJsonAsync<List<FinancialAccountDetails>>("/api/financial-accounts"))!.Count);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Assert.All(await db.FinancialAccounts.ToListAsync(),a=>Assert.Equal(0,a.OpeningBalance));Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task InstallmentCreationCannotUseOnlyNormalPaymentPermission()
    {
        var f=await SalesHttpFixture();await GrantPermission("Finance",SalesPermissions.Create);await Authenticate();
        var r=new CreateSaleRequest(Guid.NewGuid(),f.Member,f.Plan,PurchaseMode.Installment,SubscriptionActivationMode.Now,[new(f.Account,200)],Installments:[new(1000,BranchToday.AddDays(5))]);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/sales",r)).StatusCode);await GrantPermission("Finance",SalesPermissions.Installments);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/sales",r)).StatusCode);
    }
}
