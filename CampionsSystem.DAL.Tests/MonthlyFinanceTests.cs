using System.IO.Compression;
using System.Xml.Linq;
using CampionsSystem.BLL.MonthlyFinance;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace CampionsSystem.DAL.Tests;
public sealed partial class EmployeeShiftTests
{
    private MonthlyFinanceService Months()=>new(db,user,access);
    private SalaryService Salaries()=>new(db,user,access);
    private PayrollService Payroll()=>new(db,user,access);
    private MaintenanceService Maintenance()=>new(db,user,access);
    private async Task MaintenanceOwner()
    {
        var role=new IdentityRole<Guid>("Owner") { NormalizedName="OWNER" };
        db.Add(role);db.Add(new IdentityUserRole<Guid>{UserId=a.Id,RoleId=role.Id});
        await db.SaveChangesAsync();
    }
    private void FinanceAccess()=>user.Context=user.Context! with{Permissions=[..user.Context.Permissions,..MonthlyFinancePermissions.Codes,"Payroll.Manage"]};
    private async Task<FinancialMonthDetails> Month(DateOnly? end=null,decimal salary=6000)
    {
        FinanceAccess();await Salaries().ConfigureAsync(ea.Id,new(Guid.NewGuid(),salary,ea.HireDate,"Explicit salary"));
        await Salaries().ConfigureAsync(eb.Id,new(Guid.NewGuid(),salary,eb.HireDate,"Explicit salary"));
        return await Months().OpenAsync(new(Guid.NewGuid(),"Branch month",today,end??today.AddDays(29),"Initial month"));
    }
    [Fact] public async Task MonthlyClosingRejectsOversizedLedgerAtomically()
    {
        var month=await Month(today,0);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "FinancialTransactions" ("Id","FinancialAccountId","BranchId","SourceType","TransactionType","Amount","OccurredAt","CreatedByUserId","BusinessDate","FinancialMonthId")
            SELECT gen_random_uuid(),{cash.Id},{branch.Id},'Adjustment','Credit',0,{clock.Now},{a.Id},{today},{month.Id}
            FROM generate_series(1,50001);
            """);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Too many rows")));
        Assert.Equal("Open",(await Months().GetAsync(month.Id)).Status);Assert.Empty(await db.Payrolls.ToListAsync());Assert.Empty(await db.FinancialClosingRevisions.ToListAsync());
        Assert.Equal(50001,await db.FinancialTransactions.CountAsync());Assert.Equal(1,(await Months().EventsAsync(month.Id)).Total);
    }
    [Fact] public async Task WorkbookPreservesLongUnicodeEvidenceAsLiteralTextAcrossNumberedColumns()
    {
        var month=await Month(today,0);var category=new ExpenseCategory{Name="Equipment"};db.Add(category);await db.SaveChangesAsync();
        await Service().OpenAsync(new(Guid.NewGuid(),100000));await Expenses().RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,1,"=literal"));
        var report=await Months().ReportAsync(month.Id);var text=new string('a',29999)+"💪"+new string('ب',10000);
        report=report with{Transactions=[report.Transactions[0] with{AuditEvidence=text}]};
        var bytes=MonthlyWorkbook.Create(report);using var zip=new ZipArchive(new MemoryStream(bytes));using var stream=zip.GetEntry("xl/worksheets/sheet2.xml")!.Open();var xml=XDocument.Load(stream);
        XNamespace ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var rows=xml.Descendants(ns+"row").ToArray();var headings=rows[2].Elements(ns+"c").Select(x=>x.Value).ToArray();var values=rows[3].Elements(ns+"c").Select(x=>x.Value).ToArray();
        Assert.Equal(text,string.Concat(headings.Select((h,i)=>(h,i)).Where(x=>x.h.StartsWith("AuditEvidence_")).Select(x=>values[x.i])));
        Assert.Empty(xml.Descendants(ns+"f"));Assert.All(rows.SelectMany(x=>x.Elements(ns+"c")),x=>Assert.True(x.Value.Length<=30000));
    }
    [Fact] public async Task MonthlyClosingRequiresAllBranchShiftsAndAllowsOptionalNonblockingShortage()
    {
        var month=await Month(today);var one=await Service().OpenAsync(new(Guid.NewGuid(),100000));Actor(b);var two=await Service().OpenAsync(new(Guid.NewGuid(),100000));
        var close=new CloseFinancialMonthRequest(Guid.NewGuid(),0,"End month",99800);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Months().CloseAsync(month.Id,close));
        await Service().CloseAsync(two.Id,new(Guid.NewGuid(),two.Version,100000));Actor(a);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Months().CloseAsync(month.Id,close));
        await Service().CloseAsync(one.Id,new(Guid.NewGuid(),one.Version,100000));
        var closed=await Months().CloseAsync(month.Id,close);Assert.Equal("Closed",closed.Status);
        Assert.Equal(closed.Id,(await Months().CloseAsync(month.Id,close)).Id);Assert.Single(await db.FinancialClosingRevisions.ToListAsync());
        var report=await Months().ReportAsync(month.Id);Assert.Equal(100000,report.Summary.BookCash);Assert.Equal(99800,report.Summary.ExpectedPhysicalCash);
        Assert.Equal(-200,report.Summary.PhysicalVsBook);Assert.Equal(-200,report.Summary.PeriodObservedDiscrepancies);
        Assert.Empty(await db.FinancialTransactions.ToListAsync());Assert.Empty(await db.Expenses.ToListAsync());
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Service().OpenAsync(new(Guid.NewGuid(),99800)));
    }
    [Fact] public async Task ClosingWithoutRecountPreservesDrawerAndSalaryEarnedIsNotPaid()
    {
        var month=await Month();clock.Now=clock.Now.AddDays(29);await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Close without recount"));
        Assert.Empty(await db.CashCounts.ToListAsync());Assert.Empty(await db.FinancialTransactions.ToListAsync());
        var report=await Months().ReportAsync(month.Id);Assert.Equal(12000,report.Summary.BasicSalaryEarned);Assert.Equal(-12000,report.Summary.NetOperatingResult);
        Assert.Equal(12000,report.Summary.EmployeeClosingOutstanding);Assert.Equal(0,report.Summary.EmployeePaymentRecords);
        Assert.All(report.Payroll,p=>Assert.Equal("Outstanding",p.Status));
    }
    [Fact] public async Task ReopenPreservesEarlierRevisionAndExactSixSheetWorkbook()
    {
        var month=await Month(today);var closed=await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"First close"));
        var original=await Months().ExportAsync(month.Id);var old=await Months().ReportAsync(month.Id);
        ea=await db.Employees.SingleAsync(x=>x.Id==ea.Id);ea.FullName="Later name";await db.SaveChangesAsync();
        var reopened=await Months().ReopenAsync(month.Id,new(Guid.NewGuid(),closed.Version,"Investigate"));
        Assert.Equal("Reopened",reopened.Status);Assert.Equal(original.Bytes,(await Months().ExportAsync(month.Id,1)).Bytes);
        Assert.Equal(old.Payroll.First().EmployeeName,(await Months().ReportAsync(month.Id,1)).Payroll.First().EmployeeName);
        await Months().CloseAsync(month.Id,new(Guid.NewGuid(),reopened.Version,"Reissue"));Assert.Equal(2,(await Months().RevisionsAsync(month.Id)).Count);
        Assert.Equal(original.Bytes,(await Months().ExportAsync(month.Id,1)).Bytes);Assert.Equal(2,await db.Payrolls.CountAsync());
        using var zip=new ZipArchive(new MemoryStream(original.Bytes));Assert.Equal(11,zip.Entries.Count);
        using var wb=zip.GetEntry("xl/workbook.xml")!.Open();var xml=XDocument.Load(wb);XNamespace ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        Assert.Equal(new[]{"Monthly Summary","All Financial Transactions","Sales Performance","Employee Salaries & Advances","Coaches - Private - Classes","Vault & Shifts"},xml.Descendants(ns+"sheet").Select(x=>x.Attribute("name")!.Value));
        foreach(var entry in zip.Entries){using var stream=entry.Open();Assert.NotNull(XDocument.Load(stream).Root);}
        Assert.Equal(4,(await Months().EventsAsync(month.Id)).Total); // opened, closed, reopened, closed
    }
    [Fact] public async Task LateExpensePostsToNextOpenMonthWithSeparateOriginalDateAndFrozenPriorReport()
    {
        var month=await Month(today,0);var closed=await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Close"));var before=await Months().ExportAsync(month.Id);
        var next=await Months().OpenAsync(new(Guid.NewGuid(),"Next",today.AddDays(1),today.AddDays(30),"Next open"));
        await Service().OpenAsync(new(Guid.NewGuid(),100000));var category=new ExpenseCategory{Name="Maintenance"};db.Add(category);await db.SaveChangesAsync();
        await Expenses().RecordPaidAsync(new(Guid.NewGuid(),category.Id,cash.Id,15000,"Actual equipment maintenance"));var ledger=await db.FinancialTransactions.SingleAsync();
        Assert.Equal(today,ledger.BusinessDate);Assert.Equal(next.Id,ledger.FinancialMonthId);Assert.Equal(clock.Now,ledger.PostedAt);
        await Months().RecordOccurrenceAsync(ledger.Id,new(Guid.NewGuid(),clock.Now.AddDays(-5),"Known original occurrence"));
        var current=await Months().ReportAsync(next.Id);Assert.Single(current.Transactions);Assert.Equal(clock.Now.AddDays(-5),current.Transactions[0].OriginalOccurredAt);
        Assert.Equal(15000,current.Summary.OrdinaryExpenses);Assert.Equal(before.Bytes,(await Months().ExportAsync(month.Id)).Bytes);
        Assert.Empty((await Months().ReportAsync(month.Id)).Transactions);
    }
    [Fact] public async Task SalaryProrationAndTransferSplitBasicByBranchDaysAndKeepHistory()
    {
        var month=await Month();var assignment=await db.EmployeeBranchAssignments.SingleAsync(x=>x.EmployeeId==ea.Id);
        assignment.EndDate=today.AddDays(14);db.Add(new EmployeeBranchAssignment{EmployeeId=ea.Id,BranchId=other.Id,StartDate=today.AddDays(15),AssignedByUserId=a.Id});await db.SaveChangesAsync();
        var second=new FinancialMonth{BranchId=other.Id,Name="Other",StartDate=month.StartDate,EndDate=month.EndDate,CreatedByUserId=a.Id,CreatedAt=clock.Now};db.Add(second);await db.SaveChangesAsync();
        var first=await Salaries().PreviewAsync(month.Id,ea.Id);var transferred=await Salaries().PreviewAsync(second.Id,ea.Id);
        Assert.Equal(15,first.EligibleDays);Assert.Equal(3000,first.BasicSalary);Assert.Equal(3000,transferred.BasicSalary);
        await Salaries().ConfigureAsync(ea.Id,new(Guid.NewGuid(),9000,today.AddDays(30),"Future raise"));Assert.Equal(3000,(await Salaries().PreviewAsync(month.Id,ea.Id)).BasicSalary);
        Assert.Equal(2,(await Salaries().HistoryAsync(ea.Id)).Count);
    }
    [Fact] public async Task AdvancesAreCappedByBasicSalaryAndDeductedExactlyOnceAcrossReopen()
    {
        var month=await Month(today,1000);var shift=await Service().OpenAsync(new(Guid.NewGuid(),100000));
        var first=new AdvanceRequest(Guid.NewGuid(),ea.Id,cash.Id,600,"Advance");var saved=await Expenses().RecordAdvanceAsync(first);
        Assert.Equal(saved.Id,(await Expenses().RecordAdvanceAsync(first)).Id);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Expenses().RecordAdvanceAsync(first with{OperationId=Guid.NewGuid(),Amount=401}));
        await Expenses().RecordAdvanceAsync(first with{OperationId=Guid.NewGuid(),Amount=400});await Service().CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,99000));
        var closed=await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Close"));Assert.Equal(1000,(await Payroll().HistoryAsync(employeeId:ea.Id)).Items.Single().AdvancesDeducted);
        Assert.Equal(0,(await Payroll().HistoryAsync(employeeId:ea.Id)).Items.Single().Outstanding);Assert.Equal(2,await db.PayrollDeductions.CountAsync());
        var reopened=await Months().ReopenAsync(month.Id,new(Guid.NewGuid(),closed.Version,"Verify"));await Months().CloseAsync(month.Id,new(Guid.NewGuid(),reopened.Version,"Reclose"));
        Assert.Equal(2,await db.PayrollDeductions.CountAsync());Assert.Equal(2,await db.FinancialTransactions.CountAsync());
        Assert.Equal(1000,(await db.EmployeeAdvances.OrderBy(x=>x.Amount).FirstAsync()).BasicEntitlementSnapshot);
    }
    [Fact] public async Task OwnerExternalPartialPaymentsRemainOutstandingAndNeverPostCash()
    {
        var month=await Month(today);await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Close"));
        var next=await Months().OpenAsync(new(Guid.NewGuid(),"Next",today.AddDays(1),today.AddDays(30),"Next"));
        var entitlement=(await Payroll().HistoryAsync(employeeId:ea.Id)).Items.Single();var pay=new RecordPayrollPaymentRequest(Guid.NewGuid(),2000,clock.Now,"Actually paid", "receipt");
        var first=await Payroll().RecordPaymentAsync(entitlement.Id,pay);Assert.Equal(4000,first.BalanceAfter);Assert.Equal(next.Id,first.FinancialMonthId);
        Assert.Equal(first.Id,(await Payroll().RecordPaymentAsync(entitlement.Id,pay)).Id);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Payroll().RecordPaymentAsync(entitlement.Id,pay with{OperationId=Guid.NewGuid(),Amount=4000.01m}));
        await Payroll().RecordPaymentAsync(entitlement.Id,pay with{OperationId=Guid.NewGuid(),Amount=4000});Assert.Equal(0,(await Payroll().HistoryAsync(employeeId:ea.Id)).Items.Single().Outstanding);
        Assert.Empty(await db.FinancialTransactions.ToListAsync());Assert.Empty(await db.Expenses.ToListAsync());Assert.Equal(100000,(await Vault().PositionAsync()).BookCash);
        Assert.Equal(12000,(await Months().ReportAsync(month.Id)).Summary.EmployeeClosingOutstanding);
    }
    [Fact] public async Task MaintenancePlanningIsInformationalAndActualExpenseIsPaidOnce()
    {
        await MaintenanceOwner();
        var month=await Month(today,0);var plan=new MaintenancePlanRequest(Guid.NewGuid(),branch.Id,"Equipment",15000,3,today.AddMonths(3),"Expected maintenance");
        var forecast=await Maintenance().ConfigureAsync(plan);Assert.Equal(5000,forecast.SuggestedMonthlyAmount);Assert.Equal("InformationalOnly",forecast.Status);
        var historicalHash=FinancialOperations.Hash(new{plan.OperationId,plan.BranchId,plan.Name,plan.ExpectedCost,plan.FrequencyMonths,
            plan.FirstDueDate,plan.Reason,plan.PlanId,plan.ExpectedRevision,plan.IsActive});
        Assert.Equal(historicalHash,(await db.MaintenancePlanRevisions.SingleAsync()).RequestHash);
        Assert.Equal(forecast.Id,(await Maintenance().ConfigureAsync(plan)).Id);Assert.Empty(await db.FinancialTransactions.ToListAsync());
        var shift=await Service().OpenAsync(new(Guid.NewGuid(),100000));var category=new ExpenseCategory{Name="Equipment"};db.Add(category);await db.SaveChangesAsync();
        var paid=new PaidExpenseRequest(Guid.NewGuid(),category.Id,cash.Id,15000,"Actually paid maintenance");await Expenses().RecordPaidAsync(paid);await Expenses().RecordPaidAsync(paid);
        await Service().CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,85000));await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Close"));
        var report=await Months().ReportAsync(month.Id);Assert.Equal(15000,report.Summary.OrdinaryExpenses);Assert.Equal(-15000,report.Summary.NetOperatingResult);
        Assert.Single(report.Transactions);Assert.Single(report.Maintenance);Assert.Single(await db.Expenses.ToListAsync());
    }
    [Fact] public async Task PlannedExpensesSummarizeRoundedReservesAndPersistentDueRemindersByBranch()
    {
        await MaintenanceOwner();FinanceAccess();var service=Maintenance();
        var first=await service.ConfigureAsync(new(Guid.NewGuid(),branch.Id,"Equipment",15000,3,today.AddDays(-2),"Plan",Description:"Quarterly servicing"));
        var second=await service.ConfigureAsync(new(Guid.NewGuid(),branch.Id,"Renovation",24000,6,today.AddDays(10),"Plan"));
        var third=await service.ConfigureAsync(new(Guid.NewGuid(),branch.Id,"Rounding",100,3,today,"Plan"));
        Assert.Equal(33.33m,third.SuggestedMonthlyAmount);
        var summary=await service.SummaryAsync(branch.Id,today,30);
        Assert.Equal(9033.33m,summary.TotalSuggestedMonthlyAmount);Assert.Equal(3,summary.ActivePlanCount);
        Assert.Equal(1,summary.OverdueCount);Assert.Equal(1,summary.UpcomingCount);Assert.Equal(1,summary.DueTodayCount);
        var page=await service.RemindersAsync(branch.Id,today,30,pageSize:2);
        Assert.Equal(3,page.Total);Assert.Equal(2,page.Items.Count);Assert.Equal("Overdue",page.Items[0].DueStatus);
        Assert.Equal("Quarterly servicing",page.Items[0].Description);
        Assert.Single((await service.RemindersAsync(branch.Id,today,30,page:2,pageSize:2)).Items);
        Assert.Empty(await db.FinancialTransactions.ToListAsync());Assert.Empty(await db.Expenses.ToListAsync());
        Assert.Equal(first.NextExpectedDate,today.AddDays(-2));Assert.Equal(second.NextExpectedDate,today.AddDays(10));
    }
    [Fact] public async Task PlannedExpenseEditDeactivationAndFrozenMonthKeepSeparateHistory()
    {
        await MaintenanceOwner();var month=await Month(today,0);var service=Maintenance();
        var create=new MaintenancePlanRequest(Guid.NewGuid(),branch.Id,"Equipment",15000,3,today,"Plan",Description:"Initial");
        var first=await service.ConfigureAsync(create);Assert.Equal(first.Id,(await service.ConfigureAsync(create)).Id);
        await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Close"));var frozen=await Months().ExportAsync(month.Id);
        var second=await service.ConfigureAsync(create with{OperationId=Guid.NewGuid(),PlanId=first.PlanId,ExpectedRevision=1,
            ExpectedCost=18000,Description="Revised",NextExpectedDueDate=today.AddMonths(3)});
        Assert.Equal(6000,second.SuggestedMonthlyAmount);Assert.Equal("Upcoming",second.DueStatus);
        var disabled=await service.ConfigureAsync(create with{OperationId=Guid.NewGuid(),PlanId=first.PlanId,ExpectedRevision=2,IsActive=false});
        Assert.Equal("Inactive",disabled.DueStatus);Assert.Equal(3,(await service.HistoryAsync(first.PlanId)).Total);
        Assert.Equal(0,(await service.SummaryAsync(branch.Id)).TotalSuggestedMonthlyAmount);
        Assert.Equal(frozen.Bytes,(await Months().ExportAsync(month.Id)).Bytes);
        Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task PlannedExpenseMutationRequiresOwnerMembershipAndEffectivePermissions()
    {
        FinanceAccess();var request=new MaintenancePlanRequest(Guid.NewGuid(),branch.Id,"Equipment",15000,3,today,"Plan");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Maintenance().ConfigureAsync(request));
        await MaintenanceOwner();user.Context=user.Context! with{Permissions=["Branches.AccessAll",MonthlyFinancePermissions.MaintenanceView]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Maintenance().ConfigureAsync(request));
        user.Context=user.Context! with{Permissions=[..user.Context.Permissions,MonthlyFinancePermissions.MaintenanceManage]};
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Maintenance().ConfigureAsync(request with{ExpectedCost=0}));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Maintenance().ConfigureAsync(request with{FrequencyMonths=0}));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Maintenance().ConfigureAsync(request with{NextExpectedDueDate=today.AddDays(-1)}));
        await Maintenance().ConfigureAsync(request);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Maintenance().ConfigureAsync(request with{Name="Other"}));
        Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task PlannedExpenseBranchReadsRemainIsolatedAndCannotMutateOtherBranch()
    {
        await MaintenanceOwner();FinanceAccess();var service=Maintenance();
        var elsewhere=await service.ConfigureAsync(new(Guid.NewGuid(),other.Id,"Other branch servicing",12000,4,today.AddDays(2),"Plan"));
        Assert.Equal(3000,(await service.SummaryAsync(other.Id)).TotalSuggestedMonthlyAmount);
        Assert.Equal(0,(await service.SummaryAsync(branch.Id)).TotalSuggestedMonthlyAmount);
        user.Context=user.Context! with{Permissions=[MonthlyFinancePermissions.MaintenanceView,MonthlyFinancePermissions.MaintenanceManage]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.SummaryAsync(other.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.RemindersAsync(other.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.HistoryAsync(elsewhere.PlanId));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.ConfigureAsync(new(Guid.NewGuid(),other.Id,"Other",100,2,today,"Attempt")));
    }
    [Fact] public async Task FinanceScopeAndEffectiveCapabilitiesRemainIndependent()
    {
        var month=await Month(today);user.Context=user.Context! with{Permissions=[MonthlyFinancePermissions.View,MonthlyFinancePermissions.Close]};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Months().HistoryAsync(other.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Salaries().ConfigureAsync(ea.Id,new(Guid.NewGuid(),1,today,"Deny")));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Months().ReopenAsync(month.Id,new(Guid.NewGuid(),0,"Deny")));
        await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Authorized branch manager; no global shortcut"));
        Assert.Equal("Closed",(await Months().GetAsync(month.Id)).Status);
    }
}
