using System.Net;
using System.Net.Http.Json;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class AuthenticationApiTests
{
    private async Task<(Guid Branch,Guid Other,Guid Employee,Guid Cash,Guid Insta,Guid Category)> DrawerHttpFixture(bool configure=true)
    {
        var f=await BranchFixture();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cash=new FinancialAccount{BranchId=f.Source,Name="Shared physical drawer"};var insta=new FinancialAccount{BranchId=f.Source,Name="Branch InstaPay",AccountType=FinancialAccountType.InstaPay};
        var category=new ExpenseCategory{Name="Dynamic supplies"};db.AddRange(cash,insta,category);await db.SaveChangesAsync();
        if(configure)
        {
            var actor=new TestBranchUser(new(userId,"fixture",null,null,[],[..ShiftPermissions.Codes,"Branches.AccessAll"],new(f.Source,"Branch")));
            var access=new OperationAccess(actor,db,TimeProvider.System,Options.Create(new AuthContextOptions()));
            await new VaultService(db,actor,access).SetupAsync(new(Guid.NewGuid(),cash.Id,1000,"Audited initial physical position"));
        }
        return(f.Source,f.Target,f.Employee,cash.Id,insta.Id,category.Id);
    }
    private async Task GrantShifts(params string[] codes){foreach(var code in codes)await GrantPermission("ConfigurableShiftOperator",code);}
    [Fact] public async Task ShiftRoutesRequireAuthenticationAndActionCapabilities()
    {
        foreach(var route in new[]{"/api/shifts","/api/shifts/current","/api/vault/position","/api/vault/summary","/api/expenses","/api/employee-advances","/api/business-days/current"})
            Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync(route)).StatusCode);
        await Authenticate();
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/shifts",new OpenShiftRequest(Guid.NewGuid(),0))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/vault/audit")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/expenses",new PaidExpenseRequest(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),1,"Supplies"))).StatusCode);
    }
    [Fact] public async Task ReceptionOwnShiftExpenseAdvanceAndCloseHaveScopedExactFinancialEffects()
    {
        var f=await DrawerHttpFixture();await GrantShifts(ShiftPermissions.View,ShiftPermissions.Manage,ShiftPermissions.VaultView,ShiftPermissions.ExpenseCreate,ShiftPermissions.ExpenseView,ShiftPermissions.AdvanceCreate,ShiftPermissions.AdvanceView);await Authenticate();
        var expense=new PaidExpenseRequest(Guid.NewGuid(),f.Category,f.Cash,100,"Supplies");
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/expenses",expense)).StatusCode);
        var shift=(await (await client.PostAsJsonAsync("/api/shifts",new OpenShiftRequest(Guid.NewGuid(),1000))).Content.ReadFromJsonAsync<ShiftDetails>())!;
        var paid=(await (await client.PostAsJsonAsync("/api/expenses",expense)).Content.ReadFromJsonAsync<ExpenseDetails>())!;
        await using(var scope=factory.Services.CreateAsyncScope()){var context=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();await AdvanceSalaryFixture.ConfigureAsync(context,f.Branch,userId,DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo")).DateTime),f.Employee);}
        var advance=new AdvanceRequest(Guid.NewGuid(),f.Employee,f.Insta,50,"Advance via InstaPay");
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/employee-advances",advance)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/expenses",expense with{OperationId=Guid.NewGuid(),BranchId=f.Other})).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync($"/api/shifts?branchId={f.Other}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.GetAsync("/api/vault/movements?pageSize=101")).StatusCode);
        var summary=(await client.GetFromJsonAsync<ShiftFinancialSummary>($"/api/shifts/{shift.Id}/summary"))!;
        Assert.Equal(100,summary.Activity.Single(x=>x.Method==FinancialAccountType.Cash).ExpensesPaid);
        Assert.Equal(50,summary.Activity.Single(x=>x.Method==FinancialAccountType.InstaPay).AdvancesPaid);
        var close=new CloseShiftRequest(Guid.NewGuid(),shift.Version,900);
        Assert.Equal(HttpStatusCode.Conflict,(await client.PostAsJsonAsync($"/api/shifts/{shift.Id}/close",close with{ExpectedVersion=0})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync($"/api/shifts/{shift.Id}/close",close)).StatusCode);
        Assert.Equal(paid.Id,(await (await client.PostAsJsonAsync("/api/expenses",expense)).Content.ReadFromJsonAsync<ExpenseDetails>())!.Id);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/employee-advances",advance)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/expenses",expense with{OperationId=Guid.NewGuid()})).StatusCode);
    }
    [Fact] public async Task DirectDenyOverrideBlocksExpenseDespiteRoleGrantAndOpenShift()
    {
        var f=await DrawerHttpFixture();await GrantShifts(ShiftPermissions.Manage,ShiftPermissions.ExpenseCreate);await Authenticate();
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/shifts",new OpenShiftRequest(Guid.NewGuid(),1000))).StatusCode);
        await using(var scope=factory.Services.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var permission=await db.Permissions.SingleAsync(x=>x.Code==ShiftPermissions.ExpenseCreate);
            db.Add(new UserPermissionOverride{UserId=userId,PermissionId=permission.Id,Effect=PermissionOverrideEffect.Deny});await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/expenses",new PaidExpenseRequest(Guid.NewGuid(),f.Category,f.Cash,100,"Supplies"))).StatusCode);
    }
    [Fact] public async Task DrawerSetupAndDiscretionaryWithdrawalRequireExplicitGlobalCapability()
    {
        var f=await DrawerHttpFixture(false);await GrantShifts(ShiftPermissions.Manage,ShiftPermissions.VaultManage,ShiftPermissions.Withdraw);await Authenticate();
        var setup=new SetupDrawerRequest(Guid.NewGuid(),f.Cash,1000,"Audited opening position");
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/vault/setup",setup)).StatusCode);
        await GrantShifts("Branches.AccessAll");
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/vault/setup",setup)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/vault/setup",setup)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/shifts",new OpenShiftRequest(Guid.NewGuid(),1000))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/vault/withdrawals",new WithdrawalRequest(Guid.NewGuid(),1001,"Owner withdrawal"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/vault/withdrawals",new WithdrawalRequest(Guid.NewGuid(),1000,"Owner withdrawal"))).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(2,await db.FinancialTransactions.CountAsync());Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());
    }
    [Theory][InlineData("employeeId")][InlineData("expectedCash")][InlineData("businessDate")]
    public async Task ShiftOpeningRejectsAuthoritativeEmployeeAndComputedCashInput(string field)
    {
        await DrawerHttpFixture();await GrantShifts(ShiftPermissions.Manage);await Authenticate();
        var body=new Dictionary<string,object?>{{"operationId",Guid.NewGuid()},{"openingCash",1000},{field,null}};
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/shifts",body)).StatusCode);
    }
}
