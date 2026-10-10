using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;

public sealed partial class SubscriptionService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    private async Task<CurrentUserContext> RequireAsync(string permission, Guid? branch, bool global, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        if (user is null || !user.Permissions.Contains(permission)
            || global && !user.Permissions.Contains("Branches.AccessAll")) throw new UnauthorizedAccessException();
        if (branch.HasValue) BranchAccessService.EnsureScope(user, branch);
        return user;
    }
    public async Task<DateOnly> EffectiveBusinessDateAsync(Guid branchId, CancellationToken ct = default)
        => await db.BusinessDays.Where(x => x.BranchId == branchId && x.Status == BusinessDayStatus.Open)
            .Select(x => (DateOnly?)x.BusinessDate).SingleOrDefaultAsync(ct) ?? access.Today;

    private sealed class Evaluation
    {
        public MemberSubscription Subscription { get; init; } = null!;
        public SubscriptionStatus Status { get; init; }
        public string? PlanName { get; init; }
        public PlanCategory? PlanType { get; init; }
    }
    private IQueryable<Evaluation> Evaluated(DateOnly? at = null)
    {
        var today = access.Today;
        var facts = db.MemberSubscriptions.AsNoTracking().Select(s => new
        {
            S = s,
            Day = s.EmployeeId != null ? today : at ?? db.BusinessDays.Where(d => d.BranchId == s.PurchasedAtBranchId && d.Status == BusinessDayStatus.Open)
                .Select(d => (DateOnly?)d.BusinessDate).SingleOrDefault() ?? today,
            EmploymentValid = s.EmployeeId == null || s.Employee != null && s.Employee.MemberId == s.MemberId
                && s.Employee.EmploymentStatus == EmploymentStatus.Active && s.Employee.HireDate <= today
                && (s.Employee.TerminationDate == null || s.Employee.TerminationDate >= today),
            MemberActive = s.Member.IsActive,
            UnreconciledFreeze = s.Status == SubscriptionStatus.Frozen && !db.SubscriptionFreezes.Any(f => f.MemberSubscriptionId == s.Id && f.FreezeType == FreezeType.Normal),
            Frozen = db.SubscriptionFreezes.Any(f => f.MemberSubscriptionId == s.Id && f.FreezeType == FreezeType.Normal
                && f.StartDate <= (at ?? db.BusinessDays.Where(d => d.BranchId == s.PurchasedAtBranchId && d.Status == BusinessDayStatus.Open)
                    .Select(d => (DateOnly?)d.BusinessDate).SingleOrDefault() ?? today)
                && (f.EndDate == null || f.EndDate >= (at ?? db.BusinessDays.Where(d => d.BranchId == s.PurchasedAtBranchId && d.Status == BusinessDayStatus.Open)
                    .Select(d => (DateOnly?)d.BusinessDate).SingleOrDefault() ?? today))),
            Overdue = db.Installments.Any(i => i.InstallmentAgreement.SaleTransaction.OperationId != null
                && i.InstallmentAgreement.SaleTransaction.PurchaseMode == PurchaseMode.Installment && i.InstallmentAgreement.SaleTransaction.CancelledAt == null
                && i.DueDate < (at ?? db.BusinessDays.Where(d=>d.BranchId==s.PurchasedAtBranchId && d.Status==BusinessDayStatus.Open).Select(d=>(DateOnly?)d.BusinessDate).SingleOrDefault() ?? today)
                && i.AmountDue > db.InstallmentPaymentAllocations.Where(a=>a.InstallmentId==i.Id && a.Payment.Status==PaymentStatus.Completed).Sum(a=>a.Amount)
                && db.MembershipSubscriptionTransactions.Any(g=>g.SaleTransactionId==i.InstallmentAgreement.SaleTransactionId && g.MemberSubscriptionId==s.Id)),
            Name = db.MembershipSubscriptionTransactions.Where(t => t.MemberSubscriptionId == s.Id)
                .OrderBy(t => t.CreatedAt).ThenBy(t => t.Id).Select(t => t.PlanNameSnapshot).FirstOrDefault(),
            Type = db.MembershipSubscriptionTransactions.Where(t => t.MemberSubscriptionId == s.Id)
                .OrderBy(t => t.CreatedAt).ThenBy(t => t.Id).Select(t => (PlanCategory?)t.PlanCategorySnapshot).FirstOrDefault()
        });
        return facts.Select(x => new Evaluation
        {
            Subscription = x.S, PlanName = x.Name, PlanType = x.Type,
            Status = x.S.Status == SubscriptionStatus.Cancelled ? SubscriptionStatus.Cancelled
                : x.S.Status == SubscriptionStatus.SuspendedForInstallment ? SubscriptionStatus.SuspendedForInstallment
                : !x.MemberActive || !x.EmploymentValid || x.S.EmployeeId != null && x.S.EndDate != null ? SubscriptionStatus.Expired
                : x.Overdue ? SubscriptionStatus.SuspendedForInstallment
                : x.Day < x.S.StartDate ? SubscriptionStatus.Scheduled
                : x.S.EndDate != null && x.Day > x.S.EndDate || x.S.AccessMode == AccessMode.SessionBased && x.S.RemainingSessions <= 0 ? SubscriptionStatus.Expired
                : x.Frozen || x.UnreconciledFreeze ? SubscriptionStatus.Frozen : SubscriptionStatus.Active
        });
    }
    private static SubscriptionSummary Summary(Evaluation e) => new(e.Subscription.Id, e.Subscription.MemberId,
        e.Subscription.MembershipPlanId, e.PlanName, e.PlanType?.ToString() ?? (e.Subscription.EmployeeId != null ? "Employee" : null),
        e.Subscription.PurchasedAtBranchId, e.Subscription.CityId, e.Subscription.AccessMode.ToString(),
        e.Subscription.StartDate, e.Subscription.EndDate, e.Status.ToString(), e.Subscription.TotalSessions,
        e.Subscription.RemainingSessions, e.Subscription.Version, e.Subscription.EmployeeId, e.Subscription.FollowsSubscriptionId, e.Subscription.IsStandaloneVisit);

    // Assembly-internal operational projection. Check-In authorizes the visited branch first;
    // it must not inherit originating-branch administration permissions.
    internal async Task<IReadOnlyList<SubscriptionSummary>> GetGymAccessFactsAsync(Guid memberId, DateOnly date, CancellationToken ct)
    {
        var lastEnd=await db.MemberSubscriptions.Where(s=>s.MemberId==memberId && !s.IsStandaloneVisit && s.EndDate<date).MaxAsync(s=>s.EndDate,ct);
        var rows=await Evaluated(date).Where(x=>x.Subscription.MemberId==memberId
            && (!x.Subscription.IsStandaloneVisit || x.Subscription.EndDate>=date && x.Subscription.RemainingSessions>0)
            && (x.Subscription.EndDate==null || x.Subscription.EndDate>=date || !x.Subscription.IsStandaloneVisit && x.Subscription.EndDate==lastEnd))
            .OrderByDescending(x=>x.Subscription.StartDate).Take(101).ToListAsync(ct);
        return rows.Select(Summary).ToArray();
    }

    public async Task<SubscriptionHistory> GetHistoryAsync(Guid memberId, SubscriptionHistoryRequest r, CancellationToken ct = default)
    {
        var user = await RequireAsync(SubscriptionPermissions.View, r.BranchId, false, ct);
        if (r.Page < 1 || r.PageSize is < 1 or > 100 || (long)(r.Page - 1) * r.PageSize > int.MaxValue
            || r.From > r.To || r.Status.HasValue && !Enum.IsDefined(r.Status.Value)
            || r.PlanType.HasValue && !Enum.IsDefined(r.PlanType.Value)) throw new BusinessRuleException("Invalid subscription filters or pagination.");
        var q = Evaluated().Where(x => x.Subscription.MemberId == memberId);
        if (!user.Permissions.Contains("Branches.AccessAll") || r.BranchId != null)
        {
            var branch = r.BranchId ?? user.CurrentBranchId ?? throw new UnauthorizedAccessException();
            q = q.Where(x => x.Subscription.PurchasedAtBranchId == branch);
        }
        if (r.Status.HasValue) q = q.Where(x => x.Status == r.Status);
        if (r.PlanType.HasValue) q = q.Where(x => x.PlanType == r.PlanType || r.PlanType == PlanCategory.Employee && x.Subscription.EmployeeId != null);
        if (r.From.HasValue) q = q.Where(x => x.Subscription.StartDate >= r.From);
        if (r.To.HasValue) q = q.Where(x => x.Subscription.StartDate <= r.To);
        var count = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.Subscription.StartDate).ThenBy(x => x.Subscription.Id)
            .Skip((r.Page - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
        return new(rows.Select(Summary).ToArray(), count, r.Page, r.PageSize);
    }
    public async Task<SubscriptionDetails> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        var e = await Evaluated().SingleOrDefaultAsync(x => x.Subscription.Id == id, ct)
            ?? throw new BranchNotFoundException("Subscription not found.");
        await RequireAsync(SubscriptionPermissions.View, e.Subscription.PurchasedAtBranchId, false, ct);
        var grants = await db.MembershipSubscriptionTransactions.AsNoTracking().Where(x => x.MemberSubscriptionId == id)
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToListAsync(ct);
        var freezes = await db.SubscriptionFreezes.AsNoTracking().Where(x => x.MemberSubscriptionId == id).OrderBy(x => x.StartDate).ToListAsync(ct);
        var cancellations=await db.SubscriptionGrantCancellations.AsNoTracking().Where(c=>c.Grant.MemberSubscriptionId==id).ToListAsync(ct);
        var liveGrants=grants.Where(g=>!cancellations.Any(c=>c.GrantId==g.Id)).ToArray();
        int? allowance = liveGrants.Length == 0 || liveGrants.Any(x => x.FreezeDaysSnapshot == null) ? null : liveGrants.Sum(x => x.FreezeDaysSnapshot!.Value);
        var used = freezes.Where(x => x.FreezeType == FreezeType.Normal).Sum(x => x.DaysApplied ?? 0);
        return new(Summary(e), await db.Members.Where(x => x.Id == e.Subscription.MemberId).Select(x => x.FullName).SingleAsync(ct),
            allowance, used, allowance.HasValue?Math.Max(0,allowance.Value-used):null,
            await db.MemberSubscriptionBenefits.AsNoTracking().Where(x => x.MembershipSubscriptionTransaction.MemberSubscriptionId == id)
                .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Select(x => new BenefitSnapshot(x.Id, x.MembershipSubscriptionTransactionId,
                    x.BenefitCodeSnapshot, x.BenefitNameSnapshot, x.GrantTypeSnapshot.ToString(), x.GrantedQuantity,
                    db.SubscriptionGrantCancellations.Any(c=>c.GrantId==x.MembershipSubscriptionTransactionId))).ToListAsync(ct),
            grants.Select(x => new SubscriptionGrantDetails(x.Id, x.MembershipPlanId, x.PlanNameSnapshot, x.PlanCategorySnapshot.ToString(),
                x.PlanBranchIdSnapshot, x.CityIdSnapshot, x.AccessModeSnapshot?.ToString(), x.PurchasedDurationDays, x.PurchasedSessions,
                x.FreezeDaysSnapshot, x.PlanPriceSnapshot, x.NetPriceSnapshot, x.FreelanceCoachEmployeeIdSnapshot, x.TransactionType.ToString(),
                x.StartDateSnapshot, x.PreviousEndDate, x.NewEndDate, x.PreviousRemainingSessions, x.RenewedFromSubscriptionId, x.BusinessDayId, x.CreatedByUserId, x.CreatedAt,
                cancellations.FirstOrDefault(c=>c.GrantId==x.Id)?.RefundId)).ToArray(),
            freezes.Select(FreezeView).ToArray(),
            await db.SubscriptionHolidayExtensions.AsNoTracking().Where(x => x.MemberSubscriptionId == id)
                .OrderBy(x => x.ProtectedDate).Select(x => new HolidayExtensionDetails(x.BranchClosureId, x.ProtectedDate, x.DaysApplied)).ToListAsync(ct));
    }
    // Cross-branch access reads are permitted only in the caller's operational branch and snapshot city.
    // Administration/history continue to use the subscription's originating branch.
    public async Task<MemberEntitlement> GetCurrentEntitlementAsync(Guid memberId, Guid? branchId = null, CancellationToken ct = default)
    {
        var user=await RequireAsync(SubscriptionPermissions.View, branchId, false, ct);
        branchId ??= user.CurrentBranchId ?? throw new BusinessRuleException("Select an operational branch for access lookup.");
        BranchAccessService.EnsureScope(user,branchId);
        var branch = await db.GymBranches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == branchId && x.IsActive && x.City.IsActive, ct)
            ?? throw new BranchNotFoundException("Active access branch not found.");
        var date = await EffectiveBusinessDateAsync(branchId.Value, ct);
        var candidates = await Evaluated(date).Where(x => x.Subscription.MemberId == memberId && x.Subscription.CityId == branch.CityId
            && (x.Status == SubscriptionStatus.Active || x.Status == SubscriptionStatus.Frozen)).Take(2).ToListAsync(ct);
        if (candidates.Count > 1) throw new BusinessRuleException("Conflicting entitlements require management reconciliation.");
        var closed = await db.BranchClosures.AnyAsync(x => x.BranchId == branchId && x.AppliesToGym && x.StartDate <= date && x.EndDate >= date, ct);
        return new(candidates.Count == 1 && candidates[0].Status == SubscriptionStatus.Active && !closed,
            candidates.Count == 1 ? Summary(candidates[0]) : null);
    }
}
