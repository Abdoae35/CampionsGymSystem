using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

public sealed class GuestInvitationTests : IAsyncLifetime
{
    private AuthTestDatabase fixture = null!;
    private ApplicationDbContext db = null!;
    private GymBranch branch = null!;
    private ApplicationUser user = null!;
    public async Task InitializeAsync()
    {
        fixture = await AuthTestDatabase.CreateAsync(); db = fixture.CreateContext();
        branch = new GymBranch { Name = "Trial branch", City = new City { Name = "Trial city" } };
        user = new ApplicationUser { UserName = "trial-tests" };
        db.AddRange(branch, user); await db.SaveChangesAsync();
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await fixture.DisposeAsync(); }
    private Member Person() => new() { MemberCode = Guid.NewGuid().ToString(), FullName = "Member",
        PrimaryPhone = Guid.NewGuid().ToString(), HomeBranchId = branch.Id, CreatedByUserId = user.Id };
    private GuestInvitation Invitation(string phone = "+201000000001", GuestInvitationStatus status = GuestInvitationStatus.Available)
    {
        var now = DateTimeOffset.UtcNow;
        return new() { InviteeName = "Guest", InviteePhone = phone, InviteePhoneNormalized = phone,
            BranchId = branch.Id, Status = status, CreatedAt = now,
            RedeemedAt = status == GuestInvitationStatus.Redeemed ? now : null,
            CancelledAt = status == GuestInvitationStatus.Cancelled ? now : null };
    }
    [Fact] public async Task GymGuestNeedsNoMemberInviterOrFinance()
    {
        db.Add(Invitation()); await db.SaveChangesAsync();
        var row = await db.GuestInvitations.SingleAsync();
        Assert.Null(row.InviteeMemberId); Assert.Null(row.InvitedByMemberId);
        Assert.Empty(await db.Members.ToListAsync()); Assert.Empty(await db.SaleTransactions.ToListAsync());
        Assert.Empty(await db.Payments.ToListAsync()); Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Theory] [InlineData(true)] [InlineData(false)]
    public async Task MemberSourceRequiresInviter(bool supplied)
    {
        var row = Invitation(); row.Source = GuestInvitationSource.Member;
        if (supplied) row.InvitedByMember = Person();
        db.Add(row);
        if (supplied) { await db.SaveChangesAsync(); Assert.NotNull(row.InvitedByMemberId); }
        else await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task InviteeMayBeLinkedAfterIssueAndRedemptionHasGuestCheckIn()
    {
        var row = Invitation(); db.Add(row); await db.SaveChangesAsync();
        row.Status = GuestInvitationStatus.Redeemed; row.RedeemedAt = DateTimeOffset.UtcNow;
        row.RedeemedCheckIn = new CheckIn { BranchId = branch.Id, CheckInType = CheckInType.Trial,
            CheckInMethod = CheckInMethod.Manual, CheckedInAt = DateTimeOffset.UtcNow, CreatedByUserId = user.Id };
        db.Add(row.RedeemedCheckIn);
        await db.SaveChangesAsync(); Assert.Null(row.RedeemedCheckIn.MemberId);
        row.InviteeMember = Person(); db.Add(row.InviteeMember); await db.SaveChangesAsync();
        Assert.NotNull(row.InviteeMemberId); Assert.NotNull(row.RedeemedCheckInId);
    }
    [Fact] public async Task DuplicateRedeemedPhoneRejected()
    {
        db.Add(Invitation(status: GuestInvitationStatus.Redeemed)); await db.SaveChangesAsync();
        db.Add(Invitation(status: GuestInvitationStatus.Redeemed));
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
    }
    [Fact] public async Task DuplicateRedeemedMemberWithDifferentPhoneRejected()
    {
        var member = Person(); var first = Invitation(status: GuestInvitationStatus.Redeemed); first.InviteeMember = member;
        db.Add(first); await db.SaveChangesAsync();
        var second = Invitation("+201000000002", GuestInvitationStatus.Redeemed); second.InviteeMember = member; db.Add(second);
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task DifferentPeopleAndCancelledHistoryDoNotBlockTrials()
    {
        db.AddRange(Invitation(status: GuestInvitationStatus.Cancelled), Invitation(),
            Invitation(status: GuestInvitationStatus.Redeemed), Invitation("+201000000002", GuestInvitationStatus.Redeemed));
        await db.SaveChangesAsync(); Assert.Equal(2, await db.GuestInvitations.CountAsync(x => x.Status == GuestInvitationStatus.Redeemed));
    }
    [Theory] [InlineData("Delete")] [InlineData("Status")] [InlineData("Phone")]
    public async Task RedeemedHistoryCannotBeErasedEvenThroughSql(string operation)
    {
        var row = Invitation(status: GuestInvitationStatus.Redeemed); db.Add(row); await db.SaveChangesAsync();
        var sql = operation switch {
            "Delete" => """DELETE FROM "GuestInvitations" WHERE "Id" = {0}""",
            "Status" => """UPDATE "GuestInvitations" SET "Status" = 'Available', "RedeemedAt" = NULL WHERE "Id" = {0}""",
            _ => """UPDATE "GuestInvitations" SET "InviteePhoneNormalized" = '+201000000099' WHERE "Id" = {0}""" };
        await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlRawAsync(sql, row.Id));
    }
    [Fact] public async Task OptionalBenefitGrantConnectsToSpecificPurchaseBatch()
    {
        var member = Person();
        var plan = new MembershipPlan { Name = "Guest benefit plan", CityId = branch.CityId, DurationDays = 30 };
        var subscription = new MemberSubscription { Member = member, MembershipPlan = plan, CityId = branch.CityId,
            PurchasedAtBranchId = branch.Id, StartDate = new(2026,10,8), EndDate = new(2026,11,8) };
        var grant = new MemberSubscriptionBenefit { MembershipBenefit = new MembershipBenefit { Code = "GUEST", Name = "Guest Invitations" },
            BenefitCodeSnapshot = "GUEST", BenefitNameSnapshot = "Guest Invitations", GrantedQuantity = 3,
            MembershipSubscriptionTransaction = new MembershipSubscriptionTransaction { MemberSubscription = subscription,
                MembershipPlan = plan, PurchasedDurationDays = 30, NewEndDate = subscription.EndDate,
                SaleTransaction = new SaleTransaction { Member = member, BranchId = branch.Id, CreatedByUserId = user.Id, SaleType = SaleType.GymMembership } } };
        var row = Invitation(); row.Source = GuestInvitationSource.Member; row.InvitedByMember = member;
        row.SourceSubscriptionBenefit = grant; db.Add(row); await db.SaveChangesAsync();
        Assert.Equal(grant.Id, row.SourceSubscriptionBenefitId); Assert.Empty(await db.MemberBenefitUsages.ToListAsync());
        Assert.Equal(3, grant.GrantedQuantity);
    }
    [Fact] public void RelationshipsRestrictDeletesAndHaveNoFinancialForeignKeys()
    {
        var entity = db.Model.FindEntityType(typeof(GuestInvitation))!;
        Assert.All(entity.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        Assert.DoesNotContain(entity.GetProperties(), p => p.IsShadowProperty());
        Assert.DoesNotContain(entity.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(SaleTransaction) || fk.PrincipalEntityType.ClrType == typeof(Payment));
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task NonTrialStillRequiresMemberAndTrialCannotConsumePaidAccess(bool trial)
    {
        var checkIn = new CheckIn { BranchId = branch.Id, CheckInType = trial ? CheckInType.Trial : CheckInType.Gym,
            CreatedByUserId = user.Id, CheckedInAt = DateTimeOffset.UtcNow };
        if (trial) checkIn.MemberSubscriptionId = Guid.NewGuid();
        db.Add(checkIn); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact] public async Task ReferencedCheckInCannotBeDeleted()
    {
        var row = Invitation(status: GuestInvitationStatus.Redeemed);
        row.RedeemedCheckIn = new CheckIn { BranchId = branch.Id, CheckInType = CheckInType.Trial,
            CreatedByUserId = user.Id, CheckedInAt = DateTimeOffset.UtcNow };
        db.Add(row); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        db.Remove(await db.CheckIns.SingleAsync());
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

}
