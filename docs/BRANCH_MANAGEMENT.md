# Branch Management

## Branch Model

GymBranch has a Guid identity and a required City relationship. It stores name,
address/phone, IsActive, CountryCallingCode, the Member ClientCode counter and
borrowed-session/refund-window configuration. These settings do not themselves
implement attendance, borrowing or refund execution. City is separate from branch
administrative scope; same-city branches remain isolated.

Branch create/update validates settings and active newly selected cities. An unchanged
inactive city may remain when editing a branch. Creating a branch requires
Branches.Manage plus Branches.AccessAll; scoped managers may edit their current
branch. Deactivation preserves IDs and history. No branch hard-delete route exists.
CountryCallingCode configures local Member/Lead phone normalization; changing it does
not rewrite persisted canonical phones. The internal counter is not client-editable.

## Employee Branch Assignment

EmployeeBranchAssignments retain Employee, Branch, StartDate, optional EndDate,
IsPrimary and assigning user. Dates are inclusive. Current scope requires exactly
one date-valid assignment and an active branch, using the configured gym calendar.
Future/expired rows do not grant scope; overlapping legacy rows fail closed.
An Employee does not have a separately maintained CurrentBranchId column.

Initial assignment requires active employment/branch, a start today or later within
employment dates, and no overlapping interval. A scoped manager cannot claim an
Employee with other-branch history through initial assignment. Global authorized
users can manage unassigned Employees. Backdated corrections are not exposed.

## Employee Transfer

```text
Ahmed: working branch A → B
Existing Members from A/B → still owned by Ahmed
Existing Leads from A → still assigned to Ahmed
New Leads from B → may now be assigned to Ahmed
```

Transfer closes the source assignment on the preceding day and creates a new primary
assignment on the effective date. It requires Employees.Transfer and access to both
branches; ordinary current-branch managers cannot perform an A-to-B transfer without
global scope. The effective date must follow the current assignment's start date.
Future transfers keep A current until the effective date. Pending/overlapping transfers
are rejected; no rescheduling/cancellation workflow is exposed.

The service locks the Employee and saves history changes plus audit in one transaction
(or ambient savepoint). It does not close MemberSalesAssignments or LeadAssignments,
change Member registration branches, change Lead operational branches, or transfer
historical transactions. Previously owned CRM work remains visible across branches.

## Transfer vs Termination

| Operation | Sales ownership effect |
|---|---|
| Branch transfer while employed | Existing Member/Lead assignments remain active |
| Employment becomes Resigned or Suspended | Current Sales Member and Lead assignments close atomically |
| Employment reactivated | Closed assignments are not restored automatically |

On non-active employment, Members become House and open Member work enters management
House queues. Leads remain in their operational branch, unassigned for management
redistribution. History remains; there is no random reassignment. Future-dated Sales
assignments must be reconciled before status closure can complete. See
[Employee Management](EMPLOYEE_MANAGEMENT.md).

## Cross-Branch Data and Security

Historical records retain their stored transaction branches. Employee transfer is not
a financial migration and does not recalculate revenue or commission. Ownership gives
access only to the specific CRM subjects/workflows, not the old branch's directory,
employees or financial areas. New Lead distribution normally requires the target's
current operational branch to match the Lead branch.

Plan ownership also follows its explicit BranchId; CityId is access configuration,
not administrative authority. Member branch transfer is not an implemented workflow.

## API Overview

| Method | Route | Purpose / permission |
|---|---|---|
| GET | /api/branches; /api/branches/{id} | List/details; Branches.View |
| POST / PUT | /api/branches; /api/branches/{id} | Create/update; Branches.Manage plus applicable scope |
| GET | /api/employees/{id}/branch-assignment | Current assignment; Employees.View |
| GET | /api/employees/{id}/branch-history | History; Employees.View |
| POST | /api/employees/{id}/assign-branch | Initial assignment; Employees.Manage |
| POST | /api/employees/{id}/transfer-branch | Transfer; Employees.Transfer |

Audit actions include Branch.Created/Updated/Activated/Deactivated and
Employee.BranchAssigned/BranchTransferred. See [Authorization](AUTHORIZATION.md) for
scope resolution and [older branch API details](branches/BRANCH_MANAGEMENT.md) for
request examples and date/concurrency rules.
