# Employee management

Employees are personnel records, separate from Identity accounts. `Employee.UserId`
is optional. Creating an employee does not create a user, password or role, and
these APIs cannot link/change an account. Responses expose only `hasSystemAccount`,
not Identity secrets. The current model has no employee email or employee code.

## Endpoints

| Method | Route | Permission |
| --- | --- | --- |
| GET | /api/employees | Employees.View |
| GET | /api/employees/{id} | Employees.View |
| GET | /api/branches/{branchId}/employees | Employees.View |
| POST | /api/employees | Employees.Manage |
| PUT | /api/employees/{id} | Employees.Manage |
| PATCH | /api/employees/{id}/status | Employees.Manage |

Existing `Employees.Manage` covers creation, profile edits and status changes;
no speculative new grants or role bypasses were added. Branch assignment, history
and transfer remain the existing endpoints described in
[branch management](../branches/BRANCH_MANAGEMENT.md).

## Lists, search and counts

Example: `GET /api/employees?branchId={id}&search=Ahmed&status=Active&employmentType=Staff&page=1&pageSize=25`.
Search is case-insensitive literal matching on name or phone. Optional filters
combine. Status values are Active, Suspended, Resigned; type is Staff or Freelance.
Page defaults to 1; pageSize defaults to 25 and is limited to 100. Stable ordering
is FullName then Id. Responses contain `items`, `totalCount`, `page`, `pageSize`.
`totalCount` counts all matching employees before paging. To see how many employees
are assigned to a branch, use its employee list without additional filters; filter
`status=Active` when counting only active employees. No separate count endpoint.

Each item/detail includes Id, FullName, Phone, JobTitle, EmploymentType, Status,
HireDate, TerminationDate, CurrentBranch and HasSystemAccount. Salary, payroll,
commission, shifts and leave are not loaded. Queries use AsNoTracking and apply
scope/filter/count/page in PostgreSQL before materializing records.

## Scope

Permission determines the action; branch scope determines which records. Ordinary
users see employees with exactly one current assignment to their active branch.
A current assignment uses inclusive StartDate/EndDate and the configured gym date,
not a transaction's historical branch. Ambiguous assignments fail closed. Branch
filters and branch routes cannot override the caller's scope. Off-scope detail IDs
return 404; forbidden branch filters/writes return 403. A user with no operational
branch and no global permission receives 403.

Effective `Branches.AccessAll` plus the relevant employee action permission allows
all branches and unassigned employees. Owner role alone is not a bypass. Global
unfiltered lists include historical/inactive and unassigned employees. CurrentBranch
is null if no reliable active branch exists; the existing resolver is reused.
Removing a current assignment can make an employee globally visible only, including
ended employees; historical assignments do not grant current branch access.

## Create and update

POST example:

```json
{
  "fullName": "Ahmed Hassan",
  "phone": "01000000000",
  "jobTitle": "Trainer",
  "employmentType": "Staff",
  "hireDate": "2026-10-07",
  "initialBranchId": "REPLACE_WITH_EXISTING_BRANCH_GUID"
}
```

Only global users may omit initialBranchId. Scoped creators must select their own
branch. Initial assignment calls the existing assignment service, using today or
the hire date when it is in the future. Employee, assignment and both audits are
atomic; inactive/invalid branch assignment rolls everything back. Creation starts
Active. No system account is created.

PUT sends all profile fields above except initialBranchId. Omitted required fields
fail validation. Profile updates cannot transfer branches or change status, accounts,
salary, permissions or historical records. Unknown body fields are rejected with
400, including branchId, userId, initialBranchId on PUT and terminationDate. Hire
Date cannot follow an existing assignment start or termination date. Whitespace-only
names/phones, invalid enums and missing hire dates are rejected.

## Status and history

```json
{
  "status": "Suspended",
  "reason": "Pending management review"
}
```

Use PATCH for Active, Suspended or **Resigned**. The existing database calls ended
employment Resigned; there is no Terminated enum value. Setting Resigned records
today in TerminationDate and rejects a hire date later than today. Reactivation
sets Active and clears TerminationDate. An ended employee must be reactivated before
suspension. Setting the same status again returns 400. Reason is optional, up to
2048 characters, and is stored in the audit; no status-reason schema field was added.

Status changes preserve the employee, branch history and all old business records.
They do not rewrite shifts, client assignments, sales, payments or payroll, or
modify ApplicationUser activation. Existing authentication rules check employee
activity during login; no new token-revocation feature is introduced here.
No hard-delete endpoint is provided. Future HR correction/backdated termination and
account management are separate workflows, not part of these APIs.

Profile/status writes reuse the employee transaction/row lock used by transfers,
refresh tracked values under that lock, check scope and save audit atomically.
Actions are EmployeeCreated, EmployeeUpdated, EmployeeStatusChanged; existing
Employee.BranchAssigned and Employee.BranchTransferred are reused unchanged.

## Testing and Postman

Re-import `postman/Campions.Auth.postman_collection.json`. The Employee management
folder includes all six new routes. Login through the primary Login request as an
account with effective action/scope permissions. Set employeeId/branchId/hireDate
and select records deliberately; creation captures its returned employeeId.
Run modifying requests individually in development, not the entire collection.
Automated coverage uses isolated PostgreSQL fixtures and WebApplicationFactory;
no app database writes are made by these tests.
