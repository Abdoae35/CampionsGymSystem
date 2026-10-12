using CampionsSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.DAL.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<City> Cities => Set<City>();
    public DbSet<GymBranch> GymBranches => Set<GymBranch>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeBranchAssignment> EmployeeBranchAssignments => Set<EmployeeBranchAssignment>();
    public DbSet<EmployeeSalarySetting> EmployeeSalarySettings => Set<EmployeeSalarySetting>();
    public DbSet<GuestInvitation> GuestInvitations => Set<GuestInvitation>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberSalesAssignment> MemberSalesAssignments => Set<MemberSalesAssignment>();
    public DbSet<MemberFollowUpTask> MemberFollowUpTasks => Set<MemberFollowUpTask>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadAssignment> LeadAssignments => Set<LeadAssignment>();
    public DbSet<LeadInteraction> LeadInteractions => Set<LeadInteraction>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<MemberSubscription> MemberSubscriptions => Set<MemberSubscription>();
    public DbSet<MembershipSubscriptionTransaction> MembershipSubscriptionTransactions => Set<MembershipSubscriptionTransaction>();
    public DbSet<SubscriptionGrantCancellation> SubscriptionGrantCancellations => Set<SubscriptionGrantCancellation>();
    public DbSet<BorrowedDebtSettlementEvent> BorrowedDebtSettlementEvents => Set<BorrowedDebtSettlementEvent>();
    public DbSet<SubscriptionFreeze> SubscriptionFreezes => Set<SubscriptionFreeze>();
    public DbSet<BorrowedGymSession> BorrowedGymSessions => Set<BorrowedGymSession>();
    public DbSet<BranchClosure> BranchClosures => Set<BranchClosure>();
    public DbSet<PrivatePackage> PrivatePackages => Set<PrivatePackage>();
    public DbSet<PrivateSubscription> PrivateSubscriptions => Set<PrivateSubscription>();
    public DbSet<PrivateSubscriptionTransaction> PrivateSubscriptionTransactions => Set<PrivateSubscriptionTransaction>();
    public DbSet<PrivateCoachAssignment> PrivateCoachAssignments => Set<PrivateCoachAssignment>();
    public DbSet<PrivateExpirySettlement> PrivateExpirySettlements => Set<PrivateExpirySettlement>();
    public DbSet<PrivateSession> PrivateSessions => Set<PrivateSession>();
    public DbSet<SportsActivity> SportsActivities => Set<SportsActivity>();
    public DbSet<ActivityPackage> ActivityPackages => Set<ActivityPackage>();
    public DbSet<ActivityCoachAssignment> ActivityCoachAssignments => Set<ActivityCoachAssignment>();
    public DbSet<ActivitySchedule> ActivitySchedules => Set<ActivitySchedule>();
    public DbSet<ActivitySubscription> ActivitySubscriptions => Set<ActivitySubscription>();
    public DbSet<ActivitySubscriptionTransaction> ActivitySubscriptionTransactions => Set<ActivitySubscriptionTransaction>();
    public DbSet<GroupClass> GroupClasses => Set<GroupClass>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<SaleTransaction> SaleTransactions => Set<SaleTransaction>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentPart> PaymentParts => Set<PaymentPart>();
    public DbSet<InstallmentAgreement> InstallmentAgreements => Set<InstallmentAgreement>();
    public DbSet<Installment> Installments => Set<Installment>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<RefundPart> RefundParts => Set<RefundPart>();
    public DbSet<FinancialAccount> FinancialAccounts => Set<FinancialAccount>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftEmployee> ShiftEmployees => Set<ShiftEmployee>();
    public DbSet<CommissionPeriod> CommissionPeriods => Set<CommissionPeriod>();
    public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();
    public DbSet<CommissionCollection> CommissionCollections => Set<CommissionCollection>();
    public DbSet<CommissionRefundAllocation> CommissionRefundAllocations => Set<CommissionRefundAllocation>();
    public DbSet<CommissionStatementLine> CommissionStatementLines => Set<CommissionStatementLine>();
    public DbSet<EmployeeCommissionSummary> EmployeeCommissionSummaries => Set<EmployeeCommissionSummary>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    public DbSet<PayrollDeduction> PayrollDeductions => Set<PayrollDeduction>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<OfferMembershipPlan> OfferMembershipPlans => Set<OfferMembershipPlan>();
    public DbSet<OfferActivityPackage> OfferActivityPackages => Set<OfferActivityPackage>();
    public DbSet<MemberOfferEntitlement> MemberOfferEntitlements => Set<MemberOfferEntitlement>();
    public DbSet<FreelanceAgreement> FreelanceAgreements => Set<FreelanceAgreement>();
    public DbSet<ContractTemplate> ContractTemplates => Set<ContractTemplate>();
    public DbSet<MemberContract> MemberContracts => Set<MemberContract>();
    public DbSet<MessageLog> MessageLogs => Set<MessageLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<BranchNetworkRule> BranchNetworkRules => Set<BranchNetworkRule>();
    public DbSet<AuthorizedDevice> AuthorizedDevices => Set<AuthorizedDevice>();

    public DbSet<ActivityCoach> ActivityCoaches => Set<ActivityCoach>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<MembershipPlanEligibleMember> MembershipPlanEligibleMembers => Set<MembershipPlanEligibleMember>();
    public DbSet<ActivityCoachSettlement> ActivityCoachSettlements => Set<ActivityCoachSettlement>();

    public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();
    public DbSet<BusinessDay> BusinessDays => Set<BusinessDay>();
    public DbSet<MembershipPlanCategory> MembershipPlanCategories => Set<MembershipPlanCategory>();
    public DbSet<EmployeeLeaveRequest> EmployeeLeaveRequests => Set<EmployeeLeaveRequest>();
    public DbSet<ShiftEmployeeChange> ShiftEmployeeChanges => Set<ShiftEmployeeChange>();

    public DbSet<MembershipBenefit> MembershipBenefits => Set<MembershipBenefit>();
    public DbSet<MembershipPlanBenefit> MembershipPlanBenefits => Set<MembershipPlanBenefit>();
    public DbSet<MemberSubscriptionBenefit> MemberSubscriptionBenefits => Set<MemberSubscriptionBenefit>();
    public DbSet<MemberBenefitUsage> MemberBenefitUsages => Set<MemberBenefitUsage>();

    public DbSet<SubscriptionHolidayExtension> SubscriptionHolidayExtensions => Set<SubscriptionHolidayExtension>();

    public DbSet<SaleMembershipSnapshot> SaleMembershipSnapshots => Set<SaleMembershipSnapshot>();
    public DbSet<SaleBenefitSnapshot> SaleBenefitSnapshots => Set<SaleBenefitSnapshot>();
    public DbSet<InstallmentPaymentAllocation> InstallmentPaymentAllocations => Set<InstallmentPaymentAllocation>();

    public DbSet<MemberAudienceCategory> MemberAudienceCategories => Set<MemberAudienceCategory>();

    public DbSet<CashDrawer> CashDrawers => Set<CashDrawer>();
    public DbSet<CashCount> CashCounts => Set<CashCount>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public DbSet<EmployeeAdvance> EmployeeAdvances => Set<EmployeeAdvance>();
    public DbSet<ExpenseReversal> ExpenseReversals => Set<ExpenseReversal>();

    public DbSet<ClassesPackage> ClassesPackages => Set<ClassesPackage>();
    public DbSet<ClassesSubscription> ClassesSubscriptions => Set<ClassesSubscription>();
    public DbSet<CoachTargetRule> CoachTargetRules => Set<CoachTargetRule>();
    public DbSet<CoachEarning> CoachEarnings => Set<CoachEarning>();
    public DbSet<CoachPayout> CoachPayouts => Set<CoachPayout>();
    public DbSet<CoachPayoutAllocation> CoachPayoutAllocations => Set<CoachPayoutAllocation>();

    public DbSet<FinancialMonth> FinancialMonths => Set<FinancialMonth>();
    public DbSet<FinancialMonthEvent> FinancialMonthEvents => Set<FinancialMonthEvent>();
    public DbSet<FinancialClosingRevision> FinancialClosingRevisions => Set<FinancialClosingRevision>();
    public DbSet<PayrollPaymentRecord> PayrollPaymentRecords => Set<PayrollPaymentRecord>();
    public DbSet<MaintenancePlanRevision> MaintenancePlanRevisions => Set<MaintenancePlanRevision>();
    public DbSet<FinancialOccurrenceEvidence> FinancialOccurrenceEvidence => Set<FinancialOccurrenceEvidence>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private void EnforceAppendOnlyAudit()
    {
        if (ChangeTracker.Entries<FinancialClosingRevision>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<FinancialMonthEvent>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<PayrollPaymentRecord>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<MaintenancePlanRevision>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<FinancialOccurrenceEvidence>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<Payroll>().Any(x => x.OriginalValues.GetValue<Guid?>(nameof(Payroll.FinancialMonthId)) != null && x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<PayrollDeduction>().Any(x => x.OriginalValues.GetValue<Guid?>(nameof(PayrollDeduction.EmployeeAdvanceId)) != null && x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Monthly reports, payroll evidence and maintenance revisions are append-only.");
        foreach (var entry in ChangeTracker.Entries<FinancialMonth>())
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified && entry.Properties.Any(p => p.IsModified && p.Metadata.Name is not
                (nameof(FinancialMonth.Status) or nameof(FinancialMonth.Version) or nameof(FinancialMonth.LastRevision) or nameof(FinancialMonth.FirstClosedAt))))
                throw new InvalidOperationException("Financial month boundaries and creation facts are immutable.");
        foreach (var entry in ChangeTracker.Entries<EmployeeSalarySetting>())
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified &&
                (entry.OriginalValues.GetValue<DateOnly?>(nameof(EmployeeSalarySetting.EffectiveTo)) != null || entry.Properties.Any(p => p.IsModified && p.Metadata.Name != nameof(EmployeeSalarySetting.EffectiveTo))))
                throw new InvalidOperationException("Salary history preserves original values; close once and append a new setting.");

        if (ChangeTracker.Entries<CoachTargetRule>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<CoachEarning>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<CoachPayout>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<CoachPayoutAllocation>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<PrivateSession>().Any(x => x.OriginalValues.GetValue<Guid?>(nameof(PrivateSession.OperationId)) != null && x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<PrivateExpirySettlement>().Any(x => x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Coach earnings, payouts and Private evidence are append-only.");
        foreach (var entry in ChangeTracker.Entries<ClassSession>().Where(x => x.OriginalValues.GetValue<Guid?>(nameof(ClassSession.OperationId)) != null))
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified &&
                (entry.OriginalValues.GetValue<CampionsSystem.DAL.Enums.ClassSessionStatus>(nameof(ClassSession.Status)) != CampionsSystem.DAL.Enums.ClassSessionStatus.Scheduled
                || entry.Properties.Any(p => p.IsModified && p.Metadata.Name is not (nameof(ClassSession.Status) or nameof(ClassSession.EndedAt) or nameof(ClassSession.Version)))))
                throw new InvalidOperationException("Managed class occurrence attribution and completed history are immutable.");
        foreach (var entry in ChangeTracker.Entries<ClassesSubscription>())
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified && entry.Properties.Any(p => p.IsModified && p.Metadata.Name is not (nameof(ClassesSubscription.RemainingSessions) or nameof(ClassesSubscription.Version))))
                throw new InvalidOperationException("Classes purchased terms are immutable.");
        foreach (var entry in ChangeTracker.Entries<PrivateCoachAssignment>().Where(x => x.OriginalValues.GetValue<Guid?>(nameof(PrivateCoachAssignment.OperationId)) != null))
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified && (entry.OriginalValues.GetValue<DateTimeOffset?>(nameof(PrivateCoachAssignment.EndedAt)) != null || entry.Properties.Any(p => p.IsModified && p.Metadata.Name != nameof(PrivateCoachAssignment.EndedAt))))
                throw new InvalidOperationException("Private assignment history can only be ended once.");
        foreach (var entry in ChangeTracker.Entries<PrivateSubscriptionTransaction>())
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified && entry.Properties.Any(p => p.IsModified && p.Metadata.Name != nameof(PrivateSubscriptionTransaction.RemainingSessions)))
                throw new InvalidOperationException("Private purchase facts are immutable.");
        if (ChangeTracker.Entries<CommissionCollection>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<CommissionRefundAllocation>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<CommissionStatementLine>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<EmployeeCommissionSummary>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<CommissionRule>().Any(x => x.OriginalValues.GetValue<Guid?>(nameof(CommissionRule.OperationId)) != null && x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Commission evidence, rules and finalized statements are append-only.");
        foreach (var entry in ChangeTracker.Entries<CommissionPeriod>().Where(x => x.OriginalValues.GetValue<Guid?>(nameof(CommissionPeriod.OperationId)) != null))
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified &&
                (entry.OriginalValues.GetValue<CampionsSystem.DAL.Enums.CommissionPeriodStatus>(nameof(CommissionPeriod.Status)) == CampionsSystem.DAL.Enums.CommissionPeriodStatus.Closed
                || entry.Properties.Any(p => p.IsModified && p.Metadata.Name is not (nameof(CommissionPeriod.Status) or nameof(CommissionPeriod.ClosedAt) or nameof(CommissionPeriod.ClosedByUserId) or nameof(CommissionPeriod.CloseOperationId) or nameof(CommissionPeriod.CloseRequestHash) or nameof(CommissionPeriod.Version)))))
                throw new InvalidOperationException("Commission period boundaries and finalized periods are immutable.");
        if (ChangeTracker.Entries<CashCount>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<CashMovement>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<EmployeeAdvance>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<ExpenseReversal>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<Expense>().Any(x => x.OriginalValues.GetValue<Guid?>(nameof(Expense.OperationId)) != null && x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Managed cash observations, payouts and reversals are append-only.");
        foreach (var entry in ChangeTracker.Entries<Shift>().Where(x => x.OriginalValues.GetValue<Guid?>(nameof(Shift.OperationId)) != null))
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified &&
                (entry.OriginalValues.GetValue<CampionsSystem.DAL.Enums.ShiftStatus>(nameof(Shift.Status)) == CampionsSystem.DAL.Enums.ShiftStatus.Closed
                || entry.Properties.Any(p => p.IsModified && p.Metadata.Name is not (nameof(Shift.Status) or nameof(Shift.ClosedAt) or nameof(Shift.ClosedByUserId)
                    or nameof(Shift.CloseOperationId) or nameof(Shift.CloseRequestHash) or nameof(Shift.ExpectedClosingCash) or nameof(Shift.ActualClosingCash)
                    or nameof(Shift.Difference) or nameof(Shift.ClosingBusinessDate) or nameof(Shift.ClosingReason) or nameof(Shift.Version)))))
                throw new InvalidOperationException("Shift opening facts and closed shifts are immutable.");
        foreach (var entry in ChangeTracker.Entries<CashDrawer>())
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified && entry.Properties.Any(p => p.IsModified && p.Metadata.Name != nameof(CashDrawer.MovementSequence)))
                throw new InvalidOperationException("Drawer mapping and setup facts are immutable.");
        foreach (var entry in ChangeTracker.Entries<MemberAudienceCategory>())
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified &&
                (entry.OriginalValues.GetValue<DateTimeOffset?>(nameof(MemberAudienceCategory.EndedAt)) != null
                 || entry.Properties.Any(p => p.IsModified && p.Metadata.Name != nameof(MemberAudienceCategory.EndedAt))))
                throw new InvalidOperationException("Audience assignments preserve history; close and append a new assignment.");
        foreach (var entry in ChangeTracker.Entries<MemberOfferEntitlement>())
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified &&
                (entry.OriginalValues.GetValue<DateTimeOffset?>(nameof(MemberOfferEntitlement.RevokedAt)) != null
                 || entry.Properties.Any(p => p.IsModified && p.Metadata.Name is not (nameof(MemberOfferEntitlement.RevokedAt) or nameof(MemberOfferEntitlement.RevokedByUserId)))))
                throw new InvalidOperationException("Member Offer grants preserve original dates; revoke once instead of rewriting history.");
        foreach (var entry in ChangeTracker.Entries<SaleTransaction>().Where(x => x.OriginalValues.GetValue<Guid?>(nameof(SaleTransaction.OperationId)) != null))
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified && entry.Properties.Any(p => p.IsModified && p.Metadata.Name is not
                (nameof(SaleTransaction.Status) or nameof(SaleTransaction.Version) or nameof(SaleTransaction.CancelledAt) or nameof(SaleTransaction.CancelledByUserId) or nameof(SaleTransaction.CancellationReason))))
                throw new InvalidOperationException("Managed sale commercial facts are immutable; cancellation preserves original history.");
        if(ChangeTracker.Entries<FinancialAccount>().Any(x=>x.State==EntityState.Modified && x.Properties.Any(p=>p.IsModified && p.Metadata.Name is
            nameof(FinancialAccount.BranchId) or nameof(FinancialAccount.AccountType) or nameof(FinancialAccount.OpeningBalance))))
            throw new InvalidOperationException("Account branch, method and opening facts are immutable; record money movements in the ledger.");
        if (ChangeTracker.Entries<SaleMembershipSnapshot>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<SaleBenefitSnapshot>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<InstallmentPaymentAllocation>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<PaymentPart>().Any(x => (x.Entity.Payment is null || x.Entity.Payment.OperationId != null) && x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<RefundPart>().Any(x => (x.Entity.Refund is null || x.Entity.Refund.OperationId != null) && x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<FinancialTransaction>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<Payment>().Any(x => x.OriginalValues.GetValue<Guid?>(nameof(Payment.OperationId)) != null && x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<Refund>().Any(x => x.OriginalValues.GetValue<Guid?>(nameof(Refund.OperationId)) != null && x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Financial payments, refunds, parts, allocations and sold snapshots are append-only.");
        if (ChangeTracker.Entries<MemberBenefitUsage>().Any(x => x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Benefit usage history is append-only.");
        if (ChangeTracker.Entries<CheckIn>().Any(x => x.OriginalValues.GetValue<Guid?>(nameof(CheckIn.OperationId)) != null
            && x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Managed Check-In history is append-only.");
        foreach (var entry in ChangeTracker.Entries<BorrowedGymSession>())
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified
                && (entry.OriginalValues.GetValue<CampionsSystem.DAL.Enums.BorrowedSessionStatus>(nameof(BorrowedGymSession.Status)) != CampionsSystem.DAL.Enums.BorrowedSessionStatus.Outstanding
                    || entry.Entity.Status != CampionsSystem.DAL.Enums.BorrowedSessionStatus.Settled
                    || entry.Properties.Any(p => p.IsModified && p.Metadata.Name is not (nameof(BorrowedGymSession.Status) or nameof(BorrowedGymSession.SettledAt) or nameof(BorrowedGymSession.SettledBySubscriptionId) or nameof(BorrowedGymSession.SettledByTransactionId)))))
                throw new InvalidOperationException("Borrowed session facts are historical; only first settlement is permitted.");
        if (ChangeTracker.Entries<MembershipSubscriptionTransaction>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<SubscriptionGrantCancellation>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<BorrowedDebtSettlementEvent>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<MemberSubscriptionBenefit>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<SubscriptionHolidayExtension>().Any(x => x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Subscription grants, benefit snapshots and holiday compensation are append-only.");
        foreach (var entry in ChangeTracker.Entries<SubscriptionFreeze>())
        {
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified
                && (entry.OriginalValues.GetValue<DateTimeOffset?>(nameof(SubscriptionFreeze.CompletedAt)) != null
                    || entry.Properties.Any(p => p.IsModified && p.Metadata.Name is not (nameof(SubscriptionFreeze.CompletedAt) or nameof(SubscriptionFreeze.CompletedByUserId)))))
                throw new InvalidOperationException("Freeze/extension facts are historical; only an uncompleted freeze can be completed once.");
        }
        foreach (var entry in ChangeTracker.Entries<MemberSubscription>()
                     .Where(x => x.OriginalValues.GetValue<Guid?>(nameof(MemberSubscription.EmployeeId)) is not null))
        {
            if (entry.State == EntityState.Deleted
                || (entry.State == EntityState.Modified &&
                    (entry.OriginalValues.GetValue<DateOnly?>(nameof(MemberSubscription.EndDate)) is not null
                     || new[] { nameof(MemberSubscription.EmployeeId), nameof(MemberSubscription.MemberId),
                         nameof(MemberSubscription.MembershipPlanId), nameof(MemberSubscription.StartDate) }
                         .Any(name => entry.Property(name).IsModified))))
                throw new InvalidOperationException("Employee membership ownership and closed periods are historical records.");
        }
        foreach (var entry in ChangeTracker.Entries<ActivitySubscriptionTransaction>().Where(x => x.State == EntityState.Modified))
            if (new[] { nameof(ActivitySubscriptionTransaction.ActivityCoachIdSnapshot), nameof(ActivitySubscriptionTransaction.CoachPercentageSnapshot),
                    nameof(ActivitySubscriptionTransaction.ActivityPackageId), nameof(ActivitySubscriptionTransaction.SaleTransactionId),
                    nameof(ActivitySubscriptionTransaction.ActivitySubscriptionId) }.Any(name => entry.Property(name).IsModified))
                throw new InvalidOperationException("Activity purchase coach/rate attribution is immutable; reconcile legacy data explicitly.");
        foreach (var entry in ChangeTracker.Entries<ActivityCoach>().Where(x => x.State == EntityState.Modified))
            if (entry.Property(x => x.EmployeeId).IsModified || entry.Property(x => x.CoachType).IsModified)
                throw new InvalidOperationException("An activity coach's internal/external identity cannot be reassigned.");
        if (ChangeTracker.Entries<ActivityCoachSettlement>().Any(x => x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Finalized activity settlements are historical snapshots.");
        if (ChangeTracker.Entries<AuditLog>().Any(entry =>
            entry.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Audit logs are append-only.");
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnforceAppendOnlyAudit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnforceAppendOnlyAudit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
