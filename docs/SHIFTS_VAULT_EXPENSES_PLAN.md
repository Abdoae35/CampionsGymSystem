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

## Confirmed Owner policy — continuation 2026-10-11

This section supersedes the original exclusive branch-shift proposal. The verified
inventory above remains valid; existing models are extended without a second ledger.
All six policy decisions are now confirmed, including the subsequent Cash funds reply.

- Each authorized employee manually opens/closes their own Shift. At most one open
  Shift per employee across branches; different employees may overlap at one branch.
  Open/Closed are sufficient. No manager approval, automatic closing or replacement
  of the owning employee. Existing legacy participation/change histories remain intact.
- One physical drawer per branch, explicitly bound to one Cash FinancialAccount.
  Opening/closing counts observe this shared drawer; never sum employee opening counts.
- New Member registration, Sale/renewal, collections, refunds and operational payouts
  require the acting employee's own open Shift at the trusted operational branch.
  Identical committed retries survive closure. Legacy null Shift links remain null.
- Counted cash becomes the next physical baseline. Differences remain unresolved,
  branch-wide observations, never an automatic employee shortage/salary deduction,
  expense, revenue or ledger adjustment.
- Expenses are paid directly under capability checks, without approval requests,
  self-approval rules, thresholds or category seeds. Reversals require explicit actual
  returned money and append history. Employee advances are distinct payouts, not
  operating expenses or payroll; no repayment/interest/deduction workflow.
- Discretionary withdrawals require Vault.Withdraw plus Branches.AccessAll by default.
  No Manager default grant. No routine external Cash-In workflow.
- Initial physical cash is an explicit globally authorized audited setup. Existing
  accounting balances are preserved; an empty zero-opening account may receive one
  non-sales initial-position Adjustment, never a credit on employee Shift opening.
- Confirmed funds policy: reject Cash payouts above tracked physical cash; InstaPay
  is reporting of actual authorized money, not a presumed live bank/overdraft balance.
  Latest physical count plus subsequent Cash movements is authoritative availability,
  even when unresolved count discrepancies differ from accounting balance.
- BusinessDay remains explicit, branch scoped and independent from shifts. No fixed
  seasonal hours, midnight/logout closure, backdated recovery or fake legacy shifts.

## Minimal domain alignment

Reuse Shift with nullable legacy-compatible EmployeeId, drawer/account reference,
operation identity/hash, version and opening/closing effective dates. A filtered unique
EmployeeId index protects one open owned shift, not one per branch. Reuse ShiftEmployee
for the single owning employee's historical participation; no employee-specific ledger.

Add CashDrawer (branch key, unique FinancialAccount, movement sequence only), CashCount
(immutable physical/expected/book snapshots, sequence boundary, actor/date/time/reason,
optional Shift), and CashMovement (initial position or discretionary withdrawal).
The sequence is an ordering counter, never a balance. FinancialTransaction gains nullable
ShiftId and DrawerSequence for new writes. Counts create no ledger movement. CashCount
Difference = CountedCash - ExpectedCash. Every nonzero observation is preserved as an
unresolved discrepancy; later counts do not erase it or attribute it to an employee.

Reuse Expense/ExpenseCategory with immutable managed payment terms, employee/date/op
identity and explicit ExpenseReversal facts. Legacy Recorded must not be inferred paid.
Add EmployeeAdvance separate from Expense with receiving/recording employees and ledger
reference. Existing source enum values stay stable; add explicit Advance/CashMovement
sources with restrictive references. Guid IDs, numeric(18,2), UTC instants, BusinessDate,
unique operations, indexed FKs and legacy nullable associations remain consistent.

## Transaction and reconciliation invariants

1. Lock the branch coordination row before employee/shift, Member/Sale/account locks.
   All money and cash observations take the same lock: closing one employee cannot
   miss other employees' concurrent drawer movements or charge them to that employee.
2. Under that lock validate the acting employee's open Shift and branch; close affects
   only that Shift. Other open employees continue normally. No active shift is invented.
3. Increment CashDrawer movement sequence for each new cash ledger row. CashCount
   snapshots the sequence, so equal timestamps cannot lose/double-count movements.
4. Physical expected = latest counted cash + signed Cash ledger rows after its sequence.
   Accounting = immutable account OpeningBalance + all credits - debits. Report both.
5. Cash parts only affect Cash. InstaPay only affects its account. Sale commercial
   amount is not collection. Refund/expense/advance/withdrawal cash debits validate
   combined payout against physical funds before posting, in the same transaction.
6. Cash setup on a virgin account can record initial non-sales position once. Accounts
   with existing money are counted against preserved book facts; never credited again.
7. Direct expenses and advances post exactly one debit. Expense return posts explicit
   compensating credit only for confirmed returned funds; original records immutable.
8. Checks, counts, source facts, ledger, audit and idempotency commit/rollback together.
   Retry verifies original actor/request/scope before returning original outcome.
9. Shift opening/closing count is branch reconciliation, not an employee liability.
   Closed Shift snapshots are final; recounts are separate immutable observations.
10. No production access, historical migration edits, money backfills or payroll effects.

## Contracts, services and API plan

Concrete scoped ShiftService handles manual own open/close/current, scoped details and
bounded history. VaultService handles global setup, count/position/movement queries and
withdrawals. ExpenseService handles direct paid expenses, real-money reversal, categories
and distinct advances. A concrete financial coordination helper binds operations and
orders locks/sequences; no repository or scheduling framework.

| Candidate route | Input / output | Permission |
|---|---|---|
| POST /api/shifts | OperationId, OpeningCash, optional authorized BranchId; server employee/date/expected | Shifts.Manage |
| POST /api/shifts/{id}/close | OperationId, ExpectedVersion, CountedCash, reason | Shifts.Manage, own Shift |
| GET /api/shifts/current, /api/shifts, /api/shifts/{id} | Own current or bounded branch history/details | Shifts.View |
| POST/GET /api/business-days | Explicit day lifecycle, no seasonal automation | Shifts.Manage and trusted scope |
| POST /api/vault/setup | Cash account, initial physical count, operation/reason | Vault.Manage + AccessAll |
| POST /api/vault/counts | CountedCash, operation/reason; own open shift | Vault.Manage |
| GET /api/vault/position, /movements, /counts | Physical/book/discrepancies, bounded branch facts | Vault.View |
| POST /api/vault/withdrawals | Positive amount, operation/reason; own Shift | Vault.Withdraw + AccessAll |
| POST/GET /api/expenses | Category/account/amount/reason/reference, immutable paid result/history | Expenses.Create/View |
| POST /api/expenses/{id}/reverse | Actual returned amount, operation/reason; own Shift | Expenses.Reverse |
| GET/POST /api/expense-categories | Existing dynamic catalog, no default categories | Expenses.View / Expenses.Manage |
| POST/GET /api/employee-advances | Receiving employee/account/amount/reason/reference | Advances.Create/View |

Reuse existing Sales branch totals rather than duplicate their endpoint. Employee shift
reports distinguish actor's collections/refunds/payouts from all branch drawer movements.
Counts never imply blame. Source histories include legacy null shifts explicitly.

## Risks and tests

Test overlapping employees and isolated branches; counts do not multiply cash; tied
UTC timestamp sequence boundaries; actual 100000 opening + 8000 cash receipts;
split tender/expenses/advances/refunds/withdrawals; funds vs unresolved book variance;
Cash rejection and permitted InstaPay ledger-negative reporting; own shift gate and
forgotten current shift; cash close/money races; returning retries after close; legacy
null preservation; BusinessDay midnight; active employment/branch checks; exact money;
unique operations; append-only history; capability Deny overrides and branch spoofing.
All existing finance/subscription/check-in regressions must pass with real operational
shift fixtures where the confirmed new preconditions apply.

## Delivery sequence

Domain/migrations → employee lifecycle → shared vault → expenses → advances →
withdrawals → existing financial/registration integration → reporting/API → full
validation/docs/Postman. Each completed stage is built, tested, reviewed, committed
and pushed. New migrations apply only to verified local Development.

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

## Deferred scope

Payroll/advance settlement, commissions, monthly closing, automatic/seasonal shift
schedules, exceptional forced closure, routine external cash funding, Private/Classes
finance, frontend and offline synchronization remain separate future features.
