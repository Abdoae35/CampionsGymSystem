# Private coaching, Classes and coach earnings

Implemented in the existing API/BLL/DAL on `PrivateCoaching-Classes-Implementation`,
from baseline `0bc4476`. This document supersedes earlier Private/Classes financial
assumptions. Member collections and coach payouts are separate operations.

## Private packages and attendance

Catalog packages specify branch, name, session count, duration in days, price and
active status. A purchase creates a separate Private subscription, immutable sale
and purchased batch, plus its original coach assignment. It uses the existing
SalesService Payment/PaymentParts, installment obligations, financial accounts,
shift, drawer and FinancialTransactions. Full payment and scheduled installments
with a positive initial collection are supported; reservation and Offer pricing are
outside this package contract. Price comes from the catalog, never from the client.
No second collection ledger exists. Further collections use the existing
`POST /api/sales/{saleId}/payments`, including actual installment allocations;
refunds use `POST /api/sales/{saleId}/refunds`.

The purchase snapshots name, count, price, dates, original coach and unit value.
Unit value is price/count rounded **down** to cents; the remainder belongs to the
last session. EGP 100/3 yields 33.33, 33.33, 33.34. Catalog edits do not change
purchased history. Expiration is after the inclusive EndDate in the branch's
effective BusinessDate infrastructure. Each new purchase starts normal coach rules.

The assigned coach searches purchased members by normalized phone, MemberCode,
branch-local ClientCode or name and explicitly registers one delivered session.
Registration requires:

- An active member and eligible, unexpired, uncancelled Private balance in that branch.
- The authenticated active employee assigned as the current coach at that branch.
- A reception `Gym` CheckIn for the same member, branch and effective BusinessDate.
- A valid regular gym membership underlying that reception entry, with the correct
  access city, current dates, no freeze and no overdue obligation. Borrowed, Trial,
  standalone visits and employee memberships do not qualify.

Reception never consumes Private. A session-based gym membership whose final valid
session was just consumed by reception remains valid for that same-date Private
session if its dates and other conditions hold. Date-expired membership is rejected.
One explicit registration consumes exactly one Private session. One reception entry
can back at most one Private session. To record another actual session, obtain its
own legitimate reception entry. No target configuration is needed for attendance.

Transfers close the current assignment once and append a new audited assignment,
using ExpectedVersion. Actual sessions retain their delivered coach and value.
Every incoming assignment on that purchased package earns **fixed 50%**, including
transfer back to its original person. A newly purchased package starts normal rules.

## Expiration authorization, funding and targets

The Owner explicitly authorizes expired unused quantity with a reason and an open
branch commission period containing the authorization BusinessDate. **All unused
sessions belong to the coach assigned at expiry.** A transferred assignment uses
fixed 50%. Authorization records quantity, exact value/remainder, coach assignment,
branch/date, actor, timestamp and operation evidence. It creates no attendance,
check-ins, earnings payment or cash. Actual-used and unused-authorized quantities
remain separate; actual remaining counters are preserved. Each batch can be
settled once; retries return its existing evidence. Cancelled packages cannot settle.

Coach thresholds are explicit per employee and branch CommissionPeriod. Existing
`/api/commissions/periods` supplies the contiguous inclusive BusinessDate bounds;
Sales New/Renewal rules are never used as coach targets. Configure a positive
PrivateTarget and/or ClassesAverageTarget; neither has a default. Configuration is
immutable per coach/period, so supply both desired dimensions in one request and
configure a new period for changed thresholds. Periods need explicit date bounds;
legacy undated periods fail closed.

Private target progress is net collected commission-eligible value attributed to
actual delivered sessions and Owner-authorized expiry sources. Collections fund
sources in delivered session order, with expiry unused value after deliveries.
Only retained completed Payment amounts count, and PaymentParts must reconcile.
Existing newest-first refund allocations determine retained collections, including
refunds after evaluation end but before recognition. Unfunded source value is
reported as deferred; later collections can recognize another immutable slice of
that original source in a subsequent evaluation period. No session is recognized
twice and no unused face value counts before explicit expiry authorization.

For normal assignments, eligible funded value below target earns **50%**; reaching
target earns **60%** on that period's normal eligible value. Transferred funded value
contributes to its coach's target but its own earnings stay **50%**. A normal source
without a configured target has unavailable projected earnings and cannot finalize.
Fixed transferred earnings do not require a target. Missing settings never block
operational attendance. Money rounds to cents away from zero; cumulative source
rounding prevents extra cents from repeated partial funding.

Example: ten EGP 100 sessions; Ahmed delivers two, earning 100 at 50% or 120 at 60%.
Mohamed receives the purchased package and delivers eight, earning 400 at fixed 50%.
If only EGP 200 is collected, only EGP 200 of eligible source value is funded;
unfunded delivered value waits for later collection with its original attribution.

Owner recognition is explicit after the period's last effective BusinessDate ends.
Recognize preceding branch periods first, including zero-earnings periods.
Recognition snapshots source slices, rate, target/result and progress, with immutable
operation/audit evidence. It does not close the independent Sales calculation or
move money. Financial finalization and payout are separate. Refunds reducing net
collections below finalized Private eligible value reject atomically until an
Owner adjustment policy is defined, whether those earnings are unpaid or paid.
Existing finalized Sales-source refund protections also continue to apply.

## Partial payouts

Outstanding = recognized earned commission − actual coach payouts. The Owner may
pay any positive part up to the outstanding branch/coach balance. EGP 4,000 earned,
then 2,000 paid, then another 2,000 in a future month gives 4,000 → 2,000 → 0.
Payouts do not recompute targets or earnings.

Each payout stores authorizer, paid timestamp, BusinessDate, branch, linked Expense,
account/method and before/after outstanding snapshots; FIFO earning allocations
preserve which evidence it pays. It creates **one existing Expense and one debit**
FinancialTransaction. It requires the authorizer's own active employee shift, an
active branch account/category and sufficient available funds. Cash uses existing
physical shared-drawer availability; electronic accounts require actual book funds.
No opening funds, fictional cash, automatic categories or grants are created.
Generic expense reversal cannot reverse a coach payout without a defined adjustment
workflow. Historical source account type is immutable under existing account rules.

## Classes

Classes packages are distinct from Private and regular gym memberships. Purchased
sessions share one balance across Yoga, Zumba, CrossFit and any other active class
type at their purchased branch. Each registered member consumes one shared session
per occurrence, regardless of type. **No reception entry or gym membership is needed.**

Types reuse GroupClass. Managed occurrences reuse ClassSession with branch,
coach, operation, effective BusinessDate and version snapshots. Create an occurrence
with a future start on the current local calendar day; BusinessDate follows the
branch's effective operational day, including overnight operation. There is no
booking, waitlist or capacity subsystem. The assigned coach registers members
individually before start, using a selected eligible Classes subscription or earliest
expiry first. Active member, current package dates, uncancelled sale, positive balance
and no overdue package installments are required. Duplicate member/occurrence
registrations are rejected even across different packages or operation IDs.

Assigned coach completes after start with ExpectedVersion. Managed occurrence attribution is
immutable under tracked EF saves; only completion fields may change while scheduled.
Only unattended occurrences
can be cancelled; completed attendance is preserved. Old package-free class write
routes return 410 and cannot bypass the new consumption workflow. Historical legacy
class reports remain readable, but undated legacy occurrences do not enter new
BusinessDate target reports without explicit reconciliation.

Statistics count eligible **completed** occurrences, including zero-attendance classes,
and actual registrations: total attendance / completed occurrence count. No occurrences
yields zero. Counts 10, 8, 12 produce total 30, classes 3 and average 10. Reports accept
up to 367 days. Supply a matching period ID to compare an explicitly configured Classes
average threshold; omitted/missing threshold reports null target attainment.
`CompensationStatus = "NotConfigured"`: no Classes earnings formula is implemented.

## Authorization and endpoints

All routes below are under `/api/coaching`, require authentication and effective
capability authorization, including direct Deny overrides and existing branch/city
scope. Attendance additionally verifies the authenticated delivering employee.
Discretionary expiry authorization, targets, recognition and payouts require both
the listed capability **and Branches.AccessAll**, the existing Owner-authorized
management pattern. No role-name shortcut or changed default grants exists.

| Route | Capability |
|---|---|
| GET private/packages | Private.View |
| POST private/packages; PUT private/packages/{id} | Private.Manage |
| POST private/purchases | Private.Manage + Payments.Create |
| GET private/members?branchId&search | Private.View |
| GET private/subscriptions?branchId&memberId; GET private/subscriptions/{id} | Private.View |
| POST private/subscriptions/{id}/sessions | Private.RegisterSession |
| GET private/subscriptions/{id}/sessions | Private.View |
| POST private/subscriptions/{id}/transfers | Private.Transfer |
| POST private/subscriptions/{id}/expiry-settlements | Private.SettleExpiry + Branches.AccessAll |
| GET private/subscriptions/{id}/expiry-settlements | Private.View |
| GET classes/packages | Classes.View |
| POST classes/packages; PUT classes/packages/{id} | Classes.Manage |
| POST classes/purchases | Classes.Manage + Payments.Create |
| GET classes/members?branchId&search; GET classes/subscriptions?branchId&memberId | Classes.View |
| GET classes/members/{memberId}/attendance?branchId | Classes.View |
| GET classes/types | Classes.View |
| POST classes/types; PUT classes/types/{id} | Classes.Manage |
| GET classes/occurrences?branchId&from&to | Classes.View |
| POST classes/occurrences; POST classes/occurrences/{id}/complete | Classes.Manage |
| POST classes/occurrences/{id}/attendance | Classes.RegisterAttendance |
| GET classes/occurrences/{id}/attendance | Classes.View |
| GET classes/coaches/{coachId}/statistics?branchId&from&to[&periodId] | Classes.ViewAttendanceReport |
| POST earnings/targets | CoachEarnings.ManageTargets + Branches.AccessAll |
| GET earnings/periods/{periodId}/targets; GET earnings/periods/{periodId}/progress | CoachEarnings.View |
| POST earnings/recognitions | CoachEarnings.Recognize + Branches.AccessAll |
| GET earnings/coaches/{coachId}/balance?branchId | CoachEarnings.View |
| GET earnings/coaches/{coachId}/history?branchId; GET earnings/coaches/{coachId}/payouts?branchId | CoachEarnings.View |
| POST earnings/payouts | CoachEarnings.Pay + Branches.AccessAll |

Installment purchases additionally need Installments.Manage. Collections, purchases
and payouts require existing shift/account setup; attendance, configuration and
reporting do not require an employee cash shift. Lists/history use page=1 and
pageSize=25 by default; maximum pageSize=100. Response: `{items,totalCount,page,pageSize}`.
Invalid business input returns 400, denied scope 403, missing resource 404, and
concurrency/uniqueness conflict 409, following existing controller conventions.
Write DTOs reject unknown fields; client earnings, price overrides and arbitrary
attendance attributions are rejected.

## Request examples

IDs below are placeholders for real selected resources. Reuse OperationId and exact
body for a retry; generate a new UUID for a new intentional operation. Purchase
returns subscriptionId, saleId and paymentIds. Private details returns current version.

```json
{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "memberId": "22222222-2222-4222-8222-222222222222",
  "packageId": "33333333-3333-4333-8333-333333333333",
  "coachEmployeeId": "44444444-4444-4444-8444-444444444444",
  "purchaseMode": 0,
  "paymentParts": [{"financialAccountId": "55555555-5555-4555-8555-555555555555", "amount": 1000}]
}
```

Private registration: `{"operationId":"<new UUID>","gymCheckInId":"<reception ID>","notes":"Delivered"}`.
Transfer: `{"operationId":"<new UUID>","coachEmployeeId":"<incoming coach>","expectedVersion":2,"reason":"Transfer"}`.
Expiry: `{"operationId":"<new UUID>","commissionPeriodId":"<open period>","reason":"Recognize expired unused balance"}`.
Class registration: `{"operationId":"<new UUID>","memberId":"<member>","classesSubscriptionId":"<optional chosen package>"}`.
Targets: `{"operationId":"<new UUID>","commissionPeriodId":"<period>","coachEmployeeId":"<coach>","privateTarget":10000,"classesAverageTarget":10}`.
These target numbers are examples, never defaults. Recognition:
`{"operationId":"<new UUID>","commissionPeriodId":"<ended period>"}`.

```json
{
  "operationId": "66666666-6666-4666-8666-666666666666",
  "branchId": "77777777-7777-4777-8777-777777777777",
  "coachEmployeeId": "44444444-4444-4444-8444-444444444444",
  "amount": 2000,
  "financialAccountId": "55555555-5555-4555-8555-555555555555",
  "expenseCategoryId": "88888888-8888-4888-8888-888888888888",
  "reason": "Partial payment of recognized earnings",
  "receiptReference": "Actual receipt reference"
}
```

Postman folder **18** contains 37 requests covering all 36 routes plus a partial
collection purchase alternative, response ID/version capture and stable operation
initialization. Set confirmed numeric target variables, real IDs, dates and start
instant; login as the appropriate coach or Owner. Clear an operation variable only
when starting a different deliberate action. These examples were validated without
sending operations against application data.

## Persistence, rollout and verification

New forward migrations:

- `20261012101654_ImplementPrivateClassesAndCoachEarnings`: six new Classes/earnings/
  payout tables, extensions to existing Private/class/check-in evidence, indexes,
  precision and restrictive foreign keys; eight capability catalog rows only.
- `20261012104721_EnforceCoachingEvidenceCompleteness`: validates required operational
  snapshots and explicit normal-coach target facts, closing nullable check loopholes.

Both are **generated, not applied to the application or Production database**. Isolated
PostgreSQL tests apply them inside owned disposable schemas. Existing migration files
are unchanged. Review the forward SQL in `docs/database` and use your normal controlled
migration rollout. Grant new capabilities explicitly through trusted authorization
administration. Configure actual periods, coach thresholds, active catalog, accounts,
drawer, expense category and employee shifts. No defaults, historical backfills,
operational seed money or permission grants were created.

Legacy missing original assignment/value/date evidence fails closed for operational
Private mutations and requires explicit reconciliation. Do not fabricate past attendance
or infer old transfers. Append-only EF evidence guards and restrictive FKs preserve
history; raw/bulk SQL bypasses application guards and must follow existing runtime
access restrictions. Transactions/savepoints, consistent drawer/member locks, balance
versions, unique operations/attendance and advisory operation locks protect retries,
concurrent consumption, refund/recognition races and overpayment.

Final full regression: **822/822 passed**, zero failures/skips, 50 new cases over the
verified 772 baseline. Clean rebuild has zero warnings/errors; EF drift none; all
46 historical migration files are unchanged. Focused 84/84, migration/expiry/new-package
29/29 and class history 10/10 passed. Earlier fixture/resource failures and their
corrections are recorded in PROJECT_HISTORY.md. Test assembly
parallelism is capped at two isolated fixtures to respect local PostgreSQL lock-table
limits; competing transactions within concurrency tests still run in parallel.
No server memory/lock settings are changed. Tests cover
membership/reception prerequisites, one-session consumption, fixed transfer attribution,
50/60 targets, late funded slices, expiry without attendance, refund protection,
partial payouts/account integrity, shared Classes balances, averages, effective
permissions, branch scope, historical snapshots and committed PostgreSQL races.

The only undefined compensation decision is the Classes earnings formula; it remains
disabled. Refunds reaching finalized earnings and payout correction remain rejected
until the Owner defines an adjustment policy. Automatic expiry recognition and
booking/capacity/waitlists remain outside scope. The later
[Monthly Closing](MONTHLY_CLOSING.md) feature adds salary/payment records and frozen
branch-month reports, reusing these immutable coach earnings and actual payouts.
Classes compensation remains disabled; the frontend handoff is preserved separately.
