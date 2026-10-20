using CampionsSystem.BLL.CheckIns;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class CheckInTests
{
    private async Task<GuestInvitation> IssuedTrial(string phone="+201000000099",GuestInvitationStatus status=GuestInvitationStatus.Available)
    {
        var i=new GuestInvitation{BranchId=b.Id,InviteeName="Invited guest",InviteePhone=phone,InviteePhoneNormalized=phone,
            CreatedAt=clock.Now.AddDays(-1),CreatedByUserId=actor.Id,Status=status,CancelledAt=status==GuestInvitationStatus.Cancelled?clock.Now:null};
        db.Add(i);await db.SaveChangesAsync();return i;
    }
    [Fact] public async Task TrialRedemptionAndRetryAreOneTimeHistoricalAndNonFinancial()
    {
        var i=await IssuedTrial();var op=Guid.NewGuid();var r=new RedeemTrialRequest(op,i.Id,"+20 100 000 0099");
        var first=await service.RedeemTrialAsync(r);Assert.True(first.Allowed);Assert.Equal("Trial",first.CheckIn!.Type);Assert.Null(first.CheckIn.MemberId);Assert.Null(first.CheckIn.ActorName);
        Assert.False(first.CheckIn.SessionConsumed);Assert.Null(first.CheckIn.SubscriptionId);Assert.Equal(b.Id,first.CheckIn.BranchId);Assert.Equal(today,first.CheckIn.BusinessDate);
        var retry=await service.RedeemTrialAsync(r);Assert.Equal(first.CheckIn.Id,retry.CheckIn!.Id);Assert.Null(retry.CheckIn.ActorName);Assert.Single(await db.CheckIns.ToListAsync());
        Assert.Equal(GuestInvitationStatus.Redeemed,i.Status);Assert.Equal(first.CheckIn.Id,i.RedeemedCheckInId);
        var other=await IssuedTrial();Assert.Equal(TrialReasons.TrialAlreadyUsed,(await service.RedeemTrialAsync(new(Guid.NewGuid(),other.Id,other.InviteePhone))).ReasonCode);
        Assert.Empty(await db.SaleTransactions.ToListAsync());Assert.Empty(await db.Payments.ToListAsync());Assert.Empty(await db.FinancialTransactions.ToListAsync());
        Assert.Single(await db.Members.ToListAsync());
    }
    [Fact] public async Task TrialUsesExistingMemberIdentityWithoutChangingRegistrationOrOwnership()
    {
        member.PrimaryPhone="+201000000099";member.PrimaryPhoneNormalized=member.PrimaryPhone;await db.SaveChangesAsync();
        var i=await IssuedTrial();i.Source=GuestInvitationSource.Member;i.InvitedByMemberId=member.Id;await db.SaveChangesAsync();
        var result=await service.RedeemTrialAsync(new(Guid.NewGuid(),i.Id,i.InviteePhone));Assert.True(result.Allowed);Assert.Equal(member.Id,result.CheckIn!.MemberId);
        Assert.Equal(member.Id,i.InviteeMemberId);Assert.Equal(a.Id,member.HomeBranchId);Assert.Empty(await db.MemberSalesAssignments.ToListAsync());
        Assert.Equal(result.CheckIn.Id,(await service.GetHistoryAsync(new CheckInHistoryRequest{Type=CheckInType.Trial})).Items.Single().Id);
        Assert.Null(await service.GetCurrentGymEntryAsync(member.Id));
    }
    [Theory][InlineData(GuestInvitationStatus.Cancelled)][InlineData(GuestInvitationStatus.Expired)]
    public async Task TrialUnavailableStatesDoNotCreateEntrance(GuestInvitationStatus status)
    {var i=await IssuedTrial(status:status);Assert.Equal(TrialReasons.InvitationUnavailable,(await service.RedeemTrialAsync(new(Guid.NewGuid(),i.Id,i.InviteePhone))).ReasonCode);Assert.Empty(await db.CheckIns.ToListAsync());}
    [Fact] public async Task TrialIdentityAndSelectedBranchMustMatchIssuedRecord()
    {
        var i=await IssuedTrial();Assert.Equal(TrialReasons.InvitationIdentityMismatch,(await service.RedeemTrialAsync(new(Guid.NewGuid(),i.Id,"+201000000088"))).ReasonCode);
        Assert.Equal(TrialReasons.InvitationBranchMismatch,(await service.RedeemTrialAsync(new(Guid.NewGuid(),i.Id,i.InviteePhone,a.Id))).ReasonCode);
        i.InviteeMemberId=member.Id;await db.SaveChangesAsync();Assert.Equal(TrialReasons.InvitationIdentityMismatch,(await service.RedeemTrialAsync(new(Guid.NewGuid(),i.Id,i.InviteePhone))).ReasonCode);
        Assert.Empty(await db.CheckIns.ToListAsync());
    }
    [Fact] public async Task TrialRejectsInactiveKnownMemberAndClosureRegardlessOfCompensation()
    {
        member.PrimaryPhone="+201000000099";member.PrimaryPhoneNormalized=member.PrimaryPhone;member.IsActive=false;await db.SaveChangesAsync();var i=await IssuedTrial();
        Assert.Equal(AccessReasons.MemberInactive,(await service.RedeemTrialAsync(new(Guid.NewGuid(),i.Id,i.InviteePhone))).ReasonCode);
        member.IsActive=true;db.Add(new BranchClosure{BranchId=b.Id,StartDate=today,EndDate=today,AppliesToGym=true,Reason="Closed",ExtensionDays=0,CreatedByUserId=actor.Id});await db.SaveChangesAsync();
        Assert.Equal(AccessReasons.BranchClosed,(await service.RedeemTrialAsync(new(Guid.NewGuid(),i.Id,i.InviteePhone))).ReasonCode);Assert.Empty(await db.CheckIns.ToListAsync());
    }
    [Fact] public async Task TrialOperationCannotBeReboundToAnotherInvitation()
    {
        var first=await IssuedTrial();var second=await IssuedTrial("+201000000088");var op=Guid.NewGuid();await service.RedeemTrialAsync(new(op,first.Id,first.InviteePhone));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>service.RedeemTrialAsync(new(op,second.Id,second.InviteePhone)));
        Assert.Single(await db.CheckIns.ToListAsync());
    }
}
public sealed partial class CheckInTests
{
    private async Task<MemberSubscriptionBenefit> GuestBenefit(int quantity=1)
    {
        var benefit=new MembershipBenefit{Code="GUEST",Name="Guest invitations"};
        db.Add(new MembershipPlanBenefit{MembershipPlanId=plan.Id,MembershipBenefit=benefit,Quantity=quantity});await db.SaveChangesAsync();
        await subscriptions.CreateCustomerGrantAsync(new(member.Id,plan.Id,a.Id,Guid.NewGuid()));
        return await db.MemberSubscriptionBenefits.SingleAsync();
    }
    [Fact] public async Task MemberInvitationConsumesExactlyOneSnapshotUnitOnlyAtRedemption()
    {
        var benefit=await GuestBenefit();var first=await IssuedTrial();first.Source=GuestInvitationSource.Member;first.InvitedByMemberId=member.Id;first.SourceSubscriptionBenefitId=benefit.Id;await db.SaveChangesAsync();
        var r=new RedeemTrialRequest(Guid.NewGuid(),first.Id,first.InviteePhone);Assert.True((await service.RedeemTrialAsync(r)).Allowed);Assert.True((await service.RedeemTrialAsync(r)).Allowed);
        var usage=await db.MemberBenefitUsages.SingleAsync();Assert.Equal(1,usage.QuantityUsed);Assert.Equal(benefit.Id,usage.MemberSubscriptionBenefitId);Assert.Equal(b.Id,usage.BranchId);Assert.Contains(first.Id.ToString(),usage.Notes);
        var second=await IssuedTrial("+201000000088");second.Source=GuestInvitationSource.Member;second.InvitedByMemberId=member.Id;second.SourceSubscriptionBenefitId=benefit.Id;await db.SaveChangesAsync();
        Assert.Equal(TrialReasons.InvitationBenefitExhausted,(await service.RedeemTrialAsync(new(Guid.NewGuid(),second.Id,second.InviteePhone))).ReasonCode);
        Assert.Single(await db.CheckIns.ToListAsync());Assert.Equal(1,benefit.GrantedQuantity);
        usage.QuantityUsed=2;await Assert.ThrowsAsync<InvalidOperationException>(()=>db.SaveChangesAsync());
    }
    [Fact] public async Task NonGuestBenefitCannotBeRedeemedAsInvitation()
    {
        var benefit=await GuestBenefit();await db.MemberSubscriptionBenefits.Where(x=>x.Id==benefit.Id).ExecuteUpdateAsync(x=>x.SetProperty(y=>y.BenefitCodeSnapshot,"OTHER"));
        var i=await IssuedTrial();i.Source=GuestInvitationSource.Member;i.InvitedByMemberId=member.Id;i.SourceSubscriptionBenefitId=benefit.Id;await db.SaveChangesAsync();
        Assert.Equal(TrialReasons.InvitationBenefitInvalid,(await service.RedeemTrialAsync(new(Guid.NewGuid(),i.Id,i.InviteePhone))).ReasonCode);
        Assert.Empty(await db.MemberBenefitUsages.ToListAsync());Assert.Empty(await db.CheckIns.ToListAsync());
    }
}
public sealed partial class CheckInConcurrencyTests
{
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ConcurrentTrialRedemptionsConsumePersonOnce(bool sameOperation)
    {
        var id=Guid.NewGuid();await using(var db=Context())
        {db.Add(new GuestInvitation{Id=id,BranchId=branch,InviteeName="Guest",InviteePhone="+201000000099",InviteePhoneNormalized="+201000000099",CreatedAt=new(2026,10,9,10,0,0,TimeSpan.Zero)});await db.SaveChangesAsync();}
        async Task<TrialCheckInResult> Redeem(Guid op){await using var db=Context();return await Services(db).CheckIns.RedeemTrialAsync(new(op,id,"+201000000099"));}
        var op=Guid.NewGuid();var results=await Task.WhenAll(Redeem(op),Redeem(sameOperation?op:Guid.NewGuid()));
        Assert.Equal(sameOperation?2:1,results.Count(x=>x.Allowed));await using var verify=Context();Assert.Single(await verify.CheckIns.ToListAsync());
        Assert.Single(await verify.GuestInvitations.Where(x=>x.Status==GuestInvitationStatus.Redeemed).ToListAsync());
    }
}
public sealed partial class CheckInConcurrencyTests
{
    [Fact] public async Task ConcurrentInvitationsCannotConsumeSameFinalGuestBenefitUnit()
    {
        Guid benefitId;var ids=new[]{Guid.NewGuid(),Guid.NewGuid()};var phones=new[]{"+201000000099","+201000000088"};
        await using(var db=Context())
        {
            var grant=await db.MembershipSubscriptionTransactions.SingleAsync();var benefit=new MemberSubscriptionBenefit{MembershipSubscriptionTransactionId=grant.Id,
                MembershipBenefit=new(){Code="GUEST",Name="Guests"},BenefitCodeSnapshot="GUEST",BenefitNameSnapshot="Guests",GrantedQuantity=1};benefitId=benefit.Id;db.Add(benefit);
            for(var n=0;n<2;n++)db.Add(new GuestInvitation{Id=ids[n],BranchId=branch,Source=GuestInvitationSource.Member,InvitedByMemberId=member,SourceSubscriptionBenefitId=benefitId,
                InviteeName="Guest",InviteePhone=phones[n],InviteePhoneNormalized=phones[n],CreatedAt=new(2026,10,9,10,0,0,TimeSpan.Zero)});
            await db.SaveChangesAsync();
        }
        async Task<TrialCheckInResult> Redeem(int n){await using var db=Context();return await Services(db).CheckIns.RedeemTrialAsync(new(Guid.NewGuid(),ids[n],phones[n]));}
        var results=await Task.WhenAll(Redeem(0),Redeem(1));Assert.Single(results,x=>x.Allowed);Assert.Equal(TrialReasons.InvitationBenefitExhausted,Assert.Single(results,x=>!x.Allowed).ReasonCode);
        await using var verify=Context();Assert.Single(await verify.CheckIns.ToListAsync());Assert.Equal(1,(await verify.MemberBenefitUsages.SingleAsync()).QuantityUsed);
    }
}
public sealed partial class CheckInTests
{
    [Fact] public async Task FailedTrialInsertRollsBackBenefitAndRedemptionTogether()
    {
        var benefit=await GuestBenefit();var i=await IssuedTrial();i.Source=GuestInvitationSource.Member;i.InvitedByMemberId=member.Id;i.SourceSubscriptionBenefitId=benefit.Id;await db.SaveChangesAsync();
        user.Context=user.Context! with{UserId=Guid.NewGuid()};await Assert.ThrowsAsync<DbUpdateException>(()=>service.RedeemTrialAsync(new(Guid.NewGuid(),i.Id,i.InviteePhone)));
        Assert.Equal(GuestInvitationStatus.Available,(await db.GuestInvitations.AsNoTracking().SingleAsync()).Status);Assert.Empty(await db.MemberBenefitUsages.ToListAsync());Assert.Empty(await db.CheckIns.ToListAsync());
    }
}
public sealed partial class CheckInTests
{
    [Fact] public async Task GuestTrialRetryAfterMemberRegistrationPreservesOriginalGuestEntryIdentity()
    {
        var i=await IssuedTrial();var r=new RedeemTrialRequest(Guid.NewGuid(),i.Id,i.InviteePhone);var first=await service.RedeemTrialAsync(r);
        // The same phone later belongs to a registered Member; the original guest entry remains unlinked.
        member.PrimaryPhone=i.InviteePhone;member.PrimaryPhoneNormalized=i.InviteePhoneNormalized;await db.SaveChangesAsync();
        var retry=await service.RedeemTrialAsync(r);Assert.Equal(first.CheckIn,retry.CheckIn);Assert.Null(retry.CheckIn!.MemberId);Assert.Null(retry.CheckIn.MemberCode);Assert.Single(await db.CheckIns.ToListAsync());
    }
}
