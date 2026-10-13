# Employee Management

## Employee vs ApplicationUser

Employee is the personnel/business profile; ApplicationUser is the Identity login.
Employee.UserId is optional and unique when present. Employee creation does not
create an account, password or role; normal Employee profile APIs cannot link or
replace an account. Account/role management is separate from these endpoints.

The profile contains Guid Id, name, phone, optional job title, Staff/Freelance
employment type, hire/termination dates and employment status. Responses include
current branch and HasSystemAccount without exposing authentication secrets. The
model has no dedicated employee email or employee code.

Effective salary administration, branch proration, advance ceilings and Owner partial
employee payment records are implemented through separate
[Monthly Closing](MONTHLY_CLOSING.md) APIs. Normal profile responses still do not
include a salary/payroll graph. Employment/status/transfer changes cannot strand
existing branch advances or rewrite recognized basic salary.

## Employee Status

| Status | Current behavior |
|---|---|
| Active | Eligible employment state; authentication/ownership also check dates and account capability |
| Suspended | Non-active employment; linked authentication fails and current customer ownership closes |
| Resigned | Ended employment; TerminationDate is set to the gym's current date and ownership closes |

There is no separate Terminated enum. Reactivation sets Active and clears termination
date. Resigned employment must be reactivated before suspension; repeating the same
status is rejected. Status changes do not toggle ApplicationUser.IsActive, although
authentication independently rejects non-active linked employment.

## Branch Assignment and Transfer

The effective-dated EmployeeBranchAssignments history determines current working
branch. Creation starts Active and may create an initial assignment atomically.
Scoped creators must select their own branch; global creators may leave an Employee
unassigned. Profiles cannot transfer branches; use the dedicated branch endpoints.

Transfer changes only working-branch history and preserves Member/Lead Sales ownership.
Ahmed may move A → B and retain both his old A Leads and customers from A/B. New B
Leads may then be assigned to him. See [Branch Management](BRANCH_MANAGEMENT.md).

## Termination / Suspension

The status transaction locks the Employee and closes current Sales assignments when
employment becomes non-active:

- Current MemberSalesAssignments receive EndedAt; affected Members become House.
- Current LeadAssignments receive EndedAt; Leads retain status, schedule and operational
  branch and become unassigned management redistribution work.
- Open Member follow-up tasks move into House queues through derived ownership.
- Assignment rows, interactions, Member profiles and historical transactions remain.
- No random reassignment occurs; reactivation does not reopen closed ownership.

Future-dated Sales assignments block closure pending explicit reconciliation. Employee
branch assignments are preserved rather than deleted by status changes. These are
customer-ownership effects, not an implemented subscription cancellation, payroll or
commission calculation workflow.

## Employee Gym Membership Foundation

Employee.MemberId is an optional unique link to a normal Member profile. The current
Employee CRUD does not create/link that Member or grant a subscription.

DAL supports an employment entitlement through MemberSubscription.EmployeeId, Unlimited
access and nullable EndDate for open Scheduled/Active employee entitlements. Restricted
FKs and partial unique indexes allow at most one open entitlement per Employee and
per linked Member. Employee plan definitions are internal, free and Unlimited.

These are model/constraint foundations. Automatic entitlement grant, employment-based
revocation and attendance enforcement are not implemented by Employee Management.
Updating employment status closes Sales ownership, not employee subscriptions.

## API Overview and Permissions

| Method | Route | Purpose / permission |
|---|---|---|
| GET | /api/employees | Search/filter/page; Employees.View |
| GET | /api/employees/{id} | Profile details; Employees.View |
| GET | /api/branches/{branchId}/employees | Branch directory; Employees.View |
| POST | /api/employees | Create personnel profile; Employees.Manage |
| PUT | /api/employees/{id} | Update profile; Employees.Manage |
| PATCH | /api/employees/{id}/status | Employment status; Employees.Manage |

Filters include branch, name/phone search, status, employment type and pagination.
Ordinary users are limited to Employees reliably assigned to their current branch.
Branches.AccessAll plus the action permission permits other branches/unassigned
Employees. There is no hard-delete endpoint or salary/payroll graph in responses.

Profile updates reject account, branch, status and historical fields. Audit actions
are EmployeeCreated, EmployeeUpdated and EmployeeStatusChanged; ownership closure also
writes Member.ReturnedToHouse and Lead.Unassigned. Branch assignment audits are separate.
