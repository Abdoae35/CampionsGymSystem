using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    [Fact] public async Task ScopedHistoryDetailsAndMethodTotalsPreserveFinancialFactsAndRetainedFee()
    {
        var cash=await Account();var insta=await Account(FinancialAccountType.InstaPay);var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,700),new PaymentPartInput(insta.Id,500)));
        await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),300,[new(cash.Id,100),new(insta.Id,200)],"Decision"));
        var details=await Finance().GetDetailsAsync(sale.SaleId);Assert.Equal(1200,details.Sale.Collected);Assert.Equal(300,details.Sale.Refunded);Assert.Equal(0,details.Sale.Outstanding);
        Assert.Equal(900,details.RetainedCancellationRevenue);Assert.False(details.RetainedCommissionEligible);Assert.False(details.RetainedTargetEligible);Assert.Equal("Cancelled",details.ActivationStatus);
        var totals=await Finance().GetBranchTotalsAsync(b.Id,today,today);Assert.Equal(700,totals.CashCollected);Assert.Equal(500,totals.InstaPayCollected);
        Assert.Equal(100,totals.CashRefunded);Assert.Equal(200,totals.InstaPayRefunded);Assert.Equal(900,totals.NetCollected);
        Assert.Equal(0,(await Finance().GetBranchTotalsAsync(a.Id)).GrossCollected);
        var salePage=await Finance().GetHistoryAsync(new(){BranchId=b.Id,MemberId=member.Id,From=today,To=today,PageSize=1});
        var listed=Assert.Single(salePage.Items);
        Assert.Equal(1,salePage.TotalCount);
        Assert.Equal(member.FullName,listed.MemberName);
        Assert.Equal(member.MemberCode,listed.MemberCode);
        Assert.Equal(details.Sale.Collected,listed.Collected);
        Assert.Equal(member.FullName,details.Sale.MemberName);
        member.FullName="Current Buyer";
        member.MemberCode="M-CURRENT-BUYER";
        await db.SaveChangesAsync();
        Assert.Equal("Current Buyer",Assert.Single((await Finance().GetHistoryAsync(new(){BranchId=b.Id,MemberId=member.Id})).Items).MemberName);
        Assert.Equal("M-CURRENT-BUYER",(await Finance().GetDetailsAsync(sale.SaleId)).Sale.MemberCode);
        Assert.Single((await Finance().GetPaymentsAsync(sale.SaleId)).Items);Assert.Single((await Finance().GetRefundsAsync(sale.SaleId)).Items);
        Assert.Empty((await Finance().GetHistoryAsync(new(){BranchId=a.Id})).Items);
        user.Context=user.Context! with{Permissions=[SalesPermissions.View],CurrentBranch=new(a.Id,a.Name)};
        Assert.Empty((await Finance().GetHistoryAsync(new())).Items);await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Finance().GetDetailsAsync(sale.SaleId));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Finance().GetBranchTotalsAsync(b.Id));
    }
    [Fact] public async Task ReservationStatusPaymentHistoryAndPaginationAreAuthoritative()
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,200)) with{PurchaseMode=PurchaseMode.Reservation});
        await Finance().RecordPaymentAsync(sale.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,100)]));var details=await Finance().GetDetailsAsync(sale.SaleId);
        Assert.Equal("Reserved",details.Sale.Status);Assert.Equal(900,details.Sale.Outstanding);Assert.Equal("PendingPayment",details.ActivationStatus);
        var history=await Finance().GetPaymentsAsync(sale.SaleId,2,1);Assert.Equal(2,history.TotalCount);Assert.Single(history.Items);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().GetHistoryAsync(new(){PageSize=101}));await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().GetPaymentsAsync(sale.SaleId,0));
    }
    [Fact] public async Task InstallmentQueriesIgnoreTamperedCachesAndUseActualAllocations()
    {
        var cash=await Account();var sale=await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,200)) with{PurchaseMode=PurchaseMode.Installment,
            Installments=[new(1000,today.AddDays(1))]});var line=await db.Installments.SingleAsync();line.AmountPaid=1000;line.Status=InstallmentStatus.Paid;await db.SaveChangesAsync();clock.Now=clock.Now.AddDays(2);
        var detail=Assert.Single(await Finance().GetInstallmentsAsync(sale.SaleId));Assert.Equal(0,detail.Paid);Assert.Equal("Overdue",detail.Status);
        await Finance().RecordPaymentAsync(sale.SaleId,new(Guid.NewGuid(),1000,[new(cash.Id,1000)],[new(line.Id,1000)]));Assert.Equal("Paid",Assert.Single(await Finance().GetInstallmentsAsync(sale.SaleId)).Status);
    }
    [Fact] public async Task AccountMethodBranchAndOpeningFactsCannotRewriteHistory()
    {
        var cash=await Account();await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,1200)));cash.AccountType=FinancialAccountType.InstaPay;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>db.SaveChangesAsync());db.ChangeTracker.Clear();Assert.Equal(1200,(await Finance().GetBranchTotalsAsync(b.Id)).CashCollected);
    }
    [Fact] public async Task OwnedFreelanceRevenueReportAcceptsFixedGymSaleWithoutReclassifyingOwner()
    {
        var cash=await Account();var coach=new Employee{FullName="Coach",Phone="query-coach",HireDate=today.AddYears(-1),EmploymentType=EmploymentType.Freelance};
        var owner=new Employee{FullName="Owner",Phone="query-owner",HireDate=today.AddYears(-1)};db.AddRange(coach,owner);await db.SaveChangesAsync();
        plan.PlanCategory=PlanCategory.Freelance;plan.FreelanceCoachEmployeeId=coach.Id;plan.Price=300;
        db.Add(new MembershipPlanEligibleMember{MembershipPlanId=plan.Id,MemberId=member.Id,CreatedByUserId=actor.Id});db.Add(new MemberSalesAssignment{MemberId=member.Id,SalesEmployeeId=owner.Id,AssignmentType=AssignmentType.Sales,AssignedAt=access.Now,AssignedByUserId=actor.Id});await db.SaveChangesAsync();
        await Finance().CreateAsync(Purchase(new PaymentPartInput(cash.Id,300)));user.Context=user.Context! with{Permissions=[..user.Context!.Permissions,"FreelancePlans.View"]};
        var report=Assert.Single(await new CoachReportService(db,access).FreelanceAsync(plan.Id,b.Id,clock.Now.AddDays(-1),clock.Now.AddDays(1)));
        Assert.Equal(300,report.ActualGymRevenue);Assert.Equal(1,report.EligibleMemberCount);Assert.Equal(owner.Id,(await db.SaleTransactions.SingleAsync()).SalesEmployeeId);
    }
}
