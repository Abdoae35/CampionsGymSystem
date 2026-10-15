using CampionsSystem.BLL.MonthlyFinance;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.DAL.Tests;

public sealed partial class SalesConcurrencyTests
{
    private async Task<(Guid First, Guid Second)> FinalizedPayrollPeriods()
    {
        await using var db = Context();
        var now = new Clock().GetUtcNow();
        var firstEmployee = await db.Employees.SingleAsync(x => x.UserId == actor);
        var firstBranch = await db.GymBranches.SingleAsync(x => x.Id == branch);
        var secondBranch = new GymBranch { Name = "Other payroll branch", CityId = firstBranch.CityId };
        var secondAccount = new FinancialAccount { Branch = secondBranch, Name = "Other drawer", AccountType = FinancialAccountType.Cash };
        var secondDrawer = new CashDrawer { Branch = secondBranch, FinancialAccount = secondAccount, OperationId = Guid.NewGuid(),
            RequestHash = new string('A', 64), CreatedAt = now, CreatedByUserId = actor };
        var secondEmployee = new Employee { FullName = "Other sales employee", Phone = "payroll-other", HireDate = new(2026, 10, 1),
            EmploymentStatus = EmploymentStatus.Active };
        var firstPeriod = new CommissionPeriod { BranchId = branch, Name = "First finalized", StartAt = now.AddDays(-1), EndAt = now,
            Status = CommissionPeriodStatus.Closed, ClosedAt = now };
        var secondPeriod = new CommissionPeriod { Branch = secondBranch, Name = "Second finalized", StartAt = now.AddDays(-1), EndAt = now,
            Status = CommissionPeriodStatus.Closed, ClosedAt = now };
        var firstMonth = new FinancialMonth { BranchId = branch, Name = "First payroll month", StartDate = new(2026, 10, 10),
            EndDate = new(2026, 11, 9), CreatedAt = now, CreatedByUserId = actor };
        var secondMonth = new FinancialMonth { Branch = secondBranch, Name = "Second payroll month", StartDate = new(2026, 10, 10),
            EndDate = new(2026, 11, 9), CreatedAt = now, CreatedByUserId = actor };
        var firstSummary = new EmployeeCommissionSummary { CommissionPeriod = firstPeriod, Employee = firstEmployee, BranchId = branch,
            CommissionType = CommissionType.SalesNew, TotalEligibleAmount = 1000, CommissionAmount = 50, FinalizedAt = now };
        var secondSummary = new EmployeeCommissionSummary { CommissionPeriod = secondPeriod, Employee = secondEmployee, Branch = secondBranch,
            CommissionType = CommissionType.SalesRenewal, TotalEligibleAmount = 1400, CommissionAmount = 70, FinalizedAt = now };
        db.AddRange(secondDrawer, secondEmployee, firstMonth, secondMonth, firstSummary, secondSummary);
        await db.SaveChangesAsync();
        return (firstPeriod.Id, secondPeriod.Id);
    }

    private async Task<IReadOnlyList<PayrollDetails>> RecognizePayroll(RecognizePayrollCommissionsRequest request)
    {
        await using var db = Context();
        return await MonthlyServices(db).Payroll.RecognizeCommissionPeriodAsync(request);
    }

    [Fact] public async Task SameBranchConcurrentAndPostCommitRetriesReturnOneRecognition()
    {
        var periods = await FinalizedPayrollPeriods(); var request = new RecognizePayrollCommissionsRequest(Guid.NewGuid(), periods.First);
        var results = await Task.WhenAll(RecognizePayroll(request), RecognizePayroll(request));
        Assert.Equal(Assert.Single(results[0]).Id, Assert.Single(results[1]).Id);
        Assert.Equal(Assert.Single(results[0]).Id, Assert.Single(await RecognizePayroll(request)).Id); // response-lost retry
        await using var verify = Context();
        Assert.Equal(50, await verify.Payrolls.SumAsync(x => x.CommissionAmount));
        Assert.Single(await verify.Payrolls.ToListAsync());
        Assert.Single(await verify.AuditLogs.Where(x => x.Action == "Payroll.RecognitionOperation").ToListAsync());
    }

    [Fact] public async Task CrossBranchConcurrentReuseRejectsDifferentRequestWithoutDuplicateMarkers()
    {
        var periods = await FinalizedPayrollPeriods(); var operationId = Guid.NewGuid();
        async Task<(IReadOnlyList<PayrollDetails>? Result, Exception? Error)> Attempt(Guid period)
        {
            try { return (await RecognizePayroll(new(operationId, period)), null); }
            catch (Exception error) { return (null, error); }
        }
        var results = await Task.WhenAll(Attempt(periods.First), Attempt(periods.Second));
        Assert.Single(results, x => x.Result != null);
        Assert.Single(results, x => x.Error is BusinessRuleException);
        await using var verify = Context();
        var marker = Assert.Single(await verify.AuditLogs.Where(x => x.Action == "Payroll.RecognitionOperation").ToListAsync());
        var payroll = Assert.Single(await verify.Payrolls.ToListAsync());
        Assert.Equal(marker.BranchId, payroll.BranchId);
        Assert.Contains(payroll.CommissionAmount, new[] { 50m, 70m });
        Assert.Equal(payroll.CommissionAmount, await verify.Payrolls.SumAsync(x => x.CommissionAmount));
    }

    [Fact] public async Task ReusedOperationWithDifferentPeriodIsPredictableAfterCommit()
    {
        var periods = await FinalizedPayrollPeriods(); var operationId = Guid.NewGuid();
        await RecognizePayroll(new(operationId, periods.First));
        await Assert.ThrowsAsync<BusinessRuleException>(() => RecognizePayroll(new(operationId, periods.Second)));
        await using var verify = Context();
        Assert.Single(await verify.Payrolls.ToListAsync());
        Assert.Single(await verify.AuditLogs.Where(x => x.Action == "Payroll.RecognitionOperation").ToListAsync());
    }

    [Fact] public async Task FailedMarkerInsertRollsBackPayrollAndAllowsSameOperationRetry()
    {
        var periods = await FinalizedPayrollPeriods(); var request = new RecognizePayrollCommissionsRequest(Guid.NewGuid(), periods.First);
        await using (var db = Context())
            await db.Database.ExecuteSqlRawAsync("""
                CREATE FUNCTION reject_payroll_marker() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN RAISE EXCEPTION 'forced payroll marker failure'; END; $$;
                CREATE TRIGGER reject_payroll_marker BEFORE INSERT ON "AuditLogs"
                FOR EACH ROW WHEN (NEW."Action" = 'Payroll.RecognitionOperation')
                EXECUTE FUNCTION reject_payroll_marker();
                """);
        await Assert.ThrowsAnyAsync<Exception>(() => RecognizePayroll(request));
        await using (var verify = Context())
        {
            Assert.Empty(await verify.Payrolls.ToListAsync());
            Assert.Empty(await verify.AuditLogs.Where(x => x.Action == "Payroll.RecognitionOperation").ToListAsync());
            await verify.Database.ExecuteSqlRawAsync("DROP TRIGGER reject_payroll_marker ON \"AuditLogs\"; DROP FUNCTION reject_payroll_marker();");
        }
        Assert.Equal(50, Assert.Single(await RecognizePayroll(request)).Commission);
        await using var final = Context();
        Assert.Single(await final.Payrolls.ToListAsync());
        Assert.Single(await final.AuditLogs.Where(x => x.Action == "Payroll.RecognitionOperation").ToListAsync());
    }

    [Fact] public async Task DatabaseRejectsDuplicateRecognitionMarkersAcrossBranches()
    {
        var periods = await FinalizedPayrollPeriods(); var operationId = Guid.NewGuid();
        await using var db = Context();
        var branches = await db.CommissionPeriods.Where(x => x.Id == periods.First || x.Id == periods.Second).Select(x => x.BranchId).ToListAsync();
        db.Add(new AuditLog { Action = "Payroll.RecognitionOperation", EntityType = nameof(CommissionPeriod), EntityId = operationId,
            BranchId = branches[0], UserId = actor, NewValues = "{}" });
        await db.SaveChangesAsync();
        db.Add(new AuditLog { Action = "Payroll.RecognitionOperation", EntityType = nameof(CommissionPeriod), EntityId = operationId,
            BranchId = branches[1], UserId = actor, NewValues = "{}" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
