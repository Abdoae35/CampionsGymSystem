# CheckIns endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/CheckIns.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE at completed backend a2a0c56, inspected at 476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80. Runtime business operations were not exercised by this documentation task.

**Scope/actor rules:** Trusted visited operational branch + sold City snapshot. Ordinary users cannot target another branch; global target allowed with workflow capability. Exactly one identity; authoritative re-evaluation and own branch closure/date rules. Check-in does not require own Shift.

<a id="post-api-check-ins"></a>
## POST /api/check-ins
Source: `CampionsSystem.API/Controllers/CheckInsController.cs:8`, action `Create`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CheckIns.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/CheckIns.md). HTTP 200 may contain decision.allowed=false and checkIn=null. Stable operation retry returns original actual entry. Scanner=0/Manual=1; Coach=2 rejected. Borrow requires explicit confirmation.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [CheckIns.CreateCheckInRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-createcheckinrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [CheckIns.CheckInResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-checkinresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:CheckIns.Create")]
    public Task<IActionResult> Create(CreateCheckInRequest request,CancellationToken ct)=>Execute(()=>checkIns.CreateAsync(request,ct));
```
Service entry points: `CampionsSystem.BLL.CheckIns.CheckInService.CreateAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/check-ins HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "method": 1,
  "memberId": "11111111-1111-4111-8111-111111111111",
  "memberCode": null,
  "confirmBorrow": false,
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "decision": {
    "allowed": false,
    "reasonCode": "NoGymEntitlement",
    "memberId": "33333333-3333-4333-8333-333333333333",
    "memberCode": "M-FAKE-EXAMPLE",
    "memberName": "FAKE Member",
    "branchId": "11111111-1111-4111-8111-111111111111",
    "businessDate": "2026-10-12",
    "businessDayId": null,
    "subscriptionId": null,
    "subscriptionMode": null,
    "remainingSessions": null,
    "borrowAvailable": false,
    "borrowedUsed": 0,
    "borrowedRemaining": 0,
    "canActivateOnCheckIn": false
  },
  "checkIn": null
}
```
<a id="post-api-check-ins-evaluate"></a>
## POST /api/check-ins/evaluate
Source: `CampionsSystem.API/Controllers/CheckInsController.cs:10`, action `Evaluate`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CheckIns.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/CheckIns.md). Read-only HTTP 200 decision denial; preview is neither reservation nor authorization token. Creation rechecks.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [CheckIns.AccessRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-accessrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [CheckIns.AccessDecision](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-accessdecision). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("evaluate"),Authorize(Policy="Permission:CheckIns.Create")]
    public Task<IActionResult> Evaluate(AccessRequest request,CancellationToken ct)=>Execute(()=>checkIns.EvaluateAsync(request,ct));
```
Service entry points: `CampionsSystem.BLL.CheckIns.CheckInService.EvaluateAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/check-ins/evaluate HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "memberId": "11111111-1111-4111-8111-111111111111",
  "memberCode": null,
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "allowed": false,
  "reasonCode": "NoGymEntitlement",
  "memberId": "33333333-3333-4333-8333-333333333333",
  "memberCode": "M-FAKE-EXAMPLE",
  "memberName": "FAKE Member",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "businessDate": "2026-10-12",
  "businessDayId": null,
  "subscriptionId": null,
  "subscriptionMode": null,
  "remainingSessions": null,
  "borrowAvailable": false,
  "borrowedUsed": 0,
  "borrowedRemaining": 0,
  "canActivateOnCheckIn": false
}
```
<a id="get-api-check-ins"></a>
## GET /api/check-ins
Source: `CampionsSystem.API/Controllers/CheckInsController.cs:12`, action `History`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CheckIns.View`.
Scope/actor: apply module rules above and [guard evidence](rules/CheckIns.md). Trusted visited operational branch + sold City snapshot. Ordinary users cannot target another branch; global target allowed with workflow capability. Exactly one identity; authoritative re-evaluation and own branch closure/date rules. Check-in does not require own Shift.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `MemberId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `From` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `To` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `Type` / query | false | `{"$ref":"#/components/schemas/CheckInType"}` | See query DTO defaults / guard evidence |
| `Method` / query | false | `{"$ref":"#/components/schemas/CheckInMethod"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [CheckIns.CheckInHistoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-checkinhistoryrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [CheckIns.CheckInHistory](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-checkinhistory). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet,Authorize(Policy="Permission:CheckIns.View")]
    public Task<IActionResult> History([FromQuery]CheckInHistoryRequest request,CancellationToken ct)=>Execute(()=>checkIns.GetHistoryAsync(request,ct));
```
Service entry points: `CampionsSystem.BLL.CheckIns.CheckInService.GetHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/check-ins?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-check-ins-members"></a>
## GET /api/check-ins/members
Source: `CampionsSystem.API/Controllers/CheckInsController.cs:14`, action `Lookup`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CheckIns.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/CheckIns.md). Trusted visited operational branch + sold City snapshot. Ordinary users cannot target another branch; global target allowed with workflow capability. Exactly one identity; authoritative re-evaluation and own branch closure/date rules. Check-in does not require own Shift.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `Search` / query | false | `{"type":"string"}` | See query DTO defaults / guard evidence |
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [CheckIns.CheckInMemberSearchRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-checkinmembersearchrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [CheckIns.CheckInMemberMatches](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-checkinmembermatches). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("members"),Authorize(Policy="Permission:CheckIns.Create")]
    public Task<IActionResult> Lookup([FromQuery]CheckInMemberSearchRequest request,CancellationToken ct)=>Execute(()=>checkIns.FindMembersAsync(request,ct));
```
Service entry points: `CampionsSystem.BLL.CheckIns.CheckInService.FindMembersAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/check-ins/members?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-members-memberid-guid-check-ins-current-business-day"></a>
## GET /api/members/{memberId:guid}/check-ins/current-business-day
Source: `CampionsSystem.API/Controllers/CheckInsController.cs:16`, action `Today`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CheckIns.View`.
Scope/actor: apply module rules above and [guard evidence](rules/CheckIns.md). Trusted visited operational branch + sold City snapshot. Ordinary users cannot target another branch; global target allowed with workflow capability. Exactly one identity; authoritative re-evaluation and own branch closure/date rules. Check-in does not require own Shift.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `memberId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [CheckIns.CheckInHistory](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-checkinhistory). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("/api/members/{memberId:guid}/check-ins/current-business-day"),Authorize(Policy="Permission:CheckIns.View")]
    public Task<IActionResult> Today(Guid memberId,[FromQuery]Guid? branchId,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        =>Execute(()=>checkIns.GetCurrentBusinessDayAsync(memberId,branchId,page,pageSize,ct));
```
Service entry points: `CampionsSystem.BLL.CheckIns.CheckInService.GetCurrentBusinessDayAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/members/11111111-1111-4111-8111-111111111111/check-ins/current-business-day?page=1&pageSize=25 HTTP/1.1
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
<a id="get-api-members-memberid-guid-borrowed-gym-sessions"></a>
## GET /api/members/{memberId:guid}/borrowed-gym-sessions
Source: `CampionsSystem.API/Controllers/CheckInsController.cs:19`, action `Debts`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CheckIns.View`.
Scope/actor: apply module rules above and [guard evidence](rules/CheckIns.md). Trusted visited operational branch + sold City snapshot. Ordinary users cannot target another branch; global target allowed with workflow capability. Exactly one identity; authoritative re-evaluation and own branch closure/date rules. Check-in does not require own Shift.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `memberId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `ExpiredSubscriptionId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [CheckIns.BorrowedDebtHistoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-borroweddebthistoryrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [CheckIns.BorrowedDebtHistory](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-borroweddebthistory). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("/api/members/{memberId:guid}/borrowed-gym-sessions"),Authorize(Policy="Permission:CheckIns.View")]
    public Task<IActionResult> Debts(Guid memberId,[FromQuery]BorrowedDebtHistoryRequest request,CancellationToken ct)=>Execute(()=>checkIns.GetBorrowedDebtHistoryAsync(memberId,request,ct));
```
Service entry points: `CampionsSystem.BLL.CheckIns.CheckInService.GetBorrowedDebtHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/members/11111111-1111-4111-8111-111111111111/borrowed-gym-sessions?Page=1&PageSize=25 HTTP/1.1
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
<a id="post-api-check-ins-trials"></a>
## POST /api/check-ins/trials
Source: `CampionsSystem.API/Controllers/CheckInsController.cs:21`, action `Trial`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:CheckIns.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/CheckIns.md). HTTP 200 may be denied; existing issued invitation/phone/branch, one trial identity and exact GUEST benefit unit where linked. No invitation issuance API.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [CheckIns.RedeemTrialRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-redeemtrialrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [CheckIns.TrialCheckInResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-trialcheckinresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("trials"),Authorize(Policy="Permission:CheckIns.Create")]
    public Task<IActionResult> Trial(RedeemTrialRequest request,CancellationToken ct)=>Execute(()=>checkIns.RedeemTrialAsync(request,ct));
```
Service entry points: `CampionsSystem.BLL.CheckIns.CheckInService.RedeemTrialAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/check-ins/trials HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "invitationId": "11111111-1111-4111-8111-111111111111",
  "phone": "+201000000099",
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "allowed": false,
  "reasonCode": "InvitationNotFound",
  "invitationId": "11111111-1111-4111-8111-111111111111",
  "checkIn": null
}
```
