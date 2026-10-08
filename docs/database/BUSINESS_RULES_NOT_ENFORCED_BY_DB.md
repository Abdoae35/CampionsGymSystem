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
  and CommissionPeriod, then apply the applicable rule (ordinary configurable rules;
  transferred-client work/expiry uses 50% per the latest alignment below). Basis is not final
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
  permissions and branch scope. Global accounts may operate without a fake branch when explicitly granted
  Branches.AccessAll; Owner role name alone is no bypass.
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

## Final alignment — future BLL responsibilities (2026-10-07)

- Direct authorization resolves active permissions as **Roles + direct Allow - direct
  Deny**. Deny wins over Owner/other role grants and direct Allows. The database stores
  one effect per user/permission. The later branch feature now implements this resolution
  in the existing PermissionService; no new policy/override management API was added.
- Open/close BusinessDay explicitly through an authorized branch operation. Never
  close it automatically at midnight. A real after-midnight event may still reference
  the prior BusinessDate while retaining its true UTC timestamp. Closing a Shift does
  not imply closing BusinessDay. A day can contain multiple shifts.
- On each operation, validate the chosen BusinessDay belongs to its event's Branch,
  is valid/open for that workflow, and is preserved historically. Payment/refund days
  may differ from sale day. Derived purchase-batch day comes from its own sale; payment
  and refund parts inherit their own parent. Do not infer unknown legacy days from
  CreatedAt. Financial adjustment/day and zero-attendee class/day links are explicit.
- Subscription StartDate may follow the linked business date according to the purchase
  rule; never replace real CreatedAt with midnight or the business date. Day assignment,
  subscription start logic, date boundaries/timezone and immutable history are BLL work.
- Management controls dynamic plan audience categories independently of Normal/Freelance.
  Preserve existing city-based access. Deactivate categories/plans instead of deleting
  referenced data; classification edits must be auditable.
- Employees submit leave; a manager resolved through branch + permissions reviews it.
  Pending/Approved/Rejected and reviewer/time are stored. Future BLL validates reviewer
  authority, leave overlaps, and approved leave conflicts with shifts before assignment
  or replacement. No Employee.ManagerId or automated hierarchy/workflow was added.
- Shift replacement atomically preserves assignment participation and appends a
  ShiftEmployeeChange (original, replacement, actor, time, reason). AuditLog also records
  the action; it does not replace the business event. Reject invalid/cross-branch or
  approved-leave-conflicting replacements in BLL, not a cross-table trigger.
- Private transfer is allowed after closed periods. Old delivered sessions stay with
  their actual coach/batch/period. **Transferred-client work and transferred-client expiry
  settlement use the fixed 50% rule in future BLL**, superseding older ordinary configurable
  rate guidance for this case. Preserve full eligible basis; apply 50% to final entitlement,
  never halve recorded delivered sessions or move already-earned historical basis.
  Assignment histories and event/settlement timestamps preserve the transfer facts.
- Class average remains total actual attendance / completed actual sessions, including
  completed zero-attendee sessions and excluding cancelled/scheduled sessions. Financial
  class compensation remains **BUSINESS RULE NOT FINALIZED**.
- Activity entitlement remains based on actual collected eligible funds and linked refunds,
  using preserved coach/rate history; no new calculation, payment table or trigger here.
- Freelance plans require explicit eligibility and coach/price snapshots; new purchases
  are House with no Sales commission. Do not rewrite prior Sales history. One Freelance
  coach per member is an expectation, not permanent global database uniqueness.
- Audit override changes, BusinessDay opening/closing, leave review, shift replacements,
  audience changes, eligibility changes and activity coach/rate/assignment changes using
  the existing append-only AuditLog. Do not log secrets.
- Offline synchronization is **not implemented**. Existing UUIDs, ownership FKs, real
  timestamps and history are retained. No outbox/inbox/nodes/conflicts/replication added.

## Membership benefit rules for future BLL

- Purchase/renewal must copy current plan benefits into new transaction-based grant
  snapshots in the purchase transaction. No automatic grant creation exists yet.
- Each renewal has its own grant batch; changing a plan/catalog must never rewrite
  historical grant code/name/type/quantity. The unique transaction/benefit constraint
  supports avoiding duplicate grants, but full request idempotency remains BLL work.
- Quantity grants require positive quantity; Access grants have NULL quantity, never
  artificial unlimited counts. Access validation is future subscription-context logic.
- Usage must target a Quantity grant and satisfy membership/branch/actor authority and
  validity rules. The DB enforces positive QuantityUsed and FKs, not cross-table type,
  overconsumption or permission validation. Use one transaction and suitable locking
  later to prevent concurrent overconsumption; no balance triggers exist.
- Remaining quantity = GrantedQuantity - SUM(usage.QuantityUsed). Usage history is the
  source of truth; future BLL must preserve history and define any correction policy.
  There is no mutable UsedQuantity/RemainingQuantity or automatic reversal mechanism.
- **BUSINESS RULE NOT FINALIZED:** unused-benefit expiration, renewal carryover,
  consumption FIFO/reset policy and exact applicable validity boundaries. Separate
  purchase/renewal batches and existing subscription/transaction dates are retained;
  no automatic expiry, carryover, merge or independent expiration engine was invented.
- Benefits apply to all MembershipPlans, including audience classifications and
  Freelance plans. They do not automatically apply to ActivityPackages.
- Included benefit consumption creates no SaleTransaction, Payment or FinancialTransaction.
- Deactivate catalog benefits instead of deleting referenced history. Removing plan
  configuration does not remove old grants/usages. Future BLL controls new grants and
  usage against inactive records; deactivation alone does not erase entitlements.
- Use existing AuditLogs for catalog creation/edit/deactivation, plan benefit addition,
  removal/quantity changes and usage, with entity IDs and actor context; no duplicate
  audit system or automatic benefit auditing was implemented here.

## Implemented branch foundation and future module contracts

- Employee branch dates are inclusive and resolved using the configured gym calendar
  timezone, independently of an overnight BusinessDay. One current assignment is required.
  Legacy overlaps fail closed and need explicit reconciliation; no silent history rewrite.
- Initial assignments/transfers use employment/active-branch validation, all-interval overlap
  checks and a common per-employee DB row lock. The existing index protects only open
  primary rows; BLL guards bounded/nonprimary intervals. Direct SQL writers bypass BLL
  and must not create overlapping history. No schema change was necessary.
- Transfers require Employees.Transfer and scope on both source and target. Scoped
  managers have no broad cross-branch authority. Effective dates cannot be backdated or
  equal the current assignment's start; pending future transfers cannot be overwritten.
  Backdated corrections, intra-day moves and rescheduling/cancellation remain separate work.
- Audit branch creation/settings/activation and assignment/transfer atomically using
  existing AuditLogs. Historical financial/attendance/shift BranchIds never move.
- All future branch-scoped BLL queries and writes must use branch-scope validation and
  actual resource BranchId, not trust a request ID. No global EF filter is assumed.
- Cross-branch member identification for legitimate subscription access is allowed only
  with necessary operational fields; it does not grant financial visibility or change
  home branch. Future SessionBased check-ins consume shared sessions; Unlimited does not.
- Money received/spent at B belongs to B's sale/payment/ledger/expense even if a member
  registered at A. Subsequent employee/member transfers do not rewrite these facts.
- Sales client/lead/follow-up/future-shift/payroll consequences of employee transfers are
  deferred to their own features. No automatic reassignment or finance/check-in workflow
  was implemented. See ../branches/BRANCH_MANAGEMENT.md and BRANCH_ISOLATION.md.

## Member registration branch and legacy ClientCode

HomeBranchId represents the original registration branch for ClientCode identity.
Preserve that context independently from later check-ins, purchases or staff moves.
ClientCode is nullable for uncoded legacy members and unique per HomeBranchId when
present; positive values are enforced by the DB. Import must retain exact old codes,
never silently renumber. No fake backfill is performed. New-member allocation now
uses the branch counter atomically in Member Management;
plain unprotected request-time MAX+1 remains prohibited. Offline synchronization and
import are not implemented. Future imports must advance the counter under branch locking.

## Guest invitations and House customers

- One successfully redeemed free trial per person: the DB enforces uniqueness
  independently for canonical phone and known MemberId, plus immutable redeemed
  identity/status and no redeemed deletion. BLL must match phone/member identities
  across records, normalize consistently, and handle changed/shared phone numbers;
  indexes cannot identify a human using different unlinked identities.
- Member Management now provides shared PhoneNormalizer using configured branch calling
  codes and persists Members.PrimaryPhoneNormalized. Future invitation issuance/redemption
  must reuse that canonical policy; trimming alone is not normalization or verification.
- Future BLL requires Available before redemption, validates that a linked
  CheckIn is Trial and matches the invitee/actual branch, and writes atomically.
  The FK alone does not enforce these cross-record facts.
- Benefit source must belong to the inviting member and represent an invitation
  benefit with available quantity. Consumption and its timing remain future BLL.
- Expiration duration/calculation is not finalized. Expired is stored but no
  automatic expiry calculation is introduced.
- Inactive House threshold is configurable later (proposed
  HouseClientInactiveAfterDays); derive inactivity from existing history.
- Freelance purchases and normal purchases while still House have no Sales
  commission and remain House revenue. Management may explicitly assign Sales
  later. Existing House assignment history/eligibility schema is unchanged.
- MembershipPlan.Price for Freelance is only the fixed gym receipt. The coach's
  external envelope money is outside gym accounting; no customer-total,
  coach-share, envelope or percentage-settlement columns are introduced.
- AuditLogs remains the audit mechanism for invitations, eligibility changes
  and House redistribution. No second audit system or workflow is implemented.

## Employee gym membership — future BLL

- Automatically create an employee membership later only when employment is Active,
  a linked Member exists and no open employee period exists. No creation/linking
  or lifecycle automation is implemented in this database task.
- Access requires Active employment, valid employment dates and a valid employee
  subscription; subscription status alone is insufficient. Suspended or departed
  employees cannot enter on this benefit. Current permanent departure is represented
  by Resigned + TerminationDate, not a new Terminated status.
- Employee plans are internal/system-only. Exclude them from customer sale/catalog
  workflows and commissions. Validate plan type, subscription EmployeeId and linked
  MemberId together; the database does not enforce cross-table equivalence.
- Employee memberships create no SaleTransaction, Payment, PaymentPart,
  FinancialTransaction, Sales/Coach commission or revenue. Subscription does not
  require a purchase batch. The database cannot prohibit arbitrary unrelated
  finance rows; future BLL must avoid generating them.
- Open employment entitlements have null EndDate; no fake 2099/9999 expiration.
  Normal/Freelance retain required expiration. Employee plans use DurationDays = 0
  as no fixed duration, never to calculate a zero-day purchase.
- Close/deactivate on suspension or departure with a real end date. Rehire creates
  a new subscription ID and period; preserve old ownership and access history.
  The DB prevents duplicate open periods per employee/member. BLL must reject
  overlapping historical date ranges and ensure changes are atomic.
- Tracked EF saves reject employee-period deletion/reassignment and changes to
  closed periods; raw SQL/bulk operations bypass that guard and must be restricted
  operationally. Future services also preserve Employee–Member identity links.
- Benefits continue without a paid transaction only as the employee gym entitlement;
  the existing purchase-batch Membership Benefits system is not redesigned.

## Member Management service-enforced rules (implemented)

See [Member Management](../MEMBER_MANAGEMENT.md) for the complete implemented contract.
NormalSales resolves the authenticated eligible employee; normal ManagementCreated
requires explicit eligible Sales selection and assignment permission. Normal registrations
never default to House. FreelanceHouse explicitly creates eligibility and no Sales row.
House is derived from no current Sales-type row; legacy House rows remain history.
Assign/reassign/return-to-House lock the Member, preserve old rows and audit atomically.
CreateCrossBranch authorizes only NormalSales creation, never broadens member/finance scope.
HomeBranchId, ClientCode, MemberCode and ownership cannot be changed by profile update.
Primary-phone edits require dedicated permission; canonical changes reset verification.
Status changes preserve relationships and do not implement employment/subscription lifecycle.

Database uniqueness protects canonical phones, branch ClientCodes and one current assignment;
BLL validates eligible active Sales/account/effective permissions and active target branches.
New/changed phones are canonicalized; nullable unresolved legacy phones require administrative
reconciliation. Branch calling-code edits never bulk rewrite stored canonical phones.
Raw SQL/bulk writes bypass service authorization/immutability; operational grants must prevent
uncontrolled writes. Imports/corrections/counter maintenance are future explicit workflows.
Commission attribution remains future BLL and must use actual revenue per branch independently.

## CRM / Leads BLL rules (2026-10-08)

Database partial indexes enforce current Lead assignment, canonical Lead phone,
conversion link and open Member/reason uniqueness. Restrict FKs preserve business
history. BLL adds locked transactions, shared Lead/Member phone locks, legacy-safe
matching, branch/owner scope, employee/account/effective ownership capability and
operational branch eligibility. Cross-table phone identity is a workflow rule, not
a cross-table unique index; future imports must use equivalent locking/validation.

Lifecycle timestamp/state consistency, append-only interaction workflow, next-contact
ordering, safe owner-preserving conversion and explicit Freelance House exceptions
are BLL rules. Legacy nullable canonical phones/actors/dates are not guessed. Existing
historical assignment/conversion duplicates must be reconciled before deploying new
uniqueness on another populated database. No ownership is silently changed on employee
transfer/termination. No automatic inactivity/expiry/attendance scanner exists; CRM
work is opened manually or through the internal integration method. Legacy import
must preserve original Sales/House ownership. See [CRM](../CRM_LEADS.md).
