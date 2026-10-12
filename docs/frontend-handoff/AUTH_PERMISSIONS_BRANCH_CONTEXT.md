# Authentication, capabilities and context

Latest branch update: frozen 2026-10-12T10:44:04.604445+00:00 at `bf733e2502a85603b80f8ad8a01747264a0f518b` plus uncommitted operational files adds **35 Coaching routes** (190 current actions total). The later isolated snapshot builds; initial CS8999 was superseded by concurrent domain publication. Stable contracts below remain at 0bc4476; new draft contracts/confirmed policies and three legacy Class 410 changes are in [branch-only status](BRANCH_ONLY_PRIVATE_CLASSES_STATUS.md). **Use that update before interpreting any earlier pending/no-route statement below.**


**VERIFIED FROM CODE at baseline**: `CampionsSystem.API/Authentication/*`, `Authorization/*`, `Controllers/AuthController.cs`, `BLL/Authentication/*`, `BLL/Branches/*`, `BLL/Operations/OperationAccess.cs`.

`POST /api/auth/login` JSON `{userNameOrEmail,password}` returns `{accessToken,expiresAt,user:{id,userName,employeeId,fullName,roles}}`. Required inputs have maximum length 256. Username is tried first, then unambiguous normalized email. Invalid credentials, inactive/locked accounts, two-factor-enabled accounts (challenge not implemented) or invalid linked employment produce 401. Failed passwords may update Identity failure/lockout state, so the audit did not call login. Five failures lock out for 15 minutes; account creation password minimum 12. No registration, password reset, refresh, logout, token revocation or account/role/override administration endpoint exists.

Send `Authorization: Bearer <token>`; no auth cookie/session/CSRF mechanism is configured. Validation requires HS256 signature, configured issuer/audience, expiry, 30-second skew. `GET /api/auth/me` returns current user identity, optional Employee, roles, effective `permissions`, `currentBranch:{id,name}|null`, and computed `currentBranchId`. It is the bootstrap authority; login does not contain capabilities. Active-user JWT events reload current account/employment facts; per-request context re-resolves roles, capabilities and branch from DB. Do not treat JWT role claims as durable grants.

**RECOMMENDATION**: keep token storage behind one client abstraction; avoid logging tokens, add Bearer centrally, call `/me` before protected navigation, clear auth state/cache on logout locally, and re-authenticate on expiry/401. No backend logout request should be invented. A 403 means authenticated but denied; do not loop login. There are no server redirects to a login page.

Effective permissions = active role grants union active direct Allow, minus active direct Deny. **Deny wins**; inactive account/permission contributes nothing. Names are case-sensitive ordinal codes. Policies dynamically use `Permission:<code>`; multiple policy attributes are AND. BLL adds scope/ownership/capabilities independently. See [permission inventory](api/PERMISSION_REFERENCE.md) and exact per-route policies in [catalog](API_ENDPOINT_CATALOG.md). A role called Owner/Manager/Sales/Reception/Coach is not an authorization bypass.

Frontend action visibility uses `/me.permissions`, with record-specific states too. Never derive access from role labels alone, and always handle backend refusal after a visible button is pressed. There is no permission-management endpoint to fetch/modify all grants. Refresh `/me` after relevant employment/assignment changes; a cached context can become stale.

Current branch derives from the Employee's exactly one date-valid assignment to an active branch, using configured gym calendar date (default Africa/Cairo). Assignment date bounds are inclusive. Ambiguity fails closed; an account may have no Employee/current branch. There is **no active-branch mutation API, branch header or persistent branch-switch cookie**. Global users may select explicit `branchId`/target-registration-branch fields on individual contracts; ordinary staff use current context and cannot gain scope by selecting a branch. Do not add `X-Branch-Id`.

`Branches.AccessAll` grants global scope only alongside the action capability. Record ownership, active-branch validation, own Employee assignment and own operational Shift requirements still apply. Same city is not shared administrative authority. Member registration branch, plan owning branch, subscription origin branch, visited branch, financial branch and Sales owner's working branch are distinct. Gym entitlement access consumes its sold City snapshot at appropriate active same-city branches.

Transfers preserve Member/Lead ownership and historical financial branches. CRM owned queues/interactions permit narrow cross-branch contact work; ordinary Member/employee/financial directories remain scoped. Cross-branch Lead redistribution needs management scope. Conversion requires Leads.Convert + Members.Create, an active converter Employee, destination checks, and additional cross-branch/Freelance capability when applicable; it preserves the Lead's current eligible owner. It does not grant private Lead browsing. Suspension/resignation closes ownership atomically; rehire does not restore it. Transfer requires the Employee's open shift to be closed first.

Role-facing behavior (capabilities are the actual gate):

| Label | Implemented behavior and limit |
|---|---|
| Owner/global administration | Explicit global + action permissions for global catalogs, setup, commission administration and withdrawal; financial mutations still need own shift where required |
| Manager/BranchManager | Current branch management when granted; no default withdrawal authority; fresh role template cannot establish an existing user's grants |
| Sales | Eligible owner needs Members.OwnSalesCustomers + active linked employment/account; assigned CRM work survives transfer; own shift for registration/finance |
| Reception | Permitted registration/conversion/check-in and own shift operations; entering data does not make Reception the Sales owner |
| Coach | Legacy delivery coach fields/internal-external activity profile exist; no implemented dedicated Coach mobile auth flow or new Private earnings API at baseline |

Development seeding/catalog CLI commands are explicit, not startup jobs. Fresh templates preserve existing customized roles and overrides. Historical 2026-10-12 verification reported Offers.Manage, Installments.Manage and Sales.QuickCancel active but with no grants/overrides. **DOCUMENTED ONLY for the present deployment**: this audit did not query operational DB grant rows. Request intended grants from backend administration; never silently provision them from the frontend.

CORS: no `AddCors`/`UseCors` in Program. Cross-origin browser calls require approved CORS configuration or a development same-origin Vite proxy. This is a backend integration gap; no cookies/credentials-mode workaround is implied.
