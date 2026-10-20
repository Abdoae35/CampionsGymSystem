using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.CheckIns;
public sealed partial class CheckInService
{
    // Existing issued invitation identity + canonical phone; no new token, issuance or Member-registration workflow.
    public async Task<TrialCheckInResult> RedeemTrialAsync(RedeemTrialRequest r,CancellationToken ct=default)
    {
        if(r.OperationId==Guid.Empty || r.InvitationId==Guid.Empty)throw new BusinessRuleException("Invitation and operation identities are required.");
        var context=await ContextAsync(CheckInPermissions.Create,r.BranchId,ct);
        return await TransactionAsync(async ()=>
        {
            var branch=await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={context.Branch.Id} FOR SHARE")
                .AsNoTracking().Include(x=>x.City).SingleAsync(ct);
            var day=await db.BusinessDays.FromSqlInterpolated($"SELECT * FROM \"BusinessDays\" WHERE \"BranchId\"={branch.Id} AND \"Status\"='Open' FOR SHARE")
                .AsNoTracking().SingleOrDefaultAsync(ct);
            if(day!=null && (day.OpenedAt>access.Now || day.BusinessDate>access.Today))throw new BusinessRuleException("Invalid open BusinessDay.");
            var phone=PhoneNormalizer.Normalize(r.Phone,branch.CountryCallingCode);
            await PhoneIdentity.LockAsync(db,phone,ct);
            var memberId=await PhoneIdentity.FindMemberAsync(db,phone,ct);
            var issued=await db.GuestInvitations.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==r.InvitationId,ct);
            var identities=new[]{memberId,issued?.InvitedByMemberId}.Where(x=>x.HasValue).Select(x=>x!.Value).Distinct().Order().ToArray();
            Member? member=null;
            foreach(var identity in identities){var locked=await LockMemberAsync(identity,ct);if(identity==memberId)member=locked;}
            var invitation=await db.GuestInvitations.FromSqlInterpolated($"SELECT * FROM \"GuestInvitations\" WHERE \"Id\"={r.InvitationId} FOR UPDATE").SingleOrDefaultAsync(ct);
            TrialCheckInResult Denied(string reason)=>new(false,reason,r.InvitationId,null);
            if(invitation==null)return Denied(TrialReasons.InvitationNotFound);
            await db.Entry(invitation).ReloadAsync(ct);
            if(invitation.InvitedByMemberId!=issued?.InvitedByMemberId)throw new BusinessRuleException("Invitation changed; reload before redemption.");
            if(invitation.BranchId!=branch.Id)return Denied(TrialReasons.InvitationBranchMismatch);
            if(invitation.InviteePhoneNormalized!=phone || PhoneNormalizer.Normalize(invitation.InviteePhone,branch.CountryCallingCode)!=phone
                || invitation.InviteeMemberId.HasValue && invitation.InviteeMemberId!=memberId)
                return Denied(TrialReasons.InvitationIdentityMismatch);
            var retry=await db.CheckIns.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
            if(retry!=null)
            {
                if(invitation.RedeemedCheckInId!=retry.Id || retry.CheckInType!=CheckInType.Trial || retry.BranchId!=branch.Id || retry.CreatedByUserId!=context.Actor.UserId)
                    throw new BusinessRuleException("Operation identity belongs to a different Check-In request.");
                return new(true,AccessReasons.Allowed,invitation.Id,(await DetailsWithActorAsync(retry,member,ct)) with{MemberName=invitation.InviteeName});
            }
            if(!branch.IsActive || !branch.City.IsActive)return Denied(AccessReasons.BranchInactive);
            var date=day?.BusinessDate??access.Today;
            if(await db.BranchClosures.AnyAsync(x=>x.BranchId==branch.Id && x.AppliesToGym && x.StartDate<=date && x.EndDate>=date,ct))return Denied(AccessReasons.BranchClosed);
            if(member is {IsActive:false})return Denied(AccessReasons.MemberInactive);
            if(invitation.Status!=GuestInvitationStatus.Available || invitation.CreatedAt>access.Now)return Denied(TrialReasons.InvitationUnavailable);
            if(await db.GuestInvitations.AnyAsync(x=>x.Status==GuestInvitationStatus.Redeemed && (x.InviteePhoneNormalized==phone || memberId!=null && x.InviteeMemberId==memberId),ct))
                return Denied(TrialReasons.TrialAlreadyUsed);
            MemberBenefitUsage? usage=null;
            if(invitation.SourceSubscriptionBenefitId is Guid benefitId)
            {
                var benefit=await db.MemberSubscriptionBenefits.FromSqlInterpolated($"SELECT * FROM \"MemberSubscriptionBenefits\" WHERE \"Id\"={benefitId} FOR UPDATE")
                    .AsNoTracking().Include(x=>x.MembershipSubscriptionTransaction).ThenInclude(x=>x.MemberSubscription).SingleAsync(ct);
                if(invitation.Source!=GuestInvitationSource.Member || benefit.MembershipSubscriptionTransaction.MemberSubscription.MemberId!=invitation.InvitedByMemberId
                    || benefit.BenefitCodeSnapshot!="GUEST" || benefit.GrantTypeSnapshot!=MembershipBenefitGrantType.Quantity
                    || benefit.MembershipSubscriptionTransaction.MemberSubscription.Status==SubscriptionStatus.Cancelled
                    || await db.SubscriptionGrantCancellations.AnyAsync(c=>c.GrantId==benefit.MembershipSubscriptionTransactionId,ct))
                    return Denied(TrialReasons.InvitationBenefitInvalid);
                var consumed=await db.MemberBenefitUsages.Where(x=>x.MemberSubscriptionBenefitId==benefitId).SumAsync(x=>(long)x.QuantityUsed,ct);
                if(benefit.GrantedQuantity is null || consumed>=benefit.GrantedQuantity)return Denied(TrialReasons.InvitationBenefitExhausted);
                usage=new(){MemberSubscriptionBenefitId=benefitId,QuantityUsed=1,BranchId=branch.Id,UsedAt=access.Now,CreatedAt=access.Now,CreatedByUserId=context.Actor.UserId,
                    Notes=$"Guest invitation {invitation.Id} redeemed"};db.Add(usage);
            }
            var row=new CheckIn{OperationId=r.OperationId,MemberId=memberId,BranchId=branch.Id,BusinessDate=date,BusinessDayId=day?.Id,
                CheckInType=CheckInType.Trial,CheckInMethod=CheckInMethod.Manual,CheckedInAt=access.Now,CreatedByUserId=context.Actor.UserId};
            db.Add(row);invitation.InviteeMemberId=memberId;invitation.Status=GuestInvitationStatus.Redeemed;invitation.RedeemedAt=access.Now;invitation.RedeemedCheckIn=row;
            access.Audit(context.Actor.UserId,branch.Id,"TrialCheckIn.Created",nameof(CheckIn),row.Id,null,
                new{InvitationId=invitation.Id,BenefitUsageId=usage?.Id,row.MemberId,row.BusinessDate,row.BusinessDayId,row.CheckInType,row.CheckInMethod,row.OperationId});
            await db.SaveChangesAsync(ct);
            return new(true,AccessReasons.Allowed,invitation.Id,(await DetailsWithActorAsync(row,member,ct)) with{MemberName=invitation.InviteeName});
        },ct);
    }
}
