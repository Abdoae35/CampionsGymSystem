using CampionsSystem.BLL.Commissions;
using CampionsSystem.BLL.Employees;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    private async Task<Employee> CommercialOwner()
    {
        CommissionAccess(); var owner = new Employee { FullName = "Commercial Sales", Phone = "commission-owner", HireDate = today.AddYears(-1) };
        db.AddRange(owner, new EmployeeBranchAssignment { EmployeeId = owner.Id, BranchId = b.Id, StartDate = today.AddYears(-1), AssignedByUserId = actor.Id },
            new MemberSalesAssignment { MemberId = member.Id, SalesEmployeeId = owner.Id, AssignmentType = AssignmentType.Sales, AssignedAt = access.Now, AssignedByUserId = actor.Id });
        await db.SaveChangesAsync();
        await Commissions().ConfigureAsync(new(Guid.NewGuid(), owner.Id, today, new(30000, 5, 7), new(30000, 3, 5))); return owner;
    }
    private async Task<SaleReceipt> Collected(decimal price, decimal? collected = null)
    {
        var p = await db.MembershipPlans.SingleAsync(p => p.Id == plan.Id); p.Price = price; await db.SaveChangesAsync();
        var cash = await Account(); return await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id, collected ?? price))
            with { PurchaseMode = collected.HasValue ? PurchaseMode.Reservation : PurchaseMode.FullPayment });
    }
    [Fact] public async Task CommissionIndependentTargetsProduceConfirmed2500Example()
    {
        var owner = await CommercialOwner(); var period = await Commissions().OpenAsync(new(Guid.NewGuid(), "October", today, today.AddDays(20)));
        await Collected(20000); await Collected(30000);
        var progress = await Commissions().ProgressAsync(period.Id); var statement = progress.Employees.Single();
        Assert.Equal(owner.Id, statement.EmployeeId); Assert.Equal(5, statement.NewClients.AppliedRate); Assert.False(statement.NewClients.TargetReached);
        Assert.Equal(1000, statement.NewClients.Commission); Assert.Equal(5, statement.Renewals.AppliedRate); Assert.True(statement.Renewals.TargetReached);
        Assert.Equal(1500, statement.Renewals.Commission); Assert.Equal(2500, progress.TotalCommission); Assert.Equal(50000, progress.GrossCollections);
        Assert.Equal(new[] { "NewMembership", "Renewal" }, (await Commissions().SourcesAsync(period.Id)).Items.OrderBy(x => x.PaidAt).ThenBy(x => x.GrossCollection).Select(x => x.Category));
        Assert.All(await db.Payments.ToListAsync(), p => Assert.Equal(period.Id, p.CommissionPeriodId));
    }
    [Fact] public async Task CommissionNewTargetUnlocksAllNewRevenueWithoutChangingRenewalRate()
    {
        await CommercialOwner(); var period = await Commissions().OpenAsync(new(Guid.NewGuid(), "October", today, today.AddDays(20)));
        await Collected(30000); await Collected(20000);
        var s = (await Commissions().ProgressAsync(period.Id)).Employees.Single();
        Assert.Equal(2100, s.NewClients.Commission); Assert.Equal(600, s.Renewals.Commission); Assert.Equal(3, s.Renewals.AppliedRate);
        Assert.Equal(2700, s.TotalCommission);
    }
    [Theory][InlineData(0,4000,200)][InlineData(1000,3000,150)][InlineData(4000,0,0)]
    public async Task CommissionCancelledRetainedCashCountsOriginalEligibleMoneyOnce(int refunded, int eligible, int earned)
    {
        await CommercialOwner(); var period = await Commissions().OpenAsync(new(Guid.NewGuid(), "October", today, today.AddDays(20)));
        var sale = await Collected(12000,4000); var cash = await Account();
        var r = new RefundRequest(Guid.NewGuid(),refunded,refunded>0?[new(cash.Id,refunded)]:[],"Cancel and retain the balance");
        await Finance().RefundAsync(sale.SaleId,r); await Finance().RefundAsync(sale.SaleId,r);
        var p = await Commissions().ProgressAsync(period.Id); Assert.Equal(4000,p.GrossCollections); Assert.Equal(refunded,p.Refunds);
        Assert.Equal(eligible,p.EmployeeEligibleCollections); Assert.Equal(earned,p.TotalCommission); Assert.Single(await db.Refunds.ToListAsync());
        Assert.Equal(refunded,await db.CommissionRefundAllocations.SumAsync(a=>a.Amount));
        Assert.Equal(4000-refunded,await db.FinancialTransactions.SumAsync(t=>t.TransactionType==FinancialTransactionType.Credit?t.Amount:-t.Amount));
        var details=await Finance().GetDetailsAsync(sale.SaleId);Assert.Equal(eligible>0,details.RetainedCommissionEligible);
    }
    [Fact] public async Task CommissionNewestFirstPreservesOriginalPeriodAndInstallmentCategory()
    {
        await CommercialOwner(); var october = await Commissions().OpenAsync(new(Guid.NewGuid(), "October", today, today.AddDays(20)));
        var sale = await Collected(12000,4000); clock.Now = clock.Now.AddDays(22);
        var cash = await Account(); var r = new RecordPaymentRequest(Guid.NewGuid(),4000,[new(cash.Id,4000)]);
        var payment = await Finance().RecordPaymentAsync(sale.SaleId,r); await Finance().RecordPaymentAsync(sale.SaleId,r);
        await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),1000,[new(cash.Id,1000)],"Partial"));
        Assert.Equal(4000,(await Commissions().ProgressAsync(october.Id)).EmployeeEligibleCollections);
        Assert.Equal(payment.Id,(await db.CommissionRefundAllocations.SingleAsync()).PaymentId);
        var close = await Commissions().FinalizeAsync(october.Id,new(Guid.NewGuid(),0)); Assert.Equal("Closed",close.Status);
        var november = await Commissions().OpenAsync(new(Guid.NewGuid(), "November", today.AddDays(21), today.AddDays(50)));
        Assert.Equal(3000,(await Commissions().ProgressAsync(november.Id)).NewClientEligibleCollections);
        Assert.Equal(150,(await Commissions().ProgressAsync(november.Id)).TotalCommission);
        Assert.Equal(2,await db.Payments.CountAsync());Assert.Equal(0,(await Commissions().ProgressAsync(november.Id)).RenewalEligibleCollections);
    }
    [Fact] public async Task CommissionRefundWalksBackwardWithoutNegativeCollectionsOrDuplicateParts()
    {
        await CommercialOwner(); var period = await Commissions().OpenAsync(new(Guid.NewGuid(), "Period", today, today.AddDays(20)));
        var sale = await Collected(12000,4000); var cash = await Account(); var insta = await Account(FinancialAccountType.InstaPay);
        clock.Now=clock.Now.AddHours(1);await Finance().RecordPaymentAsync(sale.SaleId,new(Guid.NewGuid(),4000,[new(cash.Id,2000),new(insta.Id,2000)]));
        await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),5000,[new(cash.Id,5000)],"Refund"));
        var lines=(await Commissions().SourcesAsync(period.Id)).Items;Assert.Equal(2,lines.Count);Assert.Equal(0,lines[0].RetainedCollection);Assert.Equal(3000,lines[1].RetainedCollection);
        Assert.Equal(8000,(await Commissions().ProgressAsync(period.Id)).GrossCollections);Assert.Equal(5000,await db.CommissionRefundAllocations.SumAsync(a=>a.Amount));
    }
    [Fact] public async Task CommissionFinalizedSourceRejectsRefundAndPreservesStatementAndGrantAtomically()
    {
        await CommercialOwner(); var period = await Commissions().OpenAsync(new(Guid.NewGuid(), "Period", today, today));var sale=await Collected(1200);
        clock.Now=clock.Now.AddDays(1);await Commissions().FinalizeAsync(period.Id,new(Guid.NewGuid(),0));var cash=await Account();var before=await db.FinancialTransactions.CountAsync();
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)],"Late")));
        Assert.Empty(await db.Refunds.ToListAsync());Assert.Equal(before,await db.FinancialTransactions.CountAsync());Assert.Null((await db.SaleTransactions.AsNoTracking().SingleAsync()).CancelledAt);
        Assert.Equal(60,(await Commissions().ProgressAsync(period.Id)).TotalCommission);Assert.True((await Commissions().ProgressAsync(period.Id)).Finalized);
        Assert.Equal(2,await db.EmployeeCommissionSummaries.CountAsync());Assert.Single(await db.CommissionStatementLines.ToListAsync());
        // Zero-refund cancellation does not change collected commission facts.
        await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),0,[],"Retain all"));Assert.Equal(60,(await Commissions().ProgressAsync(period.Id)).TotalCommission);
    }
    [Fact] public async Task CommissionResignationAndRehireDoNotRestoreOldContractCollectionRights()
    {
        var owner=await CommercialOwner();var period=await Commissions().OpenAsync(new(Guid.NewGuid(),"Period",today,today.AddDays(20)));var sale=await Collected(12000,4000);
        user.Context=user.Context! with{Permissions=[..user.Context!.Permissions,"Employees.Manage"]};
        var manager=new EmployeeManagementService(db,user,access,new(db),new(db,user,access),subscriptions);
        await manager.ChangeStatusAsync(owner.Id,new(){Status=EmploymentStatus.Resigned});clock.Now=clock.Now.AddHours(1);
        var cash=await Account();await Finance().RecordPaymentAsync(sale.SaleId,new(Guid.NewGuid(),1000,[new(cash.Id,1000)])); // last working day remains earned
        clock.Now=clock.Now.AddDays(1);await Finance().RecordPaymentAsync(sale.SaleId,new(Guid.NewGuid(),1000,[new(cash.Id,1000)]));
        await manager.ChangeStatusAsync(owner.Id,new(){Status=EmploymentStatus.Active});await Finance().RecordPaymentAsync(sale.SaleId,new(Guid.NewGuid(),1000,[new(cash.Id,1000)]));
        var p=await Commissions().ProgressAsync(period.Id);Assert.Equal(5000,p.EmployeeEligibleCollections);Assert.Equal(2000,p.HouseCollections);Assert.Equal(250,p.TotalCommission);
        await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),500,[new(cash.Id,500)],"Newest House collection"));
        p=await Commissions().ProgressAsync(period.Id);Assert.Equal(5000,p.EmployeeEligibleCollections);Assert.Equal(1500,p.HouseCollections);Assert.Equal(250,p.TotalCommission);
        Assert.Equal(4,(await Commissions().SourcesAsync(period.Id,owner.Id)).Total);
        Assert.All(await db.SaleTransactions.ToListAsync(),s=>Assert.Equal(owner.Id,s.SalesEmployeeId));
    }
    [Fact] public async Task CommissionFirstPurchaseHistoryIncludesPendingCancelledHouseAndCrossBranchSales()
    {
        CommissionAccess();var first=await Collected(1200,200);Assert.Empty(await db.MemberSubscriptions.ToListAsync());
        var cash=await Account();await Finance().RefundAsync(first.SaleId,new(Guid.NewGuid(),0,[],"Cancel pending"));
        await CommercialOwner();var second=await Collected(1200);
        Assert.Equal(SaleCategory.NewMembership,(await db.SaleTransactions.SingleAsync(s=>s.Id==first.SaleId)).SaleCategory);
        Assert.Equal(SaleCategory.Renewal,(await db.SaleTransactions.SingleAsync(s=>s.Id==second.SaleId)).SaleCategory);
        Assert.Equal(a.Id,(await db.Members.SingleAsync()).HomeBranchId);Assert.Equal(b.Id,(await db.SaleTransactions.SingleAsync(s=>s.Id==second.SaleId)).BranchId);
    }
    [Fact] public async Task CommissionOwnerReassignmentChangesFutureRenewalOnly()
    {
        var firstOwner=await CommercialOwner();var first=await Collected(1200);var assignment=await db.MemberSalesAssignments.SingleAsync();assignment.EndedAt=access.Now;
        var next=new Employee{FullName="Next Sales",Phone="next-owner",HireDate=today.AddYears(-1)};db.Add(next);
        db.Add(new MemberSalesAssignment{MemberId=member.Id,SalesEmployeeId=next.Id,AssignmentType=AssignmentType.Sales,AssignedAt=access.Now,AssignedByUserId=actor.Id});await db.SaveChangesAsync();
        var second=await Collected(1200);var sales=await db.SaleTransactions.ToListAsync();
        Assert.Equal(firstOwner.Id,sales.Single(s=>s.Id==first.SaleId).SalesEmployeeId);Assert.Equal(next.Id,sales.Single(s=>s.Id==second.SaleId).SalesEmployeeId);
        Assert.Equal(SaleCategory.Renewal,sales.Single(s=>s.Id==second.SaleId).SaleCategory);
        Assert.Equal(firstOwner.Id,(await db.CommissionCollections.SingleAsync(c=>c.PaymentId==first.PaymentIds.Single())).EmployeeId);
    }
    [Fact] public async Task CommissionRegistrationAndUnpaidInstallmentsContributeZero()
    {
        await CommercialOwner();var p=await Commissions().OpenAsync(new(Guid.NewGuid(),"Period",today,today.AddDays(20)));
        Assert.Equal(0,(await Commissions().ProgressAsync(p.Id)).TotalCommission);
        await Finance().CreateAsync(Purchase() with{PurchaseMode=PurchaseMode.Installment,InitialPaymentRequired=100,Installments=[new(1200,today.AddDays(2))]});
        Assert.Empty(await db.Payments.ToListAsync());Assert.Equal(0,(await Commissions().ProgressAsync(p.Id)).GrossCollections);
    }
    [Fact] public async Task CommissionOpenRefundRecomputesAllCategoryMoneyAtBelowTargetRate()
    {
        await CommercialOwner();var period=await Commissions().OpenAsync(new(Guid.NewGuid(),"Period",today,today.AddDays(20)));var sale=await Collected(30000);
        Assert.Equal(2100,(await Commissions().ProgressAsync(period.Id)).TotalCommission);var cash=await Account();
        await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),1,[new(cash.Id,1)],"Partial"));
        var p=await Commissions().ProgressAsync(period.Id);Assert.False(p.Employees.Single().NewClients.TargetReached);Assert.Equal(1499.95m,p.TotalCommission);
        Assert.Equal(3,p.Employees.Single().Renewals.AppliedRate);
    }
    [Fact] public async Task CommissionRoundsCategoryAggregateOnceWithAwayFromZero()
    {
        await CommercialOwner();var period=await Commissions().OpenAsync(new(Guid.NewGuid(),"Period",today,today.AddDays(20)));var sale=await Collected(.2m,.1m);var cash=await Account();
        Assert.Equal(.01m,(await Commissions().ProgressAsync(period.Id)).TotalCommission);
        await Finance().RecordPaymentAsync(sale.SaleId,new(Guid.NewGuid(),.1m,[new(cash.Id,.1m)]));
        Assert.Equal(.01m,(await Commissions().ProgressAsync(period.Id)).TotalCommission);
    }
    [Fact] public async Task CommissionBusinessDateUsesOpenOperationalDayAcrossMidnight()
    {
        await CommercialOwner();var period=await Commissions().OpenAsync(new(Guid.NewGuid(),"Period",today,today));clock.Now=new(2026,10,10,21,30,0,TimeSpan.Zero);
        db.Add(new BusinessDay{BranchId=b.Id,BusinessDate=today,OpenedAt=clock.Now.AddHours(-12)});await db.SaveChangesAsync();await Collected(1200);
        Assert.Equal(today.AddDays(1),access.Today);var line=(await Commissions().SourcesAsync(period.Id)).Items.Single();Assert.Equal(today,line.BusinessDate);Assert.Equal(clock.Now,line.PaidAt);
        Assert.Equal(60,(await Commissions().ProgressAsync(period.Id)).TotalCommission);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Commissions().FinalizeAsync(period.Id,new(Guid.NewGuid(),0)));
    }
    [Fact] public async Task CommissionActualInstallmentAllocationsKeepNewCategoryAcrossPeriods()
    {
        await CommercialOwner();var october=await Commissions().OpenAsync(new(Guid.NewGuid(),"October",today,today.AddDays(20)));plan.Price=12000;await db.SaveChangesAsync();var cash=await Account();
        var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,4000)) with{PurchaseMode=PurchaseMode.Installment,InitialPaymentRequired=4000,
            Installments=[new(4000,today.AddDays(21)),new(4000,today.AddDays(50))]});
        clock.Now=clock.Now.AddDays(21);await Commissions().FinalizeAsync(october.Id,new(Guid.NewGuid(),0));
        var november=await Commissions().OpenAsync(new(Guid.NewGuid(),"November",today.AddDays(21),today.AddDays(50)));
        var obligation=await db.Installments.OrderBy(x=>x.InstallmentNumber).FirstAsync();
        var request=new RecordPaymentRequest(Guid.NewGuid(),4000,[new(cash.Id,4000)],[new(obligation.Id,4000)]);
        await Finance().RecordPaymentAsync(sale.SaleId,request);await Finance().RecordPaymentAsync(sale.SaleId,request);
        Assert.Equal(4000,(await Commissions().ProgressAsync(october.Id)).NewClientEligibleCollections);Assert.Equal(4000,(await Commissions().ProgressAsync(november.Id)).NewClientEligibleCollections);
        Assert.Equal(8000,await db.Payments.SumAsync(p=>p.Amount));Assert.Single(await db.InstallmentPaymentAllocations.ToListAsync());Assert.Equal(200,(await Commissions().ProgressAsync(november.Id)).TotalCommission);
    }
    [Fact] public async Task CommissionStaleBusinessDayCannotRestoreDepartedEmployeesRights()
    {
        var owner=await CommercialOwner();var period=await Commissions().OpenAsync(new(Guid.NewGuid(),"Period",today,today.AddDays(20)));var sale=await Collected(12000,4000);
        user.Context=user.Context! with{Permissions=[..user.Context!.Permissions,"Employees.Manage"]};var manager=new EmployeeManagementService(db,user,access,new(db),new(db,user,access),subscriptions);
        await manager.ChangeStatusAsync(owner.Id,new(){Status=EmploymentStatus.Resigned});
        db.Add(new BusinessDay{BranchId=b.Id,BusinessDate=today,OpenedAt=clock.Now.AddHours(-1)});await db.SaveChangesAsync();clock.Now=clock.Now.AddDays(2);
        var cash=await Account();await Finance().RecordPaymentAsync(sale.SaleId,new(Guid.NewGuid(),1000,[new(cash.Id,1000)]));
        var p=await Commissions().ProgressAsync(period.Id);Assert.Equal(4000,p.EmployeeEligibleCollections);Assert.Equal(1000,p.HouseCollections);
        Assert.All((await Commissions().SourcesAsync(period.Id)).Items,l=>Assert.Equal(today,l.BusinessDate));
    }
    [Fact] public async Task CommissionCrossBranchPurchaseIsRenewalAndRevenueNeverMoves()
    {
        var owner=await CommercialOwner();var pb=await Commissions().OpenAsync(new(Guid.NewGuid(),"B",today,today.AddDays(13)));await Collected(1200);
        var otherActor=new ApplicationUser{UserName="Other branch cashier"};db.Add(otherActor);var otherPlan=new MembershipPlan{Name="Other branch plan",BranchId=a.Id,CityId=a.CityId,DurationDays=30,Price=2000};db.Add(otherPlan);await db.SaveChangesAsync();
        await OperationalShiftFixture.OpenAsync(db,otherActor.Id,a.Id,clock);
        user.Context=user.Context! with{UserId=otherActor.Id,CurrentBranch=new(a.Id,"A")};
        await Commissions().ConfigureAsync(new(Guid.NewGuid(),owner.Id,today,new(30000,5,7),new(30000,3,5)));
        var pa=await Commissions().OpenAsync(new(Guid.NewGuid(),"A",today,today.AddDays(20)));var acash=await db.CashDrawers.SingleAsync(d=>d.BranchId==a.Id);
        await Finance().CreateAsync(new(Guid.NewGuid(),member.Id,otherPlan.Id,PurchaseMode.FullPayment,SubscriptionActivationMode.Now,[new(acash.FinancialAccountId,2000)]));
        Assert.Equal(1200,(await Commissions().ProgressAsync(pb.Id)).NewClientEligibleCollections);Assert.Equal(2000,(await Commissions().ProgressAsync(pa.Id)).RenewalEligibleCollections);
        Assert.Equal(a.Id,(await db.Members.AsNoTracking().SingleAsync()).HomeBranchId);Assert.Equal(owner.Id,(await db.MemberSalesAssignments.SingleAsync()).SalesEmployeeId);
    }
    [Fact] public async Task CommissionFinalizedRatesStayImmutableAfterFutureEmployeeConfiguration()
    {
        var owner=await CommercialOwner();var period=await Commissions().OpenAsync(new(Guid.NewGuid(),"First",today,today));await Collected(1200);
        await Commissions().ConfigureAsync(new(Guid.NewGuid(),owner.Id,today.AddDays(1),new(10000,1,2),new(10000,2,4)));
        clock.Now=clock.Now.AddDays(1);await Commissions().FinalizeAsync(period.Id,new(Guid.NewGuid(),0));
        var next=await Commissions().OpenAsync(new(Guid.NewGuid(),"Next",today.AddDays(1),today.AddDays(20)));await Collected(1200);
        Assert.Equal(60,(await Commissions().ProgressAsync(period.Id)).TotalCommission);Assert.Equal(24,(await Commissions().ProgressAsync(next.Id)).TotalCommission);
        Assert.Equal(5,(await Commissions().EmployeeStatementAsync(period.Id,owner.Id)).NewClients.BelowRate);
    }
    [Fact] public async Task CommissionBranchDefaultsCoverNewEmployeeAndFutureOverrideLeavesCurrentEstimateUnchanged()
    {
        CommissionAccess();
        await Commissions().ConfigureAsync(new(Guid.NewGuid(),null,today,new(30000,5,7),new(30000,3,5)));
        var owner=new Employee{FullName="New Sales",Phone="branch-default-owner",HireDate=today.AddYears(-1)};db.Add(owner);
        db.Add(new MemberSalesAssignment{MemberId=member.Id,SalesEmployeeId=owner.Id,AssignmentType=AssignmentType.Sales,AssignedAt=access.Now,AssignedByUserId=actor.Id});await db.SaveChangesAsync();
        var first=await Commissions().OpenAsync(new(Guid.NewGuid(),"First",today,today));await Collected(1200);
        await Commissions().ConfigureAsync(new(Guid.NewGuid(),owner.Id,today.AddDays(1),new(30000,1,2),new(30000,2,4)));
        var estimate=await Commissions().EmployeeStatementAsync(first.Id,owner.Id);Assert.False(estimate.Finalized);Assert.Equal(first.Id,estimate.PeriodId);Assert.Equal(60,estimate.TotalCommission);
        clock.Now=clock.Now.AddDays(1);await Commissions().FinalizeAsync(first.Id,new(Guid.NewGuid(),0));
        var next=await Commissions().OpenAsync(new(Guid.NewGuid(),"Next",today.AddDays(1),today.AddDays(20)));await Collected(1200);
        Assert.Equal(24,(await Commissions().EmployeeStatementAsync(next.Id,owner.Id)).TotalCommission);
        var frozen=await Commissions().EmployeeStatementAsync(first.Id,owner.Id);Assert.True(frozen.Finalized);Assert.Equal(60,frozen.TotalCommission);
    }
    [Fact] public async Task CommissionFreelanceFixedGymRevenueRemainsExcluded()
    {
        await CommercialOwner();var coach=new Employee{FullName="Coach",Phone="commission-coach",HireDate=today.AddYears(-1),EmploymentType=EmploymentType.Freelance};db.Add(coach);await db.SaveChangesAsync();
        plan.PlanCategory=PlanCategory.Freelance;plan.FreelanceCoachEmployeeId=coach.Id;db.Add(new MembershipPlanEligibleMember{MembershipPlanId=plan.Id,MemberId=member.Id,CreatedByUserId=actor.Id});await db.SaveChangesAsync();
        var period=await Commissions().OpenAsync(new(Guid.NewGuid(),"Period",today,today.AddDays(20)));await Collected(300);
        var p=await Commissions().ProgressAsync(period.Id);Assert.Equal(300,p.ExcludedCollections);Assert.Equal(0,p.TotalCommission);Assert.Equal("Freelance",(await Commissions().SourcesAsync(period.Id)).Items.Single().EligibilityReason);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task CommissionHouseAndStandaloneAreExcludedEvenAfterCollection(bool standalone)
    {
        CommissionAccess();if(standalone){await CommercialOwner();plan.AccessMode=AccessMode.SessionBased;plan.SessionCount=1;await db.SaveChangesAsync();}
        var period=await Commissions().OpenAsync(new(Guid.NewGuid(),"Period",today,today.AddDays(20)));var cash=await Account();
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)) with{PurchaseMode=standalone?PurchaseMode.StandaloneSingleSession:PurchaseMode.FullPayment});
        var p=await Commissions().ProgressAsync(period.Id);Assert.Equal(0,p.TotalCommission);Assert.Equal(0,p.EmployeeEligibleCollections);Assert.Equal(1200,standalone?p.ExcludedCollections:p.HouseCollections);
    }
}
