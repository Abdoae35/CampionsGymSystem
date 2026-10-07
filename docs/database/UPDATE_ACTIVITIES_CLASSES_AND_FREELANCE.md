# Activities, Classes and Freelance rule update

## Existing implementation and conflicts

Inspection covered entities/configurations, context, two prior migrations, all BLL/API
code, commission/payment relationships and database documentation. Previously there
were 62 application tables plus seven Identity tables. Only authentication services
and endpoints existed; no business purchase, reporting, payment or commission service
was replaced. No duplicate repository/infrastructure/package was introduced.

| Existing structure | Conflict | Smallest change |
| --- | --- | --- |
| ActivityCoachAssignment.CoachEmployeeId required | External coaches cannot be represented | ActivityCoach profile, optional new FK; retain old Employee link for reconciliation |
| ActivitySubscription optionally links an assignment | Historical financial attribution could depend on mutable configuration | Current selected coach plus purchase-batch coach/rate snapshot |
| Generic Activity/Class CommissionRules allow target/fixed formulas | Activities now fixed percentage; Class compensation undecided | Preserve old rules; new calculation bypasses them; no Class payout |
| CheckIns has GroupClassId, no actual occurrence | Empty classes/substitutes cannot be counted reliably | ClassSession and optional occurrence link on CheckIns |
| Freelance plans have coach/price snapshots | No explicit eligibility or enforced House sale | Eligibility junction, House sale snapshot and transactional assignment transition |
| Refund.OriginalPaymentId optional | Ambiguous activity installment/period allocation | Activity reports reject unlinked refunds until explicitly reconciled |
| No external financial summary | Employee-only payroll cannot represent external coach debt | ActivityCoachSettlement; Employee summary bridge for internal coaches only |

## Files and components

- DAL: four entities/configurations (`ActivityCoach`, `ActivityCoachSettlement`,
  `ClassSession`, `MembershipPlanEligibleMember`); two enums; existing assignment,
  subscription/batch, CheckIn and SaleTransaction entities/configurations; context,
  one new migration/metadata and current snapshot. Old migrations unchanged.
- BLL/Operations: `OperationAccess` (permission/branch/audit), `ActivityManagementService`,
  `SubscriptionPurchaseService`, `FreelancePlanService`, `ClassSessionService`,
  `CoachReportService`; request/response records colocated with their services.
- API: `OperationsController` and scoped DI in Program.cs. Existing authentication,
  authorization and error conventions reused. Business validation is also in BLL.
- Tests: ActivitiesClassesFreelanceTests, ActivityRuleMigrationTests; existing auth
  HTTP tests extended, seeder tests made catalog-size aware; schema fixture supports
  selecting a previous migration for upgrade checks.
- Docs: SCHEMA, BUSINESS_RULES_NOT_ENFORCED_BY_DB, DATA_FLOWS, this rollout guide,
  generated forward SQL. No new package or test project.

## API surface

All requests require Bearer authentication plus the listed permission. Business
branch checks also run in BLL; only the existing global Owner behavior bypasses
branch restriction, never permission checks. Validation returns 400, missing grants
403, concurrent uniqueness/serialization conflicts 409.

| Method/path (under /api) | Permission |
| --- | --- |
| POST activities/coaches | Activities.ManageCoaches |
| PUT activities/coaches/{id}/percentage | Activities.ManageCoaches |
| POST activities/{id}/coach-assignments | Activities.ManageCoaches |
| DELETE activities/coach-assignments/{id} | Activities.ManageCoaches |
| POST activities/purchases | Activities.Manage |
| GET activities/coach-revenue/{periodId} | Activities.ViewCoachRevenue |
| POST activities/coach-revenue/{periodId}/finalize | Commission.ClosePeriod |
| POST classes/sessions | Classes.Manage |
| PUT classes/sessions/{id} | Classes.Manage |
| POST classes/sessions/{id}/attendance/{memberId} | Classes.Manage |
| GET classes/attendance-report?branchId=...&from=...&until=... | Classes.ViewAttendanceReport |
| POST freelance-plans | FreelancePlans.Manage |
| PUT freelance-plans/{id} | FreelancePlans.Manage |
| PUT freelance-plans/{id}/eligible-members/{memberId} | FreelancePlans.ManageEligibleMembers |
| DELETE freelance-plans/{id}/eligible-members/{memberId} | FreelancePlans.ManageEligibleMembers |
| POST freelance-plans/purchases | FreelancePlans.Manage |
| GET freelance-plans/{id}/report?branchId=...&from=...&until=... | FreelancePlans.View |

Enums in these request records use the existing numeric JSON default: ActivityCoachType
Internal=0/External=1, ClassSessionStatus Scheduled=0/Completed=1/Cancelled=2,
AccessMode Unlimited=0/SessionBased=1. Reports use [from,until) intervals and UTC.
A purchase accepts an optional ExistingSubscriptionId for renewal. There is no arbitrary
retroactive coach reassignment endpoint; selecting another coach on renewal only
changes future batches. Payment posting/access activation remains outside this update.

Examples:

```json
{"coachType":1,"employeeId":null,"externalName":"Boxing Coach","externalPhone":null,"percentage":30}
```

```json
{"memberId":"<guid>","activityPackageId":"<guid>","activityCoachId":"<guid>"}
```

```json
{"cityId":"<guid>","coachEmployeeId":"<guid>","name":"Coach Monthly","price":300,"durationDays":30,"accessMode":0,"sessionCount":null,"isActive":true}
```

```json
{"memberId":"<guid>","membershipPlanId":"<guid>","branchId":"<guid>"}
```

## Accounting and historical choices

The configured rate is snapshotted at **purchase/renewal**, not collection time.
All installments/refunds for that purchase retain its coach/rate. Changed rates apply
to subsequent batches. Coach type/Employee identity and batch attribution are immutable
through tracked EF saves; raw/bulk SQL requires separate operational protection.

Activity reports count each completed Payment.Amount once and validate PaymentParts.
They never sum payment amount after joining its parts. Refunds must identify their
original payment and reduce that payment's period while open. Subscriber count is
unique paid members per coach/activity/rate row (including a fully refunded purchase).
Multiple purchase rates may produce multiple rows; entitlements round per row to two
decimal places, midpoint away from zero. No targets or CommissionRules lookup applies.

Closed periods require an explicit reviewed finalization after EndAt is fixed and all
records are complete. The settlement stores names, type, Employee, rate, revenue and
entitlement. Internal rates aggregate into one EmployeeCommissionSummary per employee/
activity, with AppliedRate null when multiple rates apply. External settlements answer
amount owed without fake Employees or payroll rows. Finalization never closes/reopens
a whole period or moves money. Existing employee summaries block automatic duplicates.
Even reopening a period will not recalculate its existing activity snapshot.

Class reports use completed actual occurrences and their CheckIns. Completed empty
classes count; scheduled/cancelled classes do not. Statistics belong to the occurrence's
actual Employee. **Class financial compensation is BUSINESS RULE NOT FINALIZED**.
Legacy Class/Freelance/Activity generic commission configuration is preserved but not
expanded or used for these new formulas. No class financial summary is generated.

Freelance membership records only the gym plan amount. The focused purchase endpoint
uses configured Price and accepts no manual discounts; existing OfferMembershipPlans
is still the sole future system-offer targeting mechanism. Eligibility is checked in
BLL on every purchase/renewal. Current Sales ownership ends and a House row is appended,
without altering earlier assignments/sales. House sales have no SalesEmployeeId or
SaleCategory; they cannot contribute to SalesNew/SalesRenewal attribution. Removing
eligibility does not affect earlier purchases. No global member/coach unique constraint.

The Freelance report is branch-scoped: eligibility counts members with that HomeBranch;
purchases/revenue use the sale/payment/refund branch. Historical purchases group by
snapshotted coach, even after plan reassignment. Net gym cash in a range may be negative
if refunds in that range relate to money received earlier. Legacy sales without House
snapshots require explicit reconciliation rather than silently appearing as new House sales.

## Migration and deployment

New migration: `20261007000549_UpdateActivitiesClassesAndFreelanceRules`.
Forward SQL: [UpdateActivitiesClassesAndFreelanceRules.sql](UpdateActivitiesClassesAndFreelanceRules.sql).

Adds four tables and six nullable columns on existing tables; makes legacy assignment
CoachEmployeeId nullable. Adds restricted FKs, coach/rate/type/time checks, composite
reporting indexes and unique eligibility/occurrence-attendance/period-settlement indexes.
The assignment SportsActivityId single-column index is replaced by the composite with
the same prefix. No existing table/column is dropped by Up. Eight new permission codes
are inserted idempotently and granted to an existing Owner role. Existing inactive
permissions/custom other-role grants are preserved. Down intentionally retains permission
catalog entries; schema rollback drops new data and must not be run casually.

No migration is applied to the user's local development or production database by this
update. Integration tests apply migrations only in isolated transactional PostgreSQL
schemas and roll everything back. Review the pending older Private/Freelance migration
separately: it has a deliberate reconciliation guard for existing Private history.

Before deployment, review the SQL and migration state, then explicitly approve/apply the
pending migrations using the normal deployment process. [EF Core recommends reviewing
migration SQL before execution](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying).

Manual reconciliation, where historical rows exist:

1. Create ActivityCoach profiles for known coaches with management-approved percentages.
   No fake coach or assumed historical percentage is seeded automatically.
2. Explicitly reconcile old Employee-only assignments/subscriptions. Purchase coach/rate
   snapshots remain null until a reviewed data migration establishes the historical facts;
   reports refuse eligible legacy payments without them.
3. Reconcile refunds without OriginalPaymentId and ambiguous multiple batches per sale.
4. Link old class CheckIns to verified real occurrences; never manufacture zero/attended
   classes from check-in timestamps alone.
5. Configure Freelance eligibility explicitly. Never infer eligibility or rewrite old
   Sales ownership from current plan state. Review legacy House sale attribution separately.
6. Grant new permissions to appropriate management roles; only Owner receives baseline
   new grants automatically if that role already exists. Fresh development seeding includes
   all 45 permissions, with conservative existing non-Owner defaults unchanged.

## Remaining business decisions

- Class payout formula: intentionally unresolved and not implemented.
- Late refunds/reopened activity snapshot corrections: require a future authorized
  adjustment workflow; never silently rewrite finalized payroll/settlements.
- Mid-batch activity coach changes are not allowed by these endpoints. Renewal can
  select another coach; existing purchases remain attributed to the original coach.
- Payment reception, ledger posting, access activation and system-offer evaluation
  remain their separate business workflows. No speculative accounting engine was added.

## Verification

- Build before migration and after implementation: zero warnings/errors.
- Final full suite: **103 passed, 0 failed, 0 skipped** (42 additional cases).
- Real PostgreSQL transactional schemas exercised the migrations, constraints,
  reporting SQL, purchase/assignment/audit paths and HTTP authorization.
- Legacy Employee-only activity assignments/subscriptions and class CheckIns survived
  upgrade with their new links null. No synthetic historical coaches/classes created.
- Forward migration SQL inspected: no table/column drops, four new tables, six nullable
  added columns, one existing FK made nullable; no unrelated schema changes.
- EF has-pending-model-changes: no drift. git diff --check: clean.
- User development/production database was not migrated. No class payout, new payment
  engine, payroll redesign, vendor module or unrelated business CRUD implemented.

## Exact changed/created paths

- `AGENTS.md`
- `CampionsSystem.API/Controllers/OperationsController.cs`
- `CampionsSystem.API/Program.cs`
- `CampionsSystem.BLL/Authentication/DevelopmentAuthSeeder.cs`
- `CampionsSystem.BLL/Operations/ActivityManagementService.cs`
- `CampionsSystem.BLL/Operations/ClassSessionService.cs`
- `CampionsSystem.BLL/Operations/CoachReportService.cs`
- `CampionsSystem.BLL/Operations/FreelancePlanService.cs`
- `CampionsSystem.BLL/Operations/OperationAccess.cs`
- `CampionsSystem.BLL/Operations/SubscriptionPurchaseService.cs`
- `CampionsSystem.DAL.Tests/ActivitiesClassesFreelanceTests.cs`
- `CampionsSystem.DAL.Tests/ActivityRuleMigrationTests.cs`
- `CampionsSystem.DAL.Tests/AuthTestDatabase.cs`
- `CampionsSystem.DAL.Tests/AuthenticationApiTests.cs`
- `CampionsSystem.DAL.Tests/DevelopmentAuthSeederTests.cs`
- `CampionsSystem.DAL/Configurations/ActivityCoachAssignmentConfiguration.cs`
- `CampionsSystem.DAL/Configurations/ActivityCoachConfiguration.cs`
- `CampionsSystem.DAL/Configurations/ActivityCoachSettlementConfiguration.cs`
- `CampionsSystem.DAL/Configurations/ActivitySubscriptionConfiguration.cs`
- `CampionsSystem.DAL/Configurations/ActivitySubscriptionTransactionConfiguration.cs`
- `CampionsSystem.DAL/Configurations/CheckInConfiguration.cs`
- `CampionsSystem.DAL/Configurations/ClassSessionConfiguration.cs`
- `CampionsSystem.DAL/Configurations/MembershipPlanEligibleMemberConfiguration.cs`
- `CampionsSystem.DAL/Configurations/SaleTransactionConfiguration.cs`
- `CampionsSystem.DAL/Data/ApplicationDbContext.cs`
- `CampionsSystem.DAL/Entities/ActivityCoach.cs`
- `CampionsSystem.DAL/Entities/ActivityCoachAssignment.cs`
- `CampionsSystem.DAL/Entities/ActivityCoachSettlement.cs`
- `CampionsSystem.DAL/Entities/ActivitySubscription.cs`
- `CampionsSystem.DAL/Entities/ActivitySubscriptionTransaction.cs`
- `CampionsSystem.DAL/Entities/CheckIn.cs`
- `CampionsSystem.DAL/Entities/ClassSession.cs`
- `CampionsSystem.DAL/Entities/MembershipPlanEligibleMember.cs`
- `CampionsSystem.DAL/Entities/SaleTransaction.cs`
- `CampionsSystem.DAL/Enums/ActivityCoachType.cs`
- `CampionsSystem.DAL/Enums/ClassSessionStatus.cs`
- `CampionsSystem.DAL/Migrations/20261007000549_UpdateActivitiesClassesAndFreelanceRules.Designer.cs`
- `CampionsSystem.DAL/Migrations/20261007000549_UpdateActivitiesClassesAndFreelanceRules.cs`
- `CampionsSystem.DAL/Migrations/ApplicationDbContextModelSnapshot.cs`
- `PROJECT_HISTORY.md`
- `docs/authentication/LOCAL_API_TESTING.md`
- `docs/database/BUSINESS_RULES_NOT_ENFORCED_BY_DB.md`
- `docs/database/DATA_FLOWS.md`
- `docs/database/SCHEMA.md`
- `docs/database/UPDATE_ACTIVITIES_CLASSES_AND_FREELANCE.md`
- `docs/database/UpdateActivitiesClassesAndFreelanceRules.sql`
