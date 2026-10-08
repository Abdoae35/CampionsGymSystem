# Member Management backend

Implemented on MemberManagement, using the existing DbContext, permission policies,
branch access checks, audit helper and Employee Management response conventions.
Member registration is independent from purchasing or attendance. All endpoints are
under `/api/members`; successful operations return HTTP 200 (a missing current owner
may serialize as HTTP 204 through MVC's existing null formatter). Validation failures
return 400, denied permissions/scope 403, inaccessible read IDs 404 and concurrent
unique/serialization conflicts 409. Anonymous calls require authentication (401).

## Identity and profile

- `Id`: Guid global primary/foreign-key identity; unchanged.
- `ClientCode`: positive bigint, sequential within the original registration branch.
  Nullable only to preserve uncoded legacy records. New registrations always allocate it.
- `MemberCode`: existing independently unique global display/QR/barcode string, retained.
  New registrations generate `M-` plus a random Guid; no scanning is implemented.
- `HomeBranchId`: existing original registration branch, exposed as RegistrationBranch
  in responses. No duplicate persisted RegistrationBranchId was introduced.
- FullName, PrimaryPhone (staff-entered display, outer whitespace trimmed),
  PrimaryPhoneNormalized, optional SecondaryPhone, DateOfBirth, Gender, Notes,
  IsPrimaryPhoneVerified, verification timestamp, IsActive, creator and creation time.
- Existing Employee.MemberId link, subscriptions, invitations, CheckIns, Sales history,
  eligibility and financial relationships are retained. Member Management does not
  create or operate employment entitlements.

`Id`, ClientCode, MemberCode, registration branch, verification flags, status and Sales
ownership are absent from normal update input; unknown fields are rejected. Profile
updates edit only name, phones, date of birth, gender and notes. Status and ownership
use their dedicated endpoints. There is no Member delete endpoint.

## Branch ClientCode allocation

Migration `20261008110432_AddBranchClientCodeSequenceSupport` adds
`GymBranches.LastMemberClientCode` (bigint, default 0, nonnegative). Migration-time
initialization uses existing maximum codes per branch under the schema transaction;
existing Members are never renumbered or backfilled with fake codes.

Registration performs atomic `UPDATE ... SET LastMemberClientCode = LastMemberClientCode
+ 1 ... RETURNING` against the active target branch, in the same transaction as Member,
assignment/eligibility and audit writes. PostgreSQL serializes writers to that branch
row. Different branches have separate rows. Failed registration rolls allocation back;
no promise of permanently gap-free numbering or offline allocation is made. Exhausted
bigint capacity fails validation rather than overflowing. No unprotected request-time
MAX+1, new table, distributed service or global sequence is used.

Existing partial unique `IX_Members_HomeBranchId_ClientCode` and the positive check
remain the final database protection. Branch A ClientCode 100 and Branch B ClientCode
100 are valid; two Members with 100 in the same branch are invalid. Code-only search
can return multiple Members. Explicit branch plus ClientCode filters disambiguate.
Future imports/admin correction must preserve exact identifiers, lock the branch and
advance its counter as necessary; neither import nor correction is implemented here.

## Phones and branch calling codes

`GymBranches.CountryCallingCode` is nullable for legacy branches, configurable through
existing branch create/update API contracts, validated as `+` and 1–3 digits starting
with 1–9. There is no implicit country default. Authorized branch management configures
it before local-number registration/phone changes. BranchDetails includes the setting;
client requests never control the internal counter.

The shared `PhoneNormalizer` accepts digits and common spaces, parentheses, dots and
hyphens. It removes formatting; for local input it removes one leading zero and prefixes
the registration branch calling code. Input beginning `+` is already international and
never receives another prefix. The result must have 7–15 digits, with a nonzero first
digit. This is a simple format/canonicalization policy, not country-specific validation,
carrier validation, phone verification or a complete international numbering library.

Examples:

| Branch calling code | Staff input | Persisted normalized phone |
| --- | --- | --- |
| +20 | 01212345678 | +201212345678 |
| +44 | 01212345678 | +441212345678 |
| +44 | +20 (12) 1234-5678 | +201212345678 |

The globally unique partial index on PrimaryPhoneNormalized provides concurrent duplicate
protection. PrimaryPhone's previous unique index becomes a normal lookup index: identical
local display strings in different countries can represent different canonical phones.
BLL performs canonical duplicate prechecks and conservative comparisons with safely
derivable legacy phones; database conflicts remain 409 under races.

Migration backfills only valid, unique international representations. Local/invalid
phones and international collisions remain unchanged with normalized value null; migration
reports their aggregate count. No branch country is guessed. Existing ambiguous records
require explicit reconciliation; absolute duplicate-person detection is impossible while
legacy identity data is unresolved. Normal profile edits can preserve an unchanged legacy
phone without requiring normalization. New/changed phones must be normalized.

Any primary display-phone change requires Members.ChangePrimaryPhone in addition to Edit.
A canonical-number change clears verification and its timestamp. Changes in display
format with the same canonical number retain verification. Changing a branch calling code
does not rewrite previously persisted normalized phones; an unchanged phone stays unchanged.
SecondaryPhone remains optional display/contact data, without an independent unique policy.
Future invitation/integration services should reuse this canonicalizer when implemented.

## Registration workflows

Common flow: permission and request validation → explicit creation type → eligible owner
if required → authorized active target branch → target-branch counter lock/allocation →
phone normalization/duplicate checks → Member → assignment or Freelance eligibility → audit
→ atomic commit → MemberDetails response. No subscription, sale, payment, financial
transaction or CheckIn is created.

NormalSales requires Members.Create + Members.OwnSalesCustomers and a linked active
employee/account. Ownership resolves from the authenticated context; SalesEmployeeId and
FreelancePlanId are rejected. A Sales assignment is created and the Member is not House.

ManagementCreated requires Members.Create + Members.AssignSales and an explicitly selected
eligible SalesEmployeeId. The manager does not need OwnSalesCustomers. Missing owner is
rejected: a normal registration is never silently House. Members.Create alone does not
authorize a caller to choose arbitrary owners or create a normal unowned Member. Reception
must receive the appropriate workflow permission if authorized to register for a selected
Sales owner; it never automatically owns customers simply because it has Create.

For NormalSales referrals to another branch, Members.CreateCrossBranch is additionally
required, even when the actor has Branches.AccessAll; the actor must have current branch
context. Example: Ahmed works in A; customer chooses B; Member.HomeBranchId = B; ClientCode
comes from B; current Sales owner = Ahmed. Ahmed's assignment stays in A. The creation
response includes the new Member, but referral/ownership grants no additional browsing,
employee or financial scope. Later list/detail/update/history calls continue to require
normal branch visibility. Global branch access remains an independent permission.

FreelanceHouse requires Members.Create + FreelancePlans.ManageEligibleMembers, an explicit
active FreelancePlanId in the registration branch's city, active coach with current Freelance employment type and no
SalesEmployeeId. It creates eligibility but no Sales assignment; Member is House. It does
not buy a plan. Later normal/Freelance purchases and any commission consequences remain
outside this feature. Cross-branch referral permission is scoped to NormalSales; it cannot
be used to create or manage arbitrary House customers in another branch.

## Ownership and status

House is derived from no current Sales-type assignment (EndedAt == null). Legacy explicit
House history rows remain supported and are never deleted; no IsHouse database column was
added. Employee permission eligibility is checked through effective permission resolution,
including active permissions, role grants and direct overrides, plus active employment,
hire/termination dates, linked active account and lockout state. There is no runtime Sales
role-name check or same-branch restriction on the selected Sales employee.

AssignSales serializes writers on the Member row. It validates branch scope and target
eligibility, closes the current assignment, saves that closure, then appends a new Sales
row and audit in the same transaction. Existing one-current-row partial unique index
prevents duplicate current rows, including races outside this service. Assigning the
same current owner is rejected. Future-dated inconsistent current assignments require
reconciliation; old assigned timestamps are never rewritten.

Return-to-House closes the current row and creates no new row. Existing assignment history
remains available, including legacy House rows. Management can assign again later. Sales
may own Members registered in multiple branches without changing staff branch assignments.

Status uses the existing IsActive bool (Active/Inactive). Repeating the same status or
omitting it is rejected. Status changes do not close Sales assignments or alter employment
entitlements, subscriptions, invitations, CheckIns or financial records. History remains.

Ownership is sufficient for future branch-specific attribution. Future revenue belongs to
the actual transaction branch; future eligible revenue/targets/rates must be calculated
separately for each branch, never combined into one global target bucket. No commission,
30K threshold, percentage or payroll implementation is included here.

## Queries and details

GET list filters: Search, RegistrationBranchId, ClientCode, IsActive, SalesEmployeeId,
HouseOnly, Page and PageSize. HouseOnly=false selects Sales-owned Members. Search matches
case-insensitive name/MemberCode, display phone, formatted/normalized numeric phone and
numeric ClientCode. Branch-scoped users cannot use an explicit other-branch filter to
escape scope. Global actors may filter one branch or search all branches. Page defaults
1, size 25, maximum 100; total count and deterministic name/Guid order are returned.

MemberSummary: Id, ClientCode, MemberCode, FullName, PrimaryPhone, IsActive,
RegistrationBranch {Id, Name}, nullable CurrentSalesOwner and derived IsHouse.

MemberDetails additionally returns PrimaryPhoneNormalized, SecondaryPhone,
IsPrimaryPhoneVerified, DateOfBirth, Gender, Notes, CreatedAt,
Employee {Id, FullName} when linked and EligibleFreelancePlans [{PlanId, PlanName,
CoachName}]. CurrentSalesOwner contains assignment Id, SalesEmployeeId, employee name,
AssignedAt, EndedAt and Reason. Details deliberately excludes full subscription,
financial, attendance and commission graphs. Sales history is a dedicated query.

## API and service methods

| Method and route | Service method | Permission |
| --- | --- | --- |
| GET /api/members | GetMembersAsync | Members.View |
| GET /api/members/{id} | GetMemberByIdAsync | Members.View |
| POST /api/members | CreateMemberAsync | Members.Create + workflow permissions above |
| PUT /api/members/{id} | UpdateMemberAsync | Members.Edit; ChangePrimaryPhone for phone changes |
| PATCH /api/members/{id}/status | ChangeMemberStatusAsync | Members.ChangeStatus |
| GET /api/members/{id}/sales-assignment | GetCurrentSalesAssignmentAsync | Members.View |
| GET /api/members/{id}/sales-history | GetSalesAssignmentHistoryAsync | Members.ViewSalesHistory |
| POST /api/members/{id}/assign-sales | AssignSalesAsync | Members.AssignSales |
| POST /api/members/{id}/return-to-house | ReturnMemberToHouseAsync | Members.AssignSales |

All service entry points enforce permissions and applicable branch scope independently
of controller policies. Updates reject unknown fields. No role names control runtime
feature access. Existing Members.View/Create/Edit are reused; added codes are
Members.ChangeStatus, Members.ChangePrimaryPhone, Members.AssignSales,
Members.ViewSalesHistory, Members.CreateCrossBranch and Members.OwnSalesCustomers.
FreelancePlans.ManageEligibleMembers and Branches.AccessAll are reused independently.

Example normal referral input (authenticated Sales with referral permission):

```json
{
  "fullName": "Customer name",
  "primaryPhone": "01212345678",
  "targetRegistrationBranchId": "00000000-0000-0000-0000-000000000001",
  "creationType": "NormalSales"
}
```

FreelanceHouse additionally requires freelancePlanId. ManagementCreated additionally
requires salesEmployeeId. Assignment takes salesEmployeeId and optional reason; return
House takes optional reason; status takes isActive and optional reason. Source/type is
request-only workflow information; assignments persist ownership truth.

Audit actions: Member.Created, Member.Updated, Member.PrimaryPhoneChanged,
Member.StatusChanged, Member.SalesAssigned, Member.SalesReassigned,
Member.ReturnedToHouse, and FreelancePlan.MemberEligible for registration eligibility.
The existing OperationAccess audit helper captures actor, registration branch, entity,
old/new values and timestamp. Its current implementation does not capture HTTP IP/device;
this feature does not add a second audit system or invent such metadata.

## Deployment and verification

- New migration only; old migrations unchanged. Up changes only Members/GymBranches:
  three columns, three checks, normalized-phone unique partial index and display index
  uniqueness replacement. Existing Guid/FKs/ClientCode/assignment constraints remain.
- Applied only to local development; aggregate Members=0 and branches=2 preserved.
  Both existing branch calling codes deliberately remain null until configured.
  No legacy phone reconciliation is needed there because it has no Members.
- Explicit local command: `dotnet run --project CampionsSystem.API -- --configure-member-permissions`.
  The command guards Development + localhost, provisions nine Member permissions and
  bootstrap grants to existing Owner, preserves disabled permissions and existing grants,
  and audits setup. No startup writes/demo users/password changes are introduced.
- Demo seeding defaults give new Sales roles OwnSalesCustomers/CreateCrossBranch and
  new BranchManager roles assignment/status/history permissions. Existing customized
  demo roles are not rewritten. Grant capabilities explicitly to authorized existing
  staff; no grants to Sales/Reception are inferred from their role names at runtime.
- Production migration, permission provisioning and branch configuration are separate
  rollout operations and were not performed. Rollback requires reconciling canonical
  phones/display duplicates before restoring the old display-phone unique index.

Tests cover Guid identity, branch-local code uniqueness and real independent-connection
concurrent registration, independent branch locks, rollback, normalized phones,
backfill ambiguity/collisions, automatic Sales ownership, cross-branch referral/scope,
House eligibility, manager assignment, reassignment/history, competing assignments,
legacy House rows, employee links, protected fields, status/related history, search,
filters/pagination, permissions/overrides and actual HTTP 401/403/allowed behavior.
Existing Employee, Branch, Authorization, benefits and invitation suites remain included.

Excluded: membership purchase/renewal/freeze, payments, finance/commission calculations,
30K or 5%/7% logic, payroll, CheckIn processing/scanning, invitation/trial redemption,
Excel import, inactive House reports and frontend. Existing out-of-scope services were
not expanded by Member Management.

Final verification on 2026-10-08: `dotnet build` succeeded with zero warnings/errors;
full solution suite 316 passed, zero failed/skipped (52 added tests versus baseline 264).
EF reported no model changes since the new migration. Final implementation commits
were individually verified and pushed; see PROJECT_HISTORY.md and Git history for
chronology. The final documentation commit carries the completion checkpoint.

## CRM integration (2026-10-08)

CRM adds an assembly-internal Lead-ID-based creation path which preserves the locked
Lead owner's assignment and persisted canonical phone, including cross-country branch
conversion. Public registration and primary-phone changes reject conflicting Leads
so they cannot bypass CRM ownership. No arbitrary trusted owner is exposed publicly.
CRM queues permit lightweight owned cross-branch contact data without expanding these
Member list/details branch rules. Member status behavior remains unchanged: automatic
CRM task creation on status, expiry or attendance is a future integration; explicit
CRM task opening is available now. See [CRM management](CRM_LEADS.md).
