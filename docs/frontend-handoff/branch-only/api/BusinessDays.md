# BusinessDays endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Shifts.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. 

**Scope/actor rules:** Resolved branch scope; explicit Shifts permission management, no own Shift prerequisite, may roll over while Shifts remain open. Create operation ID is day ID; no automatic midnight transition.

<a id="get-api-business-days-current"></a>
## GET /api/business-days/current
Source: `CampionsSystem.API/Controllers/ShiftsController.cs:24`, action `Current`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Shifts.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Resolved branch scope; explicit Shifts permission management, no own Shift prerequisite, may roll over while Shifts remain open. Create operation ID is day ID; no automatic midnight transition.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.BusinessDayDetails?](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-businessdaydetails). Nullable output may become **204 no body**.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("current"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Current([FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>shifts.CurrentBusinessDayAsync(branchId,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftService.CurrentBusinessDayAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/business-days/current HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
null
```
<a id="post-api-business-days"></a>
## POST /api/business-days
Source: `CampionsSystem.API/Controllers/ShiftsController.cs:26`, action `Open`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Shifts.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Resolved branch scope; explicit Shifts permission management, no own Shift prerequisite, may roll over while Shifts remain open. Create operation ID is day ID; no automatic midnight transition.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Shifts.OpenBusinessDayRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-openbusinessdayrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.BusinessDayDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-businessdaydetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:Shifts.Manage")]
    public Task<IActionResult> Open(OpenBusinessDayRequest r,CancellationToken ct)=>Execute(async()=>ShiftService.DayDetails(await shifts.OpenBusinessDayAsync(r,ct)));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftService.DayDetails`, `CampionsSystem.BLL.Shifts.ShiftService.OpenBusinessDayAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/business-days HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "businessDate": "2026-10-12",
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "businessDate": "2026-10-12",
  "status": 0,
  "openedAt": "2026-10-12T08:00:00Z",
  "closedAt": null,
  "openedByUserId": null,
  "closedByUserId": null
}
```
<a id="post-api-business-days-id-guid-close"></a>
## POST /api/business-days/{id:guid}/close
Source: `CampionsSystem.API/Controllers/ShiftsController.cs:28`, action `Close`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Shifts.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Resolved branch scope; explicit Shifts permission management, no own Shift prerequisite, may roll over while Shifts remain open. Create operation ID is day ID; no automatic midnight transition.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.BusinessDayDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-businessdaydetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/close"),Authorize(Policy="Permission:Shifts.Manage")]
    public Task<IActionResult> Close(Guid id,CancellationToken ct)=>Execute(async()=>ShiftService.DayDetails(await shifts.CloseBusinessDayAsync(id,ct)));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftService.CloseBusinessDayAsync`, `CampionsSystem.BLL.Shifts.ShiftService.DayDetails`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/business-days/11111111-1111-4111-8111-111111111111/close HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "businessDate": "2026-10-12",
  "status": 0,
  "openedAt": "2026-10-12T08:00:00Z",
  "closedAt": null,
  "openedByUserId": null,
  "closedByUserId": null
}
```
