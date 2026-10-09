using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CheckInTests:IAsyncLifetime
{
    private AuthTestDatabase fixture=null!;private ApplicationDbContext db=null!;private TestBranchUser user=null!;
    private OperationAccess access=null!;private SubscriptionService subscriptions=null!;private CheckInService service=null!;
    private GymBranch a=null!,b=null!,c=null!;private Member member=null!;private MembershipPlan plan=null!;private ApplicationUser actor=null!;
    private readonly DateOnly today=new(2026,10,10);
    private sealed class Clock:TimeProvider {public DateTimeOffset Now=new(2026,10,10,10,0,0,TimeSpan.Zero);public override DateTimeOffset GetUtcNow()=>Now;}
    private Clock clock=null!;
    public async Task InitializeAsync()
    {
        fixture=await AuthTestDatabase.CreateAsync();db=fixture.CreateContext();actor=new(){UserName="checkin-staff"};
        a=new(){Name="A",City=new(){Name="Tanta"}};b=new(){Name="B",City=a.City};c=new(){Name="C",City=new(){Name="Cairo"}};
        member=new(){MemberCode="M-CHECKIN",FullName="Member",PrimaryPhone="ci-phone",HomeBranch=a,CreatedByUser=actor};
        plan=new(){Name="Gym",Branch=a,City=a.City,DurationDays=30,SessionCount=null,Price=1000};db.AddRange(member,plan,b,c);await db.SaveChangesAsync();
        user=new(new(actor.Id,"staff",null,null,[],[..CheckInPermissions.Codes,..SubscriptionPermissions.Codes,"Branches.AccessAll"],new(b.Id,b.Name)));
        clock=new();access=new(user,db,clock,Options.Create(new AuthContextOptions()));subscriptions=new(db,user,access);service=new(db,user,access,subscriptions);
    }
    public async Task DisposeAsync(){await db.DisposeAsync();await fixture.DisposeAsync();}
    private async Task<MemberSubscription> Period(AccessMode mode=AccessMode.Unlimited,int? sessions=null,DateOnly? start=null,DateOnly? end=null)
    {
        var s=new MemberSubscription{MemberId=member.Id,MembershipPlanId=plan.Id,CityId=a.CityId,PurchasedAtBranchId=a.Id,StartDate=start??today,EndDate=end??today.AddDays(29),AccessMode=mode,TotalSessions=sessions,RemainingSessions=sessions};db.Add(s);await db.SaveChangesAsync();return s;
    }
    private Task<AccessDecision> Evaluate(Guid? branch=null)=>service.EvaluateAsync(new(member.Id,null,branch));
    [Fact] public async Task InactiveAndUnknownMembersHaveExactReasons()
    {
        member.IsActive=false;await db.SaveChangesAsync();Assert.Equal(AccessReasons.MemberInactive,(await Evaluate()).ReasonCode);
        Assert.Equal(AccessReasons.MemberNotFound,(await service.EvaluateAsync(new(null,"unknown"))).ReasonCode);
    }
    [Fact] public async Task NoEntitlementDoesNotGrantOrBorrow(){var d=await Evaluate();Assert.Equal(AccessReasons.NoGymEntitlement,d.ReasonCode);Assert.False(d.BorrowAvailable);}
    [Theory][InlineData(AccessMode.Unlimited,null)][InlineData(AccessMode.SessionBased,12)]
    public async Task SameCityAcceptsDifferentRegistrationAndPlanBranches(AccessMode mode,int? sessions)
    {
        await Period(mode,sessions);var d=await Evaluate();Assert.True(d.Allowed);Assert.Equal(b.Id,d.BranchId);Assert.Equal(sessions,d.RemainingSessions);
        Assert.Equal(AccessReasons.AccessCityMismatch,(await Evaluate(c.Id)).ReasonCode);
        Assert.True((await service.EvaluateAsync(new(null,member.MemberCode))).Allowed);
    }
    [Fact] public async Task ZeroSessionsBeforeDateExpiryDoesNotPermitBorrowing()
    {await Period(AccessMode.SessionBased,0);var d=await Evaluate();Assert.Equal(AccessReasons.NoSessionsRemaining,d.ReasonCode);Assert.False(d.BorrowAvailable);}
    [Fact] public async Task DateExpiryPreservesUnusedSessionsAndOffersExplicitBorrow()
    {var s=await Period(AccessMode.SessionBased,4,start:today.AddDays(-30),end:today.AddDays(-1));var d=await Evaluate();Assert.Equal(AccessReasons.SubscriptionExpired,d.ReasonCode);Assert.True(d.BorrowAvailable);Assert.Equal(4,s.RemainingSessions);}
    [Fact] public async Task FrozenAndScheduledPeriodsDoNotGrantAccess()
    {
        var s=await Period();db.Add(new SubscriptionFreeze{MemberSubscriptionId=s.Id,StartDate=today,EndDate=today.AddDays(2),DaysApplied=3,ApprovedByUserId=actor.Id});await db.SaveChangesAsync();Assert.Equal(AccessReasons.SubscriptionFrozen,(await Evaluate()).ReasonCode);
        await db.SubscriptionFreezes.Where(x=>x.MemberSubscriptionId==s.Id).ExecuteDeleteAsync();s.StartDate=today.AddDays(1);s.EndDate=today.AddDays(30);await db.SaveChangesAsync();Assert.Equal(AccessReasons.SubscriptionNotStarted,(await Evaluate()).ReasonCode);
        clock.Now=clock.Now.AddDays(1);Assert.True((await Evaluate()).Allowed);
    }
    [Theory][InlineData(0)][InlineData(1)]
    public async Task ClosureBlocksRegardlessOfCompensation(int compensation)
    {await Period();db.Add(new BranchClosure{BranchId=b.Id,StartDate=today,EndDate=today,ExtensionDays=compensation,AppliesToGym=true,Reason="Closed",CreatedByUserId=actor.Id});await db.SaveChangesAsync();Assert.Equal(AccessReasons.BranchClosed,(await Evaluate()).ReasonCode);}
    [Fact] public async Task OvernightBusinessDateAndSubscriptionStatusAreShared()
    {
        await Period(end:today);clock.Now=new(2026,10,10,22,30,0,TimeSpan.Zero);
        db.Add(new BusinessDay{BranchId=b.Id,BusinessDate=today,OpenedAt=clock.Now.AddHours(-12),Status=BusinessDayStatus.Open});await db.SaveChangesAsync();
        var d=await Evaluate();Assert.True(d.Allowed);Assert.Equal(today,d.BusinessDate);Assert.NotNull(d.BusinessDayId);
    }
    [Fact] public async Task AmbiguousPeriodsFailClosed(){await Period();await Period();Assert.Equal(AccessReasons.AmbiguousEntitlement,(await Evaluate()).ReasonCode);}
    [Fact] public async Task OperationalScopeIsIndependentOfOwnershipPrivileges()
    {
        await Period();user.Context=user.Context! with{Permissions=[..CheckInPermissions.Codes,"Members.OwnSalesCustomers"],CurrentBranch=new(b.Id,b.Name)};
        Assert.True((await Evaluate()).Allowed);await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Evaluate(a.Id));
    }
    [Fact] public async Task EmployeeEntitlementTracksEmploymentAndSurvivesBranchChange()
    {
        var e=new Employee{MemberId=member.Id,FullName="Employee",Phone="employee-ci",HireDate=today.AddYears(-1)};db.Add(e);await db.SaveChangesAsync();
        var s=new MemberSubscription{EmployeeId=e.Id,MemberId=member.Id,MembershipPlanId=plan.Id,PurchasedAtBranchId=a.Id,CityId=a.CityId,StartDate=today};db.Add(s);await db.SaveChangesAsync();
        Assert.True((await Evaluate()).Allowed);user.Context=user.Context! with{CurrentBranch=new(a.Id,a.Name)};Assert.True((await Evaluate()).Allowed);
        e.EmploymentStatus=EmploymentStatus.Suspended;await db.SaveChangesAsync();Assert.Equal(AccessReasons.EmployeeEntitlementInactive,(await Evaluate()).ReasonCode);
    }
}
