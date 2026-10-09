using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    public async Task<HolidayApplicationResult> CreateOfficialHolidayAsync(OfficialHolidayRequest r, CancellationToken ct = default)
    {
        var actor=await RequireAsync(SubscriptionPermissions.Extend,r.BranchId,true,ct);
        if (r.StartDate == default || r.EndDate < r.StartDate || r.EndDate.DayNumber-r.StartDate.DayNumber>=366
            || string.IsNullOrWhiteSpace(r.Reason) || r.Reason.Length>2048)
            throw new BusinessRuleException("Supply an inclusive holiday range of at most 366 days and a reason.");
        if (!await db.GymBranches.AnyAsync(x=>x.Id==r.BranchId && x.IsActive,ct)) throw new BranchNotFoundException("Active holiday branch not found.");
        return await TransactionAsync(async () =>
        {
            // Coordinate operational closure declarations with entrance branch locks without blocking FK key-share readers.
            var branch=await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={r.BranchId} FOR NO KEY UPDATE").AsNoTracking().SingleAsync(ct);
            if(!branch.IsActive)throw new BusinessRuleException("Holiday branch is inactive.");
            // Preserve module applicability. ExtensionDays=0 explicitly disables subscription compensation.
            var closure=new BranchClosure { BranchId=r.BranchId,StartDate=r.StartDate,EndDate=r.EndDate,
                AppliesToGym=true,ExtensionDays=r.ExtendSubscriptions?r.EndDate.DayNumber-r.StartDate.DayNumber+1:0,
                Reason=r.Reason.Trim(),CreatedByUserId=actor.UserId,CreatedAt=access.Now };
            db.Add(closure);
            access.Audit(actor.UserId,r.BranchId,"BranchClosure.OfficialHolidayCreated",nameof(BranchClosure),closure.Id,
                after:new { closure.StartDate,closure.EndDate,r.ExtendSubscriptions,closure.Reason });
            await db.SaveChangesAsync(ct);return await ApplyClosureAsync(closure,actor.UserId,null,ct);
        },ct);
    }
    public async Task<HolidayApplicationResult> ApplyOfficialHolidayAsync(Guid id, CancellationToken ct = default)
    {
        var actor=await RequireAsync(SubscriptionPermissions.Extend,null,true,ct);
        return await TransactionAsync(async () =>
        {
            // Shared closure lock prevents configuration edits; Member locks serialize compensation writers.
            var closure=await db.BranchClosures.FromSqlInterpolated($"SELECT * FROM \"BranchClosures\" WHERE \"Id\"={id} FOR SHARE")
                .AsNoTracking().SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Official holiday not found.");
            return await ApplyClosureAsync(closure,actor.UserId,null,ct);
        },ct);
    }
    private async Task ApplyDeclaredHolidaysForMemberAsync(Guid memberId, Guid actor, CancellationToken ct)
    {
        var scopes=await db.MemberSubscriptions.Where(x=>x.MemberId==memberId && x.EmployeeId==null)
            .Select(x=>x.PurchasedAtBranchId).Distinct().ToListAsync(ct);
        var closures=await db.BranchClosures.AsNoTracking().Where(x=>scopes.Contains(x.BranchId) && x.AppliesToGym && x.ExtensionDays>0)
            .OrderBy(x=>x.StartDate).ThenBy(x=>x.Id).ToListAsync(ct);
        foreach(var closure in closures) await ApplyClosureAsync(closure,actor,memberId,ct);
    }
    private async Task<HolidayApplicationResult> ApplyClosureAsync(BranchClosure closure, Guid actor, Guid? onlyMember, CancellationToken ct)
    {
        if (!closure.AppliesToGym || closure.ExtensionDays==0) return new(closure.Id,0,0);
        var length=closure.EndDate.DayNumber-closure.StartDate.DayNumber+1;
        if (length is <1 or >366 || closure.ExtensionDays!=length)
            throw new BusinessRuleException("Holiday compensation requires one protected day per declared closure day; reconcile legacy closure configuration.");
        var cutoff=await EffectiveBusinessDateAsync(closure.BranchId,ct);var last=closure.EndDate<cutoff?closure.EndDate:cutoff;
        if(last<closure.StartDate)return new(closure.Id,0,0);
        var members=await db.MemberSubscriptions.Where(x=>x.PurchasedAtBranchId==closure.BranchId && x.EmployeeId==null
            && x.EndDate>=closure.StartDate && x.StartDate<=last && (!onlyMember.HasValue || x.MemberId==onlyMember))
            .Select(x=>x.MemberId).Distinct().OrderBy(x=>x).ToListAsync(ct);
        var protectedDays=0;var extendedDays=0;
        foreach(var memberId in members)
        {
            var member=await LockMemberAsync(memberId,ct);if(!member.IsActive)continue;
            var periods=await db.MemberSubscriptions.Where(x=>x.MemberId==memberId && x.PurchasedAtBranchId==closure.BranchId
                && x.EmployeeId==null && x.EndDate!=null).OrderBy(x=>x.StartDate).ToListAsync(ct);
            foreach(var sub in periods)await db.Entry(sub).ReloadAsync(ct);
            for(var day=closure.StartDate;day<=last;day=PlusDays(day,1))
            {
                foreach(var sub in periods.Where(x=>x.StartDate<=day && x.EndDate>=day
                    && x.Status is not (SubscriptionStatus.Cancelled or SubscriptionStatus.SuspendedForInstallment)
                    && (x.AccessMode==AccessMode.Unlimited || x.RemainingSessions>0)))
                {
                    if(await db.SubscriptionHolidayExtensions.AnyAsync(x=>x.MemberSubscriptionId==sub.Id && x.ProtectedDate==day,ct))continue;
                    var frozen=await db.SubscriptionFreezes.AnyAsync(x=>x.MemberSubscriptionId==sub.Id && x.FreezeType==FreezeType.Normal
                        && x.StartDate<=day && (x.EndDate==null || x.EndDate>=day),ct);
                    if(sub.Status==SubscriptionStatus.Frozen && !await db.SubscriptionFreezes.AnyAsync(x=>x.MemberSubscriptionId==sub.Id && x.FreezeType==FreezeType.Normal,ct))
                        throw new BusinessRuleException("Legacy Frozen state has no dated history; reconcile it before compensation.");
                    var old=sub.EndDate!.Value;var days=frozen?0:1;
                    if(days>0)
                    {
                        sub.EndDate=PlusDays(old,days);sub.Version++;
                        await ShiftSuccessorAsync(sub,days,actor,ct);await EnsureNoOverlapAsync(sub,ct);
                        access.Audit(actor,closure.BranchId,"Subscription.HolidayExtended",nameof(MemberSubscription),sub.Id,
                            new { EndDate=old },new { sub.MemberId,HolidayId=closure.Id,ProtectedDate=day,sub.EndDate });
                    }
                    db.Add(new SubscriptionHolidayExtension { MemberSubscriptionId=sub.Id,BranchClosureId=closure.Id,
                        ProtectedDate=day,DaysApplied=days,PreviousEndDate=old,NewEndDate=sub.EndDate.Value,
                        CreatedByUserId=actor,CreatedAt=access.Now });protectedDays++;extendedDays+=days;
                    // Make the unique protected date visible to subsequent days/overlapping closures in this transaction.
                    await db.SaveChangesAsync(ct);
                }
                if(day==last)break;
            }
        }
        if(protectedDays>0)
        {
            access.Audit(actor,closure.BranchId,"BranchClosure.SubscriptionCompensationApplied",nameof(BranchClosure),closure.Id,
                after:new { ProtectedDays=protectedDays,ExtendedDays=extendedDays });await db.SaveChangesAsync(ct);
        }
        return new(closure.Id,protectedDays,extendedDays);
    }
}
