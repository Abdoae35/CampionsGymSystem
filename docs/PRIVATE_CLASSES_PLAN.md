# Private, Classes and coach earnings implementation

Baseline: clean `PrivateCoaching&Classes` at
`0bc4476ef184dd95a4f771624d4a8860c00f1332`; dedicated implementation branch
`PrivateCoaching-Classes-Implementation`. Baseline full suite passed 772/772, zero failures/skips; baseline EF drift none.

## Concrete integration

1. Extend existing Private batches with original coach, start date, name and exact
   remainder snapshots; extend PrivateSessions with assignment, operation identity,
   BusinessDate and delivered value. Preserve legacy unknowns and old migrations.
2. Add Classes packages and sale-linked shared session subscriptions. Reuse
   GroupClass/ClassSession and CheckIn for direct class attendance; require a linked
   Classes subscription for new operational attendance. No reception prerequisite.
3. Purchase through SalesService's existing Payment/PaymentPart/financial posting
   helpers and existing full-payment/installment machinery. Refund through the
   existing refund route; revoke unused entitlements and preserve delivered evidence.
4. Lock drawer, member and subscription in the existing financial lock order.
   Serialize attendance, transfer, refunds and earnings recognition. Unique request
   identities, occurrence/member attendance and Private reception-check-in guards
   prevent retry or alternate-operation double consumption.
5. Explicit independent coach rule configuration and immutable recognition evidence
   use existing branch CommissionPeriods, without Sales New/Renewal target reuse.
   Target thresholds/basis, collection timing and Classes earnings are required
   configuration, with no business defaults. Original Private coach earns 50/60;
   any incoming assignment on an already purchased package earns fixed 50.
6. Owner expiry recognition preserves quantity/value/coach/allocation/reason evidence;
   never inserts PrivateSessions or CheckIns. The Owner confirmed all unused
   sessions belong to the coach assigned at expiry; transferred assignment is fixed 50%.
7. Partial payouts consume recognized outstanding earnings, with immutable payout
   allocations and balance snapshots. Reuse Expense and FinancialTransaction debit,
   configured shared cash drawer and physical/account balance checks exactly once.
   Owner capabilities use effective permission resolution and global-owner scope;
   do not silently alter role grants.
8. Authorized controller APIs, bounded search/history/reporting, documentation and
   real Postman payloads; focused PostgreSQL and HTTP tests plus complete regression,
   clean build, EF drift/forward SQL and historical migration integrity checks.

## Confirmed Owner decisions

- All unused expired sessions go to the coach assigned at expiry. Incoming transferred
  assignments receive fixed 50%; no attendance is fabricated.
- Private target basis is actual net funded delivered/authorized expiry value.
  Unfunded delivered value is deferred; thresholds are explicit per coach/branch
  commission period. Normal rates are 50/60. Transferred earnings stay fixed 50.
- Classes targets use average actual attendees per completed occurrence. Thresholds
  are configurable without defaults; Classes compensation remains disabled until
  the Owner confirms a formula.
- Refunds affecting finalized Private earnings reject atomically pending an explicit
  Owner adjustment policy. Historical evidence and payouts are never recalculated.

## Verification/publication

Commit and push each verified logical checkpoint. No old migration edits, no
production or application database updates. Tests use isolated PostgreSQL schemas.
Refresh PROJECT_HISTORY.md and AGENTS.md at checkpoints. Record actual results,
failures, migration names, routes and commit/push status in the final report.
