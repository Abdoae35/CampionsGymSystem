using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    private CreateSaleRequest InstallmentPurchase(Guid account,decimal initial=200) => Purchase(new PaymentPartInput(account,initial)) with
        {PurchaseMode=PurchaseMode.Installment,Installments=[new(500,today.AddDays(3)),new(1200-initial-500,today.AddDays(45))],InitialPaymentRequired=200};
    [Fact] public async Task FlexibleScheduleAndPositiveInitialPaymentGrantCompliantAccess()
    {
        Reception();var cash=await Account();var result=await Finance().CreateAsync(InstallmentPurchase(cash.Id));
        var lines=await db.Installments.OrderBy(x=>x.InstallmentNumber).ToListAsync();Assert.Equal(2,lines.Count);Assert.Equal(today.AddDays(45),lines[1].DueDate);
        Assert.Equal(1000,lines.Sum(x=>x.AmountDue));Assert.Equal(200,(await db.SaleTransactions.SingleAsync()).InitialPaymentRequired);
        Assert.True((await Entrance().EvaluateAsync(new(member.Id,null))).Allowed);Assert.Single(await db.MemberSubscriptions.ToListAsync());
    }
    [Fact] public async Task UnpaidInstallmentSaleCannotActivateUntilAgreedPositivePaymentIsReceived()
    {
        var cash=await Account();var result=await Finance().CreateAsync(InstallmentPurchase(cash.Id,0) with{PaymentParts=[]});
        Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.MemberSubscriptions.ToListAsync());var line=await db.Installments.OrderBy(x=>x.InstallmentNumber).FirstAsync();
        await Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)],[new(line.Id,100)]));Assert.Empty(await db.MemberSubscriptions.ToListAsync());
        await Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)],[new(line.Id,100)]));Assert.Single(await db.MemberSubscriptions.ToListAsync());
    }
    [Fact] public async Task OverdueAccessIsDerivedAndPaymentResumesWithoutExtendingEndDate()
    {
        Reception();var cash=await Account();var result=await Finance().CreateAsync(InstallmentPurchase(cash.Id));var end=(await db.MemberSubscriptions.SingleAsync()).EndDate;
        clock.Now=clock.Now.AddDays(4);var denied=await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id));Assert.Equal(AccessReasons.InstallmentOverdue,denied.Decision.ReasonCode);
        var line=await db.Installments.OrderBy(x=>x.InstallmentNumber).FirstAsync();line.AmountPaid=line.AmountDue;line.Status=InstallmentStatus.Paid;await db.SaveChangesAsync();
        Assert.Equal(AccessReasons.InstallmentOverdue,(await Entrance().EvaluateAsync(new(member.Id,null))).ReasonCode);
        await Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),500,[new(cash.Id,500)],[new(line.Id,500)]));Assert.True((await Entrance().EvaluateAsync(new(member.Id,null))).Allowed);
        Assert.Equal(end,(await db.MemberSubscriptions.SingleAsync()).EndDate);Assert.Single(await db.AuditLogs.Where(x=>x.Action=="Installment.OverdueResolved").ToListAsync());
    }
    [Fact] public async Task OnePaymentCanExplicitlySettleMultipleInstallmentsAndRetryDoesNotDuplicate()
    {
        var cash=await Account();var result=await Finance().CreateAsync(InstallmentPurchase(cash.Id));var lines=await db.Installments.OrderBy(x=>x.InstallmentNumber).ToListAsync();
        var r=new RecordPaymentRequest(Guid.NewGuid(),1000,[new(cash.Id,1000)],[new(lines[0].Id,500),new(lines[1].Id,500)]);
        var first=await Finance().RecordPaymentAsync(result.SaleId,r);var retry=await Finance().RecordPaymentAsync(result.SaleId,r);Assert.Equal(first.Id,retry.Id);
        Assert.Equal(2,await db.InstallmentPaymentAllocations.CountAsync());Assert.Equal(1200,await db.Payments.SumAsync(x=>x.Amount));
        Assert.All(await db.Installments.ToListAsync(),x=>Assert.Equal(InstallmentStatus.Paid,x.Status));Assert.Equal(InstallmentAgreementStatus.Completed,(await db.InstallmentAgreements.SingleAsync()).Status);
        Assert.Single(await db.MembershipSubscriptionTransactions.ToListAsync());
    }
    [Fact] public async Task InvalidSchedulesAndMissingPermissionCannotCreateFinanceFacts()
    {
        var cash=await Account();await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().CreateAsync(InstallmentPurchase(cash.Id) with{Installments=[new(999,today)]}));
        user.Context=user.Context! with{Permissions=[SalesPermissions.Create]};await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Finance().CreateAsync(InstallmentPurchase(cash.Id)));
        Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());
    }
    [Fact] public async Task MissingWrongAndOverAllocatedObligationsFailWithoutMoneyChanges()
    {
        var cash=await Account();var result=await Finance().CreateAsync(InstallmentPurchase(cash.Id));var line=await db.Installments.OrderBy(x=>x.InstallmentNumber).FirstAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)])));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)],[new(Guid.NewGuid(),100)])));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),501,[new(cash.Id,501)],[new(line.Id,501)])));
        Assert.Equal(200,await db.Payments.SumAsync(x=>x.Amount));Assert.Empty(await db.InstallmentPaymentAllocations.ToListAsync());
    }
    [Fact] public async Task OverduePendingFirstEntryDoesNotStartUntilObligationsAreCompliant()
    {
        Reception();var cash=await Account();var result=await Finance().CreateAsync(InstallmentPurchase(cash.Id) with{ActivationMode=SubscriptionActivationMode.FirstCheckIn});clock.Now=clock.Now.AddDays(4);
        var denied=await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id));Assert.Equal(AccessReasons.InstallmentOverdue,denied.Decision.ReasonCode);Assert.Empty(await db.MemberSubscriptions.ToListAsync());
        var line=await db.Installments.OrderBy(x=>x.InstallmentNumber).FirstAsync();await Finance().RecordPaymentAsync(result.SaleId,new(Guid.NewGuid(),500,[new(cash.Id,500)],[new(line.Id,500)]));
        var entry=await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id));Assert.True(entry.Decision.Allowed);Assert.Equal(today.AddDays(4),(await db.MemberSubscriptions.SingleAsync()).StartDate);
    }
}
public sealed partial class SalesConcurrencyTests
{
    [Fact] public async Task ConcurrentInstallmentAllocationsCannotOverpaySameObligation()
    {
        Guid sale,line;await using(var db=Context())
        {
            sale=(await Services(db).Sales.CreateAsync(new(Guid.NewGuid(),member,plan,PurchaseMode.Installment,SubscriptionActivationMode.Now,[new(account,200)],Installments:[new(1000,new DateOnly(2026,11,10))]))).SaleId;
            line=(await db.Installments.SingleAsync()).Id;
        }
        async Task<bool> Pay(){await using var db=Context();try{await Services(db).Sales.RecordPaymentAsync(sale,new(Guid.NewGuid(),700,[new(account,700)],[new(line,700)]));return true;}catch(BusinessRuleException){return false;}}
        var results=await Task.WhenAll(Pay(),Pay());Assert.Single(results,x=>x);await using var verify=Context();Assert.Equal(900,await verify.Payments.SumAsync(x=>x.Amount));Assert.Equal(700,await verify.InstallmentPaymentAllocations.SumAsync(x=>x.Amount));
        Assert.Single(await verify.MembershipSubscriptionTransactions.ToListAsync());
    }
}
