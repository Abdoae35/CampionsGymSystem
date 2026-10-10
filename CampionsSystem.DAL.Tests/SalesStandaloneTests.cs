using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    private async Task<CreateSaleRequest> Visitor()
    {
        plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=1;plan.Price=100;await db.SaveChangesAsync();
        var cash=await Account();return Purchase(new PaymentPartInput(cash.Id,100)) with{PurchaseMode=PurchaseMode.StandaloneSingleSession};
    }
    [Fact] public async Task PaidVisitorHasExactlyOneEntryAndNeverCreatesBorrowCycle()
    {
        Reception();var r=await Visitor();var receipt=await Finance().CreateAsync(r);
        var sub=await db.MemberSubscriptions.SingleAsync();Assert.True(sub.IsStandaloneVisit);Assert.Equal(1,sub.RemainingSessions);
        var entry=await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id));Assert.True(entry.Decision.Allowed);Assert.Equal(CheckInType.Gym.ToString(),entry.CheckIn!.Type);
        Assert.Equal(0,entry.CheckIn.SessionsAfter);Assert.False((await Entrance().EvaluateAsync(new(member.Id,null))).BorrowAvailable);
        clock.Now=clock.Now.AddDays(31);Assert.False((await Entrance().EvaluateAsync(new(member.Id,null))).BorrowAvailable);
        Assert.Empty(await db.BorrowedGymSessions.ToListAsync());Assert.Equal(receipt.SaleId,(await db.MembershipSubscriptionTransactions.SingleAsync()).SaleTransactionId);
    }
    [Fact] public async Task VisitorPreservesTwoOldDebtsAndCannotResetBorrowAllowance()
    {
        Reception();var cash=await Account();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));var old=await db.MemberSubscriptions.SingleAsync();clock.Now=clock.Now.AddDays(30);
        for(var i=0;i<2;i++)Assert.True((await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id,ConfirmBorrow:true))).Decision.Allowed);
        await Finance().CreateAsync(await Visitor());Assert.All(await db.BorrowedGymSessions.ToListAsync(),x=>{Assert.Equal(BorrowedSessionStatus.Outstanding,x.Status);Assert.Equal(old.Id,x.ExpiredSubscriptionId);Assert.Null(x.SettledByTransactionId);});
        Assert.True((await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id))).Decision.Allowed);
        var denied=await Entrance().CreateAsync(new(Guid.NewGuid(),CheckInMethod.Manual,member.Id,ConfirmBorrow:true));Assert.False(denied.Decision.Allowed);
        Assert.Equal(AccessReasons.BorrowLimitReached,(await Entrance().EvaluateAsync(new(member.Id,null))).ReasonCode);
        plan.AccessMode=AccessMode.Unlimited;plan.SessionCount=null;plan.Price=1200;await db.SaveChangesAsync();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        Assert.All(await db.BorrowedGymSessions.ToListAsync(),x=>Assert.Equal(BorrowedSessionStatus.Settled,x.Status));
    }
    [Fact] public async Task OwnedStandaloneSalePreservesOwnerAndIsExcludedFromCommissionAndTargets()
    {
        var r=await Visitor();var owner=new Employee{FullName="Owner",Phone="visit-owner",HireDate=today.AddYears(-1)};db.Add(owner);await db.SaveChangesAsync();
        var assignment=new MemberSalesAssignment{MemberId=member.Id,SalesEmployeeId=owner.Id,AssignmentType=AssignmentType.Sales,AssignedAt=access.Now,AssignedByUserId=actor.Id};db.Add(assignment);await db.SaveChangesAsync();
        await Finance().CreateAsync(r);var sale=await db.SaleTransactions.SingleAsync();Assert.Equal(owner.Id,sale.SalesEmployeeId);Assert.False(sale.CommissionEligible);Assert.False(sale.TargetEligible);Assert.Null(sale.SaleCategory);Assert.Null(assignment.EndedAt);
    }
    [Theory][InlineData(SubscriptionActivationMode.FirstCheckIn)][InlineData(SubscriptionActivationMode.SpecificDate)]
    public async Task VisitorHonorsActivationChoiceAndRetry(SubscriptionActivationMode mode)
    {
        Reception();var r=await Visitor();r=r with{ActivationMode=mode,StartDate=mode==SubscriptionActivationMode.SpecificDate?today.AddDays(3):null};await Finance().CreateAsync(r);await Finance().CreateAsync(r);
        Assert.False((await Entrance().EvaluateAsync(new(member.Id,null))).Allowed);clock.Now=clock.Now.AddDays(3);
        var request=new CreateCheckInRequest(Guid.NewGuid(),CheckInMethod.Manual,member.Id);var entry=await Entrance().CreateAsync(request);Assert.True(entry.Decision.Allowed);
        Assert.Equal(entry.CheckIn,(await Entrance().CreateAsync(request)).CheckIn);Assert.Single(await db.MemberSubscriptions.ToListAsync());Assert.Single(await db.CheckIns.ToListAsync());
    }
    [Fact] public async Task ConflictingVisitorPurchaseRollsBackMoneyAndRegularRenewalCannotUseVisitorCycle()
    {
        var r=await Visitor();await Finance().CreateAsync(r);await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().CreateAsync(r with{OperationId=Guid.NewGuid()}));
        var sub=await db.MemberSubscriptions.SingleAsync();await Assert.ThrowsAsync<BusinessRuleException>(()=>subscriptions.RenewCustomerAsync(new(sub.Id,plan.Id,b.Id,Guid.NewGuid(),sub.Version)));
        Assert.Single(await db.SaleTransactions.ToListAsync());Assert.Single(await db.Payments.ToListAsync());
    }
    [Fact] public async Task VisitorIntentRejectsMultiSessionPlan()
    {
        var cash=await Account();await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)) with{PurchaseMode=PurchaseMode.StandaloneSingleSession}));Assert.Empty(await db.SaleTransactions.ToListAsync());
    }
}
