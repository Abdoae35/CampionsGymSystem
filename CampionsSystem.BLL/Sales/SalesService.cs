using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Subscriptions;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;

public sealed partial class SalesService(ApplicationDbContext db, ICurrentUserService current,
    OperationAccess access, OfferService offers, SubscriptionService subscriptions)
{
    private readonly FinancialOperations shiftMoney = new(db, current, access);
    private static string Hash<T>(T request) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))));
    private static void Operation(Guid id) { if (id == Guid.Empty) throw new BusinessRuleException("A nonempty operation identity is required."); }
    private static void Amount(decimal value, bool positive = true)
    {
        OperationAccess.Money(value);
        if (value > 9999999999999999.99m || positive && value <= 0) throw new BusinessRuleException("A valid positive money amount is required.");
    }
    private async Task<CurrentUserContext> RequireAsync(string permission, Guid? branch, CancellationToken ct)
    {
        var actor = await current.GetAsync(ct);
        if (actor is null || !actor.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        BranchAccessService.EnsureScope(actor, branch); return actor;
    }
    private async Task<T> TransactionAsync<T>(Func<Task<T>> change, CancellationToken ct)
    {
        var owned = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(ct) : null;
        var tx = db.Database.CurrentTransaction!; var point = "finance_" + Guid.NewGuid().ToString("N");
        try { await tx.CreateSavepointAsync(point, ct); var result = await change(); await tx.ReleaseSavepointAsync(point, ct); if (owned != null) await owned.CommitAsync(ct); return result; }
        catch { if (owned != null) await owned.RollbackAsync(CancellationToken.None); else await tx.RollbackToSavepointAsync(point, CancellationToken.None); db.ChangeTracker.Clear(); throw; }
        finally { if (owned != null) await owned.DisposeAsync(); }
    }
    private async Task<Member> LockMemberAsync(Guid id, CancellationToken ct)
    {
        var row = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Member not found."); await db.Entry(row).ReloadAsync(ct); return row;
    }
    private async Task<SaleTransaction> LockSaleAsync(Guid id, string permission, CancellationToken ct)
    {
        var info = await db.SaleTransactions.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.MemberId, x.BranchId }).SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Sale not found.");
        await RequireAsync(permission, info.BranchId, ct); await shiftMoney.LockBranchAsync(info.BranchId, ct); await LockMemberAsync(info.MemberId, ct);
        var sale = await db.SaleTransactions.FromSqlInterpolated($"SELECT * FROM \"SaleTransactions\" WHERE \"Id\"={id} FOR UPDATE").SingleAsync(ct);
        await db.Entry(sale).ReloadAsync(ct);
        if (sale.OperationId == null) throw new BusinessRuleException("Legacy Sale requires reconciliation before managed Finance operations.");
        return sale;
    }
    private async Task<(DateOnly Date, Guid? Day)> BusinessDateAsync(Guid branch, CancellationToken ct)
    {
        var day = await db.BusinessDays.FromSqlInterpolated($"SELECT * FROM \"BusinessDays\" WHERE \"BranchId\"={branch} AND \"Status\"='Open' FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (day != null && (day.OpenedAt > access.Now || day.BusinessDate > access.Today)) throw new BusinessRuleException("Invalid open BusinessDay.");
        return (day?.BusinessDate ?? access.Today, day?.Id);
    }
    private async Task<IReadOnlyList<FinancialAccount>> AccountsAsync(Guid branch, IReadOnlyList<PaymentPartInput> parts, decimal amount, CancellationToken ct, bool historicalReversal=false)
    {
        Amount(amount);
        if (parts is null || parts.Count<1 || !historicalReversal && parts.Count>20 || parts.Any(x => x is null || x.FinancialAccountId == Guid.Empty || x.ProviderReference?.Length > 256)
            ) throw new BusinessRuleException("PaymentParts must sum exactly to the payment amount.");
        foreach (var part in parts) Amount(part.Amount);
        if (parts.Sum(x=>x.Amount) != amount) throw new BusinessRuleException("PaymentParts must sum exactly to the payment amount.");
        var accounts = new List<FinancialAccount>();
        foreach (var id in parts.Select(x => x.FinancialAccountId).Distinct().Order())
        {
            var account = await db.FinancialAccounts.FromSqlInterpolated($"SELECT * FROM \"FinancialAccounts\" WHERE \"Id\"={id} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
            if (account is null || !historicalReversal && !account.IsActive || account.BranchId != branch || account.AccountType is not (FinancialAccountType.Cash or FinancialAccountType.InstaPay))
                throw new BusinessRuleException("Select an active Cash or InstaPay account of the operational branch.");
            await shiftMoney.AccountAsync(branch, id, ct, historicalReversal);
            accounts.Add(account);
        }
        return accounts;
    }
    private async Task<Payment> PaymentAsync(SaleTransaction sale, Guid operation, string hash, decimal amount,
        IReadOnlyList<PaymentPartInput> parts, Guid actor, CancellationToken ct)
    {
        var activeShift = await shiftMoney.RequireShiftAsync(sale.BranchId, actor, ct);
        await AccountsAsync(sale.BranchId, parts, amount, ct);
        var (date, day) = await BusinessDateAsync(sale.BranchId, ct);
        var payment = new Payment { SaleTransaction = sale, BranchId = sale.BranchId, OperationId = operation, RequestHash = hash,
            ShiftId = activeShift.Id, Amount = amount, Status = PaymentStatus.Completed, PaidAt = access.Now, ReceivedByUserId = actor, BusinessDate = date, BusinessDayId = day };
        db.Add(payment);
        foreach (var p in parts)
        {
            var part = new PaymentPart { Payment = payment, FinancialAccountId = p.FinancialAccountId, Amount = p.Amount, ProviderReference = p.ProviderReference, CreatedAt = access.Now };
            db.Add(part); access.Audit(actor, sale.BranchId, "Payment.PartRecorded", nameof(PaymentPart), part.Id,
                after: new { payment.Id, p.FinancialAccountId, p.Amount });
        }
        foreach (var group in parts.GroupBy(x => x.FinancialAccountId)) await shiftMoney.PostAsync(new FinancialTransaction {
            FinancialAccountId = group.Key, BranchId = sale.BranchId, ShiftId = activeShift.Id, Payment = payment, SourceType = FinancialSourceType.Payment,
            TransactionType = FinancialTransactionType.Credit, Amount = group.Sum(x => x.Amount), BusinessDate = date,
            BusinessDayId = day, OccurredAt = access.Now, CreatedByUserId = actor }, ct);
        access.Audit(actor, sale.BranchId, "Payment.Recorded", nameof(Payment), payment.Id, after: new { sale.Id, amount, date });
        return payment;
    }
    // Internal until the same transaction also grants the purchased entitlement; never an HTTP money-only shortcut.
    internal Task<SaleReceipt> RecordSaleAsync(CreateSaleRequest r, CancellationToken ct = default) => TransactionAsync(async () =>
    {
        Operation(r.OperationId);
        var actor = await current.GetAsync(ct) ?? throw new UnauthorizedAccessException();
        var branch = r.BranchId ?? actor.CurrentBranchId ?? throw new BusinessRuleException("Select an operational branch.");
        actor = await RequireAsync(SalesPermissions.Create, branch, ct);
        await shiftMoney.LockBranchAsync(branch, ct);
        var branchRow = await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={branch} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (branchRow is null || !branchRow.IsActive || !await db.Cities.AnyAsync(x => x.Id == branchRow.CityId && x.IsActive, ct)) throw new BusinessRuleException("Selling branch must be active.");
        // Match the existing plan-edit lock before member locks; delayed activation uses the immutable sold snapshot.
        var plan = await db.MembershipPlans.FromSqlInterpolated($"SELECT * FROM \"MembershipPlans\" WHERE \"Id\"={r.MembershipPlanId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Plan not found.");
        var member = await LockMemberAsync(r.MemberId, ct); var hash = Hash(r with { BranchId = branch });
        var prior = await db.SaleTransactions.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
        if (prior != null)
        {
            if (prior.RequestHash != hash || prior.CreatedByUserId != actor.UserId || prior.BranchId != branch) throw new BusinessRuleException("Operation identity belongs to another Sale request.");
            return new SaleReceipt(prior.Id, await db.Payments.Where(x => x.SaleTransactionId == prior.Id).Select(x => x.Id).ToListAsync(ct));
        }
        var activeShift = await shiftMoney.RequireShiftAsync(branch, actor.UserId, ct);
        if (r.PurchaseMode is not (PurchaseMode.FullPayment or PurchaseMode.Reservation or PurchaseMode.Installment or PurchaseMode.StandaloneSingleSession) || !Enum.IsDefined(r.ActivationMode) || r.PaymentParts is null
            || (r.ActivationMode == SubscriptionActivationMode.SpecificDate) != r.StartDate.HasValue
            || r.PaymentParts.Count > 20 || r.PurchaseMode != PurchaseMode.Installment && r.PaymentParts.Count == 0 || r.PaymentParts.Any(x => x is null)
            || r.PurchaseMode != PurchaseMode.Reservation && r.ReservationDueDate != null
            || r.PurchaseMode != PurchaseMode.Installment && (r.Installments is { Count: > 0 } || r.InitialPaymentRequired != null)) throw new BusinessRuleException("Invalid Sale configuration.");
        foreach (var part in r.PaymentParts) Amount(part.Amount);
        var (date, day) = await BusinessDateAsync(branch, ct);
        if (r.ReservationDueDate < date) throw new BusinessRuleException("Reservation due date cannot precede the effective BusinessDate.");
        if (r.StartDate < date) throw new BusinessRuleException("Specific activation date must not precede the effective BusinessDate.");
        if (r.OfferId.HasValue) await db.Offers.FromSqlInterpolated($"SELECT * FROM \"Offers\" WHERE \"Id\"={r.OfferId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (r.PurchaseMode == PurchaseMode.StandaloneSingleSession && (plan.AccessMode != AccessMode.SessionBased || plan.SessionCount != 1))
            throw new BusinessRuleException("Standalone visits require an eligible one-session SessionBased plan.");
        var price = await offers.PriceAsync(member, plan, r.OfferId, branch, date, ct); Amount(price.FinalPrice);
        if(r.PurchaseMode!=PurchaseMode.StandaloneSingleSession)await subscriptions.ValidatePaidDebtCapacityAsync(member.Id,plan,ct);
        var tender = r.PaymentParts.Sum(x => x.Amount); Amount(tender, r.PurchaseMode != PurchaseMode.Installment);
        var initialRequired = r.PurchaseMode == PurchaseMode.Installment ? r.InitialPaymentRequired ?? tender : (decimal?)null;
        if (r.PurchaseMode == PurchaseMode.Installment)
        {
            await RequireAsync(SalesPermissions.Installments, branch, ct);
            ValidateInstallmentSchedule(r.Installments, price.FinalPrice - tender, initialRequired!.Value, price.FinalPrice, date);
        }
        if (tender > price.FinalPrice || r.PurchaseMode is PurchaseMode.FullPayment or PurchaseMode.StandaloneSingleSession && tender != price.FinalPrice)
            throw new BusinessRuleException("Payment must not exceed the Sale price; full-payment purchases require the exact final price.");
        var owner = await db.MemberSalesAssignments.AsNoTracking().SingleOrDefaultAsync(x => x.MemberId == member.Id && x.EndedAt == null && x.AssignedAt <= access.Now, ct);
        var owned = owner?.AssignmentType == AssignmentType.Sales;
        var renewal = await db.MemberSubscriptions.AnyAsync(x => x.MemberId == member.Id && x.EmployeeId == null && !x.IsStandaloneVisit, ct);
        var sale = new SaleTransaction { MemberId = member.Id, BranchId = branch, SaleType = SaleType.GymMembership,
            OperationId = r.OperationId, RequestHash = hash, CreatedShiftId = activeShift.Id, BusinessDate = date, BusinessDayId = day,
            PurchaseMode = r.PurchaseMode, ActivationMode = r.ActivationMode, RequestedStartDate = r.StartDate, ReservationDueDate = r.ReservationDueDate, InitialPaymentRequired = initialRequired,
            GrossAmount = price.OriginalPrice, DiscountAmount = price.Discount, NetAmount = price.FinalPrice, Status = tender == price.FinalPrice ? SaleStatus.Completed : SaleStatus.Open,
            OfferId = price.OfferId, OfferNameSnapshot = price.OfferName, OfferTypeSnapshot = price.OfferType, OfferValueSnapshot = price.OfferValue,
            OfferEligibilityModeSnapshot = price.EligibilityMode, OfferAudienceCategoryIdSnapshot = price.AudienceCategoryId, ExtraDaysSnapshot = price.ExtraDays,
            SalesAttributionSnapshot = owned ? AssignmentType.Sales : AssignmentType.House, SalesEmployeeId = owned ? owner!.SalesEmployeeId : null,
            SaleCategory = owned && r.PurchaseMode != PurchaseMode.StandaloneSingleSession ? renewal ? SaleCategory.Renewal : SaleCategory.NewMembership : null,
            CommissionEligible = owned && plan.PlanCategory == PlanCategory.Normal && r.PurchaseMode != PurchaseMode.StandaloneSingleSession,
            TargetEligible = owned && plan.PlanCategory == PlanCategory.Normal && r.PurchaseMode != PurchaseMode.StandaloneSingleSession,
            CreatedByUserId = actor.UserId, CreatedAt = access.Now };
        var snapshot = new SaleMembershipSnapshot { SaleTransaction = sale, MembershipPlanId = plan.Id, PlanName = plan.Name,
            PlanCategory = plan.PlanCategory, CityId = plan.CityId, AccessMode = plan.AccessMode, DurationDays = plan.DurationDays,
            SessionCount = plan.SessionCount, FreezeDays = plan.AllowFreeze ? plan.IncludedFreezeDays : 0,
            FreelanceCoachEmployeeId = plan.FreelanceCoachEmployeeId, AudienceCategoryId = plan.MembershipPlanCategoryId };
        db.Add(snapshot);
        var configs = await db.MembershipPlanBenefits.AsNoTracking().Where(x => x.MembershipPlanId == plan.Id).ToListAsync(ct);
        foreach (var config in configs)
        {
            var benefit = await db.MembershipBenefits.FromSqlInterpolated($"SELECT * FROM \"MembershipBenefits\" WHERE \"Id\"={config.MembershipBenefitId} FOR SHARE").AsNoTracking().SingleAsync(ct);
            if (!benefit.IsActive) throw new BusinessRuleException("Plan contains an inactive benefit.");
            db.Add(new SaleBenefitSnapshot { SaleMembershipSnapshot = snapshot, MembershipBenefitId = benefit.Id, Code = benefit.Code, Name = benefit.Name,
                GrantType = config.GrantType, Quantity = config.Quantity });
        }
        if (r.PurchaseMode == PurchaseMode.Installment) CreateInstallmentSchedule(sale,r.Installments!,date,actor.UserId);
        var payment = tender > 0 ? await PaymentAsync(sale, r.OperationId, hash, tender, r.PaymentParts, actor.UserId, ct) : null;
        access.Audit(actor.UserId, branch, r.PurchaseMode == PurchaseMode.Reservation ? "Sale.Reserved" : "Sale.Created", nameof(SaleTransaction), sale.Id, after: new { member.Id, sale.NetAmount, sale.SalesEmployeeId,
            sale.SalesAttributionSnapshot, sale.CommissionEligible, sale.TargetEligible, date });
        if (price.OfferId != null) access.Audit(actor.UserId, branch, "Offer.Applied", nameof(SaleTransaction), sale.Id, after: price);
        await db.SaveChangesAsync(ct); return new SaleReceipt(sale.Id, payment == null ? [] : [payment.Id]);
    }, ct);
}
