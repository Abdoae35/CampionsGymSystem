# EmployeeAdvances endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Shifts.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE at completed backend a2a0c56, inspected at 476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80. Runtime business operations were not exercised by this documentation task.

**Scope/actor rules:** Branch/account history and active receiving Employee; recording Employee must have own operational Shift. Separate from payroll or expense settlement.

<a id="post-api-employee-advances"></a>
## POST /api/employee-advances
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:28`, action `Paid`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Advances.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account history and active receiving Employee; recording Employee must have own operational Shift. Separate from payroll or expense settlement.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Shifts.AdvanceRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-advancerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.AdvanceDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-advancedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:Advances.Create")]
    public Task<IActionResult> Paid(AdvanceRequest r,CancellationToken ct)=>Execute(()=>expenses.RecordAdvanceAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.RecordAdvanceAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/employee-advances HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "receivingEmployeeId": "11111111-1111-4111-8111-111111111111",
  "financialAccountId": "11111111-1111-4111-8111-111111111111",
  "amount": 1200,
  "reason": "FAKE reason",
  "reference": null,
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "shiftId": "11111111-1111-4111-8111-111111111111",
  "receivingEmployeeId": "11111111-1111-4111-8111-111111111111",
  "receivingEmployeeName": "FAKE receivingEmployeeName",
  "recordingEmployeeId": "11111111-1111-4111-8111-111111111111",
  "accountId": "11111111-1111-4111-8111-111111111111",
  "method": 0,
  "amount": 1200,
  "reason": "FAKE reason",
  "reference": null,
  "occurredAt": "2026-10-12T08:00:00Z",
  "businessDate": "2026-10-12"
}
```
<a id="get-api-employee-advances"></a>
## GET /api/employee-advances
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:30`, action `History`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Advances.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account history and active receiving Employee; recording Employee must have own operational Shift. Separate from payroll or expense settlement.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `AccountId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `From` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `To` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Shifts.VaultHistoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-vaulthistoryrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.ShiftPage<Shifts.AdvanceDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-advancedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet,Authorize(Policy="Permission:Advances.View")]
    public Task<IActionResult> History([FromQuery]VaultHistoryRequest r,CancellationToken ct)=>Execute(()=>expenses.AdvancesAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.AdvancesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/employee-advances?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-employee-advances-id-guid"></a>
## GET /api/employee-advances/{id:guid}
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:32`, action `Get`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Advances.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account history and active receiving Employee; recording Employee must have own operational Shift. Separate from payroll or expense settlement.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.AdvanceDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-advancedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"),Authorize(Policy="Permission:Advances.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>expenses.GetAdvanceAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.GetAdvanceAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/employee-advances/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "shiftId": "11111111-1111-4111-8111-111111111111",
  "receivingEmployeeId": "11111111-1111-4111-8111-111111111111",
  "receivingEmployeeName": "FAKE receivingEmployeeName",
  "recordingEmployeeId": "11111111-1111-4111-8111-111111111111",
  "accountId": "11111111-1111-4111-8111-111111111111",
  "method": 0,
  "amount": 1200,
  "reason": "FAKE reason",
  "reference": null,
  "occurredAt": "2026-10-12T08:00:00Z",
  "businessDate": "2026-10-12"
}
```
