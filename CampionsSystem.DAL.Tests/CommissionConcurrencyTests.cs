using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Commissions;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesConcurrencyTests
{
    private CommissionService CommissionServiceFor(DAL.Data.ApplicationDbContext db, TimeProvider? clock = null)
    {
        var user = new TestBranchUser(new(actor,"commission",null,null,[],[CommissionPermissions.View,CommissionPermissions.Manage,CommissionPermissions.Close,"Branches.AccessAll"],new(branch,"Branch")));
        return new(db,user,new(user,db,clock??new Clock(),Options.Create(new AuthContextOptions())));
    }
    [Fact] public async Task CommissionConcurrentFirstPurchasesClassifyOnceUnderMemberLock()
    {
        await Task.WhenAll(Buy(Guid.NewGuid()),Buy(Guid.NewGuid()));await using var db=Context();
        Assert.Single(await db.SaleTransactions.Where(s=>s.SaleCategory==SaleCategory.NewMembership).ToListAsync());
        Assert.Single(await db.SaleTransactions.Where(s=>s.SaleCategory==SaleCategory.Renewal).ToListAsync());Assert.Equal(2,await db.CommissionCollections.CountAsync());
    }
    [Fact] public async Task CommissionConcurrentPeriodRequestsCannotOverlapAndExactRetryIsIdempotent()
    {
        async Task<CommissionPeriodDetails?> Open(Guid op){await using var db=Context();try{return await CommissionServiceFor(db).OpenAsync(new(op,"Period",new(2026,10,10),new(2026,10,23)));}catch(BusinessRuleException){return null;}}
        var operations=new[]{Guid.NewGuid(),Guid.NewGuid()};
        var first=await Task.WhenAll(Open(operations[0]),Open(operations[1]));Assert.Single(first,p=>p!=null);
        var winner=Array.FindIndex(first,p=>p!=null);
        Assert.Equal(first[winner]!.Id,(await Open(operations[winner]))!.Id);
        await using var check=Context();Assert.Single(await check.CommissionPeriods.ToListAsync());
    }
    [Fact] public async Task CommissionConcurrentRefundRetryAllocatesOneRefundExactlyOnce()
    {
        var sale=await Buy(Guid.NewGuid());var op=Guid.NewGuid();
        async Task<RefundDetails> Refund(){await using var db=Context();return await Services(db).Sales.RefundAsync(sale.SaleId,new(op,500,[new(account,500)],"Partial"));}
        var refunds=await Task.WhenAll(Refund(),Refund());Assert.Equal(refunds[0].Id,refunds[1].Id);await using var verify=Context();
        Assert.Single(await verify.CommissionRefundAllocations.ToListAsync());Assert.Equal(500,await verify.CommissionRefundAllocations.SumAsync(x=>x.Amount));
    }
    private sealed class NextDayClock:TimeProvider{public override DateTimeOffset GetUtcNow()=>new(2026,10,11,10,0,0,TimeSpan.Zero);}
    [Fact] public async Task CommissionRefundAndFinalizationSerializeWithoutChangingFinalizedFacts()
    {
        Guid period;
        await using(var db=Context()){period=(await CommissionServiceFor(db).OpenAsync(new(Guid.NewGuid(),"Day",new(2026,10,10),new(2026,10,10)))).Id;}
        var sale=await Buy(Guid.NewGuid());
        async Task<bool> Refund(){await using var db=Context();try{await Services(db).Sales.RefundAsync(sale.SaleId,new(Guid.NewGuid(),500,[new(account,500)],"Partial"));return true;}catch(BusinessRuleException){return false;}}
        async Task Close(){await using var db=Context();await CommissionServiceFor(db,new NextDayClock()).FinalizeAsync(period,new(Guid.NewGuid(),0));}
        var refund=Refund();await Task.WhenAll(refund,Close());await using var check=Context();
        var p=await CommissionServiceFor(check).ProgressAsync(period);Assert.True(p.Finalized);Assert.Equal(await refund?500:0,p.Refunds);
        Assert.Equal(await refund?700:1200,p.HouseCollections);Assert.Single(await check.CommissionStatementLines.ToListAsync());
    }
}
