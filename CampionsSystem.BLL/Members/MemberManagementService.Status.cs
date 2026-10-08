using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Members;

public sealed partial class MemberManagementService
{
    public async Task<MemberDetails> ChangeMemberStatusAsync(Guid id, ChangeMemberStatusRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.ChangeStatus", ct);
        if (!request.IsActive.HasValue || request.Reason?.Length > 2048)
            throw new BusinessRuleException("An active/inactive status and optional reason up to 2048 characters are required.");
        return await TransactionAsync(async () =>
        {
            var member = await LockedMemberAsync(id, user, ct);
            if (member.IsActive == request.IsActive) throw new BusinessRuleException("Member already has this status.");
            var before = new { member.IsActive };
            member.IsActive = request.IsActive.Value;
            access.Audit(user.UserId, member.HomeBranchId, "Member.StatusChanged", nameof(Member), id,
                before, new { member.IsActive, Reason = request.Reason?.Trim() });
            await db.SaveChangesAsync(ct);
            return await DetailsAsync(member, ct);
        }, ct);
    }
}
