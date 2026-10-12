# Shifts endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Shifts.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. 

**Scope/actor rules:** Manual actor-owned Shift. Open requires active linked Employee/assignment/configured drawer; close only original opener/current expectedVersion. Scoped history can view branch colleagues; summary also checks Vault.View.

<a id="post-api-shifts"></a>
## POST /api/shifts
Source: `CampionsSystem.API/Controllers/ShiftsController.cs:8`, action `Open`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Shifts.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Manual actor-owned Shift. Open requires active linked Employee/assignment/configured drawer; close only original opener/current expectedVersion. Scoped history can view branch colleagues; summary also checks Vault.View.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Shifts.OpenShiftRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-openshiftrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.ShiftDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:Shifts.Manage")]
    public Task<IActionResult> Open(OpenShiftRequest r,CancellationToken ct)=>Execute(()=>shifts.OpenAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftService.OpenAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/shifts HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "openingCash": 1200,
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "employeeId": null,
  "employeeName": null,
  "status": 0,
  "version": 1,
  "openedAt": "2026-10-12T08:00:00Z",
  "closedAt": null,
  "openingBusinessDate": null,
  "closingBusinessDate": null,
  "openingCash": 1200,
  "expectedClosingCash": null,
  "actualClosingCash": null,
  "difference": null
}
```
<a id="post-api-shifts-id-guid-close"></a>
## POST /api/shifts/{id:guid}/close
Source: `CampionsSystem.API/Controllers/ShiftsController.cs:10`, action `Close`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Shifts.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Manual actor-owned Shift. Open requires active linked Employee/assignment/configured drawer; close only original opener/current expectedVersion. Scoped history can view branch colleagues; summary also checks Vault.View.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Shifts.CloseShiftRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-closeshiftrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.ShiftDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. expectedVersion is required by this body; stale EF concurrency returns 409. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/close"),Authorize(Policy="Permission:Shifts.Manage")]
    public Task<IActionResult> Close(Guid id,CloseShiftRequest r,CancellationToken ct)=>Execute(()=>shifts.CloseAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftService.CloseAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/shifts/11111111-1111-4111-8111-111111111111/close HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "expectedVersion": 1,
  "countedCash": 1200,
  "reason": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "employeeId": null,
  "employeeName": null,
  "status": 0,
  "version": 1,
  "openedAt": "2026-10-12T08:00:00Z",
  "closedAt": null,
  "openingBusinessDate": null,
  "closingBusinessDate": null,
  "openingCash": 1200,
  "expectedClosingCash": null,
  "actualClosingCash": null,
  "difference": null
}
```
<a id="get-api-shifts-current"></a>
## GET /api/shifts/current
Source: `CampionsSystem.API/Controllers/ShiftsController.cs:12`, action `Current`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Shifts.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Manual actor-owned Shift. Open requires active linked Employee/assignment/configured drawer; close only original opener/current expectedVersion. Scoped history can view branch colleagues; summary also checks Vault.View.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.ShiftDetails?](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftdetails). Nullable output may become **204 no body**.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("current"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Current(CancellationToken ct)=>Execute(()=>shifts.CurrentAsync(ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftService.CurrentAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/shifts/current HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
null
```
<a id="get-api-shifts"></a>
## GET /api/shifts
Source: `CampionsSystem.API/Controllers/ShiftsController.cs:14`, action `History`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Shifts.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Manual actor-owned Shift. Open requires active linked Employee/assignment/configured drawer; close only original opener/current expectedVersion. Scoped history can view branch colleagues; summary also checks Vault.View.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `EmployeeId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `From` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `To` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Shifts.ShiftHistoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shifthistoryrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.ShiftPage<Shifts.ShiftDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-shiftdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet,Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> History([FromQuery]ShiftHistoryRequest r,CancellationToken ct)=>Execute(()=>shifts.HistoryAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftService.HistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/shifts?Page=1&PageSize=25 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "items": [],
  "total": 0,
  "page": 1,
  "pageSize": 25
}
```
<a id="get-api-shifts-id-guid"></a>
## GET /api/shifts/{id:guid}
Source: `CampionsSystem.API/Controllers/ShiftsController.cs:16`, action `Get`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Shifts.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Manual actor-owned Shift. Open requires active linked Employee/assignment/configured drawer; close only original opener/current expectedVersion. Scoped history can view branch colleagues; summary also checks Vault.View.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.ShiftDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>shifts.GetAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftService.GetAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/shifts/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "employeeId": null,
  "employeeName": null,
  "status": 0,
  "version": 1,
  "openedAt": "2026-10-12T08:00:00Z",
  "closedAt": null,
  "openingBusinessDate": null,
  "closingBusinessDate": null,
  "openingCash": 1200,
  "expectedClosingCash": null,
  "actualClosingCash": null,
  "difference": null
}
```
<a id="get-api-shifts-id-guid-summary"></a>
## GET /api/shifts/{id:guid}/summary
Source: `CampionsSystem.API/Controllers/ShiftsController.cs:18`, action `Summary`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Shifts.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). BLL requires Vault.View in addition to Shifts.View.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.ShiftFinancialSummary](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftfinancialsummary). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/summary"),Authorize(Policy="Permission:Shifts.View")]
    public Task<IActionResult> Summary(Guid id,CancellationToken ct)=>Execute(()=>reports.ShiftAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftReportService.ShiftAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/shifts/11111111-1111-4111-8111-111111111111/summary HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "shift": {
    "id": "11111111-1111-4111-8111-111111111111",
    "branchId": "11111111-1111-4111-8111-111111111111",
    "employeeId": null,
    "employeeName": null,
    "status": 0,
    "version": 1,
    "openedAt": "2026-10-12T08:00:00Z",
    "closedAt": null,
    "openingBusinessDate": null,
    "closingBusinessDate": null,
    "openingCash": 1200,
    "expectedClosingCash": null,
    "actualClosingCash": null,
    "difference": null
  },
  "activity": [],
  "currentSharedDrawer": {
    "branchId": "11111111-1111-4111-8111-111111111111",
    "accountId": "11111111-1111-4111-8111-111111111111",
    "bookCash": 0,
    "expectedPhysicalCash": 0,
    "lastCountedCash": 0,
    "movementSequence": 0,
    "lastCountId": null,
    "observedDiscrepancies": 0
  }
}
```
