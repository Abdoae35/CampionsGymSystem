using CampionsSystem.BLL.Coaching;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;

public sealed partial class SalesService
{
    // Same sale/payment/installment/ledger path as Gym sales. The entitlement grant
    // and collection are one transaction, without an independent payment engine.
    public Task<CoachingPurchaseReceipt> PurchaseCoachingAsync(CoachingPurchaseRequest r, bool classes, CancellationToken ct = default)
        => TransactionAsync(async () =>
        {
            Operation(r.OperationId);
            var packageBranch = classes
                ? await db.ClassesPackages.Where(x => x.Id == r.PackageId).Select(x => (Guid?)x.BranchId).SingleOrDefaultAsync(ct)
                : await db.PrivatePackages.Where(x => x.Id == r.PackageId).Select(x => (Guid?)x.BranchId).SingleOrDefaultAsync(ct);
            var branch = packageBranch ?? throw new BranchNotFoundException("Package not found.");
            var actor = await RequireAsync(SalesPermissions.Create, branch, ct);
            await RequireAsync(classes ? CoachingPermissions.ClassesManage : CoachingPermissions.PrivateManage, branch, ct);
            await shiftMoney.LockBranchAsync(branch, ct);
            if (!await db.GymBranches.AnyAsync(b => b.Id == branch && b.IsActive && b.City.IsActive, ct)) throw new BusinessRuleException("Active selling branch and city required.");
            var member = await LockMemberAsync(r.MemberId, ct); var hash = Hash(new { r, classes, branch });
            var prior = await db.SaleTransactions.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
            if (prior != null)
            {
                if (prior.RequestHash != hash || prior.CreatedByUserId != actor.UserId || prior.BranchId != branch) throw new BusinessRuleException("Operation belongs to another coaching purchase.");
                var subId = classes ? await db.ClassesSubscriptions.Where(s => s.SaleTransactionId == prior.Id).Select(s => s.Id).SingleAsync(ct)
                    : await db.PrivateSubscriptionTransactions.Where(t => t.SaleTransactionId == prior.Id).Select(t => t.PrivateSubscriptionId).SingleAsync(ct);
                return new CoachingPurchaseReceipt(subId, prior.Id, await db.Payments.Where(p => p.SaleTransactionId == prior.Id).Select(p => p.Id).ToListAsync(ct));
            }
            var shift = await shiftMoney.RequireShiftAsync(branch, actor.UserId, ct);
            var (date, day) = await BusinessDateAsync(branch, ct);
            string name; int count, days; decimal price;
            if (classes)
            {
                if (r.CoachEmployeeId != null) throw new BusinessRuleException("Classes packages share sessions across types and are not assigned to a Private coach.");
                var p = await db.ClassesPackages.FromSqlInterpolated($"SELECT * FROM \"ClassesPackages\" WHERE \"Id\"={r.PackageId} FOR SHARE").AsNoTracking().SingleAsync(ct);
                if (!p.IsActive) throw new BusinessRuleException("Package is inactive.");
                (name, count, days, price) = (p.Name, p.SessionCount, p.DurationDays, p.Price);
            }
            else
            {
                var p = await db.PrivatePackages.FromSqlInterpolated($"SELECT * FROM \"PrivatePackages\" WHERE \"Id\"={r.PackageId} FOR SHARE").AsNoTracking().SingleAsync(ct);
                if (!p.IsActive || r.CoachEmployeeId == null) throw new BusinessRuleException("Active Private package and assigned coach required.");
                await new CoachingService(db, current, access, subscriptions).ValidateCoachAsync(r.CoachEmployeeId.Value, branch, date, ct);
                (name, count, days, price) = (p.Name, p.SessionCount, p.DurationDays, p.Price);
            }
            if (!member.IsActive || count <= 0 || days <= 0 || days > DateOnly.MaxValue.DayNumber - date.DayNumber) throw new BusinessRuleException("Invalid package duration/count or inactive member.");
            Amount(price);
            if (r.PurchaseMode is not (PurchaseMode.FullPayment or PurchaseMode.Installment) || r.PaymentParts == null || r.PaymentParts.Count is < 1 or > 20 || r.PaymentParts.Any(p => p is null))
                throw new BusinessRuleException("Coaching packages support full payment or an explicitly scheduled installment purchase with an initial collection.");
            foreach (var p in r.PaymentParts) Amount(p.Amount);
            var tender = r.PaymentParts.Sum(p => p.Amount); Amount(tender);
            if (tender > price || r.PurchaseMode == PurchaseMode.FullPayment && (tender != price || r.Installments is { Count: > 0 })) throw new BusinessRuleException("Invalid package collection total.");
            if (r.PurchaseMode == PurchaseMode.Installment)
            {
                await RequireAsync(SalesPermissions.Installments, branch, ct);
                ValidateInstallmentSchedule(r.Installments, price - tender, tender, price, date);
            }
            var sale = new SaleTransaction { MemberId = member.Id, BranchId = branch, SaleType = classes ? SaleType.Classes : SaleType.Private,
                OperationId = r.OperationId, RequestHash = hash, CreatedShiftId = shift.Id, BusinessDate = date, BusinessDayId = day,
                PurchaseMode = r.PurchaseMode, ActivationMode = SubscriptionActivationMode.Now, InitialPaymentRequired = r.PurchaseMode == PurchaseMode.Installment ? tender : null,
                GrossAmount = price, NetAmount = price, Status = tender == price ? SaleStatus.Completed : SaleStatus.Open,
                SalesAttributionSnapshot = AssignmentType.House, CommissionEligible = false, TargetEligible = false,
                CreatedByUserId = actor.UserId, CreatedAt = access.Now };
            db.Add(sale);
            if (r.PurchaseMode == PurchaseMode.Installment) CreateInstallmentSchedule(sale, r.Installments!, date, actor.UserId);
            var end = date.AddDays(days - 1); var unit = CoachingService.UnitValue(price, count); var remainder = price - unit * count;
            Guid subscriptionId;
            if (classes)
            {
                var sub = new ClassesSubscription { MemberId = member.Id, BranchId = branch, ClassesPackageId = r.PackageId, SaleTransaction = sale,
                    PackageName = name, PurchasedSessions = count, RemainingSessions = count, Price = price, UnitSessionValue = unit, RemainderValue = remainder,
                    StartDate = date, EndDate = end, CreatedAt = access.Now }; db.Add(sub); subscriptionId = sub.Id;
            }
            else
            {
                var sub = new PrivateSubscription { MemberId = member.Id, BranchId = branch, StartDate = date, EndDate = end,
                    TotalSessions = count, RemainingSessions = count, Status = PrivateSubscriptionStatus.Active, CreatedAt = access.Now };
                var assignment = new PrivateCoachAssignment { PrivateSubscription = sub, CoachEmployeeId = r.CoachEmployeeId!.Value,
                    AssignedAt = access.Now, CreatedAt = access.Now, CreatedByUserId = actor.UserId, OperationId = r.OperationId, RequestHash = hash };
                var batch = new PrivateSubscriptionTransaction { PrivateSubscription = sub, SaleTransaction = sale, PrivatePackageId = r.PackageId,
                    TransactionType = SubscriptionTransactionType.Initial, SessionsAdded = count, RemainingSessions = count, DurationDaysAdded = days,
                    NetAmount = price, UnitSessionValue = unit, RemainderValue = remainder, NewEndDate = end, StartDateSnapshot = date,
                    OriginalCoachEmployeeId = r.CoachEmployeeId, OriginalAssignment = assignment, PackageNameSnapshot = name, CreatedAt = access.Now };
                db.Add(batch); subscriptionId = sub.Id;
            }
            var payment = await PaymentAsync(sale, r.OperationId, hash, tender, r.PaymentParts, actor.UserId, ct);
            access.Audit(actor.UserId, branch, classes ? "Classes.Purchased" : "Private.Purchased", nameof(SaleTransaction), sale.Id,
                after: new { subscriptionId, count, price, unit, remainder, date, end, r.CoachEmployeeId });
            await db.SaveChangesAsync(ct); return new CoachingPurchaseReceipt(subscriptionId, sale.Id, [payment.Id]);
        }, ct);

    private async Task GuardCoachingRefundAsync(SaleTransaction sale, decimal newNetCollection, CancellationToken ct)
    {
        if (sale.SaleType != SaleType.Private) return;
        var protectedValue = await db.CoachEarnings.Where(e => e.SaleTransactionId == sale.Id).SumAsync(e => e.EligibleValue, ct);
        if (newNetCollection < protectedValue) throw new BusinessRuleException("Refund affects finalized Private earnings. An Owner adjustment policy is required; nothing was changed.");
    }
    private async Task CancelCoachingGrantAsync(SaleTransaction sale, Guid actor, CancellationToken ct)
    {
        if (sale.SaleType == SaleType.Private)
        {
            var sub = await db.PrivateSubscriptions.SingleOrDefaultAsync(s => db.PrivateSubscriptionTransactions.Any(t => t.PrivateSubscriptionId == s.Id && t.SaleTransactionId == sale.Id), ct);
            if (sub != null) { sub.Status = PrivateSubscriptionStatus.Cancelled; sub.Version++; access.Audit(actor, sale.BranchId, "Private.CancelledFromRefund", nameof(PrivateSubscription), sub.Id, after: new { sale.Id }); }
        }
        // Classes eligibility checks the original Sale cancellation; the historical
        // balance and consumed attendance are preserved without fake counter changes.
    }
}
