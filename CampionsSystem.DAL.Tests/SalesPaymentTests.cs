using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SalesTests
{
    private SalesService Finance() => new(db, user, access, offers, subscriptions);
    private async Task<FinancialAccount> Account(FinancialAccountType type = FinancialAccountType.Cash, Guid? branch = null)
    { if(type==FinancialAccountType.Cash && (branch==null||branch==b.Id)) return await db.FinancialAccounts.SingleAsync(x=>x.Id==db.CashDrawers.Where(d=>d.BranchId==b.Id).Select(d=>d.FinancialAccountId).Single()); var account = new FinancialAccount { BranchId = branch ?? b.Id, Name = type.ToString(), AccountType = type }; db.Add(account); await db.SaveChangesAsync(); return account; }
    private CreateSaleRequest Purchase(params PaymentPartInput[] parts) => new(Guid.NewGuid(), member.Id, plan.Id, PurchaseMode.FullPayment, SubscriptionActivationMode.Now, parts);
    [Theory][InlineData(FinancialAccountType.Cash)][InlineData(FinancialAccountType.InstaPay)]
    public async Task FullSaleRecordsRealOwnShiftMoneyWithoutCommission(FinancialAccountType type)
    {
        var account = await Account(type); var receipt = await Finance().RecordSaleAsync(Purchase(new PaymentPartInput(account.Id, 1200)));
        var sale = await db.SaleTransactions.SingleAsync(); var payment = await db.Payments.SingleAsync(); var ledger = await db.FinancialTransactions.SingleAsync();
        Assert.Equal(receipt.SaleId, sale.Id); Assert.Equal(b.Id, sale.BranchId); Assert.Equal(b.Id, payment.BranchId); Assert.Equal(b.Id, ledger.BranchId);
        Assert.Equal(a.Id, member.HomeBranchId); Assert.Equal(1200, payment.Amount); Assert.Equal(1200, ledger.Amount);
        Assert.Equal(FinancialTransactionType.Credit, ledger.TransactionType); Assert.Equal(account.Id, ledger.FinancialAccountId);
        Assert.Equal((await db.Shifts.SingleAsync()).Id,payment.ShiftId); Assert.Null(payment.CommissionPeriodId); Assert.Equal(AssignmentType.House, sale.SalesAttributionSnapshot);
        Assert.Null(sale.SalesEmployeeId); Assert.False(sale.CommissionEligible); Assert.False(sale.TargetEligible);
        Assert.Empty(await db.CommissionPeriods.ToListAsync()); Assert.Empty(await db.Payrolls.ToListAsync()); Assert.Single(await db.Shifts.ToListAsync());
    }
    [Fact] public async Task SplitTenderIsOneSaleOnePaymentWithExactSourcesAndRetryIdentity()
    {
        var cash = await Account(); var insta = await Account(FinancialAccountType.InstaPay); var r = Purchase(new PaymentPartInput(cash.Id, 700), new PaymentPartInput(insta.Id, 500, "reference"));
        var service = Finance(); var result = await service.RecordSaleAsync(r); var retry = await service.RecordSaleAsync(r);
        Assert.Equal(result.SaleId, retry.SaleId); Assert.Equal(result.PaymentIds, retry.PaymentIds);
        Assert.Single(await db.SaleTransactions.ToListAsync()); Assert.Single(await db.Payments.ToListAsync()); Assert.Equal(2, await db.PaymentParts.CountAsync());
        var movements = await db.FinancialTransactions.OrderBy(x => x.Amount).ToListAsync(); Assert.Equal(new decimal[] { 500, 700 }, movements.Select(x => x.Amount));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.RecordSaleAsync(r with { PaymentParts = [new PaymentPartInput(cash.Id,1200)] }));
    }
    [Theory][InlineData(1199)][InlineData(1201)][InlineData(0)][InlineData(-1)]
    public async Task InvalidTenderNeverWritesSaleOrPayment(int amount)
    {
        var cash = await Account(); await Assert.ThrowsAsync<BusinessRuleException>(() => Finance().RecordSaleAsync(Purchase(new PaymentPartInput(cash.Id, amount))));
        Assert.Empty(await db.SaleTransactions.ToListAsync()); Assert.Empty(await db.Payments.ToListAsync()); Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task BranchSpoofingWrongAccountAndOwnershipPrivilegesCannotBypassScope()
    {
        var wrong = await Account(branch: a.Id); await Assert.ThrowsAsync<BusinessRuleException>(() => Finance().RecordSaleAsync(Purchase(new PaymentPartInput(wrong.Id,1200))));
        user.Context = user.Context! with { Permissions = [SalesPermissions.Create, "Members.OwnSalesCustomers"] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Finance().RecordSaleAsync(Purchase(new PaymentPartInput(wrong.Id,1200)) with { BranchId = a.Id }));
        Assert.Empty(await db.SaleTransactions.ToListAsync());
    }
    [Fact] public async Task OfferAndBenefitTermsAreSoldSnapshotsRatherThanLiveReferences()
    {
        var cash = await Account(); var offer = await offers.CreateAsync(OfferInput()); var benefit = new MembershipBenefit { Code = "GUEST", Name = "Guests" };
        db.Add(new MembershipPlanBenefit { MembershipPlan = plan, MembershipBenefit = benefit, GrantType = MembershipBenefitGrantType.Quantity, Quantity = 2 }); await db.SaveChangesAsync();
        await Finance().RecordSaleAsync(Purchase(new PaymentPartInput(cash.Id,1000)) with { OfferId = offer.Id });
        plan.Price = 1500; plan.DurationDays = 25; plan.CityId = c.CityId; await db.SaveChangesAsync();
        await offers.UpdateAsync(offer.Id, OfferInput(OfferEligibilityMode.MemberOnly, women.Id, value: 300));
        var snapshot = await db.SaleMembershipSnapshots.AsNoTracking().SingleAsync(); var sale = await db.SaleTransactions.AsNoTracking().SingleAsync();
        Assert.Equal(30, snapshot.DurationDays); Assert.Equal(a.CityId, snapshot.CityId); Assert.Equal(1000, sale.NetAmount);
        Assert.Equal(OfferEligibilityMode.General, sale.OfferEligibilityModeSnapshot); Assert.Equal(2, (await db.SaleBenefitSnapshots.SingleAsync()).Quantity);
    }
    [Fact] public async Task MoneyHistoryCannotBeRewrittenAndOperationRequiresPermission()
    {
        var cash = await Account(); await Finance().RecordSaleAsync(Purchase(new PaymentPartInput(cash.Id,1200))); var payment = await db.Payments.SingleAsync();
        payment.Amount = 1; await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync()); db.ChangeTracker.Clear();
        user.Context = user.Context! with { Permissions = [] }; await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Finance().RecordSaleAsync(Purchase(new PaymentPartInput(cash.Id,1200))));
        Assert.Equal(1200, (await db.Payments.AsNoTracking().SingleAsync()).Amount);
    }
    [Fact] public async Task OvernightMoneyPreservesOpenBusinessDateAndRealTimestamp()
    {
        var cash = await Account(); clock.Now = new(2026,10,10,21,30,0,TimeSpan.Zero);
        var day = new BusinessDay { BranchId = b.Id, BusinessDate = today, Status = BusinessDayStatus.Open, OpenedAt = clock.Now.AddHours(-12) };
        db.Add(day); await db.SaveChangesAsync(); await Finance().RecordSaleAsync(Purchase(new PaymentPartInput(cash.Id,1200)));
        var payment = await db.Payments.SingleAsync(); Assert.Equal(today.AddDays(1), access.Today); Assert.Equal(today, payment.BusinessDate);
        Assert.Equal(day.Id, payment.BusinessDayId); Assert.Equal(clock.Now, payment.PaidAt); Assert.Equal(today,(await db.FinancialTransactions.SingleAsync()).BusinessDate);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task NormalAndFreelanceSalesSnapshotExistingOwnerWithoutReassignment(bool freelance)
    {
        var cash = await Account(); var owner = new Employee { FullName = "Owner", Phone = "sales-owner", HireDate = today.AddYears(-1) };
        db.Add(owner); await db.SaveChangesAsync();
        var assignment = new MemberSalesAssignment { MemberId = member.Id, SalesEmployeeId = owner.Id, AssignmentType = AssignmentType.Sales, AssignedAt = access.Now, AssignedByUserId = actor.Id };
        db.Add(assignment);
        if (freelance)
        {
            var coach = new Employee { FullName = "Coach", Phone = "sales-coach", HireDate = today.AddYears(-1), EmploymentType = EmploymentType.Freelance };
            db.Add(coach); await db.SaveChangesAsync(); plan.PlanCategory = PlanCategory.Freelance; plan.FreelanceCoachEmployeeId = coach.Id; plan.Price = 300;
            db.Add(new MembershipPlanEligibleMember { MembershipPlanId = plan.Id, MemberId = member.Id, CreatedByUserId = actor.Id });
        }
        await db.SaveChangesAsync(); await Finance().RecordSaleAsync(Purchase(new PaymentPartInput(cash.Id, freelance ? 300 : 1200)));
        var sale = await db.SaleTransactions.SingleAsync(); Assert.Equal(owner.Id, sale.SalesEmployeeId); Assert.Equal(AssignmentType.Sales, sale.SalesAttributionSnapshot);
        Assert.Equal(!freelance, sale.CommissionEligible); Assert.Equal(!freelance, sale.TargetEligible); Assert.Equal(freelance ? 300 : 1200, sale.NetAmount);
        Assert.Null(assignment.EndedAt); Assert.Single(await db.MemberSalesAssignments.ToListAsync()); Assert.Equal(a.Id, member.HomeBranchId);
        Assert.Empty(await db.EmployeeCommissionSummaries.ToListAsync());
    }
    [Fact] public async Task InsertFailureRollsBackSalePartsSnapshotsLedgerAndAuditTogether()
    {
        var cash = await Account(); var previousAudits = await db.AuditLogs.CountAsync();
        await db.Database.ExecuteSqlRawAsync("CREATE FUNCTION reject_finance_insert() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'forced finance failure'; END; $$; CREATE TRIGGER reject_finance BEFORE INSERT ON \"FinancialTransactions\" FOR EACH ROW EXECUTE FUNCTION reject_finance_insert();");
        await Assert.ThrowsAsync<DbUpdateException>(() => Finance().RecordSaleAsync(Purchase(new PaymentPartInput(cash.Id,1200))));
        Assert.Empty(await db.SaleTransactions.ToListAsync()); Assert.Empty(await db.Payments.ToListAsync()); Assert.Empty(await db.PaymentParts.ToListAsync());
        Assert.Empty(await db.SaleMembershipSnapshots.ToListAsync()); Assert.Empty(await db.FinancialTransactions.ToListAsync()); Assert.Equal(previousAudits,await db.AuditLogs.CountAsync());
    }

}
