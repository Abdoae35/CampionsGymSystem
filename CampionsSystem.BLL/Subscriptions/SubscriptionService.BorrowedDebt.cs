using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    internal async Task ValidatePaidDebtCapacityAsync(Guid memberId,MembershipPlan plan,CancellationToken ct)
        =>_ = await OutstandingBorrowedDebtAsync(memberId,plan,ct);
    // Caller already holds the Member lock used by Check-In, so no new debt can race this grant.
    private async Task<List<BorrowedGymSession>> OutstandingBorrowedDebtAsync(Guid memberId,MembershipPlan plan,CancellationToken ct)
    {
        var debts=await db.BorrowedGymSessions.Include(x=>x.ExpiredSubscription)
            .Where(x=>x.MemberId==memberId && (db.BorrowedDebtSettlementEvents.Where(e=>e.BorrowedGymSessionId==x.Id)
                .OrderByDescending(e=>e.Sequence).Select(e=>(bool?)e.IsReversal).FirstOrDefault() ?? x.Status==BorrowedSessionStatus.Outstanding))
            .OrderBy(x=>x.BorrowedAt).ThenBy(x=>x.Id).ToListAsync(ct);
        if(debts.Any(x=>x.ExpiredSubscription.MemberId!=memberId || x.ExpiredSubscription.EmployeeId!=null
            || x.ExpiredSubscription.EndDate==null || x.BorrowedAt>access.Now))
            throw new BusinessRuleException("Outstanding borrowed-session source history requires reconciliation.");
        if(plan.AccessMode==AccessMode.SessionBased && plan.SessionCount<debts.Count)
            throw new BusinessRuleException("The new SessionBased grant must cover all outstanding borrowed-session debt; partial settlement is not allowed.");
        return debts;
    }
    private async Task SettleBorrowedDebtAsync(MemberSubscription target,MembershipSubscriptionTransaction grant,List<BorrowedGymSession> debts,Guid actor,CancellationToken ct)
    {
        if(debts.Count==0)return;
        if(target.AccessMode==AccessMode.SessionBased)target.RemainingSessions-=debts.Count;
        foreach(var debt in debts)
        {
            if(debt.Status==BorrowedSessionStatus.Outstanding)
            {
                debt.Status=BorrowedSessionStatus.Settled;debt.SettledAt=access.Now;
                debt.SettledBySubscriptionId=target.Id;debt.SettledByTransaction=grant;
            }
            db.Add(new BorrowedDebtSettlementEvent{BorrowedGymSessionId=debt.Id,Grant=grant,ActorId=actor,CreatedAt=access.Now,
                Sequence=(await db.BorrowedDebtSettlementEvents.Where(e=>e.BorrowedGymSessionId==debt.Id).MaxAsync(e=>(int?)e.Sequence,ct)??0)+1});
            access.Audit(actor,target.PurchasedAtBranchId,"BorrowedSessionDebt.Settled",nameof(BorrowedGymSession),debt.Id,
                new{Status=BorrowedSessionStatus.Outstanding},new{debt.MemberId,debt.ExpiredSubscriptionId,SubscriptionId=target.Id,
                    GrantId=grant.Id,debt.SettledAt,target.AccessMode,DebtBranchId=debt.BranchId});
        }
    }
}
