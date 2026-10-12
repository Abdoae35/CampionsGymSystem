using System.IO.Compression;
using System.Xml.Linq;
using CampionsSystem.BLL.MonthlyFinance;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Sales;
using CampionsSystem.BLL.Coaching;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace CampionsSystem.DAL.Tests;

public sealed partial class SalesTests
{
    private MonthlyFinanceService Monthly()=>new(db,user,access);
    private async Task<FinancialMonthDetails> ReportingMonth(DateOnly end,decimal cashierBasic=0)
    {
        user.Context=user.Context! with{Permissions=[..user.Context.Permissions,..MonthlyFinancePermissions.Codes,..ShiftPermissions.Codes,"Payroll.Manage"]};
        foreach(var e in await db.Employees.ToListAsync())
            await new SalaryService(db,user,access).ConfigureAsync(e.Id,new(Guid.NewGuid(),e.UserId==actor.Id?cashierBasic:0,e.HireDate,"Explicit test basic"));
        return await Monthly().OpenAsync(new(Guid.NewGuid(),"Reporting",today,end,"Start"));
    }
    private async Task CloseReportingShifts()
    {
        var shift=await db.Shifts.SingleAsync(x=>x.BranchId==b.Id&&x.Status==ShiftStatus.Open);
        var position=await new VaultService(db,user,access).PositionAsync(b.Id);
        await new ShiftService(db,user,access).CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,position.ExpectedPhysicalCash));
    }
    [Fact] public async Task MonthlyIntegrationLedgerReportsSplitRefundExpenseReturnAdvanceAndWithdrawalOnce()
    {
        var owner=await CommercialOwner();var month=await ReportingMonth(today,1000);
        var cashier=await db.Employees.SingleAsync(x=>x.UserId==actor.Id);
        var cash=await Account();var insta=await Account(FinancialAccountType.InstaPay);
        var sale=await Finance().CreateAsync(Purchase(new(cash.Id,700),new(insta.Id,500)));
        await Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),100,[new(cash.Id,50),new(insta.Id,50)],"Refund"));
        var category=new ExpenseCategory{Name="Equipment"};db.Add(category);await db.SaveChangesAsync();
        var expenses=new ExpenseService(db,user,access);
        var paid=await expenses.RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,200,"=literal receipt",ReceiptReference:"receipt-1"));
        var reversal=await expenses.ReverseAsync(paid.Id,new(Guid.NewGuid(),75,true,"Returned","return-1"));
        await expenses.RecordAdvanceAsync(new(Guid.NewGuid(),cashier.Id,cash.Id,100,"Basic advance"));
        await new VaultService(db,user,access).WithdrawAsync(new(Guid.NewGuid(),25,"Owner actual withdrawal"));
        var report=await Monthly().ReportAsync(month.Id);
        Assert.Equal(8,report.Transactions.Count);Assert.Equal(1275,report.Summary.LedgerCredits);Assert.Equal(425,report.Summary.LedgerDebits);
        Assert.Equal(1200,report.Summary.GrossCollections);Assert.Equal(100,report.Summary.Refunds);Assert.Equal(125,report.Summary.OrdinaryExpenses);
        Assert.Equal(100,report.Summary.AdvanceCashPayouts);Assert.Equal(-25,report.Summary.NetOperatingResult);
        Assert.All(report.Transactions.Where(x=>x.SaleId!=null),x=>{Assert.Equal(owner.Id,x.CommercialSalesOwnerId);Assert.Equal(cashier.Id,x.PerformingEmployeeId);Assert.Equal(actor.Id,x.ActorUserId);});
        var returned=Assert.Single(report.Transactions,x=>x.SourceType=="Expense"&&x.Direction=="Credit");
        Assert.Equal(reversal.Id,returned.ReversalId);Assert.Equal("return-1",returned.Receipt);Assert.Contains("Expense.ReturnRecorded",returned.AuditEvidence);
        Assert.Equal(new[]{"Cash","InstaPay"},report.Transactions.Where(x=>x.SourceType=="Payment").Select(x=>x.PaymentMethod).Order());
        Assert.Equal(report.Transactions.Select(x=>x.LedgerId).Distinct().Count(),report.Transactions.Count);
        await CloseReportingShifts();await Monthly().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Freeze"));
        var download=await Monthly().ExportAsync(month.Id);
        using var archive=new ZipArchive(new MemoryStream(download.Bytes));using var stream=archive.GetEntry("xl/worksheets/sheet2.xml")!.Open();var xml=XDocument.Load(stream);
        XNamespace ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        Assert.Empty(xml.Descendants(ns+"f"));Assert.Contains("=literal receipt",xml.ToString());
    }
    [Fact] public async Task MonthlyIntegrationManagerPersonalFinalizedSalesStatementsRemainIndependentAndImmutable()
    {
        var owner=await CommercialOwner();var manager=new ApplicationUser{UserName="manager-sales"};var role=new IdentityRole<Guid>("Manager"){Id=Guid.NewGuid()};
        db.AddRange(manager,role);owner.UserId=manager.Id;db.Add(new IdentityUserRole<Guid>{UserId=manager.Id,RoleId=role.Id});await db.SaveChangesAsync();
        var month=await ReportingMonth(today.AddDays(1));var period=await Commissions().OpenAsync(new(Guid.NewGuid(),"Personal targets",today,today));
        var sale=await Collected(20000);await Collected(30000);
        clock.Now=clock.Now.AddDays(1);await Commissions().FinalizeAsync(period.Id,new(Guid.NewGuid(),0));
        var summaryIds=await db.EmployeeCommissionSummaries.Select(x=>x.Id).OrderBy(x=>x).ToArrayAsync();
        await CloseReportingShifts();var closed=await Monthly().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Close"));
        var report=await Monthly().ReportAsync(month.Id);var personal=Assert.Single(Assert.Single(report.SalesPerformance).Employees);
        Assert.Equal(owner.Id,personal.EmployeeId);Assert.Equal(1000,personal.NewClients.Commission);Assert.Equal(1500,personal.Renewals.Commission);
        Assert.Equal(2500,report.Summary.SalesCommissionRecognized);Assert.Equal(2500,report.Summary.EmployeeClosingOutstanding);
        Assert.All(report.Payroll.Where(x=>x.CommissionSummaryId!=null),x=>Assert.Equal(owner.Id,x.EmployeeId));
        Assert.All(report.Transactions,x=>{Assert.Equal(owner.Id,x.CommercialSalesOwnerId);Assert.NotEqual(owner.Id,x.PerformingEmployeeId);});
        var reopened=await Monthly().ReopenAsync(month.Id,new(Guid.NewGuid(),closed.Version,"Review"));
        await Monthly().CloseAsync(month.Id,new(Guid.NewGuid(),reopened.Version,"Reissue"));
        Assert.Equal(summaryIds,await db.EmployeeCommissionSummaries.Select(x=>x.Id).OrderBy(x=>x).ToArrayAsync());
        Assert.Equal(2500,(await Monthly().ReportAsync(month.Id)).Summary.SalesCommissionRecognized);
        var cash=await Account();await Assert.ThrowsAsync<BusinessRuleException>(()=>Finance().RefundAsync(sale.SaleId,new(Guid.NewGuid(),1,[new(cash.Id,1)],"Final source protected")));
        Assert.Empty(await db.Refunds.ToListAsync());Assert.Equal(2,await db.Payrolls.CountAsync(x=>x.CommissionSummaryId!=null));
    }
    [Fact] public async Task MonthlyIntegrationCoachPartialPayoutsHaveHistoricalOpeningClosingBalancesAndNoDoubleCost()
    {
        await CoachingSetup();var month=await ReportingMonth(today.AddDays(1));var (coach,category)=await CoachingEarned();
        await Coaching().PayAsync(new(Guid.NewGuid(),b.Id,coach,2000,(await Account()).Id,category,"First half"));
        await CloseReportingShifts();await Monthly().CloseAsync(month.Id,new(Guid.NewGuid(),0,"First"));
        var frozen=await Monthly().ReportAsync(month.Id);var row=Assert.Single(frozen.Coaches);
        Assert.Equal(4000,row.ActualSessionEarnings);Assert.Equal(0,row.ExpirationEarnings);Assert.Equal(2000,row.Paid);Assert.Equal(2000,row.ClosingOutstanding);
        Assert.Equal(0,frozen.Summary.OrdinaryExpenses);Assert.Equal(4000,frozen.Summary.PrivateCoachEarningsRecognized);Assert.Equal(2000,frozen.Summary.CoachCashPayouts);
        Assert.Contains("GymCheckInId",Assert.Single(frozen.CoachEarnings).AttributionEvidence);
        Assert.Contains("CoachEarningId",Assert.Single(frozen.Transactions,x=>x.CoachPayoutId!=null).SourceEvidence);
        var next=await Monthly().OpenAsync(new(Guid.NewGuid(),"Next",today.AddDays(2),today.AddDays(31),"Next"));clock.Now=clock.Now.AddDays(1);
        await new ShiftService(db,user,access).OpenAsync(new(Guid.NewGuid(),(await new VaultService(db,user,access).PositionAsync(b.Id)).ExpectedPhysicalCash));
        await Coaching().PayAsync(new(Guid.NewGuid(),b.Id,coach,2000,(await Account()).Id,category,"Remainder"));
        var current=Assert.Single((await Monthly().ReportAsync(next.Id)).Coaches);
        Assert.Equal(2000,current.OpeningOutstanding);Assert.Equal(0,current.Earned);Assert.Equal(2000,current.Paid);Assert.Equal(0,current.ClosingOutstanding);
        Assert.Equal(2000,Assert.Single((await Monthly().ReportAsync(month.Id)).Coaches).ClosingOutstanding);Assert.Single(await db.CoachEarnings.ToListAsync());
    }
    [Fact] public async Task MonthlyIntegrationClassesReportsCompletedOccurrencesIncludingZeroWithoutCompensation()
    {
        await CoachingSetup();var month=await ReportingMonth(today);await ClassesPurchase();var yoga=await ClassOccurrence();var zumba=await ClassOccurrence("Zumba");
        await Coaching().RegisterClassAsync(yoga.Id,new(Guid.NewGuid(),member.Id));clock.Now=clock.Now.AddHours(2);
        await Coaching().CompleteOccurrenceAsync(yoga.Id,new(Guid.NewGuid(),0));await Coaching().CompleteOccurrenceAsync(zumba.Id,new(Guid.NewGuid(),0));
        var report=await Monthly().ReportAsync(month.Id);var coach=Assert.Single(report.Coaches);
        Assert.Equal(2,coach.CompletedClasses);Assert.Equal(1,coach.ClassAttendances);Assert.Equal(.5m,coach.AverageAttendees);Assert.Equal("NotConfigured",coach.ClassesCompensationStatus);
        Assert.Empty(report.CoachEarnings);Assert.Equal(0,report.Summary.PrivateCoachEarningsRecognized);Assert.Equal(1000,report.Summary.NetCollections);
    }
    [Fact] public async Task MonthlyIntegrationTransferredExpiryShowsUnusedQuantityFixedRateAndNoFakeAttendance()
    {
        await CoachingSetup();var month=await ReportingMonth(today.AddDays(3));var sale=await PrivatePurchase(10,1000,2);
        await DeliverPrivate(sale.SubscriptionId);var incoming=await IncomingCoach();
        await new SalaryService(db,user,access).ConfigureAsync(incoming.Id,new(Guid.NewGuid(),0,incoming.HireDate,"Explicit coach basic"));
        await Coaching().TransferAsync(sale.SubscriptionId,new(Guid.NewGuid(),incoming.Id,1,"Transferred package"));clock.Now=clock.Now.AddDays(2);
        var period=await CoachingPeriod(today,today.AddDays(2));var original=(await db.PrivateSubscriptionTransactions.SingleAsync()).OriginalCoachEmployeeId!.Value;
        await Coaching().ConfigureTargetsAsync(new(Guid.NewGuid(),period,original,100,null));
        await Coaching().SettleExpiryAsync(sale.SubscriptionId,new(Guid.NewGuid(),period,"Owner recognizes unused expiry"));await FinalizeCoaching(period);
        var report=await Monthly().ReportAsync(month.Id);var row=Assert.Single(report.Coaches,x=>x.CoachEmployeeId==incoming.Id);
        Assert.Equal(450,row.ExpirationEarnings);Assert.Equal(0,row.ActualSessionEarnings);
        var earning=Assert.Single(report.CoachEarnings,x=>x.CoachEmployeeId==incoming.Id);
        Assert.True(earning.TransferredPackage);Assert.Equal(50,earning.Rate);Assert.Contains("\"RemainingSessions\":9",earning.AttributionEvidence);
        Assert.Contains("Owner recognizes unused expiry",earning.AttributionEvidence);Assert.Single(await db.PrivateSessions.ToListAsync());Assert.Single(await db.CheckIns.ToListAsync());
    }
}
