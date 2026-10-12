using System.Net;
using System.Net.Http.Json;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.MonthlyFinance;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class AuthenticationApiTests
{
    [Fact] public async Task MonthlyFinanceApisRequireAuthenticationAndEffectiveActionPermissions()
    {
        foreach(var path in new[]{"/api/financial-months","/api/payroll","/api/maintenance-plans"})
            Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync(path)).StatusCode);
        await DrawerHttpFixture();await Authenticate();
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/financial-months")).StatusCode);
        await GrantPermission("BranchManager",MonthlyFinancePermissions.View);
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/api/financial-months")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/financial-months",new OpenFinancialMonthRequest(Guid.NewGuid(),"Month",DateOnly.FromDateTime(DateTime.Today),DateOnly.FromDateTime(DateTime.Today),"Start"))).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var p=await db.Permissions.SingleAsync(x=>x.Code==MonthlyFinancePermissions.View);
        db.Add(new UserPermissionOverride{UserId=userId,PermissionId=p.Id,Effect=PermissionOverrideEffect.Deny});await db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/financial-months")).StatusCode);
    }
    [Fact] public async Task MonthlyFinanceBranchManagerCanCloseReopenAndExportWhileOwnerPaymentNeedsGlobalCapability()
    {
        var f=await DrawerHttpFixture();
        foreach(var code in new[]{MonthlyFinancePermissions.Manage,MonthlyFinancePermissions.Close,MonthlyFinancePermissions.Reopen,MonthlyFinancePermissions.View,MonthlyFinancePermissions.Export,MonthlyFinancePermissions.PayrollPay,MonthlyFinancePermissions.PayrollView})
            await GrantPermission("BranchManager",code);
        FinancialMonthDetails month;DateTimeOffset now=DateTimeOffset.UtcNow;
        await using(var scope=factory.Services.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var user=new TestBranchUser(new(userId,"fixture",null,null,[],[..MonthlyFinancePermissions.Codes,"Branches.AccessAll"],new(f.Branch,"Branch")));
            var access=new OperationAccess(user,db,TimeProvider.System,Options.Create(new AuthContextOptions()));
            var employee=await db.Employees.SingleAsync(x=>x.Id==f.Employee);
            await new SalaryService(db,user,access).ConfigureAsync(employee.Id,new(Guid.NewGuid(),6000,employee.HireDate,"Salary"));
            month=await new MonthlyFinanceService(db,user,access).OpenAsync(new(Guid.NewGuid(),"Current day",access.Today,access.Today,"Start"));
        }
        await Authenticate();
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync($"/api/financial-months?branchId={f.Other}")).StatusCode);
        var result=await client.PostAsJsonAsync($"/api/financial-months/{month.Id}/close",new CloseFinancialMonthRequest(Guid.NewGuid(),0,"Close",950));
        Assert.Equal(HttpStatusCode.OK,result.StatusCode);var closed=(await result.Content.ReadFromJsonAsync<FinancialMonthDetails>())!;
        var download=await client.GetAsync($"/api/financial-months/{month.Id}/export");Assert.Equal(HttpStatusCode.OK,download.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",download.Content.Headers.ContentType!.MediaType);Assert.True(download.Headers.Contains("X-Workbook-SHA256"));
        var payroll=(await client.GetFromJsonAsync<CampionsSystem.BLL.Shifts.ShiftPage<PayrollDetails>>("/api/payroll"))!.Items.Single();
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync($"/api/payroll/{payroll.Id}/payments",new RecordPayrollPaymentRequest(Guid.NewGuid(),100,now,"Owner payment"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync($"/api/payroll/{payroll.Id}/payments",new{operationId=Guid.NewGuid(),amount=100,paidAt=now,reason="Payment",financialAccountId=f.Cash})).StatusCode);
        var reopened=await client.PostAsJsonAsync($"/api/financial-months/{month.Id}/reopen",new ReopenFinancialMonthRequest(Guid.NewGuid(),closed.Version,"Investigate"));Assert.Equal(HttpStatusCode.OK,reopened.StatusCode);
        Assert.Equal(await download.Content.ReadAsByteArrayAsync(),await client.GetByteArrayAsync($"/api/financial-months/{month.Id}/export?revision=1"));
    }
    [Fact] public async Task MonthlyFinanceIndividualAllowWorksWithoutChangingRoleGrants()
    {
        await DrawerHttpFixture();
        await using(var scope=factory.Services.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var p=await db.Permissions.SingleAsync(x=>x.Code==MonthlyFinancePermissions.View);
            db.Add(new UserPermissionOverride{UserId=userId,PermissionId=p.Id,Effect=PermissionOverrideEffect.Allow});await db.SaveChangesAsync();
            Assert.False(await db.RolePermissions.AnyAsync(x=>x.PermissionId==p.Id));
        }
        await Authenticate();Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/api/financial-months")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.GetAsync("/api/financial-months?pageSize=101")).StatusCode);
    }
}
