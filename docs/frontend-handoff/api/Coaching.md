# Coaching endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Coaching.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE at completed backend a2a0c56, inspected at 476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80. Runtime business operations were not exercised by this documentation task.

**Scope/actor rules:** Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.

<a id="get-api-coaching-private-packages"></a>
## GET /api/coaching/private/packages
Source: `CampionsSystem.API/Controllers/CoachingController.cs:12`, action `PrivatePackages`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.CoachingPackageDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-coachingpackagedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("private/packages"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> PrivatePackages(Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.PackagesAsync(branchId, false, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.PackagesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/private/packages?page=1&pageSize=25&branchId=99999999-9999-4999-8999-999999999999 HTTP/1.1
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
<a id="post-api-coaching-private-packages"></a>
## POST /api/coaching/private/packages
Source: `CampionsSystem.API/Controllers/CoachingController.cs:14`, action `CreatePrivatePackage`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Coaching.CoachingPackageRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpackagerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.CoachingPackageDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpackagedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("private/packages"), Authorize(Policy = "Permission:Private.Manage")]
    public Task<IActionResult> CreatePrivatePackage(CoachingPackageRequest r, CancellationToken ct) => Execute(() => coaching.SavePackageAsync(null, r, false, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.SavePackageAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/private/packages HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE Sessions Package",
  "sessionCount": 10,
  "durationDays": 30,
  "price": 1200,
  "isActive": true
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "sessionCount": 1,
  "durationDays": 30,
  "price": 1200,
  "isActive": true
}
```
<a id="put-api-coaching-private-packages-id-guid"></a>
## PUT /api/coaching/private/packages/{id:guid}
Source: `CampionsSystem.API/Controllers/CoachingController.cs:16`, action `UpdatePrivatePackage`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Coaching.CoachingPackageRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpackagerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.CoachingPackageDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpackagedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("private/packages/{id:guid}"), Authorize(Policy = "Permission:Private.Manage")]
    public Task<IActionResult> UpdatePrivatePackage(Guid id, CoachingPackageRequest r, CancellationToken ct) => Execute(() => coaching.SavePackageAsync(id, r, false, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.SavePackageAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/coaching/private/packages/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE Sessions Package",
  "sessionCount": 10,
  "durationDays": 30,
  "price": 1200,
  "isActive": true
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "sessionCount": 1,
  "durationDays": 30,
  "price": 1200,
  "isActive": true
}
```
<a id="post-api-coaching-private-purchases"></a>
## POST /api/coaching/private/purchases
Source: `CampionsSystem.API/Controllers/CoachingController.cs:18`, action `PurchasePrivate`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.Manage`, `Permission:Payments.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Coaching.CoachingPurchaseRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpurchaserequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.CoachingPurchaseReceipt](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpurchasereceipt). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("private/purchases"), Authorize(Policy = "Permission:Private.Manage"), Authorize(Policy = "Permission:Payments.Create")]
    public Task<IActionResult> PurchasePrivate(CoachingPurchaseRequest r, CancellationToken ct) => Execute(() => sales.PurchaseCoachingAsync(r, false, ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.PurchaseCoachingAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/private/purchases HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "memberId": "11111111-1111-4111-8111-111111111111",
  "packageId": "11111111-1111-4111-8111-111111111111",
  "paymentParts": [
    {
      "financialAccountId": "11111111-1111-4111-8111-111111111111",
      "amount": 1200,
      "providerReference": null
    }
  ],
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "purchaseMode": 0,
  "installments": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "subscriptionId": "11111111-1111-4111-8111-111111111111",
  "saleId": "11111111-1111-4111-8111-111111111111",
  "paymentIds": []
}
```
<a id="get-api-coaching-private-members"></a>
## GET /api/coaching/private/members
Source: `CampionsSystem.API/Controllers/CoachingController.cs:20`, action `PrivateMembers`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `search` / query | false | `{"type":"string"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.CoachingMemberResult>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-coachingmemberresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("private/members"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> PrivateMembers(Guid branchId, string search, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.SearchAsync(branchId, false, search, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.SearchAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/private/members?page=1&pageSize=25&branchId=99999999-9999-4999-8999-999999999999&search=FAKE HTTP/1.1
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
<a id="get-api-coaching-private-subscriptions"></a>
## GET /api/coaching/private/subscriptions
Source: `CampionsSystem.API/Controllers/CoachingController.cs:22`, action `PrivateSubscriptions`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `memberId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.PrivateDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-privatedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("private/subscriptions"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> PrivateSubscriptions(Guid branchId, Guid memberId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.PrivateSubscriptionsAsync(branchId, memberId, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.PrivateSubscriptionsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/private/subscriptions?page=1&pageSize=25&branchId=99999999-9999-4999-8999-999999999999&memberId=99999999-9999-4999-8999-999999999999 HTTP/1.1
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
<a id="get-api-coaching-private-subscriptions-id-guid"></a>
## GET /api/coaching/private/subscriptions/{id:guid}
Source: `CampionsSystem.API/Controllers/CoachingController.cs:24`, action `PrivateSubscription`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.PrivateDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-privatedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("private/subscriptions/{id:guid}"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> PrivateSubscription(Guid id, CancellationToken ct) => Execute(() => coaching.PrivateDetailsAsync(id, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.PrivateDetailsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/private/subscriptions/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "memberId": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "startDate": "2026-10-12",
  "endDate": "2026-10-12",
  "purchasedSessions": 10,
  "usedSessions": 0,
  "remainingSessions": 10,
  "version": 1,
  "cancelled": false,
  "packageName": null,
  "price": 1200,
  "unitSessionValue": 0,
  "remainderValue": 0,
  "originalCoachEmployeeId": null,
  "assignments": []
}
```
<a id="post-api-coaching-private-subscriptions-id-guid-sessions"></a>
## POST /api/coaching/private/subscriptions/{id:guid}/sessions

**Assigned coach must equal actor. Existing same-branch/day Reception Gym entry and valid regular membership required; no BorrowedGym/Trial/standalone bypass.**
Source: `CampionsSystem.API/Controllers/CoachingController.cs:26`, action `RegisterPrivate`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.RegisterSession`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Coaching.PrivateAttendanceRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-privateattendancerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.PrivateAttendanceDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-privateattendancedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("private/subscriptions/{id:guid}/sessions"), Authorize(Policy = "Permission:Private.RegisterSession")]
    public Task<IActionResult> RegisterPrivate(Guid id, PrivateAttendanceRequest r, CancellationToken ct) => Execute(() => coaching.RegisterPrivateAsync(id, r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.RegisterPrivateAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/private/subscriptions/11111111-1111-4111-8111-111111111111/sessions HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "gymCheckInId": "11111111-1111-4111-8111-111111111111",
  "notes": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "subscriptionId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "gymCheckInId": "11111111-1111-4111-8111-111111111111",
  "businessDate": null,
  "sessionAt": "2026-10-12T08:00:00Z",
  "sessionNumber": null,
  "sessionValue": null,
  "transferredPackage": null
}
```
<a id="get-api-coaching-private-subscriptions-id-guid-sessions"></a>
## GET /api/coaching/private/subscriptions/{id:guid}/sessions
Source: `CampionsSystem.API/Controllers/CoachingController.cs:28`, action `PrivateSessions`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.PrivateAttendanceDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-privateattendancedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("private/subscriptions/{id:guid}/sessions"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> PrivateSessions(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.PrivateHistoryAsync(id, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.PrivateHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/private/subscriptions/11111111-1111-4111-8111-111111111111/sessions?page=1&pageSize=25 HTTP/1.1
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
<a id="post-api-coaching-private-subscriptions-id-guid-transfers"></a>
## POST /api/coaching/private/subscriptions/{id:guid}/transfers
Source: `CampionsSystem.API/Controllers/CoachingController.cs:30`, action `Transfer`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.Transfer`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Coaching.PrivateTransferRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-privatetransferrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.PrivateAssignmentDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-privateassignmentdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. expectedVersion is required by this body; stale EF concurrency returns 409. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("private/subscriptions/{id:guid}/transfers"), Authorize(Policy = "Permission:Private.Transfer")]
    public Task<IActionResult> Transfer(Guid id, PrivateTransferRequest r, CancellationToken ct) => Execute(() => coaching.TransferAsync(id, r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.TransferAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/private/subscriptions/11111111-1111-4111-8111-111111111111/transfers HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "expectedVersion": 1,
  "reason": "FAKE reason"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "assignedAt": "2026-10-12T08:00:00Z",
  "endedAt": null,
  "reason": null
}
```
<a id="post-api-coaching-private-subscriptions-id-guid-expiry-settlements"></a>
## POST /api/coaching/private/subscriptions/{id:guid}/expiry-settlements
Source: `CampionsSystem.API/Controllers/CoachingController.cs:32`, action `SettleExpiry`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.SettleExpiry`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Coaching.PrivateExpiryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-privateexpiryrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.PrivateExpiryDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-privateexpirydetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("private/subscriptions/{id:guid}/expiry-settlements"), Authorize(Policy = "Permission:Private.SettleExpiry")]
    public Task<IActionResult> SettleExpiry(Guid id, PrivateExpiryRequest r, CancellationToken ct) => Execute(() => coaching.SettleExpiryAsync(id, r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.SettleExpiryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/private/subscriptions/11111111-1111-4111-8111-111111111111/expiry-settlements HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "commissionPeriodId": "11111111-1111-4111-8111-111111111111",
  "reason": "FAKE reason"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "subscriptionId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "unusedSessions": 0,
  "eligibleValue": 0,
  "transferredPackage": null,
  "businessDate": null,
  "settledAt": "2026-10-12T08:00:00Z",
  "reason": null
}
```
<a id="get-api-coaching-private-subscriptions-id-guid-expiry-settlements"></a>
## GET /api/coaching/private/subscriptions/{id:guid}/expiry-settlements
Source: `CampionsSystem.API/Controllers/CoachingController.cs:34`, action `ExpiryHistory`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Private.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.PrivateExpiryDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-privateexpirydetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("private/subscriptions/{id:guid}/expiry-settlements"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> ExpiryHistory(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.ExpiryHistoryAsync(id, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.ExpiryHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/private/subscriptions/11111111-1111-4111-8111-111111111111/expiry-settlements?page=1&pageSize=25 HTTP/1.1
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
<a id="get-api-coaching-classes-packages"></a>
## GET /api/coaching/classes/packages
Source: `CampionsSystem.API/Controllers/CoachingController.cs:37`, action `ClassesPackages`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.CoachingPackageDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-coachingpackagedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("classes/packages"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassesPackages(Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.PackagesAsync(branchId, true, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.PackagesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/classes/packages?page=1&pageSize=25&branchId=99999999-9999-4999-8999-999999999999 HTTP/1.1
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
<a id="post-api-coaching-classes-packages"></a>
## POST /api/coaching/classes/packages
Source: `CampionsSystem.API/Controllers/CoachingController.cs:39`, action `CreateClassesPackage`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Coaching.CoachingPackageRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpackagerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.CoachingPackageDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpackagedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("classes/packages"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> CreateClassesPackage(CoachingPackageRequest r, CancellationToken ct) => Execute(() => coaching.SavePackageAsync(null, r, true, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.SavePackageAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/classes/packages HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE Sessions Package",
  "sessionCount": 10,
  "durationDays": 30,
  "price": 1200,
  "isActive": true
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "sessionCount": 1,
  "durationDays": 30,
  "price": 1200,
  "isActive": true
}
```
<a id="put-api-coaching-classes-packages-id-guid"></a>
## PUT /api/coaching/classes/packages/{id:guid}
Source: `CampionsSystem.API/Controllers/CoachingController.cs:41`, action `UpdateClassesPackage`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Coaching.CoachingPackageRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpackagerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.CoachingPackageDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpackagedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("classes/packages/{id:guid}"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> UpdateClassesPackage(Guid id, CoachingPackageRequest r, CancellationToken ct) => Execute(() => coaching.SavePackageAsync(id, r, true, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.SavePackageAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/coaching/classes/packages/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE Sessions Package",
  "sessionCount": 10,
  "durationDays": 30,
  "price": 1200,
  "isActive": true
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "sessionCount": 1,
  "durationDays": 30,
  "price": 1200,
  "isActive": true
}
```
<a id="post-api-coaching-classes-purchases"></a>
## POST /api/coaching/classes/purchases
Source: `CampionsSystem.API/Controllers/CoachingController.cs:43`, action `PurchaseClasses`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.Manage`, `Permission:Payments.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Coaching.CoachingPurchaseRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpurchaserequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.CoachingPurchaseReceipt](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpurchasereceipt). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("classes/purchases"), Authorize(Policy = "Permission:Classes.Manage"), Authorize(Policy = "Permission:Payments.Create")]
    public Task<IActionResult> PurchaseClasses(CoachingPurchaseRequest r, CancellationToken ct) => Execute(() => sales.PurchaseCoachingAsync(r, true, ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.PurchaseCoachingAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/classes/purchases HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "memberId": "11111111-1111-4111-8111-111111111111",
  "packageId": "11111111-1111-4111-8111-111111111111",
  "paymentParts": [
    {
      "financialAccountId": "11111111-1111-4111-8111-111111111111",
      "amount": 1200,
      "providerReference": null
    }
  ],
  "coachEmployeeId": null,
  "purchaseMode": 0,
  "installments": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "subscriptionId": "11111111-1111-4111-8111-111111111111",
  "saleId": "11111111-1111-4111-8111-111111111111",
  "paymentIds": []
}
```
<a id="get-api-coaching-classes-members"></a>
## GET /api/coaching/classes/members
Source: `CampionsSystem.API/Controllers/CoachingController.cs:45`, action `ClassMembers`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `search` / query | false | `{"type":"string"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.CoachingMemberResult>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-coachingmemberresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("classes/members"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassMembers(Guid branchId, string search, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.SearchAsync(branchId, true, search, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.SearchAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/classes/members?page=1&pageSize=25&branchId=99999999-9999-4999-8999-999999999999&search=FAKE HTTP/1.1
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
<a id="get-api-coaching-classes-subscriptions"></a>
## GET /api/coaching/classes/subscriptions
Source: `CampionsSystem.API/Controllers/CoachingController.cs:47`, action `ClassSubscriptions`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `memberId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.ClassesSubscriptionDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-classessubscriptiondetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("classes/subscriptions"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassSubscriptions(Guid branchId, Guid memberId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.ClassesSubscriptionsAsync(branchId, memberId, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.ClassesSubscriptionsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/classes/subscriptions?page=1&pageSize=25&branchId=99999999-9999-4999-8999-999999999999&memberId=99999999-9999-4999-8999-999999999999 HTTP/1.1
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
<a id="get-api-coaching-classes-types"></a>
## GET /api/coaching/classes/types
Source: `CampionsSystem.API/Controllers/CoachingController.cs:51`, action `ClassTypes`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.ClassTypeDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-classtypedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("classes/types"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassTypes(Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.ClassTypesAsync(branchId, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.ClassTypesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/classes/types?page=1&pageSize=25&branchId=99999999-9999-4999-8999-999999999999 HTTP/1.1
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
<a id="post-api-coaching-classes-types"></a>
## POST /api/coaching/classes/types
Source: `CampionsSystem.API/Controllers/CoachingController.cs:53`, action `CreateClassType`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Coaching.ClassTypeRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-classtyperequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.ClassTypeDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-classtypedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("classes/types"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> CreateClassType(ClassTypeRequest r, CancellationToken ct) => Execute(() => coaching.SaveClassTypeAsync(null, r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.SaveClassTypeAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/classes/types HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "isActive": true,
  "description": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "isActive": true,
  "description": null
}
```
<a id="put-api-coaching-classes-types-id-guid"></a>
## PUT /api/coaching/classes/types/{id:guid}
Source: `CampionsSystem.API/Controllers/CoachingController.cs:55`, action `UpdateClassType`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Coaching.ClassTypeRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-classtyperequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.ClassTypeDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-classtypedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("classes/types/{id:guid}"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> UpdateClassType(Guid id, ClassTypeRequest r, CancellationToken ct) => Execute(() => coaching.SaveClassTypeAsync(id, r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.SaveClassTypeAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/coaching/classes/types/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "isActive": true,
  "description": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "isActive": true,
  "description": null
}
```
<a id="get-api-coaching-classes-occurrences"></a>
## GET /api/coaching/classes/occurrences
Source: `CampionsSystem.API/Controllers/CoachingController.cs:57`, action `Occurrences`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `from` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `to` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.ClassOccurrenceDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-classoccurrencedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("classes/occurrences"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> Occurrences(Guid branchId, DateOnly from, DateOnly to, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.OccurrencesAsync(branchId, from, to, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.OccurrencesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/classes/occurrences?page=1&pageSize=25&branchId=99999999-9999-4999-8999-999999999999&from=2026-10-12&to=2026-10-12 HTTP/1.1
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
<a id="post-api-coaching-classes-occurrences"></a>
## POST /api/coaching/classes/occurrences
Source: `CampionsSystem.API/Controllers/CoachingController.cs:59`, action `CreateOccurrence`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Coaching.ClassOccurrenceRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-classoccurrencerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.ClassOccurrenceDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-classoccurrencedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("classes/occurrences"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> CreateOccurrence(ClassOccurrenceRequest r, CancellationToken ct) => Execute(() => coaching.CreateOccurrenceAsync(r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.CreateOccurrenceAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/classes/occurrences HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "classTypeId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "startsAt": "2026-10-12T18:00:00Z"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "classTypeId": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "businessDate": null,
  "startsAt": "2026-10-12T08:00:00Z",
  "endedAt": null,
  "status": "Scheduled",
  "attendance": 0,
  "version": 1
}
```
<a id="post-api-coaching-classes-occurrences-id-guid-complete"></a>
## POST /api/coaching/classes/occurrences/{id:guid}/complete

**Specific error rule: stale version is 400 BusinessRuleException in this action, not 409. Actual coach actor and same BusinessDate required.**
Source: `CampionsSystem.API/Controllers/CoachingController.cs:61`, action `CompleteOccurrence`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Coaching.CompleteClassRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-completeclassrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.ClassOccurrenceDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-classoccurrencedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. expectedVersion is required by this body; stale EF concurrency returns 409. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("classes/occurrences/{id:guid}/complete"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> CompleteOccurrence(Guid id, CompleteClassRequest r, CancellationToken ct) => Execute(() => coaching.CompleteOccurrenceAsync(id, r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.CompleteOccurrenceAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/classes/occurrences/11111111-1111-4111-8111-111111111111/complete HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "expectedVersion": 0,
  "cancelled": false
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "classTypeId": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "businessDate": null,
  "startsAt": "2026-10-12T08:00:00Z",
  "endedAt": null,
  "status": "Scheduled",
  "attendance": 0,
  "version": 1
}
```
<a id="post-api-coaching-classes-occurrences-id-guid-attendance"></a>
## POST /api/coaching/classes/occurrences/{id:guid}/attendance
Source: `CampionsSystem.API/Controllers/CoachingController.cs:63`, action `RegisterClass`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.RegisterAttendance`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Coaching.ClassAttendanceRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-classattendancerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.ClassAttendanceDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-classattendancedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("classes/occurrences/{id:guid}/attendance"), Authorize(Policy = "Permission:Classes.RegisterAttendance")]
    public Task<IActionResult> RegisterClass(Guid id, ClassAttendanceRequest r, CancellationToken ct) => Execute(() => coaching.RegisterClassAsync(id, r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.RegisterClassAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/classes/occurrences/11111111-1111-4111-8111-111111111111/attendance HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "memberId": "11111111-1111-4111-8111-111111111111",
  "classesSubscriptionId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "memberId": "11111111-1111-4111-8111-111111111111",
  "subscriptionId": "11111111-1111-4111-8111-111111111111",
  "classSessionId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "businessDate": "2026-10-12",
  "remainingSessions": 0,
  "registeredAt": "2026-10-12T08:00:00Z"
}
```
<a id="get-api-coaching-classes-occurrences-id-guid-attendance"></a>
## GET /api/coaching/classes/occurrences/{id:guid}/attendance
Source: `CampionsSystem.API/Controllers/CoachingController.cs:65`, action `ClassAttendance`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.ClassAttendanceDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-classattendancedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("classes/occurrences/{id:guid}/attendance"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassAttendance(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.ClassAttendanceHistoryAsync(id, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.ClassAttendanceHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/classes/occurrences/11111111-1111-4111-8111-111111111111/attendance?page=1&pageSize=25 HTTP/1.1
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
<a id="get-api-coaching-classes-coaches-coach-guid-statistics"></a>
## GET /api/coaching/classes/coaches/{coach:guid}/statistics
Source: `CampionsSystem.API/Controllers/CoachingController.cs:67`, action `ClassStatistics`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.ViewAttendanceReport`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `coach` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `from` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `to` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `periodId` / query | false | `{"type":"string","format":"uuid"}` | null |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.ClassCoachStatistics](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-classcoachstatistics). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("classes/coaches/{coach:guid}/statistics"), Authorize(Policy = "Permission:Classes.ViewAttendanceReport")]
    public Task<IActionResult> ClassStatistics(Guid coach, Guid branchId, DateOnly from, DateOnly to, Guid? periodId = null, CancellationToken ct = default) => Execute(() => coaching.ClassStatisticsAsync(branchId, coach, from, to, periodId, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.ClassStatisticsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/classes/coaches/11111111-1111-4111-8111-111111111111/statistics?branchId=99999999-9999-4999-8999-999999999999&from=2026-10-12&to=2026-10-12 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "completedClasses": 0,
  "totalAttendance": 0,
  "averageAttendance": 0,
  "target": null,
  "targetReached": null,
  "occurrences": [],
  "compensationStatus": "NotConfigured"
}
```
<a id="post-api-coaching-earnings-targets"></a>
## POST /api/coaching/earnings/targets
Source: `CampionsSystem.API/Controllers/CoachingController.cs:70`, action `Targets`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CoachEarnings.ManageTargets`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Coaching.CoachTargetRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachtargetrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.CoachTargetDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachtargetdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("earnings/targets"), Authorize(Policy = "Permission:CoachEarnings.ManageTargets")]
    public Task<IActionResult> Targets(CoachTargetRequest r, CancellationToken ct) => Execute(() => coaching.ConfigureTargetsAsync(r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.ConfigureTargetsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/earnings/targets HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "commissionPeriodId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "privateTarget": 30000,
  "classesAverageTarget": 10
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "periodId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "privateTarget": null,
  "classesAverageTarget": null
}
```
<a id="get-api-coaching-earnings-periods-period-guid-targets"></a>
## GET /api/coaching/earnings/periods/{period:guid}/targets
Source: `CampionsSystem.API/Controllers/CoachingController.cs:72`, action `TargetHistory`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CoachEarnings.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `period` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Coaching.CoachTargetDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-coaching-coachtargetdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("earnings/periods/{period:guid}/targets"), Authorize(Policy = "Permission:CoachEarnings.View")]
    public Task<IActionResult> TargetHistory(Guid period, CancellationToken ct) => Execute(() => coaching.TargetsAsync(period, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.TargetsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/earnings/periods/11111111-1111-4111-8111-111111111111/targets HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="get-api-coaching-earnings-periods-period-guid-progress"></a>
## GET /api/coaching/earnings/periods/{period:guid}/progress
Source: `CampionsSystem.API/Controllers/CoachingController.cs:74`, action `Progress`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CoachEarnings.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `period` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachEarningsProgress](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachearningsprogress). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("earnings/periods/{period:guid}/progress"), Authorize(Policy = "Permission:CoachEarnings.View")]
    public Task<IActionResult> Progress(Guid period, CancellationToken ct) => Execute(() => coaching.ProgressAsync(period, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.ProgressAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/earnings/periods/11111111-1111-4111-8111-111111111111/progress HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "periodId": "11111111-1111-4111-8111-111111111111",
  "finalized": true,
  "coaches": []
}
```
<a id="post-api-coaching-earnings-recognitions"></a>
## POST /api/coaching/earnings/recognitions
Source: `CampionsSystem.API/Controllers/CoachingController.cs:76`, action `Recognize`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CoachEarnings.Recognize`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Coaching.RecognizeCoachEarningsRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-recognizecoachearningsrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.CoachEarningsProgress](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachearningsprogress). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("earnings/recognitions"), Authorize(Policy = "Permission:CoachEarnings.Recognize")]
    public Task<IActionResult> Recognize(RecognizeCoachEarningsRequest r, CancellationToken ct) => Execute(() => coaching.RecognizeAsync(r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.RecognizeAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/earnings/recognitions HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "commissionPeriodId": "11111111-1111-4111-8111-111111111111"
}
```
Schema-shaped response (not an executed response):
```json
{
  "periodId": "11111111-1111-4111-8111-111111111111",
  "finalized": true,
  "coaches": []
}
```
<a id="get-api-coaching-earnings-coaches-coach-guid-balance"></a>
## GET /api/coaching/earnings/coaches/{coach:guid}/balance
Source: `CampionsSystem.API/Controllers/CoachingController.cs:78`, action `Balance`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CoachEarnings.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `coach` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachBalance](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachbalance). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("earnings/coaches/{coach:guid}/balance"), Authorize(Policy = "Permission:CoachEarnings.View")]
    public Task<IActionResult> Balance(Guid coach, Guid branchId, CancellationToken ct) => Execute(() => coaching.BalanceAsync(branchId, coach, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.BalanceAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/earnings/coaches/11111111-1111-4111-8111-111111111111/balance?branchId=99999999-9999-4999-8999-999999999999 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "earned": 0,
  "paid": 0,
  "outstanding": 0
}
```
<a id="get-api-coaching-earnings-coaches-coach-guid-history"></a>
## GET /api/coaching/earnings/coaches/{coach:guid}/history
Source: `CampionsSystem.API/Controllers/CoachingController.cs:80`, action `EarningsHistory`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CoachEarnings.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `coach` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.CoachEarningDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-coachearningdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("earnings/coaches/{coach:guid}/history"), Authorize(Policy = "Permission:CoachEarnings.View")]
    public Task<IActionResult> EarningsHistory(Guid coach, Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.EarningsHistoryAsync(branchId, coach, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.EarningsHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/earnings/coaches/11111111-1111-4111-8111-111111111111/history?page=1&pageSize=25&branchId=99999999-9999-4999-8999-999999999999 HTTP/1.1
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
<a id="post-api-coaching-earnings-payouts"></a>
## POST /api/coaching/earnings/payouts

**Additionally Branches.AccessAll, own Shift, outstanding earnings and source funds are required. Private recognized earnings only; Classes compensation disabled.**
Source: `CampionsSystem.API/Controllers/CoachingController.cs:82`, action `Payout`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CoachEarnings.Pay`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Coaching.CoachPayoutRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachpayoutrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Coaching.CoachPayoutDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachpayoutdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("earnings/payouts"), Authorize(Policy = "Permission:CoachEarnings.Pay")]
    public Task<IActionResult> Payout(CoachPayoutRequest r, CancellationToken ct) => Execute(() => coaching.PayAsync(r, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.PayAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/coaching/earnings/payouts HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "amount": 1200,
  "financialAccountId": "11111111-1111-4111-8111-111111111111",
  "expenseCategoryId": "11111111-1111-4111-8111-111111111111",
  "reason": "FAKE reason",
  "receiptReference": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "expenseId": "11111111-1111-4111-8111-111111111111",
  "accountId": "11111111-1111-4111-8111-111111111111",
  "method": "Cash",
  "amount": 1200,
  "balanceBefore": 2400,
  "balanceAfter": 1200,
  "businessDate": "2026-10-12",
  "paidAt": "2026-10-12T08:00:00Z",
  "authorizedByUserId": "11111111-1111-4111-8111-111111111111"
}
```
<a id="get-api-coaching-earnings-coaches-coach-guid-payouts"></a>
## GET /api/coaching/earnings/coaches/{coach:guid}/payouts
Source: `CampionsSystem.API/Controllers/CoachingController.cs:84`, action `PayoutHistory`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CoachEarnings.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Coaching.md). Completed backend. Explicit branch/period scope. Private session and Class attendance/completion require the actual assigned coach Employee to equal actor; global scope does not bypass CoachAsync. Owner functions mean action capability AND Branches.AccessAll, not role name. Purchases additionally require Sales.Create (BLL), Installments.Manage for mode 2, and the actor’s own Shift; payout also requires the actor’s own Shift. See Coaching evidence for all guards.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `coach` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Coaching.CoachingPage<Coaching.CoachPayoutDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-coachpayoutdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("earnings/coaches/{coach:guid}/payouts"), Authorize(Policy = "Permission:CoachEarnings.View")]
    public Task<IActionResult> PayoutHistory(Guid coach, Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.PayoutHistoryAsync(branchId, coach, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Coaching.CoachingService.PayoutHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/coaching/earnings/coaches/11111111-1111-4111-8111-111111111111/payouts?page=1&pageSize=25&branchId=99999999-9999-4999-8999-999999999999 HTTP/1.1
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


<a id="get-api-coaching-classes-members-member-guid-attendance"></a>
## GET /api/coaching/classes/members/{member:guid}/attendance

Source: `CampionsSystem.API/Controllers/CoachingController.cs:49`, action `ClassMemberAttendance`.
Bearer authentication and `Permission:Classes.View`; additional branch scope applies. No own shift, reception check-in or gym membership is required for this history read.

| Input | Type | Validation/default |
|---|---|---|
| member (path) | UUID | Guid route constraint |
| branchId (query) | UUID | Send explicit authorized branch; ordinary staff must use current branch |
| page (query) | int | >=1, default 1 |
| pageSize (query) | int | 1..100, default 25 |

No body. **200** [CoachingPage<ClassAttendanceDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-classattendancedetails): `{items,totalCount,page,pageSize}`. Fields of each attendance are `id,memberId,subscriptionId,classSessionId,coachEmployeeId,businessDate,remainingSessions,registeredAt`; exact types/nullability are in the DTO reference. Historical rows ordered by registered timestamp descending then ID; remainingSessions is the registration snapshot, not current subscription balance. Nonexistent member/no history gives an empty page, without explicit member 404. Invalid paging 400; missing/denied identity or capability/scope 401/403; route constraint mismatch 404; unexpected failure 500 ProblemDetails. No operation ID/version or mutation retry behavior.

Fabricated request and possible response:

```http
GET /api/coaching/classes/members/33333333-3333-4333-8333-333333333333/attendance?branchId=99999999-9999-4999-8999-999999999999&page=1&pageSize=25 HTTP/1.1
Authorization: Bearer <TOKEN_PLACEHOLDER>
```

```json
{"items":[],"totalCount":0,"page":1,"pageSize":25}
```
