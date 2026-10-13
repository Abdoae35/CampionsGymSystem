# Employee Shifts, Shared Vault, Expenses and Advances

This feature uses the existing API → BLL → ApplicationDbContext architecture,
capability authorization, PostgreSQL ledger and effective BusinessDate conventions.
The [implementation plan](SHIFTS_VAULT_EXPENSES_PLAN.md) preserves the inspection
and Owner decisions. Sales, subscriptions and check-in remain their existing services.

## Employee shifts and the shared drawer

Each active linked employee manually opens and closes their own Shift. One employee
may have at most one open shift across all branches. Different employees may overlap
in one branch. A returning employee retrieves their existing open shift; no shift
is automatically opened, closed, replaced or backdated. Normal closure is final and
requires the opener, capability, current version and an actual cash count. There is
no approval or pending state. Legacy ShiftEmployee/ShiftEmployeeChange history remains
intact; new owned shifts have one owning participant with actual JoinedAt/LeftAt.

Each branch has one explicitly configured physical drawer, bound to one existing
Cash FinancialAccount. There are no employee cash accounts. Opening and closing
cash counts observe that shared branch drawer. An employee's collections/payouts are
reported by explicit ShiftId, separately from all branch cash movements and counts.
A count shortage is never automatically blamed on that employee or deducted from salary.

New Member registration, Sale/paid renewal, Cash/InstaPay collection, reservation or
installment payment, refunds/cancellation and operational payouts require the acting
employee's own open shift at the trusted operational branch. Another employee's open
shift is insufficient. Global capability does not replace an own shift for payouts.
Reading reports, authentication, explicit day management and initial setup remain
available without a shift. Existing null Shift associations remain unchanged.

Employee branch transfer requires closing that employee's own shift first. Transfer
and shift opening coordinate on the employee row; they cannot strand an open shift
in the old branch. There is no forced recovery endpoint. Forgotten shifts do not
block colleagues or explicit BusinessDay rollover.

## Cash positions and reconciliation

Accounting position = immutable account OpeningBalance + all credits − all debits.

Tracked physical cash = latest actual CountedCash + subsequent signed Cash ledger
movements after that count's sequence boundary.

Every physical observation preserves expected/count/book cash, the difference,
branch/account, optional shift, actor, UTC timestamp, effective date, reason and
sequence boundary. ObservationOrder orders counts even when timestamps match.
The actual count becomes the next physical baseline. Nonzero discrepancies remain
historical unresolved branch observations. Recounting does not erase earlier differences.
ObservedDiscrepancies is their cumulative signed total, not employee liability or a
separate spendable balance; individual observations remain available.

Example: explicit initial position 100,000; cash collections 8,000; no payouts:
expected physical cash 108,000. A count of 107,800 records −200. The next baseline
is 107,800; book cash stays 108,000. No fake expense/revenue or ledger adjustment is
created by that observation. A later Cash expense of 500 leaves expected physical
cash 107,300 and book cash 107,500.

Initial setup is globally authorized and audited, not sales revenue. A virgin account
with zero opening balance and no transactions receives one explicit non-Sales
InitialPosition CashMovement credit when the initial count is positive. Existing
accounting balances are preserved: setup only observes cash and records any difference.
Employee opening/closing counts never credit the ledger. There is no routine external
Cash-In API. A configured drawer cannot be rebound through these workflows.

Cash refunds, expenses, advances and discretionary withdrawals reject amounts above
tracked physical availability, including unresolved shortages/surpluses. InstaPay is
kept separate and is not assumed to expose a live bank balance: authorized actual
transactions may produce a negative ledger position without an invented overdraft rule.
Split tender affects only each part's source account. New Cash uses the mapped drawer;
legacy other Cash accounts remain visible as historical accounting facts.

## BusinessDay and time

Actual timestamps are UTC instants. BusinessDate comes from the branch's explicitly
open BusinessDay, falling back to the existing gym-local date convention when none
is open. After-midnight operations may therefore belong to the previous date.
No seasonal hours or midnight closure is hardcoded. A shift may span midnight and
multiple explicitly opened days. Authorized staff may close/open a BusinessDay with
employee shifts still open; each later transaction records the newly open day.
Closing a shift does not close a BusinessDay. Day creation uses OperationId as its ID;
repeated close is an idempotent transition of that same day.

## Direct paid expenses and actual returns

Expenses use the existing dynamic ExpenseCategory catalog. No category list, approval
queue, threshold or self-approval rule is introduced. An authorized employee records
an actual paid expense directly: branch, recording employee/shift, account, Cash or
InstaPay, amount, description, optional receipt, UTC time, effective day and audit.
One ledger debit is written atomically with the expense. New managed expenses are
immutable; their original Recorded status is a historical fact, not an approval state.

An expense return requires Expenses.Reverse, an own open shift, a positive amount,
reason and explicit FundsActuallyReturned=true. It cannot exceed the paid amount less
earlier returns. Each return appends ExpenseReversal, an Expense-source ledger credit
and audit; the original expense is preserved. No credit is created merely because an
operator wants to correct text. Returned money uses the original account. Legacy
expenses without explicit managed PaidAt cannot be assumed paid or automatically reversed.
The schema and returned-money history do not implement vendor refunds/reimbursements.

## Advances and discretionary withdrawals

EmployeeAdvance is distinct from Expense and Payroll. It records active receiving
employee, recording employee, branch, own shift, source account/method, amount,
UTC/business date, reason/reference and audit. Each actual advance writes one debit.
The [Monthly Closing](MONTHLY_CLOSING.md) extension now requires explicit branch
financial month/salary settings, caps cumulative advances at prorated branch BASIC
salary, and deducts each advance once at salary recognition. No interest or repayment
schedule is implemented.

Discretionary withdrawals require Vault.Withdraw **and** Branches.AccessAll, plus
the actor's own shift at that branch. Managers receive no default withdrawal grant.
Delegation is an explicit capability/global-scope decision. Operational expenses and
advances retain their separate permissions and classifications.

## Financial integrity, concurrency and retries

- Money uses the existing numeric(18,2), rejecting negative/invalid precision and
  zero meaningless payout/return amounts. Counts may be zero.
- A PostgreSQL CashDrawer row lock coordinates new financial writes, counts and
  closure for one branch before member/sale/account locks. Distinct branches have
  independent coordination. Bootstrap setup locks its branch before the drawer exists.
- Cash ledger rows have a monotonic drawer movement sequence; counts snapshot the
  boundary. This is an ordering counter, not a mutable balance.
- Existing financial/subscription transactions retain their atomic boundary. A failed
  payment/refund/entitlement or insufficient-Cash payout rolls back all new effects.
- Unique operation identities, request hashes and actor checks distinguish exact retries
  from changed requests. Previously committed financial results are returned before
  requiring a still-open shift. Keep both OperationId and original payload on retry.
- Shift closure requires ExpectedVersion; stale requests conflict. Closing snapshots
  include all branch movements committed before its count, including other employees.
- Financial facts, managed expenses, advances, counts and movements are append-only;
  closed shifts cannot be rewritten. Expense returns append compensating history.
- Legacy null Shift/date/sequence links are never repaired silently or inferred from
  overlapping timestamps. LegacyUnshiftedNetMovement exposes them in branch reporting.

## Authorization and setup

BLL and API both enforce capability and trusted branch scope. Normal clients omit
BranchId; a supplied unrelated branch requires Branches.AccessAll and the actual
own-employee operational assignment/shift for mutations. Sales ownership grants no
extra financial scope. Direct Allow/Deny overrides retain existing semantics; Deny wins.
Clients cannot submit authoritative employee, expected balance or BusinessDate fields
to new mutation contracts; unknown JSON fields are rejected.

Capabilities: Shifts.View/Manage; Vault.View/Manage/Withdraw; Expenses.View/Create/
Manage/Reverse; Advances.View/Create. Existing Expenses.Approve remains catalog-compatible
but is not used by this direct-paid workflow. No role-name authorization is used.

Fresh Development role templates give Sales/Reception own-shift, expense/advance
and operational viewing capabilities. Fresh BranchManager adds branch vault/category/
return management, with global scope still required for the shared category catalog.
Fresh Owner receives all capabilities. Existing role grants, inactive permissions and
custom user overrides are never replaced by seeding or catalog setup.

Explicit local catalog setup:

```sh
dotnet run --project CampionsSystem.API -- --configure-shift-permissions
```

It refuses non-Development or non-localhost targets. With no Shifts:GlobalRoleId it
only adds missing catalog entries; with an explicitly configured existing globally
capable role it adds missing active grants to that role. Other staff grants are explicit
administration. It does not provision users, shifts, accounts, money or expense categories.

The earlier Sales permission provisioning gap remains: local Development
Offers.Manage, Installments.Manage and Sales.QuickCancel have no role grants or direct
overrides. This feature does not silently assign those unrelated capabilities.

## API contracts and routes

All routes use the existing authenticated API and ProblemDetails conventions:
400 invalid business operation, 403 capability/scope/ownership, 404 missing record,
409 stale version/unique or conflicting concurrent mutation. Lists return
`{ items, total, page, pageSize }`; pageSize is 1–100. Optional date filters are
effective BusinessDate filters; unknown legacy dates are excluded by date bounds.

| Method | Route | Permission / additional conditions |
|---|---|---|
| POST | /api/shifts | Shifts.Manage, own active employee |
| POST | /api/shifts/{id}/close | Shifts.Manage, original opener, ExpectedVersion |
| GET | /api/shifts/current | Shifts.View; own open shift |
| GET | /api/shifts | Shifts.View; BranchId/EmployeeId/From/To, pagination |
| GET | /api/shifts/{id} | Shifts.View, branch scope |
| GET | /api/shifts/{id}/summary | Shifts.View + Vault.View; explicit shift activity |
| GET | /api/business-days/current | Shifts.View, optional authorized BranchId |
| POST | /api/business-days | Shifts.Manage; explicit date advance |
| POST | /api/business-days/{id}/close | Shifts.Manage; open shifts may remain |
| POST | /api/vault/setup | Vault.Manage + Branches.AccessAll; once per branch |
| GET | /api/vault/position | Vault.View; current physical/book/discrepancies |
| POST | /api/vault/counts | Vault.Manage, own open shift |
| GET | /api/vault/counts | Vault.View; bounded observation history |
| GET | /api/vault/movements | Vault.View; account/date filters, pagination |
| POST | /api/vault/withdrawals | Vault.Withdraw + Branches.AccessAll, own open shift |
| GET | /api/vault/summary | Vault.View; branch/date totals and current drawer |
| GET | /api/vault/audit | Vault.View; branch, optional EntityId, pagination |
| POST | /api/expenses | Expenses.Create; actual paid expense, own shift |
| GET | /api/expenses | Expenses.View; bounded branch/account/date history |
| GET | /api/expenses/{id} | Expenses.View |
| POST | /api/expenses/{id}/returns | Expenses.Reverse; actual returned funds, own shift |
| GET | /api/expenses/{id}/returns | Expenses.View; bounded return history |
| GET | /api/expenses/categories | Expenses.View; paginated dynamic global catalog |
| POST | /api/expenses/categories | Expenses.Manage + Branches.AccessAll |
| PUT | /api/expenses/categories/{id} | Expenses.Manage + Branches.AccessAll |
| POST | /api/employee-advances | Advances.Create, own shift |
| GET | /api/employee-advances | Advances.View; bounded branch/account/date history |
| GET | /api/employee-advances/{id} | Advances.View |

Existing /api/sales branch totals, account administration, Sales, payment and refund
routes remain their established endpoints. New reports add operational source totals:
collected/refunded, expenses/returns, advances, withdrawals, initial positions, other
ledger movements and net movement, separated by Cash/InstaPay. They do not equate net
cash movement to Sales revenue or calculate commission. CurrentSharedDrawer is always
the current position, while a closed Shift's cash snapshot remains historical.
Financial audit history covers Shift/Vault/Expense/Advance/BusinessDay events;
existing Sales/Payment/Refund audit and history remain in their original services.

Example shift open:

```json
{ "operationId": "b3d3586a-baad-4766-a2b0-dbf16d8f6201", "openingCash": 100000 }
```

Example result (selected fields):

```json
{ "id": "7c1798af-40cc-47e9-bb27-d2909a28726c", "employeeName": "Ahmed",
  "status": 0, "version": 1, "openingCash": 100000,
  "openedAt": "2026-10-12T06:00:00Z", "openingBusinessDate": "2026-10-12" }
```

Example close, preserving request body on retry:

```json
{ "operationId": "db420e27-89f2-444e-9149-f48ee0a764a0",
  "expectedVersion": 1, "countedCash": 107800, "reason": "Physical drawer count" }
```

Example paid expense (source account determines Cash/InstaPay):

```json
{ "operationId": "fe9a26eb-09be-4c0b-b370-f221ad4a9964",
  "expenseCategoryId": "a2a59fb1-1fa0-48b4-ab3d-8bf5f3e36d53",
  "financialAccountId": "b12a13af-51b6-45f5-ab16-2873979b3649",
  "amount": 500, "description": "Actual supplies payout", "receiptReference": "Receipt 123" }
```

## Persistence and verification

Reuse Shift, ShiftEmployee, Expense, ExpenseCategory, Payment/Refund/Sale links and
FinancialAccount/FinancialTransaction. New facts: CashDrawer, CashCount, CashMovement,
EmployeeAdvance and ExpenseReversal. Existing source enum values are preserved;
EmployeeAdvance and CashMovement sources are appended. Managed Shift/Expense fields
and new ledger links are nullable for legacy compatibility. Restrictive FKs, money
checks, unique operation identities, one-open-employee partial index, drawer/account
uniqueness, sequence uniqueness and branch/date/history indexes protect the model.

New migrations only:

- 20261010225923_PrepareEmployeeShiftsAndSharedDrawer
- 20261011133824_OrderSharedDrawerObservations

Generated SQL is in docs/database. Both were applied only to guarded localhost
Development. Historical migrations are unchanged; production is untouched. Tests use
disposable PostgreSQL schemas, including independent committed connections for races.
Coverage includes overlapping shifts, shared drawer/count ordering, physical carry-
forward, operational gates/retries, source separation, expenses/returns/advances,
global withdrawals, transfer/opening and close/payment/refund/payout races, branch
isolation, direct Deny overrides, legacy nulls and earlier Sales/Subscription/Check-In.
Final full suite: **733 passed, zero failed/skipped** (55 added cases over the
verified 678-test Sales baseline). Final build has zero warnings/errors; EF drift
verification is clean. Development catalog verification confirmed five support
tables, 36 indexes and 38 validated constraints, restrictive new foreign keys and
exact money precision. Seven missing capability catalog entries were added explicitly,
without changing existing grants/overrides. PROJECT_HISTORY.md records earlier fixture
failures and corrections as well as the final result.

Postman folder **16. Employee shifts, shared vault, expenses and advances** contains
28 examples with operation identity variables. Examples were not executed against
application data. Initialize an account/drawer and explicitly grant intended
capabilities before operational rollout; no fake initialization data is seeded.

## Deferred scope

Monthly closing, salary entitlements and employee payment records are implemented
in [Monthly Closing](MONTHLY_CLOSING.md); Private/Classes finance is implemented in
[Private / Classes](PRIVATE_CLASSES.md). Remaining deferred scope: discrepancy
resolution/ledger-adjustment approval, forced shift recovery, automatic/seasonal
closing, routine external Cash-In, Shift/Vault transfer approvals, frontend and
offline synchronization. Durable Guid facts, explicit branches/dates,
UTC instants and operation identities support a later sync design without building it.
