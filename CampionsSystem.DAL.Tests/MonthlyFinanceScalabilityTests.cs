using CampionsSystem.BLL.MonthlyFinance;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.DAL.Tests;

public sealed partial class EmployeeShiftTests
{
    [Fact] public async Task CurrentPaymentAgainstPriorPayrollReducesCarryForwardWithoutOldPayrollDetail()
    {
        var previous=await Month(today,6000);
        await Months().CloseAsync(previous.Id,new(Guid.NewGuid(),0,"Close prior month"));
        clock.Now=clock.Now.AddDays(1);
        var current=await Months().OpenAsync(new(Guid.NewGuid(),"Current",today.AddDays(1),today.AddDays(1),"Next month"));
        var entitlement=await db.Payrolls.SingleAsync(x=>x.FinancialMonthId==previous.Id&&x.EmployeeId==ea.Id);
        await Payroll().RecordPaymentAsync(entitlement.Id,new(Guid.NewGuid(),2000,clock.Now,"Deferred salary payment"));
        var report=await Months().ReportAsync(current.Id);
        Assert.Equal(12000,report.Summary.EmployeeOpeningOutstanding);
        Assert.Equal(2000,report.Summary.EmployeePaymentRecords);
        Assert.Equal(22000,report.Summary.EmployeeClosingOutstanding);
        Assert.Equal(entitlement.Id,Assert.Single(report.EmployeePayments).PayrollId);
        Assert.Empty(report.Payroll);Assert.Empty(report.Transactions);
    }

    [Fact] public async Task HistoricalPayrollPaymentsBeyondDetailLimitRemainCarryForwardOnly()
    {
        var previous=await Month(today,6000);
        await Months().CloseAsync(previous.Id,new(Guid.NewGuid(),0,"Close prior month"));
        var frozen=await Months().ExportAsync(previous.Id);
        var entitlement=await db.Payrolls.SingleAsync(x=>x.FinancialMonthId==previous.Id&&x.EmployeeId==ea.Id);
        // Bulk historical evidence in this disposable schema exercises the old
        // 50,000-row failure without invoking the payment endpoint 50,001 times.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "PayrollPaymentRecords" ("Id","PayrollId","FinancialMonthId","OperationId","RequestHash","Amount","BalanceBefore","BalanceAfter","PaidAt","BusinessDate","RecordedAt","ActorUserId","Reason")
            SELECT gen_random_uuid(),{entitlement.Id},{previous.Id},gen_random_uuid(),repeat('a',64),0.01,
                   6000.00-(n-1)*0.01,6000.00-n*0.01,{clock.Now},{today},{clock.Now},{a.Id},'Historical test payment'
            FROM generate_series(1,50001) AS n;
            """);
        clock.Now=clock.Now.AddDays(1);
        var current=await Months().OpenAsync(new(Guid.NewGuid(),"Current",today.AddDays(1),today.AddDays(1),"Next month"));
        var report=await Months().ReportAsync(current.Id);
        Assert.Equal(11499.99m,report.Summary.EmployeeOpeningOutstanding);
        Assert.Equal(23499.99m,report.Summary.EmployeeClosingOutstanding);
        Assert.Empty(report.Payroll);Assert.Empty(report.EmployeePayments);Assert.Empty(report.Transactions);
        Assert.Equal(frozen.Bytes,(await Months().ExportAsync(previous.Id)).Bytes);
    }
}

public sealed partial class SalesTests
{
    [Fact] public async Task HistoricalCoachEarningsBeyondDetailLimitRemainCarryForwardOnly()
    {
        await CoachingSetup();
        var previous=await ReportingMonth(today.AddDays(1));
        var (coach,_)=await CoachingEarned();
        await CloseReportingShifts();await Monthly().CloseAsync(previous.Id,new(Guid.NewGuid(),0,"Freeze prior month"));
        var frozen=await Monthly().ExportAsync(previous.Id);
        var original=await db.CoachEarnings.SingleAsync(x=>x.CoachEmployeeId==coach);
        var originalSession=await db.PrivateSessions.SingleAsync(x=>x.Id==original.PrivateSessionId);
        // Synthetic legacy rows are inserted only into this isolated test schema.
        // The query must aggregate historical value without loading its detail.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            WITH added_sessions AS (
              INSERT INTO "PrivateSessions" ("Id","PrivateSubscriptionId","PrivateSubscriptionTransactionId","CoachEmployeeId","GymCheckInId","SessionAt","CreatedByUserId")
              SELECT gen_random_uuid(),{originalSession.PrivateSubscriptionId},{originalSession.PrivateSubscriptionTransactionId},
                     {coach},{originalSession.GymCheckInId},{originalSession.SessionAt},{actor.Id}
              FROM generate_series(1,50001)
              RETURNING "Id"
            )
            INSERT INTO "CoachEarnings" ("Id","BranchId","CoachEmployeeId","CommissionPeriodId","SaleTransactionId","PrivateSessionId",
              "RecognitionOperationId","RequestHash","EligibleValue","Rate","Amount","TransferredPackage","TargetProgress","BusinessDate","CreatedByUserId","CreatedAt","FinancialMonthId")
            SELECT gen_random_uuid(),{b.Id},{coach},{original.CommissionPeriodId},{original.SaleTransactionId},"Id",
                   gen_random_uuid(),repeat('a',64),0.02,50,0.01,true,0,{original.BusinessDate},{actor.Id},{original.CreatedAt},{previous.Id}
            FROM added_sessions;
            """);
        clock.Now=clock.Now.AddDays(2);
        var current=await Monthly().OpenAsync(new(Guid.NewGuid(),"Current",today.AddDays(2),today.AddDays(2),"Next month"));
        var report=await Monthly().ReportAsync(current.Id);
        var row=Assert.Single(report.Coaches,x=>x.CoachEmployeeId==coach);
        Assert.Equal(4500.01m,row.OpeningOutstanding);
        Assert.Equal(row.OpeningOutstanding,row.ClosingOutstanding);
        Assert.Equal(0,row.Earned);Assert.Empty(report.CoachEarnings);Assert.Empty(report.CoachPayments);
        Assert.Empty(report.Transactions);Assert.Equal(frozen.Bytes,(await Monthly().ExportAsync(previous.Id)).Bytes);
    }
}
