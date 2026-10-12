# Auth endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Authentication.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline and matching local master/live remote master at audit. Runtime operations were not exercised here. 

**Scope/actor rules:** No operation branch selector. Login anonymous; /me re-resolves identity/permissions/current branch.

<a id="post-api-auth-login"></a>
## POST /api/auth/login
Source: `CampionsSystem.API/Controllers/AuthController.cs:13`, action `Login`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: anonymous allowed. Exact controller policies (AND): none on anonymous action.
Scope/actor: apply module rules above and [guard evidence](rules/Authentication.md). No operation branch selector. Login anonymous; /me re-resolves identity/permissions/current branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Authentication.LoginRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-authentication-loginrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Authentication.LoginResponse](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-authentication-loginresponse). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 model validation; Login: 401 ProblemDetails invalid credentials; Me: 401 can be empty. 500 unexpected. No scope/409 implementation in AuthController. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await authentication.ValidateCredentialsAsync(request, cancellationToken);
        if (user is null) return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid credentials.");
        return Ok(tokens.Issue(user));
    }
```
Service entry points: `CampionsSystem.BLL.Authentication.IAuthenticationService.ValidateCredentialsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/auth/login HTTP/1.1
Host: <configured-api-host>
Content-Type: application/json

{
  "userNameOrEmail": "fake.frontend.operator",
  "password": "FAKE_PASSWORD_PLACEHOLDER"
}
```
Schema-shaped response (not an executed response):
```json
{
  "accessToken": "FAKE_TOKEN_PLACEHOLDER",
  "expiresAt": "2026-10-12T08:00:00Z",
  "user": {
    "id": "11111111-1111-4111-8111-111111111111",
    "userName": null,
    "employeeId": null,
    "fullName": null,
    "roles": []
  }
}
```
<a id="get-api-auth-me"></a>
## GET /api/auth/me
Source: `CampionsSystem.API/Controllers/AuthController.cs:22`, action `Me`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `None`.
Scope/actor: apply module rules above and [guard evidence](rules/Authentication.md). No operation branch selector. Login anonymous; /me re-resolves identity/permissions/current branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Authentication.CurrentUserContext](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-authentication-currentusercontext). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 model validation; Login: 401 ProblemDetails invalid credentials; Me: 401 can be empty. 500 unexpected. No scope/409 implementation in AuthController. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserContext>> Me(CancellationToken cancellationToken)
    {
        var user = await currentUser.GetAsync(cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }
```
Service entry points: `CampionsSystem.BLL.Authentication.ICurrentUserService.GetAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/auth/me HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "userId": "11111111-1111-4111-8111-111111111111",
  "userName": null,
  "employeeId": null,
  "employeeName": null,
  "roles": [],
  "permissions": [],
  "currentBranch": null,
  "currentBranchId": null
}
```
