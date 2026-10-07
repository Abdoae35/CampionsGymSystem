# Final business schema alignment

## Inspection and schema-gap report (before implementation)

Inspected all 66 application entities/configurations, enum storage, Guid Identity
context/registration, three existing migrations and their snapshot, FK delete behavior,
indexes/checks, and schema/business-rule/flow documentation. Existing feature code is
preserved; this operation adds no controllers, services, permission handler or workflows.

| Latest rule | Before status | Gap / decision |
| --- | --- | --- |
| Role inheritance | Already Supported | Identity user-role tables and RolePermissions exist |
| Direct user Allow/Deny | Missing | Add UserPermissionOverrides; resolution remains later work |
| BusinessDay and operational transaction date | Missing | Add day entity and nullable operational FKs |
| Subscription actual timestamp + business date | Partially Supported | CreatedAt exists; derive purchase BusinessDay via its SaleTransaction |
| Dynamic audience classification | Missing | Add MembershipPlanCategories, nullable MembershipPlan FK |
| Flexible plan price/duration/access/sessions/freezes/status | Already Supported | Preserve typed relational fields |
| Allowed city/branch access | Already Supported | MembershipPlan.CityId, MemberSubscription.CityId and branch Cities exist; no new access junction |
| Employee leave requests | Missing | Add request/review storage; no manager hierarchy |
| Shift replacements | Partially Supported | ShiftEmployees has join/leave times, but no replacement/actor/reason fact |
| Actual ClassSession and delivering coach | Already Supported | Reuse actual session, optional schedule and completed/cancelled statuses |
| Class occurrence attendance and averages | Already Supported | CheckIns.ClassSessionId and unique occurrence/member index exist |
| Class financial compensation | Already Supported (intentional absence) | Rule NOT FINALIZED; no payout formula added |
| Activity internal/external coach | Already Supported | Reuse ActivityCoach with checked Employee/external shape |
| Activity fixed percentage and history | Already Supported | numeric(5,2), range check, immutable purchase coach/rate snapshots |
| Activity subscription coach attribution | Already Supported with legacy transition | New purchases select coach; old unknown links/snapshots remain nullable for reconciliation |
| Actual paid activity financial source | Already Supported | Sale/Payment/PaymentParts/Refund relationships exist; no duplicate payment model |
| Freelance coach-specific plans and eligibility | Already Supported | Existing plan coach FK, eligibility uniqueness, no global member-coach lock |
| Freelance purchase history and House facts | Already Supported | Category/coach/price snapshots and sale House attribution exist |
| Private session/batch/coach/check-in/period history | Already Supported | All required explicit FKs and timestamps exist |
| Private transfer and expiry / transferred-client 50% | Already Supported (facts) | Assignment time history, actual coach/session time, expiry coach/time and configurable financial snapshots; document latest BLL rule only |
| Auditability | Already Supported | Existing AuditLog supports type/entity ID/actor/branch and JSON history |
| Future offline prerequisites | Already Supported | UUID business IDs, branch FKs where owned, real event timestamps/history; no sync system added |
| Conventions/indexes/migration safety | Already Supported | Preserve Fluent configurations, string enums, exact decimals and restricted FKs |

## Implementation decisions

Five new tables: BusinessDays, UserPermissionOverrides, MembershipPlanCategories,
EmployeeLeaveRequests, ShiftEmployeeChanges. Junction overrides use composite Guid
(UserId, PermissionId) PK, matching existing RolePermissions convention. Other new
entities use Guid Id PK. No duplicate override Id/index is needed. Future AuditLog entries can use UserId as
EntityId and include both UserId/PermissionId in JSON to identify this composite key.

BusinessDayId is nullable on Shifts, SaleTransactions, Payments, Refunds, Expenses,
CheckIns, FinancialTransactions and ClassSessions. Direct event links preserve the
actual operational day even if collection/refund occurs on a different day from sale.
Ledger adjustments need their own link; actual classes need one even with zero attendees.
MembershipSubscriptionTransactions, PrivateSubscriptionTransactions and activity
purchase batches use their SaleTransaction.BusinessDayId; do not duplicate the same
purchase day. PaymentParts/RefundParts inherit their parent's day. Subscription dates
remain explicit, and real CreatedAt/OccurredAt/CheckedInAt timestamps are untouched.

No BusinessDate copies, midnight triggers or automatic historical day inference.
Existing links remain null until a reviewed reconciliation establishes the actual day.
Future BLL must assign day/branch consistently and prevent later financial history edits.

Classification is independent of PlanCategory Normal/Freelance. Name is unique and
nonblank; no Male/Female/Kids rows are automatically assigned or hardcoded. Existing
plans may remain unclassified. Classification rename/deactivation must not change the
meaning of stored purchase facts; if historical audience reporting is required, version
categories or snapshot them when that feature's explicit requirements are established.

ShiftEmployeeChanges stores original/replacement Employee, shift, actor, timestamp and
reason without rewriting the existing ShiftEmployees key. Multiple replacement events
may be recorded for the same shift; update attendance join/leave rows and append the
business change atomically in the future BLL. The event table supplements AuditLogs.

All new FKs are Restrict. Review/date/status/type checks and useful indexes are defined
in individual IEntityTypeConfiguration files. No cross-table business triggers.

Existing Freelance eligibility creator is required; this already supported structure
is retained rather than relaxed just because the example allowed a nullable creator.
No ManagerId, EAV fields, duplicate class/activity/eligibility tables, new payroll engine,
percentage trigger, offline node/outbox/inbox/conflict or replication tables added.

## Important runtime boundary

UserPermissionOverrides is **storage only** in this task. Existing permission resolution
still reads role permissions; direct overrides must not be presented as effective until
the dedicated authorization feature implements active permissions + Allows - Denies.
No auth handler, permission service, controller or business operation was changed here.
Likewise existing operations do not automatically assign BusinessDay or enforce approved
leave; these belong to their later vertical feature implementations.

Latest Private rule: transferred-client delivered work and expiry remainder use 50%
in future BLL. Existing assignment histories identify transfer context; PrivateSession
and expiry records preserve the earning coach/time, purchased batch, basis and period.
No hardcoded database rate/trigger or destructive change to existing private history.
This supersedes earlier guidance treating every Private rate as solely configurable.

## Migration and deployment

Migration: `20261007151235_FinalBusinessSchemaAlignment` after the three existing
migrations. Forward SQL: [FinalBusinessSchemaAlignment.sql](FinalBusinessSchemaAlignment.sql).

Up creates five tables and adds nine nullable FK columns (eight BusinessDay links,
one plan audience category), their indexes/constraints and restricted relationships.
No old table/column is dropped or renamed; no old data is altered or backfilled.
No fake day/category/override/leave/replacement rows or baseline classification seeds.
No applied/previous migration was edited. Down drops the newly added tables/columns
and therefore destroys their data; rollback requires explicit review and is not run.

The user's development and production databases are not updated by this task. Deploy
only after reviewing migration state, pending earlier migration reconciliation guards,
and generated SQL. Existing null day/category links remain explicitly unknown. New
vertical workflows should require and preserve day assignment where appropriate.

Remaining behavior is deliberately future work: override authorization resolution,
operational-day assignment/close logic, leave approval and shift-conflict validation,
and business audit writers. Class compensation remains unresolved. The previously
identified finalized-activity correction workflow is separate. None prevents storage
alignment; no speculative features or payment/accounting redesign were added.

## Exact changed/created files

- `AGENTS.md`
- `CampionsSystem.DAL.Tests/ActivityRuleMigrationTests.cs`
- `CampionsSystem.DAL.Tests/FinalAlignmentMigrationTests.cs`
- `CampionsSystem.DAL.Tests/FinalSchemaAlignmentTests.cs`
- `CampionsSystem.DAL/Configurations/BusinessDayConfiguration.cs`
- `CampionsSystem.DAL/Configurations/CheckInConfiguration.cs`
- `CampionsSystem.DAL/Configurations/ClassSessionConfiguration.cs`
- `CampionsSystem.DAL/Configurations/EmployeeLeaveRequestConfiguration.cs`
- `CampionsSystem.DAL/Configurations/ExpenseConfiguration.cs`
- `CampionsSystem.DAL/Configurations/FinancialTransactionConfiguration.cs`
- `CampionsSystem.DAL/Configurations/MembershipPlanCategoryConfiguration.cs`
- `CampionsSystem.DAL/Configurations/MembershipPlanConfiguration.cs`
- `CampionsSystem.DAL/Configurations/PaymentConfiguration.cs`
- `CampionsSystem.DAL/Configurations/RefundConfiguration.cs`
- `CampionsSystem.DAL/Configurations/SaleTransactionConfiguration.cs`
- `CampionsSystem.DAL/Configurations/ShiftConfiguration.cs`
- `CampionsSystem.DAL/Configurations/ShiftEmployeeChangeConfiguration.cs`
- `CampionsSystem.DAL/Configurations/UserPermissionOverrideConfiguration.cs`
- `CampionsSystem.DAL/Data/ApplicationDbContext.cs`
- `CampionsSystem.DAL/Entities/BusinessDay.cs`
- `CampionsSystem.DAL/Entities/CheckIn.cs`
- `CampionsSystem.DAL/Entities/ClassSession.cs`
- `CampionsSystem.DAL/Entities/EmployeeLeaveRequest.cs`
- `CampionsSystem.DAL/Entities/Expense.cs`
- `CampionsSystem.DAL/Entities/FinancialTransaction.cs`
- `CampionsSystem.DAL/Entities/MembershipPlan.cs`
- `CampionsSystem.DAL/Entities/MembershipPlanCategory.cs`
- `CampionsSystem.DAL/Entities/Payment.cs`
- `CampionsSystem.DAL/Entities/Refund.cs`
- `CampionsSystem.DAL/Entities/SaleTransaction.cs`
- `CampionsSystem.DAL/Entities/Shift.cs`
- `CampionsSystem.DAL/Entities/ShiftEmployeeChange.cs`
- `CampionsSystem.DAL/Entities/UserPermissionOverride.cs`
- `CampionsSystem.DAL/Enums/BusinessDayStatus.cs`
- `CampionsSystem.DAL/Enums/EmployeeLeaveRequestStatus.cs`
- `CampionsSystem.DAL/Enums/PermissionOverrideEffect.cs`
- `CampionsSystem.DAL/Migrations/20261007151235_FinalBusinessSchemaAlignment.Designer.cs`
- `CampionsSystem.DAL/Migrations/20261007151235_FinalBusinessSchemaAlignment.cs`
- `CampionsSystem.DAL/Migrations/ApplicationDbContextModelSnapshot.cs`
- `PROJECT_HISTORY.md`
- `docs/database/BUSINESS_RULES_NOT_ENFORCED_BY_DB.md`
- `docs/database/DATA_FLOWS.md`
- `docs/database/FINAL_SCHEMA_ALIGNMENT.md`
- `docs/database/FinalBusinessSchemaAlignment.sql`
- `docs/database/SCHEMA.md`
- `docs/database/UPDATE_PRIVATE_AND_FREELANCE.md`

## Verification result

- Build before migration and final build: zero warnings/errors.
- Final suite: **128 passed, 0 failed, 0 skipped**; 25 added schema/migration cases.
- PostgreSQL constraints, actual after-midnight timestamp/day preservation, legacy
  null transitions, replacement facts and model relationships verified in isolated
  transactional schemas. All fixture data/schema rolled back.
- Existing legacy activity upgrade test now applies through the current migration
  before querying the current EF model; no old migration was edited.
- EF pending-model check: no drift. Forward SQL and migration inspected: five tables,
  nine nullable FK columns; no Up drops/renames/alterations or fake-data backfill.
- API/BLL/Identity configuration and previous migration files remain unchanged.
  No new packages, endpoints, business services, triggers or sync components.
- Changes remain uncommitted/unpushed on DataBaseInit. No development/production
  migration applied. Database deployment is a separate reviewed operation.
