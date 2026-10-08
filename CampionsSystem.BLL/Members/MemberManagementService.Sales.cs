using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Members;

public sealed partial class MemberManagementService
{
    private async Task RequireVisibleAsync(Guid id, string permission, CancellationToken ct)
    {
        var user = await RequireAsync(permission, ct);
        if (!await Scoped(user).AnyAsync(m => m.Id == id, ct))
            throw new BranchNotFoundException("Member not found in accessible scope.");
    }
    public async Task<SalesAssignmentDetails?> GetCurrentSalesAssignmentAsync(Guid id, CancellationToken ct = default)
    {
        await RequireVisibleAsync(id, "Members.View", ct);
        return await Sales().Where(a => a.MemberId == id).Select(a => new SalesAssignmentDetails(a.Id,
            a.SalesEmployeeId!.Value, a.SalesEmployee!.FullName, a.AssignedAt, a.EndedAt, a.Reason)).SingleOrDefaultAsync(ct);
    }
    public async Task<IReadOnlyList<SalesAssignmentHistoryItem>> GetSalesAssignmentHistoryAsync(Guid id, CancellationToken ct = default)
    {
        await RequireVisibleAsync(id, "Members.ViewSalesHistory", ct);
        return await db.MemberSalesAssignments.AsNoTracking().Where(a => a.MemberId == id)
            .OrderByDescending(a => a.AssignedAt).ThenByDescending(a => a.CreatedAt).ThenBy(a => a.Id)
            .Select(a => new SalesAssignmentHistoryItem(a.Id, a.SalesEmployeeId,
                a.SalesEmployee == null ? null : a.SalesEmployee.FullName, a.AssignmentType.ToString(),
                a.AssignedAt, a.EndedAt, a.Reason)).ToListAsync(ct);
    }
    private async Task<MemberSalesAssignment?> CurrentAssignmentAsync(Guid id, CancellationToken ct)
    {
        var assignment = await db.MemberSalesAssignments.SingleOrDefaultAsync(a => a.MemberId == id && a.EndedAt == null, ct);
        if (assignment is not null) await db.Entry(assignment).ReloadAsync(ct);
        return assignment;
    }
    private void CloseAssignment(MemberSalesAssignment assignment)
    {
        // Preserve chronological integrity even if a legacy row starts in the future.
        if (assignment.AssignedAt > access.Now) throw new BusinessRuleException("Reconcile the future-dated current assignment first.");
        assignment.EndedAt = access.Now;
    }
    public async Task<MemberDetails> AssignSalesAsync(Guid id, SalesAssignmentRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.AssignSales", ct);
        if (request.SalesEmployeeId == Guid.Empty || request.Reason?.Length > 2048)
            throw new BusinessRuleException("Sales employee and an optional reason up to 2048 characters are required.");
        return await TransactionAsync(async () =>
        {
            var member = await LockedMemberAsync(id, user, ct);
            var employee = await EligibleOwnerAsync(request.SalesEmployeeId, ct);
            var old = await CurrentAssignmentAsync(id, ct);
            if (old?.AssignmentType == AssignmentType.Sales && old.SalesEmployeeId == employee.Id)
                throw new BusinessRuleException("Member already belongs to this Sales employee.");
            var previous = old is null ? null : new { old.Id, old.SalesEmployeeId, old.AssignedAt, old.AssignmentType };
            if (old is not null)
            {
                CloseAssignment(old);
                // Release the existing partial-unique slot before inserting the successor.
                await db.SaveChangesAsync(ct);
            }
            var next = Assignment(id, employee.Id, user.UserId, request.Reason);
            db.MemberSalesAssignments.Add(next);
            access.Audit(user.UserId, member.HomeBranchId, old?.AssignmentType == AssignmentType.Sales
                ? "Member.SalesReassigned" : "Member.SalesAssigned", nameof(Member), id, previous,
                new { next.Id, next.SalesEmployeeId, next.AssignedAt, next.Reason });
            await db.SaveChangesAsync(ct);
            return await DetailsAsync(member, ct);
        }, ct);
    }
    public async Task<MemberDetails> ReturnMemberToHouseAsync(Guid id, ReturnToHouseRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.AssignSales", ct);
        if (request.Reason?.Length > 2048) throw new BusinessRuleException("Reason must not exceed 2048 characters.");
        return await TransactionAsync(async () =>
        {
            var member = await LockedMemberAsync(id, user, ct);
            var old = await CurrentAssignmentAsync(id, ct);
            if (old is null) throw new BusinessRuleException("Member is already House.");
            var before = new { old.Id, old.SalesEmployeeId, old.AssignedAt, old.AssignmentType };
            CloseAssignment(old);
            access.Audit(user.UserId, member.HomeBranchId, "Member.ReturnedToHouse", nameof(Member), id, before,
                new { old.EndedAt, Reason = request.Reason?.Trim() });
            await db.SaveChangesAsync(ct);
            return await DetailsAsync(member, ct);
        }, ct);
    }
}
