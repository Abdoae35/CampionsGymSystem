# Subscriptions endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Subscriptions.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE at completed backend a2a0c56, inspected at 476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80. Runtime business operations were not exercised by this documentation task.

**Scope/actor rules:** History/management uses subscription origin branch; current entitlement resolves visited operational branch and sold City snapshot. Ownership is not subscription authority. Employee entitlement also needs Employees.Manage; holidays need global scope.

<a id="get-api-members-memberid-guid-subscriptions"></a>
## GET /api/members/{memberId:guid}/subscriptions
Source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:9`, action `History`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Subscriptions.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Subscriptions.md). History/management uses subscription origin branch; current entitlement resolves visited operational branch and sold City snapshot. Ownership is not subscription authority. Employee entitlement also needs Employees.Manage; holidays need global scope.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `memberId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Status` / query | false | `{"$ref":"#/components/schemas/SubscriptionStatus"}` | See query DTO defaults / guard evidence |
| `PlanType` / query | false | `{"$ref":"#/components/schemas/PlanCategory"}` | See query DTO defaults / guard evidence |
| `From` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `To` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
Query DTO: [Subscriptions.SubscriptionHistoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-subscriptionhistoryrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Subscriptions.SubscriptionHistory](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-subscriptionhistory). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("/api/members/{memberId:guid}/subscriptions"), Authorize(Policy="Permission:Subscriptions.View")]
    public Task<IActionResult> History(Guid memberId, [FromQuery] SubscriptionHistoryRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.GetHistoryAsync(memberId,request,ct));
```
Service entry points: `CampionsSystem.BLL.Subscriptions.SubscriptionService.GetHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/members/11111111-1111-4111-8111-111111111111/subscriptions?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-members-memberid-guid-subscriptions-current"></a>
## GET /api/members/{memberId:guid}/subscriptions/current
Source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:12`, action `Current`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Subscriptions.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Subscriptions.md). History/management uses subscription origin branch; current entitlement resolves visited operational branch and sold City snapshot. Ownership is not subscription authority. Employee entitlement also needs Employees.Manage; holidays need global scope.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `memberId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Subscriptions.MemberEntitlement](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-memberentitlement). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("/api/members/{memberId:guid}/subscriptions/current"), Authorize(Policy="Permission:Subscriptions.View")]
    public Task<IActionResult> Current(Guid memberId, [FromQuery] Guid? branchId, CancellationToken ct)
        => Execute(()=>subscriptions.GetCurrentEntitlementAsync(memberId,branchId,ct));
```
Service entry points: `CampionsSystem.BLL.Subscriptions.SubscriptionService.GetCurrentEntitlementAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/members/11111111-1111-4111-8111-111111111111/subscriptions/current HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "hasAccess": true,
  "subscription": null
}
```
<a id="get-api-subscriptions-id-guid"></a>
## GET /api/subscriptions/{id:guid}
Source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:15`, action `Details`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Subscriptions.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Subscriptions.md). History/management uses subscription origin branch; current entitlement resolves visited operational branch and sold City snapshot. Ownership is not subscription authority. Employee entitlement also needs Employees.Manage; holidays need global scope.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Subscriptions.SubscriptionDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-subscriptiondetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"), Authorize(Policy="Permission:Subscriptions.View")]
    public Task<IActionResult> Details(Guid id, CancellationToken ct) => Execute(()=>subscriptions.GetDetailsAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Subscriptions.SubscriptionService.GetDetailsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/subscriptions/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "subscription": {
    "id": "11111111-1111-4111-8111-111111111111",
    "memberId": "11111111-1111-4111-8111-111111111111",
    "sourcePlanId": "11111111-1111-4111-8111-111111111111",
    "planName": null,
    "planType": null,
    "originBranchId": "11111111-1111-4111-8111-111111111111",
    "accessCityId": "11111111-1111-4111-8111-111111111111",
    "mode": "Unlimited",
    "startDate": "2026-10-12",
    "endDate": null,
    "status": "Active",
    "totalSessions": null,
    "remainingSessions": null,
    "version": 1,
    "employeeId": null,
    "followsSubscriptionId": null,
    "isStandaloneVisit": false
  },
  "memberName": "FAKE memberName",
  "freezeAllowance": null,
  "consumedFreezeDays": 0,
  "remainingFreezeDays": null,
  "benefits": [],
  "grants": [],
  "freezes": [],
  "holidays": []
}
```
<a id="post-api-subscriptions-id-guid-freeze"></a>
## POST /api/subscriptions/{id:guid}/freeze
Source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:17`, action `Freeze`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Subscriptions.Freeze`.
Scope/actor: apply module rules above and [guard evidence](rules/Subscriptions.md). History/management uses subscription origin branch; current entitlement resolves visited operational branch and sold City snapshot. Ownership is not subscription authority. Employee entitlement also needs Employees.Manage; holidays need global scope.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Subscriptions.FreezeRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-freezerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Subscriptions.FreezeDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-freezedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. expectedVersion is required by this body; stale EF concurrency returns 409. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/freeze"), Authorize(Policy="Permission:Subscriptions.Freeze")]
    public Task<IActionResult> Freeze(Guid id, FreezeRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.StartFreezeAsync(id,request,ct));
```
Service entry points: `CampionsSystem.BLL.Subscriptions.SubscriptionService.StartFreezeAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/subscriptions/11111111-1111-4111-8111-111111111111/freeze HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "expectedVersion": 1,
  "startDate": "2026-10-12",
  "days": 2
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "type": "Normal",
  "startDate": "2026-10-12",
  "endDate": null,
  "daysApplied": null,
  "reason": null,
  "approvedBy": "11111111-1111-4111-8111-111111111111",
  "createdAt": "2026-10-12T08:00:00Z",
  "completedAt": null,
  "daysAlreadyCompensated": 0,
  "previousSubscriptionEndDate": null,
  "newSubscriptionEndDate": null
}
```
<a id="post-api-subscriptions-id-guid-freezes-freezeid-guid-complete"></a>
## POST /api/subscriptions/{id:guid}/freezes/{freezeId:guid}/complete
Source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:20`, action `Complete`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Subscriptions.Freeze`.
Scope/actor: apply module rules above and [guard evidence](rules/Subscriptions.md). History/management uses subscription origin branch; current entitlement resolves visited operational branch and sold City snapshot. Ownership is not subscription authority. Employee entitlement also needs Employees.Manage; holidays need global scope.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `freezeId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Subscriptions.CompleteFreezeRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-completefreezerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Subscriptions.FreezeDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-freezedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. expectedVersion is required by this body; stale EF concurrency returns 409. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/freezes/{freezeId:guid}/complete"), Authorize(Policy="Permission:Subscriptions.Freeze")]
    public Task<IActionResult> Complete(Guid id, Guid freezeId, CompleteFreezeRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.CompleteFreezeAsync(id,freezeId,request,ct));
```
Service entry points: `CampionsSystem.BLL.Subscriptions.SubscriptionService.CompleteFreezeAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/subscriptions/11111111-1111-4111-8111-111111111111/freezes/11111111-1111-4111-8111-111111111111/complete HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "expectedVersion": 1
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "type": "Normal",
  "startDate": "2026-10-12",
  "endDate": null,
  "daysApplied": null,
  "reason": null,
  "approvedBy": "11111111-1111-4111-8111-111111111111",
  "createdAt": "2026-10-12T08:00:00Z",
  "completedAt": null,
  "daysAlreadyCompensated": 0,
  "previousSubscriptionEndDate": null,
  "newSubscriptionEndDate": null
}
```
<a id="post-api-subscriptions-id-guid-custom-extension"></a>
## POST /api/subscriptions/{id:guid}/custom-extension
Source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:23`, action `Extend`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Subscriptions.Extend`.
Scope/actor: apply module rules above and [guard evidence](rules/Subscriptions.md). History/management uses subscription origin branch; current entitlement resolves visited operational branch and sold City snapshot. Ownership is not subscription authority. Employee entitlement also needs Employees.Manage; holidays need global scope.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Subscriptions.CustomExtensionRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-customextensionrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Subscriptions.FreezeDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-freezedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. expectedVersion is required by this body; stale EF concurrency returns 409. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/custom-extension"), Authorize(Policy="Permission:Subscriptions.Extend")]
    public Task<IActionResult> Extend(Guid id, CustomExtensionRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.GrantCustomExtensionAsync(id,request,ct));
```
Service entry points: `CampionsSystem.BLL.Subscriptions.SubscriptionService.GrantCustomExtensionAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/subscriptions/11111111-1111-4111-8111-111111111111/custom-extension HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "expectedVersion": 1,
  "days": 2,
  "reason": "FAKE medical extension"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "type": "Normal",
  "startDate": "2026-10-12",
  "endDate": null,
  "daysApplied": null,
  "reason": null,
  "approvedBy": "11111111-1111-4111-8111-111111111111",
  "createdAt": "2026-10-12T08:00:00Z",
  "completedAt": null,
  "daysAlreadyCompensated": 0,
  "previousSubscriptionEndDate": null,
  "newSubscriptionEndDate": null
}
```
<a id="post-api-employees-id-guid-membership-entitlement"></a>
## POST /api/employees/{id:guid}/membership-entitlement
Source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:26`, action `Employee`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Subscriptions.Manage`, `Permission:Employees.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Subscriptions.md). History/management uses subscription origin branch; current entitlement resolves visited operational branch and sold City snapshot. Ownership is not subscription authority. Employee entitlement also needs Employees.Manage; holidays need global scope.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Subscriptions.EmployeeEntitlementRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-employeeentitlementrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Subscriptions.SubscriptionGrantResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-subscriptiongrantresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("/api/employees/{id:guid}/membership-entitlement"), Authorize(Policy="Permission:Subscriptions.Manage"), Authorize(Policy="Permission:Employees.Manage")]
    public Task<IActionResult> Employee(Guid id, EmployeeEntitlementRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.StartEmployeeEntitlementAsync(id,request,ct));
```
Service entry points: `CampionsSystem.BLL.Subscriptions.SubscriptionService.StartEmployeeEntitlementAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/employees/11111111-1111-4111-8111-111111111111/membership-entitlement HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "memberId": "11111111-1111-4111-8111-111111111111",
  "planId": "11111111-1111-4111-8111-111111111111",
  "operationId": "11111111-1111-4111-8111-111111111111"
}
```
Schema-shaped response (not an executed response):
```json
{
  "subscriptionId": "11111111-1111-4111-8111-111111111111",
  "grantId": "11111111-1111-4111-8111-111111111111",
  "scheduled": false
}
```
<a id="post-api-subscriptions-official-holidays"></a>
## POST /api/subscriptions/official-holidays
Source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:29`, action `Holiday`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Subscriptions.Extend`, `Permission:Branches.AccessAll`.
Scope/actor: apply module rules above and [guard evidence](rules/Subscriptions.md). History/management uses subscription origin branch; current entitlement resolves visited operational branch and sold City snapshot. Ownership is not subscription authority. Employee entitlement also needs Employees.Manage; holidays need global scope.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Subscriptions.OfficialHolidayRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-officialholidayrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Subscriptions.HolidayApplicationResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-holidayapplicationresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("official-holidays"), Authorize(Policy="Permission:Subscriptions.Extend"), Authorize(Policy="Permission:Branches.AccessAll")]
    public Task<IActionResult> Holiday(OfficialHolidayRequest request, CancellationToken ct)
        => Execute(()=>subscriptions.CreateOfficialHolidayAsync(request,ct));
```
Service entry points: `CampionsSystem.BLL.Subscriptions.SubscriptionService.CreateOfficialHolidayAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/subscriptions/official-holidays HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "startDate": "2026-10-12",
  "endDate": "2026-10-12",
  "extendSubscriptions": false,
  "reason": "FAKE reason"
}
```
Schema-shaped response (not an executed response):
```json
{
  "closureId": "11111111-1111-4111-8111-111111111111",
  "protectedDays": 0,
  "extendedDays": 0
}
```
<a id="post-api-subscriptions-official-holidays-id-guid-apply"></a>
## POST /api/subscriptions/official-holidays/{id:guid}/apply
Source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:32`, action `ApplyHoliday`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Subscriptions.Extend`, `Permission:Branches.AccessAll`.
Scope/actor: apply module rules above and [guard evidence](rules/Subscriptions.md). History/management uses subscription origin branch; current entitlement resolves visited operational branch and sold City snapshot. Ownership is not subscription authority. Employee entitlement also needs Employees.Manage; holidays need global scope.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Subscriptions.HolidayApplicationResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-holidayapplicationresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("official-holidays/{id:guid}/apply"), Authorize(Policy="Permission:Subscriptions.Extend"), Authorize(Policy="Permission:Branches.AccessAll")]
    public Task<IActionResult> ApplyHoliday(Guid id, CancellationToken ct) => Execute(()=>subscriptions.ApplyOfficialHolidayAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Subscriptions.SubscriptionService.ApplyOfficialHolidayAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/subscriptions/official-holidays/11111111-1111-4111-8111-111111111111/apply HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "closureId": "11111111-1111-4111-8111-111111111111",
  "protectedDays": 0,
  "extendedDays": 0
}
```
