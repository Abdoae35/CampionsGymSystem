using CampionsSystem.BLL.CheckIns;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CheckInTests
{
    [Fact] public async Task BorrowNeedsConfirmationAndNeverReactivatesOrConsumesExpiredBalance()
    {
        var sub=await Period(AccessMode.SessionBased,4,start:today.AddDays(-30),end:today.AddDays(-1));var end=sub.EndDate;var status=sub.Status;
        var denied=await Entry();Assert.False(denied.Decision.Allowed);Assert.True(denied.Decision.BorrowAvailable);Assert.Empty(await db.BorrowedGymSessions.ToListAsync());
        var first=await Entry(borrow:true);var second=await Entry(borrow:true);var third=await Entry(borrow:true);
        Assert.True(first.Decision.Allowed);Assert.True(second.Decision.Allowed);Assert.Equal("BorrowedGym",first.CheckIn!.Type);Assert.False(first.CheckIn.SessionConsumed);
        Assert.Equal(AccessReasons.BorrowLimitReached,third.Decision.ReasonCode);Assert.False(third.Decision.Allowed);
        Assert.Equal(2,await db.CheckIns.CountAsync());var loans=await db.BorrowedGymSessions.ToListAsync();Assert.Equal(2,loans.Count);
        Assert.All(loans,x=>{Assert.Equal(sub.Id,x.ExpiredSubscriptionId);Assert.Equal(b.Id,x.BranchId);Assert.Equal(BorrowedSessionStatus.Outstanding,x.Status);Assert.NotNull(x.CheckInId);});
        var old=await db.MemberSubscriptions.FindAsync(sub.Id);Assert.Equal(end,old!.EndDate);Assert.Equal(status,old.Status);Assert.Equal(4,old.RemainingSessions);
        Assert.Empty(await db.MemberSubscriptionBenefits.ToListAsync());Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());
        Assert.Equal(2,await db.AuditLogs.CountAsync(x=>x.Action=="BorrowedSessionDebt.Created"));
    }
    [Fact] public async Task BorrowRetryCreatesOneDebtAndEntry()
    {
        await Period(start:today.AddDays(-30),end:today.AddDays(-1));var op=Guid.NewGuid();var first=await Entry(op,true);var again=await Entry(op,true);
        Assert.Equal(first.CheckIn,again.CheckIn);Assert.Single(await db.BorrowedGymSessions.ToListAsync());Assert.Single(await db.CheckIns.ToListAsync());
    }
    [Fact] public async Task FuturePeriodDoesNotGrantEarlyNormalAccessOrHideExpiredBorrowCycle()
    {
        var expired=await Period(start:today.AddDays(-30),end:today.AddDays(-1));await Period(start:today.AddDays(1),end:today.AddDays(30));
        var d=await Evaluate();Assert.False(d.Allowed);Assert.True(d.BorrowAvailable);var entry=await Entry(borrow:true);Assert.Equal(expired.Id,entry.CheckIn!.SubscriptionId);
    }
    [Fact] public async Task CancelledExhaustedFrozenAndWrongCityPeriodsCannotBorrow()
    {
        var s=await Period(AccessMode.SessionBased,0);Assert.False((await Entry(borrow:true)).Decision.Allowed);
        s.StartDate=today.AddDays(-30);s.EndDate=today.AddDays(-1);s.Status=SubscriptionStatus.Cancelled;await db.SaveChangesAsync();Assert.False((await Evaluate()).BorrowAvailable);
        s.Status=SubscriptionStatus.Frozen;await db.SaveChangesAsync();Assert.Equal(AccessReasons.SubscriptionFrozen,(await Evaluate()).ReasonCode);
        s.Status=SubscriptionStatus.Expired;await db.SaveChangesAsync();Assert.False((await Evaluate(c.Id)).BorrowAvailable);Assert.Empty(await db.BorrowedGymSessions.ToListAsync());
    }
}
public sealed partial class CheckInConcurrencyTests
{
    private async Task Expire()
    {
        await using var db=Context();await db.MemberSubscriptions.ExecuteUpdateAsync(s=>s.SetProperty(x=>x.StartDate,new DateOnly(2026,9,10)).SetProperty(x=>x.EndDate,new DateOnly(2026,10,9)));
    }
    [Fact] public async Task ConcurrentBorrowersCannotUseMoreThanOneRemainingSlot()
    {
        await Expire();await Entry(Guid.NewGuid(),true);
        var results=await Task.WhenAll(Entry(Guid.NewGuid(),true),Entry(Guid.NewGuid(),true));Assert.Single(results,x=>x.Decision.Allowed);
        Assert.Equal(AccessReasons.BorrowLimitReached,Assert.Single(results,x=>!x.Decision.Allowed).Decision.ReasonCode);
        await using var db=Context();Assert.Equal(2,await db.BorrowedGymSessions.CountAsync());Assert.Equal(2,await db.CheckIns.CountAsync());
    }
    [Fact] public async Task ConcurrentIdenticalBorrowCreatesOnlyOneDebt()
    {
        await Expire();var op=Guid.NewGuid();var results=await Task.WhenAll(Entry(op,true),Entry(op,true));Assert.All(results,x=>Assert.True(x.Decision.Allowed));
        await using var db=Context();Assert.Single(await db.BorrowedGymSessions.ToListAsync());Assert.Single(await db.CheckIns.ToListAsync());
    }
}
