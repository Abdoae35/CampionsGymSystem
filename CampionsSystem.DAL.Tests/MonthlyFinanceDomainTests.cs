using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;

public sealed partial class EmployeeShiftTests
{
    [Fact] public async Task MonthlyFinanceHistoryIsAppendOnlyAndDoesNotInventMoneyOrGrants()
    {
        var month = new FinancialMonth { BranchId = branch.Id, Name = "October", StartDate = today, EndDate = today.AddDays(29), CreatedAt = clock.Now, CreatedByUserId = a.Id };
        var plan = new MaintenancePlanRevision { BranchId = branch.Id, PlanId = Guid.NewGuid(), Revision = 1, Name = "Equipment",
            ExpectedCost = 15000, FrequencyMonths = 3, FirstDueDate = today.AddMonths(3), OperationId = Guid.NewGuid(), RequestHash = new('A',64), ActorUserId = a.Id, RecordedAt = clock.Now };
        db.AddRange(month,plan); await db.SaveChangesAsync();
        Assert.Empty(await db.FinancialTransactions.ToListAsync()); Assert.Empty(await db.Expenses.ToListAsync());
        plan.ExpectedCost = 1; await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync()); db.ChangeTracker.Clear();
        var row = await db.FinancialMonths.SingleAsync(); row.EndDate = row.EndDate.AddDays(1);
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task SalaryHistoryPreservesOriginalValuesAndOnlyClosesOnce()
    {
        var setting = new EmployeeSalarySetting { EmployeeId = ea.Id, BasicSalary = 6000, EffectiveFrom = today, CreatedByUserId = a.Id };
        db.Add(setting); await db.SaveChangesAsync(); setting.BasicSalary = 9000;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync()); db.ChangeTracker.Clear();
        setting = await db.EmployeeSalarySettings.SingleAsync(); setting.EffectiveTo = today.AddDays(10); await db.SaveChangesAsync();
        setting.EffectiveTo = today.AddDays(20); await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task FinancialMonthOpenSlotIsBranchLocalAndDatabaseRejectsDuplicates()
    {
        db.AddRange(new FinancialMonth { BranchId = branch.Id, Name = "A", StartDate = today, EndDate = today.AddDays(29), CreatedAt = clock.Now, CreatedByUserId = a.Id },
            new FinancialMonth { BranchId = other.Id, Name = "B", StartDate = today, EndDate = today.AddDays(20), CreatedAt = clock.Now, CreatedByUserId = a.Id });
        await db.SaveChangesAsync(); Assert.Equal(2,await db.FinancialMonths.CountAsync());
        db.Add(new FinancialMonth { BranchId = branch.Id, Name = "Duplicate", StartDate = today.AddDays(30), EndDate = today.AddDays(59), CreatedAt = clock.Now, CreatedByUserId = a.Id });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
