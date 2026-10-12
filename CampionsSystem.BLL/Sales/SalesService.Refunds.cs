using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
public sealed partial class SalesService
{
    public Task<RefundDetails> RefundAsync(Guid saleId,RefundRequest r,CancellationToken ct=default)=>TransactionAsync(async()=>
    {
        Operation(r.OperationId);var sale=await LockSaleAsync(saleId,SalesPermissions.Refund,ct);var actor=await RequireAsync(SalesPermissions.Refund,sale.BranchId,ct);
        var hash=Hash(new{SaleId=saleId,Request=r,Type=RefundType.ManagerRefund});
        var retry=await RefundRetryAsync(sale,r.OperationId,hash,actor.UserId,ct);if(retry!=null)return retry;
        return await RefundCoreAsync(sale,r.OperationId,hash,r.Amount,r.Parts,r.Reason,RefundType.ManagerRefund,actor.UserId,ct);
    },ct);
    private async Task<RefundDetails?> RefundRetryAsync(SaleTransaction sale,Guid op,string hash,Guid actor,CancellationToken ct)
    {
        var retry=await db.Refunds.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==op,ct);if(retry==null)return null;
        if(retry.SaleTransactionId!=sale.Id || retry.RequestHash!=hash || retry.ProcessedByUserId!=actor)throw new BusinessRuleException("Operation identity belongs to another refund request.");
        return await RefundDetailsAsync(retry.Id,ct);
    }
    private async Task<RefundDetails> RefundCoreAsync(SaleTransaction sale,Guid op,string hash,decimal amount,IReadOnlyList<PaymentPartInput> parts,
        string reason,RefundType type,Guid actor,CancellationToken ct)
    {
        var activeShift=await shiftMoney.RequireShiftAsync(sale.BranchId,actor,ct);
        Amount(amount,false);if(string.IsNullOrWhiteSpace(reason)||reason.Length>2000)throw new BusinessRuleException("A cancellation/refund reason is required (maximum 2000 characters).");
        var collected=await db.Payments.Where(p=>p.SaleTransactionId==sale.Id && p.Status==PaymentStatus.Completed).SumAsync(p=>p.Amount,ct);
        var refunded=await db.Refunds.Where(r=>r.SaleTransactionId==sale.Id).SumAsync(r=>r.Amount,ct);
        if(amount>collected-refunded)throw new BusinessRuleException("Refund exceeds the collected, unrefunded amount.");
        if(amount>0)await AccountsAsync(sale.BranchId,parts,amount,ct,type==RefundType.QuickRefund);
        else if(parts==null || parts.Count!=0)throw new BusinessRuleException("A zero refund/cancellation must have no financial parts.");
        var (date,day)=await BusinessDateAsync(sale.BranchId,ct);
        var refund=new Refund{ShiftId=activeShift.Id,SaleTransactionId=sale.Id,BranchId=sale.BranchId,OperationId=op,RequestHash=hash,Amount=amount,RefundType=type,
            Reason=reason.Trim(),ProcessedByUserId=actor,ApprovedByUserId=actor,CreatedAt=access.Now,BusinessDate=date,BusinessDayId=day};
        await new Commissions.CommissionSources(db,access).AllocateRefundAsync(refund,ct);
        db.Add(refund);
        foreach(var part in parts)db.Add(new RefundPart{Refund=refund,FinancialAccountId=part.FinancialAccountId,Amount=part.Amount,ProviderReference=part.ProviderReference});
        foreach(var group in parts.GroupBy(p=>p.FinancialAccountId))await shiftMoney.PostAsync(new FinancialTransaction{ShiftId=activeShift.Id,Refund=refund,FinancialAccountId=group.Key,BranchId=sale.BranchId,
            TransactionType=FinancialTransactionType.Debit,SourceType=FinancialSourceType.Refund,Amount=group.Sum(p=>p.Amount),BusinessDate=date,BusinessDayId=day,OccurredAt=access.Now,CreatedByUserId=actor},ct,type==RefundType.QuickRefund);
        if(sale.CancelledAt==null)
        {
            await subscriptions.CancelSaleGrantAsync(sale,refund,actor,ct);
            sale.CancelledAt=access.Now;sale.CancelledByUserId=actor;sale.CancellationReason=reason.Trim();
            access.Audit(actor,sale.BranchId,sale.PurchaseMode==PurchaseMode.Reservation?"Reservation.Cancelled":"Sale.Cancelled",nameof(SaleTransaction),sale.Id,after:new{sale.MemberId,RefundId=refund.Id});
        }
        var retained=collected-refunded-amount;
        sale.Status=refunded+amount==collected && collected>0?SaleStatus.Refunded:refunded+amount>0?SaleStatus.PartiallyRefunded:SaleStatus.Cancelled;sale.Version++;
        access.Audit(actor,sale.BranchId,"Refund.Created",nameof(Refund),refund.Id,after:new{sale.Id,amount,date,type});
        if(retained>0)access.Audit(actor,sale.BranchId,"Reservation.AmountRetained",nameof(SaleTransaction),sale.Id,
            after:new{Amount=retained,Classification="CancellationFee",OriginalSaleCommissionEligible=sale.CommissionEligible,
                OriginalSaleTargetEligible=sale.TargetEligible,CommissionAttribution="Original retained collections"});
        await db.SaveChangesAsync(ct);return await RefundDetailsAsync(refund.Id,ct);
    }
    private async Task<RefundDetails> RefundDetailsAsync(Guid id,CancellationToken ct)
    {
        var r=await db.Refunds.AsNoTracking().SingleAsync(x=>x.Id==id,ct);
        return new(r.Id,r.OperationId,r.BranchId,r.BusinessDate,r.CreatedAt,r.Amount,r.RefundType.ToString(),r.Reason,
            await db.RefundParts.AsNoTracking().Where(p=>p.RefundId==id).OrderBy(p=>p.Id).Select(p=>new FinancialPartDetails(p.FinancialAccountId,p.FinancialAccount.Name,p.FinancialAccount.AccountType.ToString(),p.Amount,p.ProviderReference)).ToListAsync(ct));
    }
}
