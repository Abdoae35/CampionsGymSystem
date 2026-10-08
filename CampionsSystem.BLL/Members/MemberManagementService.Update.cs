using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Members;

public sealed partial class MemberManagementService
{
    private async Task<Member> LockedMemberAsync(Guid id, CurrentUserContext user, CancellationToken ct)
    {
        var rows = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\" = {id} FOR UPDATE")
            .AsNoTracking().ToListAsync(ct);
        var snapshot = rows.SingleOrDefault() ?? throw new BranchNotFoundException("Member not found.");
        BranchAccessService.EnsureScope(user, snapshot.HomeBranchId);
        var existing = db.ChangeTracker.Entries<Member>().SingleOrDefault(e => e.Entity.Id == id);
        if (existing is not null) { await existing.ReloadAsync(ct); return existing.Entity; }
        db.Attach(snapshot); return snapshot;
    }
    private static object ProfileSnapshot(Member m) => new
    {
        m.FullName, m.PrimaryPhone, m.PrimaryPhoneNormalized, m.SecondaryPhone, m.DateOfBirth, m.Gender, m.Notes
    };
    public async Task<MemberDetails> UpdateMemberAsync(Guid id, UpdateMemberRequest request, CancellationToken ct = default)
    {
        var user = await RequireAsync("Members.Edit", ct);
        ValidateProfile(request);
        return await TransactionAsync(async () =>
        {
            var member = await LockedMemberAsync(id, user, ct);
            var before = ProfileSnapshot(member);
            if (request.PrimaryPhone.Trim() != member.PrimaryPhone)
            {
                if (!user.Permissions.Contains("Members.ChangePrimaryPhone")) throw new UnauthorizedAccessException();
                var country = await db.GymBranches.Where(b => b.Id == member.HomeBranchId).Select(b => b.CountryCallingCode).SingleAsync(ct);
                var normalized = PhoneNormalizer.Normalize(request.PrimaryPhone, country);
                await PhoneIdentity.LockAsync(db,normalized,ct);
                var leadId = await PhoneIdentity.FindLeadAsync(db,normalized,ct);
                if (leadId.HasValue && !await db.Leads.AnyAsync(l=>l.Id==leadId && l.ConvertedMemberId==id,ct))
                    throw new BusinessRuleException("An existing Lead uses this phone; reconcile through CRM first.");
                await EnsurePhoneAvailableAsync(request.PrimaryPhone.Trim(), normalized, member.HomeBranchId, id, ct);
                var phoneBefore = new { member.PrimaryPhone, member.PrimaryPhoneNormalized, member.IsPrimaryPhoneVerified, member.PrimaryPhoneVerifiedAt };
                if (normalized != member.PrimaryPhoneNormalized)
                {
                    member.IsPrimaryPhoneVerified = false; member.PrimaryPhoneVerifiedAt = null;
                }
                member.PrimaryPhoneNormalized = normalized; member.PrimaryPhone = request.PrimaryPhone.Trim();
                access.Audit(user.UserId, member.HomeBranchId, "Member.PrimaryPhoneChanged", nameof(Member), id, phoneBefore,
                    new { member.PrimaryPhone, member.PrimaryPhoneNormalized, member.IsPrimaryPhoneVerified, member.PrimaryPhoneVerifiedAt });
            }
            Profile(member, request);
            access.Audit(user.UserId, member.HomeBranchId, "Member.Updated", nameof(Member), id, before, ProfileSnapshot(member));
            await db.SaveChangesAsync(ct);
            return await DetailsAsync(member, ct);
        }, ct);
    }
}
