# Branch isolation foundation

**Permission != Branch Scope.** Permissions answer what an account can do; branch
scope answers where. Both are required in API and BLL. Owner is an ordinary Identity
role with configured grants, not a runtime bypass.

## Effective permissions and global access

Existing PermissionService now resolves live active codes as role grants plus direct
Allow minus direct Deny. A Deny wins even over Owner/global grants. Inactive permissions
and inactive users are ignored. No permissions are embedded in JWT or persistently
cached; changes affect the next request. Override CRUD is not part of this feature.

Reused: Branches.View, Branches.Manage, Employees.View, Employees.Manage,
Employees.Transfer. Added: **Branches.AccessAll** for explicit global scope. This
permission does not grant branch-management or employee-transfer actions by itself.
Any Identity role/user may receive it through trusted permission configuration.

An explicit local bootstrap command adds the code if absent and grants it to the
existing OWNER Identity role; it preserves an inactive code's state and creates no
roles, accounts or employees. It also records Permission.BranchAccessConfigured.
It is not a startup seeder and has **not** been run against the application database
by this feature task, per the instruction not to update the database automatically.
Existing Owner accounts therefore need this deliberate permission configuration:

```sh
dotnet run --project CampionsSystem.API --no-launch-profile -- \
  --environment Development --configure-branch-permissions
```

The command requires localhost and Development. Re-running deliberately restores a
missing Owner grant; do not treat it as an automatic recurring seed. It does not change
passwords, schema or migrations. Production must use separately reviewed administrative
permission configuration. Existing Branches.View/Manage and Employees permissions must
also be configured; global scope does not replace action grants.

## Shared context and access checks

ICurrentBranchContext.GetAsync returns CurrentBranchId, CurrentBranch, HasBranch and
HasGlobalAccess, derived from the DB-backed ICurrentUserService. It never reads raw
HttpContext in business services or invents a branch for a global account.

IBranchAccessService provides:

- CanAccessBranchAsync: scope plus active branch existence.
- EnsureCanAccessBranchAsync: rejects off-scope, missing or inactive branches.
- ResolveOperationBranchAsync: defaults to the current branch; validates any explicit
  target instead of trusting a request BranchId. A branchless global user must select
  an explicit target for branch operations.

For future branch-scoped features:

```csharp
// API also declares the feature's action permission.
var branchId = await branchAccess.ResolveOperationBranchAsync(ct: ct);
var rows = await database.Expenses.AsNoTracking()
    .Where(x => x.BranchId == branchId)
    .ToListAsync(ct);
// Example only: no Expense endpoint/service implemented by this feature.
```

For a resource addressed by ID, load only authorized fields and validate its actual
BranchId before returning/updating it. Never authorize using only a BranchId supplied
by the client. Explicitly selected targets are allowed only with appropriate scope;
a scoped employee cannot spoof another branch. These operational methods require an
active target. Inactive-branch administration uses the shared scope predicate and
explicit management permission; future historical reports need their own reviewed
read policy rather than weakening operational validation.

Queries are explicitly scoped in BLL; no global EF filters were added. This preserves
room for authorized multi-branch administration, historical reports and cross-branch
member access. The foundation does not automatically filter every existing table:
each future feature must apply the branch predicate/check deliberately. Existing
OperationAccess now reuses the same scope check instead of an Owner role-name bypass.

## Future member access is different from financial visibility

Reception may identify a member from another home branch using only fields necessary
to validate access: code, name, permitted photo, subscription status/session balance
and allowed branches. The future check-in service must check subscription eligibility
for the actual target branch. It must not expose unrelated financial information.

A permitted check-in at B records CheckIn.BranchId = B. SessionBased attendance consumes
one session from the shared subscription; Unlimited does not deduct quantity. This does
not transfer the member's home branch. No member/check-in feature is implemented here.

## Future financial ownership

Money belongs to the branch where it is actually received/spent. Renewal at B records
SaleTransaction.BranchId, Payment.BranchId and FinancialTransaction.BranchId as B,
regardless of the member's registration branch. Expense at B belongs to B. Employee or
member transfers must never change old transaction BranchIds. These are documented
contracts for future financial features; no financial workflow was added here.
