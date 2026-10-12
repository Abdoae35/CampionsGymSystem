# Offers endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Sales.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline and matching local master/live remote master at audit. Runtime operations were not exercised here. 

**Scope/actor rules:** Plan/Sale operational branch and plan/offer/audience/Member qualification. Global offer/cross-branch administration needs effective scope; Member audiences use registration branch and Members.Manage.

<a id="get-api-offers"></a>
## GET /api/offers
Source: `CampionsSystem.API/Controllers/OffersController.cs:8`, action `Eligible`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Plan/Sale operational branch and plan/offer/audience/Member qualification. Global offer/cross-branch administration needs effective scope; Member audiences use registration branch and Members.Manage.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `memberId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `membershipPlanId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Sales.OfferDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-sales-offerdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet,Authorize(Policy="Permission:Payments.Create")]
    public Task<IActionResult> Eligible([FromQuery]Guid memberId,[FromQuery]Guid membershipPlanId,[FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>offers.GetEligibleAsync(memberId,membershipPlanId,branchId,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.OfferService.GetEligibleAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/offers HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="get-api-offers-quote"></a>
## GET /api/offers/quote
Source: `CampionsSystem.API/Controllers/OffersController.cs:10`, action `Quote`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Plan/Sale operational branch and plan/offer/audience/Member qualification. Global offer/cross-branch administration needs effective scope; Member audiences use registration branch and Members.Manage.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `memberId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `membershipPlanId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `offerId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Sales.PricingResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-pricingresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("quote"),Authorize(Policy="Permission:Payments.Create")]
    public Task<IActionResult> Quote([FromQuery]Guid memberId,[FromQuery]Guid membershipPlanId,[FromQuery]Guid? offerId,[FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>offers.QuoteAsync(memberId,membershipPlanId,offerId,branchId,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.OfferService.QuoteAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/offers/quote HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "originalPrice": 1200,
  "discount": 0,
  "finalPrice": 1200,
  "extraDays": 0,
  "offerId": null,
  "offerName": null,
  "offerType": null,
  "offerValue": null,
  "eligibilityMode": null,
  "audienceCategoryId": null
}
```
<a id="get-api-offers-management"></a>
## GET /api/offers/management
Source: `CampionsSystem.API/Controllers/OffersController.cs:12`, action `Management`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Offers.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Plan/Sale operational branch and plan/offer/audience/Member qualification. Global offer/cross-branch administration needs effective scope; Member audiences use registration branch and Members.Manage.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Sales.OfferHistory](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-offerhistory). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("management"),Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Management([FromQuery]Guid? branchId,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>offers.GetManagementHistoryAsync(branchId,page,pageSize,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.OfferService.GetManagementHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/offers/management?page=1&pageSize=25 HTTP/1.1
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
<a id="get-api-offers-id-guid"></a>
## GET /api/offers/{id:guid}
Source: `CampionsSystem.API/Controllers/OffersController.cs:14`, action `Details`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Offers.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Plan/Sale operational branch and plan/offer/audience/Member qualification. Global offer/cross-branch administration needs effective scope; Member audiences use registration branch and Members.Manage.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Sales.OfferDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-offerdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"),Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Details(Guid id,CancellationToken ct)=>Execute(()=>offers.GetAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.OfferService.GetAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/offers/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": null,
  "name": "FAKE name",
  "type": "FixedDiscount",
  "value": 0,
  "startDate": null,
  "endDate": null,
  "isPermanent": false,
  "isActive": true,
  "membershipPlanIds": [],
  "eligibilityMode": "General",
  "audienceCategoryId": null
}
```
<a id="post-api-offers"></a>
## POST /api/offers
Source: `CampionsSystem.API/Controllers/OffersController.cs:16`, action `Create`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Offers.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Plan/Sale operational branch and plan/offer/audience/Member qualification. Global offer/cross-branch administration needs effective scope; Member audiences use registration branch and Members.Manage.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Sales.OfferRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-offerrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Sales.OfferDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-offerdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Create(OfferRequest r,CancellationToken ct)=>Execute(()=>offers.CreateAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.OfferService.CreateAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/offers HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "name": "FAKE General Discount",
  "type": 1,
  "value": 100,
  "membershipPlanIds": [
    "11111111-1111-4111-8111-111111111111"
  ],
  "eligibilityMode": 0,
  "branchId": "11111111-1111-4111-8111-111111111111",
  "isPermanent": true,
  "isActive": true
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": null,
  "name": "FAKE name",
  "type": "FixedDiscount",
  "value": 0,
  "startDate": null,
  "endDate": null,
  "isPermanent": false,
  "isActive": true,
  "membershipPlanIds": [],
  "eligibilityMode": "General",
  "audienceCategoryId": null
}
```
<a id="put-api-offers-id-guid"></a>
## PUT /api/offers/{id:guid}
Source: `CampionsSystem.API/Controllers/OffersController.cs:18`, action `Update`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Offers.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Plan/Sale operational branch and plan/offer/audience/Member qualification. Global offer/cross-branch administration needs effective scope; Member audiences use registration branch and Members.Manage.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Sales.OfferRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-offerrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Sales.OfferDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-offerdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("{id:guid}"),Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Update(Guid id,OfferRequest r,CancellationToken ct)=>Execute(()=>offers.UpdateAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.OfferService.UpdateAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/offers/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "name": "FAKE General Discount",
  "type": 1,
  "value": 100,
  "membershipPlanIds": [
    "11111111-1111-4111-8111-111111111111"
  ],
  "eligibilityMode": 0,
  "branchId": "11111111-1111-4111-8111-111111111111",
  "isPermanent": true,
  "isActive": true
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": null,
  "name": "FAKE name",
  "type": "FixedDiscount",
  "value": 0,
  "startDate": null,
  "endDate": null,
  "isPermanent": false,
  "isActive": true,
  "membershipPlanIds": [],
  "eligibilityMode": "General",
  "audienceCategoryId": null
}
```
<a id="post-api-offers-id-guid-member-entitlements"></a>
## POST /api/offers/{id:guid}/member-entitlements
Source: `CampionsSystem.API/Controllers/OffersController.cs:20`, action `Grant`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Offers.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Plan/Sale operational branch and plan/offer/audience/Member qualification. Global offer/cross-branch administration needs effective scope; Member audiences use registration branch and Members.Manage.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Sales.GrantMemberOfferRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-grantmemberofferrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Sales.MemberOfferGrantDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-memberoffergrantdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/member-entitlements"),Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Grant(Guid id,GrantMemberOfferRequest r,CancellationToken ct)=>Execute(()=>offers.GrantMemberAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.OfferService.GrantMemberAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/offers/11111111-1111-4111-8111-111111111111/member-entitlements HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "memberId": "11111111-1111-4111-8111-111111111111",
  "startDate": null,
  "endDate": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "memberId": "11111111-1111-4111-8111-111111111111",
  "offerId": "11111111-1111-4111-8111-111111111111",
  "startDate": null,
  "endDate": null,
  "revokedAt": null
}
```
<a id="delete-api-offers-id-guid-member-entitlements-grantid-guid"></a>
## DELETE /api/offers/{id:guid}/member-entitlements/{grantId:guid}
Source: `CampionsSystem.API/Controllers/OffersController.cs:22`, action `Revoke`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Offers.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Plan/Sale operational branch and plan/offer/audience/Member qualification. Global offer/cross-branch administration needs effective scope; Member audiences use registration branch and Members.Manage.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `grantId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Sales.MemberOfferGrantDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-memberoffergrantdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpDelete("{id:guid}/member-entitlements/{grantId:guid}"),Authorize(Policy="Permission:Offers.Manage")]
    public Task<IActionResult> Revoke(Guid id,Guid grantId,CancellationToken ct)=>Execute(()=>offers.RevokeMemberAsync(id,grantId,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.OfferService.RevokeMemberAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
DELETE /api/offers/11111111-1111-4111-8111-111111111111/member-entitlements/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "memberId": "11111111-1111-4111-8111-111111111111",
  "offerId": "11111111-1111-4111-8111-111111111111",
  "startDate": null,
  "endDate": null,
  "revokedAt": null
}
```
<a id="put-api-members-memberid-guid-audience-categories"></a>
## PUT /api/members/{memberId:guid}/audience-categories
Source: `CampionsSystem.API/Controllers/OffersController.cs:24`, action `Audiences`.
Status: **VERIFIED FROM CODE / implemented at baseline**, not branch-only new Coaching.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Members.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Plan/Sale operational branch and plan/offer/audience/Member qualification. Global offer/cross-branch administration needs effective scope; Member audiences use registration branch and Members.Manage.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `memberId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Sales.SetMemberAudienceRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-setmemberaudiencerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [IReadOnlyList<Sales.MemberAudienceDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-sales-memberaudiencedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("/api/members/{memberId:guid}/audience-categories"),Authorize(Policy="Permission:Members.Manage")]
    public Task<IActionResult> Audiences(Guid memberId,SetMemberAudienceRequest r,CancellationToken ct)=>Execute(()=>offers.SetMemberAudiencesAsync(memberId,r,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.OfferService.SetMemberAudiencesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/members/11111111-1111-4111-8111-111111111111/audience-categories HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "audienceCategoryIds": []
}
```
Schema-shaped response (not an executed response):
```json
[]
```
