# MembershipPlanCategories endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Plans.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. 

**Scope/actor rules:** Global shared catalog. Reads need View; all writes require MembershipPlans.Manage plus Branches.AccessAll in BLL.

<a id="get-api-membership-plan-categories"></a>
## GET /api/membership-plan-categories
Source: `CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs:8`, action `List`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Global shared catalog. Reads need View; all writes require MembershipPlans.Manage plus Branches.AccessAll in BLL.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `Search` / query | false | `{"type":"string"}` | See query DTO defaults / guard evidence |
| `IsActive` / query | false | `{"type":"boolean"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Plans.CatalogListRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-cataloglistrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Plans.CatalogListResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-cataloglistresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet,Authorize(Policy="Permission:MembershipPlans.View")]
    public Task<IActionResult> List([FromQuery]CatalogListRequest r,CancellationToken ct)=>Execute(()=>plans.GetCategoriesAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.GetCategoriesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/membership-plan-categories?Page=1&PageSize=25 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "items": [],
  "totalCount": 0,
  "page": 1,
  "pageSize": 25
}
```
<a id="post-api-membership-plan-categories"></a>
## POST /api/membership-plan-categories
Source: `CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs:10`, action `Create`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Global shared catalog. Reads need View; all writes require MembershipPlans.Manage plus Branches.AccessAll in BLL.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Plans.SaveCategoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-savecategoryrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Plans.CatalogDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-catalogdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Create(SaveCategoryRequest r,CancellationToken ct)=>Execute(()=>plans.CreateCategoryAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.CreateCategoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/membership-plan-categories HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "name": "FAKE name",
  "description": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "code": null,
  "name": "FAKE name",
  "description": null,
  "isActive": true,
  "createdAt": "2026-10-12T08:00:00Z"
}
```
<a id="put-api-membership-plan-categories-id-guid"></a>
## PUT /api/membership-plan-categories/{id:guid}
Source: `CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs:12`, action `Update`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Global shared catalog. Reads need View; all writes require MembershipPlans.Manage plus Branches.AccessAll in BLL.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Plans.SaveCategoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-savecategoryrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Plans.CatalogDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-catalogdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("{id:guid}"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Update(Guid id,SaveCategoryRequest r,CancellationToken ct)=>Execute(()=>plans.UpdateCategoryAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.UpdateCategoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/membership-plan-categories/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "name": "FAKE name",
  "description": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "code": null,
  "name": "FAKE name",
  "description": null,
  "isActive": true,
  "createdAt": "2026-10-12T08:00:00Z"
}
```
<a id="patch-api-membership-plan-categories-id-guid-status"></a>
## PATCH /api/membership-plan-categories/{id:guid}/status
Source: `CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs:14`, action `Status`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Global shared catalog. Reads need View; all writes require MembershipPlans.Manage plus Branches.AccessAll in BLL.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Plans.ChangePlanStatusRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-changeplanstatusrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Plans.CatalogDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-catalogdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPatch("{id:guid}/status"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Status(Guid id,ChangePlanStatusRequest r,CancellationToken ct)=>Execute(()=>plans.ChangeCategoryStatusAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.ChangeCategoryStatusAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PATCH /api/membership-plan-categories/11111111-1111-4111-8111-111111111111/status HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "isActive": true
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "code": null,
  "name": "FAKE name",
  "description": null,
  "isActive": true,
  "createdAt": "2026-10-12T08:00:00Z"
}
```
