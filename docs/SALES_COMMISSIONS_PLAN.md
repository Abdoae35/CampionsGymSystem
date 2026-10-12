# Sales commissions and independent targets — implementation plan

Status: source-contract inspection complete; implementation awaits the refund
allocation decision below. No commission domain or financial data has been changed.

## Verified baseline and existing capabilities

- Dedicated existing branch: `SalesCommissions&Targets`, based on
  `5244d48226e9122558081869cadf8f2a97837d6c`. Local and remote feature, master and
  completed Shifts/Vault heads agree. Initial working tree was clean.
- Existing API → BLL → EF/PostgreSQL, capability/Allow-Deny authorization,
  trusted branch context, effective BusinessDay, AuditLog and decimal money remain.
- Reuse `CommissionPeriod` (branch, timestamps, Open/Closed, closure actors),
  `CommissionRule` (branch/optional employee, SalesNew/SalesRenewal, target/rates,
  effective dates), and `EmployeeCommissionSummary` (period/employee/category,
  eligible amount, target, applied rate and finalized commission).
- Payment already has nullable CommissionPeriodId. SaleCategory already distinguishes
  NewMembership/Renewal. Sale ownership, eligibility, branch, plan and commercial
  snapshots are immutable. Managed Payments/Refunds/parts and ledger are append-only.
- Current sale category uses customer subscription existence, and is omitted on
  House sales. It therefore needs prospective purchase-history classification,
  independently of ownership, including pending/reserved/cancelled prior purchases.
  Existing sale facts must not be silently rewritten.
- CashDrawer coordination precedes Member/Sale locks; owned active shifts are
  required for interactive collections/refunds. Preserve exact retry behavior.
- Employee resignation closes ownership assignments; termination date is cleared
  on reactivation. Payment-time commission attribution needs durable evidence so
  later status/ownership changes cannot erase earlier earnings.
- Guarded read-only localhost Development inspection found zero rules, periods,
  summaries, Sales and Payments. No configured commission default conflicts exist.
  Commission.View/ManageRules/ClosePeriod are active catalog capabilities with
  existing grants. Offers.Manage/Installments.Manage/Sales.QuickCancel remain active
  but ungranted; do not change unrelated grants or direct overrides.

## Confirmed calculation rules

New and Renewal are independent targets and rate calculations. Suggested initial
configuration is 30,000 each; New 5%/7%, Renewal 3%/5%. These values are explicit
configurable inputs, not an automatic overwrite of existing rules. Target attainment
applies the higher rate to all eligible collections of that category in the period.

Count completed Payment.Amount once, never payment parts or installment allocations.
Preserve the original purchase category for deposits and later installments. House,
Freelance and standalone visits do not contribute to employee targets/commissions.
Attribution uses the commercial employee, not cashier. Collections through the last
working day remain earned; later collections become House. Ownership reassignment
does not transfer historical contract earnings or selling-branch attribution.

Otherwise eligible collected money retained after cancellation remains eligible in
its original category. Actual refunds reduce it. Preserve existing CancellationFee
accounting/history; never credit retained money again. The old commission exclusion
in Sales documentation/results must be updated prospectively without changing ledger
or historical audit facts.

## Minimal domain and integration proposal

- Extend existing periods with nullable explicit inclusive BusinessDate boundaries
  and mutation identity/version where necessary; legacy timestamps remain unchanged.
  Define branch periods explicitly, require contiguous/non-overlapping boundaries,
  and retain one Open period per branch. No calendar-month assumption or fake backfill.
- Reuse effective-dated CommissionRule for two independently configured employee
  categories. New rule revisions apply from the next applicable period, never
  recalculate a current/finalized period. Retain immutable configuration history
  and snapshot targets/rates used by finalized summaries.
- Record minimal immutable payment-time attribution evidence linked uniquely to
  Payment, including category, employee or House/exclusion reason and employment
  eligibility. Use existing source facts for legacy rows where reproducible; expose
  unknown legacy facts explicitly instead of inventing actors/dates/periods.
- Classify new customer purchases while holding the existing global Member row lock:
  prior real customer purchase history survives cancellation, expiry, plan/branch
  changes; standalone and employee entitlement do not reset/create customer cycles.
- Reuse finalized EmployeeCommissionSummary; add only necessary rule/source line
  references/snapshots for immutable, reproducible reconciliation. No second ledger,
  payroll, payout state, commission clawback or monthly financial-closing engine.
- Narrow finalization uses existing Closed lifecycle and audit. Coordinate with
  collections/refunds to prevent close-versus-money races. Reject explicitly any
  mutation that would invalidate finalized commission source facts; do not rewrite
  finalized statements. Exact late-refund handling depends on allocation below.

## Blocking source-policy decision: partial refund allocation

Refund has optional OriginalPaymentId, but current manual RefundRequest accepts
only Sale, amount, outbound account parts and reason. RefundCore does not populate
OriginalPaymentId; Quick Cancel also aggregates source-account parts across Payments.
No existing refund-to-collection allocation/history or ordering policy was found.

A Sale may have payments in different commission periods, or employee-attributed
payments before resignation and House payments afterward. A partial refund therefore
cannot be assigned to a period/employee without a business allocation rule. Selecting
oldest/newest/proportional/explicit payments produces different target attainment and
commission. Outbound Cash/InstaPay accounts do not identify the original collections.

Decision required: should manual partial refunds reduce newest collected Payments,
oldest collected Payments, or explicitly selected original-payment allocations?
Explicit allocations are recommended for transparent source reconciliation. Whichever
policy is confirmed, preserve original Refund/Payment/ledger facts, cap each collection
at its unrefunded amount, and guard finalized affected periods. Quick Cancel's full
reversal has no partial allocation ambiguity. Do not implement an inferred policy.

## BLL/API, integrity and verification plan

Concrete CommissionService with thin authenticated controllers: branch period
creation/history/details/finalization, employee rule revisions/history, live period
progress, bounded source lines and employee/branch statements. Reuse Commission.View,
Commission.ManageRules and Commission.ClosePeriod; rule administration requires
global capability and trusted branch scope. No client-earned commission override.

Responses separate gross collections/refunds/retained amounts, New/Renewal,
employee/House/exclusions, target attainment/commission, and Open estimates/finalized
snapshots. Bound list/source queries; do not duplicate existing Sales financial APIs.
Money numeric(18,2), rates numeric(5,2); round each employee/category's aggregate
commission once to two decimals using AwayFromZero, then sum categories. Preserve
Guid IDs, actual UTC instants, BusinessDate, actor, branch and operation identities.

Tests: independent 20k New + 30k Renewal = 2,500; exact boundaries/rate changes on all
category money; first purchases/cancelled history/concurrent purchases; deposits and
installments across periods; split tender/retries counted once; 4k retained with
zero/1k/4k refunds; resignation/House/reassignment; excluded products; different branch
boundaries/midnight; future-only configuration/finalized integrity; forbidden actions,
Deny overrides and branch isolation; collection/refund/finalization races. Run full
existing Sales/Shifts/Subscription/Check-In suite, build, EF drift, reviewed new
migrations and guarded Development-only migration verification. Add Postman and
docs/SALES_COMMISSIONS.md with actual contracts and rollout prerequisites.

Delivery: verified planning checkpoint; rules/domain; attribution; refund-aware
eligibility; calculation/statements; API/tests/docs. Build/test/diff/commit/push each
working checkpoint. Production, payroll/payouts, full monthly closing, private-coach
finance, discretionary bonuses, frontend and offline sync remain untouched/deferred.
