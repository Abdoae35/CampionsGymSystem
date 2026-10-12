using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
public sealed partial class SalesService
{
    private static void Page(int page,int size){if(page is <1 or >1000000 || size is <1 or >100)throw new BusinessRuleException("Invalid pagination: page >= 1 and page size 1..100.");}
    private async Task<Guid?> ReadScopeAsync(Guid? selected,CancellationToken ct)
    {
        var actor=await current.GetAsync(ct)??throw new UnauthorizedAccessException();
        var branch=selected??(actor.Permissions.Contains("Branches.AccessAll")?null:actor.CurrentBranchId);
        await RequireAsync(SalesPermissions.View,branch,ct);return branch;
    }
    private async Task<SaleTransaction> ReadSaleAsync(Guid id,CancellationToken ct)
    {
        var sale=await db.SaleTransactions.AsNoTracking().SingleOrDefaultAsync(s=>s.Id==id,ct)??throw new BranchNotFoundException("Sale not found.");
        await RequireAsync(SalesPermissions.View,sale.BranchId,ct);return sale;
    }
    private sealed class MoneyFacts
    {
        public SaleTransaction Sale {get;init;}=null!;
        public decimal Collected {get;init;}
        public decimal Refunded {get;init;}
    }
    private IQueryable<MoneyFacts> Facts(IQueryable<SaleTransaction> sales)=>sales.Select(s=>new MoneyFacts{Sale=s,
        Collected=db.Payments.Where(p=>p.SaleTransactionId==s.Id && p.Status==PaymentStatus.Completed).Sum(p=>p.Amount),
        Refunded=db.Refunds.Where(r=>r.SaleTransactionId==s.Id).Sum(r=>r.Amount)});
    private static SaleSummary Summary(MoneyFacts f)
    {
        var s=f.Sale;
        var status=s.OperationId==null?s.Status.ToString():s.CancelledAt!=null?f.Refunded==f.Collected && f.Collected>0?"Refunded":f.Refunded>0?"PartiallyRefunded":"Cancelled"
            :f.Collected>=s.NetAmount?"Paid":s.PurchaseMode==PurchaseMode.Reservation?"Reserved":s.PurchaseMode==PurchaseMode.Installment
                && f.Collected>0 && f.Collected>=s.InitialPaymentRequired?"InstallmentActive":f.Collected>0?"PartiallyPaid":"PendingPayment";
        return new(s.Id,s.MemberId,s.BranchId,s.BusinessDate,s.CreatedAt,s.GrossAmount,s.DiscountAmount,s.NetAmount,f.Collected,f.Refunded,
            s.CancelledAt!=null?0:Math.Max(0,s.NetAmount-f.Collected),status,s.PurchaseMode?.ToString(),s.ActivationMode?.ToString(),s.CancelledAt!=null,s.Version);
    }
    public async Task<SaleHistory> GetHistoryAsync(SaleHistoryRequest r,CancellationToken ct=default)
    {
        Page(r.Page,r.PageSize);if(r.From>r.To || r.PurchaseMode.HasValue && !Enum.IsDefined(r.PurchaseMode.Value))throw new BusinessRuleException("Invalid Sale history filter.");
        var branch=await ReadScopeAsync(r.BranchId,ct);var q=db.SaleTransactions.AsNoTracking();
        if(branch!=null)q=q.Where(s=>s.BranchId==branch);if(r.MemberId!=null)q=q.Where(s=>s.MemberId==r.MemberId);
        if(r.PurchaseMode!=null)q=q.Where(s=>s.PurchaseMode==r.PurchaseMode);if(r.From!=null)q=q.Where(s=>s.BusinessDate>=r.From);if(r.To!=null)q=q.Where(s=>s.BusinessDate<=r.To);
        var count=await q.CountAsync(ct);var rows=await Facts(q.OrderByDescending(s=>s.CreatedAt).ThenBy(s=>s.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)).ToListAsync(ct);
        return new(rows.Select(Summary).ToArray(),count,r.Page,r.PageSize);
    }
    public async Task<IReadOnlyList<InstallmentDetails>> GetInstallmentsAsync(Guid saleId,CancellationToken ct=default)
    {
        var sale=await ReadSaleAsync(saleId,ct);var date=(await BusinessDateAsync(sale.BranchId,ct)).Date;
        var rows=await db.Installments.AsNoTracking().Where(i=>i.InstallmentAgreement.SaleTransactionId==saleId).OrderBy(i=>i.InstallmentNumber).Take(100)
            .Select(i=>new{i.Id,i.InstallmentNumber,i.DueDate,i.AmountDue,Paid=db.InstallmentPaymentAllocations.Where(a=>a.InstallmentId==i.Id && a.Payment.Status==PaymentStatus.Completed).Sum(a=>a.Amount)}).ToListAsync(ct);
        return rows.Select(i=>new InstallmentDetails(i.Id,i.InstallmentNumber,i.DueDate,i.AmountDue,i.Paid,
            sale.CancelledAt!=null?"Cancelled":i.Paid>=i.AmountDue?"Paid":i.DueDate<date?"Overdue":i.Paid>0?"PartiallyPaid":"Pending")).ToArray();
    }
    public async Task<PaymentHistory> GetPaymentsAsync(Guid saleId,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        Page(page,pageSize);await ReadSaleAsync(saleId,ct);var q=db.Payments.AsNoTracking().Where(p=>p.SaleTransactionId==saleId);var count=await q.CountAsync(ct);
        var ids=await q.OrderByDescending(p=>p.PaidAt).ThenBy(p=>p.Id).Skip((page-1)*pageSize).Take(pageSize).Select(p=>p.Id).ToListAsync(ct);
        var rows=new List<PaymentDetails>();foreach(var id in ids)rows.Add(await PaymentDetailsAsync(id,ct));return new(rows,count,page,pageSize);
    }
    public async Task<RefundHistory> GetRefundsAsync(Guid saleId,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        Page(page,pageSize);await ReadSaleAsync(saleId,ct);var q=db.Refunds.AsNoTracking().Where(p=>p.SaleTransactionId==saleId);var count=await q.CountAsync(ct);
        var ids=await q.OrderByDescending(p=>p.CreatedAt).ThenBy(p=>p.Id).Skip((page-1)*pageSize).Take(pageSize).Select(p=>p.Id).ToListAsync(ct);
        var rows=new List<RefundDetails>();foreach(var id in ids)rows.Add(await RefundDetailsAsync(id,ct));return new(rows,count,page,pageSize);
    }
    public async Task<SaleDetails> GetDetailsAsync(Guid id,CancellationToken ct=default)
    {
        var sale=await ReadSaleAsync(id,ct);var facts=await Facts(db.SaleTransactions.AsNoTracking().Where(s=>s.Id==id)).SingleAsync(ct);
        var snapshot=await db.SaleMembershipSnapshots.AsNoTracking().SingleOrDefaultAsync(s=>s.SaleTransactionId==id,ct);
        var grant=await db.MembershipSubscriptionTransactions.AsNoTracking().SingleOrDefaultAsync(g=>g.SaleTransactionId==id && g.OperationId!=null,ct);
        var status=sale.CancelledAt!=null?"Cancelled":grant!=null?await subscriptions.PaidGrantStatusAsync(grant.Id,ct)
            :facts.Collected>0 && (sale.PurchaseMode==PurchaseMode.Installment?facts.Collected>=sale.InitialPaymentRequired:facts.Collected>=sale.NetAmount)
                && sale.ActivationMode==SubscriptionActivationMode.FirstCheckIn?"PendingFirstCheckIn":"PendingPayment";
        var retainedEligible=false;
        if(sale.CancelledAt!=null && sale.CommissionEligible==true && sale.TargetEligible==true)
        {
            var payments=await db.Payments.AsNoTracking().Where(p=>p.SaleTransactionId==id && p.Status==PaymentStatus.Completed).ToListAsync(ct);
            var refunded=await new Commissions.CommissionSources(db,access).RefundedAsync(payments,ct);
            var evidence=await db.CommissionCollections.Where(c=>payments.Select(p=>p.Id).Contains(c.PaymentId)).ToDictionaryAsync(c=>c.PaymentId,ct);
            foreach(var p in payments.Where(p=>p.Amount>refunded.GetValueOrDefault(p.Id)))
            {
                var reason=evidence.TryGetValue(p.Id,out var fact)?fact.EligibilityReason:(await new Commissions.CommissionSources(db,access).AttributionAsync(sale,p.BusinessDate,p.PaidAt,ct)).Reason;
                if(reason=="Eligible"){retainedEligible=true;break;}
            }
        }
        return new(Summary(facts),await db.Members.Where(m=>m.Id==sale.MemberId).Select(m=>m.FullName).SingleAsync(ct),snapshot?.PlanName,snapshot?.PlanCategory.ToString(),
            snapshot?.MembershipPlanId,snapshot?.CityId,sale.SalesEmployeeId,sale.SalesAttributionSnapshot?.ToString(),sale.CommissionEligible,sale.TargetEligible,
            grant?.MemberSubscriptionId,grant?.Id,status,sale.RequestedStartDate,sale.ReservationDueDate,sale.OfferNameSnapshot,sale.ExtraDaysSnapshot,
            sale.CancelledAt!=null?Math.Max(0,facts.Collected-facts.Refunded):0,(await GetPaymentsAsync(id,1,100,ct)).Items,(await GetRefundsAsync(id,1,100,ct)).Items,
            await GetInstallmentsAsync(id,ct),sale.OfferId,sale.OfferTypeSnapshot?.ToString(),sale.OfferValueSnapshot,sale.OfferEligibilityModeSnapshot?.ToString(),sale.OfferAudienceCategoryIdSnapshot,
            RetainedCommissionEligible:retainedEligible,RetainedTargetEligible:retainedEligible);
    }
    public async Task<BranchFinancialTotals> GetBranchTotalsAsync(Guid? branchId=null,DateOnly? from=null,DateOnly? to=null,CancellationToken ct=default)
    {
        if(from>to)throw new BusinessRuleException("Invalid financial date range.");var actor=await current.GetAsync(ct)??throw new UnauthorizedAccessException();
        var branch=branchId??actor.CurrentBranchId??throw new BusinessRuleException("Select a branch for financial totals.");await RequireAsync(SalesPermissions.View,branch,ct);
        var p=db.PaymentParts.Where(p=>p.Payment.BranchId==branch && p.Payment.Status==PaymentStatus.Completed && (from==null || p.Payment.BusinessDate>=from) && (to==null || p.Payment.BusinessDate<=to));
        var r=db.RefundParts.Where(r=>r.Refund.BranchId==branch && (from==null || r.Refund.BusinessDate>=from) && (to==null || r.Refund.BusinessDate<=to));
        var pc=await p.GroupBy(p=>p.FinancialAccount.AccountType).Select(g=>new{Method=g.Key,Amount=g.Sum(p=>p.Amount)}).ToListAsync(ct);
        var rc=await r.GroupBy(p=>p.FinancialAccount.AccountType).Select(g=>new{Method=g.Key,Amount=g.Sum(p=>p.Amount)}).ToListAsync(ct);
        var gross=pc.Sum(p=>p.Amount);var refunds=rc.Sum(p=>p.Amount);
        return new(branch,pc.Where(p=>p.Method==FinancialAccountType.Cash).Sum(p=>p.Amount),pc.Where(p=>p.Method==FinancialAccountType.InstaPay).Sum(p=>p.Amount),
            rc.Where(p=>p.Method==FinancialAccountType.Cash).Sum(p=>p.Amount),rc.Where(p=>p.Method==FinancialAccountType.InstaPay).Sum(p=>p.Amount),gross,refunds,gross-refunds);
    }
}
