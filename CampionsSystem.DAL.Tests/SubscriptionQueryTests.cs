using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class SubscriptionLifecycleTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private TestBranchUser user = null!;
    private OperationAccess access = null!;
    private SubscriptionService service = null!;
    private GymBranch a = null!, b = null!;
    private ApplicationUser actor = null!;
    private Member member = null!;
    private MembershipPlan plan = null!;
    private readonly DateOnly today = new(2026, 10, 10);
    private sealed class Clock : TimeProvider { public DateTimeOffset Now = new(2026,10,10,10,0,0,TimeSpan.Zero); public override DateTimeOffset GetUtcNow() => Now; }
    private Clock clock = null!;
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        actor = new() { UserName = "subscription-manager" };
        a = new() { Name = "A", City = new() { Name = "Tanta" } }; b = new() { Name = "B", City = a.City };
        member = new() { FullName = "Member A", PrimaryPhone = "subscription-member", MemberCode = "SUB-1", HomeBranch = a, CreatedByUser = actor };
        plan = new() { Name = "Original", Branch = b, City = a.City, DurationDays = 30, Price = 1000, AllowFreeze = true, IncludedFreezeDays = 14 };
        db.AddRange(actor, a, b, member, plan); await db.SaveChangesAsync();
        user = new(new(actor.Id, "manager", null, null, [], [..SubscriptionPermissions.Codes, "Branches.AccessAll"], new(b.Id,b.Name)));
        clock = new(); access = new(user, db, clock, Options.Create(new AuthContextOptions())); service = new(db,user,access);
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private async Task<MemberSubscription> Period(AccessMode mode = AccessMode.Unlimited, DateOnly? start = null, DateOnly? end = null, int? sessions = null)
    {
        var s = new MemberSubscription { MemberId = member.Id, MembershipPlanId = plan.Id, CityId = a.CityId,
            PurchasedAtBranchId = b.Id, StartDate = start ?? today, EndDate = end ?? today.AddDays(29), AccessMode = mode,
            TotalSessions = sessions, RemainingSessions = sessions, Status = SubscriptionStatus.Active };
        db.Add(s); await db.SaveChangesAsync(); return s;
    }
    [Theory] [InlineData(-1,"Scheduled")] [InlineData(0,"Active")] [InlineData(29,"Active")] [InlineData(30,"Expired")]
    public async Task InclusiveThirtyDayPeriodAndEffectiveStatus(int elapsed, string expected)
    {
        var s = await Period(); clock.Now = clock.Now.AddDays(elapsed);
        Assert.Equal(expected, (await service.GetDetailsAsync(s.Id)).Subscription.Status);
        Assert.Equal(30, s.EndDate!.Value.DayNumber - s.StartDate.DayNumber + 1);
    }
    [Fact] public async Task SessionBasedDateExpiryWinsOverUnusedSessions()
    {
        var s = await Period(AccessMode.SessionBased, end: today.AddDays(-1), start: today.AddDays(-30), sessions: 4);
        Assert.Equal("Expired", (await service.GetDetailsAsync(s.Id)).Subscription.Status);
        Assert.False((await service.GetCurrentEntitlementAsync(member.Id,b.Id)).HasAccess);
        Assert.Equal(4,(await db.MemberSubscriptions.FindAsync(s.Id))!.RemainingSessions);
    }
    [Fact] public async Task ExhaustedSessionsDoNotGrantAccess()
    {
        var s = await Period(AccessMode.SessionBased, sessions: 0);
        Assert.Equal("Expired", (await service.GetDetailsAsync(s.Id)).Subscription.Status);
    }
    [Fact] public async Task LegacyFrozenStateWithoutDatedHistoryFailsClosed()
    {
        var s=await Period();s.Status=SubscriptionStatus.Frozen;await db.SaveChangesAsync();
        var current=await service.GetCurrentEntitlementAsync(member.Id,b.Id);
        Assert.False(current.HasAccess);Assert.Equal("Frozen",current.Subscription!.Status);
    }
    [Fact] public async Task FrozenAndScheduledSubscriptionsDoNotGrantCurrentAccess()
    {
        var s = await Period(); db.Add(new SubscriptionFreeze { MemberSubscriptionId=s.Id, StartDate=today, EndDate=today.AddDays(2),
            DaysApplied=3, ApprovedByUserId=actor.Id }); await db.SaveChangesAsync();
        Assert.Equal("Frozen",(await service.GetDetailsAsync(s.Id)).Subscription.Status);
        Assert.False((await service.GetCurrentEntitlementAsync(member.Id,b.Id)).HasAccess);
        var next = await Period(start: today.AddDays(33),end:today.AddDays(62));
        Assert.Equal("Scheduled", (await service.GetDetailsAsync(next.Id)).Subscription.Status);
        Assert.Equal(1,(await service.GetHistoryAsync(member.Id,new(){Status=SubscriptionStatus.Scheduled})).TotalCount);
    }
    [Fact] public async Task CurrentLookupUsesSnapshotCityAndBranchAdministrationUsesOrigin()
    {
        var s = await Period(); user.Context=user.Context! with { Permissions=[SubscriptionPermissions.View],CurrentBranch=new(a.Id,a.Name) };
        Assert.True((await service.GetCurrentEntitlementAsync(member.Id,a.Id)).HasAccess);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.GetDetailsAsync(s.Id));
        Assert.Empty((await service.GetHistoryAsync(member.Id,new())).Items);
        var cairo=new GymBranch{Name="Cairo",City=new(){Name="Cairo"}};db.Add(cairo);await db.SaveChangesAsync();
        user.Context=user.Context! with { CurrentBranch=new(cairo.Id,cairo.Name) };
        Assert.False((await service.GetCurrentEntitlementAsync(member.Id,cairo.Id)).HasAccess);
    }
    [Fact] public async Task OvernightOpenBusinessDayPreventsPrematureDateExpiry()
    {
        var s = await Period(end:today); clock.Now=new(2026,10,10,22,30,0,TimeSpan.Zero); // next Cairo calendar date
        db.Add(new BusinessDay{BranchId=b.Id,BusinessDate=today,OpenedAt=clock.Now.AddHours(-15),Status=BusinessDayStatus.Open});await db.SaveChangesAsync();
        Assert.Equal(today,await service.EffectiveBusinessDateAsync(b.Id));
        Assert.Equal("Active",(await service.GetDetailsAsync(s.Id)).Subscription.Status);
    }
    [Fact] public async Task InactiveEmploymentStopsOpenEntitlementWithoutFarFutureDates()
    {
        var e=new Employee{FullName="Staff",Phone="staff",MemberId=member.Id,HireDate=today.AddYears(-1)};
        var ep=new MembershipPlan{Name="Staff",Branch=b,CityId=a.CityId,PlanCategory=PlanCategory.Employee};
        var s=new MemberSubscription{Employee=e,MemberId=member.Id,MembershipPlan=ep,CityId=a.CityId,PurchasedAtBranchId=b.Id,
            StartDate=today,Status=SubscriptionStatus.Active};db.Add(s);await db.SaveChangesAsync();
        Assert.True((await service.GetCurrentEntitlementAsync(member.Id,b.Id)).HasAccess);
        e.EmploymentStatus=EmploymentStatus.Suspended;await db.SaveChangesAsync();
        Assert.False((await service.GetCurrentEntitlementAsync(member.Id,b.Id)).HasAccess);Assert.Null(s.EndDate);
    }
}
