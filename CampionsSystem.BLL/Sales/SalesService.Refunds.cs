using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
public sealed partial class SalesService
{
    public Task<RefundDetails> RefundAsync(Guid saleId,RefundRequest r,CancellationToken ct=default)=>TransactionAsync(async()=>
    {
        Operation(r.OperationId);var sale=await LockSaleAsync(saleId,SalesPermissions.Refund,ct);var actor=await RequireAsync(SalesPermissions.Refund,sale.BranchId,ct);
        await RequireRefundRoleAsync(actor.UserId,quick:false,ct);
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
    private async Task RequireRefundRoleAsync(Guid actor,bool quick,CancellationToken ct)
    {
        var role=quick?"RECEPTION":"BRANCHMANAGER";
        if(!quick && await new OwnerAdministrationAccess(db,current).IsOwnerAsync(actor,ct))return;
        if(!await (from membership in db.UserRoles.AsNoTracking()
                   join identityRole in db.Roles.AsNoTracking() on membership.RoleId equals identityRole.Id
                   where membership.UserId==actor && identityRole.NormalizedName==role
                   select membership.UserId).AnyAsync(ct))throw new UnauthorizedAccessException();
    }
    private async Task<decimal> RetainedAsync(Guid saleId,CancellationToken ct)
        =>await db.Payments.Where(p=>p.SaleTransactionId==saleId && p.Status==PaymentStatus.Completed).SumAsync(p=>p.Amount,ct)
          -await db.Refunds.Where(r=>r.SaleTransactionId==saleId).SumAsync(r=>r.Amount,ct);
    private async Task<bool> HistoricalQuickRefundAsync(SaleTransaction sale,IReadOnlyList<PaymentPartInput> parts,CancellationToken ct)
    {
        var ids=parts.Where(p=>p is not null).Select(p=>p.FinancialAccountId).Distinct().ToArray();
        var inactive=await db.FinancialAccounts.AsNoTracking().Where(a=>ids.Contains(a.Id) && !a.IsActive).Select(a=>a.Id).ToListAsync(ct);
        if(inactive.Count==0)return false;
        var original=await db.PaymentParts.AsNoTracking().Where(p=>p.Payment.SaleTransactionId==sale.Id && p.Payment.Status==PaymentStatus.Completed)
            .Select(p=>p.FinancialAccountId).Distinct().ToListAsync(ct);
        if(inactive.Any(id=>!original.Contains(id)))throw new BusinessRuleException("An inactive refund account must be an original payment account of this Sale.");
        return true;
    }
    private async Task<RefundDetails> RefundCoreAsync(SaleTransaction sale,Guid op,string hash,decimal amount,IReadOnlyList<PaymentPartInput> parts,
        string reason,RefundType type,Guid actor,CancellationToken ct)
    {
        var owner=type==RefundType.ManagerRefund && await new OwnerAdministrationAccess(db,current).IsOwnerAsync(actor,ct);
        var activeShift=owner?null:await shiftMoney.RequireShiftAsync(sale.BranchId,actor,ct);
        Amount(amount,false);if(string.IsNullOrWhiteSpace(reason)||reason.Length>2000)throw new BusinessRuleException("A cancellation/refund reason is required (maximum 2000 characters).");
        if(parts is null)throw new BusinessRuleException("Refund parts are required (empty for a zero refund).");
        var collected=await db.Payments.Where(p=>p.SaleTransactionId==sale.Id && p.Status==PaymentStatus.Completed).SumAsync(p=>p.Amount,ct);
        var refunded=await db.Refunds.Where(r=>r.SaleTransactionId==sale.Id).SumAsync(r=>r.Amount,ct);
        if(amount>collected-refunded)throw new BusinessRuleException("Refund exceeds the collected, unrefunded amount.");
        await GuardCoachingRefundAsync(sale,collected-refunded-amount,ct);
        var historical=amount>0 && type==RefundType.QuickRefund && await HistoricalQuickRefundAsync(sale,parts,ct);
        if(amount>0)await AccountsAsync(sale.BranchId,parts,amount,ct,historical);
        else if(parts.Count!=0)throw new BusinessRuleException("A zero refund/cancellation must have no financial parts.");
        var (date,day)=await BusinessDateAsync(sale.BranchId,ct);
        var refund=new Refund{ShiftId=activeShift?.Id,SaleTransactionId=sale.Id,BranchId=sale.BranchId,OperationId=op,RequestHash=hash,Amount=amount,RefundType=type,
            Reason=reason.Trim(),ProcessedByUserId=actor,ApprovedByUserId=type==RefundType.QuickRefund?null:actor,CreatedAt=access.Now,BusinessDate=date,BusinessDayId=day};
        await new Commissions.CommissionSources(db,access).AllocateRefundAsync(refund,ct);
        db.Add(refund);
        foreach(var part in parts)db.Add(new RefundPart{Refund=refund,FinancialAccountId=part.FinancialAccountId,Amount=part.Amount,ProviderReference=part.ProviderReference});
        foreach(var group in parts.GroupBy(p=>p.FinancialAccountId))await shiftMoney.PostAsync(new FinancialTransaction{ShiftId=activeShift?.Id,Refund=refund,FinancialAccountId=group.Key,BranchId=sale.BranchId,
            TransactionType=FinancialTransactionType.Debit,SourceType=FinancialSourceType.Refund,Amount=group.Sum(p=>p.Amount),BusinessDate=date,BusinessDayId=day,OccurredAt=access.Now,CreatedByUserId=actor},ct,historical);
        if(sale.CancelledAt==null)
        {
            await subscriptions.CancelSaleGrantAsync(sale,refund,actor,ct);
            await CancelCoachingGrantAsync(sale,actor,ct);
            sale.CancelledAt=access.Now;sale.CancelledByUserId=actor;sale.CancellationReason=reason.Trim();
            access.Audit(actor,sale.BranchId,sale.PurchaseMode==PurchaseMode.Reservation?"Reservation.Cancelled":"Sale.Cancelled",nameof(SaleTransaction),sale.Id,after:new{sale.MemberId,RefundId=refund.Id});
        }
        var retained=collected-refunded-amount;
        sale.Status=refunded+amount==collected && collected>0?SaleStatus.Refunded:refunded+amount>0?SaleStatus.PartiallyRefunded:SaleStatus.Cancelled;sale.Version++;
        access.Audit(actor,sale.BranchId,"Refund.Created",nameof(Refund),refund.Id,after:new{sale.Id,amount,Retained=retained,date,type,ShiftId=activeShift?.Id,ProcessedByUserId=actor,refund.ApprovedByUserId});
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
