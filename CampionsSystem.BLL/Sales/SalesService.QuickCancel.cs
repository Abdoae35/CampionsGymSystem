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
        await RequireRefundRoleAsync(actor.UserId,quick:true,ct);
        var hash=Hash(new{SaleId=saleId,Request=r,Type=RefundType.QuickRefund});var retry=await RefundRetryAsync(sale,r.OperationId,hash,actor.UserId,ct);if(retry!=null)return retry;
        if(sale.CancelledAt!=null || sale.CreatedAt>access.Now || access.Now-sale.CreatedAt>TimeSpan.FromHours(24))
            throw new BusinessRuleException("Quick Cancel requires an uncancelled Sale created within the last 24 actual hours.");
        if(r.Amount is null)throw new BusinessRuleException("Choose an explicit refund amount, including zero for cancellation without payment.");
        var refund=await RefundCoreAsync(sale,r.OperationId,hash,r.Amount.Value,r.Parts??[],r.Reason,RefundType.QuickRefund,actor.UserId,ct);
        access.Audit(actor.UserId,sale.BranchId,"Sale.QuickCancelled",nameof(SaleTransaction),sale.Id,
            after:new{RefundId=refund.Id,refund.Amount,Retained=await RetainedAsync(sale.Id,ct)});
        await db.SaveChangesAsync(ct);return refund;
    },ct);
}
