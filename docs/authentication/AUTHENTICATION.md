# Authentication and current user context

The existing Guid Identity stores/N-Tier architecture are reused. This feature
adds login, short-lived JWT Bearer tokens and user/employee/branch context only.
No refresh tokens, registration, MFA challenge, branch switching, business
endpoints, schema changes or migration are included.

## Configuration

API appsettings.json has non-secret issuer/audience, AccessTokenMinutes=30 and an
empty Key. Startup rejects missing issuer/audience, expiry outside 1..60 minutes,
invalid business timezone or a base64 key decoding to fewer than 32 bytes.
The API intentionally requires a signing key supplied externally before starting.

Generate a development key in existing user secrets without displaying it or
putting its value in command-line arguments, from the repository root:

```sh
python3 - <<'PY'
import base64, json, secrets, subprocess
subprocess.run(
    ["dotnet", "user-secrets", "set", "--project", "CampionsSystem.API"],
    input=json.dumps({"Jwt:Key": base64.b64encode(secrets.token_bytes(32)).decode()}),
    text=True, check=True)
PY
```

Use Development to load user secrets. Existing ConnectionStrings:DefaultConnection
setup remains required. Production supplies Jwt__Key through its secret provider,
with correct Jwt__Issuer/Jwt__Audience. Never commit/reuse development secrets in
production. Validators need matching keys/settings. Key rotation invalidates old
tokens. Use HTTPS for credentials and bearer tokens.

## Login and JWT flow

POST `/api/auth/login`:

```json
{"userNameOrEmail":"configured-user","password":"supplied-securely"}
```

Required fields/max length 256 are validated (400). Identity resolves username,
then normalized email; ambiguous legacy email matches are refused. Active user,
no lockout, correct Identity-verified password and eligible linked Employee are
required. Known-user failures increment Identity failure count; successful login
resets it. Existing password length 12/Identity defaults and five failures for
fifteen-minute lockout are retained. Invalid credentials/state return generic 401.
MFA-enabled accounts are rejected because password-only tokens bypass MFA; the
challenge flow is future work. Accounts without Employee are allowed.

Success 200 contains accessToken, expiresAt (UTC) and user {id,userName,employeeId,
fullName,roles}. Employee name falls back to username; EmployeeId may be null.
Only DTOs are returned, never PasswordHash/security stamps or whole Identity rows.

JWT claims are sub (Guid UserId), optional name, role, jti, iat, nbf, exp and
issuer/audience. No permissions, sensitive data or branch authority snapshot.
Tokens are never logged/stored. Expiration requires login again.

## `/me` and request validation

GET `/api/auth/me` requires `Authorization: Bearer <accessToken>`; anonymous or
invalid tokens get 401. HS256 signature, issuer, audience and lifetime are checked,
with thirty-second clock skew. Active user/employee/lockout state is rechecked on
bearer requests. Role claims are refreshed from live Identity membership. Thus
account deactivation and role revocation affect the next request with the same JWT.
Password changes alone do not revoke tokens; deactivation/lockout/key rotation/
expiration do in this version.

Response: userId, userName, nullable employeeId/employeeName, roles, distinct active
permissions, nullable currentBranch {id,name}, and currentBranchId.

## Employee and branch resolution

Employee.UserId maps ApplicationUser.Id using the existing optional unique FK;
no EmployeeId duplication or fake Employee/branch is introduced. Linked Employee
must have Active status and inclusive current employment dates. Global Owner or
technical accounts without Employee/branch are safe.

Current assignment satisfies StartDate<=today and EndDate null or >=today, with
active GymBranch. Require exactly one date-valid assignment; overlapping primary/
nonprimary history fails closed. Ignore future/ended assignments and inactive branches;
no unambiguous eligible assignment means null.
Never use historical transaction branches or add Employee.CurrentBranchId.
Today uses TimeProvider and AuthContext:TimeZoneId (Africa/Cairo default), independent
of server local timezone. Existing schema has no per-branch timezone.

Inject BLL ICurrentUserService; UserId/IsAuthenticated use the API claims accessor,
while GetAsync resolves DB-backed EmployeeId/CurrentBranchId/Roles/Permissions:

```csharp
var context = await currentUser.GetAsync(cancellationToken);
```

BLL never uses HttpContext. Services are scoped; read-only queries use filtered
AsNoTracking/projections and async cancellation where supported. UserManager APIs
lack CancellationToken; cancellation is checked around them.

Future branch-scoped actions MUST enforce resource BranchId in BLL/API, explicitly
use the shared branch-access service and safely handle null branch. Context is not resource
authorization. Next.js server/BFF and Flutter can send bearer headers; direct
cross-origin browser requests need an explicitly reviewed CORS configuration.
No frontend origin or permissive CORS policy was invented.

## Bootstrap and verification

No seeding mechanism existed, so no real users/roles/grants were seeded. See
AUTHORIZATION.md for catalog/defaults and controlled Identity bootstrap strategy.
The existing DAL.Tests project exercises BLL/API. Real migrations run in isolated
PostgreSQL schemas inside rolled-back transactions; test controllers are registered
only in the in-process test host. Configure GYM_SCHEMA_TEST_CONNECTION for a
disposable local login/database with CREATE SCHEMA privileges (default:
localhost/postgres/current OS username). Never point tests at production.

```sh
dotnet build CampionsGymSystem.slnx
dotnet test CampionsSystem.DAL.Tests/CampionsSystem.DAL.Tests.csproj
dotnet test CampionsGymSystem.slnx
```

## Implementation files and verification

- BLL/Authentication: AuthContracts.cs (DTOs/interfaces/options),
  AuthenticationService.cs, UserContextService.cs, CurrentUserService.cs,
  PermissionService.cs.
- API/Authentication: JwtOptions.cs, JwtTokenIssuer.cs, HttpCurrentUserIdentity.cs,
  ActiveUserJwtEvents.cs, AuthenticationRegistration.cs.
- API/Authorization: PermissionRequirement.cs, PermissionPolicyProvider.cs,
  PermissionAuthorizationHandler.cs; API/Controllers/AuthController.cs.
- Modified API Program.cs, appsettings.json, CampionsSystem.csproj and
  CampionsSystem.http; existing DAL.Tests csproj adds BLL/API references and
  Mvc.Testing. No DAL entity/configuration/migration file changed.
- Added test files: AuthTestDatabase.cs, AuthenticationServiceTests.cs,
  AuthenticationApiTests.cs, JwtConfigurationTests.cs, PermissionProbeController.cs.
- Authentication docs: this file, AUTHORIZATION.md and ADDING_A_PERMISSION.md;
  required AGENTS.md/PROJECT_HISTORY.md handoff checkpoints.

Packages added: Microsoft.AspNetCore.Authentication.JwtBearer 10.0.8 (API) and
Microsoft.AspNetCore.Mvc.Testing 10.0.8 (tests). Existing Identity/xUnit reused.
Final verification: build zero warnings/errors; 58 solution tests passed, zero
failures/skips (38 authentication/authorization/configuration plus 20 existing
schema tests); EF reported no pending model changes. Real test schema/data writes
rolled back. No existing database was updated or production connection used.

## Shared branch resolver (branch feature update)

UserContextService now delegates to EmployeeBranchResolver. Exactly one date-valid
assignment is required; inactive branches and ambiguous overlapping legacy assignments
produce no operational branch. Primary is not a bypass for conflicting active rows.
ICurrentBranchContext reuses this context, and effective transfer changes appear on the
next request without renewing JWT. See [branch management](../branches/BRANCH_MANAGEMENT.md).
