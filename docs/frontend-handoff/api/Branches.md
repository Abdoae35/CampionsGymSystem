# Branches endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Branches.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE at completed backend a2a0c56, inspected at 476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80. Runtime business operations were not exercised by this documentation task.

**Scope/actor rules:** Directory/read/update limited to effective current branch unless Branches.AccessAll. Create additionally requires Branches.AccessAll. Active City/branch rules still apply.

<a id="get-api-branches"></a>
## GET /api/branches
Source: `CampionsSystem.API/Controllers/BranchesController.cs:11`, action `List`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Branches.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Branches.md). Directory/read/update limited to effective current branch unless Branches.AccessAll. Create additionally requires Branches.AccessAll. Active City/branch rules still apply.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Branches.BranchDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-branches-branchdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet, Authorize(Policy = "Permission:Branches.View")]
    public Task<IActionResult> List(CancellationToken ct) => Execute(() => branches.ListAsync(ct));
```
Service entry points: `CampionsSystem.BLL.Branches.BranchManagementService.ListAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/branches HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="get-api-branches-id-guid"></a>
## GET /api/branches/{id:guid}
Source: `CampionsSystem.API/Controllers/BranchesController.cs:14`, action `Get`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Branches.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Branches.md). Directory/read/update limited to effective current branch unless Branches.AccessAll. Create additionally requires Branches.AccessAll. Active City/branch rules still apply.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Branches.BranchDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-branches-branchdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"), Authorize(Policy = "Permission:Branches.View")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) => Execute(() => branches.GetAsync(id, ct));
```
Service entry points: `CampionsSystem.BLL.Branches.BranchManagementService.GetAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/branches/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "cityId": "11111111-1111-4111-8111-111111111111",
  "cityName": "FAKE cityName",
  "address": null,
  "phone": null,
  "isActive": true,
  "maxBorrowedGymSessions": 0,
  "quickRefundWindowHours": 0,
  "countryCallingCode": null
}
```
<a id="post-api-branches"></a>
## POST /api/branches
Source: `CampionsSystem.API/Controllers/BranchesController.cs:17`, action `Create`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Branches.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Branches.md). Directory/read/update limited to effective current branch unless Branches.AccessAll. Create additionally requires Branches.AccessAll. Active City/branch rules still apply.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Branches.SaveBranchRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-branches-savebranchrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Branches.BranchDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-branches-branchdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost, Authorize(Policy = "Permission:Branches.Manage")]
    public Task<IActionResult> Create(SaveBranchRequest request, CancellationToken ct) => Execute(() => branches.SaveAsync(null, request, ct));
```
Service entry points: `CampionsSystem.BLL.Branches.BranchManagementService.SaveAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/branches HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "name": "FAKE Branch",
  "cityId": "11111111-1111-4111-8111-111111111111",
  "isActive": true,
  "countryCallingCode": "+20"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "cityId": "11111111-1111-4111-8111-111111111111",
  "cityName": "FAKE cityName",
  "address": null,
  "phone": null,
  "isActive": true,
  "maxBorrowedGymSessions": 0,
  "quickRefundWindowHours": 0,
  "countryCallingCode": null
}
```
<a id="put-api-branches-id-guid"></a>
## PUT /api/branches/{id:guid}
Source: `CampionsSystem.API/Controllers/BranchesController.cs:20`, action `Update`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Branches.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Branches.md). Directory/read/update limited to effective current branch unless Branches.AccessAll. Create additionally requires Branches.AccessAll. Active City/branch rules still apply.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Branches.SaveBranchRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-branches-savebranchrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Branches.BranchDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-branches-branchdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("{id:guid}"), Authorize(Policy = "Permission:Branches.Manage")]
    public Task<IActionResult> Update(Guid id, SaveBranchRequest request, CancellationToken ct) => Execute(() => branches.SaveAsync(id, request, ct));
```
Service entry points: `CampionsSystem.BLL.Branches.BranchManagementService.SaveAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/branches/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "name": "FAKE Branch",
  "cityId": "11111111-1111-4111-8111-111111111111",
  "isActive": true,
  "countryCallingCode": "+20"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "cityId": "11111111-1111-4111-8111-111111111111",
  "cityName": "FAKE cityName",
  "address": null,
  "phone": null,
  "isActive": true,
  "maxBorrowedGymSessions": 0,
  "quickRefundWindowHours": 0,
  "countryCallingCode": null
}
```
