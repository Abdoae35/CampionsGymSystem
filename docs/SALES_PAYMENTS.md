# Sales / Payments / Reservations / Installments / Refunds

Implemented scope: customer gym membership sales, Cash/InstaPay collections,
reservations, flexible installments, paid activation, standalone visitor entries,
manual cancellation/refunds and permission-controlled Quick Cancel. API → BLL →
ApplicationDbContext/PostgreSQL; completed Subscription and Check-In rules are reused.

## Financial facts and branch attribution

`SaleTransaction` records what was sold and its immutable original price, Offer
discount and final amount. `Payment` records actual money received; creating a Sale
does not mean it is paid. `PaymentPart` identifies each exact financial account and
amount. A 1200 Sale paid with 700 Cash and 500 InstaPay is one Sale and one Payment
with two parts, not two Sales. Subsequent payments have their own immutable parts.

The authenticated operational branch owns the Sale, Payments, Refunds and ledger
movements. An authorized global user can explicitly select a branch. A Member
registered in A can buy B's plan in B; registration branch and Sales ownership stay
unchanged. The plan must be offered by the selling branch. Subscription access uses
the sold City snapshot and permits appropriate same-city branches, independently
of registration, financial origin and the Sales employee's working branch.

Amounts use decimal/numeric(18,2). Positive payment parts must total the Payment
amount exactly. No negative, meaningless zero payments, overpayment, wallet credit
or arbitrary client price override is accepted. New global-managed Cash/InstaPay
accounts have zero opening money. Account method, branch and opening facts are
immutable; account balances can be derived from the opening fact and ledger.
Payment credits and refund debits are appended to `FinancialTransaction`. No fake
Shift or CommissionPeriod is created; those links remain null until later workflows.

Actual timestamps, branch and effective BusinessDate/BusinessDay are preserved.
An open branch BusinessDay determines operations after midnight; otherwise the
configured gym timezone supplies the business date. UTC calendar dates are not
used blindly. History preserves unknown legacy business dates as null.

## Pricing and Offers

Backend pricing validates active customer plan, selling branch, plan audience,
Freelance coach/Member eligibility and configured Offer conditions. No client final
price, Sales employee, commission amount or entitlement balance is authoritative.

Every configured Offer explicitly chooses `General` or `MemberOnly`:

- General requires no MemberOfferEntitlement and ignores Member registration branch.
- MemberOnly additionally requires a currently valid, unrevoked existing
  MemberOfferEntitlement. Management grants/revokes through that existing model.
- An optional audience reference uses the existing dynamic MembershipPlanCategory
  catalog. Dated MemberAudienceCategory links express qualification; gender strings
  or age are not inferred and there is no new Gender enum.
- Plan audience eligibility remains authoritative. An Offer can narrow a plan's
  eligibility, never broaden it. Audience restriction and MemberOnly are independent.

Offer status, branch, linked plan, date period and discount/extra-day configuration
are enforced. Fixed and percentage discounts must leave a positive payable price;
extra days must be positive whole days. Legacy unclassified Offers fail closed until
authorized management explicitly configures eligibility, rather than inferring it
from entitlement rows. Management requires `Offers.Manage` plus branch/global scope.
Member audience administration requires `Members.Manage` at the registration branch.

Sales snapshot original/final price, Offer reference/name/type/value, eligibility and
audience facts, and extra days. `SaleMembershipSnapshot` and `SaleBenefitSnapshot`
also preserve sold plan name/type, branch/access City, mode, duration, configured
sessions, freeze allowance, Freelance coach, audience and benefits. Editing or
deactivating a plan/Offer/benefit later does not rewrite the Sale or delayed grant.
Quantity benefits retain granted quantity; access benefits retain null quantity.

## Full payment and activation

FullPayment requires exact full collection. Finance calls the assembly-internal
`ActivatePaidSaleAsync` in the same database transaction as money and snapshots.
Qualification is rechecked against real completed Payments. A financial or entitlement
failure rolls back the entire operation. There is no raw HTTP paid subscription grant
or renewal route; legacy Freelance purchase routes remain disabled.

New/expired memberships support:

| Choice | Behavior |
|---|---|
| Now | Start on the qualifying operation's effective BusinessDate. |
| SpecificDate | Future entitlement, no early access; if reservation completion is later than the chosen date, start on final-payment BusinessDate. Original chosen date remains historical. |
| FirstCheckIn | Fully qualified undated Sale, with no artificial future subscription date. Access preview reports PendingFirstCheckIn and readiness. First legitimate entrance atomically creates the dated grant and CheckIn. |

First-entry activation validates Member, visited branch, snapshot City, branch closure
and overdue obligations through the authoritative Check-In flow. A blocked branch
never starts the membership. Actual visited BusinessDate/BusinessDay starts the period,
even when different from the selling branch's operational date; financial origin stays
unchanged. Duplicate scans activate only once. Pending/future entitlements cannot
overlap ambiguously; one qualified pending-first-entry purchase is supported per Member.

Confirmed early renewals apply immediately regardless of activation choice. Same-mode
Unlimited appends duration; SessionBased appends time and carries active remaining
sessions plus the new grant. Different mode uses the existing successor scheduling
rules. Expired unused sessions remain historical and do not carry forward. Inclusive
end dates grant exactly the configured duration (start + duration − 1).
A compatible same-mode early renewal also shifts its existing queued successor by the
added duration. It does not create a second future period; another conflicting future
purchase is rejected and its financial writes roll back.

## Reservations and deposits

Reservation requires a positive deposit and can accept multiple later payments.
DueDate is optional; passing it does not automatically cancel the reservation.
Partial collection grants no subscription/access. Full collection atomically applies
the selected activation mode. Offer expiry or later product edits do not change the
reserved commercial terms. Balance is final price minus actual collections.

## Installments

Creation additionally requires `Installments.Manage`. Define 1..100 positive obligation
lines with arbitrary business due dates, including multiple dates within a month and
across months. Their sum equals final price minus the actual initial tender. Specify a
positive `InitialPaymentRequired`, or use a positive initial tender as the threshold.
Zero-money installment creation records no Payment and grants no access. Real positive
collections must meet the agreed threshold before applying the activation choice.

Later payments explicitly allocate their entire amount to one or multiple obligations.
`InstallmentPaymentAllocation` preserves exact history; duplicate lines, foreign Sale
obligations and over-allocation are rejected. Operational paid/status caches are not
authoritative: access and query status derive from actual completed allocations.

An unpaid required obligation with DueDate **before** the effective BusinessDate is
overdue. Check-In denies with `InstallmentOverdue`, including pending-first-entry
purchases. Paying all overdue obligations restores access immediately without extending
EndDate. Suspension never cancels membership or grants compensation for customer lateness.

## Normal / House / Freelance attribution

Normal Sales snapshot the current dated Sales owner or House at creation. Later owner
changes do not rewrite that attribution. Owned Normal membership Sales carry explicit
commission/target eligibility; House has no invented owner. Future commission buckets
can use the snapshotted employee and actual Sale branch. No commission is calculated.

Freelance Sales record only the plan's fixed gym amount (for example 300), never the
Coach's privately collected money. They preserve Sales-owned Members and House Members
exactly, and explicitly exclude commission and targets. Existing owner-preserving
managed Sales are accepted by the Freelance gym-revenue query; legacy financial
history retains its recorded attribution. Employee gym entitlement remains free and
employment-bound through its existing protected workflow; Employee plans cannot be sold
as paid customer plans.

## Standalone one-session visits and borrowed debt

`StandaloneSingleSession` purchase intent uses an eligible one-session SessionBased
plan without a new behavioral PlanType. Exact payment creates a new `IsStandaloneVisit`
entitlement with one entry and the configured valid date period. It supports activation
choices and cannot overlap another live/future entitlement. Check-In consumes its one
session atomically as a normal paid Gym entry, never a free Trial.

Standalone visits are excluded from commission and targets, preserve ownership, do not
settle borrowed debt and do not close/reset the original expired membership cycle.
Their exhausted/expired rows never become new borrowing cycles or membership renewal
sources. A later true renewal can settle the outstanding debt.

Ordinary membership activation/renewal fully settles effective outstanding gym debt.
SessionBased deducts all debt from the new session contribution (12 − 2 = 10); Unlimited
settles without quantity deduction. New grant quantity below debt rejects, with no
partial settlement or negative balance. Creation and final activation both validate
capacity; a reservation cannot guarantee that no new debt will occur before completion.

Borrow allowance remains two actual entries per specific expired subscription cycle.
Original BorrowedGymSession entry and first-settlement fields remain historical.
Ordered append-only BorrowedDebtSettlementEvent records express settlements/reversals:
debt → settled by Sale X → X normally cancelled → outstanding again → later resettled.
Effective debt uses the latest event, falling back to original state for legacy rows.
Normal cancellation reopens only debt most recently settled by that cancelled grant;
it never deletes a settlement or reopens another Sale's settlement. Debt history exposes
this sequence. A debt-settling grant is materially effective and cannot Quick Cancel.

## Manual cancellation, refunds and retained revenue

`Payments.Refund` plus Sale branch scope controls management's manually chosen refund.
The system does not calculate per-day or per-entry refund formulas. Zero, partial or
full amounts are accepted when financially possible; refund cannot exceed real collected
money minus earlier refunds. Positive refund parts must exactly identify outbound
Cash/InstaPay accounts and amount; zero cancellation has no parts or ledger movement.
Reasons, actors, business date/time and original Sale are preserved.

Cancelling an initial/separate grant stops its subscription immediately. Cancelling an
appended same-mode renewal reverses only that Sale's grant, preserving earlier paid
entitlement. SubscriptionGrantCancellation preserves the reversal and before/after
facts. Only unused contribution is removed from a shared period: session consumption
uses older grants first; already elapsed days are historical. Earlier cancellation
shifts are included in grant date allocation. Future successors follow remaining paid
entitlement; a successor which already started is not rewritten. Grant purchase facts,
benefits and CheckIns remain immutable. Cancelled benefits cannot redeem invitations or
supply normal freeze allowance. A later additional refund cannot reverse the grant twice.

Retained collected minus refunded money is actual branch CancellationFee revenue,
including retained reservation deposits. It is commission/target ineligible. It is not
credited a second time: original collection minus refund already records the retained
money. Original commercial eligibility snapshots stay historical; future Commission
must exclude cancelled Sales and use the explicit fee classification.

## Quick Cancel

`Sales.QuickCancel` is independent of refund permission and role names. It requires an
uncancelled Sale within exactly 24 actual hours of creation and an unused grant. CheckIns
on that period since grant creation, exact benefit usage, freeze/extension use and any
borrowed-debt settlement reject Quick Cancel. Pooled grant use checks deliberately fail
closed when an entry after the grant cannot be isolated; normal authorized cancellation
remains available. Earlier entries before a new grant do not block its cancellation.

Quick Cancel fully reverses collected money to the exact original source accounts,
including historical inactive accounts, with immutable Refund/parts/debit records.
It cancels current/future/undated purchased entitlement and preserves other paid grants.
It never deletes money/history. After 24 hours or material use, normal cancellation
with a manually chosen refund is required.

## API and authorization

| Method / route | Purpose | Capability |
|---|---|---|
| POST /api/sales | Full, reservation, installment or standalone Sale | Payments.Create; installments also Installments.Manage |
| GET /api/sales | Scoped paginated Sale history | Payments.View |
| GET /api/sales/{id} | Sold terms, balances, grant and bounded histories | Payments.View |
| POST /api/sales/{id}/payments | Additional payment and explicit allocations | Payments.Create |
| GET /api/sales/{id}/payments | Paginated payment history | Payments.View |
| GET /api/sales/{id}/installments | Actual obligation status | Payments.View |
| POST /api/sales/{id}/refunds | Manual cancellation/refund | Payments.Refund |
| GET /api/sales/{id}/refunds | Paginated refund history | Payments.View |
| POST /api/sales/{id}/quick-cancel | Exact unused recent reversal | Sales.QuickCancel |
| GET /api/sales/branch-totals | Cash/InstaPay gross, refunds and net | Payments.View |
| GET /api/financial-accounts | Active operational Cash/InstaPay accounts | Payments.View |
| POST /api/financial-accounts | Named zero-opening account provision | Branches.Manage AND Branches.AccessAll |
| GET /api/offers | Authoritatively eligible Member/plan Offers | Payments.Create |
| GET /api/offers/quote | Authoritative Member/plan/Offer price | Payments.Create |
| GET /api/offers/management | Scoped paginated administration catalog | Offers.Manage |
| GET /api/offers/{id} | Offer administration details | Offers.Manage |
| POST /api/offers | Create configured Offer | Offers.Manage |
| PUT /api/offers/{id} | Edit configured Offer | Offers.Manage |
| POST /api/offers/{id}/member-entitlements | Grant existing MemberOnly assignment | Offers.Manage |
| DELETE /api/offers/{id}/member-entitlements/{grantId} | Historical revocation | Offers.Manage |
| PUT /api/members/{id}/audience-categories | Dated dynamic audience qualification | Members.Manage at registration branch |

Normal staff are confined to their authenticated branch; global capability allows
explicit broader workflows. Sales ownership is not refund, Offer, installment or
cross-branch authorization. Mutation contracts reject unknown/authoritative fields.
Controllers use existing ProblemDetails conventions: 400 invalid business request,
403 denied scope/capability, 404 missing resource and 409 concurrency conflict.
Request enums use existing numeric JSON conventions; details use descriptive strings.
History pages are bounded to 100. Detailed payment/refund lists are the first 100;
dedicated paginated routes expose longer history. Legacy money remains readable with
unknown snapshot facts; mutation requires managed Sale reconciliation.

The explicit `--configure-sales-permissions` command runs only in Development against
localhost. It adds missing catalog codes and preserves activation, grants and direct
overrides. Optional `Sales:ManagementRoleId` must identify an existing globally capable
role. Existing staff receive no inferred grants. Demo templates apply only to newly
created roles; production authorization remains capability-based.

## BLL, audit, concurrency and future integration

SalesService provides CreateAsync, RecordPaymentAsync, RefundAsync, QuickCancelAsync,
GetHistoryAsync, GetDetailsAsync, GetPaymentsAsync, GetRefundsAsync,
GetInstallmentsAsync, GetBranchTotalsAsync, GetAccountsAsync and CreateAccountAsync.
OfferService provides CreateAsync/UpdateAsync/GetAsync/GetManagementHistoryAsync,
QuoteAsync/GetEligibleAsync, GrantMemberAsync/RevokeMemberAsync and
SetMemberAudiencesAsync. Paid activation, grant reversal, effective debt validation
and overdue checks stay inside SubscriptionService's protected integration boundary.

Existing AuditLogs record Sale.Created/Reserved/Completed/Cancelled/QuickCancelled,
Payment.Recorded/PartRecorded, Offer.Created/Updated/Applied, member grant/audience
changes, Installment.ScheduleCreated/Paid/OverdueResolved, Refund.Created,
Reservation.Cancelled/AmountRetained, Subscription.PendingFirstCheckIn/
ActivatedFromSale/GrantCancelled/ScheduleShifted, BorrowedSessionDebt.Settled/
SettlementReversed and account/catalog configuration. Actors, branches, references,
amounts, reasons and before/after facts are retained; read-only queries do not mutate.

PostgreSQL transactions/savepoints and the shared Member lock serialize payments,
borrow/debt, paid activation, freeze, CheckIn and cancellation. Sale row locks and
version checks protect balances and cancellation races. Plan/Offer/account locks
protect sold facts. Unique operation identities, exact actor/request hashes, unique
Sale-to-grant linkage and ordered debt/cancellation constraints prevent retry duplication,
double activation, over-allocation and over-refund. Session deduction and first-entry
activation remain in the same CheckIn transaction. No distributed locks are introduced.

Guid IDs, append-only money/parts/history, explicit branch/time/business date and
operation identity support later synchronization. No outbox, replication, cloud/local
routing or conflict engine is implemented. No commission/target calculation, payroll,
Coach commission, Shift/Vault closing, Expenses/monthly closing, Private/Classes/Activity
finance, Member branch financial settlement or frontend is added. Future Commission
must use immutable Sale branch/owner/product classification; future Shift/Vault uses
the real ledger and exact accounts. Stop here; those workflows remain separate features.

Database changes are new migrations only: PrepareSalesPayments,
AddOfferEligibilityAndAudience and AddSaleGrantCancellationHistory. SQL review artifacts
are in `docs/database/`; restrictive relationships, decimal precision, operation
uniqueness and historical compatibility are covered by PostgreSQL tests. Apply to
Development only after verifying the target; production is outside this task.

Postman folder 15 contains 32 deliberate examples. Keep operation UUIDs for identical
retries and change them for distinct actions; choose one real workflow at a time.

## Verification checkpoint — 2026-10-10

Build succeeded with zero warnings/errors. The final full suite passed all 678 tests,
with zero failures/skips (113 additional cases over the completed Check-In baseline).
EF reports no pending model changes. All three new migrations were applied only to
the verified localhost Development database. Catalog checks confirmed six new support
tables, 23 indexes and 32 validated constraints on those tables, restrictive new FKs,
18,2 money precision and unique Sale/Payment/Refund operation indexes. Production was
untouched. Development permission setup added the three missing management capabilities
without inferring grants for existing roles; assign those capabilities explicitly.

No known unresolved Sales / Payments issues.
