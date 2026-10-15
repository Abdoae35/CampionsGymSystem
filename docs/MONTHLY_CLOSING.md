# Monthly Closing, financial reports and employee payment records

Implemented in the existing API/BLL/DAL. Financial months are independent of Sales
and Coach commission periods. No production operations, target defaults, historical
backfill or role grants are part of this feature. Classes compensation remains
**NotConfigured** until the Owner defines its formula.

## Branch lifecycle

Create a financial month with explicit inclusive `StartDate` and `EndDate`, a name,
reason and stable nonempty `OperationId`. Bounds are 1–62 days. Each branch has one
open posting month; successors start the day after the preceding month ends.
Branches may close on different days and retain separate ledgers and reports.

Closing requires the branch's effective BusinessDate to reach the end date and
**every branch employee Shift to be closed**, including older forgotten shifts.
The existing shared drawer lock serializes closing against Shift opening and all
financial posting. Optional `CountedCash` is one branch recount using existing
CashCount evidence. Without it, the report captures the actual existing drawer
position. Shortages and surpluses remain visible and do not block closing or create
expenses, adjustments or money. Never add overlapping employees' drawer balances.

First closing recognizes prorated basic salaries and previously unrecognized
finalized Sales summaries. It freezes a report JSON snapshot and exact XLSX bytes
with SHA-256, authorizer, reason, timestamp and revision. Closed report/export reads
return that stored revision, unaffected by future names, settings or payouts.
`ExpectedVersion` protects close/reopen races. Repeating the same operation and
actor with identical input succeeds; reusing it with changed input rejects.

Reopening records an auditable `Reopened` state. It never reopens commission
periods, rewrites recognized payroll or recalculates finalized Sales/Coach facts.
Earlier report/workbook versions remain available. Reclosing issues another
immutable revision while retaining the **original financial cutoff and drawer**.
Present-day recounts use Vault, rather than replacing a historic closing count.
Known original occurrence dates may be appended as evidence for investigation.
No endpoint edits ledger amounts or overrides finalized refund restrictions.

Once financial months are configured, new financial operations need an **Open**
destination. A Reopened historic month is not a posting destination. Late discovered
operations go to the next open month: their actual timestamps and operational
BusinessDate remain intact, with a separate optional original-occurrence record.
An expired open date range must be closed and succeeded explicitly. Legacy branches
without configured months retain their existing posting behavior; employee advances
now require a financial month and salary entitlement.

## Basic salary, advances and employee payments

Owner administration appends effective-dated `EmployeeSalarySetting` history.
Different employees may have different rates; zero salary must be explicit.
Initial history can start on/after HireDate. Subsequent changes must be today/future,
chronological and cannot rewrite recognized salary. Missing or overlapping history
fails explicitly. The previous setting is closed once; its rate remains immutable.

Salary denominator is the inclusive financial period's actual number of days.
Eligible employment days run from HireDate inclusive to TerminationDate exclusive,
with branch responsibility taken from dated employee assignments. Dated suspension
and resignation gaps are excluded; **approved leave is paid**, as confirmed by the
Owner. Cash Shift counts are not attendance/payroll inputs. Existing status-change
audit evidence supplies the effective employment history; suspended employees with
no dated evidence require reconciliation. This is not a new HR/leave system.

The calculator rounds cumulative eligible daily entitlement to two decimals away
from zero and allocates incremental cents to that day's branch. Equal-boundary
transfer reports conserve total salary exactly. Different branch period bounds are
shown explicitly and are not presented as one common calendar payroll period.

Example: EGP 6,000 over 30 days, 15 eligible days in Tanta and 15 in Mahalla gives
EGP 3,000 responsibility each. A Tanta advance of EGP 1,000 leaves Tanta basic
payable EGP 2,000 and Mahalla EGP 3,000. It is not deducted again elsewhere.

Existing actual advances retain their funded account, receiving employee, recording
employee, Shift, branch, date and ledger evidence. Multiple advances cannot exceed
that employee's **prorated BASIC salary** for their branch/month; commission does
not increase the ceiling. Concurrent retries/payments serialize. Transfer, status
or salary changes that would strand already paid advances reject and need explicit
history reconciliation. Closing links each advance to one immutable PayrollDeduction.

Existing finalized New/Renewal commission summaries create separate payable Payroll
rows, identified by `CommissionSummaryId` and original `CommissionPeriodId`. Each
summary is recognized once. Open progress is provisional, not payable. Automatic
first-close recognition includes the branch's previously unrecognized finalized
Sales summaries; an Owner may recognize a later finalized period into the current
open financial month. Commission periods crossing financial months keep their full
original formula/boundaries; no proration or competing commission formula is used.
`POST /api/payroll/commission-recognitions` treats `operationId` as global across
branches. Concurrent requests with the same ID wait for one transaction to finish;
an exact retry by the same actor returns its recorded result, while a different
period or actor receives a request-identity error. Rolled-back attempts leave no
recognition marker and may be retried. A filtered unique audit index protects the
marker independently of branch drawer locks and the existing unique Payroll
commission-summary link.

A Manager can retain administrative capabilities and personal Sales ownership on
the same employee account. The unchanged commission engine follows personally
attributed collection evidence, not cashier identity or all branch revenue. Independent
New/Renewal targets and rates remain in existing CommissionRule infrastructure.

Owner records actual partial employee payments against an entitlement using amount,
actual `PaidAt`, reason and optional reference. Payments expose payable, paid and
outstanding; EGP 4,000 entitlement paid 2,000 then 2,000 gives 4,000 → 2,000 → 0.
The payment's posting financial month and recorded timestamp are separate from its
actual past/current payment date. No future payment date or overpayment is allowed.
Zero net entitlements are labelled `NoAmountDue`, rather than implying a payment.
**No funding account, cash drawer movement, Expense or FinancialTransaction is
created.** These are Owner-declared external payment records, not proof of a bank
transfer. Payment reversals/entitlement adjustments are not implemented.
Use this payment-record workflow for employee compensation; do not also enter the
same compensation as an ordinary paid Expense. Unlinked historical compensation
expenses need explicit reconciliation; descriptions/category names are not guessed
as payroll links.

## Private coaches and Classes

Reports reuse immutable CoachEarnings, session/expiry evidence, assignments and
existing funded CoachPayout/allocations. Normal configured Private rates are 50/60%;
incoming transferred-package attribution remains fixed 50%. Expiration earnings
are separate from delivered-session earnings and never fabricate attendance.
Historical source evidence includes purchased quantity/price/unit value/remainder,
coach assignment, transfer, actual Check-In or unused-expiry quantity/reason and
snapshotted target/rate. Missing settings do not block operational attendance.

Earned costs here mean **recognized immutable earnings**, not an invented estimate
for every delivered session. Unrecognized or unfunded Private value remains in the
existing Coach earnings progress/deferred workflow. Recognition after a financial
month closes belongs to the next open month; original commission/source dates remain
visible. Financial closing does not finalize Coach statements automatically.

Reports show period recognized earnings, actual payments, opening and closing
outstanding **at the original cutoff**, rather than today's live balance. Later
payouts do not change earlier revisions. Coach payment expenses remain one actual
existing funded ledger debit; they are excluded from ordinary expenses when earned
coach cost is separately included in the result. Existing shift/account/balance checks
and finalized-earnings refund protections remain in force.

Classes show individual occurrences, valid actual attendance and per-coach completed
class count, total attendees and average (including zero-attendee completed classes).
They consume existing shared Classes packages without reception Check-In. No Classes
compensation calculation, percentage or payroll entitlement is introduced.

## Financial report definitions

All Financial Transactions has one row per relevant **FinancialTransaction**. It
includes credits/debits from Payment, Refund, Expense/returns, EmployeeAdvance,
CashMovement/withdrawals and any existing Payroll/Adjustment entries. Cash/InstaPay
account type, actual occurrence/posting timestamps, BusinessDate, posting month,
branch, ledger/source/operation IDs, employee/Shift, customer, separate commercial
Sales owner, category, receipt, notes, audit JSON and reversal links are included
where actual evidence exists. Shift employee identity takes precedence over mutable
user-to-employee lookup. Legacy missing facts remain nullable.

PaymentParts, RefundParts and refund allocations explain split money and attribution;
they are **not additional financial rows or amounts**. Source evidence retains
purchased plan snapshots. Known original occurrence evidence is append-only. Undated
legacy ledger entries remain in all-time drawer book balance and are disclosed as
unassignable, rather than silently allocated to a month.

The summary distinguishes:

- Gross collections, actual refunds and net member collections.
- Net ordinary paid expenses by category (actual paid less actual returned funds).
- Earned basic salary, newly recognized finalized Sales commission and Private earnings.
- Employee external payment records, funded coach payouts, advances and outstanding.
- Ledger credits/debits/net cashflow, original book/physical cash and count discrepancies.

**Net operating result** is a management result on collected revenue:

`Net member collections − net ordinary paid operating expenses − prorated basic salary
− newly recognized finalized Sales commissions − newly recognized Private earnings`.

Payments, advances, withdrawals, cash-count discrepancies and maintenance suggestions
are not a second earned cost. This is not statutory accrual profit or a replacement
for the ledger. Provisional/unconfigured commissions are disclosed separately.
Example: EGP 50,000 net collections − EGP 10,000 ordinary expenses − EGP 6,000 basic
salary − EGP 2,500 finalized Sales commission − EGP 4,000 coach earnings = EGP 27,500.
Paying EGP 2,000 of the coach earnings reduces outstanding to 2,000; it does not
subtract another 2,000 from the earned-cost result.

## Informational maintenance

Owner configures a versioned branch plan: expected cost, frequency in months, first
due date, name, reason and active status. EGP 15,000 every 3 months suggests EGP
5,000/month and a next expected maintenance date. Frequency is 1–120 months. This
is **InformationalOnly**: no reserve balance, automatic allocation, debit, transfer,
withdrawal, expense or profit provision. Owner chooses whether/how to save money.
Actual maintenance uses the existing paid Expense workflow once.

## APIs and effective permissions

Bearer authentication and existing permission Allow/Deny overrides apply. Branch
operations use current authenticated branch unless an allowed branch ID is selected.
Branch managers need action capabilities and branch scope; no role-name bypass.
Owner-only administration additionally requires effective `Branches.AccessAll`.
No role grants are changed by migration or implementation.

FinancialMonths.View and Export authorize the complete composite branch report,
including its salary/commission/coach datasets. Standalone Salaries.View and
Payroll.View govern their respective APIs; they do not redact an authorized frozen
workbook. Provision these complete-report capabilities deliberately.

| Method | Route | Capability / additional requirement |
|---|---|---|
| POST | `/api/financial-months` | FinancialMonths.Manage |
| GET | `/api/financial-months` | FinancialMonths.View |
| GET | `/api/financial-months/{id}` | FinancialMonths.View |
| POST | `/api/financial-months/{id}/close` | FinancialMonths.Close |
| POST | `/api/financial-months/{id}/reopen` | FinancialMonths.Reopen |
| GET | `/api/financial-months/{id}/report` | FinancialMonths.View |
| GET | `/api/financial-months/{id}/transactions` | FinancialMonths.View |
| GET | `/api/financial-months/{id}/revisions` | FinancialMonths.View |
| GET | `/api/financial-months/{id}/events` | FinancialMonths.View |
| GET | `/api/financial-months/{id}/export` | FinancialMonths.Export |
| GET | `/api/financial-months/{id}/employees/{employeeId}/salary` | Salaries.View |
| GET | `/api/financial-months/consolidated?monthIds={id}&monthIds={id}` | FinancialMonths.ViewConsolidated + Branches.AccessAll + FinancialMonths.View |
| POST | `/api/financial-months/transactions/{ledgerId}/original-occurrence` | FinancialMonths.Manage |
| POST | `/api/employee-salaries/{employeeId}/settings` | Salaries.Manage + Branches.AccessAll |
| GET | `/api/employee-salaries/{employeeId}/settings` | Salaries.View; employee assigned historically to allowed branch |
| GET | `/api/payroll` | Payroll.View |
| POST | `/api/payroll/commission-recognitions` | existing Payroll.Manage + Branches.AccessAll |
| POST | `/api/payroll/{id}/payments` | Payroll.RecordPayment + Branches.AccessAll |
| GET | `/api/payroll/{id}/payments` | Payroll.View |
| POST | `/api/maintenance-plans` | Maintenance.Manage + Branches.AccessAll |
| GET | `/api/maintenance-plans` | Maintenance.View |
| GET | `/api/maintenance-plans/{id}/history` | Maintenance.View |

22 operations. Collection reads accept `page=1&pageSize=25` (size 1–100; page
1–1,000,000). Month/payroll/maintenance histories accept optional branch; payroll
also employee/month. Report, transactions and export accept optional `revision`.
Without revision, open months preview; previously closed months return latest frozen
revision, even while reopened. Consolidation selects 1–100 distinct months, one per
branch, and sums their summaries without combining ledgers.

Write DTOs reject unknown fields. Do not send computed salary, employee authorizer,
financialMonth/BusinessDate overrides or payment funding fields. Existing controller
errors: 400 validation/business rule, 401 unauthenticated, 403 denied scope/capability,
404 missing resource/revision, 409 stale concurrency version. Failed writes are atomic.
API contracts: [MonthlyFinanceContracts.cs](../CampionsSystem.BLL/MonthlyFinance/MonthlyFinanceContracts.cs).
Controllers: [MonthlyFinanceController.cs](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs).
Postman folder **19. Monthly closing, payroll and maintenance** contains every route.

Example requests (use deliberate real IDs/dates and stable retry UUIDs):

```json
{"operationId":"11111111-1111-4111-8111-111111111111","name":"October Tanta","startDate":"2026-10-01","endDate":"2026-10-23","reason":"Configured branch financial month","branchId":"22222222-2222-4222-8222-222222222222"}
```

```json
{"operationId":"33333333-3333-4333-8333-333333333333","expectedVersion":0,"reason":"All shifts reviewed and closed","countedCash":12500.00}
```

```json
{"operationId":"44444444-4444-4444-8444-444444444444","amount":2000.00,"paidAt":"2026-10-24T10:00:00Z","reason":"Owner actually paid first portion","reference":"Optional payment evidence"}
```

## Workbook and deployment

One XLSX per branch/month/revision, MIME
`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`, attachment
filename and `X-Workbook-SHA256`. Download is available after closing, with stored
bytes rather than regeneration. Six Excel-valid worksheets:

1. Monthly Summary
2. All Financial Transactions
3. Sales Performance
4. Employee Salaries & Advances
5. Coaches - Private - Classes
6. Vault & Shifts

Built with .NET ZIP/XML OOXML, with wrapped headers, frozen first rows and numeric
money cells. User text is inline text, never executable formulas. Long JSON evidence
is split into numbered columns without truncation. Stable ordering, bulk source
queries, cancellation, Excel row/column checks, 50,000 rows per dataset, 64 MiB report
JSON and 32 MiB workbook bounds prevent uncontrolled closing/export. Exceeding limits
rejects the close atomically. This is a bounded synchronous report, not a background
export service. Stored revision bytes ensure historical reproducibility.

Forward migration: `20261012164313_ImplementMonthlyClosingAndEmployeePayments`;
review [SQL](database/MONTHLY_CLOSING.sql). Existing historical migrations are
unchanged. Apply the complete prerequisite chain only through controlled deployment.
Disposable localhost PostgreSQL test schemas exercise migrations; application
database migration is a separate rollout step. Production is untouched.

Provision capabilities explicitly (11 new catalog keys; existing Payroll.View/Manage
are reused), configure existing drawer/accounts, financial dates and every eligible
employee's effective salary history. Existing Sales rules/periods and Coach targets
remain independently configured, with no invented defaults. Preserve Shift/BusinessDay
operations. Existing historical payroll without new month/source evidence needs
explicit reconciliation before being treated as a new payable obligation. Finalized
earnings adjustments, payout corrections and Classes compensation still require
separately approved rules. The pre-existing frontend handoff package is preserved;
this document and Postman describe the additional contracts.

## Verified checkpoint — 2026-10-13

Complete solution suite: **855/855 passed**, zero failures/skips, 27m20s; 33 new
cases over the prior 822 baseline. Focused 36/36 and final follow-up 7/7 passed.
Clean solution rebuild: zero warnings/errors. Final EF pending-model check: none.
All 50 historical migration files match the baseline byte-for-byte; forward SQL
matches EF generation (six tables, 28 indexes, 11 capability catalog inserts, no grants).
Migration behavior is exercised only in disposable localhost test schemas.
No application or Production migration/data/grant changes were performed.

Tests cover all-Shifts-close and concurrent Shift opening; optional discrepancy
counts; salary status/transfer/rounding/history; advance limits/retries and deduction
once; concurrent partial employee payments with no ledger; Manager personal Sales
ownership and immutable finalized summaries; partial coach balances by period,
transferred expiry evidence without attendance, Classes averages without compensation;
effective permission overrides/branch isolation; original frozen salaries/revisions;
late-entry routing; bounded atomic rejection and long Unicode/formula-safe workbook
evidence. Independent synthetic ZIP/XML checks verify six sheets, OOXML namespaces,
styles, relationships, numeric money and literal text. Native Excel was not launched.

All 22 source route/methods and required capabilities agree with the documentation
and Postman; eight write examples parse as JSON, variables and local links resolve,
and prior Postman folders/variables are preserved. No Postman operation was sent to
application data. Existing frontend-handoff contents remain unchanged.
