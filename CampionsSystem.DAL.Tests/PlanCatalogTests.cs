using CampionsSystem.BLL.Operations;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class PlanManagementTests
{
    [Fact] public async Task BenefitCatalogManagementUsesUniqueCodeAndNameAndWritesAudit()
    {
        var b=await service.CreateBenefitAsync(new(){Code="INBODY",Name="InBody"});
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CreateBenefitAsync(new(){Code="INBODY",Name="Another"}));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CreateBenefitAsync(new(){Code="OTHER",Name="InBody"}));
        await service.UpdateBenefitAsync(b.Id,new(){Code="BODY",Name="Body scan",Description="Quantity"});
        await service.ChangeBenefitStatusAsync(b.Id,new(){IsActive=false});
        Assert.Equal("BODY",(await service.GetBenefitByIdAsync(b.Id)).Code);
        Assert.Single((await service.GetBenefitsAsync(new(){Search="body",IsActive=false})).Items);
        Assert.Equal(3,await db.AuditLogs.CountAsync(x=>x.EntityId==b.Id));
    }
    [Fact] public async Task BranchManagerCannotChangeSharedBenefits()
    {
        var b=await service.CreateBenefitAsync(new(){Code="SAUNA",Name="Sauna"});Branch("MembershipPlans.View","MembershipPlans.Manage");
        Assert.Single((await service.GetBenefitsAsync(new())).Items);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.UpdateBenefitAsync(b.Id,new(){Code="S",Name="Changed"}));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.ChangeBenefitStatusAsync(b.Id,new(){IsActive=false}));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.CreateBenefitAsync(new(){Code="X",Name="Other"}));
    }
}
