# EmployeeBranches endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Branches.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline and matching local master/live remote master at audit. Runtime operations were not exercised here. 

**Scope/actor rules:** Current assignment scope for reads, destination and source scope for transfer; inactive/overlapping/future assignments fail closed. Transfer requires own Shift already closed and preserves CRM ownership.

<a id="get-api-employees-employeeid-guid-branch-assignment"></a>
## GET /api/employees/{employeeId:guid}/branch-assignment
Source: `CampionsSystem.API/Controllers/EmployeeBranchesController.cs:11`, action `Current`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Employees.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Branches.md). Current assignment scope for reads, destination and source scope for transfer; inactive/overlapping/future assignments fail closed. Transfer requires own Shift already closed and preserves CRM ownership.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `employeeId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Branches.BranchAssignmentDetails?](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-branches-branchassignmentdetails). Nullable output may become **204 no body**.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("branch-assignment"), Authorize(Policy = "Permission:Employees.View")]
    public Task<IActionResult> Current(Guid employeeId, CancellationToken ct) => Execute(() => assignments.GetCurrentAsync(employeeId, ct));
```
Service entry points: `CampionsSystem.BLL.Branches.EmployeeBranchAssignmentService.GetCurrentAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/employees/11111111-1111-4111-8111-111111111111/branch-assignment HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
null
```
<a id="get-api-employees-employeeid-guid-branch-history"></a>
## GET /api/employees/{employeeId:guid}/branch-history
Source: `CampionsSystem.API/Controllers/EmployeeBranchesController.cs:14`, action `History`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Employees.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Branches.md). Current assignment scope for reads, destination and source scope for transfer; inactive/overlapping/future assignments fail closed. Transfer requires own Shift already closed and preserves CRM ownership.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `employeeId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Branches.BranchAssignmentDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-branches-branchassignmentdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("branch-history"), Authorize(Policy = "Permission:Employees.View")]
    public Task<IActionResult> History(Guid employeeId, CancellationToken ct) => Execute(() => assignments.HistoryAsync(employeeId, ct));
```
Service entry points: `CampionsSystem.BLL.Branches.EmployeeBranchAssignmentService.HistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/employees/11111111-1111-4111-8111-111111111111/branch-history HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="post-api-employees-employeeid-guid-assign-branch"></a>
## POST /api/employees/{employeeId:guid}/assign-branch
Source: `CampionsSystem.API/Controllers/EmployeeBranchesController.cs:17`, action `Assign`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Employees.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Branches.md). Current assignment scope for reads, destination and source scope for transfer; inactive/overlapping/future assignments fail closed. Transfer requires own Shift already closed and preserves CRM ownership.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `employeeId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Branches.AssignBranchRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-branches-assignbranchrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Branches.BranchAssignmentDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-branches-branchassignmentdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("assign-branch"), Authorize(Policy = "Permission:Employees.Manage")]
    public Task<IActionResult> Assign(Guid employeeId, AssignBranchRequest request, CancellationToken ct) => Execute(() => assignments.AssignAsync(employeeId, request, ct));
```
Service entry points: `CampionsSystem.BLL.Branches.EmployeeBranchAssignmentService.AssignAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/employees/11111111-1111-4111-8111-111111111111/assign-branch HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "startDate": "2026-10-12"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "employeeId": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "branchName": "FAKE branchName",
  "startDate": "2026-10-12",
  "endDate": null,
  "isPrimary": false,
  "assignedByUserId": "11111111-1111-4111-8111-111111111111",
  "createdAt": "2026-10-12T08:00:00Z"
}
```
<a id="post-api-employees-employeeid-guid-transfer-branch"></a>
## POST /api/employees/{employeeId:guid}/transfer-branch
Source: `CampionsSystem.API/Controllers/EmployeeBranchesController.cs:19`, action `Transfer`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Employees.Transfer`.
Scope/actor: apply module rules above and [guard evidence](rules/Branches.md). Current assignment scope for reads, destination and source scope for transfer; inactive/overlapping/future assignments fail closed. Transfer requires own Shift already closed and preserves CRM ownership.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `employeeId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Branches.TransferBranchRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-branches-transferbranchrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Branches.BranchAssignmentDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-branches-branchassignmentdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("transfer-branch"), Authorize(Policy = "Permission:Employees.Transfer")]
    public Task<IActionResult> Transfer(Guid employeeId, TransferBranchRequest request, CancellationToken ct) =>
        Execute(() => assignments.TransferAsync(employeeId, request, ct));
```
Service entry points: `CampionsSystem.BLL.Branches.EmployeeBranchAssignmentService.TransferAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/employees/11111111-1111-4111-8111-111111111111/transfer-branch HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "targetBranchId": "11111111-1111-4111-8111-111111111111",
  "effectiveDate": "2026-10-12"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "employeeId": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "branchName": "FAKE branchName",
  "startDate": "2026-10-12",
  "endDate": null,
  "isPrimary": false,
  "assignedByUserId": "11111111-1111-4111-8111-111111111111",
  "createdAt": "2026-10-12T08:00:00Z"
}
```
