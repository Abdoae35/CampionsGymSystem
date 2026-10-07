# Gym database schema

Current aligned model: **75 application tables plus seven Identity tables (82 total)**.
See [final schema-gap and rollout report](FINAL_SCHEMA_ALIGNMENT.md).

Identity uses the seven standard AspNet tables and Guid user/role keys.
All business foreign keys restrict deletion. Enums are checked strings. Money
is numeric(18,2); rates numeric(5,2); timestamps are UTC timestamptz.

| Table | Purpose | Foreign key relationships |
| --- | --- | --- |
| Cities | City configuration | None |
| GymBranches | Branch settings and city ownership | CityId → Cities |
| Permissions | Dynamic permission catalog | None |
| UserPermissionOverrides | Per-user Allow/Deny storage | UserId → AspNetUsers, PermissionId → Permissions, optional CreatedByUserId → AspNetUsers |
| RolePermissions | RoleDynamic permission catalog | RoleId → AspNetRoles, PermissionId → Permissions |
| Employees | Staff/freelance identity and employment | UserId → AspNetUsers |
| EmployeeBranchAssignments | Effective-dated branch assignment history | EmployeeId → Employees, BranchId → GymBranches, AssignedByUserId → AspNetUsers |
| EmployeeLeaveRequests | Pending/approved/rejected leave and review | EmployeeId → Employees, optional ReviewedByUserId → AspNetUsers |
| EmployeeSalarySettings | Effective-dated salary history | EmployeeId → Employees, CreatedByUserId → AspNetUsers |
| Members | Member profile, unique code and phone | HomeBranchId → GymBranches, CreatedByUserId → AspNetUsers |
| MemberSalesAssignments | Current sales/house owner and assignment history | MemberId → Members, SalesEmployeeId → Employees, AssignedByUserId → AspNetUsers |
| Leads | Branch CRM prospect and conversion | BranchId → GymBranches, ConvertedMemberId → Members, CreatedByUserId → AspNetUsers |
| LeadAssignments | Sales ownership history | LeadId → Leads, SalesEmployeeId → Employees, AssignedByUserId → AspNetUsers |
| LeadInteractions | Contact and follow-up history | LeadId → Leads, EmployeeId → Employees |
| MembershipPlanCategories | Dynamic audience classification, independent of Normal/Freelance | None |
| MembershipPlans | Normal or Coach-specific Freelance gym products | CityId → Cities, optional FreelanceCoachEmployeeId → Employees |
| MemberSubscriptions | Gym entitlement with access/session snapshot | MemberId → Members, MembershipPlanId → MembershipPlans, CityId → Cities, PurchasedAtBranchId → GymBranches |
| MembershipSubscriptionTransactions | Initial/renewal purchases and price/date snapshots | MemberSubscriptionId → MemberSubscriptions, SaleTransactionId → SaleTransactions, MembershipPlanId → MembershipPlans, optional FreelanceCoachEmployeeIdSnapshot → Employees |
| SubscriptionFreezes | Approved freeze history | MemberSubscriptionId → MemberSubscriptions, ApprovedByUserId → AspNetUsers |
| BorrowedGymSessions | Expired-cycle borrowing and settlement | MemberId → Members, ExpiredSubscriptionId → MemberSubscriptions, CheckInId → CheckIns, BranchId → GymBranches, SettledBySubscriptionId → MemberSubscriptions |
| BranchClosures | Closure dates and extension scope | BranchId → GymBranches, CreatedByUserId → AspNetUsers |
| PrivatePackages | Branch coaching product | BranchId → GymBranches |
| PrivateSubscriptions | Private session entitlement | MemberId → Members, BranchId → GymBranches |
| PrivateSubscriptionTransactions | Private purchase/renewal history | PrivateSubscriptionId → PrivateSubscriptions, SaleTransactionId → SaleTransactions, PrivatePackageId → PrivatePackages |
| PrivateCoachAssignments | Assignment history only (legacy financial columns deprecated) | PrivateSubscriptionId → PrivateSubscriptions, CoachEmployeeId → Employees, CreatedByUserId → AspNetUsers |
| PrivateSessions | Delivered session, immutable batch basis and occurrence period | PrivateSubscriptionTransactionId → PrivateSubscriptionTransactions, CommissionPeriodId → CommissionPeriods, PrivateSubscriptionId → PrivateSubscriptions, CoachEmployeeId → Employees, GymCheckInId → CheckIns, CreatedByUserId → AspNetUsers |
| PrivateExpirySettlements | Historical unused batch value at final expiry | PrivateSubscriptionId → PrivateSubscriptions, unique PrivateSubscriptionTransactionId → PrivateSubscriptionTransactions, CoachEmployeeId → Employees, CommissionPeriodId → CommissionPeriods, optional CreatedByUserId → AspNetUsers |
| SportsActivities | Branch activity catalog | BranchId → GymBranches |
| ActivityPackages | Activity access/pricing configuration | SportsActivityId → SportsActivities |
| ActivityCoaches | Internal/external identity and configurable percentage | optional unique EmployeeId → Employees |
| ActivityCoachAssignments | Effective-dated activity coach assignment | SportsActivityId → SportsActivities, ActivityCoachId → ActivityCoaches; legacy CoachEmployeeId retained |
| ActivitySchedules | Weekly coach/activity time slots | ActivityCoachAssignmentId → ActivityCoachAssignments |
| ActivitySubscriptions | Independent activity entitlement and current selected coach | MemberId → Members, ActivityPackageId → ActivityPackages, BranchId → GymBranches, ActivityCoachId → ActivityCoaches, ActivityCoachAssignmentId → ActivityCoachAssignments |
| ActivitySubscriptionTransactions | Activity purchase/renewal history and immutable coach/rate | ActivitySubscriptionId → ActivitySubscriptions, SaleTransactionId → SaleTransactions, ActivityPackageId → ActivityPackages, ActivityCoachIdSnapshot → ActivityCoaches |
| GroupClasses | Branch group class configuration | BranchId → GymBranches, CoachEmployeeId → Employees |
| ClassSchedules | Weekly class time slots | GroupClassId → GroupClasses |
| ClassSessions | Actual occurrence, status and delivering coach | GroupClassId → GroupClasses, optional ClassScheduleId → ClassSchedules, BranchId → GymBranches, CoachEmployeeId → Employees |
| MembershipPlanEligibleMembers | Explicit Freelance purchase eligibility | MembershipPlanId → MembershipPlans, MemberId → Members, CreatedByUserId → AspNetUsers |
| ActivityCoachSettlements | Finalized coach/activity/rate revenue and entitlement | CommissionPeriodId → CommissionPeriods, ActivityCoachId → ActivityCoaches, SportsActivityId → SportsActivities, optional EmployeeId → Employees, CreatedByUserId → AspNetUsers |
| CheckIns | Gym/activity/class attendance and consumption | MemberId → Members, BranchId → GymBranches, MemberSubscriptionId → MemberSubscriptions, ActivitySubscriptionId → ActivitySubscriptions, GroupClassId → GroupClasses, CoachEmployeeId → Employees, CreatedByUserId → AspNetUsers |
| SaleTransactions | Historical branch sale totals and seller attribution | MemberId → Members, BranchId → GymBranches, CreatedShiftId → Shifts, SalesEmployeeId → Employees, OfferId → Offers, CreatedByUserId → AspNetUsers |
| Payments | Received funds linked to sale/shift/period | SaleTransactionId → SaleTransactions, BranchId → GymBranches, ShiftId → Shifts, CommissionPeriodId → CommissionPeriods, InstallmentId → Installments, ReceivedByUserId → AspNetUsers |
| PaymentParts | Split payment allocation across accounts | PaymentId → Payments, FinancialAccountId → FinancialAccounts |
| InstallmentAgreements | Sale installment agreement | SaleTransactionId → SaleTransactions, CreatedByUserId → AspNetUsers |
| Installments | Due/paid installment amounts and dates | InstallmentAgreementId → InstallmentAgreements |
| Refunds | Authorized refund amount and provenance | SaleTransactionId → SaleTransactions, OriginalPaymentId → Payments, BranchId → GymBranches, ShiftId → Shifts, ProcessedByUserId → AspNetUsers, ApprovedByUserId → AspNetUsers |
| RefundParts | Refund account allocations | RefundId → Refunds, FinancialAccountId → FinancialAccounts |
| FinancialAccounts | Account opening balance/configuration | BranchId → GymBranches |
| FinancialTransactions | Explicit-source credit/debit ledger | FinancialAccountId → FinancialAccounts, BranchId → GymBranches, PaymentId → Payments, RefundId → Refunds, ExpenseId → Expenses, PayrollId → Payrolls, CreatedByUserId → AspNetUsers |
| ExpenseCategories | Expense classification | None |
| Expenses | Historical expense approval and account | BranchId → GymBranches, ShiftId → Shifts, ExpenseCategoryId → ExpenseCategories, FinancialAccountId → FinancialAccounts, CreatedByUserId → AspNetUsers, ApprovedByUserId → AspNetUsers |
| BusinessDays | Branch operational day independent of midnight | BranchId → GymBranches, optional opening/closing user → AspNetUsers |
| ShiftEmployeeChanges | Original/replacement shift history and actor | ShiftId → Shifts, OriginalEmployeeId/ReplacementEmployeeId → Employees, ChangedByUserId → AspNetUsers |
| Shifts | Branch cash shift lifecycle | BranchId → GymBranches, OpenedByUserId → AspNetUsers, ClosedByUserId → AspNetUsers |
| ShiftEmployees | ShiftStaff/freelance identity and employment | ShiftId → Shifts, EmployeeId → Employees |
| CommissionPeriods | Branch close/reopen lifecycle | BranchId → GymBranches, ClosedByUserId → AspNetUsers, ReopenedByUserId → AspNetUsers |
| CommissionRules | Effective-dated branch default/employee override | BranchId → GymBranches, EmployeeId → Employees, SportsActivityId → SportsActivities, GroupClassId → GroupClasses, CreatedByUserId → AspNetUsers |
| EmployeeCommissionSummaries | Finalized branch/employee/category financial snapshot | CommissionPeriodId → CommissionPeriods, EmployeeId → Employees, BranchId → GymBranches, SportsActivityId → SportsActivities, GroupClassId → GroupClasses |
| Payrolls | Salary/commission/deduction payment snapshot | EmployeeId → Employees, BranchId → GymBranches, CommissionPeriodId → CommissionPeriods, PaidByUserId → AspNetUsers, PaidFromFinancialAccountId → FinancialAccounts |
| PayrollDeductions | Itemized payroll deductions | PayrollId → Payrolls, CreatedByUserId → AspNetUsers |
| Offers | Dynamic discount/extra-days configuration | BranchId → GymBranches, CreatedByUserId → AspNetUsers |
| OfferMembershipPlans | OfferCity-configured gym products | OfferId → Offers, MembershipPlanId → MembershipPlans |
| OfferActivityPackages | OfferActivity access/pricing configuration | OfferId → Offers, ActivityPackageId → ActivityPackages |
| MemberOfferEntitlements | Member-specific offer grants | MemberId → Members, OfferId → Offers, GrantedByEmployeeId → Employees, CreatedByUserId → AspNetUsers |
| FreelanceAgreements | Effective-dated freelance branch agreements | EmployeeId → Employees, BranchId → GymBranches, CreatedByUserId → AspNetUsers |
| ContractTemplates | Versioned contract content | CreatedByUserId → AspNetUsers |
| MemberContracts | Exact generated contract snapshot and physical archive | MemberId → Members, SaleTransactionId → SaleTransactions, ContractTemplateId → ContractTemplates, GeneratedByUserId → AspNetUsers |
| MessageLogs | Delivery metadata without OTP values or secrets | MemberId → Members |
| AuditLogs | Append-only action history and JSON snapshots | UserId → AspNetUsers, BranchId → GymBranches |
| BranchNetworkRules | Branch IP/CIDR/VPN configuration | BranchId → GymBranches |
| AuthorizedDevices | Hashed device registration/revocation | BranchId → GymBranches, RegisteredByUserId → AspNetUsers |

## Modules

Organization: Cities, GymBranches. Employees: Employees, EmployeeBranchAssignments,
EmployeeSalarySettings. Members: Members, MemberSalesAssignments. CRM: Leads,
LeadAssignments, LeadInteractions. Gym: MembershipPlans, MemberSubscriptions,
MembershipSubscriptionTransactions, SubscriptionFreezes, BorrowedGymSessions,
BranchClosures. Private: PrivatePackages, PrivateSubscriptions,
PrivateSubscriptionTransactions, PrivateCoachAssignments, PrivateSessions, PrivateExpirySettlements.
Activities: SportsActivities, ActivityPackages, ActivityCoachAssignments,
ActivitySchedules, ActivitySubscriptions, ActivitySubscriptionTransactions.
Classes: GroupClasses, ClassSchedules. Attendance: CheckIns. Sales: SaleTransactions.
Payments: Payments, PaymentParts. Installments: InstallmentAgreements, Installments.
Refunds: Refunds, RefundParts. Accounts: FinancialAccounts, FinancialTransactions.
Expenses: ExpenseCategories, Expenses. Shifts: Shifts, ShiftEmployees.
Commission: CommissionPeriods, CommissionRules, EmployeeCommissionSummaries.
Payroll: Payrolls, PayrollDeductions. Offers: Offers, OfferMembershipPlans,
OfferActivityPackages, MemberOfferEntitlements. Freelancers: FreelanceAgreements.
Contracts: ContractTemplates, MemberContracts. Messaging: MessageLogs.
Audit: AuditLogs. Network/device: BranchNetworkRules, AuthorizedDevices.
Authorization: Permissions, RolePermissions and standard Identity tables.

## Design notes

- Current row partial unique indexes cover member sales ownership, primary employee
  branch assignment, private coach ownership, and an open period per branch.
- Guid keys are application-generated; standard Identity claim IDs remain integers
  per Identity conventions. Junctions use composite keys.
- ActivitySubscription additionally snapshots AccessMode. General date fields use
  DateOnly, schedule times TimeOnly, and event instants DateTimeOffset at UTC.
- No baseline data was seeded: role/permission administration will populate the
  dynamic catalogs; commission defaults require actual branches first.
- Audit is append-only through tracked SaveChanges, not database triggers.
- Migration SQL is reviewable in InitialCreate.sql. validate-local.sql uses only
  synthetic rows within a rolled-back transaction; run on development only.
- EF design dependency is private in the API startup project; migrations/model
  and all schema ownership remain in DAL. dotnet-ef is a local tool (10.0.8).

## Private and Freelance rule update (2026-10-06)

- MembershipPlans add string PlanCategory (Normal/Freelance) and optional restricted
  FreelanceCoachEmployeeId → Employees; category/Coach consistency is checked.
  Price retains numeric(18,2), and is fixed gym money for Freelance plans.
- MembershipSubscriptionTransactions add PlanCategorySnapshot and optional restricted
  FreelanceCoachEmployeeIdSnapshot → Employees, with matching category check/index.
- PrivateSubscriptionTransactions add bounded RemainingSessions and FIFO index.
  Purchase facts are immutable through tracked EF saves; only counter changes remain.
- PrivateSessions add required batch and CommissionPeriod FKs and Coach/date index.
- PrivateExpirySettlements (new, 62nd application table) reference subscription,
  batch (unique, 0..1 per batch), Coach, CommissionPeriod and optional initiating
  user, all restricted. Snapshot count>0/unit>=0 and count*unit=EligibleAmount
  are checked; decimal snapshots use numeric(18,2).
- PrivateCoachAssignments retain the existing unique EndedAt IS NULL index and
  add subscription/EndedAt lookup. Deprecated financial columns are retained
  solely to preserve unreconciled legacy data, with zero defaults for new rows.
- FreelanceAgreement remains metadata; no extra pricing/client/offer tables.

See DATA_FLOWS.md and UPDATE_PRIVATE_AND_FREELANCE.md for flow and rollout details.
InitialCreate and its SQL remain the historical baseline, not the current schema.

## Activities, Classes and Freelance update (2026-10-07)

Before final alignment, the model contained **66 application tables plus seven Identity tables (73 total)**.

- ActivityCoaches: Guid PK; string CoachType Internal/External. CHECK requires EmployeeId
  for Internal and name/no Employee for External. Internal external-contact columns are
  null; one profile per Employee (partial unique). Percentage numeric(5,2), CHECK 0..100.
- ActivityCoachAssignments: nullable ActivityCoachId FK; legacy CoachEmployeeId made
  nullable without removal. CHECK exactly one coach link. New services use ActivityCoachId.
  Composite (SportsActivityId, ActivityCoachId, StartDate) index supports valid assignments.
- ActivitySubscriptions: nullable ActivityCoachId FK plus (ActivityCoachId, CreatedAt)
  index. Nullable is transitional for old rows; new BLL purchases require a coach.
- ActivitySubscriptionTransactions: nullable ActivityCoachIdSnapshot and
  CoachPercentageSnapshot numeric(5,2), checked as both null or both present with rate
  0..100. Existing rows remain unknown; never infer a historical rate from current config.
- ActivityCoachSettlements: unique (CommissionPeriodId, ActivityCoachId, SportsActivityId,
  Percentage); numeric(18,2) revenue/entitlement, numeric(5,2) rate, nonnegative checks,
  coach/activity name and type snapshots. Optional restricted EmployeeId for internal
  payroll integration; external coaches never acquire artificial Employee records.
- ClassSessions: GroupClass/schedule/branch/actual coach FKs. CHECK status and times;
  completed requires EndedAt. Coach+StartedAt, GroupClass+StartedAt and Branch+StartedAt
  indexes; planned schedule is optional. CheckIns adds nullable restricted ClassSessionId
  and unique (ClassSessionId, MemberId) filtered to actual sessions. CHECK permits an
  occurrence link only on Class CheckIns. Legacy check-ins remain intact and unassigned.
- MembershipPlanEligibleMembers: Guid PK, unique (MembershipPlanId, MemberId), reverse
  MemberId FK index, creator/timestamp. No member-global uniqueness and no purchase FK
  to eligibility; removal cannot erase purchase history.
- SaleTransactions: nullable string SalesAttributionSnapshot for legacy transition.
  CHECK House has no SalesEmployeeId/SaleCategory; Sales requires SalesEmployeeId.
  New Freelance sales are House and retain existing plan/coach transaction snapshots.
- All new FKs use Restrict, no business cascades. FK indexes generated by EF are
  retained unless covered by a composite prefix. DateTimeOffset uses timestamptz.
- Eight new permission codes and existing Owner grants are added by the migration;
  other roles are unchanged. No new user/role/employee/member/eligibility data is seeded.

See [deployment and reconciliation](UPDATE_ACTIVITIES_CLASSES_AND_FREELANCE.md).

## Final business schema alignment (2026-10-07)

- UserPermissionOverrides uses composite Guid PK (UserId, PermissionId), checked string
  Effect Allow/Deny, optional creator and real CreatedAt. FKs Restrict. PK enforces one
  override per user/permission; separate PermissionId/CreatedByUserId FK indexes.
- BusinessDays has Guid PK, BranchId, DateOnly BusinessDate, OpenedAt/ClosedAt,
  checked Open/Closed Status, optional opening/closing users and CreatedAt. Unique
  (BranchId, BusinessDate) and partial unique BranchId WHERE Status='Open'. Lifecycle
  check requires closed timestamp after opening; an open day cannot have a close stamp.
- Nullable BusinessDayId on Shifts, SaleTransactions, Payments, Refunds, Expenses,
  CheckIns, FinancialTransactions and ClassSessions. All FKs indexed and Restrict.
  Keep their actual timestamps. MembershipSubscriptionTransactions derives its day
  from SaleTransaction, as do Private/Activity purchase batches. Payment/refund parts
  inherit their parent event day. No redundant BusinessDate columns are stored.
- MembershipPlanCategories is an editable audience catalog (not enum): Guid ID,
  unique nonblank Name, nullable text Description, IsActive, CreatedAt. Nullable
  indexed MembershipPlans.MembershipPlanCategoryId FK Restrict preserves old plans.
  Existing PlanCategory Normal/Freelance and CityId access model remain unchanged.
- EmployeeLeaveRequests: Guid PK, EmployeeId, inclusive date FromDate/ToDate, reason,
  checked Pending/Approved/Rejected status, optional reviewer/time/notes, CreatedAt.
  Checks reject reversed dates and inconsistent review timestamps; indexes on
  (EmployeeId, FromDate, ToDate), Status and reviewer FK. BLL owns authorization,
  overlaps and preventing shift assignment during approved leave.
- ShiftEmployeeChanges: Guid PK, ShiftId, distinct OriginalEmployeeId and
  ReplacementEmployeeId, optional reason, required ChangedByUserId and ChangedAt.
  Shift/time index plus Employee/user FK indexes. Existing ShiftEmployees composite
  key and joined/left history are preserved. AuditLog remains a separate system audit.
- No Class/Activity/Freelance/Private duplication or redesign. Private transfer history
  supports the latest future-BLL 50% transferred-client work/expiry rule without a DB
  trigger or a hardcoded rate column. Existing financial snapshots remain intact.
- All new timestamps use timestamptz, dates use date and enums are checked strings.
  No fake data or automatic midnight/business-day inference was added.

## Membership benefits

| Table | Purpose | Foreign keys |
| --- | --- | --- |
| MembershipBenefits | Management-defined catalog: unique Code (128), unique Name (256), optional Description, IsActive, CreatedAt | None |
| MembershipPlanBenefits | Plan-specific GrantType and nullable Quantity; unique plan/benefit pair | MembershipPlanId → MembershipPlans; MembershipBenefitId → MembershipBenefits |
| MemberSubscriptionBenefits | Purchase/renewal grant snapshots: code, name, grant type, quantity, GrantedAt, CreatedAt; unique transaction/benefit pair | MembershipSubscriptionTransactionId → MembershipSubscriptionTransactions; MembershipBenefitId → MembershipBenefits |
| MemberBenefitUsages | Specific grant consumption, positive QuantityUsed, UsedAt, branch, optional actor/notes, CreatedAt | MemberSubscriptionBenefitId → MemberSubscriptionBenefits; BranchId → GymBranches; optional CreatedByUserId → AspNetUsers |

All four tables have Guid primary keys. All new FKs use Restrict. GrantType is a
checked string enum (`Quantity`, `Access`) on plan configuration and grant snapshots,
never on the catalog. Quantity requires a positive integer; Access requires NULL.
The unique transaction/benefit pair prevents duplicate grants for the same purchase.

Grants resolve their subscription through MembershipSubscriptionTransaction; no
redundant MemberSubscriptionId is stored. They reference neither the current plan
benefit row nor its mutable quantity/name. Removing plan configuration or deactivating
the catalog therefore leaves historical grants and usages intact. Catalog uniqueness
uses ordinary PostgreSQL case-sensitive equality; normalization is future BLL work.

The unique plan/benefit and transaction/benefit indexes cover their leading FK.
Benefit-side FK indexes support reverse queries. Usage indexes cover
(MemberSubscriptionBenefitId, UsedAt), (BranchId, UsedAt), and optional creator FK.
No redundant standalone leading-column index or low-selectivity IsActive index added.

Validity reuses subscription dates and source transaction PreviousEndDate, NewEndDate,
PurchasedDurationDays and real CreatedAt. No independent ValidFrom/ValidUntil engine
was added: these facts preserve purchase context but do not finalize expiration or
carryover policy. Remaining quantity is GrantedQuantity minus summed usage history.
There is no stored mutable balance, artificial Access quantity, payment linkage or
ActivityPackage benefit relationship. See [benefit alignment report](MEMBERSHIP_BENEFITS.md).
