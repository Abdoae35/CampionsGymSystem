# Employees endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Employees.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. 

**Scope/actor rules:** Scoped by reliably resolved Employee current working assignment; global capability includes unassigned/other-branch records. Profile requests cannot assign accounts/branches/status outside dedicated actions.

<a id="get-api-employees"></a>
## GET /api/employees
Source: `CampionsSystem.API/Controllers/EmployeesController.cs:11`, action `List`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Employees.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Employees.md). Scoped by reliably resolved Employee current working assignment; global capability includes unassigned/other-branch records. Profile requests cannot assign accounts/branches/status outside dedicated actions.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `Search` / query | false | `{"maxLength":256,"type":"string"}` | See query DTO defaults / guard evidence |
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Status` / query | false | `{"$ref":"#/components/schemas/EmploymentStatus"}` | See query DTO defaults / guard evidence |
| `EmploymentType` / query | false | `{"$ref":"#/components/schemas/EmploymentType"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"maximum":2147483647,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"maximum":100,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Employees.EmployeeListRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-employeelistrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Employees.EmployeeListResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-employeelistresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet, Authorize(Policy = "Permission:Employees.View")]
    public Task<IActionResult> List([FromQuery] EmployeeListRequest request, CancellationToken ct) => Execute(() => employees.ListAsync(request, ct));
```
Service entry points: `CampionsSystem.BLL.Employees.EmployeeManagementService.ListAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/employees?Page=1&PageSize=25 HTTP/1.1
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
<a id="post-api-employees"></a>
## POST /api/employees
Source: `CampionsSystem.API/Controllers/EmployeesController.cs:14`, action `Create`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Employees.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Employees.md). Scoped by reliably resolved Employee current working assignment; global capability includes unassigned/other-branch records. Profile requests cannot assign accounts/branches/status outside dedicated actions.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Employees.CreateEmployeeRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-createemployeerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Employees.EmployeeDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-employeedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost, Authorize(Policy = "Permission:Employees.Manage")]
    public Task<IActionResult> Create(CreateEmployeeRequest request, CancellationToken ct) => Execute(() => employees.CreateAsync(request, ct));
```
Service entry points: `CampionsSystem.BLL.Employees.EmployeeManagementService.CreateAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/employees HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "initialBranchId": null,
  "fullName": "FAKE fullName",
  "phone": "+201000000099",
  "jobTitle": null,
  "employmentType": "Staff",
  "hireDate": "2026-10-12"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "fullName": "FAKE fullName",
  "phone": "+201000000099",
  "jobTitle": null,
  "employmentType": "Staff",
  "status": "Active",
  "hireDate": "2026-10-12",
  "terminationDate": null,
  "currentBranch": null,
  "hasSystemAccount": false
}
```
<a id="get-api-employees-id-guid"></a>
## GET /api/employees/{id:guid}
Source: `CampionsSystem.API/Controllers/EmployeesController.cs:17`, action `Get`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Employees.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Employees.md). Scoped by reliably resolved Employee current working assignment; global capability includes unassigned/other-branch records. Profile requests cannot assign accounts/branches/status outside dedicated actions.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Employees.EmployeeDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-employeedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"), Authorize(Policy = "Permission:Employees.View")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) => Execute(() => employees.GetAsync(id, ct));
```
Service entry points: `CampionsSystem.BLL.Employees.EmployeeManagementService.GetAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/employees/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "fullName": "FAKE fullName",
  "phone": "+201000000099",
  "jobTitle": null,
  "employmentType": "Staff",
  "status": "Active",
  "hireDate": "2026-10-12",
  "terminationDate": null,
  "currentBranch": null,
  "hasSystemAccount": false
}
```
<a id="put-api-employees-id-guid"></a>
## PUT /api/employees/{id:guid}
Source: `CampionsSystem.API/Controllers/EmployeesController.cs:20`, action `Update`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Employees.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Employees.md). Scoped by reliably resolved Employee current working assignment; global capability includes unassigned/other-branch records. Profile requests cannot assign accounts/branches/status outside dedicated actions.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Employees.EmployeeProfileRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-employeeprofilerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Employees.EmployeeDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-employeedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("{id:guid}"), Authorize(Policy = "Permission:Employees.Manage")]
    public Task<IActionResult> Update(Guid id, EmployeeProfileRequest request, CancellationToken ct) => Execute(() => employees.UpdateAsync(id, request, ct));
```
Service entry points: `CampionsSystem.BLL.Employees.EmployeeManagementService.UpdateAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/employees/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "fullName": "FAKE fullName",
  "phone": "+201000000099",
  "jobTitle": null,
  "employmentType": "Staff",
  "hireDate": "2026-10-12"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "fullName": "FAKE fullName",
  "phone": "+201000000099",
  "jobTitle": null,
  "employmentType": "Staff",
  "status": "Active",
  "hireDate": "2026-10-12",
  "terminationDate": null,
  "currentBranch": null,
  "hasSystemAccount": false
}
```
<a id="patch-api-employees-id-guid-status"></a>
## PATCH /api/employees/{id:guid}/status
Source: `CampionsSystem.API/Controllers/EmployeesController.cs:23`, action `Status`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Employees.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Employees.md). Scoped by reliably resolved Employee current working assignment; global capability includes unassigned/other-branch records. Profile requests cannot assign accounts/branches/status outside dedicated actions.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Employees.ChangeEmployeeStatusRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-changeemployeestatusrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Employees.EmployeeDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-employeedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPatch("{id:guid}/status"), Authorize(Policy = "Permission:Employees.Manage")]
    public Task<IActionResult> Status(Guid id, ChangeEmployeeStatusRequest request, CancellationToken ct) => Execute(() => employees.ChangeStatusAsync(id, request, ct));
```
Service entry points: `CampionsSystem.BLL.Employees.EmployeeManagementService.ChangeStatusAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PATCH /api/employees/11111111-1111-4111-8111-111111111111/status HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "status": "Suspended",
  "reason": "FAKE status change"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "fullName": "FAKE fullName",
  "phone": "+201000000099",
  "jobTitle": null,
  "employmentType": "Staff",
  "status": "Active",
  "hireDate": "2026-10-12",
  "terminationDate": null,
  "currentBranch": null,
  "hasSystemAccount": false
}
```
<a id="get-api-branches-branchid-guid-employees"></a>
## GET /api/branches/{branchId:guid}/employees
Source: `CampionsSystem.API/Controllers/EmployeesController.cs:26`, action `BranchEmployees`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Employees.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Employees.md). Scoped by reliably resolved Employee current working assignment; global capability includes unassigned/other-branch records. Profile requests cannot assign accounts/branches/status outside dedicated actions.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Search` / query | false | `{"maxLength":256,"type":"string"}` | See query DTO defaults / guard evidence |
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Status` / query | false | `{"$ref":"#/components/schemas/EmploymentStatus"}` | See query DTO defaults / guard evidence |
| `EmploymentType` / query | false | `{"$ref":"#/components/schemas/EmploymentType"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"maximum":2147483647,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"maximum":100,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Employees.EmployeeListRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-employeelistrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Employees.EmployeeListResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-employees-employeelistresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("/api/branches/{branchId:guid}/employees"), Authorize(Policy = "Permission:Employees.View")]
    public Task<IActionResult> BranchEmployees(Guid branchId, [FromQuery] EmployeeListRequest request, CancellationToken ct)
    {
        request.BranchId = branchId;
        return Execute(() => employees.ListAsync(request, ct));
    }
```
Service entry points: `CampionsSystem.BLL.Employees.EmployeeManagementService.ListAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/branches/11111111-1111-4111-8111-111111111111/employees?Page=1&PageSize=25 HTTP/1.1
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
