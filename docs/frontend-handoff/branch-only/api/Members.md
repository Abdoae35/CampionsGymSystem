# Members endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Members.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. 

**Scope/actor rules:** Registration-branch directory/administration scope; Sales ownership alone does not broaden ordinary directory. Create needs active eligible Employee and own Shift; creationType controls ownership and additional capabilities.

<a id="get-api-members"></a>
## GET /api/members
Source: `CampionsSystem.API/Controllers/MembersController.cs:11`, action `List`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Members.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Members.md). Registration-branch directory/administration scope; Sales ownership alone does not broaden ordinary directory. Create needs active eligible Employee and own Shift; creationType controls ownership and additional capabilities.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `Search` / query | false | `{"maxLength":256,"type":"string"}` | See query DTO defaults / guard evidence |
| `RegistrationBranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `ClientCode` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int64"}` | See query DTO defaults / guard evidence |
| `IsActive` / query | false | `{"type":"boolean"}` | See query DTO defaults / guard evidence |
| `SalesEmployeeId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `HouseOnly` / query | false | `{"type":"boolean"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"maximum":2147483647,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"maximum":100,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Members.MemberListRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-memberlistrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Members.MemberListResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-memberlistresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet, Authorize(Policy = "Permission:Members.View")]
    public Task<IActionResult> List([FromQuery] MemberListRequest request, CancellationToken ct) => Execute(() => members.GetMembersAsync(request, ct));
```
Service entry points: `CampionsSystem.BLL.Members.MemberManagementService.GetMembersAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/members?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-members-id-guid"></a>
## GET /api/members/{id:guid}
Source: `CampionsSystem.API/Controllers/MembersController.cs:13`, action `Get`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Members.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Members.md). Registration-branch directory/administration scope; Sales ownership alone does not broaden ordinary directory. Create needs active eligible Employee and own Shift; creationType controls ownership and additional capabilities.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Members.MemberDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-memberdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"), Authorize(Policy = "Permission:Members.View")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) => Execute(() => members.GetMemberByIdAsync(id, ct));
```
Service entry points: `CampionsSystem.BLL.Members.MemberManagementService.GetMemberByIdAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/members/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
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
<a id="post-api-members"></a>
## POST /api/members
Source: `CampionsSystem.API/Controllers/MembersController.cs:15`, action `Create`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Members.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/Members.md). Members.OwnSalesCustomers for NormalSales; Members.AssignSales for ManagementCreated; FreelancePlans.ManageEligibleMembers for FreelanceHouse; Members.CreateCrossBranch when applicable; actual own Shift gate.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Members.CreateMemberRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-creatememberrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Members.MemberDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-memberdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost, Authorize(Policy = "Permission:Members.Create")]
    public Task<IActionResult> Create(CreateMemberRequest request, CancellationToken ct) => Execute(() => members.CreateMemberAsync(request, ct));
```
Service entry points: `CampionsSystem.BLL.Members.MemberManagementService.CreateMemberAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/members HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "fullName": "FAKE Member",
  "primaryPhone": "+201000000099",
  "targetRegistrationBranchId": "11111111-1111-4111-8111-111111111111",
  "creationType": "NormalSales",
  "salesEmployeeId": null,
  "freelancePlanId": null
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
<a id="put-api-members-id-guid"></a>
## PUT /api/members/{id:guid}
Source: `CampionsSystem.API/Controllers/MembersController.cs:17`, action `Update`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Members.Edit`.
Scope/actor: apply module rules above and [guard evidence](rules/Members.md). Changed primary phone additionally requires Members.ChangePrimaryPhone.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Members.UpdateMemberRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-updatememberrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Members.MemberDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-memberdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("{id:guid}"), Authorize(Policy = "Permission:Members.Edit")]
    public Task<IActionResult> Update(Guid id, UpdateMemberRequest request, CancellationToken ct) => Execute(() => members.UpdateMemberAsync(id, request, ct));
```
Service entry points: `CampionsSystem.BLL.Members.MemberManagementService.UpdateMemberAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/members/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
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
<a id="patch-api-members-id-guid-status"></a>
## PATCH /api/members/{id:guid}/status
Source: `CampionsSystem.API/Controllers/MembersController.cs:19`, action `Status`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Members.ChangeStatus`.
Scope/actor: apply module rules above and [guard evidence](rules/Members.md). Registration-branch directory/administration scope; Sales ownership alone does not broaden ordinary directory. Create needs active eligible Employee and own Shift; creationType controls ownership and additional capabilities.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Members.ChangeMemberStatusRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-changememberstatusrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Members.MemberDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-memberdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPatch("{id:guid}/status"), Authorize(Policy = "Permission:Members.ChangeStatus")]
    public Task<IActionResult> Status(Guid id, ChangeMemberStatusRequest request, CancellationToken ct) => Execute(() => members.ChangeMemberStatusAsync(id, request, ct));
```
Service entry points: `CampionsSystem.BLL.Members.MemberManagementService.ChangeMemberStatusAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PATCH /api/members/11111111-1111-4111-8111-111111111111/status HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "isActive": false,
  "reason": "FAKE status change"
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
<a id="get-api-members-id-guid-sales-assignment"></a>
## GET /api/members/{id:guid}/sales-assignment
Source: `CampionsSystem.API/Controllers/MembersController.cs:21`, action `CurrentSales`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Members.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Members.md). Registration-branch directory/administration scope; Sales ownership alone does not broaden ordinary directory. Create needs active eligible Employee and own Shift; creationType controls ownership and additional capabilities.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Members.SalesAssignmentDetails?](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-salesassignmentdetails). Nullable output may become **204 no body**.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/sales-assignment"), Authorize(Policy = "Permission:Members.View")]
    public Task<IActionResult> CurrentSales(Guid id, CancellationToken ct) => Execute(() => members.GetCurrentSalesAssignmentAsync(id, ct));
```
Service entry points: `CampionsSystem.BLL.Members.MemberManagementService.GetCurrentSalesAssignmentAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/members/11111111-1111-4111-8111-111111111111/sales-assignment HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
null
```
<a id="get-api-members-id-guid-sales-history"></a>
## GET /api/members/{id:guid}/sales-history
Source: `CampionsSystem.API/Controllers/MembersController.cs:23`, action `SalesHistory`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Members.ViewSalesHistory`.
Scope/actor: apply module rules above and [guard evidence](rules/Members.md). Registration-branch directory/administration scope; Sales ownership alone does not broaden ordinary directory. Create needs active eligible Employee and own Shift; creationType controls ownership and additional capabilities.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Members.SalesAssignmentHistoryItem>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-members-salesassignmenthistoryitem). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/sales-history"), Authorize(Policy = "Permission:Members.ViewSalesHistory")]
    public Task<IActionResult> SalesHistory(Guid id, CancellationToken ct) => Execute(() => members.GetSalesAssignmentHistoryAsync(id, ct));
```
Service entry points: `CampionsSystem.BLL.Members.MemberManagementService.GetSalesAssignmentHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/members/11111111-1111-4111-8111-111111111111/sales-history HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="post-api-members-id-guid-assign-sales"></a>
## POST /api/members/{id:guid}/assign-sales
Source: `CampionsSystem.API/Controllers/MembersController.cs:25`, action `AssignSales`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Members.AssignSales`.
Scope/actor: apply module rules above and [guard evidence](rules/Members.md). Registration-branch directory/administration scope; Sales ownership alone does not broaden ordinary directory. Create needs active eligible Employee and own Shift; creationType controls ownership and additional capabilities.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Members.SalesAssignmentRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-salesassignmentrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Members.MemberDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-memberdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/assign-sales"), Authorize(Policy = "Permission:Members.AssignSales")]
    public Task<IActionResult> AssignSales(Guid id, SalesAssignmentRequest request, CancellationToken ct) => Execute(() => members.AssignSalesAsync(id, request, ct));
```
Service entry points: `CampionsSystem.BLL.Members.MemberManagementService.AssignSalesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/members/11111111-1111-4111-8111-111111111111/assign-sales HTTP/1.1
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
<a id="post-api-members-id-guid-return-to-house"></a>
## POST /api/members/{id:guid}/return-to-house
Source: `CampionsSystem.API/Controllers/MembersController.cs:27`, action `ReturnToHouse`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Members.AssignSales`.
Scope/actor: apply module rules above and [guard evidence](rules/Members.md). Registration-branch directory/administration scope; Sales ownership alone does not broaden ordinary directory. Create needs active eligible Employee and own Shift; creationType controls ownership and additional capabilities.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Members.ReturnToHouseRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-returntohouserequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Members.MemberDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-memberdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/return-to-house"), Authorize(Policy = "Permission:Members.AssignSales")]
    public Task<IActionResult> ReturnToHouse(Guid id, ReturnToHouseRequest request, CancellationToken ct) => Execute(() => members.ReturnMemberToHouseAsync(id, request, ct));
```
Service entry points: `CampionsSystem.BLL.Members.MemberManagementService.ReturnMemberToHouseAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/members/11111111-1111-4111-8111-111111111111/return-to-house HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "reason": null
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
