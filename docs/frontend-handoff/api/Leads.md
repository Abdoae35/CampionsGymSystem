# Leads endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Crm.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline and matching local master/live remote master at audit. Runtime operations were not exercised here. 

**Scope/actor rules:** Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.

<a id="get-api-leads"></a>
## GET /api/leads
Source: `CampionsSystem.API/Controllers/LeadsController.cs:9`, action `List`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `Search` / query | false | `{"maxLength":256,"type":"string"}` | See query DTO defaults / guard evidence |
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `SalesEmployeeId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `UnassignedOnly` / query | false | `{"type":"boolean"}` | See query DTO defaults / guard evidence |
| `Status` / query | false | `{"$ref":"#/components/schemas/LeadStatus"}` | See query DTO defaults / guard evidence |
| `FollowUp` / query | false | `{"$ref":"#/components/schemas/FollowUpWindow"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"maximum":2147483647,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"maximum":100,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Crm.LeadListRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadlistrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Crm.LeadListResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadlistresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet,Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> List([FromQuery] LeadListRequest r,CancellationToken ct)=>Execute(()=>leads.GetLeadsAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.GetLeadsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/leads?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-leads-my"></a>
## GET /api/leads/my
Source: `CampionsSystem.API/Controllers/LeadsController.cs:11`, action `Mine`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `Search` / query | false | `{"maxLength":256,"type":"string"}` | See query DTO defaults / guard evidence |
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `SalesEmployeeId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `UnassignedOnly` / query | false | `{"type":"boolean"}` | See query DTO defaults / guard evidence |
| `Status` / query | false | `{"$ref":"#/components/schemas/LeadStatus"}` | See query DTO defaults / guard evidence |
| `FollowUp` / query | false | `{"$ref":"#/components/schemas/FollowUpWindow"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"maximum":2147483647,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"maximum":100,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Crm.LeadListRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadlistrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Crm.LeadListResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadlistresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("my"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> Mine([FromQuery] LeadListRequest r,CancellationToken ct)=>Execute(()=>leads.GetMyLeadsAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.GetMyLeadsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/leads/my?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-leads-lookup"></a>
## GET /api/leads/lookup
Source: `CampionsSystem.API/Controllers/LeadsController.cs:13`, action `Lookup`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `phone` / query | false | `{"type":"string"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Crm.LeadCreationResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadcreationresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("lookup"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> Lookup([FromQuery] Guid branchId,[FromQuery] string phone,CancellationToken ct)=>Execute(()=>leads.LookupPhoneAsync(branchId,phone,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.LookupPhoneAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/leads/lookup HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "result": "AlreadyMember",
  "lead": null,
  "existingMemberId": "33333333-3333-4333-8333-333333333333",
  "error": null,
  "existingLeadId": null
}
```
<a id="get-api-leads-id-guid"></a>
## GET /api/leads/{id:guid}
Source: `CampionsSystem.API/Controllers/LeadsController.cs:15`, action `Get`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Crm.LeadDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leaddetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>leads.GetLeadByIdAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.GetLeadByIdAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/leads/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "branchName": "FAKE branchName",
  "fullName": null,
  "primaryPhone": "+201000000099",
  "primaryPhoneNormalized": null,
  "secondaryPhone": null,
  "email": null,
  "source": null,
  "notes": null,
  "status": "New",
  "createdAt": "2026-10-12T08:00:00Z",
  "nextFollowUpAt": null,
  "currentAssignment": null,
  "convertedMemberId": null,
  "convertedAt": null,
  "lostAt": null,
  "lostReason": null
}
```
<a id="post-api-leads"></a>
## POST /api/leads
Source: `CampionsSystem.API/Controllers/LeadsController.cs:17`, action `Create`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Crm.CreateLeadRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-createleadrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Crm.LeadCreationResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadcreationresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:Leads.Manage")]
    public Task<IActionResult> Create(CreateLeadRequest r,CancellationToken ct)=>Execute(()=>leads.CreateLeadAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.CreateLeadAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/leads HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "primaryPhone": "+201000000099",
  "fullName": "FAKE Prospect"
}
```
Schema-shaped response (not an executed response):
```json
{
  "result": "AlreadyMember",
  "lead": null,
  "existingMemberId": "33333333-3333-4333-8333-333333333333",
  "error": null,
  "existingLeadId": null
}
```
<a id="post-api-leads-bulk"></a>
## POST /api/leads/bulk
Source: `CampionsSystem.API/Controllers/LeadsController.cs:19`, action `Bulk`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.Assign`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). BLL additionally requires Leads.Manage; 1..100 JSON rows with row-level results, not all-or-nothing importer.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Text.Json.JsonElement[]](../DTO_AND_ENUM_REFERENCE.md#system-text-json-jsonelement); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [IReadOnlyList<Crm.LeadCreationResult>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-crm-leadcreationresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("bulk"),Authorize(Policy="Permission:Leads.Assign")]
    public Task<IActionResult> Bulk([FromBody] JsonElement[] rows,CancellationToken ct)=>Execute(()=>leads.CreateLeadsBulkAsync(rows,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.CreateLeadsBulkAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/leads/bulk HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

[
  {
    "branchId": "11111111-1111-4111-8111-111111111111",
    "primaryPhone": "+201000000099",
    "fullName": "FAKE Prospect"
  }
]
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="put-api-leads-id-guid"></a>
## PUT /api/leads/{id:guid}
Source: `CampionsSystem.API/Controllers/LeadsController.cs:21`, action `Update`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Crm.UpdateLeadRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-updateleadrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Crm.LeadDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leaddetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("{id:guid}"),Authorize(Policy="Permission:Leads.Manage")]
    public Task<IActionResult> Update(Guid id,UpdateLeadRequest r,CancellationToken ct)=>Execute(()=>leads.UpdateLeadAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.UpdateLeadAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/leads/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "fullName": null,
  "primaryPhone": "+201000000099",
  "secondaryPhone": null,
  "email": null,
  "source": null,
  "notes": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "branchName": "FAKE branchName",
  "fullName": null,
  "primaryPhone": "+201000000099",
  "primaryPhoneNormalized": null,
  "secondaryPhone": null,
  "email": null,
  "source": null,
  "notes": null,
  "status": "New",
  "createdAt": "2026-10-12T08:00:00Z",
  "nextFollowUpAt": null,
  "currentAssignment": null,
  "convertedMemberId": null,
  "convertedAt": null,
  "lostAt": null,
  "lostReason": null
}
```
<a id="post-api-leads-id-guid-assign"></a>
## POST /api/leads/{id:guid}/assign
Source: `CampionsSystem.API/Controllers/LeadsController.cs:23`, action `Assign`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.Assign`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Crm.LeadAssignmentRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadassignmentrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Crm.LeadDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leaddetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/assign"),Authorize(Policy="Permission:Leads.Assign")]
    public Task<IActionResult> Assign(Guid id,LeadAssignmentRequest r,CancellationToken ct)=>Execute(()=>leads.AssignLeadAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.AssignLeadAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/leads/11111111-1111-4111-8111-111111111111/assign HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "salesEmployeeId": "11111111-1111-4111-8111-111111111111",
  "reason": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "branchName": "FAKE branchName",
  "fullName": null,
  "primaryPhone": "+201000000099",
  "primaryPhoneNormalized": null,
  "secondaryPhone": null,
  "email": null,
  "source": null,
  "notes": null,
  "status": "New",
  "createdAt": "2026-10-12T08:00:00Z",
  "nextFollowUpAt": null,
  "currentAssignment": null,
  "convertedMemberId": null,
  "convertedAt": null,
  "lostAt": null,
  "lostReason": null
}
```
<a id="get-api-leads-id-guid-assignment"></a>
## GET /api/leads/{id:guid}/assignment
Source: `CampionsSystem.API/Controllers/LeadsController.cs:25`, action `Assignment`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Crm.LeadAssignmentDetails?](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadassignmentdetails). Nullable output may become **204 no body**.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/assignment"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> Assignment(Guid id,CancellationToken ct)=>Execute(()=>leads.GetCurrentLeadAssignmentAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.GetCurrentLeadAssignmentAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/leads/11111111-1111-4111-8111-111111111111/assignment HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
null
```
<a id="get-api-leads-id-guid-assignment-history"></a>
## GET /api/leads/{id:guid}/assignment-history
Source: `CampionsSystem.API/Controllers/LeadsController.cs:27`, action `AssignmentHistory`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Crm.LeadAssignmentDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-crm-leadassignmentdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/assignment-history"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> AssignmentHistory(Guid id,CancellationToken ct)=>Execute(()=>leads.GetLeadAssignmentHistoryAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.GetLeadAssignmentHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/leads/11111111-1111-4111-8111-111111111111/assignment-history HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="post-api-leads-id-guid-interactions"></a>
## POST /api/leads/{id:guid}/interactions
Source: `CampionsSystem.API/Controllers/LeadsController.cs:29`, action `Contact`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Crm.LeadInteractionRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadinteractionrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Crm.LeadDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leaddetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/interactions"),Authorize(Policy="Permission:Leads.Manage")]
    public Task<IActionResult> Contact(Guid id,LeadInteractionRequest r,CancellationToken ct)=>Execute(()=>leads.AddLeadInteractionAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.AddLeadInteractionAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/leads/11111111-1111-4111-8111-111111111111/interactions HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "interactionType": "Call",
  "notes": "FAKE recorded contact",
  "status": "Contacted",
  "nextFollowUpAt": null,
  "contactedAt": null,
  "outcome": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "branchName": "FAKE branchName",
  "fullName": null,
  "primaryPhone": "+201000000099",
  "primaryPhoneNormalized": null,
  "secondaryPhone": null,
  "email": null,
  "source": null,
  "notes": null,
  "status": "New",
  "createdAt": "2026-10-12T08:00:00Z",
  "nextFollowUpAt": null,
  "currentAssignment": null,
  "convertedMemberId": null,
  "convertedAt": null,
  "lostAt": null,
  "lostReason": null
}
```
<a id="get-api-leads-id-guid-interactions"></a>
## GET /api/leads/{id:guid}/interactions
Source: `CampionsSystem.API/Controllers/LeadsController.cs:31`, action `Contacts`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Crm.LeadInteractionDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-crm-leadinteractiondetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/interactions"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> Contacts(Guid id,[FromQuery] int page=1,[FromQuery] int pageSize=25,CancellationToken ct=default)=>Execute(()=>leads.GetLeadInteractionsAsync(id,page,pageSize,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.GetLeadInteractionsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/leads/11111111-1111-4111-8111-111111111111/interactions?page=1&pageSize=25 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="patch-api-leads-id-guid-status"></a>
## PATCH /api/leads/{id:guid}/status
Source: `CampionsSystem.API/Controllers/LeadsController.cs:33`, action `Status`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). Read/contact visibility follows management scope or narrow active Sales ownership across branches. Assign/reassign limited to operational branch/global scope; selected new owner must be eligible in that branch. Conversion is a narrow authorized destination workflow.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Crm.LeadStatusRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadstatusrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Crm.LeadDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leaddetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPatch("{id:guid}/status"),Authorize(Policy="Permission:Leads.Manage")]
    public Task<IActionResult> Status(Guid id,LeadStatusRequest r,CancellationToken ct)=>Execute(()=>leads.ChangeLeadStatusAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.ChangeLeadStatusAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PATCH /api/leads/11111111-1111-4111-8111-111111111111/status HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "status": "Lost",
  "reason": "FAKE requested closure",
  "nextFollowUpAt": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "branchName": "FAKE branchName",
  "fullName": null,
  "primaryPhone": "+201000000099",
  "primaryPhoneNormalized": null,
  "secondaryPhone": null,
  "email": null,
  "source": null,
  "notes": null,
  "status": "New",
  "createdAt": "2026-10-12T08:00:00Z",
  "nextFollowUpAt": null,
  "currentAssignment": null,
  "convertedMemberId": null,
  "convertedAt": null,
  "lostAt": null,
  "lostReason": null
}
```
<a id="post-api-leads-id-guid-convert"></a>
## POST /api/leads/{id:guid}/convert
Source: `CampionsSystem.API/Controllers/LeadsController.cs:35`, action `Convert`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Leads.Convert`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). BLL additionally requires Members.Create, active converter Employee, own Shift and destination/cross-branch/Freelance checks. Preserve current eligible Lead owner; normal unassigned Lead rejects.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Crm.ConvertLeadRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-convertleadrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Members.MemberDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-memberdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/convert"),Authorize(Policy="Permission:Leads.Convert")]
    public Task<IActionResult> Convert(Guid id,ConvertLeadRequest r,CancellationToken ct)=>Execute(()=>leads.ConvertLeadAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.ConvertLeadAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/leads/11111111-1111-4111-8111-111111111111/convert HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "targetRegistrationBranchId": "11111111-1111-4111-8111-111111111111",
  "freelancePlanId": null,
  "fullName": "FAKE fullName",
  "primaryPhone": "+201000000099",
  "secondaryPhone": null,
  "dateOfBirth": null,
  "gender": null,
  "notes": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "clientCode": null,
  "memberCode": "M-FAKE-EXAMPLE",
  "fullName": "FAKE fullName",
  "primaryPhone": "+201000000099",
  "primaryPhoneNormalized": null,
  "secondaryPhone": null,
  "isPrimaryPhoneVerified": false,
  "dateOfBirth": null,
  "gender": null,
  "notes": null,
  "isActive": true,
  "registrationBranch": {
    "id": "11111111-1111-4111-8111-111111111111",
    "name": "FAKE name"
  },
  "createdAt": "2026-10-12T08:00:00Z",
  "currentSalesOwner": null,
  "employee": null,
  "eligibleFreelancePlans": [],
  "isHouse": true
}
```
