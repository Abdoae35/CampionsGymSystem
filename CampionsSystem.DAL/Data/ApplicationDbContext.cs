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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private void EnforceAppendOnlyAudit()
    {
        if (ChangeTracker.Entries<MembershipSubscriptionTransaction>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<MemberSubscriptionBenefit>().Any(x => x.State is EntityState.Modified or EntityState.Deleted)
            || ChangeTracker.Entries<SubscriptionHolidayExtension>().Any(x => x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Subscription grants, benefit snapshots and holiday compensation are append-only.");
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
