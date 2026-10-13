# Complete table inventory

[Navigation](README.md) · [Relationships and legend](OVERVIEW.md)

**112 total in the documented fully migrated schema:** 104 domain/application +
7 ASP.NET Identity + 1 EF migration-history table. Only the first 111 are mapped
through ApplicationDbContext. Actual installed database contents were not queried.
Each table appears once below; diagrams may repeat boundary references.

| Module | Count |
|---|---:|
| [Authentication, Employees and Branches](01-auth-employees-branches.md) | 15 |
| [Members and CRM](02-members-crm.md) | 7 |
| [Subscriptions and Check-In](03-subscriptions-check-in.md) | 17 |
| [Sales and Payments](04-sales-payments.md) | 14 |
| [Shifts, Vault and Expenses](05-shifts-vault-expenses.md) | 13 |
| [Sales Commissions](06-commissions.md) | 6 |
| [Private, Classes and Activities](07-private-classes.md) | 24 |
| [Monthly Closing and Payroll](08-monthly-closing-payroll.md) | 9 |
| [Supporting Infrastructure](09-supporting-infrastructure.md) | 7 |
| **Total** | **112** |

## Authentication, Employees and Branches

| Actual table | Kind | What it stores | Primary key |
|---|---|---|---|
| [AspNetUsers](01-auth-employees-branches.md#aspnetusers) | Framework / core identity | Stores login accounts and their Identity security settings. | `Id` |
| [AspNetRoles](01-auth-employees-branches.md#aspnetroles) | Framework / configuration | Stores the existing Identity role names. | `Id` |
| [AspNetUserRoles](01-auth-employees-branches.md#aspnetuserroles) | Framework / junction | Links accounts to their assigned roles. | `UserId, RoleId` |
| [AspNetUserClaims](01-auth-employees-branches.md#aspnetuserclaims) | Framework | Stores individual Identity claims for accounts. | `Id` |
| [AspNetRoleClaims](01-auth-employees-branches.md#aspnetroleclaims) | Framework | Stores Identity claims associated with roles. | `Id` |
| [AspNetUserLogins](01-auth-employees-branches.md#aspnetuserlogins) | Framework / junction | Stores external login provider identities linked to accounts. | `LoginProvider, ProviderKey` |
| [AspNetUserTokens](01-auth-employees-branches.md#aspnetusertokens) | Framework | Stores Identity provider tokens associated with accounts. | `UserId, LoginProvider, Name` |
| [Permissions](01-auth-employees-branches.md#permissions) | Configuration | Stores the canonical capability codes, titles, modules and active flags. | `Id` |
| [RolePermissions](01-auth-employees-branches.md#rolepermissions) | Configuration / junction | Links existing roles to their granted capabilities. | `RoleId, PermissionId` |
| [UserPermissionOverrides](01-auth-employees-branches.md#userpermissionoverrides) | Configuration / junction | Stores one direct Allow or Deny decision per account and capability. | `UserId, PermissionId` |
| [Cities](01-auth-employees-branches.md#cities) | Configuration | Stores the cities used by branches and plans. | `Id` |
| [GymBranches](01-auth-employees-branches.md#gymbranches) | Core | Stores each gym branch and its operational settings. | `Id` |
| [Employees](01-auth-employees-branches.md#employees) | Core | Stores employee profiles, employment status and optional account/member links. | `Id` |
| [EmployeeBranchAssignments](01-auth-employees-branches.md#employeebranchassignments) | History / junction | Stores dated employee assignments and transfers between branches. | `Id` |
| [EmployeeLeaveRequests](01-auth-employees-branches.md#employeeleaverequests) | Core / history | Stores employee leave requests and their review decisions. | `Id` |

## Members and CRM

| Actual table | Kind | What it stores | Primary key |
|---|---|---|---|
| [Members](02-members-crm.md#members) | Core | Stores registered members, contact details, branch and member codes. | `Id` |
| [MemberSalesAssignments](02-members-crm.md#membersalesassignments) | History / junction | Stores dated member ownership by a Sales employee or House. | `Id` |
| [MemberFollowUpTasks](02-members-crm.md#memberfollowuptasks) | Core / history | Stores member follow-up tasks, responsible employees and completion facts. | `Id` |
| [MemberAudienceCategories](02-members-crm.md#memberaudiencecategories) | History / junction | Stores dated member eligibility categories used by offers. | `Id` |
| [Leads](02-members-crm.md#leads) | Core | Stores prospective customers and their conversion or loss details. | `Id` |
| [LeadAssignments](02-members-crm.md#leadassignments) | History / junction | Stores dated Sales employee ownership of leads. | `Id` |
| [LeadInteractions](02-members-crm.md#leadinteractions) | History | Stores recorded lead contacts, outcomes and next follow-up dates. | `Id` |

## Subscriptions and Check-In

| Actual table | Kind | What it stores | Primary key |
|---|---|---|---|
| [MembershipPlans](03-subscriptions-check-in.md#membershipplans) | Configuration | Stores regular gym membership plan prices, durations and access rules. | `Id` |
| [MembershipPlanCategories](03-subscriptions-check-in.md#membershipplancategories) | Configuration | Stores named plan/member audience categories. | `Id` |
| [MembershipPlanEligibleMembers](03-subscriptions-check-in.md#membershipplaneligiblemembers) | Configuration / junction | Links restricted membership plans to eligible members. | `Id` |
| [MembershipBenefits](03-subscriptions-check-in.md#membershipbenefits) | Configuration | Stores reusable membership benefit definitions. | `Id` |
| [MembershipPlanBenefits](03-subscriptions-check-in.md#membershipplanbenefits) | Configuration / junction | Links membership plans to their configured benefit grants. | `Id` |
| [MemberSubscriptions](03-subscriptions-check-in.md#membersubscriptions) | Core | Stores a member's regular gym entitlement, dates and session balance. | `Id` |
| [MembershipSubscriptionTransactions](03-subscriptions-check-in.md#membershipsubscriptiontransactions) | History / evidence | Stores purchase or renewal grants with historical plan and price snapshots. | `Id` |
| [MemberSubscriptionBenefits](03-subscriptions-check-in.md#membersubscriptionbenefits) | Evidence | Stores the benefit quantities and descriptions granted to a subscription. | `Id` |
| [MemberBenefitUsages](03-subscriptions-check-in.md#memberbenefitusages) | History / evidence | Stores actual consumption of granted subscription benefits. | `Id` |
| [SubscriptionFreezes](03-subscriptions-check-in.md#subscriptionfreezes) | History | Stores subscription freezes and the extension days applied. | `Id` |
| [SubscriptionHolidayExtensions](03-subscriptions-check-in.md#subscriptionholidayextensions) | Evidence | Stores closure-day compensation already applied to a subscription. | `Id` |
| [SubscriptionGrantCancellations](03-subscriptions-check-in.md#subscriptiongrantcancellations) | Evidence | Stores reversals of sold subscription grants without erasing purchase history. | `Id` |
| [BorrowedGymSessions](03-subscriptions-check-in.md#borrowedgymsessions) | Core / history | Stores temporary gym entry debts and their settlement links. | `Id` |
| [BorrowedDebtSettlementEvents](03-subscriptions-check-in.md#borroweddebtsettlementevents) | History / evidence | Stores ordered settlement and reversal events for borrowed-session debts. | `Id` |
| [GuestInvitations](03-subscriptions-check-in.md#guestinvitations) | Core / history | Stores guest invitations, their source benefit and redemption status. | `Id` |
| [BranchClosures](03-subscriptions-check-in.md#branchclosures) | Configuration / history | Stores dated branch closures and entitlement compensation settings. | `Id` |
| [CheckIns](03-subscriptions-check-in.md#checkins) | Core / evidence | Stores gym, activity, class or trial attendance and session-consumption evidence. | `Id` |

## Sales and Payments

| Actual table | Kind | What it stores | Primary key |
|---|---|---|---|
| [SaleTransactions](04-sales-payments.md#saletransactions) | Core / evidence | Stores sold products, amounts, attribution snapshots and cancellation facts. | `Id` |
| [SaleMembershipSnapshots](04-sales-payments.md#salemembershipsnapshots) | Evidence | Stores the regular membership terms frozen for a sale. | `SaleTransactionId` |
| [SaleBenefitSnapshots](04-sales-payments.md#salebenefitsnapshots) | Evidence | Stores the membership benefits promised in a sale snapshot. | `Id` |
| [Payments](04-sales-payments.md#payments) | Core / evidence | Stores member collections and the user who received them. | `Id` |
| [PaymentParts](04-sales-payments.md#paymentparts) | Evidence / junction | Stores each payment's allocation across financial accounts and provider references. | `Id` |
| [Refunds](04-sales-payments.md#refunds) | Core / evidence | Stores returned member money, reasons and original sale/payment links. | `Id` |
| [RefundParts](04-sales-payments.md#refundparts) | Evidence / junction | Stores each refund's outbound account allocation and provider reference. | `Id` |
| [InstallmentAgreements](04-sales-payments.md#installmentagreements) | Core | Stores an installment schedule agreed for a sale. | `Id` |
| [Installments](04-sales-payments.md#installments) | Core | Stores individual installment due amounts, dates and paid balances. | `Id` |
| [InstallmentPaymentAllocations](04-sales-payments.md#installmentpaymentallocations) | Evidence / junction | Links actual payments to the installments they fund. | `Id` |
| [Offers](04-sales-payments.md#offers) | Configuration | Stores promotions, discount rules and eligibility descriptions. | `Id` |
| [OfferMembershipPlans](04-sales-payments.md#offermembershipplans) | Configuration / junction | Links offers to eligible membership plans. | `OfferId, MembershipPlanId` |
| [OfferActivityPackages](04-sales-payments.md#offeractivitypackages) | Configuration / junction | Links offers to eligible activity packages. | `OfferId, ActivityPackageId` |
| [MemberOfferEntitlements](04-sales-payments.md#memberofferentitlements) | History / evidence | Stores explicit member offer grants and revocation facts. | `Id` |

## Shifts, Vault and Expenses

| Actual table | Kind | What it stores | Primary key |
|---|---|---|---|
| [FinancialAccounts](05-shifts-vault-expenses.md#financialaccounts) | Configuration / core | Stores branch Cash/InstaPay accounts and their opening balances. | `Id` |
| [FinancialTransactions](05-shifts-vault-expenses.md#financialtransactions) | Core ledger / evidence | Stores actual account credits/debits and their source references. | `Id` |
| [Shifts](05-shifts-vault-expenses.md#shifts) | Core / history | Stores employee shifts, cash count snapshots and closing discrepancies. | `Id` |
| [ShiftEmployees](05-shifts-vault-expenses.md#shiftemployees) | Junction / history | Stores employee participation intervals associated with shifts. | `ShiftId, EmployeeId` |
| [ShiftEmployeeChanges](05-shifts-vault-expenses.md#shiftemployeechanges) | History | Stores recorded changes to shift employee participation. | `Id` |
| [BusinessDays](05-shifts-vault-expenses.md#businessdays) | Core / configuration | Stores each branch's explicitly opened and closed business dates. | `Id` |
| [CashDrawers](05-shifts-vault-expenses.md#cashdrawers) | Configuration / core | Links one shared branch drawer to its financial account and movement sequence. | `BranchId` |
| [CashCounts](05-shifts-vault-expenses.md#cashcounts) | Evidence | Stores physical cash observations, expected/book cash and discrepancies. | `Id` |
| [CashMovements](05-shifts-vault-expenses.md#cashmovements) | Core / evidence | Stores authorized branch cash movement or withdrawal records. | `Id` |
| [Expenses](05-shifts-vault-expenses.md#expenses) | Core / evidence | Stores expense requests, payment facts, category and receipt references. | `Id` |
| [ExpenseCategories](05-shifts-vault-expenses.md#expensecategories) | Configuration | Stores the categories used to classify expenses. | `Id` |
| [ExpenseReversals](05-shifts-vault-expenses.md#expensereversals) | Evidence | Stores money returned against an expense without erasing its payment. | `Id` |
| [EmployeeAdvances](05-shifts-vault-expenses.md#employeeadvances) | Core / evidence | Stores actual employee advances with branch, funding account and salary ceiling evidence. | `Id` |

## Sales Commissions

| Actual table | Kind | What it stores | Primary key |
|---|---|---|---|
| [CommissionPeriods](06-commissions.md#commissionperiods) | Configuration / history | Stores branch commission evaluation boundaries and finalization metadata. | `Id` |
| [CommissionRules](06-commissions.md#commissionrules) | Configuration / history | Stores configurable employee or branch commission targets and rates. | `Id` |
| [CommissionCollections](06-commissions.md#commissioncollections) | Evidence | Stores collection-time commercial employee attribution and eligibility per payment. | `PaymentId` |
| [CommissionRefundAllocations](06-commissions.md#commissionrefundallocations) | Evidence / junction | Links refunded amounts to the original commission collection payments. | `RefundId, PaymentId` |
| [CommissionStatementLines](06-commissions.md#commissionstatementlines) | Evidence / junction | Stores finalized period source lines including eligible, House and excluded collections. | `CommissionPeriodId, PaymentId` |
| [EmployeeCommissionSummaries](06-commissions.md#employeecommissionsummaries) | Evidence | Stores finalized employee commission totals, targets and rate snapshots. | `Id` |

## Private, Classes and Activities

| Actual table | Kind | What it stores | Primary key |
|---|---|---|---|
| [PrivatePackages](07-private-classes.md#privatepackages) | Configuration | Stores Private coaching package prices, session counts and durations. | `Id` |
| [PrivateSubscriptions](07-private-classes.md#privatesubscriptions) | Core | Stores a member's Private entitlement and remaining session balance. | `Id` |
| [PrivateSubscriptionTransactions](07-private-classes.md#privatesubscriptiontransactions) | History / evidence | Stores purchased Private batches with sale, original coach and session-value snapshots. | `Id` |
| [PrivateCoachAssignments](07-private-classes.md#privatecoachassignments) | History / junction | Stores Private coach allocation and transfer history. | `Id` |
| [PrivateExpirySettlements](07-private-classes.md#privateexpirysettlements) | Evidence | Stores explicitly authorized unused-session earnings entitlement at expiry. | `Id` |
| [PrivateSessions](07-private-classes.md#privatesessions) | Core / evidence | Stores actual Private sessions, coach attribution and required gym check-in evidence. | `Id` |
| [ClassesPackages](07-private-classes.md#classespackages) | Configuration | Stores shared Classes package prices, session counts and durations. | `Id` |
| [ClassesSubscriptions](07-private-classes.md#classessubscriptions) | Core / evidence | Stores purchased Classes balances shared across class types. | `Id` |
| [GroupClasses](07-private-classes.md#groupclasses) | Configuration | Stores class types such as Yoga or Zumba. | `Id` |
| [ClassSchedules](07-private-classes.md#classschedules) | Configuration | Stores recurring timetable definitions for class types. | `Id` |
| [ClassSessions](07-private-classes.md#classsessions) | Core / history | Stores dated class occurrences, assigned coaches and completion state. | `Id` |
| [SportsActivities](07-private-classes.md#sportsactivities) | Configuration | Stores the separate sports/activity catalog. | `Id` |
| [ActivityPackages](07-private-classes.md#activitypackages) | Configuration | Stores purchasable activity packages and their access rules. | `Id` |
| [ActivitySchedules](07-private-classes.md#activityschedules) | Configuration | Stores recurring timetable definitions for sports activities. | `Id` |
| [ActivitySubscriptions](07-private-classes.md#activitysubscriptions) | Core | Stores a member's activity entitlement and remaining session balance. | `Id` |
| [ActivitySubscriptionTransactions](07-private-classes.md#activitysubscriptiontransactions) | History / evidence | Stores activity purchase grants and historical coach/rate/value snapshots. | `Id` |
| [ActivityCoaches](07-private-classes.md#activitycoaches) | Configuration / core | Stores internal or external activity coach identities and percentage settings. | `Id` |
| [ActivityCoachAssignments](07-private-classes.md#activitycoachassignments) | History / junction | Stores dated activity coach assignments using the supported coach identity alternatives. | `Id` |
| [ActivityCoachSettlements](07-private-classes.md#activitycoachsettlements) | Evidence | Stores finalized activity coach revenue and entitlement snapshots. | `Id` |
| [CoachTargetRules](07-private-classes.md#coachtargetrules) | Configuration / evidence | Stores explicit Private value and Classes attendance-average targets per coach and period. | `Id` |
| [CoachEarnings](07-private-classes.md#coachearnings) | Evidence | Stores recognized funded Private session or expiry entitlement slices and commission amounts. | `Id` |
| [CoachPayouts](07-private-classes.md#coachpayouts) | Core / evidence | Stores actual partial coach payments linked to an existing paid expense. | `Id` |
| [CoachPayoutAllocations](07-private-classes.md#coachpayoutallocations) | Evidence / junction | Links each coach payout to the recognized earnings it settles. | `CoachPayoutId, CoachEarningId` |
| [FreelanceAgreements](07-private-classes.md#freelanceagreements) | Configuration / history | Links freelance coaches to their dedicated membership plans over dated agreements. | `Id` |

## Monthly Closing and Payroll

| Actual table | Kind | What it stores | Primary key |
|---|---|---|---|
| [FinancialMonths](08-monthly-closing-payroll.md#financialmonths) | Core / configuration | Stores independent branch financial reporting periods and lifecycle state. | `Id` |
| [FinancialMonthEvents](08-monthly-closing-payroll.md#financialmonthevents) | Audit / evidence | Stores immutable month opening, closing and reopening events. | `Id` |
| [FinancialClosingRevisions](08-monthly-closing-payroll.md#financialclosingrevisions) | Evidence | Stores immutable closing report JSON, exact workbook bytes and revision hashes. | `Id` |
| [EmployeeSalarySettings](08-monthly-closing-payroll.md#employeesalarysettings) | Configuration / history | Stores effective-dated employee basic salary settings. | `Id` |
| [Payrolls](08-monthly-closing-payroll.md#payrolls) | Core / evidence | Stores employee salary or finalized Sales commission payable entitlements. | `Id` |
| [PayrollDeductions](08-monthly-closing-payroll.md#payrolldeductions) | Evidence / junction | Stores deductions against payroll entitlements, including linked employee advances. | `Id` |
| [PayrollPaymentRecords](08-monthly-closing-payroll.md#payrollpaymentrecords) | Evidence | Stores Owner-declared partial employee payments without inferring a funding account. | `Id` |
| [MaintenancePlanRevisions](08-monthly-closing-payroll.md#maintenanceplanrevisions) | Configuration / history | Stores informational recurring maintenance forecasts and their revisions. | `Id` |
| [FinancialOccurrenceEvidence](08-monthly-closing-payroll.md#financialoccurrenceevidence) | Evidence | Stores a separately reported original occurrence time for a ledger movement. | `Id` |

## Supporting Infrastructure

| Actual table | Kind | What it stores | Primary key |
|---|---|---|---|
| [AuditLogs](09-supporting-infrastructure.md#auditlogs) | Audit | Stores actors, timestamps and before/after evidence for audited operations. | `Id` |
| [AuthorizedDevices](09-supporting-infrastructure.md#authorizeddevices) | Configuration | Stores registered device hashes, account ownership and revocation state. | `Id` |
| [BranchNetworkRules](09-supporting-infrastructure.md#branchnetworkrules) | Configuration | Stores branch network access rule definitions. | `Id` |
| [ContractTemplates](09-supporting-infrastructure.md#contracttemplates) | Configuration | Stores versioned contract template text. | `Id` |
| [MemberContracts](09-supporting-infrastructure.md#membercontracts) | History / evidence | Stores generated member contract snapshots and physical archive references. | `Id` |
| [MessageLogs](09-supporting-infrastructure.md#messagelogs) | Infrastructure / history | Stores outbound message status and delivery/failure metadata. | `Id` |
| [__EFMigrationsHistory](09-supporting-infrastructure.md#__efmigrationshistory) | Framework | Stores which EF Core migrations have been applied to a database. | `MigrationId` |
