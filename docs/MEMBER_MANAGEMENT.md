# Member Management

## Member Identity

| Identifier | Meaning |
|---|---|
| Id (Guid) | Global technical identity used by PKs, FKs and internal references |
| ClientCode (nullable bigint for legacy data) | Positive sequential customer number within the original registration branch |
| MemberCode | Separately unique global display/code string; new registrations use M- plus a random Guid |

MemberCode is retained for identification; QR/barcode scanning is not implemented by
Member Management. Employee.MemberId may link an Employee to the same normal Member
profile; there is no separate employee-customer model.

## ClientCode

HomeBranchId supplies the ClientCode namespace. Registration atomically increments
GymBranch.LastMemberClientCode with PostgreSQL `UPDATE ... RETURNING`, inside the same
transaction as Member, ownership/eligibility and audit writes. Concurrent writers to
one branch serialize; other branches have independent counters. Failed registration
rolls back allocation. There is no request-time unprotected MAX+1 or offline allocator.

The positive check and partial unique `(HomeBranchId, ClientCode)` index are final
constraints. A:100 and B:100 are valid; two A:100 Members are not. ClientCode alone may
match multiple branches; use the branch/code pair to disambiguate. Legacy uncoded
Members remain null. Ordinary creation/update cannot supply or change ClientCode.

## Registration/Home Branch

HomeBranchId currently means the original registration branch, exposed as
RegistrationBranch in responses. It is not the Sales employee's working branch.
Visits, referrals, employee transfers and ownership changes do not change it.
No Member branch-transfer, administrative renumbering or legacy import API is implemented.

## Phone Numbers

PrimaryPhone preserves staff-entered display data with outer whitespace trimmed.
PrimaryPhoneNormalized is the persisted canonical comparison value. SecondaryPhone
is optional display/contact data without independent uniqueness.

Shared PhoneNormalizer removes common formatting. For local input it removes one
leading zero and prefixes the registration branch's CountryCallingCode. International
`+` input is not prefixed again. There is no hardcoded country default or complete
international-number subsystem.

```text
Branch CountryCallingCode = +20
01212345678       → +201212345678
+20 (12)1234-5678  → +201212345678
```

A partial unique normalized-phone index protects global canonical uniqueness. Display
phones alone are not globally unique across countries. Safely derivable legacy phones
participate in duplicate checks; unresolved legacy values are preserved, not guessed.
Changing a branch calling code does not rewrite historical normalized numbers.

Any primary display-phone change requires Members.ChangePrimaryPhone as well as Edit.
A canonical change clears verification flags; formatting-only changes retain them.
Unchanged legacy phones may remain unnormalized. Conflicting Leads must be reconciled
through CRM rather than bypassed through Member registration or phone edits.

## Member Creation Types

Registration requires Members.Create, an authorized active target branch, valid
profile/phone, duplicate checks, ClientCode allocation and transactional audit writes.
It creates no subscription, sale, payment or CheckIn.

| Request creation type | Ownership rule |
|---|---|
| NormalSales | Linked active eligible Employee with Members.OwnSalesCustomers becomes owner; arbitrary SalesEmployeeId is rejected |
| ManagementCreated | Members.AssignSales plus explicit eligible SalesEmployeeId; manager need not be an owner themselves |
| FreelanceHouse | Explicit active Freelance plan/Coach/city validation and FreelancePlans.ManageEligibleMembers; no Sales assignment; adds eligibility |

Normal registrations cannot silently become House by omitting an owner. Cross-branch
NormalSales referral requires Members.CreateCrossBranch, generates destination-branch
ClientCode and leaves the employee's working branch unchanged. FreelanceHouse requires
destination scope and plan administrative scope; it cannot bypass another branch's
plan. Existing Member eligibility management is separate and permits other-registration-
branch Members without changing ownership. See [Membership Plans](MEMBERSHIP_PLANS.md).

## Sales Ownership and House Members

MemberSalesAssignments is the source of truth. A current Sales row has EndedAt null,
AssignmentType Sales and a SalesEmployeeId. Eligibility is validated on assignment. At most one open assignment row per
Member is protected by a partial unique index and transactional Member locking.

**House means no current active Sales assignment.** IsHouse in responses is derived;
there is no authoritative IsHouse column or fake House employee. Legacy explicit
House assignment rows are handled without treating them as Sales ownership.

Manual assignment/reassignment requires Members.AssignSales and Member branch scope.
The target requires active employment/account and effective Members.OwnSalesCustomers.
Old assignment rows are closed before new rows are inserted. Return-to-House closes
the current assignment and creates no replacement Sales row; history remains.

Ownership follows the Employee, not their working branch. Ahmed in B may still own
Members registered in A and B after an A → B transfer. Formal reassignment or
employment becoming Resigned/Suspended closes ownership; transfer does not. Status
reactivation does not restore closed ownership.

## Cross-Branch Visibility

Ownership permits specific CRM follow-up work, not unrestricted other-branch Member
browsing. `/api/members` list/details stay scoped to the actor's branch, or global
scope with Branches.AccessAll. CRM queues expose lightweight owned cross-branch
contact information. No financial/employee access is granted by ownership.

## Profile, Status and Queries

Normal update edits name, phones, birth date, gender and notes. It cannot change Id,
ClientCode, MemberCode, HomeBranchId, Sales owner, verification flags or status;
unknown input fields are rejected. Status uses IsActive and its dedicated endpoint.
Activation/deactivation preserves assignments, subscriptions, invitations, CheckIns,
financial history, Employee links and audit. No hard-delete route exists.

Lists support name/display or normalized-phone search, MemberCode/ClientCode search,
exact ClientCode, registration branch, active state, Sales owner, House filter and
pagination (pageSize 1–100). Details include profile, branch, current owner, derived
House state, optional Employee link and lightweight eligible Freelance plans. They do
not load full subscriptions, payments, attendance or commissions.

## API Overview and Permissions

| Method | Route | Purpose / permission |
|---|---|---|
| GET | /api/members | List/search/filter; Members.View |
| GET | /api/members/{id} | Details; Members.View |
| POST | /api/members | Register; Members.Create plus creation-type capabilities |
| PUT | /api/members/{id} | Profile; Members.Edit, ChangePrimaryPhone when applicable |
| PATCH | /api/members/{id}/status | Activate/deactivate; Members.ChangeStatus |
| GET | /api/members/{id}/sales-assignment | Current owner; Members.View |
| GET | /api/members/{id}/sales-history | Ownership history; Members.ViewSalesHistory |
| POST | /api/members/{id}/assign-sales | Assign/reassign; Members.AssignSales |
| POST | /api/members/{id}/return-to-house | Close ownership; Members.AssignSales |

Permissions and branch scope are checked inside services as well as controllers.
OwnSalesCustomers, AssignSales and CreateCrossBranch are separate capabilities.
The secure assembly-internal CRM creation path derives its owner from the locked
Lead, never a client-supplied trusted owner. See [CRM conversion](CRM.md).

## Audit Events

Member.Created, Member.Updated, Member.PrimaryPhoneChanged, Member.StatusChanged,
Member.SalesAssigned, Member.SalesReassigned and Member.ReturnedToHouse use AuditLogs.
FreelanceHouse registration also logs FreelancePlan.MemberEligible. OperationAccess
captures actor, branch, entity, old/new values and timestamp; it does not automatically
populate HTTP IP/device metadata. Automatic CRM work generation on Member status,
expiry or attendance is not connected; explicit follow-up opening is available.
