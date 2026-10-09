using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Members;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace CampionsSystem.DAL.Tests;
public sealed partial class AuthenticationApiTests
{
    private async Task GrantCrm(params string[] codes){foreach(var code in codes)await GrantPermission("ConfigurableCrmOperator",code);}
    private async Task<LeadDetails> CreateHttpLead(Guid branch,string phone="+201212345678")
    {
        using var response=await client.PostAsJsonAsync("/api/leads",new{branchId=branch,fullName="HTTP Prospect",primaryPhone=phone});
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);return (await response.Content.ReadFromJsonAsync<LeadCreationResult>())!.Lead!;
    }
    [Fact] public async Task CrmEndpointsRequireAuthenticationAndActionPermissions()
    {
        foreach(var route in new[]{"/api/leads","/api/leads/my","/api/crm/my-follow-ups","/api/crm/house-follow-ups"})
        {using var r=await client.GetAsync(route);Assert.Equal(HttpStatusCode.Unauthorized,r.StatusCode);}
        await Authenticate();
        foreach(var route in new[]{"/api/leads","/api/leads/my","/api/crm/my-follow-ups","/api/crm/house-follow-ups"})
        {using var r=await client.GetAsync(route);Assert.Equal(HttpStatusCode.Forbidden,r.StatusCode);}
        foreach(var route in new[]{"/api/leads",$"/api/leads/{Guid.NewGuid()}/assign",$"/api/leads/{Guid.NewGuid()}/convert",$"/api/crm/member-follow-ups/{Guid.NewGuid()}/complete"})
        {using var r=await client.PostAsJsonAsync(route,new{});Assert.Equal(HttpStatusCode.Forbidden,r.StatusCode);}
    }
    [Fact] public async Task CrmManagerDistributesContactsAndChangesLifecycleWithHistory()
    {
        var data=await BranchFixture();await GrantCrm("Leads.View","Leads.Manage","Leads.Assign","Members.OwnSalesCustomers");await Authenticate();
        var l=await CreateHttpLead(data.Source);
        using var assignment=await client.PostAsJsonAsync($"/api/leads/{l.Id}/assign",new{salesEmployeeId=data.Employee});Assert.Equal(HttpStatusCode.OK,assignment.StatusCode);
        using var contact=await client.PostAsJsonAsync($"/api/leads/{l.Id}/interactions",new{interactionType="WhatsApp",status="FollowUp",notes="Interested",nextFollowUpAt=DateTimeOffset.UtcNow.AddDays(2)});
        Assert.Equal(HttpStatusCode.OK,contact.StatusCode);
        foreach(var route in new[]{$"/api/leads/{l.Id}",$"/api/leads/{l.Id}/assignment",$"/api/leads/{l.Id}/assignment-history",$"/api/leads/{l.Id}/interactions","/api/leads/my?followUp=Upcoming"})
        {using var r=await client.GetAsync(route);Assert.Equal(HttpStatusCode.OK,r.StatusCode);}
        using var lost=await client.PatchAsJsonAsync($"/api/leads/{l.Id}/status",new{status="Lost",reason="Retry later"});Assert.Equal(HttpStatusCode.OK,lost.StatusCode);
        using var retry=await client.PatchAsJsonAsync($"/api/leads/{l.Id}/status",new{status="FollowUp",nextFollowUpAt=DateTimeOffset.UtcNow.AddDays(3)});Assert.Equal(HttpStatusCode.OK,retry.StatusCode);
        using var profile=await client.PutAsJsonAsync($"/api/leads/{l.Id}",new{fullName="Updated prospect",primaryPhone=l.PrimaryPhone});Assert.Equal(HttpStatusCode.OK,profile.StatusCode);
        using var other=await client.PostAsJsonAsync("/api/leads",new{branchId=data.Target,fullName="Other",primaryPhone="+441234567890"});Assert.Equal(HttpStatusCode.Forbidden,other.StatusCode);
    }
    [Fact] public async Task CrmBulkReturnsInvalidRowsAndDuplicateResultsWithoutRejectingBatch()
    {
        var data=await BranchFixture();await GrantCrm("Leads.View","Leads.Manage","Leads.Assign");await Authenticate();
        using var r=await client.PostAsJsonAsync("/api/leads/bulk",new object[]{
            new{branchId=data.Source,fullName="Valid",primaryPhone="+201212345678"},
            new{branchId=data.Source,fullName="Duplicate",primaryPhone="+20 (12) 1234-5678"},
            new{branchId=data.Source,primaryPhone="bad"},new{arbitrary="invalid"}});
        Assert.Equal(HttpStatusCode.OK,r.StatusCode);Assert.Equal(new[]{"Created","AlreadyLead","Invalid","Invalid"},(await r.Content.ReadFromJsonAsync<LeadCreationResult[]>())!.Select(x=>x.Result));
        using var lookup=await client.GetAsync($"/api/leads/lookup?branchId={data.Source}&phone=%2B201212345678");Assert.Equal("AlreadyLead",(await lookup.Content.ReadFromJsonAsync<LeadCreationResult>())!.Result);
    }
    [Fact] public async Task CrmSalesCannotSeeOthersOrSpoofAssignmentAndConversionOwner()
    {
        var data=await BranchFixture();await GrantCrm("Leads.View","Leads.Manage","Members.OwnSalesCustomers","Leads.Convert","Members.Create");await Authenticate();
        var l=await CreateHttpLead(data.Source);Assert.Equal(data.Employee,l.CurrentAssignment?.SalesEmployeeId);
        using var assign=await client.PostAsJsonAsync($"/api/leads/{l.Id}/assign",new{salesEmployeeId=data.Employee});Assert.Equal(HttpStatusCode.Forbidden,assign.StatusCode);
        using var spoof=await client.PostAsJsonAsync($"/api/leads/{l.Id}/convert",new{targetRegistrationBranchId=data.Source,fullName=l.FullName,primaryPhone=l.PrimaryPhone,salesEmployeeId=Guid.NewGuid()});Assert.Equal(HttpStatusCode.BadRequest,spoof.StatusCode);
        await using(var scope=factory.Services.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var other=new ApplicationUser{UserName="other-crm"};
            var employee=new Employee{FullName="Other",Phone="other",User=other,HireDate=BranchToday};
            var foreign=new Lead{FullName="Private",Phone="+441234567890",PhoneNormalized="+441234567890",BranchId=data.Target,CreatedByUserId=userId};
            db.AddRange(employee,foreign,new LeadAssignment{Lead=foreign,SalesEmployee=employee,AssignedByUserId=userId,AssignedAt=DateTimeOffset.UtcNow});await db.SaveChangesAsync();
            using var denied=await client.GetAsync($"/api/leads/{foreign.Id}");Assert.Equal(HttpStatusCode.NotFound,denied.StatusCode);
            using var duplicate=await client.GetAsync($"/api/leads/lookup?branchId={data.Source}&phone=%2B441234567890");
            var exists=(await duplicate.Content.ReadFromJsonAsync<LeadCreationResult>())!;Assert.Equal("AlreadyLead",exists.Result);Assert.Null(exists.Lead);
        }
        using var mine=await client.GetAsync("/api/leads/my");Assert.Equal(l.Id,Assert.Single((await mine.Content.ReadFromJsonAsync<LeadListResult>())!.Items).Id);
    }
    [Fact] public async Task CrmCrossBranchConversionAndFollowUpDoNotExpandMemberBrowsing()
    {
        var data=await BranchFixture();await GrantCrm("Leads.View","Leads.Manage","Leads.Convert","Members.Create","Members.OwnSalesCustomers","Members.View",
            "CRM.ViewMemberFollowUps","CRM.ManageMemberFollowUps");await Authenticate();var l=await CreateHttpLead(data.Source);
        var request=new{targetRegistrationBranchId=data.Target,fullName=l.FullName,primaryPhone=l.PrimaryPhone};
        using var noReferral=await client.PostAsJsonAsync($"/api/leads/{l.Id}/convert",request);Assert.Equal(HttpStatusCode.Forbidden,noReferral.StatusCode);
        await GrantCrm("Members.CreateCrossBranch");using var converted=await client.PostAsJsonAsync($"/api/leads/{l.Id}/convert",request);Assert.Equal(HttpStatusCode.OK,converted.StatusCode);
        var m=(await converted.Content.ReadFromJsonAsync<MemberDetails>())!;Assert.Equal(data.Employee,m.CurrentSalesOwner?.SalesEmployeeId);Assert.Equal(data.Target,m.RegistrationBranch.Id);
        using var work=await client.PostAsJsonAsync($"/api/crm/members/{m.Id}/follow-ups",new{reason="Reactivation",dueAt=DateTimeOffset.UtcNow});Assert.Equal(HttpStatusCode.OK,work.StatusCode);
        var id=await work.Content.ReadFromJsonAsync<Guid>();using var queue=await client.GetAsync("/api/crm/my-follow-ups");Assert.Equal(m.Id,Assert.Single((await queue.Content.ReadFromJsonAsync<FollowUpListResult>())!.Items).MemberId);
        using var browse=await client.GetAsync($"/api/members/{m.Id}");Assert.Equal(HttpStatusCode.NotFound,browse.StatusCode);
        using var broad=await client.GetAsync($"/api/branches/{data.Target}/employees");Assert.Equal(HttpStatusCode.Forbidden,broad.StatusCode);
        using var completed=await client.PostAsync($"/api/crm/member-follow-ups/{id}/complete",null);Assert.Equal(HttpStatusCode.OK,completed.StatusCode);
    }
    [Fact] public async Task CrmStatusAndProfileRejectMissingOrProtectedFields()
    {
        var data=await BranchFixture();await GrantCrm("Leads.View","Leads.Manage","Leads.Assign");await Authenticate();var l=await CreateHttpLead(data.Source);
        using var missing=await client.PatchAsJsonAsync($"/api/leads/{l.Id}/status",new{});Assert.Equal(HttpStatusCode.BadRequest,missing.StatusCode);
        foreach(var field in new[]{"branchId","salesEmployeeId","convertedMemberId","status","phoneNormalized"})
        {
            using var r=await client.PutAsJsonAsync($"/api/leads/{l.Id}",new Dictionary<string,object?>{{"fullName","Spoof"},{"primaryPhone",l.PrimaryPhone},{field,null}});
            Assert.Equal(HttpStatusCode.BadRequest,r.StatusCode);
        }
    }
    [Fact] public async Task CrmPhoneOnlyLeadNeedsFullMemberProfileBeforeConversion()
    {
        var data=await BranchFixture();await GrantCrm("Leads.View","Leads.Manage","Leads.Assign","Leads.Convert","Members.Create","Members.OwnSalesCustomers");await Authenticate();
        using var created=await client.PostAsJsonAsync("/api/leads",new{branchId=data.Source,primaryPhone="+201212345678"});Assert.Equal(HttpStatusCode.OK,created.StatusCode);
        var l=(await created.Content.ReadFromJsonAsync<LeadCreationResult>())!.Lead!;Assert.Null(l.FullName);
        using var assigned=await client.PostAsJsonAsync($"/api/leads/{l.Id}/assign",new{salesEmployeeId=data.Employee});Assert.Equal(HttpStatusCode.OK,assigned.StatusCode);
        using var missing=await client.PostAsJsonAsync($"/api/leads/{l.Id}/convert",new{targetRegistrationBranchId=data.Source,primaryPhone=l.PrimaryPhone});Assert.Equal(HttpStatusCode.BadRequest,missing.StatusCode);
        using var valid=await client.PostAsJsonAsync($"/api/leads/{l.Id}/convert",new{targetRegistrationBranchId=data.Source,primaryPhone=l.PrimaryPhone,fullName="Actual customer name"});Assert.Equal(HttpStatusCode.OK,valid.StatusCode);
    }

    [Fact] public async Task AuthorizedRegistrationOperatorConvertsForeignLeadWithoutBrowsingOrAssignmentGrants()
    {
        var data=await BranchFixture();await GrantCrm("Leads.View","Leads.Convert","Members.Create","Members.View");await Authenticate();
        Guid leadId,ownerId;
        await using(var scope=factory.Services.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user=new ApplicationUser{UserName="absent-original-owner"};
            var users=scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            Assert.True((await users.CreateAsync(user,password)).Succeeded);
            var owner=new Employee{UserId=user.Id,FullName="Ahmed",Phone="absent-sales",HireDate=BranchToday};ownerId=owner.Id;
            var permission=new Permission{Code="Members.OwnSalesCustomers",Name="Own",Module="Members"};
            var lead=new Lead{BranchId=data.Target,FullName="Walk-in",Phone="+201212345678",PhoneNormalized="+201212345678",CreatedByUserId=userId};leadId=lead.Id;
            db.AddRange(owner,permission,lead,new EmployeeBranchAssignment{Employee=owner,BranchId=data.Target,StartDate=BranchToday,IsPrimary=true,AssignedByUserId=userId},
                new UserPermissionOverride{UserId=user.Id,Permission=permission,Effect=CampionsSystem.DAL.Enums.PermissionOverrideEffect.Allow},
                new LeadAssignment{Lead=lead,SalesEmployee=owner,AssignedAt=DateTimeOffset.UtcNow,AssignedByUserId=userId});
            await db.SaveChangesAsync();
        }
        using(var lookup=await client.GetAsync($"/api/leads/lookup?branchId={data.Source}&phone=%2B201212345678"))
        {
            Assert.Equal(HttpStatusCode.OK,lookup.StatusCode);var reference=(await lookup.Content.ReadFromJsonAsync<LeadCreationResult>())!;
            Assert.Equal(leadId,reference.ExistingLeadId);Assert.Null(reference.Lead);
        }
        using(var hidden=await client.GetAsync($"/api/leads/{leadId}"))Assert.Equal(HttpStatusCode.NotFound,hidden.StatusCode);
        using(var assign=await client.PostAsJsonAsync($"/api/leads/{leadId}/assign",new{salesEmployeeId=data.Employee}))Assert.Equal(HttpStatusCode.Forbidden,assign.StatusCode);
        using var converted=await client.PostAsJsonAsync($"/api/leads/{leadId}/convert",new{targetRegistrationBranchId=data.Source,fullName="Walk-in",primaryPhone="+201212345678"});
        Assert.Equal(HttpStatusCode.OK,converted.StatusCode);var member=(await converted.Content.ReadFromJsonAsync<MemberDetails>())!;
        Assert.Equal(ownerId,member.CurrentSalesOwner?.SalesEmployeeId);Assert.Equal(data.Source,member.RegistrationBranch.Id);
        await using(var scope=factory.Services.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(userId,(await db.Members.SingleAsync(x=>x.Id==member.Id)).CreatedByUserId);
            Assert.Equal(userId,(await db.AuditLogs.SingleAsync(x=>x.Action=="Lead.Converted")).UserId);
            Assert.False(await db.MemberSalesAssignments.AnyAsync(x=>x.SalesEmployeeId==data.Employee));
        }
        using var hiddenAfter=await client.GetAsync($"/api/leads/{leadId}");Assert.Equal(HttpStatusCode.NotFound,hiddenAfter.StatusCode);
        using var otherBranch=await client.GetAsync($"/api/branches/{data.Target}/employees");Assert.Equal(HttpStatusCode.Forbidden,otherBranch.StatusCode);
    }

}
