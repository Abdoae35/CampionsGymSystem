# Monthly Closing implementation plan

Baseline inspected: clean `MonthlyClosing&FinancialReports`, HEAD
`1a9c81116950a399c349b05d61cc38be60dc1e5b`. Existing frontend handoff is
tracked and preserved. No Production operations or historical migration edits.

## Checkpoints

1. Persistence and posting boundaries: independent branch financial months,
   immutable closing revisions/events, effective salary history, advance evidence,
   payroll entitlements/partial payment records, informational maintenance plans.
   Forward migration, exact decimals, restrictive foreign keys, idempotency and guards.
2. Lifecycle, salaries and payment APIs: serialize close/post/shift-open using the
   existing drawer lock; require all branch shifts closed; optional cash count;
   versioned close/reopen without commission mutation. Salary proration uses
   inclusive financial-period days, employment dates (termination exclusive), and
   branch-assignment days; dated suspension/resignation gaps are excluded, and
   approved leave is paid (Owner confirmed during implementation). Missing/overlapping salary or assignment history fails
   explicitly, rather than inventing amounts. Cap advances at prorated basic salary,
   preserve evidence and deduct each advance once. Owner records partial employee
   payments without a funding account or ledger post. Maintenance is forecast only.
3. Reports/export: enrich each ledger row once with source/actor/commercial-owner
   evidence; reuse Sales commission calculations and immutable coach earnings.
   Separate cashflow, earned costs and payment records. Freeze report data and exact
   XLSX bytes per closing revision; six sheets, stable ordering, bounded bulk reads.
   Add branch-scoped APIs and Owner consolidated summaries without merging ledgers.
4. Full regression, migration SQL/drift, rollout/API/Postman documentation, required
   history handoff; review, commit and push each verified implementation checkpoint.

## Accounting and lifecycle definitions

- Financial months have explicit inclusive BusinessDate bounds and contiguous
  branch-local succession. They are independent of commission periods.
- Reopened months are review/correction states, never automatic destinations for
  new financial postings. Newly discovered operations post to the next open month;
  operational BusinessDate and real timestamps remain unchanged. Known original
  occurrence timestamps are separate evidence. No finalized refund bypass.
- Legacy undated transactions are disclosed, not silently assigned. Dated legacy
  transactions may appear by their existing BusinessDate in the first closing;
  immutable source rows are not backfilled or rewritten.
- Existing finalized Sales summaries determine payable commissions; open period
  progress is clearly provisional. An existing summary can create only one payable
  entitlement. Commission periods crossing financial bounds retain their full
  original calculation; recognition is reported separately from collection dates.
- Basic salary accrues by eligible employment/branch dates, not by the number of
  cash shifts. Advances are actual ledger payouts and offsets against the same
  branch's basic salary, not additional earned costs. Reopening preserves recognized
  payroll and finalized commission/coach facts.
- Net operating result is net member collections minus net ordinary paid operating
  expenses (excluding coach payout expenses and payroll/advances), minus prorated
  basic salary and recognized Sales/coach earnings for this financial month.
  It is a defined management result on collected revenue, not statutory accrual
  profit. Payouts reduce outstanding obligations, never earned costs a second time.
  Provisional/unconfigured compensation is disclosed separately.
- Owner employee payment records are externally funded informational evidence,
  not cash ledger transactions. Coach payments keep their existing funded Expense
  debit once. Maintenance suggestions never affect cash, expenses or this result.

## Verification and operational prerequisites

Real isolated PostgreSQL tests cover close/open races, branch scope, date routing,
immutable revisions, discrepancies, salary transfers/history, advance caps/retries,
deferred payments, independent commission attribution, report arithmetic and workbook
reproducibility. Existing tests remain in the solution. Run clean build, full suite,
EF pending-model check and forward SQL validation. Apply migrations only if the
Development host/database is independently verified; Production is untouched.
New capability catalog entries are not role grants. Deployment must explicitly
configure permissions, months and salaries. Classes compensation remains unavailable.
