using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace CampionsSystem.DAL.Tests;
public sealed class SalesMigrationTests
{
    [Fact] public async Task FinanceUpgradePreservesLegacyMoneyAndUnknownWorkflowFacts()
    {
        await using var f = await AuthTestDatabase.CreateAsync("PrepareOperationalCheckIns");
        await using var db = f.CreateContext();
        var user = new ApplicationUser { UserName = "finance-upgrade" };
        var branch = new GymBranch { Name = "Legacy", City = new City { Name = "Legacy" } };
        var member = new Member { MemberCode = "M-FIN-LEGACY", FullName = "Legacy", PrimaryPhone = "finance-upgrade", HomeBranch = branch, CreatedByUser = user };
        db.Add(member); await db.SaveChangesAsync();
        var id = Guid.NewGuid(); var time = new DateTimeOffset(2026, 10, 9, 22, 30, 0, TimeSpan.Zero);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "SaleTransactions" ("Id", "MemberId", "BranchId", "SaleType", "GrossAmount", "DiscountAmount", "NetAmount", "Status", "CreatedByUserId", "CreatedAt")
            VALUES ({id}, {member.Id}, {branch.Id}, 'GymMembership', 1200, 200, 1000, 'Open', {user.Id}, {time});
            """);
        var script = db.GetService<IMigrator>().GenerateScript("PrepareOperationalCheckIns", "PrepareSalesPayments")
            .Replace("START TRANSACTION;", "").Replace("COMMIT;", "");
        await db.Database.ExecuteSqlRawAsync(script); db.ChangeTracker.Clear();
        var sale = await db.SaleTransactions.Select(s => new { s.Id, s.NetAmount, s.DiscountAmount, s.CreatedAt, s.OperationId, s.BusinessDate, s.CommissionEligible }).SingleAsync();
        Assert.Equal(id, sale.Id); Assert.Equal(1000, sale.NetAmount); Assert.Equal(200, sale.DiscountAmount); Assert.Equal(time, sale.CreatedAt);
        Assert.Null(sale.OperationId); Assert.Null(sale.BusinessDate); Assert.Null(sale.CommissionEligible);
        Assert.False(await db.SaleMembershipSnapshots.AnyAsync()); Assert.Empty(await db.Payments.ToListAsync());
        Assert.Empty(await db.Shifts.ToListAsync()); Assert.Empty(await db.CommissionPeriods.ToListAsync());
    }
}
