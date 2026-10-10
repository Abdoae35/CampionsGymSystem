using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    internal async Task CancelSaleGrantAsync(SaleTransaction sale,Refund refund,Guid actor,CancellationToken ct)
    {
        if(db.Database.CurrentTransaction is null)throw new InvalidOperationException("Grant reversal requires the Finance transaction.");
        var grant=await db.MembershipSubscriptionTransactions.SingleOrDefaultAsync(g=>g.SaleTransactionId==sale.Id && g.OperationId!=null,ct);
        if(grant==null || await db.SubscriptionGrantCancellations.AnyAsync(c=>c.GrantId==grant.Id,ct))return;
        var sub=await LockedSubscriptionAsync(grant.MemberSubscriptionId,ct);
        var other=await db.MembershipSubscriptionTransactions.AnyAsync(g=>g.MemberSubscriptionId==sub.Id && g.Id!=grant.Id
            && !db.SubscriptionGrantCancellations.Any(c=>c.GrantId==g.Id),ct);
        var oldEnd=sub.EndDate;var oldSessions=sub.RemainingSessions;var daysRemoved=0;
        if(!other)sub.Status=SubscriptionStatus.Cancelled;
        else
        {
            if(sub.EndDate==null || grant.PurchasedDurationDays<=0)throw new BusinessRuleException("Grant reversal requires reconciled customer duration history.");
            var date=await EffectiveBusinessDateAsync(sale.BranchId,ct);
            var priorRemoved=await db.SubscriptionGrantCancellations.Where(c=>c.Grant.MemberSubscriptionId==sub.Id
                && c.Grant.StartDateSnapshot<grant.StartDateSnapshot).SumAsync(c=>c.DaysRemoved,ct);
            var grantStart=PlusDays(grant.StartDateSnapshot??throw new BusinessRuleException("Grant date history is missing."),-priorRemoved);
            daysRemoved=Math.Clamp(grant.PurchasedDurationDays-Math.Max(0,date.DayNumber-grantStart.DayNumber),0,grant.PurchasedDurationDays);
            sub.EndDate=PlusDays(sub.EndDate.Value,-daysRemoved);
            if(sub.EndDate<sub.StartDate)throw new BusinessRuleException("Grant reversal conflicts with existing entitlement history.");
            if(sub.AccessMode==AccessMode.SessionBased)
            {
                // Remaining pooled sessions are assigned newest grant first, equivalent to consuming older grants first.
                var live=await db.MembershipSubscriptionTransactions.Where(g=>g.MemberSubscriptionId==sub.Id
                    && !db.SubscriptionGrantCancellations.Any(c=>c.GrantId==g.Id)).OrderByDescending(g=>g.StartDateSnapshot).ThenByDescending(g=>g.CreatedAt).ThenByDescending(g=>g.Id).ToListAsync(ct);
                var remaining=sub.RemainingSessions!.Value;var remove=0;
                foreach(var g in live)
                {
                    var settled=await db.BorrowedGymSessions.CountAsync(d=>d.SettledByTransactionId==g.Id
                        || db.BorrowedDebtSettlementEvents.Any(e=>e.BorrowedGymSessionId==d.Id && e.GrantId==g.Id && !e.IsReversal),ct);
                    var available=Math.Min(remaining,(g.PurchasedSessions??throw new BusinessRuleException("Session grant history is missing."))-settled);
                    if(g.Id==grant.Id){remove=available;break;}remaining-=available;
                }
                sub.TotalSessions-=grant.PurchasedSessions;sub.RemainingSessions-=remove;
            }
            // A future successor follows the effective remaining entitlement; never rewrite one which already started.
            var next=await db.MemberSubscriptions.SingleOrDefaultAsync(s=>s.FollowsSubscriptionId==sub.Id && s.Status!=SubscriptionStatus.Cancelled,ct);
            if(next!=null)
            {
                var nextDate=await EffectiveBusinessDateAsync(next.PurchasedAtBranchId,ct);
                if(next.StartDate>nextDate)
                {
                    var start=PlusDays(sub.EndDate.Value,1);if(start<nextDate)start=nextDate;
                    var shift=start.DayNumber-next.StartDate.DayNumber;next.StartDate=start;next.EndDate=PlusDays(next.EndDate!.Value,shift);next.Version++;
                    access.Audit(actor,next.PurchasedAtBranchId,"Subscription.ScheduleShifted",nameof(MemberSubscription),next.Id,after:new{next.StartDate,next.EndDate,CancelledGrantId=grant.Id});
                }
            }
        }
        sub.Version++;
        db.Add(new SubscriptionGrantCancellation{GrantId=grant.Id,Refund=refund,PreviousEndDate=oldEnd,NewEndDate=sub.EndDate,
            PreviousRemainingSessions=oldSessions,NewRemainingSessions=sub.RemainingSessions,DaysRemoved=daysRemoved,CreatedAt=access.Now});
        var debts=await db.BorrowedGymSessions.Where(d=>d.MemberId==sale.MemberId).ToListAsync(ct);
        foreach(var debt in debts)
        {
            var last=await db.BorrowedDebtSettlementEvents.Where(e=>e.BorrowedGymSessionId==debt.Id).OrderByDescending(e=>e.Sequence).FirstOrDefaultAsync(ct);
            if(last!=null ? last.IsReversal || last.GrantId!=grant.Id : debt.Status!=BorrowedSessionStatus.Settled || debt.SettledByTransactionId!=grant.Id)continue;
            db.Add(new BorrowedDebtSettlementEvent{BorrowedGymSessionId=debt.Id,GrantId=grant.Id,IsReversal=true,Refund=refund,
                Sequence=(last?.Sequence??0)+1,ActorId=actor,CreatedAt=access.Now});
            access.Audit(actor,sale.BranchId,"BorrowedSessionDebt.SettlementReversed",nameof(BorrowedGymSession),debt.Id,
                after:new{sale.Id,GrantId=grant.Id,RefundId=refund.Id,debt.ExpiredSubscriptionId,Status="Outstanding"});
        }
        access.Audit(actor,sale.BranchId,"Subscription.GrantCancelled",nameof(MembershipSubscriptionTransaction),grant.Id,
            new{EndDate=oldEnd,RemainingSessions=oldSessions},new{SaleId=sale.Id,RefundId=refund.Id,SubscriptionId=sub.Id,sub.EndDate,sub.RemainingSessions,sub.Status});
    }
}
