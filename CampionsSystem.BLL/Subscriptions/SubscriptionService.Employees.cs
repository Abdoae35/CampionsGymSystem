using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Subscriptions;
public sealed partial class SubscriptionService
{
    public async Task<SubscriptionGrantResult> StartEmployeeEntitlementAsync(Guid employeeId, EmployeeEntitlementRequest r, CancellationToken ct = default)
    {
        var actor = await RequireAsync(SubscriptionPermissions.Manage, null, false, ct);
        await RequireAsync("Employees.Manage", null, false, ct);
        return await EmployeeAssignmentTransaction.RunAsync(db, employeeId, async locked =>
        {
            var branch = await new EmployeeBranchResolver(db).ResolveAsync(employeeId, access.Today, ct)
                ?? throw new BusinessRuleException("Employee requires one current active branch assignment.");
            BranchAccessService.EnsureScope(actor, branch.Id);
            var plan = await LockPlanAsync(r.PlanId, branch.Id, ct);
            var member = await LockMemberAsync(r.MemberId, ct);
            var retry = await RetryGrantAsync(r.OperationId, r.MemberId, r.PlanId, branch.Id, null, ct);
            if (retry is not null)
            {
                if (!await db.MemberSubscriptions.AnyAsync(x => x.Id == retry.SubscriptionId && x.EmployeeId == employeeId, ct))
                    throw new BusinessRuleException("Operation identity belongs to another Employee.");
                return retry;
            }
            if (locked.EmploymentStatus != EmploymentStatus.Active || locked.HireDate > access.Today || locked.TerminationDate < access.Today
                || !member.IsActive || plan.PlanCategory != PlanCategory.Employee || plan.Price != 0 || plan.AccessMode != AccessMode.Unlimited
                || plan.DurationDays != 0 || plan.SessionCount != null || locked.MemberId != null && locked.MemberId != member.Id)
                throw new BusinessRuleException("Active linked Employee/Member and free Unlimited Employee plan required.");
            if (await db.Employees.AnyAsync(x => x.Id != employeeId && x.MemberId == member.Id, ct))
                throw new BusinessRuleException("Member is linked to another Employee.");
            if (await db.MemberSubscriptions.AnyAsync(x => (x.EmployeeId == employeeId || x.MemberId == member.Id)
                && x.Status != SubscriptionStatus.Cancelled && (x.EndDate == null || x.EndDate >= access.Today), ct))
                throw new BusinessRuleException("An existing entitlement period must finish before a new Employee period can start.");
            var employee = await db.Employees.SingleAsync(x => x.Id == employeeId, ct); await db.Entry(employee).ReloadAsync(ct);
            if (employee.MemberId == null)
            {
                employee.MemberId = member.Id;
                access.Audit(actor.UserId, branch.Id, "Employee.MemberLinked", nameof(Employee), employeeId, after: new { member.Id });
            }
            var sub = new MemberSubscription { EmployeeId=employeeId, MemberId=member.Id, MembershipPlanId=plan.Id,
                PurchasedAtBranchId=branch.Id, CityId=plan.CityId, AccessMode=AccessMode.Unlimited,
                StartDate=access.Today, Status=SubscriptionStatus.Active, CreatedAt=access.Now };
            db.Add(sub);
            var grant = await SnapshotAsync(sub,plan,r.OperationId,actor.UserId,null,sub.StartDate,SubscriptionTransactionType.Initial,null,null,null,ct);
            access.Audit(actor.UserId, branch.Id, "Subscription.EmployeeEntitlementStarted", nameof(MemberSubscription), sub.Id,
                after: new { sub.MemberId, employeeId, grant.Id, sub.StartDate, sub.CityId });
            await db.SaveChangesAsync(ct); return new SubscriptionGrantResult(sub.Id,grant.Id,false);
        }, ct);
    }
    // Invoked only inside the existing authorized employment-status transaction and Employee lock.
    internal async Task CloseEmployeeEntitlementsAsync(Guid employeeId, Guid actorId, string? reason, CancellationToken ct)
    {
        var memberIds = await db.MemberSubscriptions.Where(x => x.EmployeeId == employeeId && x.EndDate == null)
            .Select(x => x.MemberId).Distinct().OrderBy(x => x).ToListAsync(ct);
        foreach (var memberId in memberIds)
        {
            await LockMemberAsync(memberId, ct);
            var periods = await db.MemberSubscriptions.Where(x => x.EmployeeId == employeeId && x.MemberId == memberId && x.EndDate == null).ToListAsync(ct);
            foreach (var sub in periods)
            {
                await db.Entry(sub).ReloadAsync(ct);
                sub.EndDate = access.Today < sub.StartDate ? sub.StartDate : access.Today;
                sub.Status = access.Today < sub.StartDate ? SubscriptionStatus.Cancelled : SubscriptionStatus.Expired;
                sub.Version++;
                access.Audit(actorId,sub.PurchasedAtBranchId,"Subscription.EmployeeEntitlementEnded",nameof(MemberSubscription),sub.Id,
                    new { EndDate=(DateOnly?)null },new { sub.MemberId,employeeId,sub.EndDate,sub.Status,Reason=reason });
            }
        }
    }
}
