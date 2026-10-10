# Shifts, Vault / Cash Drawer and Expenses — implementation plan

Inspection checkpoint: 2026-10-11. This is a design/decision record, not a claim
that operational Shifts, Vault or Expenses have been implemented.

## Verified starting point

- Initial working tree was clean on the existing dedicated branch
  `Shifts-Vault&Expenses-`, tracking its identically named origin branch.
- Local HEAD, upstream and both remote feature/Sales heads were
  `e85e4984d64727676cc1e392e1e13e6036f73048`.
- Solution: .NET 10 API, BLL, DAL and the already-wired xUnit integration test project.
- Completed Sales, Subscription and Check-In services remain authoritative. No
  financial, subscription or attendance redesign is proposed.
- Fresh baseline verification: build succeeded with zero warnings/errors; EF model
  drift is clean; complete suite passed **678/678**, zero failed/skipped, in 8m23s.
  This independently confirms the recorded Sales baseline without assuming its report.
- Read-only guarded localhost Development inspection found two active branches,
  the three applied Sales migrations, and no Cash/InstaPay accounts, BusinessDays,
  Shifts, Expenses or expense categories. Configuration will be explicit; no accounts,
  opening money, fake shifts or categories were seeded by inspection.

## Existing capabilities and gaps

| Existing foundation | Reuse | Missing operational behavior |
|---|---|---|
| Shift | Branch, Open/Closed, opening/expected/count/difference, actors/timestamps, nullable BusinessDayId | Lifecycle service/API, concurrency identity/version, account snapshot, handover policy/history, one-active-shift invariant |
| ShiftEmployee | Composite Shift/Employee key; JoinedAt/LeftAt | Multiple-participant management and branch/employment/leave validation |
| ShiftEmployeeChange | Appendable replacement, actor, time and reason | Atomic replacement workflow; preserve original participation history |
| FinancialAccount | Branch, dynamic named account, Cash/InstaPay classification, immutable opening facts | Explicit identification of the one physical branch cash drawer; no automatic name/first-account selection |
| FinancialTransaction | Append-only Credit/Debit; source Payment/Refund/Expense/Payroll/Adjustment; exact account/branch/time/day | Managed cash funding/movement facts, reversal identity and direct shift association where needed |
| Payment/Refund and parts | Exact tender, nullable Shift links, immutable managed records, transaction/idempotency | Associate only new money operations with the actual applicable shift; close-race protection |
| SaleTransaction | Nullable CreatedShiftId; commercial snapshots | New sale shift origin where applicable; never rewrite completed Sales |
| Expense | Branch/category/account, nullable Shift/BusinessDay, amount/description, creator/approver, Recorded/Cancelled | Separate recording/approval/payment/reversal facts and financial effects |
| ExpenseCategory | Existing global dynamic catalog, active flag, unique name | Authorized catalog administration; no hardcoded category list |
| BusinessDay | One open day per branch; unique branch/date; actor/time and Open/Closed | Explicit open/close workflow needed for usable overnight operations |
| Authorization | Trusted branch scope, Branches.AccessAll, role grants and direct Allow/Deny | Small new operational capability set and controlled default role provisioning |
| AuditLogs | Existing append-only audit | Shift/count/handover, expense and cash-movement event families |

No Shift, Vault or Expense service/controller was found. Current financial writers
create completed Payments/Refunds and source-specific ledger rows with no Shift link.
Account balances are OpeningBalance + credits − debits; there is no CurrentBalance.
Existing account creation intentionally sets opening money to zero. No production
or Development money changes are authorized by this inspection alone.

Existing documentation already confirms that BusinessDay is explicitly managed,
is not automatically closed at midnight, and may contain multiple shifts. Closing
a shift does not close its BusinessDay. Employee replacements preserve original
participation and append ShiftEmployeeChange; approved leave conflicts must be checked.

## Policy decisions required before operational implementation

| ID | Decision | Why it blocks safe behavior |
|---|---|---|
| P1 | Who opens/submits/closes a shift; whether a different manager must approve closure before the next shift | Determines required states, authority, handover facts and drawer availability |
| P2 | Must every new Cash/InstaPay collection/refund/expense require an open shift, or only physical Cash operations? | Determines missing-shift rejection and electronic-money reporting scope |
| P3 | Who authorizes initial funding and later cash-in/cash-out; whether ordinary manual movements are in this release | Cannot invent cash origin or withdrawal authority |
| P4 | After a shortage/surplus, does next-shift expectation remain ledger-based or use the counted physical position with separate unresolved book variance? | Different financial outcomes; counts must not silently adjust ledger or hide carried discrepancies |
| P5 | Expense approval rules, self-approval, payment authority; reversal meaning and actual return of funds | Recording, approval and actual money movement are distinct; no implicit reimbursement/threshold policy |
| P6 | Should payouts exceeding tracked account funds be rejected for Cash, InstaPay or both? | Preventing overdrafts changes existing refund behavior and requires explicit policy |

Recommendations in a clarification are proposals only. Implement confirmed choices,
not these recommendations. If an approved workflow permits a financial correction,
it must create a separately authorized compensating fact; never edit prior money.

## Proposed minimal domain alignment

Preserve existing entities and names. Add only fields/history required by confirmed
decisions, using new migrations and nullable legacy-compatible operational metadata.

- Shift: operation/request identity, optimistic Version, effective opening/closing
  BusinessDate, identified Cash account, opening book-position snapshot alongside
  existing counted OpeningCash, and closing/handover metadata. Preserve existing
  enum values; add a pending-review state only if P1 requires one.
- ShiftEmployee: reuse existing dated participation; use ShiftEmployeeChange for
  actual replacements. Do not create a drawer or ledger per employee. Joining after
  leaving must preserve the original period rather than overwrite its history.
- Bind exactly one physical Cash FinancialAccount to each branch through the
  smallest explicit configuration/reference. Account balances remain ledger-derived.
  Do not merge historical Cash accounts or infer which one was the physical drawer.
- FinancialTransaction: nullable direct Shift reference for new operational posts
  if necessary for reliable source-independent summaries; never backfill legacy rows
  by timestamp or assign them to an unrelated later shift.
- Cash movement/funding: a small append-only operation fact linked to the existing
  Adjustment ledger foundation, only if P3 authorizes it. Reason, type, source account,
  branch, actor, actual time, BusinessDate, approval and reversal reference are explicit.
- Reconciliation: preserve submitted expected/count/difference and acknowledgment
  facts. Recount/correction is a new historical observation, not a rewrite of a closed
  count. A count observation creates no money effect.
- Expense: preserve original category/account/amount/description; add responsible
  Employee, receipt/reference, effective BusinessDate, operation/version and confirmed
  lifecycle fields. Legacy Recorded rows must not be inferred to be newly approved or
  paid. Reuse ApprovedByUserId; append payment/reversal facts tied to existing ledger
  rows where needed to distinguish money from approvals and retain exact history.
- ExpenseCategory remains dynamic; preserve the existing global catalog rather
  than add a duplicate category system. No default list or threshold is inferred.

New history rows use Guid IDs, actor/time, operation identity and restrictive FKs.
Money remains numeric(18,2). Unique operation and source-posting indexes prevent
duplication; branch/active-shift uniqueness and lifecycle/date/check constraints
support BLL validation. Inspect generated SQL and legacy upgrade behavior before
applying migrations to guarded localhost Development. Production remains untouched.

## Accounting invariants

1. One physical drawer and one concurrent cash-handling shift per branch; multiple
   employees may participate. Branch A and B have independent accounts/operations.
2. Sale price is not cash collected. Expected cash counts actual ledger movements.
3. Cash part increases physical Cash; InstaPay part only affects its own account.
   Reservations/installment collections count when actually received.
4. Cash refunds and actually paid cash expenses reduce physical Cash once; merely
   recording/approving an expense has no ledger effect.
5. The initial EGP 100,000 is opening funding/position, never Sales revenue. Shift
   OpeningCash is a count snapshot, not another ledger credit. EGP 8,000 cash sales
   then yield EGP 108,000 before other actual movements.
6. Reopening/starting the next shift does not credit carried money again. Never sum
   repeated cash-count observations as though they were separate financial losses.
7. Difference convention proposed: counted − expected (shortage negative, surplus
   positive). Preserve separately observed opening/closing book and physical facts.
8. No automatic ledger adjustment for shortage/surplus. Approval to acknowledge a
   discrepancy is distinct from authorization to post a correcting money movement.
9. Expense reversals cannot pretend spent Cash has physically returned unless P5
   explicitly confirms the corresponding real financial event.
10. Existing Sale/Payment/Refund/subscription/CheckIn facts and null Shift/day links
    are preserved. Only new operations receive trusted shift references.
11. Actual UTC timestamp and effective BusinessDate are separate; source event
    branch/day may differ from the original Sale branch/day only where authorized.

## Service and concurrency integration

API → scoped concrete BLL service → ApplicationDbContext/PostgreSQL. No CQRS,
MediatR, repository framework, distributed locks or extra messaging infrastructure.

- Shift service: open, participant join/leave/replacement, close submission/approval
  as confirmed, handover acknowledgment, scoped current/history/details.
- Vault service: account balance/movements, reconciliation observations and approved
  funding/cash movements only if confirmed. Do not duplicate existing account listing
  or Sale collection/refund totals.
- Expense service: record, approve/reject or cancel as confirmed, pay, reverse actual
  payment as confirmed, category administration and scoped bounded queries.
- A small concrete financial/shift coordination helper may be shared by Sales and
  these services to bind a new financial write to its branch/shift safely. No generic
  transaction/repository abstraction is needed.
- Standardize lock order before integrating Sales: branch cash/shift coordination
  first, then existing plan/Member/Sale/source/account locks in a consistent order.
  Do not upgrade an already-held branch shared lock after acquiring Member/Sale locks.
  Close acquires the same coordination lock and revalidates Version before counting.
- Count and freeze the closing snapshot in the same transaction as final state. A
  committed concurrent receipt must be included; one arriving after close must be
  rejected or attributed to a real later shift according to P2, never the closed shift.
- Idempotent retries return the original operation even after its shift closes;
  changing request/actor with the same operation identity must fail.
- BusinessDay open/close uses trusted branch scope and locks. A day cannot close
  while an applicable active shift exists; midnight does not create/close one implicitly.
- Keep existing Sales money + subscription atomicity and CheckIn behavior intact.

## Proposed API contracts (not implemented routes)

All mutating inputs carry OperationId and, for state transitions, ExpectedVersion.
Normal staff cannot submit arbitrary authoritative BranchId, expected balance,
approver, account balance or status. Global explicit selection follows existing scope.
Use existing ProblemDetails, strict DTOs, numeric enum and bounded pagination conventions.

| Area | Candidate operation | Capability approach |
|---|---|---|
| /api/shifts | POST open; GET history/current/{id}; participant operations; close/acknowledge | Existing Shifts.View/Manage; extra close/approve capability only if policy requires |
| /api/business-days | Explicit open/close/current as needed for overnight operation | Confirmed shift-management/global scope; reuse existing BusinessDay model |
| /api/vault | GET position/movements/reconciliations; authorized movement/reversal if confirmed | Minimal Vault.View/Manage, plus global authority if P3 requires |
| /api/expenses | POST record; GET history/{id}; approve/pay/reverse/cancel | Reuse Expenses.View/Create/Approve; distinct Pay/Reverse if confirmed actors differ |
| /api/expense-categories | Dynamic management/read | Catalog administration/global scope; no seeded invented list |
| Existing /api/sales/branch-totals | Existing collection/refund totals retained | Payments.View; do not duplicate this route |

Details expose authoritative opening/closing facts, participating Employees, source
account, effective dates, actual times, lifecycle/version, financial totals and bounded
history. Per-shift and branch summaries separately expose collections, refunds, paid
expenses, reversals, authorized funding and observed discrepancies by payment source.
Document actual final routes/payloads after implementation and update Postman examples.

Candidate request/result shapes, subject to P1–P6:

- OpenShift: OperationId, counted OpeningCash, EmployeeIds and optional globally
  authorized target BranchId. Resolve the configured drawer, real BusinessDay, opening
  book position, opener and timestamp on the server; do not accept expected balances.
- CloseShift: route ShiftId, OperationId, ExpectedVersion, counted cash and reason/
  handover note. Server calculates movements and expected/count difference. P1 decides
  whether this closes the shift or submits it for approval.
- Participant change: route ShiftId, OperationId, ExpectedVersion and EmployeeId(s);
  replacement additionally identifies original/replacement Employees and reason.
- Acknowledgment/approval: route ShiftId, OperationId, ExpectedVersion and note;
  actual authenticated actor supplies authority. Never accept ApprovedByUserId.
- Cash movement, if enabled: OperationId, kind, Amount, reason/reference and permitted
  account/branch selection. Server records actual time/day/actor and corresponding
  existing ledger effect. Reversal references the original operation, not a new balance.
- RecordExpense: OperationId, CategoryId, FinancialAccountId, Amount, Description,
  ResponsibleEmployeeId and optional receipt/reference. Selected source is one Cash or
  InstaPay account, matching existing Expense; split expense tender is not invented.
- Expense approve/pay/reverse: route ExpenseId, OperationId, ExpectedVersion and
  reason/return facts as confirmed. Actor, financial effect, amount limit and lifecycle
  are authoritative server facts. Merely submitting an expense is not payment.
- ShiftDetails: identifiers, account/day, participants, Version/status, actual opening/
  closing/acknowledgment times, opening counted/book positions, expected/count/difference,
  and source-specific receipt/refund/expense/movement totals with bounded event history.
- AccountPosition: account/branch/method, opening fact, total ledger credits/debits,
  derived book balance, latest physical observation and separately reported variance.
- ExpenseDetails: immutable recorded terms, responsible Employee, approvals, actual
  payment/reversal identities and account/branch/date/time, effective status and Version.
- History input: optional authorized BranchId, date range, applicable status/account/
  Employee filters, Page/PageSize bounded to 100. Stable timestamp + Guid ordering.

## Authorization and the prior Sales provisioning issue

Runtime remains Roles + Allow − Deny (Deny wins), active user/permission and trusted
branch context. Capability checks belong in controllers and BLL; no hardcoded role
authorization. New default role templates may change only for newly created roles.
Existing customized grants/overrides must be preserved; targeted provisioning must
explicitly identify an authorized role and never infer it from a role name.

Read-only localhost Development inspection confirmed Offers.Manage,
Installments.Manage and Sales.QuickCancel exist and are active, each with **zero role
grants** and **zero direct Allow/Deny overrides**. Their prior existing-role
provisioning gap is **not resolved**. The Sales
CLI only assigns an explicitly configured globally capable role; new-role demo
templates do not update existing roles. No permission/grant/override was mutated.
Do not silently repair unrelated Sales grants as part of Shift/Expense setup.

## Risks and required tests

- Opening/count authority, self-approval and initial funding must be confirmed.
- Multiple legacy accounts or overlapping legacy shifts require explicit resolution,
  not destructive migration or arbitrary selection.
- Pending closure changes cash availability; confirm before introducing that state.
- Mandatory shifts change new Sales/refund preconditions; existing test fixtures must
  open real shifts, preserving explicit legacy-null scenarios without fake backfills.
- Negative ledger position, refunds from a different method and externally funded
  InstaPay cannot be given a new overdraft policy by inference.
- Lock-order changes can deadlock existing Member-based Finance/CheckIn workflows;
  test independent committed PostgreSQL connections, not only ambient rollback tests.

Tests must cover: multiple employees; independent branches; opening 100000 + receipts
8000 = expected 108000; Cash/InstaPay split separation; refunds/paid expenses/reversals;
midnight with an open BusinessDay; multiple shifts per day; missing/duplicate/overlapping
shift requests; close-versus-payment/refund/expense races; insufficient funds as
confirmed; under/over counts and acknowledgment; immutable closed facts; preserved
legacy null links; approval/self-approval/branch spoofing; direct Allow/Deny overrides;
operation retries before/after closure; failed insert rollback; exact ledger posting;
and the complete existing Sales/Subscription/CheckIn/Member/Auth/Branch suite.

## Delivery sequence

1. Planning/inspection checkpoint and required policy answers (no operational code).
2. Domain and new migrations, legacy-safe constraints and targeted schema tests.
3. Shift lifecycle/participation, BusinessDay integration and new money shift binding.
4. Vault/reconciliation, approved funding/movements and physical/electronic separation.
5. Expenses and actual-payment/reversal ledger integration.
6. Scoped reporting/API, authorization/default provisioning and Postman.
7. Final tests/docs/fixes: full build/suite, EF drift, guarded Development catalog,
   clean working tree and matching local/upstream/remote HEAD.

Each implementation increment: implement → build → relevant tests → inspect diff →
history checkpoint → commit → push. Do not publish broken code. Stop for genuinely
unresolved financial policy; continue independent verification/planning while waiting.

Monthly closing, commissions/targets/payroll, Private/Classes finance, Member branch
financial settlement, frontend and offline synchronization remain separate features.
