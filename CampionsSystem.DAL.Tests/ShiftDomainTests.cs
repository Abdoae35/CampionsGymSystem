using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CampionsSystem.DAL.Tests;

public sealed class ShiftDomainTests
{
    [Fact]
    public async Task EmployeeUniquenessAllowsOverlappingColleaguesAndIndependentBranches()
    {
        await using var f = await AuthTestDatabase.CreateAsync(); await using var db = f.CreateContext();
        var actor = new ApplicationUser { UserName = "shift-domain" };
        var branch = new GymBranch { Name = "A", City = new() { Name = "City" } };
        var a = new Employee { FullName = "A", Phone = "shift-a", HireDate = new(2026,1,1) };
        var b = new Employee { FullName = "B", Phone = "shift-b", HireDate = a.HireDate };
        var one = new Shift { Branch = branch, Employee = a, OpenedByUser = actor, OpenedAt = DateTimeOffset.UtcNow };
        var two = new Shift { Branch = branch, Employee = b, OpenedByUser = actor, OpenedAt = one.OpenedAt };
        db.AddRange(one, two); await db.SaveChangesAsync(); Assert.Equal(2, await db.Shifts.CountAsync());
        db.Add(new Shift { Branch = new() { Name = "B", City = branch.City }, EmployeeId = a.Id, OpenedByUserId = actor.Id, OpenedAt = one.OpenedAt });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task OnePhysicalDrawerMappingPerBranchDoesNotCreateEmployeeAccounts()
    {
        await using var f = await AuthTestDatabase.CreateAsync(); await using var db = f.CreateContext();
        var actor = new ApplicationUser { UserName = "drawer-domain" };
        var branch = new GymBranch { Name = "A", City = new() { Name = "City" } };
        var account = new FinancialAccount { Branch = branch, Name = "Drawer" };
        db.Add(new CashDrawer { Branch = branch, FinancialAccount = account, CreatedByUser = actor, OperationId = Guid.NewGuid(), RequestHash = new('A',64), CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(); Assert.Single(await db.FinancialAccounts.ToListAsync());
        var drawer = await db.CashDrawers.SingleAsync(); drawer.FinancialAccountId = Guid.NewGuid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task CashObservationsAreImmutableAndDoNotPostMoney()
    {
        await using var f = await AuthTestDatabase.CreateAsync(); await using var db = f.CreateContext();
        var actor = new ApplicationUser { UserName = "count-domain" };
        var branch = new GymBranch { Name = "A", City = new() { Name = "City" } };
        var account = new FinancialAccount { Branch = branch, Name = "Drawer" };
        var count = new CashCount { Branch = branch, FinancialAccount = account, CreatedByUser = actor, Kind = "Recount", OperationId = Guid.NewGuid(), RequestHash = new('A',64),
            ExpectedCash = 15000, CountedCash = 14800, BookCash = 15000, Difference = -200, OccurredAt = DateTimeOffset.UtcNow, BusinessDate = new(2026,10,11) };
        db.Add(count); await db.SaveChangesAsync(); Assert.Empty(await db.FinancialTransactions.ToListAsync());
        count.CountedCash = 15000; await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task UpgradeKeepsLegacyShiftOwnershipAndMoneyReferencesUnknown()
    {
        await using var f = await AuthTestDatabase.CreateAsync("AddSaleGrantCancellationHistory"); await using var db = f.CreateContext();
        var actor = new ApplicationUser { UserName = "legacy-shift" };
        var branch = new GymBranch { Name = "Legacy", City = new() { Name = "City" } };
        db.AddRange(actor, branch); await db.SaveChangesAsync(); var id = Guid.NewGuid(); var now = DateTimeOffset.UtcNow;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Shifts" ("Id","BranchId","OpenedAt","OpeningCash","Status","OpenedByUserId")
            VALUES ({id},{branch.Id},{now},100,'Open',{actor.Id});
            """);
        var script = db.GetService<IMigrator>().GenerateScript("AddSaleGrantCancellationHistory", "PrepareEmployeeShiftsAndSharedDrawer").Replace("START TRANSACTION;", "").Replace("COMMIT;", "");
        await db.Database.ExecuteSqlRawAsync(script);
        // This deliberately stops at an old migration, so project only columns
        // available there instead of materializing today's entire Shift model.
        var legacy = await db.Shifts.Select(x => new { x.OpeningCash, x.EmployeeId, x.OperationId, x.FinancialAccountId }).SingleAsync();
        Assert.Equal(100, legacy.OpeningCash); Assert.Null(legacy.EmployeeId); Assert.Null(legacy.OperationId); Assert.Null(legacy.FinancialAccountId);
        Assert.Empty(await db.CashDrawers.ToListAsync()); Assert.Empty(await db.CashCounts.Select(x => x.Id).ToListAsync()); Assert.Empty(await db.EmployeeAdvances.Select(x => x.Id).ToListAsync());
    }

    [Fact]
    public void NewMoneyColumnsUseExactPrecisionAndRestrictiveRelationships()
    {
        using var db = new DAL.Data.ApplicationDbContext(new DbContextOptionsBuilder<DAL.Data.ApplicationDbContext>().UseNpgsql("Host=localhost;Database=unused").Options);
        foreach (var type in new[] { typeof(CashCount), typeof(CashMovement), typeof(EmployeeAdvance), typeof(ExpenseReversal), typeof(CashDrawer) })
        {
            var entity = db.Model.FindEntityType(type)!;
            Assert.All(entity.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
            Assert.All(entity.GetProperties().Where(p => p.ClrType == typeof(decimal)), p => { Assert.Equal(18, p.GetPrecision()); Assert.Equal(2, p.GetScale()); });
        }
    }
}
