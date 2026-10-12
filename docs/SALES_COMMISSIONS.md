# Sales commissions and independent targets

API → CommissionService / existing SalesService → EF/PostgreSQL. This feature
calculates commission; it does not pay salaries, commission or bonuses, and does
not perform branch monthly financial closing. The [plan](SALES_COMMISSIONS_PLAN.md)
records source inspection and the confirmed refund policy.

## Two independent categories

Each employee has New Clients and Renewals rules for each selling branch's period.
Targets and rates are independently configurable. Suggested initial setup:

| Category | Target | Below target | At/above target |
|---|---:|---:|---:|
| New Clients | 30,000 EGP | 5% | 7% |
| Renewals | 30,000 EGP | 3% | 5% |

These are explicit setup inputs, not silently seeded money/rules or a global hardcoded
calculator. Existing configurations are preserved. Owner may configure branch fallback
rules (`employeeId: null`) and employee-specific overrides. Employee rules take priority.
Configure fallback rules before the first period so newly hired/unconfigured employees
have an authoritative rule. A missing applicable rule is shown as `configured: false`,
with no invented commission; finalization requires both rules for attributed employees.

Attainment is `eligible collections >= category target`. Its higher rate applies to
**all** that category's eligible money. One target never changes the other's rate:

- New 20,000 against 30,000 → 5% → 1,000.
- Renewals 30,000 against 30,000 → 5% → 1,500.
- Total 2,500. New 30,000 instead gives 7% on all 30,000 → 2,100;
  Renewal 20,000 still receives 3% → 600.

Numeric money is (18,2); percentages (5,2). Calculate each employee/category aggregate
once, then round to two decimals with `MidpointRounding.AwayFromZero`; sum the two
rounded amounts. Never round individual PaymentParts into separate commissions.
Zero collections earn zero and do not achieve a positive target.

## Purchase category and attribution

NewMembership means the first-ever customer gym purchase; Renewal means every later
customer purchase. Registration alone creates no Sale or commission. New Sales inspect
all prior GymMembership Sales, including pending/reserved/cancelled purchases, across
branches; standalone visits are excluded. Existing non-employee/non-standalone customer
subscription history is conservative legacy evidence of an earlier membership cycle.
Employee entitlement is excluded. The existing global Member row lock serializes
concurrent purchases, so both cannot become New. The classification is saved on the
Sale and copied into its collection evidence; subsequent installments stay New or
Renewal according to that same Sale. Original legacy sale categories are not rewritten.

Attribution is the original commercial Sales employee snapshot, not the cashier,
current owner, employee's current working branch, or Member's registration branch.
Reassignment affects future Sales only. A returning/cancelled/expired Member remains
Renewal for the new owner. Employee branch transfers never move historical revenue.

CommissionCollection preserves the actual Payment's employee/category/exclusion
evidence. House/unassigned collections earn no employee commission/target contribution.
Collections through an employee's last working day remain earned; later installments
of their old contract are House. Existing termination dates use the gym-local calendar
date of the actual PaidAt timestamp; a forgotten old BusinessDay cannot revive rights.
Commission periods still use the separately preserved effective Payment BusinessDate.
Historical employment-status audits preserve ended-contract evidence after rehire clears
the current termination field. Rehire does not restore old contract installment rights.

Freelance fixed gym fees and standalone paid visits remain excluded, even if their
Member has a Sales owner. Existing explicitly ineligible product facts remain excluded.
No Member ownership change, Coach commission or payroll mutation is performed.

## Actual collections, retained cash and refunds

Count completed Payment.Amount once. Cash, InstaPay, split parts and installment
allocations reconcile the same Payment; they are not extra revenue. Unpaid installments,
unpaid reservations and Sale face value contribute nothing. A deposit contributes
actual received money even while reservation access is still pending.

Cancellation does not forfeit otherwise earned commission. Collected 4,000:

| Actual refund | Retained eligible money |
|---:|---:|
| 0 | 4,000 |
| 1,000 | 3,000 |
| 4,000 | 0 |

The financial CancellationFee meaning remains intact. No extra credit/new Payment is
created. Existing source/audit facts remain historical. Sale details now expose retained
eligibility according to its original unrefunded eligible collections.

Owner confirmed **newest collections first**. Order completed Sale payments by actual
PaidAt descending, then deterministic .NET Guid comparison descending for timestamp
ties. Consume still-unrefunded amounts, working backward only as necessary. Never
reduce a payment below zero. Each new Refund appends CommissionRefundAllocation links
to the affected Payments. Retries reuse the original Refund and allocations. Source
employee/House, category, branch and BusinessDate stay attached to the original money.

Example: October 4,000, November 4,000, November refund 1,000 → October stays 4,000;
November is 3,000. If the newest collection is House after resignation, its refund
reduces House first rather than the former employee's earlier eligible money.
OPEN estimates recompute attainment/rate on retained collections. A New category
falling from 30,000 to 29,999 after refund changes 7% to 5% on the whole retained sum.

**Owner confirmed the finalized-period boundary:** if any allocated portion reaches
a finalized source period, reject the whole refund before money, grant cancellation,
debt reopening, audit or allocation commits. Do not skip that newest collection to
reach an older/different period. Zero-refund cancellation can preserve all retained
money without changing a finalized statement. No negative payout/clawback or salary
deduction is created. A separately approved future adjustment policy is deferred.

## Periods, configuration and finalization

Periods belong to a selling/collection branch, with explicit inclusive StartDate and
EndDate. Examples may end on the 23rd, 25th or 30th; no calendar-month grouping or
automatic closing is hardcoded. One period is Open per branch; the next starts on
the previous EndDate + 1. Different branches have independent boundaries. Timestamp
bounds use the configured gym timezone; actual CreatedAt/ClosedAt remain UTC instants.

CommissionRule revisions append history. Initial configuration is explicit and cannot
be backdated; subsequent revisions must use the next applicable period's StartDate
(current/latest period EndDate + 1). EffectiveFrom/revision selects the applicable
employee rule or branch fallback at the period start. New settings never recalculate
the current period. Initial/fallback setup must precede opening the first period.

New Payment.CommissionPeriodId is linked only when a matching managed period exists;
money can be recorded before period setup, keeping its real BusinessDate and attribution.
Later explicitly defined periods can report those dated sources without updating their
null period links. Collections into an already finalized BusinessDate reject atomically.

Finalize only after the branch's effective BusinessDate exceeds the period EndDate.
A stale open BusinessDay prevents premature close. Finalization stores immutable
CommissionStatementLine reconciliation and the existing EmployeeCommissionSummary
with rule ID, target, below/reached rates, applied rate and commission snapshots.
Closed responses read these snapshots. No Paid status, payout, reopening endpoint or
competing financial/monthly closing is added. Exact finalize retry is idempotent;
other stale versions return 409. Viewing/configuration/period administration need no
employee shift. Existing Sales/payment/refund own-shift requirements still apply.

## API and authorization

All routes require authenticated capabilities and trusted branch scope. Normal users
omit BranchId and use their current branch. Global users may explicitly select a branch.
Owner administration is expressed through capability plus Branches.AccessAll; no role
name checks or client-earned commission overrides. Existing Allow/Deny overrides remain.

| Method | Route | Permission |
|---|---|---|
| POST | /api/commissions/rules | Commission.ManageRules + Branches.AccessAll |
| GET | /api/commissions/rules | Commission.View |
| POST | /api/commissions/periods | Commission.ClosePeriod + Branches.AccessAll |
| GET | /api/commissions/periods | Commission.View |
| GET | /api/commissions/periods/{id} | Commission.View |
| GET | /api/commissions/periods/{id}/employees/{employeeId} | Commission.View |
| GET | /api/commissions/periods/{id}/sources | Commission.View |
| GET | /api/commissions/periods/{id}/refund-allocations | Commission.View |
| POST | /api/commissions/periods/{id}/finalize | Commission.ClosePeriod + Branches.AccessAll |

Lists use page/pageSize (1..100). Rules additionally select employeeId (null selects
branch defaults) and branchId; period history selects branchId. Source lines optionally
filter employeeId, including their original commercial Sales attribution for transparent
House/exclusion traces. Existing Sales details/payment/refund routes remain authoritative
financial histories. Allocation history returns RefundId, PaymentId, amount, original
collection date and actual refund date/time.

Period progress distinguishes gross collections, refunds **allocated to those source
collections** (not a second cash-flow report), retained money, employee eligible money,
House New/Renewal totals, excluded money, separate employee category targets/rates and
earned commission. Branch totals include all employees; the Employees array is paginated
with EmployeeCount. `finalized: false` denotes an estimate. Employee statements expose
both categories, retained amounts, total commission, PeriodId/Finalized and original
contract House/excluded amounts; source routes show individual exclusions.
Source lines identify Payment/Sale/Member,
original seller versus entitled employee, branch, BusinessDate/PaidAt, gross, refund,
retained and eligible amounts. No phone/private profile graph is returned.

Initial/future configuration example (select actual dates and employee):

```json
{
  "operationId": "e6cfcb7d-cf9c-457a-bbc9-faf86a7518ae",
  "employeeId": "bd14dd18-d641-42c1-aa0c-e175e487c522",
  "effectiveFrom": "2026-10-12",
  "newClients": { "target": 30000, "belowRate": 5, "reachedRate": 7 },
  "renewals": { "target": 30000, "belowRate": 3, "reachedRate": 5 }
}
```

Opening example: `{ operationId, name, startDate, endDate, branchId? }`.
Finalize example: `{ "operationId": "...", "expectedVersion": 0 }`.
JSON mutation contracts reject unknown authoritative fields. Existing ProblemDetails:
400 business/invalid bounds or finalized-source refund rejection, 403 denied capability/
scope, 404 missing record, 409 stale/concurrent mutation. Preserve payload and OperationId
on retries; change operation identity for a distinct action.

Example progress response excerpt for the confirmed 20k New / 30k Renewal case
(IDs, source lines and additional fields omitted):

```json
{
  "finalized": false,
  "grossCollections": 50000,
  "refunds": 0,
  "retainedCollections": 50000,
  "employeeEligibleCollections": 50000,
  "houseCollections": 0,
  "excludedCollections": 0,
  "newClientEligibleCollections": 20000,
  "renewalEligibleCollections": 30000,
  "totalCommission": 2500,
  "employeeCount": 1,
  "page": 1,
  "pageSize": 25
}
```

The employee's `newClients` reports target 30000, targetReached false, appliedRate 5,
commission 1000; `renewals` reports targetReached true, appliedRate 5, commission 1500.
Their statement also returns PeriodId and Finalized so an estimate is never confused
with a frozen statement. A separate source request provides the underlying Payments.

## Integrity, database and rollout

Mutation transactions/savepoints reuse shared drawer coordination before Member/Sale
locks. Initial commission setup can lock the branch before a drawer exists. Period/
rule mutation and finance share coordination when configured, protecting close versus
payments/refunds. Member locks protect cross-branch first purchases. Unique source/
operation keys and append-only guards prevent duplicate evidence/allocations/statements.
Multi-query production reports use repeatable-read snapshots. No distributed locks,
second money ledger or changes to collected financial values.

New migrations:

- 20261012013412_AddSalesCommissionEvidence: three evidence tables; nullable period,
  rule and summary fields; restrictive FKs, source/operation indexes, exact precision.
- 20261012015229_SeparatePurchaseCategoryFromOwnership: relaxes only the obsolete
  House-category-null constraint; House still requires no SalesEmployeeId. No row backfill.

Reviewed SQL: [20261012_SalesCommissions.sql](database/20261012_SalesCommissions.sql).
Both applied only to guarded localhost Development. Old migration files remain intact.
Legacy null Shift/period/date facts remain null. Legacy complete attribution uses existing
sale/employment evidence; dated sources with missing category/eligibility are explicitly
excluded as LegacyOrExcluded, not guessed. A Payment with no BusinessDate cannot be
assigned to a period and remains outside these period calculations pending explicit
historical reconciliation. Legacy refunds with OriginalPaymentId retain that source;
otherwise newest-first allocation is derived read-only. The allocation endpoint lists
new explicit allocation records; legacy derived amounts appear in source reconciliation.
Legacy periods without BusinessDate bounds require explicit reconciliation and cannot
be silently adopted or reopened through these APIs. Schema downgrade can reject the old
House constraint after new House categories exist; never erase history to force rollback.

Existing Commission.View/ManageRules/ClosePeriod catalog permissions are reused. No
existing grant or direct override was changed. Development already grants view to two
roles and management/close to one; exact intended user provisioning remains administration.
Offers.Manage, Installments.Manage and Sales.QuickCancel still have zero grants/overrides.
This unrelated setup gap is preserved. No default commission records, periods, money or
fake application data are seeded. Configure branch defaults/employee overrides and first
period explicitly before rollout. Apply migrations only after verifying Development.

Audit: Commission.RuleConfigured, PeriodOpened, PeriodFinalized, RefundAllocated,
plus existing Sale/Payment/Refund/retained-fee events. Original seller, actor, branch,
operations, rules, source IDs and financial histories remain traceable. Guid identities,
immutable source facts, UTC instants and effective dates support future synchronization
without an outbox or sync subsystem.

Postman folder 17 adds ten intentional authenticated configuration/period/report examples
with stable operation variables. Dates are illustrative; choose actual boundaries and
reset operation IDs for new operations. Examples were not run against application data.

## Validation and deferred scope

Targeted configuration/collection/API/concurrency tests and migration preservation
tests cover independent targets, actual installments, split tender/retries, retained
cancellations, newest-first refunds, last-day/resignation/rehire, House, cross-branch
purchase history, first-purchase races, midnight, rounding, rate snapshots, capability
overrides and finalized-refund races. Final full suite: **772 passed, zero failed or
skipped**, including 39 new cases above the independently verified 733 baseline.
Latest targeted commission/historical migration run: 44 passed; four committed-connection
race cases rerun successfully. Build zero warnings/errors; EF no pending model changes.
Development catalog verification confirms both migrations, three evidence tables, seven
indexes and 14 validated constraints; all new FKs restrictive and precision correct.
All 42 historical migration files match the completed backend baseline byte-for-byte.
No production database is accessed.

Deferred: payroll, actual commission payout, salary deductions, late-refund adjustment
workflow, full branch/monthly closing, private/coach/class/activity commission expansion,
bonuses, transfer of old contract earnings, frontend and offline synchronization.
