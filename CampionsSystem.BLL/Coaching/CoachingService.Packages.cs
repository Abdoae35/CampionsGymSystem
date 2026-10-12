using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Coaching;

public sealed partial class CoachingService
{
    public Task<CoachingPackageDetails> SavePackageAsync(Guid? id, CoachingPackageRequest r, bool classes, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            var actor = await access.RequireAsync(classes ? CoachingPermissions.ClassesManage : CoachingPermissions.PrivateManage, r.BranchId, ct);
            FinancialOperations.Money(r.Price, true);
            if (string.IsNullOrWhiteSpace(r.Name) || r.Name.Trim().Length > 256 || r.SessionCount is < 1 or > 100000 || r.DurationDays is < 1 or > 36500)
                throw new BusinessRuleException("Invalid package name, session count or duration.");
            if (!await db.GymBranches.AnyAsync(b => b.Id == r.BranchId && b.IsActive && b.City.IsActive, ct)) throw new BusinessRuleException("Active branch/city required.");
            Guid key;
            if (classes)
            {
                var p = id == null ? new ClassesPackage { BranchId = r.BranchId } : await db.ClassesPackages.SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new BranchNotFoundException("Package not found.");
                if (p.BranchId != r.BranchId) throw new UnauthorizedAccessException();
                p.Name = r.Name.Trim(); p.Price = r.Price; p.SessionCount = r.SessionCount; p.DurationDays = r.DurationDays; p.IsActive = r.IsActive;
                if (id == null) db.Add(p); key = p.Id;
            }
            else
            {
                var p = id == null ? new PrivatePackage { BranchId = r.BranchId } : await db.PrivatePackages.SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new BranchNotFoundException("Package not found.");
                if (p.BranchId != r.BranchId) throw new UnauthorizedAccessException();
                p.Name = r.Name.Trim(); p.Price = r.Price; p.SessionCount = r.SessionCount; p.DurationDays = r.DurationDays; p.IsActive = r.IsActive;
                if (id == null) db.Add(p); key = p.Id;
            }
            access.Audit(actor, r.BranchId, classes ? "Classes.PackageConfigured" : "Private.PackageConfigured", classes ? nameof(ClassesPackage) : nameof(PrivatePackage), key, after: r);
            return new CoachingPackageDetails(key, r.BranchId, r.Name.Trim(), r.SessionCount, r.DurationDays, r.Price, r.IsActive);
        }, ct);
    public async Task<CoachingPage<CoachingPackageDetails>> PackagesAsync(Guid branch, bool classes, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(classes ? CoachingPermissions.ClassesView : CoachingPermissions.PrivateView, branch, ct);
        IQueryable<CoachingPackageDetails> q = classes
            ? db.ClassesPackages.AsNoTracking().Where(p => p.BranchId == branch).OrderBy(p => p.Name).ThenBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(p => new CoachingPackageDetails(p.Id, p.BranchId, p.Name, p.SessionCount, p.DurationDays, p.Price, p.IsActive))
            : db.PrivatePackages.AsNoTracking().Where(p => p.BranchId == branch).OrderBy(p => p.Name).ThenBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(p => new CoachingPackageDetails(p.Id, p.BranchId, p.Name, p.SessionCount, p.DurationDays, p.Price, p.IsActive));
        var total = classes ? await db.ClassesPackages.CountAsync(p => p.BranchId == branch, ct) : await db.PrivatePackages.CountAsync(p => p.BranchId == branch, ct);
        return new(await q.ToListAsync(ct), total, page, pageSize);
    }
    public async Task<CoachingPage<PrivateDetails>> PrivateSubscriptionsAsync(Guid branch, Guid member, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.PrivateView, branch, ct);
        var q = db.PrivateSubscriptions.AsNoTracking().Where(s => s.BranchId == branch && s.MemberId == member); var total = await q.CountAsync(ct);
        var ids = await q.OrderByDescending(s => s.CreatedAt).ThenBy(s => s.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(s => s.Id).ToListAsync(ct);
        var rows = new List<PrivateDetails>(); foreach (var id in ids) rows.Add(await PrivateDetailsAsync(id, ct)); return new(rows, total, page, pageSize);
    }
    public async Task<CoachingPage<PrivateExpiryDetails>> ExpiryHistoryAsync(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); var branch = await db.PrivateSubscriptions.Where(s => s.Id == id).Select(s => (Guid?)s.BranchId).SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Subscription not found.");
        await access.RequireAsync(CoachingPermissions.PrivateView, branch, ct);
        var q = db.PrivateExpirySettlements.AsNoTracking().Where(s => s.PrivateSubscriptionId == id); var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(s => s.SettledAt).ThenBy(s => s.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct); return new(rows.Select(ExpiryDetails).ToArray(), total, page, pageSize);
    }
}
