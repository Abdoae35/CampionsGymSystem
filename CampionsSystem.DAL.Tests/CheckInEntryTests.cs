using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CheckInTests
{
    [Fact] public async Task FailedCheckInInsertRollsBackSessionDeduction()
    {
        var sub=await Period(AccessMode.SessionBased,1);
        user.Context=user.Context! with{UserId=Guid.NewGuid()};
        await Assert.ThrowsAsync<DbUpdateException>(()=>Entry());
        Assert.Equal(1,(await db.MemberSubscriptions.AsNoTracking().SingleAsync(x=>x.Id==sub.Id)).RemainingSessions);
        Assert.Empty(await db.CheckIns.ToListAsync());Assert.Empty(await db.AuditLogs.ToListAsync());
    }
    [Fact] public async Task EntryPreservesActualMidnightTimestampAndOpenBusinessDate()
    {
        await Period(end:today);clock.Now=new(2026,10,10,22,30,0,TimeSpan.Zero);
        var day=new CampionsSystem.DAL.Entities.BusinessDay{BranchId=b.Id,BusinessDate=today,OpenedAt=clock.Now.AddHours(-12),Status=BusinessDayStatus.Open};
        db.Add(day);await db.SaveChangesAsync();var entry=await Entry();
        Assert.Equal(today,entry.CheckIn!.BusinessDate);Assert.Equal(day.Id,entry.CheckIn.BusinessDayId);Assert.Equal(clock.Now,entry.CheckIn.OccurredAt);
    }
    private Task<CheckInResult> Entry(Guid? op=null,bool borrow=false)=>service.CreateAsync(new(op??Guid.NewGuid(),CheckInMethod.Manual,member.Id,ConfirmBorrow:borrow));
    [Theory][InlineData(AccessMode.Unlimited,null)][InlineData(AccessMode.SessionBased,12)]
    public async Task EveryRealEntryIsRecordedAndOnlySessionBasedDeducts(AccessMode mode,int? sessions)
    {
        var sub=await Period(mode,sessions);var first=await Entry();var second=await Entry();Assert.True(first.Decision.Allowed);Assert.True(second.Decision.Allowed);
        Assert.Equal(2,await db.CheckIns.CountAsync());Assert.Equal(b.Id,first.CheckIn!.BranchId);Assert.Equal(today,first.CheckIn.BusinessDate);
        Assert.Equal(sessions.HasValue?sessions-2:null,(await db.MemberSubscriptions.FindAsync(sub.Id))!.RemainingSessions);
        Assert.Equal(sessions,first.CheckIn.SessionsBefore);Assert.Equal(sessions.HasValue?sessions-1:null,first.CheckIn.SessionsAfter);
        Assert.Equal(a.Id,member.HomeBranchId);Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.EmployeeCommissionSummaries.ToListAsync());
    }
    [Fact] public async Task RetryReturnsOriginalSnapshotAfterFurtherEntryAndExpiry()
    {
        await Period(AccessMode.SessionBased,12);var op=Guid.NewGuid();var first=await Entry(op);await Entry();clock.Now=clock.Now.AddDays(40);
        var retry=await Entry(op);Assert.Equal(first.CheckIn,retry.CheckIn);Assert.Equal(2,await db.CheckIns.CountAsync());Assert.Equal(11,retry.CheckIn!.SessionsAfter);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CreateAsync(new(op,CheckInMethod.Scanner,MemberCode:member.MemberCode)));
    }
    [Fact] public async Task DeniedEntriesNeverWriteOrConsume()
    {
        var sub=await Period(AccessMode.SessionBased,0);var result=await Entry();Assert.False(result.Decision.Allowed);Assert.Null(result.CheckIn);
        Assert.Empty(await db.CheckIns.ToListAsync());Assert.Equal(0,sub.RemainingSessions);Assert.Empty(await db.AuditLogs.ToListAsync());
    }
    [Fact] public async Task ScannerAndManualShareValidationWithoutCoachOverride()
    {
        await Period();Assert.True((await service.CreateAsync(new(Guid.NewGuid(),CheckInMethod.Scanner,MemberCode:member.MemberCode))).Decision.Allowed);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.CreateAsync(new(Guid.NewGuid(),CheckInMethod.Coach,member.Id)));
        member.IsActive=false;await db.SaveChangesAsync();Assert.Equal(AccessReasons.MemberInactive,(await Entry()).Decision.ReasonCode);
    }
    [Fact] public async Task NewCheckInHistoryCannotBeRewritten()
    {
        await Period();var entry=await Entry();var row=await db.CheckIns.SingleAsync();row.BranchId=a.Id;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>db.SaveChangesAsync());
    }
}
