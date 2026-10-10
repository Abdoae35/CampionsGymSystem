using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    private static MembershipPlan SoldPlan(SaleTransaction sale)
    {
        var s = sale.MembershipSnapshot ?? throw new BusinessRuleException("Sale entitlement snapshot is missing.");
        if (s.PlanCategory == PlanCategory.Employee || s.DurationDays <= 0 || (long)s.DurationDays + sale.ExtraDaysSnapshot > int.MaxValue)
            throw new BusinessRuleException("Sale contains invalid paid entitlement terms.");
        return new MembershipPlan { Id = s.MembershipPlanId, BranchId = sale.BranchId, CityId = s.CityId, Name = s.PlanName,
            PlanCategory = s.PlanCategory, AccessMode = s.AccessMode, DurationDays = s.DurationDays + sale.ExtraDaysSnapshot,
            SessionCount = s.SessionCount, AllowFreeze = s.FreezeDays > 0, IncludedFreezeDays = s.FreezeDays,
            FreelanceCoachEmployeeId = s.FreelanceCoachEmployeeId, Price = sale.GrossAmount, IsActive = true };
    }
    private IQueryable<SaleTransaction> QualifiedSales() => db.SaleTransactions.Where(s => s.OperationId != null && s.CancelledAt == null
        && db.Payments.Where(p => p.SaleTransactionId == s.Id && p.Status == PaymentStatus.Completed).Sum(p => p.Amount) > 0
        && (s.PurchaseMode == PurchaseMode.Installment
            ? s.InitialPaymentRequired != null && s.InitialPaymentRequired > 0 && db.Payments.Where(p => p.SaleTransactionId == s.Id && p.Status == PaymentStatus.Completed).Sum(p => p.Amount) >= s.InitialPaymentRequired
            : db.Payments.Where(p => p.SaleTransactionId == s.Id && p.Status == PaymentStatus.Completed).Sum(p => p.Amount) >= s.NetAmount));
    private IQueryable<SaleTransaction> PendingSales(Guid member) => QualifiedSales().Where(s => s.MemberId == member
        && s.ActivationMode == SubscriptionActivationMode.FirstCheckIn && !db.MembershipSubscriptionTransactions.Any(g => g.SaleTransactionId == s.Id));
    internal async Task<SubscriptionGrantResult?> ActivatePaidSaleAsync(Guid saleId, Guid actor, bool firstEntry, DateOnly? entranceDate,
        CancellationToken ct, Guid? entranceDay = null)
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("Paid activation requires the financial/entrance transaction.");
        var memberId = await db.SaleTransactions.Where(x => x.Id == saleId).Select(x => x.MemberId).SingleAsync(ct);
        await LockMemberAsync(memberId, ct);
        var sale = await db.SaleTransactions.FromSqlInterpolated($"SELECT * FROM \"SaleTransactions\" WHERE \"Id\"={saleId} FOR UPDATE").SingleAsync(ct);
        await db.Entry(sale).ReloadAsync(ct);
        var prior = await db.MembershipSubscriptionTransactions.AsNoTracking().Include(x => x.MemberSubscription).SingleOrDefaultAsync(x => x.SaleTransactionId == saleId && x.OperationId != null, ct);
        if (prior != null) return new(prior.MemberSubscriptionId, prior.Id, prior.MemberSubscription.StartDate > (entranceDate ?? await EffectiveBusinessDateAsync(sale.BranchId,ct)));
        if (!await QualifiedSales().AnyAsync(x => x.Id == sale.Id, ct)) return null;
        if (await PendingSales(memberId).AnyAsync(x => x.Id != sale.Id,ct))
            throw new BusinessRuleException("Member already has a paid entitlement pending first Check-In.");
        sale.MembershipSnapshot = await db.SaleMembershipSnapshots.Include(x => x.Benefits).SingleOrDefaultAsync(x => x.SaleTransactionId == sale.Id,ct);
        var date = entranceDate ?? await EffectiveBusinessDateAsync(sale.BranchId,ct);
        if (sale.PurchaseMode == PurchaseMode.StandaloneSingleSession)
            return await ActivateStandaloneSaleAsync(sale,actor,firstEntry,entranceDate,entranceDay,date,ct);
        if (await db.MemberSubscriptions.AnyAsync(x=>x.MemberId==memberId && x.IsStandaloneVisit && x.Status!=SubscriptionStatus.Cancelled
            && x.EndDate>=date && x.RemainingSessions>0,ct)) throw new BusinessRuleException("Use or cancel the existing paid visit before activating a membership.");
        var periods = await Evaluated(date).Where(x => x.Subscription.MemberId == memberId && !x.Subscription.IsStandaloneVisit
            && x.Status != SubscriptionStatus.Cancelled && (x.Subscription.EndDate == null || x.Subscription.EndDate >= date)
            && !(x.Status == SubscriptionStatus.Expired && x.Subscription.AccessMode == AccessMode.SessionBased && x.Subscription.RemainingSessions <= 0)).Take(3).ToListAsync(ct);
        if (periods.Count > 1 || periods.Any(x => x.Subscription.EmployeeId != null || x.Subscription.StartDate > date))
            throw new BusinessRuleException("Conflicting current/future entitlement must be resolved before paid activation.");
        var current = periods.SingleOrDefault();
        if (current != null && current.Status is not (SubscriptionStatus.Active or SubscriptionStatus.Frozen or SubscriptionStatus.Expired))
            throw new BusinessRuleException("Current entitlement cannot be renewed in its present state.");
        var ongoing = current?.Status is SubscriptionStatus.Active or SubscriptionStatus.Frozen ? current : null;
        if (ongoing == null && sale.ActivationMode == SubscriptionActivationMode.FirstCheckIn && !firstEntry)
        {
            if (!await db.AuditLogs.AnyAsync(x=>x.Action=="Subscription.PendingFirstCheckIn" && x.EntityId==sale.Id,ct))
                access.Audit(actor,sale.BranchId,"Subscription.PendingFirstCheckIn",nameof(SaleTransaction),sale.Id,after:new{sale.MemberId,date});
            await db.SaveChangesAsync(ct); return null;
        }
        var exhausted = current == null ? await Evaluated(date).Where(x=>x.Subscription.MemberId==memberId && !x.Subscription.IsStandaloneVisit
            && x.Subscription.EmployeeId==null && x.Status==SubscriptionStatus.Expired && x.Subscription.StartDate<=date && x.Subscription.EndDate>=date)
            .OrderByDescending(x=>x.Subscription.StartDate).ThenByDescending(x=>x.Subscription.CreatedAt).FirstOrDefaultAsync(ct) : null;
        var source = current?.Subscription ?? exhausted?.Subscription ?? await db.MemberSubscriptions.AsNoTracking().Where(x => x.MemberId == memberId && x.EmployeeId == null
            && !x.IsStandaloneVisit && x.Status != SubscriptionStatus.Cancelled && x.EndDate < date).OrderByDescending(x => x.EndDate).ThenByDescending(x=>x.CreatedAt).FirstOrDefaultAsync(ct);
        // Early renewals apply immediately. Only new/expired periods honor a chosen activation date.
        var start = ongoing == null && sale.ActivationMode == SubscriptionActivationMode.SpecificDate
            ? sale.RequestedStartDate > date ? sale.RequestedStartDate : date : entranceDate;
        var result = source == null
            ? await CreateCustomerGrantCoreAsync(new(memberId,sale.MembershipSnapshot!.MembershipPlanId,sale.BranchId,sale.OperationId!.Value),actor,sale,start,ct,entranceDate.HasValue ? (entranceDate.Value,entranceDay) : null)
            : await RenewCustomerCoreAsync(new(source.Id,sale.MembershipSnapshot!.MembershipPlanId,sale.BranchId,sale.OperationId!.Value,source.Version),actor,sale,start,ct,entranceDate.HasValue ? (entranceDate.Value,entranceDay) : null);
        access.Audit(actor,sale.BranchId,"Subscription.ActivatedFromSale",nameof(SaleTransaction),sale.Id,
            after:new{sale.MemberId,result.SubscriptionId,result.GrantId,result.Scheduled});
        await db.SaveChangesAsync(ct); return result;
    }
    internal async Task<Guid?> PendingFirstEntrySaleAsync(Guid memberId, Guid cityId, DateOnly date, CancellationToken ct)
    {
        var pending = await PendingSales(memberId).Where(x => x.MembershipSnapshot!.CityId == cityId).Select(x => x.Id).Take(2).ToListAsync(ct);
        if (pending.Count > 1) throw new BusinessRuleException("Ambiguous paid entitlements pending first entry.");
        return pending.Count == 1 ? pending[0] : null;
    }
    internal Task<bool> IsPaidSaleOverdueAsync(Guid saleId,DateOnly date,CancellationToken ct)
        => db.Installments.AnyAsync(i=>i.InstallmentAgreement.SaleTransactionId==saleId && i.InstallmentAgreement.SaleTransaction.CancelledAt==null
            && i.InstallmentAgreement.SaleTransaction.PurchaseMode==PurchaseMode.Installment && i.DueDate<date
            && i.AmountDue>db.InstallmentPaymentAllocations.Where(a=>a.InstallmentId==i.Id && a.Payment.Status==PaymentStatus.Completed).Sum(a=>a.Amount),ct);
    internal Task<bool> IsSubscriptionOverdueAsync(Guid subscriptionId,DateOnly date,CancellationToken ct)
        => db.Installments.AnyAsync(i=>i.InstallmentAgreement.SaleTransaction.OperationId!=null && i.InstallmentAgreement.SaleTransaction.CancelledAt==null
            && i.InstallmentAgreement.SaleTransaction.PurchaseMode==PurchaseMode.Installment && i.DueDate<date
            && i.AmountDue>db.InstallmentPaymentAllocations.Where(a=>a.InstallmentId==i.Id && a.Payment.Status==PaymentStatus.Completed).Sum(a=>a.Amount)
            && db.MembershipSubscriptionTransactions.Any(g=>g.MemberSubscriptionId==subscriptionId && g.SaleTransactionId==i.InstallmentAgreement.SaleTransactionId),ct);

}
