# Roles and permission authorization

Current administration: [Owner-only access APIs](../PERMISSIONS_ADMINISTRATION.md)
now manage existing role memberships, grants and user overrides with audit/version
checks. The baseline catalog and suggested grants below are historical proposals,
not rollout defaults. Owner membership is additionally required for privileged
access administration; business endpoints continue to use capabilities and scope.

Roles are dynamic Identity records, not an enum. Roles collect permissions;
authorize business actions by permission rather than a hardcoded job title.
Owner has no magic bypass: configure all intended role grants explicitly.

## Permission resolution

User → AspNetUserRoles → Identity Roles → RolePermissions → Permissions.
IPermissionService.GetUserPermissionsAsync queries live role mappings, filters
active Permission/active ApplicationUser, unions direct Allow overrides, then removes
direct Deny overrides and returns a distinct ordinal-code HashSet. Deny wins over role
grants. All roles contribute. No Redis, persistent cache or JWT permission list.
Grant/revoke/deactivate affects the next request using the same JWT. Employee
eligibility is additionally checked during JWT/current context validation.

```csharp
[Authorize(Policy = "Permission:Members.View")]
[HttpGet]
// Apply to the future Members action; none is implemented in this task.
```

Minimal endpoint equivalent: `.RequireAuthorization("Permission:Members.View")`.
`[Authorize]` suffices for identity-only `/api/auth/me`.
PermissionPolicyProvider dynamically builds authenticated policies with
PermissionRequirement, delegating ordinary/default/fallback policies to the
framework. Scoped PermissionAuthorizationHandler queries current DB grants.
No per-code registration is needed. Prefix/codes are case-sensitive; unknown,
ungranted or inactive codes deny access. Anonymous gets 401; authenticated without
grant gets 403. Role-only checks remain for rare role-specific cases; JWT role
claims refresh from current Identity membership. Permission alone never grants
cross-branch access or record ownership.

## Requested baseline catalog (not automatically seeded)

Create Permission rows through trusted configuration with Code, human Name,
Module, optional Description and IsActive. Code is unique; DB is runtime truth.

| Module | Codes |
| --- | --- |
| Users | Users.View, Users.Manage |
| Roles | Roles.View, Roles.Manage |
| Employees | Employees.View, Employees.Manage, Employees.Transfer |
| Branches | Branches.View, Branches.Manage, Branches.AccessAll |
| Members | Members.View, Members.Create, Members.Edit |
| MembershipPlans | MembershipPlans.View, MembershipPlans.Manage |
| Payments | Payments.View, Payments.Create, Payments.Refund |
| Expenses | Expenses.View, Expenses.Create, Expenses.Approve |
| Shifts | Shifts.View, Shifts.Manage |
| Private | Private.View, Private.Manage |
| Activities | Activities.View, Activities.Manage |
| Leads | Leads.View, Leads.Manage |
| Commission | Commission.View, Commission.ClosePeriod, Commission.ReopenPeriod, Commission.ManageRules |
| Payroll | Payroll.View, Payroll.Manage |
| Reports | Reports.View, Reports.ViewRevenue |
| AuditLogs | AuditLogs.View |

Conservative suggested defaults, requiring explicit business-owner review:

| Role | Initial grants |
| --- | --- |
| Owner | All reviewed active permissions; explicitly grant new ones later |
| BranchManager | Branches.View, Employees.View, Members.View/Create/Edit, MembershipPlans.View, Payments.View/Create, Expenses.View/Create, Shifts.View/Manage, Private.View, Activities.View, Leads.View, Commission.View, Reports.View |
| Sales | Members.View/Create, MembershipPlans.View, Leads.View/Manage |
| Reception | Members.View/Create, MembershipPlans.View, Payments.View/Create, Shifts.View |
| PrivateCoach | Private.View; confirm a narrower session-management code later |
| ClassesCoach | No grants yet: requested catalog has no class-attendance-specific code |

These are proposed data settings, not hardcoded authorization. No non-Owner gets
global security/roles management, refunds, expense approval, commission close/
reopen, payroll or revenue-report privileges by default here. Branch checks remain
required; confirm broader operational privileges before granting them.

## Controlled bootstrap strategy

There was no seeder/admin workflow. This feature does not add startup privilege
writes or create real accounts. Use a trusted one-time administrative runner against
a reviewed database, reusing existing API DI; never expose a bootstrap endpoint.

1. Use RoleManager<IdentityRole<Guid>> to find/create approved roles. Generic Guid
   roles need an explicit Id: `new IdentityRole<Guid>(roleName) { Id=Guid.NewGuid() }`.
2. In a transaction, create missing Permission/RolePermission rows with reviewed
   grants. Do not overwrite existing inactive codes/custom grants on reruns.
3. Supply initial Owner username/email/password through a secure bootstrap provider.
   Use UserManager.CreateAsync and AddToRoleAsync("Owner"); check every IdentityResult.
   Never manually write PasswordHash, hardcode/log credentials or silently overwrite
   an existing account. Grant Owner all intended active permissions explicitly.
4. Link an actual Employee only if needed; never fabricate Employee/branch/member
   rows. Disable/remove administrative bootstrap access after setup.

Wrap provisioning/grants/account creation in one transaction using the scoped
ApplicationDbContext, so failures cannot leave partial privileged accounts. This is
a strategy for a later explicitly authorized bootstrap operation, not a claim that
roles/users/grants exist now. All test accounts/grants roll back.

## Branch feature integration

See [branch isolation](../branches/BRANCH_ISOLATION.md) for explicit global permission
setup and reusable branch-scope checks. Branches.AccessAll controls WHERE, not action
authority. Effective direct Denies are now resolved at runtime from the existing
UserPermissionOverrides table; its Owner-only management API is documented above.
