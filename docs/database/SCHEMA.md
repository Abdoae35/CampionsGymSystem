# Gym database schema

Current model: **77 application tables plus seven Identity tables (84 model tables)**.
The database also has EF migration history (85 tables including that bookkeeping table).
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
| Members | Member profile, unique MemberCode, nullable positive branch-scoped ClientCode (bigint), display phone and unique normalized phone | HomeBranchId → GymBranches, CreatedByUserId → AspNetUsers |
| MemberSalesAssignments | Current sales/house owner and assignment history | MemberId → Members, SalesEmployeeId → Employees, AssignedByUserId → AspNetUsers |
| Leads | Branch CRM prospect and conversion | BranchId → GymBranches, ConvertedMemberId → Members, CreatedByUserId → AspNetUsers |
| LeadAssignments | Sales ownership history | LeadId → Leads, SalesEmployeeId → Employees, AssignedByUserId → AspNetUsers |
| LeadInteractions | Contact and follow-up history | LeadId → Leads, EmployeeId → Employees, optional CreatedByUserId → AspNetUsers |
| MemberFollowUpTasks | Explicit Member contact work; owner derived from current Sales assignment | MemberId → Members, optional CreatedByUserId / CompletedByUserId → AspNetUsers |
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
LeadAssignments, LeadInteractions, MemberFollowUpTasks. Gym: MembershipPlans, MemberSubscriptions,
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

## Member legacy client identifier

`Members.Id` remains the Guid/UUID primary key and the target of every member foreign
key. `HomeBranchId` is reused as the original registration branch: no duplicate
RegistrationBranchId field is added. Visiting or purchasing at another branch must
not change this original registration context or historical identifiers.

`ClientCode` is nullable long/bigint for numeric legacy serial codes, reception lookup
and future Excel imports. The same number may occur in different registration branches.
Partial unique IX_Members_HomeBranchId_ClientCode enforces uniqueness of
(HomeBranchId, ClientCode) WHERE ClientCode IS NOT NULL. Multiple uncoded members may
remain null. CK_Members_ClientCode_Positive (>0) rejects zero/negative values when
present. New registrations allocate codes atomically using GymBranches.LastMemberClientCode
in their transaction; existing uncoded records remain null. Existing string MemberCode
and its separate unique index remain unchanged.

The previously applied 20261007182227_AddMemberClientCode remains unchanged.
New 20261007183558_ScopeMemberClientCodeByHomeBranch replaces only its global code
index and relaxes nullability, without rewriting member data. Down requires prior
manual reconciliation of nulls and cross-branch duplicate codes; it must never
invent substitute codes to recreate the old stricter schema. This change was applied
to local development only; production deployment remains separate.

Future import must preserve codes exactly and reject branch-local duplicates rather
than silently renumbering. Numeric storage assumes codes have no meaningful leading
zeros/letters; verify the legacy file and avoid floating-point conversion of large
integer codes. Future new-customer generation must be concurrency-safe per branch;
MAX+1 without serialization is unsafe. No generation, import, Member Management or
offline synchronization feature is implemented by this schema change.

## GuestInvitations — one-time free trial

GuestInvitation is an access entitlement, never a MembershipPlan or financial sale.
Guid Id identifies each issued record. Required InviteeName (200), InviteePhone
(32) and InviteePhoneNormalized (32) identify guests before Member creation.
InviteeMemberId is optional and may be linked later without changing HomeBranchId.
Source is a string-backed Gym/Member enum; Gym has no inviter or benefit source,
and Member requires InvitedByMemberId. SourceSubscriptionBenefitId optionally
references the specific MemberSubscriptionBenefits purchase grant batch.
BranchId is the issuing/operational branch; the actual visit branch is on CheckIn.
CreatedByUserId and Notes are optional.

Status is string-backed Available/Redeemed/Cancelled/Expired. CreatedAt,
RedeemedAt and CancelledAt use UTC timestamptz; redemption/cancellation timestamps
must match status and cannot precede creation. RedeemedCheckInId is optional and
unique when present. All six foreign keys use Restrict: two Members references,
MemberSubscriptionBenefits, GymBranches, CheckIns and AspNetUsers.

Partial unique indexes reserve InviteePhoneNormalized WHERE Status = 'Redeemed',
and InviteeMemberId WHERE Status = 'Redeemed' AND InviteeMemberId IS NOT NULL.
Multiple available/cancelled/expired records are permitted. A PostgreSQL trigger
rejects deleting a redeemed row or changing its redeemed status, normalized phone,
redemption timestamp, or already-linked Member/CheckIn. Null Member/CheckIn links
may be populated later; unique/FK constraints still apply. This protects history
even through bulk/direct SQL under ordinary runtime DML privileges.

CheckInType.Trial = 4 is stored as 'Trial'. CheckIns.MemberId is nullable only
when CheckInType = Trial; every existing type still requires MemberId and its
existing subscription/class relationship. Trial has no subscription/class and
does not consume a paid session. No financial relationships were added.
Migration Down requires explicit reconciliation of Trial check-ins and nullable
MemberIds before rollback; it does not invent member IDs. Down is destructive
rollback of the new table and was not applied.

No existing phone normalization strategy was found. This task stores the supplied
canonical phone (required, nonblank, trimmed); future BLL must define and share a
single normalization policy across guest/member matching before enabling issuance.
No separate normalization algorithm is introduced by this domain migration.

## Employee gym membership support

Employee.MemberId is nullable, uniquely indexed and references Members with
Restrict deletion. Employees can exist before a profile is linked; no automatic
profile creation or personal-data copying is introduced.

PlanCategory.Employee (value 2, stored as 'Employee') is a behavioral/system type,
independent of the dynamic MembershipPlanCategory audience. Employee plans require
Price = 0, Unlimited access, no session count, no Freelance Coach and
DurationDays = 0 (no fixed duration). Normal/Freelance still require positive
duration and retain their previous rules. No deterministic plan row is seeded.
Employee category itself marks internal-only entitlement plans; future normal
purchase/catalog services must exclude it.

MemberSubscription.EmployeeId is an optional restricted Employees FK. Non-null
identifies employment entitlement and preserves its historical owner independent
of later plan/profile edits. Future BLL must ensure Employee.MemberId matches the
subscription MemberId and the plan is Employee type; cross-table matching is not
a CHECK constraint. The explicit relationship avoids deriving old ownership from
a mutable plan or current employee profile link.

EndDate is nullable only for employee periods. Open employee periods are
Scheduled/Active, Unlimited, without counters or EndDate; closed employee periods
are Expired/Cancelled with a real EndDate >= StartDate. Normal/Freelance subscriptions
still require EndDate. Unique partial indexes WHERE EmployeeId IS NOT NULL AND
EndDate IS NULL enforce one open period per employee and per member. Historical
closed periods remain alongside new rehire periods; no future year is fabricated.
EmployeeId/StartDate and the original unfiltered MemberId lookup support history.

Subscriptions already allow zero purchase transactions. Employee memberships
therefore create no purchase batch, sale, payment or revenue; required paid
transaction FKs and dates remain unchanged. Paid transaction category snapshots
continue accepting only Normal/Freelance. The existing Freelance renewal entry
point rejects employee-owned periods rather than converting an entitlement.

CheckIns is unchanged: use Gym + MemberSubscriptionId + linked MemberId, with
SessionConsumed false. Future BLL verifies Active employment and employment dates.
EF SaveChanges protects employee period ownership/start/plan, rejects deletion,
and prevents modifying closed periods. This tracked guard does not intercept
raw SQL/bulk writes; production write permissions and future services must preserve
history. Restrict FKs protect referenced employee/member/plan/check-in history.

EmploymentStatus remains Active/Resigned/Suspended. Permanent departures currently
use Resigned and TerminationDate; no separate Terminated API/status is introduced.

## Member Management implementation (2026-10-08)

Migration 20261008110432_AddBranchClientCodeSequenceSupport adds nullable
GymBranches.CountryCallingCode (1–3 digit calling code with +), nonnegative bigint
LastMemberClientCode default 0 and nullable Members.PrimaryPhoneNormalized (16).
Three basic checks validate these fields. PrimaryPhoneNormalized has a globally
unique partial index where nonnull; PrimaryPhone's index becomes nonunique so
identical local displays can belong to different countries. Existing Guid PK,
branch-local ClientCode unique index and one-current-assignment constraint remain.
Counters start above existing codes; only unique unambiguous international phones
are backfilled. Branch calling codes/local legacy phones are never guessed.
Applied to local development only; no Members existed to reconcile. No new tables.
See [Member Management](../MEMBER_MANAGEMENT.md) for API, ownership, security,
normalization, deployment and verification details.

## CRM / Leads implementation (2026-10-08)

Existing Lead/assignment/interaction tables are reused. Lead.Phone remains display;
PhoneNormalized is a persisted globally unique nullable legacy-safe canonical value.
FullName is now optional for phone-only intake; Member name remains mandatory.
Lead gains SecondaryPhone, Notes, NextFollowUpAt, LostAt/LostReason and ConvertedAt.
Interactions gain nullable legacy-safe ContactedAt and CreatedByUserId (Restrict FK).

MemberFollowUpTasks has explicit Member FK, reason, due/notes, creating/completing
user/timestamps, Restrict FKs and valid reason/completion checks. Lead scheduling
stays on Lead; task owner derives from current MemberSalesAssignment. No duplicate
owner/House flag, polymorphic subject ID or generic task engine was introduced.

Partial uniqueness protects Lead.PhoneNormalized, Lead.ConvertedMemberId, current
LeadAssignments (EndedAt null) and open Member/reason work (CompletedAt null).
Lead branch/status/schedule and assignment employee/end indexes support queues.
Migration 20261008143406_AddCrmLeadManagement adds this support;
20261008162710_AllowPhoneOnlyCrmLeads changes only Lead name nullability. Safe unique
phones are backfilled; ambiguous legacy data stays preserved. Business transitions
and privacy rules are enforced in BLL rather than retroactively inventing legacy
lifecycle timestamps. See [CRM workflows](../CRM_LEADS.md) and reviewed SQL export.
