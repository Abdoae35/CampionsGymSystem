# Crm endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Crm.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. 

**Scope/actor rules:** My queue resolves actor Employee, across owned subjects. House queue/distribution is registration-branch management scope; open/complete requires ownership or Members.AssignSales with branch scope. Distribution also calls Members.AssignSales.

<a id="get-api-crm-my-follow-ups"></a>
## GET /api/crm/my-follow-ups
Source: `CampionsSystem.API/Controllers/CrmController.cs:9`, action `Mine`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CRM.ViewMemberFollowUps`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). My queue resolves actor Employee, across owned subjects. House queue/distribution is registration-branch management scope; open/complete requires ownership or Members.AssignSales with branch scope. Distribution also calls Members.AssignSales.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `Window` / query | false | `{"$ref":"#/components/schemas/FollowUpWindow"}` | See query DTO defaults / guard evidence |
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"maximum":2147483647,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"maximum":100,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Crm.FollowUpListRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-followuplistrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Crm.FollowUpListResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-followuplistresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("my-follow-ups"),Authorize(Policy="Permission:CRM.ViewMemberFollowUps")]
    public Task<IActionResult> Mine([FromQuery] FollowUpListRequest r,CancellationToken ct)=>Execute(()=>crm.GetMyFollowUpsAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.GetMyFollowUpsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/crm/my-follow-ups?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-crm-house-follow-ups"></a>
## GET /api/crm/house-follow-ups
Source: `CampionsSystem.API/Controllers/CrmController.cs:11`, action `House`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CRM.ManageHouseFollowUps`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). My queue resolves actor Employee, across owned subjects. House queue/distribution is registration-branch management scope; open/complete requires ownership or Members.AssignSales with branch scope. Distribution also calls Members.AssignSales.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `Window` / query | false | `{"$ref":"#/components/schemas/FollowUpWindow"}` | See query DTO defaults / guard evidence |
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"maximum":2147483647,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"maximum":100,"minimum":1,"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Crm.FollowUpListRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-followuplistrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Crm.FollowUpListResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-followuplistresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("house-follow-ups"),Authorize(Policy="Permission:CRM.ManageHouseFollowUps")]
    public Task<IActionResult> House([FromQuery] FollowUpListRequest r,CancellationToken ct)=>Execute(()=>crm.GetHouseFollowUpsAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.GetHouseFollowUpsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/crm/house-follow-ups?Page=1&PageSize=25 HTTP/1.1
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
<a id="post-api-crm-members-id-guid-follow-ups"></a>
## POST /api/crm/members/{id:guid}/follow-ups
Source: `CampionsSystem.API/Controllers/CrmController.cs:13`, action `Open`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CRM.ManageMemberFollowUps`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). My queue resolves actor Employee, across owned subjects. House queue/distribution is registration-branch management scope; open/complete requires ownership or Members.AssignSales with branch scope. Distribution also calls Members.AssignSales.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Crm.MemberFollowUpRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-memberfollowuprequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Guid](../DTO_AND_ENUM_REFERENCE.md#system-guid). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("members/{id:guid}/follow-ups"),Authorize(Policy="Permission:CRM.ManageMemberFollowUps")]
    public Task<IActionResult> Open(Guid id,MemberFollowUpRequest r,CancellationToken ct)=>Execute(()=>crm.OpenMemberFollowUpAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.OpenMemberFollowUpAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/crm/members/11111111-1111-4111-8111-111111111111/follow-ups HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "reason": "Reactivation",
  "dueAt": "2026-10-13T08:00:00Z",
  "notes": "FAKE follow-up"
}
```
Schema-shaped response (not an executed response):
```json
"11111111-1111-4111-8111-111111111111"
```
<a id="post-api-crm-member-follow-ups-id-guid-complete"></a>
## POST /api/crm/member-follow-ups/{id:guid}/complete
Source: `CampionsSystem.API/Controllers/CrmController.cs:15`, action `Complete`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CRM.ManageMemberFollowUps`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). My queue resolves actor Employee, across owned subjects. House queue/distribution is registration-branch management scope; open/complete requires ownership or Members.AssignSales with branch scope. Distribution also calls Members.AssignSales.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Guid](../DTO_AND_ENUM_REFERENCE.md#system-guid). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("member-follow-ups/{id:guid}/complete"),Authorize(Policy="Permission:CRM.ManageMemberFollowUps")]
    public Task<IActionResult> Complete(Guid id,CancellationToken ct)=>Execute(()=>crm.CompleteMemberFollowUpAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.CompleteMemberFollowUpAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/crm/member-follow-ups/11111111-1111-4111-8111-111111111111/complete HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
"11111111-1111-4111-8111-111111111111"
```
<a id="post-api-crm-house-follow-ups-id-guid-assign"></a>
## POST /api/crm/house-follow-ups/{id:guid}/assign
Source: `CampionsSystem.API/Controllers/CrmController.cs:17`, action `Assign`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CRM.ManageHouseFollowUps`.
Scope/actor: apply module rules above and [guard evidence](rules/Crm.md). BLL invokes Members.AssignSales and checks registration-branch scope; no arbitrary random owner.
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
[HttpPost("house-follow-ups/{id:guid}/assign"),Authorize(Policy="Permission:CRM.ManageHouseFollowUps")]
    public Task<IActionResult> Assign(Guid id,SalesAssignmentRequest r,CancellationToken ct)=>Execute(()=>crm.AssignHouseFollowUpAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Crm.LeadManagementService.AssignHouseFollowUpAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/crm/house-follow-ups/11111111-1111-4111-8111-111111111111/assign HTTP/1.1
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
