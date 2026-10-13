# Owner-only permissions administration V1

This API configures existing Identity roles, role capability grants and individual
Allow/Deny overrides. It does not create accounts/roles/permissions or provision
Owners. One employee can retain Manager and Sales roles on the same account.
Access changes never edit branch assignments, member/lead commercial ownership,
collection attribution, commission statements, payroll or financial evidence.

## Security and effective access

Every route requires authentication and **current database Owner membership**:
an Identity role with `NormalizedName == "OWNER"`, the same convention used by
existing permission setup. Both the API OwnerAccessAdministration policy and BLL
OwnerAdministrationAccess enforce it. CurrentUserService also requires an active,
unlocked account and eligible linked employment through UserContextService.
An Owner without a linked employee is supported. Client role/permission flags and
stale JWT claims cannot authorize access.

The applicable existing Users.View/Manage or Roles.View/Manage capability is also
required. A Manager with all four capabilities and Branches.AccessAll still
receives 403. Assigning those capabilities to another role does not delegate this
administration API. There are no default grant/seeder changes.

PermissionService remains authoritative: union active role grants, add active
user Allows, subtract active user Denies. Deny wins; clearing an override restores
the role result. Inactive permissions cannot be newly granted/Allowed; they can
be revoked, Denied or cleared. Accounts that are inactive contribute no effective
permissions. For locked/ineligible accounts, raw permission explanations may
still show grants, but `runtimeAccountEligible=false` means protected requests
cannot use them. Overrides are one row per user/permission, not competing lists.

The privileged Owner inventory is global and requires the above Owner gate;
BranchId is an inventory filter. It does not confer global operational scope.
Normal endpoints still enforce the actor's action capability and branch scope.
`hasGlobalBranchScope`, `currentBranch` and dated `branchAssignments` are distinct
from role names and permission inheritance. CurrentBranch and HasGlobalBranchScope
come from the usable runtime context: an ineligible account returns null/false,
while its assignment history and raw grant explanations remain inspectable.
An unresolved branch is null rather than broadened access. Employees without login accounts remain available through
the existing scoped `/api/employees` APIs. Employee lifecycle and transfer use
those existing services, never these access mutation routes.

Roles and permissions reload from the database on every authenticated request.
ActiveUserJwtEvents replaces JWT roles from the store; permission checks query
PermissionService. Removed roles/grants and Deny overrides affect existing tokens
on their next request. No new cache/token mechanism is introduced; a request
already executing is not retroactively cancelled.

## Routes

All paths begin with `/api/access-administration`. Successful responses are 200.

| Method | Path | Effective capability | Response |
|---|---|---|---|
| GET | `/permissions` | Roles.View | PermissionModule[] |
| GET | `/roles` | Roles.View | AccessRole[] |
| GET | `/roles/{roleId}` | Roles.View | RoleAccessDetails |
| GET | `/users` | Users.View | AccessPage<AccessUserItem> |
| GET | `/users/{userId}` | Users.View | UserAccessDetails |
| GET | `/history` | Users.View AND Roles.View | AccessPage<AccessAuditDetails> |
| PUT | `/users/{userId}/roles/{roleId}` | Users.Manage | AccessChangeResult |
| PUT | `/roles/{roleId}/permissions/{permissionId}` | Roles.Manage | AccessChangeResult |
| PUT | `/users/{userId}/permissions/{permissionId}` | Users.Manage | AccessChangeResult |

DTO definitions: [AccessAdministrationContracts.cs](../CampionsSystem.BLL/Authentication/AccessAdministrationContracts.cs).
Thin endpoints: [AccessAdministrationController.cs](../CampionsSystem.API/Controllers/AccessAdministrationController.cs).
BLL: [AccessAdministrationService.cs](../CampionsSystem.BLL/Authentication/AccessAdministrationService.cs).

`PermissionModule` groups canonical ID/code/title/description/active state by the
stored Module. Titles/descriptions come from Permission.Name/Description; missing
descriptions are null. No guessed descriptions or display-name authorization.
RoleAccessDetails includes Role, Version and granted canonical permission rows.

UserAccessDetails contains account identity/active/lock/runtime eligibility,
employee employment dates/status, current branch, historical assignments, roles,
opaque Version and permission explanations. Each explanation includes permission
ID/code/title/module/active state, inheritedFromRoles, override (Allow/Deny/null)
and effective. The effective value comes from the existing evaluator.

User inventory: `page=1&pageSize=25` defaults, size 1–100; optional `search`
(case-sensitive stored username/employee name/phone, max 256), `roleId`,
`branchId` (date-valid assignments), `isActive`. History uses the same pagination
and optional `targetUserId`, `roleId`, `actorUserId`, `operationId`. TotalCount
counts matching rows. Users sort username then ID; history timestamp then ID
descending. Overflowing offsets reject. Catalog and roles are bounded canonical
configuration lists rather than arbitrary user-created resources.

## Mutation requests, retries and concurrency

Read the user detail before changing membership/override, or role detail before
changing a role grant. Send its exact Version with a new UUID and a nonblank reason
of at most 2,000 characters. Permission/role IDs must exist; arbitrary codes/names
are never accepted. Unknown JSON properties reject.

RoleMembershipRequest:

```json
{"operationId":"11111111-1111-4111-8111-111111111111","expectedVersion":"<64-character version from user GET>","reason":"Owner approves Sales duties alongside management","assigned":true}
```

`assigned=false` removes that role only. RoleGrantRequest has the same common
fields and `granted=true` or `false`; use the version from role GET.

PermissionOverrideRequest has the same common fields and **required** `effect`:
`"Allow"`, `"Deny"`, or explicit `null` to clear. Numeric enums, other strings and
omitted effect reject. Clearing an absent override is a recorded no-op.

Response: `{ "operationId": "...", "changed": true, "version": "..." }`.
Desired state already present returns changed=false and an audit record; no
duplicate membership/grant is inserted. Retry the exact body and operation ID
after a lost response: original result is returned without a second audit.
Changed body/actor/target using that ID conflicts. A retry still requires current
Owner/capability authorization. A returned retry version is historical: GET again
before the next change. Stale versions return 409, never overwrite other changes.

The version fingerprints the relevant roles, inherited grants, overrides,
catalog active state and account/employment/branch facts. Role grant changes thus
invalidate affected user versions. A transaction advisory lock serializes access
mutations; affected account and Owner employment rows are locked before the
actor is revalidated and the change evaluated. Savepoints roll back forbidden
or conflicting writes atomically. A unique filtered AuditLogs operation index
also prevents duplicate operation evidence.

Owner provisioning is deliberately unavailable. Removing **another** existing
Owner is allowed only while recovery safeguards hold. Self-Owner removal always
rejects. After every mutation at least one active/unlocked eligible Owner must
retain all four administration capabilities, and the administering Owner must
also retain them. A role-grant change affecting the actor or a direct self-Deny
cannot cause self-lockout. These protections apply even to otherwise valid writes.
Every mutation must leave the administering Owner with the complete recovery set.
A partially configured legacy Owner can use an available action capability only
if that change restores the complete set; otherwise another configured Owner or
controlled offline repair is required.

## Errors and audit

401: anonymous, expired/invalid token, inactive/locked/ineligible authenticated
account. 403: usable account without actual Owner membership or action capability.
400 ProblemDetails: malformed body, invalid state/UUID/reason/version syntax,
inactive grant/Allow, or attempted Owner provisioning. 404 ProblemDetails: missing
user, role or permission. 409 ProblemDetails: stale version, operation reuse,
self/last-Owner protection, unique/concurrent conflict. No credentials/security
stamps/password hashes are exposed.

AuditLog Action `AccessAdministration.Changed`, EntityType `AccessAdministration`,
EntityId=operation UUID, actor UserId and CreatedAt UTC. Description stores reason;
OldValues contains prior safe access snapshot; NewValues contains request hash,
change kind, target user/role/permission, reason, before/after snapshots and result.
History exposes those immutable values, timestamp and actor. Global role changes
do not invent a branch; BranchId is null. Tracked update/delete of these logs is
rejected by ApplicationDbContext, consistent with existing evidence safeguards.
There is no audit-edit endpoint; this is not a database-administrator tamper guard.

## Setup and recovery

Forward migration `20261013125623_ImplementOwnerAccessAdministration` adds only
the unique filtered AuditLogs operation index. Existing models/keys are reused;
no permission catalog, role grant or account data is seeded by the migration.
Reviewed SQL is in [database/ImplementOwnerAccessAdministration.sql](database/ImplementOwnerAccessAdministration.sql).
Application/Production migration and grant setup are separate rollout operations.

An established Owner must have active Users.View, Users.Manage, Roles.View and
Roles.Manage through current grants/overrides, with no corresponding Deny, and a
usable account/employment status. Existing guarded Development-only auth seeding
can bootstrap a fresh local demo; it preserves existing accounts/roles and is
not a repair tool or Production setup claim. Follow
[local auth setup](authentication/LOCAL_API_TESTING.md); do not enable public
registration/Owner provisioning or run Developer seeding in Production.

For a legacy deployment with no usable Owner, a trusted deployment/database
administrator must use a controlled, backed-up, offline maintenance procedure:
verify the intended existing Owner and authoritative OWNER role; repair only
its required memberships/grants/overrides and account/employment eligibility;
record actor, reason, exact old/new values and timestamp in AuditLog; verify
Owner login and all four capabilities before resuming service. Coordinate this
with live administration (offline is safest). This is not exposed as an
unauthenticated HTTP recovery endpoint. No new Owner account is provisioned by V1.

UI integration: use server explanations to display inherited versus direct state,
refresh GET after each successful mutation, confirm changes to shared role grants,
and show 409 as reload/review rather than automatically retrying a new decision.
Hide Owner assignment and self-demotion controls. Never infer operational branch
access or Sales commission ownership from the role title.

Postman folder **20. Owner access administration** includes all nine operations
and a separate explicit clear-override example. Log in as the established Owner;
choose actual user/role/permission IDs and GET the relevant version before a PUT.

## Verification

Full regression: **887/887 passed**, zero failures/skips (32 new cases), 33m35s.
Clean rebuild: zero warnings/errors. EF model drift: none. Reviewed/regenerated
forward SQL matched; 52 historical migration files and frontend handoff unchanged.
Application/Production migrations and data were not changed.

Focused HTTP/security tests are in
[AccessAdministrationApiTests.cs](../CampionsSystem.DAL.Tests/AccessAdministrationApiTests.cs),
with cross-connection retry/stale write/revocation/coordination tests in
[AccessAdministrationConcurrencyTests.cs](../CampionsSystem.DAL.Tests/AccessAdministrationConcurrencyTests.cs).
The complete suite also verifies existing operational permissions and branch,
financial, payroll and commission behavior. See [PROJECT_HISTORY.md](../PROJECT_HISTORY.md) for corrected
fixture/runner failures and the checkpoint record.
