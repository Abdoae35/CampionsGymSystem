# Planned periodic expenses (V1)

Planned periodic expenses extend the existing append-only `MaintenancePlanRevisions` store. A plan is an Owner's estimate, not a paid expense or an actual reserve account. The API never posts a `FinancialTransaction`, creates an `Expense`, changes Cash/InstaPay balances, or reduces monthly operating profit. Actual payments continue through `POST /api/expenses` and are reported once from the financial ledger. V1 has no plan-to-expense reconciliation link.
Planning can be configured for an active branch before its shared cash drawer is set up; plan writes use their own transaction-scoped lock.

An authorized Owner with effective `Maintenance.Manage` and `Branches.AccessAll` may create, edit, or deactivate a plan using `POST /api/maintenance-plans`. Actual Owner membership is checked from Identity role membership at the service layer; a Manager granted those capabilities alone cannot mutate plans. Effective Deny still prevails. Reads require `Maintenance.View` and normal branch scope; no role grants are changed.

The request includes `operationId`, `branchId`, `name`, `description` (optional, up to 2,000 characters), `expectedCost` (positive EGP with at most two decimals), `frequencyMonths` (1–120), `firstDueDate`, optional `nextExpectedDueDate`, `reason`, optional `planId`, `expectedRevision`, and `isActive`. Dates must be in years 1–9900; next due cannot precede first due. Omit `planId` and use revision 0 for creation. For editing, send the stable `planId`, latest revision, and the complete desired plan. An omitted next due on an edit carries the previous one unless the first due date changes. Owner can advance a missed due date by explicitly editing the plan; no payment is implied. Reuse an operation ID only for an exact retry. A changed request with the same ID fails; a stale revision conflicts.
Retries of pre-extension requests that omit both new optional fields retain their original operation hash.

`suggestedMonthlyAmount = Round(expectedCost / frequencyMonths, 2, AwayFromZero)`. The branch total sums these individually rounded active-plan suggestions. For example, 15,000 / 3 = 5,000 EGP; 100 / 3 = 33.33 EGP. There is no accumulated reserve balance. `nextExpectedDate` is the persisted next due date or the first due date for older rows with no value. It does not silently advance with the clock. `dueStatus` is `Overdue` when next due is before the selected `asOf` date, `DueToday` when equal, `Upcoming` when later, and `Inactive` for deactivated plans. `daysUntilDue` is signed. These are informational reminders; the Owner explicitly edits the next due date after reviewing a cycle. Previously stored rows have no payment evidence, so older first due dates may initially appear overdue.

| Method | Route | Purpose |
|---|---|
| POST | `/api/maintenance-plans` | Create/edit/deactivate, append a revision; Owner + `Maintenance.Manage` + `Branches.AccessAll` |
| GET | `/api/maintenance-plans?branchId=&asOf=&page=1&pageSize=25` | Paginated latest plans, including inactive; `Maintenance.View` + branch scope |
| GET | `/api/maintenance-plans/summary?branchId=&asOf=&horizonDays=90` | Active count, branch monthly suggested total, overdue/today/upcoming counts; `Maintenance.View` + branch scope |
| GET | `/api/maintenance-plans/reminders?branchId=&asOf=&horizonDays=90&page=1&pageSize=25` | Paginated active due plans through the horizon, including all overdue; `Maintenance.View` + branch scope |
| GET | `/api/maintenance-plans/{planId}/history?page=1&pageSize=25` | Append-only revisions and historical values; `Maintenance.View` + branch scope |

`horizonDays` is 1–366; page size is 1–100. If `asOf` is omitted, the server's existing local BusinessDate convention is used. Summary's `upcomingCount` excludes due-today and overdue plans. Reminders are ordered by due date then plan ID, with `total` for the full matching dataset. Normal API 400/401/403/404/409 behavior applies. Branch IDs remain independent.

Existing financial closing revisions retain their frozen JSON/XLSX snapshots when a plan changes later. New closings include the plan state available at closing. The informational forecast is separate from actual expenses and net operating result. The only schema change is two nullable `MaintenancePlanRevisions` columns (`Description`, `NextExpectedDueDate`); the migration must be approved and applied to each target database before using this version. No Development or Production migration is applied as part of this feature.
The existing six-sheet workbook structure is unchanged; new reports can show the added forecast description and due-status fields in their informational maintenance section.
