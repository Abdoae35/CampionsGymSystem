using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    private async Task<SubscriptionGrantResult?> ActivateStandaloneSaleAsync(SaleTransaction sale,Guid actor,bool firstEntry,
        DateOnly? entranceDate,Guid? entranceDay,DateOnly date,CancellationToken ct)
    {
        var plan=SoldPlan(sale);
        if(plan.AccessMode!=AccessMode.SessionBased || plan.SessionCount!=1)throw new BusinessRuleException("A standalone visit grants exactly one paid Gym entry.");
        if(await Evaluated(date).AnyAsync(x=>x.Subscription.MemberId==sale.MemberId && x.Status!=SubscriptionStatus.Cancelled
            && (x.Subscription.EndDate==null || x.Subscription.EndDate>=date)
            && !(x.Status==SubscriptionStatus.Expired && x.Subscription.AccessMode==AccessMode.SessionBased && x.Subscription.RemainingSessions<=0),ct))
            throw new BusinessRuleException("Standalone visits cannot overlap another current/future gym entitlement.");
        if(sale.ActivationMode==SubscriptionActivationMode.FirstCheckIn && !firstEntry)
        {
            if(!await db.AuditLogs.AnyAsync(x=>x.Action=="Subscription.PendingFirstCheckIn" && x.EntityId==sale.Id,ct))
                access.Audit(actor,sale.BranchId,"Subscription.PendingFirstCheckIn",nameof(SaleTransaction),sale.Id,after:new{sale.MemberId,Standalone=true});
            await db.SaveChangesAsync(ct);return null;
        }
        var member=await db.Members.SingleAsync(x=>x.Id==sale.MemberId,ct);if(!member.IsActive)throw new BusinessRuleException("Member must be active.");
        var start=sale.ActivationMode==SubscriptionActivationMode.SpecificDate && sale.RequestedStartDate>date?sale.RequestedStartDate.Value:date;
        var day=entranceDate.HasValue?entranceDay:(await GrantDateAsync(sale.BranchId,ct)).DayId;
        var sub=new MemberSubscription{MemberId=sale.MemberId,MembershipPlanId=plan.Id,PurchasedAtBranchId=sale.BranchId,CityId=plan.CityId,
            AccessMode=AccessMode.SessionBased,TotalSessions=1,RemainingSessions=1,IsStandaloneVisit=true,StartDate=start,EndDate=PlusDays(start,plan.DurationDays-1),
            Status=start>date?SubscriptionStatus.Scheduled:SubscriptionStatus.Active,CreatedAt=access.Now};db.Add(sub);
        var grant=await SnapshotAsync(sub,plan,sale.OperationId!.Value,actor,day,start,SubscriptionTransactionType.Initial,null,null,null,ct,sale);
        // A paid visitor entry is not a renewal: no borrowed debt settlement and no predecessor/cycle closure.
        access.Audit(actor,sale.BranchId,"Subscription.ActivatedFromSale",nameof(SaleTransaction),sale.Id,after:new{sale.MemberId,SubscriptionId=sub.Id,GrantId=grant.Id,Standalone=true});
        await db.SaveChangesAsync(ct);await ApplyDeclaredHolidaysForMemberAsync(member.Id,actor,ct);return new(sub.Id,grant.Id,start>date);
    }
}
