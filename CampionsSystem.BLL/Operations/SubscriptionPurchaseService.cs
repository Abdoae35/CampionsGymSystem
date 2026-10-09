using System.Data;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Operations;

public sealed record ActivityPurchaseRequest(Guid MemberId, Guid ActivityPackageId, Guid ActivityCoachId,
    Guid? ExistingSubscriptionId = null);
public sealed record FreelancePurchaseRequest(Guid MemberId, Guid MembershipPlanId, Guid BranchId,
    Guid? ExistingSubscriptionId = null);
public sealed record PurchaseResult(Guid SubscriptionId, Guid TransactionId, Guid SaleTransactionId, decimal GymAmountDue);

// Creates the entitlement/purchase history and amount due. Receiving funds remains the existing payment model's concern.
public sealed class SubscriptionPurchaseService(ApplicationDbContext db, OperationAccess access)
{
    public async Task<PurchaseResult> PurchaseActivityAsync(ActivityPurchaseRequest request, CancellationToken ct = default)
    {
        await using var transaction = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        var package = await db.ActivityPackages.Include(x => x.SportsActivity)
            .SingleOrDefaultAsync(x => x.Id == request.ActivityPackageId && x.IsActive && x.SportsActivity.IsActive, ct)
            ?? throw new BusinessRuleException("Active activity package not found.");
        var actor = await access.RequireAsync("Activities.Manage", package.SportsActivity.BranchId, ct);
        if (!await db.Members.AnyAsync(x => x.Id == request.MemberId && x.IsActive, ct)) throw new BusinessRuleException("Active member required.");
        var coach = await db.ActivityCoaches.Include(x => x.Employee)
            .SingleOrDefaultAsync(x => x.Id == request.ActivityCoachId && x.IsActive, ct)
            ?? throw new BusinessRuleException("An active activity coach must be selected.");
        if (coach.CoachType == ActivityCoachType.Internal && (coach.Employee is null
            || coach.Employee.EmploymentStatus != EmploymentStatus.Active || coach.Employee.HireDate > access.Today
            || coach.Employee.TerminationDate < access.Today)) throw new BusinessRuleException("Coach Employee is not active.");
        var assignment = await db.ActivityCoachAssignments.Where(x => x.SportsActivityId == package.SportsActivityId
            && x.ActivityCoachId == coach.Id && x.IsActive && x.StartDate <= access.Today
            && (x.EndDate == null || x.EndDate >= access.Today)).OrderByDescending(x => x.StartDate).FirstOrDefaultAsync(ct)
            ?? throw new BusinessRuleException("Coach is not assigned to this activity.");
        var subscription = request.ExistingSubscriptionId.HasValue
            ? await db.ActivitySubscriptions.Include(x => x.ActivityPackage).SingleOrDefaultAsync(x => x.Id == request.ExistingSubscriptionId, ct)
                ?? throw new BusinessRuleException("Subscription not found.")
            : new ActivitySubscription { MemberId = request.MemberId, BranchId = package.SportsActivity.BranchId,
                StartDate = access.Today, AccessMode = package.AccessMode, Status = ActivitySubscriptionStatus.Scheduled };
        if (subscription.MemberId != request.MemberId || subscription.BranchId != package.SportsActivity.BranchId
            || (request.ExistingSubscriptionId.HasValue && (subscription.ActivityPackage.SportsActivityId != package.SportsActivityId
                || subscription.AccessMode != package.AccessMode))) throw new BusinessRuleException("Renewal context does not match.");
        var previousEnd = request.ExistingSubscriptionId.HasValue ? subscription.EndDate : (DateOnly?)null;
        var previousCoach = subscription.ActivityCoachId;
        subscription.ActivityCoachId = coach.Id; subscription.ActivityCoachAssignmentId = assignment.Id;
        subscription.ActivityPackageId = package.Id;
        subscription.EndDate = (previousEnd >= access.Today ? previousEnd.Value : access.Today).AddDays(package.DurationDays);
        if (package.AccessMode == AccessMode.SessionBased)
        {
            subscription.TotalSessions = (subscription.TotalSessions ?? 0) + package.SessionCount;
            subscription.RemainingSessions = (subscription.RemainingSessions ?? 0) + package.SessionCount;
        }
        if (!request.ExistingSubscriptionId.HasValue) db.Add(subscription);
        var sale = Sale(request.MemberId, subscription.BranchId, actor, SaleType.Activity, package.Price);
        var batch = new ActivitySubscriptionTransaction { ActivitySubscription = subscription, SaleTransaction = sale,
            ActivityPackageId = package.Id, ActivityCoachIdSnapshot = coach.Id, CoachPercentageSnapshot = coach.Percentage,
            TransactionType = request.ExistingSubscriptionId.HasValue ? SubscriptionTransactionType.Renewal : SubscriptionTransactionType.Initial,
            SessionsAdded = package.SessionCount, DurationDaysAdded = package.DurationDays, NetAmount = package.Price,
            UnitSessionValue = package.SessionCount.HasValue ? decimal.Round(package.Price / package.SessionCount.Value, 2) : null,
            PreviousEndDate = previousEnd, NewEndDate = subscription.EndDate, CreatedAt = access.Now };
        db.Add(batch);
        access.Audit(actor, subscription.BranchId, previousCoach.HasValue && previousCoach != coach.Id
            ? "ActivitySubscription.CoachChangedForRenewal" : "ActivitySubscription.CoachSelected", nameof(ActivitySubscription), subscription.Id,
            new { ActivityCoachId = previousCoach }, new { ActivityCoachId = coach.Id, batch.Id, coach.Percentage });
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return new(subscription.Id, batch.Id, sale.Id, sale.NetAmount);
    }

    // Retained signature for old callers; this financial/ownership path is deliberately disabled.
    public Task<PurchaseResult> PurchaseFreelanceAsync(FreelancePurchaseRequest request, CancellationToken ct = default)
        => Task.FromException<PurchaseResult>(new BusinessRuleException(
            "Legacy Freelance purchasing is disabled. Customer activation requires the future Sales/Payments integration."));

    private SaleTransaction Sale(Guid member, Guid branch, Guid actor, SaleType type, decimal price) => new()
    {
        MemberId = member, BranchId = branch, CreatedByUserId = actor, SaleType = type,
        SalesAttributionSnapshot = AssignmentType.House, SalesEmployeeId = null, SaleCategory = null,
        GrossAmount = price, NetAmount = price, Status = SaleStatus.Open, CreatedAt = access.Now
    };
}
