using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
public sealed partial class SalesService
{
    public Task<RefundDetails> QuickCancelAsync(Guid saleId,QuickCancelRequest r,CancellationToken ct=default)=>TransactionAsync(async()=>
    {
        Operation(r.OperationId);var sale=await LockSaleAsync(saleId,SalesPermissions.QuickCancel,ct);var actor=await RequireAsync(SalesPermissions.QuickCancel,sale.BranchId,ct);
        var hash=Hash(new{SaleId=saleId,Request=r,Type=RefundType.QuickRefund});var retry=await RefundRetryAsync(sale,r.OperationId,hash,actor.UserId,ct);if(retry!=null)return retry;
        if(sale.CancelledAt!=null || sale.CreatedAt>access.Now || access.Now-sale.CreatedAt>TimeSpan.FromHours(24))
            throw new BusinessRuleException("Quick Cancel requires an uncancelled Sale created within the last 24 actual hours.");
        if(await db.Refunds.AnyAsync(x=>x.SaleTransactionId==sale.Id,ct))throw new BusinessRuleException("Existing refund history requires the normal cancellation workflow.");
        var grant=await db.MembershipSubscriptionTransactions.AsNoTracking().SingleOrDefaultAsync(g=>g.SaleTransactionId==sale.Id && g.OperationId!=null,ct);
        if(grant!=null && (await db.BorrowedGymSessions.AnyAsync(d=>d.SettledByTransactionId==grant.Id,ct)
            || await db.BorrowedDebtSettlementEvents.AnyAsync(e=>e.GrantId==grant.Id && !e.IsReversal,ct)
            || await db.MemberBenefitUsages.AnyAsync(u=>u.MemberSubscriptionBenefit.MembershipSubscriptionTransactionId==grant.Id,ct)
            || await db.CheckIns.AnyAsync(c=>c.MemberSubscriptionId==grant.MemberSubscriptionId && c.CheckedInAt>=grant.CreatedAt,ct)
            || await db.SubscriptionFreezes.AnyAsync(f=>f.MemberSubscriptionId==grant.MemberSubscriptionId && f.CreatedAt>=grant.CreatedAt,ct)))
            throw new BusinessRuleException("The purchased grant has been used/effective; use authorized normal cancellation instead.");
        var parts=await db.PaymentParts.Where(p=>p.Payment.SaleTransactionId==sale.Id && p.Payment.Status==PaymentStatus.Completed)
            .GroupBy(p=>p.FinancialAccountId).Select(g=>new PaymentPartInput(g.Key,g.Sum(p=>p.Amount),null)).ToListAsync(ct);
        var refund=await RefundCoreAsync(sale,r.OperationId,hash,parts.Sum(p=>p.Amount),parts,r.Reason,RefundType.QuickRefund,actor.UserId,ct);
        access.Audit(actor.UserId,sale.BranchId,"Sale.QuickCancelled",nameof(SaleTransaction),sale.Id,after:new{RefundId=refund.Id,refund.Amount});
        await db.SaveChangesAsync(ct);return refund;
    },ct);
}
