# Rules for the BLL

Enforce these rules in authorized BLL services and transactional operations,
not controllers, entity methods, or database triggers.

## Access and attendance

- Borrowing is limited to the branch-configured maximum (default 2) per expired
  gym membership cycle. Count/create atomically to prevent concurrent excess.
- Gym access across branches is allowed only in the subscription's city; verify
  member, plan, city, branch, and subscription ownership agree.
- Overdue installments block access immediately, with no grace period. Update
  affected subscription state and check overdue debt at access time.
- Private coaching requires active gym membership and a prior reception gym
  check-in for the same member/date/branch. Only the current assigned coach may
  consume a session. Check ownership and do not decrement unlimited sessions.
- Enforce activity package/coach/activity and check-in membership consistency.
- Consume sessions atomically, deduplicate scanner requests, and prevent repeated
  attendance/session creation. PrivateSession links to immutable subscription
  branch context and the actual CheckIn, not an employee's current branch.
- Apply freezes, closures, renewals, borrowed-session settlement, and start/end
  date changes transactionally; preserve purchase/access/price snapshots.
- Schedules assume same-day time ranges and DayOfWeek 0=Sunday through 6=Saturday.
  Handle branch local timezone and DST in BLL; persist instants at UTC offset 0.

## Employees and assignments

- Partial indexes enforce one open-ended primary branch assignment, current
  member sales assignment, and current private coach assignment. They do not
  prevent overlapping dated intervals or scheduled future assignments.
- Validate employment dates, active staff, permitted roles, and current branch
  access; close assignment rows rather than replacing historical records.
- House assignment requires no sales employee. Normalize phone numbers before
  storing; uniqueness compares stored values. Only owners may change primary
  phones. Validate and manage phone verification/timestamp consistency.
- Private coach may transfer at any time, including after a CommissionPeriod
  closes. End the current assignment and add a new one; EndedAt IS NULL defines
  current status. Assignments contain history only, not financial ownership.
- Already delivered sessions remain credited to the delivering Coach, purchased
  batch and period of occurrence. Future sessions belong to the new current
  Coach. Closed CommissionPeriods are never recalculated because of a transfer.
- At final subscription expiration, unused batch value is assigned to the current
  Coach in the open period at expiration/settlement. See DATA_FLOWS.md.
- Freelance Coaches have dedicated MembershipPlans. Freelance plan Price is the
  fixed amount due to the gym; external coach/client money is outside gym accounting.
  Validate the referenced Employee is currently active and EmploymentType=Freelance.
- Snapshot PlanCategory and FreelanceCoachEmployeeId on every membership purchase
  or renewal. Never reconstruct old purchases from the current plan definition.
- Only system Offers attached through OfferMembershipPlans to the specific plan
  may affect gym-recorded sale amounts. A coach discount from their external margin
  is not a system Offer. Never request the coach's external client charge.
- FreelanceAgreement retains Coach/Branch/dates/notes only; MembershipPlan.Price
  is the single pricing source. Plan eligibility is explicit, not permanent client ownership.

## Private financial history and concurrency

- Each PrivateSession represents one delivered session. Its basis is the referenced
  immutable PrivateSubscriptionTransaction.UnitSessionValue, not an assignment amount.
  Tracked EF saves reject purchase-fact changes after insertion; raw/bulk SQL bypasses
  this guard and must be prohibited for those facts by service/runtime permissions.
- Initialize each batch RemainingSessions=SessionsAdded. Consume oldest valid batch
  first (CreatedAt, then Id); validate batch/subscription ownership and eligibility.
- Sum delivered-session unit values plus expiry settlement EligibleAmount per Coach
  and CommissionPeriod, then apply configurable CommissionRules. Basis is not final
  commission: 1000 basis at 60 percent gives 600; no rate is hardcoded in entities.
- Preserve all session/settlement history and finalized summaries. Restrict deletes
  do not make records append-only: BLL must reject history edits/deletes.
- Lock the subscription consistently for attendance, transfer, renewal and expiry;
  use row locks or an equivalent concurrency strategy and one transaction per flow.
  Counter updates must not race, go negative or allow double consumption.
- Expiry settles each remaining batch once (database unique batch FK), records
  session count/unit value/eligible amount/current Coach/open period, zeros live
  batch and subscription counters, and marks Expired atomically. Settlement counts
  remain historical snapshots. A renewal before final expiry prevents settlement.
- DateOnly EndDate is retained; BLL must define branch-local end-of-day expiry,
  convert to UTC and select the appropriate open period. If no current Coach/open
  period exists, fail/retry without partially expiring or inventing financial history.
- Legacy PrivateCoachAssignment SessionsAllocated, SessionsConsumed and
  CommissionBasisAmount are deprecated preserved data only. Never use them for new
  allocation or commission. Explicit reconciliation is required before removal.

## Money, commission, and payroll

- PaymentParts must sum to Payment.Amount, RefundParts to Refund.Amount. Require
  appropriate accounts, branch/shift/period consistency, and source ownership.
- Enforce sale totals, installment totals/payments, overpayment/refund limits,
  cancellation effects, closed-shift restrictions, and request idempotency.
- Financial transactions have explicit checked source FKs. BLL must validate
  matching branches/accounts and Credit/Debit meaning, and ensure one consistent
  ledger posting per source/account. Derive balance from OpeningBalance plus
  credits minus debits; no stored CurrentBalance source of truth.
- Commission rules are effective-dated configuration. Choose employee overrides
  over branch defaults and validate method-specific rates/targets, relevant
  applicable scope, overlap, and eligible payment/refund timing. These generic rules
  do not control the new activity percentage or create class compensation.
- On branch setup, create separate configurable SalesNew and SalesRenewal rules:
  each target 30000; SalesNew below/reached 5/7 percent, renewal 3/5 percent.
  Do not seed these without a real BranchId or hardcode them in calculations.
- A reached target applies the higher rate to ALL eligible category revenue in
  that period, not only the excess. New and renewal targets are separate.
- Calculate commission, retroactive rate changes, refund effects in open periods,
  coach allocation across periods, and employee earnings across branches in BLL.
- Period close/reopen, immutable finalized snapshots, recalculation/version
  policy, snapshot uniqueness, payroll totals and salary-setting selection need
  BLL validation. The database allows cross-branch summaries; never infer a
  historical branch from the employee's current assignment.
- Refund amount is manually authorized, not automatically calculated. Enforce
  the branch quick-refund window (default 24 hours), manager approval, payout
  accounts, and applicable commission adjustments.
- OpeningBalance and Shift.Difference may be signed. Other financial magnitudes
  are nonnegative. Rates are percent points (5 means 5 percent), not fractions.
- One nullable OfferId per sale structurally limits it to one offer. Validate
  eligibility, dates, package junctions, entitlements, and value semantics;
  percentages cap at 100, ExtraDays must be an integer value in BLL.

## Authorization, audit, and contracts

- Standard Identity roles and Permissions are data-driven. Existing JWT authentication
  and dynamic Permission policies protect API actions; BLL also validates current
  permissions and branch scope. Owner may operate globally without a fake branch.
- Preserve financial/history records; deactivate/cancel instead of deletion.
  Restrict FKs prevent cascading history loss, not arbitrary direct deletions.
- AuditLog updates/deletes are rejected by tracked EF SaveChanges. This does
  not intercept ExecuteUpdate/Delete, raw SQL, or administrator actions. Grant
  production runtime INSERT/SELECT only on AuditLogs (migration owner separate),
  route audit writes centrally, and enforce append-only policy in all services.
- Redact secrets and sensitive authentication data before serializing jsonb:
  no passwords/hashes, OTPs, JWTs, refresh tokens, API keys or database credentials.
  MessageLogs contain delivery metadata only, never OTP values/provider secrets.
- Version ContractTemplate rather than mutating used versions. Save exact
  generated text/version on MemberContract and reference physical archives.
- Validate branch IP/CIDR/VPN rules, trusted proxy handling, authorized device
  hash verification/revocation, and active permissions before operations.
- No lazy-loading proxies, triggers, payment-provider workflows, background jobs,
  or automatic startup migrations are included. Focused activity settlement and
  reporting services are described below.

## Activities, Classes and Freelance — latest rules (2026-10-07)

- Activity access and Gym membership are independent. A new activity purchase must
  select an active ActivityCoach assigned to its activity. Internal coaches reference
  Employees; external coaches have a name/contact and no Employee/Identity account.
- ActivityCoach.Percentage is a fixed configurable percentage, never target/member
  dependent. Every purchased batch snapshots coach and percentage. Changes apply
  to subsequent purchases/renewals, including their installments, not old batches.
  The coach identity/type and purchase attribution cannot be reassigned by tracked saves.
- Eligible activity revenue is completed Payment.Amount, counted once, validated
  against its PaymentParts. Authorized Refund.Amount tied to OriginalPaymentId
  reduces the original payment's eligible revenue in an open period. Unlinked,
  ambiguous legacy activity payments/refunds must be reconciled, never guessed.
- No package-price-times-subscriber calculation. Period grouping uses explicit
  Payment.CommissionPeriodId, branch and [StartAt, EndAt) payment timestamps.
  Refunds after a closed period's end never change its finalized settlement.
  Their later adjustment/approval workflow remains outside this update.
- Finalize a closed period explicitly after all original payment/refund records
  are complete. ActivityCoachSettlement stores immutable per-coach/activity/rate
  financial snapshots. Internal totals feed EmployeeCommissionSummary exactly
  once; external totals remain separate amounts owed. No payroll/ledger payment
  is generated. Reopening does not silently replace prior activity snapshots.
- Actual ClassSession has the delivering Employee, independent of planned coach.
  CheckIns link to the occurrence; one member can attend it once. Historical
  CheckIns without an occurrence are preserved and excluded pending reconciliation.
- Count completed sessions, including zero attendance. Exclude scheduled/cancelled
  sessions. AverageAttendance = TotalAttendances / CompletedClassesCount.
  Class financial compensation is **BUSINESS RULE NOT FINALIZED**. Existing Class
  CommissionRules/summary fields are preserved historical/configuration structures;
  this implementation does not use them to pay bonuses, percentages or targets.
- Freelance purchases require explicit MembershipPlanEligibleMember and an active
  Freelance Employee. MembershipPlan.Price is the only configured gym price; external
  coach/client charges remain outside accounting. No new settlement pricing engine.
- Freelance sales snapshot House attribution, with no SalesEmployeeId or SaleCategory.
  End a current Sales assignment and append House inside the purchase transaction.
  Never rewrite prior assignments or prior sales; no SalesNew/SalesRenewal summary.
- Removing eligibility blocks subsequent purchases/renewals but preserves past
  subscription/sale snapshots. Eligibility is not ownership or a subscription.
  One member/one Freelance coach is a business expectation, **not a permanent DB
  constraint**. A member may appear in multiple plans; no automatic eligibility backfill.
- Services audit coach percentage/assignment changes, subscription coach selection,
  Freelance plan edits and eligibility changes with business action names.
- New purchases use a serializable transaction (or the caller's existing transaction).
  A caller providing a transaction owns its isolation/rollback. Concurrent conflicts
  return 409 through the API; reload/retry. Payment posting still must enforce the
  existing account/refund/idempotency rules when that workflow is implemented.
