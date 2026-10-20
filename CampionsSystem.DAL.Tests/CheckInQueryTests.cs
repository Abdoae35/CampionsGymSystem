using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CheckInTests
{
    [Fact] public async Task HistoryIsPaginatedFilteredAndScopedToVisitedBranch()
    {
        await Period();await Entry();await Entry();await service.CreateAsync(new(Guid.NewGuid(),CheckInMethod.Scanner,MemberCode:member.MemberCode,BranchId:a.Id));
        user.Context=user.Context! with{Permissions=[..CheckInPermissions.Codes],CurrentBranch=new(b.Id,b.Name)};
        var history=await service.GetHistoryAsync(new(){MemberId=member.Id,PageSize=1,From=today,To=today});Assert.Equal(2,history.TotalCount);Assert.Single(history.Items);Assert.Equal(b.Id,history.Items[0].BranchId);
        Assert.Empty((await service.GetHistoryAsync(new(){Method=CheckInMethod.Scanner})).Items);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.GetHistoryAsync(new(){BranchId=a.Id}));
        user.Context=user.Context with{Permissions=[..CheckInPermissions.Codes,"Branches.AccessAll"]};Assert.Equal(3,(await service.GetHistoryAsync(new())).TotalCount);
    }
    [Fact] public async Task CurrentDayHelperFindsEntranceButDoesNotPreventReentry()
    {
        await Period();Assert.Null(await service.GetCurrentGymEntryAsync(member.Id));var entry=await Entry();
        Assert.Equal(entry.CheckIn,await service.GetCurrentGymEntryAsync(member.Id));Assert.Single((await service.GetCurrentBusinessDayAsync(member.Id)).Items);
        await Entry();Assert.Equal(2,(await service.GetCurrentBusinessDayAsync(member.Id)).TotalCount);
        clock.Now=clock.Now.AddDays(1);Assert.Null(await service.GetCurrentGymEntryAsync(member.Id));Assert.Empty((await service.GetCurrentBusinessDayAsync(member.Id)).Items);
    }
    [Fact] public async Task SharedCheckInDetailsResolveCurrentActorNameAcrossEntryAndReads()
    {
        var employee=new Employee{UserId=actor.Id,FullName="Reception Operator",Phone="checkin-operator",HireDate=today.AddYears(-1)};
        db.Add(employee);await db.SaveChangesAsync();
        await Period();
        var entry=await Entry();
        Assert.Equal("Reception Operator",entry.CheckIn!.ActorName);
        Assert.Equal("Reception Operator",(await service.GetCurrentGymEntryAsync(member.Id))!.ActorName);
        Assert.Equal("Reception Operator",Assert.Single((await service.GetCurrentBusinessDayAsync(member.Id)).Items).ActorName);
        Assert.Equal("Reception Operator",Assert.Single((await service.GetHistoryAsync(new(){MemberId=member.Id,PageSize=1})).Items).ActorName);

        employee.FullName="Current Reception Operator";
        employee.EmploymentStatus=EmploymentStatus.Suspended;
        await db.SaveChangesAsync();
        Assert.Equal("Current Reception Operator",Assert.Single((await service.GetHistoryAsync(new(){MemberId=member.Id})).Items).ActorName);
        Assert.Equal("Current Reception Operator",(await service.GetCurrentGymEntryAsync(member.Id))!.ActorName);
    }
    [Fact] public async Task OperationalLookupReusesSearchAndKeepsClientCodesBranchLocal()
    {
        member.PrimaryPhone="01000000001";member.PrimaryPhoneNormalized="+201000000001";member.ClientCode=7;
        var other=new Member{FullName="Other",MemberCode="M-OTHER",PrimaryPhone="02000000001",HomeBranch=b,ClientCode=7,CreatedByUserId=actor.Id};db.Add(other);await db.SaveChangesAsync();
        user.Context=user.Context! with{Permissions=[..CheckInPermissions.Codes],CurrentBranch=new(b.Id,b.Name)};
        var code=await service.FindMembersAsync(new(){Search="7"});Assert.Equal(other.Id,Assert.Single(code.Items).Id);
        var name=await service.FindMembersAsync(new(){Search="Member"});Assert.Equal(member.Id,Assert.Single(name.Items).Id);Assert.Null(name.Items[0].ClientCode);
        Assert.Contains((await service.FindMembersAsync(new(){Search="+20 100 000 0001"})).Items,x=>x.Id==member.Id);
        Assert.Equal(member.Id,Assert.Single((await service.FindMembersAsync(new(){Search=member.MemberCode})).Items).Id);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.FindMembersAsync(new(){Search=""}));
    }
    [Fact] public async Task DebtHistoryShowsCycleAndExactSettlingGrant()
    {
        var old=await BorrowedCycle();var renewal=await RenewCycle(old);var history=await service.GetBorrowedDebtHistoryAsync(member.Id,new(){PageSize=1});
        Assert.Equal(2,history.TotalCount);Assert.Equal(old,Assert.Single(history.Items).ExpiredSubscriptionId);Assert.Equal(renewal.GrantId,history.Items[0].SettledByTransactionId);
        user.Context=user.Context! with{Permissions=[CheckInPermissions.View],CurrentBranch=new(a.Id,a.Name)};Assert.Empty((await service.GetBorrowedDebtHistoryAsync(member.Id,new())).Items);
    }
    [Theory][InlineData(0,25)][InlineData(1,101)][InlineData(int.MaxValue,100)]
    public async Task InvalidPaginationRejected(int page,int size)
    {await Assert.ThrowsAsync<BusinessRuleException>(()=>service.GetHistoryAsync(new(){Page=page,PageSize=size}));}
    [Fact] public async Task LegacyUnknownBusinessDateIsNotGuessedFromUtcTimestamp()
    {
        var sub=await Period();db.Add(new CheckIn{MemberId=member.Id,BranchId=b.Id,MemberSubscriptionId=sub.Id,CheckInType=CheckInType.Gym,CheckedInAt=access.Now,CreatedByUserId=actor.Id});await db.SaveChangesAsync();
        Assert.Null(Assert.Single((await service.GetHistoryAsync(new())).Items).BusinessDate);Assert.Null(await service.GetCurrentGymEntryAsync(member.Id));
    }
}
