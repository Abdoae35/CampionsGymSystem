using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
public sealed partial class SalesService
{
    public Task<PaymentDetails> RecordPaymentAsync(Guid saleId, RecordPaymentRequest r, CancellationToken ct=default) => TransactionAsync(async () =>
    {
        Operation(r.OperationId); var sale = await LockSaleAsync(saleId,SalesPermissions.Create,ct);
        var actor = await RequireAsync(SalesPermissions.Create,sale.BranchId,ct); var hash=Hash(new{SaleId=saleId,Request=r});
        var retry=await db.Payments.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(retry!=null)
        {
            if(retry.SaleTransactionId!=saleId || retry.RequestHash!=hash || retry.ReceivedByUserId!=actor.UserId)
                throw new BusinessRuleException("Operation identity belongs to another payment request.");
            return await PaymentDetailsAsync(retry.Id,ct);
        }
        if(sale.CancelledAt!=null)throw new BusinessRuleException("Cancelled Sales cannot receive additional payments.");
        if(sale.PurchaseMode!=PurchaseMode.Installment && r.Allocations is {Count:>0})throw new BusinessRuleException("This Sale has no installment obligations.");
        Amount(r.Amount);
        var collected=await db.Payments.Where(x=>x.SaleTransactionId==sale.Id && x.Status==PaymentStatus.Completed).SumAsync(x=>x.Amount,ct);
        if(r.Amount>sale.NetAmount-collected)throw new BusinessRuleException("Payment exceeds the outstanding Sale amount.");
        var payment=await PaymentAsync(sale,r.OperationId,hash,r.Amount,r.Parts,actor.UserId,ct);
        if(sale.PurchaseMode==PurchaseMode.Installment)await AllocateInstallmentsAsync(sale,payment,r.Allocations,actor.UserId,ct);
        sale.Status=collected+r.Amount==sale.NetAmount?SaleStatus.Completed:SaleStatus.Open;sale.Version++;
        if(sale.Status==SaleStatus.Completed)access.Audit(actor.UserId,sale.BranchId,"Sale.Completed",nameof(SaleTransaction),sale.Id,
            after:new{sale.MemberId,Collected=collected+r.Amount,sale.NetAmount});
        await db.SaveChangesAsync(ct);
        if(sale.SaleType==SaleType.GymMembership)await subscriptions.ActivatePaidSaleAsync(sale.Id,actor.UserId,false,null,ct);
        return await PaymentDetailsAsync(payment.Id,ct);
    },ct);
    private async Task<PaymentDetails> PaymentDetailsAsync(Guid id,CancellationToken ct)
    {
        var p=await db.Payments.AsNoTracking().SingleAsync(x=>x.Id==id,ct);
        return new(p.Id,p.OperationId,p.BranchId,p.BusinessDate,p.BusinessDayId,p.PaidAt,p.Amount,
            await db.PaymentParts.AsNoTracking().Where(x=>x.PaymentId==id).OrderBy(x=>x.Id).Select(x=>new FinancialPartDetails(x.FinancialAccountId,x.FinancialAccount.Name,x.FinancialAccount.AccountType.ToString(),x.Amount,x.ProviderReference)).ToListAsync(ct),
            await db.InstallmentPaymentAllocations.AsNoTracking().Where(x=>x.PaymentId==id).OrderBy(x=>x.InstallmentId).Select(x=>new InstallmentAllocationInput(x.InstallmentId,x.Amount)).ToListAsync(ct));
    }
}
