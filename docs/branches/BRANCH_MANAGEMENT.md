# Branch management and employee assignments

GymBranch is reused with City, Name, Address, Phone, IsActive, borrowed-session
limit and refund-window settings. No schema changes, Employee.CurrentBranchId,
full Employee CRUD or hard-delete endpoint were added. Existing historical FKs
restrict deletion. Deactivate a branch through PUT, preserving its ID/history.

## Endpoints

All routes require JWT authentication and the listed effective permission.
Permission alone does not grant branch scope; see [isolation](BRANCH_ISOLATION.md).

| Method | Route | Permission | Scope |
| --- | --- | --- | --- |
| GET | /api/branches | Branches.View | Only current branch, or all with AccessAll |
| GET | /api/branches/{id} | Branches.View | Current branch or AccessAll |
| POST | /api/branches | Branches.Manage | AccessAll required to create a new branch |
| PUT | /api/branches/{id} | Branches.Manage | Current branch or AccessAll |
| GET | /api/employees/{id}/branch-assignment | Employees.View | Employee's current branch or AccessAll |
| GET | /api/employees/{id}/branch-history | Employees.View | Employee's current branch or AccessAll |
| POST | /api/employees/{id}/assign-branch | Employees.Manage | Target branch; outside-branch history requires AccessAll |
| POST | /api/employees/{id}/transfer-branch | Employees.Transfer | Both source and target branch scope |

Authorized management may read/update inactive branches. Operational branch context
excludes inactive branches. A scoped account that deactivates its own branch loses
operational branch access; an authorized global account can reactivate it.
No DELETE route exists, including for branches without history.

Save branch body (POST/PUT is a complete settings update):

```json
{
  "name": "Branch B",
  "cityId": "existing-active-city-guid",
  "address": "Optional address",
  "phone": "Optional phone",
  "isActive": true,
  "maxBorrowedGymSessions": 2,
  "quickRefundWindowHours": 24
}
```

New/changed cities must be active. An unchanged inactive city is allowed when editing
an existing branch. No City CRUD is included. Names/addresses/phones are bounded and
limits nonnegative; names cannot be whitespace. The schema does not require globally
unique branch names, so this feature does not invent that restriction.

## Current assignment and dates

Employee.UserId maps the Identity user to zero or one Employee. The shared
EmployeeBranchResolver evaluates `StartDate <= today` and `EndDate == null ||
EndDate >= today`. Today is the calendar date in AuthContext.TimeZoneId (default
Africa/Cairo), not the currently open financial BusinessDay. Both ends are inclusive.
Future and expired assignments are excluded; inactive branches provide no scope.

There must be exactly one date-valid assignment to resolve an operational branch.
Overlapping legacy primary/nonprimary rows fail closed with a null current branch;
management queries reject ambiguous history for explicit reconciliation. No old rows
are rewritten automatically. Owner/global users need no fabricated Employee/branch.
`GET /api/auth/me` retains its shape and uses this shared resolver. No branch ID is
stored in JWT or Employee; the next request reflects an effective transfer.

Initial assignment body:

```json
{ "branchId": "target-branch-guid", "startDate": "2026-10-21" }
```

Dates must be today or later, within employment dates; employee and branch must be
active. An open-ended assignment cannot overlap any existing interval, including
bounded/nonprimary rows. Reassignment after ended history is permitted without
overlap. A scoped manager cannot claim an employee with other-branch history through
initial assignment. Existing assignment queries return full dated history. A global
query for an unassigned employee may return 204 (null result); transfers with no current
assignment return 400. Backdated HR corrections are deliberately not implemented.

## Transfer

```json
{ "targetBranchId": "branch-b-guid", "effectiveDate": "2026-10-21" }
```

The service checks effective Employees.Transfer and branch scope, then starts a DB
transaction and locks the Employee row (`FOR UPDATE`). It rechecks employment/current
history, target branch/activity, same-branch/date/overlap/pending-transfer rules.
October 21 transfer ends the original assignment October 20 and creates a new primary
assignment starting October 21. EffectiveDate must follow the current StartDate: two
assignments cannot represent an intra-day transfer with this DateOnly model.

Closing the original row is saved before inserting the new row to release the existing
unique primary slot. Both saves and the audit are in one transaction, or a savepoint
when a caller already owns a transaction. Any failure rolls back both saves. All
assignment writers use the same employee row lock; the existing partial unique index
is a second defense. No distributed locks or new migration are needed.

A future transfer keeps the source branch current through the preceding day. An already
scheduled future assignment/transfer blocks another transfer; rescheduling/cancellation
requires a separate future workflow. Planned end dates cannot be extended implicitly.

Managers with only current-branch scope cannot transfer across branches. An authorized
global user must have both Employees.Transfer and Branches.AccessAll. Role names alone
provide neither permission nor global scope.

Transfers never change historical sales, payments, check-ins, shifts, commissions or
reports. Sales clients, leads, follow-ups, future shifts, payroll and other dependent
module consequences remain work for those features; no automatic reassignment exists.

## Audit and errors

Branch.Created, Branch.Updated, Branch.Activated/Deactivated,
Employee.BranchAssigned and Employee.BranchTransferred use existing AuditLogs.
Transfer audits identify employee, old/new branch, assignment IDs, effective date and
acting user. Audit writes are atomic with business changes and contain no secrets.

API responses use existing ProblemDetails conventions: 200 success, 400 business/date/
inactive/overlap errors, 401 unauthenticated, 403 permission/scope denied, 404 missing
accessible branch/employee, 409 database duplicate/serialization/deadlock conflict.
A concurrent loser may instead receive a 400 business conflict after rereading the
winner's committed assignment. Off-scope IDs return 403 without exposing their details.

## Verification

202 tests passed, including 46 additional branch/security cases. Tests cover HTTP and
BLL paths, same-JWT current branch changes, effective direct Deny, deactivation/no-delete,
rollback after the first transfer save and two independent-connection races. Standard
integration fixtures roll back isolated schemas. Race tests need committed visibility,
so they create an internally generated private test schema and drop it afterward.
No tests migrate or write to the configured gym application database.
