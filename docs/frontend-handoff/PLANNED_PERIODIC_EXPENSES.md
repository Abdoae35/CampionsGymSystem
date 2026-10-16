# Planned periodic expenses: V1 React API addendum

This addendum supersedes the maintenance-plan entries in the older handoff catalog/OpenAPI capture. Those captures predate Monthly Closing and later audit fixes; do not generate a new client for maintenance from them. Use the [feature contract](../PLANNED_PERIODIC_EXPENSES.md) and this addendum until the complete frontend handoff is recaptured. The checked-in source is authoritative.

Authenticate with the existing Bearer token. Render create/edit/deactivate only when the user is an actual Owner and effective `Maintenance.Manage` and `Branches.AccessAll` are available. The server verifies Owner membership independently of UI flags. Reads require effective `Maintenance.View` and branch scope. A Deny overrides inherited role grants. Choose a branch explicitly with `branchId` when permitted; otherwise use the current branch. All dates are `YYYY-MM-DD`; money is a JSON decimal number in EGP.

## Routes

| Method | Path | Response | Security |
|---|---|---|---|
| POST | `/api/maintenance-plans` | `MaintenanceForecast` | Owner + `Maintenance.Manage` + `Branches.AccessAll` |
| GET | `/api/maintenance-plans?branchId={guid}&asOf=2026-10-16&page=1&pageSize=25` | `ShiftPage<MaintenanceForecast>` | `Maintenance.View` + branch scope |
| GET | `/api/maintenance-plans/summary?branchId={guid}&asOf=2026-10-16&horizonDays=90` | `MaintenanceBranchSummary` | `Maintenance.View` + branch scope |
| GET | `/api/maintenance-plans/reminders?branchId={guid}&asOf=2026-10-16&horizonDays=90&page=1&pageSize=25` | `ShiftPage<MaintenanceForecast>` | `Maintenance.View` + branch scope |
| GET | `/api/maintenance-plans/{planId}/history?page=1&pageSize=25` | `ShiftPage<MaintenanceForecast>` | `Maintenance.View` + branch scope |

`branchId` and `asOf` are optional on reads. `horizonDays` defaults to 90 and accepts 1–366. Pages start at 1; page size accepts 1–100. `ShiftPage` serializes as `{items,total,page,pageSize}`. Summary counts all active plans in the authorized branch; reminder pagination includes all overdue and plans due through `asOf + horizonDays`, sorted due date then plan ID. `upcomingCount` excludes due-today and overdue.

Create body (`planId` omitted, `expectedRevision: 0`):

```json
{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "branchId": "22222222-2222-4222-8222-222222222222",
  "name": "Equipment servicing",
  "description": "Quarterly preventive service",
  "expectedCost": 15000.00,
  "frequencyMonths": 3,
  "firstDueDate": "2027-01-15",
  "nextExpectedDueDate": "2027-01-15",
  "reason": "Initial plan",
  "expectedRevision": 0,
  "isActive": true
}
```

`MaintenancePlanRequest` also accepts nullable `planId`. To edit or deactivate, send the complete desired body with the returned stable `planId`, latest `expectedRevision`, a **new** `operationId`, and `isActive: false` to deactivate. Omitted `nextExpectedDueDate` on an edit retains the previous value if first due date is unchanged; when first due changes it becomes the new first due date. To clear an overdue reminder, the Owner explicitly sets a later next expected due date in a new revision. That action does **not** record a payment. Reusing the same operation ID with altered input fails.

Forecast response example:

```json
{
  "id": "33333333-3333-4333-8333-333333333333",
  "planId": "44444444-4444-4444-8444-444444444444",
  "branchId": "22222222-2222-4222-8222-222222222222",
  "revision": 1,
  "name": "Equipment servicing",
  "expectedCost": 15000.00,
  "frequencyMonths": 3,
  "firstDueDate": "2027-01-15",
  "nextExpectedDate": "2027-01-15",
  "suggestedMonthlyAmount": 5000.00,
  "isActive": true,
  "status": "InformationalOnly",
  "description": "Quarterly preventive service",
  "dueStatus": "Upcoming",
  "daysUntilDue": 91
}
```

`MaintenanceBranchSummary` is `{branchId,asOf,horizonDays,totalSuggestedMonthlyAmount,activePlanCount,upcomingCount,dueTodayCount,overdueCount}`. Individual monthly suggestions are rounded to two decimal places away from zero **before** summing. `dueStatus` is `Overdue`, `DueToday`, `Upcoming`, or `Inactive`; `daysUntilDue` is signed. Existing plans with no stored next due use their first due date and may appear overdue because the system has no payment-completion evidence.
An active branch can have plans before a shared cash drawer is configured; plans have no cash movement.

Validation: positive `expectedCost` with at most two decimals, nonblank name up to 256 characters, optional description up to 2,000, frequency 1–120 months, dates in years 1–9900, next due no earlier than first due, nonblank reason, valid operation ID, and matching expected revision. Standard 400 validation/business error, 401 unauthenticated, 403 unauthorized/branch scope, 404 unknown plan, and 409 stale revision/transaction conflict apply. Never infer a paid Expense, an actual reserve balance, or profit reduction from these endpoints. Actual payment uses the existing Expenses API; V1 has no automatic plan/payment link.

This feature adds two nullable columns through migration `20261016114618_AddPlannedPeriodicExpenseDescriptionAndDueDate`. The API needs that migration on its target database before these routes are used. It was prepared and tested only against isolated schemas; Development and Production activation require separate approval.
