using System.Text.Json;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Commissions;

// Called inside the existing Sale/member/drawer transaction, not through a public grant route.
internal sealed class CommissionSources(ApplicationDbContext db, OperationAccess access)
{
    internal async Task RecordCollectionAsync(Payment payment, SaleTransaction sale, CancellationToken ct)
    {
        var date = payment.BusinessDate!.Value;
        var period = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.BranchId == sale.BranchId
            && p.StartBusinessDate <= date && p.EndBusinessDate >= date, ct);
        if (period?.Status == CommissionPeriodStatus.Closed) throw new BusinessRuleException("Collection BusinessDate belongs to a finalized commission period.");
        payment.CommissionPeriodId = period?.Id;
        var (employee, reason) = await AttributionAsync(sale, date, payment.PaidAt, ct);
        db.Add(new CommissionCollection { Payment = payment, EmployeeId = employee, Category = sale.SaleCategory,
            EligibilityReason = reason, CreatedAt = access.Now });
    }
    internal async Task<(Guid? Employee, string Reason)> AttributionAsync(SaleTransaction sale, DateOnly? date, DateTimeOffset paidAt, CancellationToken ct)
    {
        if (sale.PurchaseMode == PurchaseMode.StandaloneSingleSession) return (null, "StandaloneVisit");
        if (sale.MembershipSnapshot?.PlanCategory == PlanCategory.Freelance) return (null, "Freelance");
        if (sale.SalesAttributionSnapshot == AssignmentType.House || sale.SalesEmployeeId == null) return (null, "House");
        if (sale.CommissionEligible != true || sale.TargetEligible != true || sale.SaleCategory == null || date == null) return (null, "LegacyOrExcluded");
        var e = await db.Employees.AsNoTracking().SingleAsync(x => x.Id == sale.SalesEmployeeId, ct);
        // Historical source audit preserves ended contracts after rehire clears the current termination field.
        var ended = await db.AuditLogs.AsNoTracking().Where(a => a.EntityId == e.Id && a.Action == "EmployeeStatusChanged"
            && a.CreatedAt >= sale.CreatedAt && a.CreatedAt <= paidAt).Select(a => a.NewValues).ToListAsync(ct);
        foreach (var json in ended.Where(x => x != null))
        {
            using var doc = JsonDocument.Parse(json!);
            if (doc.RootElement.TryGetProperty("TerminationDate", out var term) && term.ValueKind == JsonValueKind.String
                && DateOnly.TryParse(term.GetString(), out var end) && date > end) return (null, "EmploymentEnded");
        }
        if (e.HireDate > date || e.TerminationDate < date || e.EmploymentStatus != EmploymentStatus.Active
            && !(e.EmploymentStatus == EmploymentStatus.Resigned && e.TerminationDate >= date)) return (null, "EmploymentInactive");
        return (e.Id, "Eligible");
    }
    internal static Dictionary<Guid, decimal> AllocateNewest(IReadOnlyList<Payment> payments, decimal amount,
        Dictionary<Guid, decimal> already, DateTimeOffset refundAt)
    {
        var result = new Dictionary<Guid, decimal>();
        foreach (var p in payments.Where(p => p.PaidAt <= refundAt).OrderByDescending(p => p.PaidAt).ThenByDescending(p => p.Id))
        {
            var take = Math.Min(amount, p.Amount - already.GetValueOrDefault(p.Id));
            if (take <= 0) continue;
            result[p.Id] = take; already[p.Id] = already.GetValueOrDefault(p.Id) + take; amount -= take;
            if (amount == 0) break;
        }
        if (amount != 0) throw new BusinessRuleException("Refund cannot be reconciled to actual original collections.");
        return result;
    }
    internal async Task<Dictionary<Guid, decimal>> RefundedAsync(IReadOnlyList<Payment> payments, CancellationToken ct)
    {
        var sales = payments.Select(p => p.SaleTransactionId).Distinct().ToArray();
        var refunds = await db.Refunds.AsNoTracking().Where(r => sales.Contains(r.SaleTransactionId)).OrderBy(r => r.CreatedAt).ThenBy(r => r.Id).ToListAsync(ct);
        var ids = refunds.Select(r => r.Id).ToArray();
        var allocations = await db.CommissionRefundAllocations.AsNoTracking().Where(a => ids.Contains(a.RefundId)).ToListAsync(ct);
        var already = new Dictionary<Guid, decimal>();
        foreach (var refund in refunds.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id))
        {
            var explicitRows = allocations.Where(a => a.RefundId == refund.Id).ToList();
            if (explicitRows.Count > 0)
            {
                if (explicitRows.Sum(a => a.Amount) != refund.Amount) throw new BusinessRuleException("Refund allocation history does not reconcile.");
                foreach (var a in explicitRows) already[a.PaymentId] = already.GetValueOrDefault(a.PaymentId) + a.Amount;
            }
            else if (refund.OriginalPaymentId is Guid original)
            {
                var p = payments.SingleOrDefault(p => p.Id == original && p.SaleTransactionId == refund.SaleTransactionId)
                    ?? throw new BusinessRuleException("Legacy refund has an invalid original collection.");
                if (p.Amount - already.GetValueOrDefault(p.Id) < refund.Amount) throw new BusinessRuleException("Legacy refund exceeds its original collection.");
                already[p.Id] = already.GetValueOrDefault(p.Id) + refund.Amount;
            }
            else AllocateNewest(payments.Where(p => p.SaleTransactionId == refund.SaleTransactionId).ToArray(), refund.Amount, already, refund.CreatedAt);
        }
        if (payments.Any(p => already.GetValueOrDefault(p.Id) > p.Amount)) throw new BusinessRuleException("Refund allocation exceeds a collection.");
        return already;
    }
    internal async Task AllocateRefundAsync(Refund refund, CancellationToken ct)
    {
        var payments = await db.Payments.AsNoTracking().Where(p => p.SaleTransactionId == refund.SaleTransactionId && p.Status == PaymentStatus.Completed).ToListAsync(ct);
        var prior = await RefundedAsync(payments, ct);
        var allocation = AllocateNewest(payments, refund.Amount, prior, refund.CreatedAt);
        foreach (var (paymentId, amount) in allocation)
        {
            var p = payments.Single(p => p.Id == paymentId);
            if (await db.CommissionPeriods.AnyAsync(period => period.Status == CommissionPeriodStatus.Closed && period.BranchId == p.BranchId
                && (period.Id == p.CommissionPeriodId || period.StartBusinessDate <= p.BusinessDate && period.EndBusinessDate >= p.BusinessDate), ct))
                throw new BusinessRuleException("Refund would change a finalized commission period; no financial or entitlement change was made.");
            db.Add(new CommissionRefundAllocation { Refund = refund, PaymentId = paymentId, Amount = amount });
        }
        access.Audit(refund.ProcessedByUserId, refund.BranchId, "Commission.RefundAllocated", nameof(Refund), refund.Id, after: allocation);
    }
}
