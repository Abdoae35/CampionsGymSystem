# Authentication & Authorization

## Authentication

ApplicationUser extends `IdentityUser<Guid>` with `IsActive`. Identity stores users,
password hashes and roles in the shared ApplicationDbContext. Employee.UserId is an
optional unique link: an account may exist without an Employee, and an Employee may
exist without a login.

`POST /api/auth/login` accepts username or unambiguous email plus password and returns
an expiring signed JWT. `GET /api/auth/me` returns current user/Employee identity,
roles, effective permissions and resolved branch. There is no registration, refresh
or logout/token-revocation API in AuthController.

Inactive accounts and locked-out users are rejected. Linked Employees must be Active,
hired by the gym's current date, and not past their termination date. Password login
rejects accounts with TwoFactorEnabled rather than implementing a two-factor challenge.
Identity lockout is configured for five failures and fifteen minutes; password creation
requires at least twelve characters under the configured Identity policy.

JWT validation checks signature/HS256, issuer, audience and expiry. ActiveUserJwtEvents
reloads current account/employment state on authenticated requests. Roles, permissions
and branch context are database-backed, not permanently trusted JWT grants. Tokens
contain identity/time/role claims, not a branch or permission snapshot.

## Authorization Model

**Permission = WHAT the user may do. Scope = WHERE they may do it.**

Roles group permissions through RolePermissions. Effective permission calculation:

1. Union active permissions granted by all current roles.
2. Add active direct UserPermissionOverrides with Allow.
3. Remove active direct overrides with Deny.

Deny therefore wins over role grants/direct Allow. Inactive permissions and inactive
users contribute no effective grants. The unique user/permission override relationship
prevents simultaneous conflicting override rows. Role names are not runtime feature
bypasses; a role named Owner still needs the actual action/scope permissions.

API policies use `Permission:<code>` with the dynamic policy provider and handler.
BLL checks permissions and record scope independently. The current repository has
permission/role/override persistence and evaluation; it does not expose general role
or permission administration controllers.

## Branch Scope

CurrentUserService and UserContextService resolve the optional linked Employee;
EmployeeBranchResolver finds exactly one date-valid assignment to an active branch.
Ambiguous assignments fail closed. Dates use `AuthContext:TimeZoneId` (default
Africa/Cairo), not the server's local date or an open financial BusinessDay.
CurrentBranchContext and BranchAccessService provide reusable operation scope.

`Branches.AccessAll` supplies global branch scope, alongside the relevant action
permission. It is not a replacement for Members.Create, Employees.Transfer or another
operation's capability. Active-branch and feature-specific validation still apply.

## Ownership Exceptions

CRM permits contact work on explicitly owned Leads/Members across branches. Ahmed may
own a Branch A customer after moving to B, and may use permitted CRM follow-up
workflows for that customer. He does not acquire unrestricted A Member browsing,
employee administration, vault, expenses, payroll or general reporting access.

The ordinary `/api/members` list/details remain branch-scoped. CRM queues expose
lightweight contact information for owned subjects. Lead ownership grants follow-up
visibility, not management redistribution outside the actor's normal branch scope.
Operational conversion is another narrow capability: an authorized registration
employee may convert someone else's Lead without changing its owner or gaining
private Lead browsing. See [CRM](CRM.md).

## Important Permission Groups

| Domain | Representative implemented codes |
|---|---|
| Branches | Branches.View, Branches.Manage, Branches.AccessAll |
| Employees | Employees.View, Employees.Manage, Employees.Transfer |
| Members | Members.View, Create, Edit, ChangeStatus, ChangePrimaryPhone, AssignSales, ViewSalesHistory, CreateCrossBranch, OwnSalesCustomers (all prefixed Members.) |
| Leads | Leads.View, Leads.Manage, Leads.Assign, Leads.Convert |
| CRM | CRM.ViewMemberFollowUps, CRM.ManageMemberFollowUps, CRM.ManageHouseFollowUps |
| Plans | MembershipPlans.View, MembershipPlans.Manage |
| Freelance | FreelancePlans.Manage, FreelancePlans.ManageEligibleMembers |

`Members.OwnSalesCustomers` makes an active Employee/account eligible to own customers;
`Members.AssignSales` authorizes assignment. Neither implies the other. Global category
and benefit writes require MembershipPlans.Manage plus Branches.AccessAll. Plan
administration follows BranchId, never CityId.

## Source Pointers

Runtime behavior lives in API/Authentication, API/Authorization and
BLL/Authentication; branch scope is in BLL/Branches. See
[local authentication setup](authentication/LOCAL_API_TESTING.md) and
[adding a permission](authentication/ADDING_A_PERMISSION.md) for operational details.
