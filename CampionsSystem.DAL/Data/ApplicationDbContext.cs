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
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberSalesAssignment> MemberSalesAssignments => Set<MemberSalesAssignment>();
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private void EnforceAppendOnlyAudit()
    {
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
