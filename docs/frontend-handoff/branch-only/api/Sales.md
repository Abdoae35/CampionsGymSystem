# Sales endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Sales.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. 

**Scope/actor rules:** Immutable Sale/Payment financial branch scope; normal actor uses current branch, global explicit target still requires own Employee assignment/own open Shift for finance. Original commercial owner differs from cashier/current owner/registration.

<a id="post-api-sales"></a>
## POST /api/sales
Source: `CampionsSystem.API/Controllers/SalesController.cs:8`, action `Create`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Installment additionally needs Installments.Manage. Full/standalone exact tender; reservation positive deposit; initial threshold/schedule rules. Returns SaleReceipt only; refetch details.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Sales.CreateSaleRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-createsalerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Sales.SaleReceipt](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-salereceipt). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:Payments.Create")]
    public Task<IActionResult> Create(CreateSaleRequest r,CancellationToken ct)=>Execute(()=>sales.CreateAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.CreateAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/sales HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "memberId": "11111111-1111-4111-8111-111111111111",
  "membershipPlanId": "11111111-1111-4111-8111-111111111111",
  "purchaseMode": 0,
  "activationMode": 0,
  "paymentParts": [
    {
      "financialAccountId": "11111111-1111-4111-8111-111111111111",
      "amount": 1200,
      "providerReference": null
    }
  ]
}
```
Schema-shaped response (not an executed response):
```json
{
  "saleId": "11111111-1111-4111-8111-111111111111",
  "paymentIds": []
}
```
<a id="get-api-sales"></a>
## GET /api/sales
Source: `CampionsSystem.API/Controllers/SalesController.cs:10`, action `History`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Immutable Sale/Payment financial branch scope; normal actor uses current branch, global explicit target still requires own Employee assignment/own open Shift for finance. Original commercial owner differs from cashier/current owner/registration.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `MemberId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `PurchaseMode` / query | false | `{"$ref":"#/components/schemas/PurchaseMode"}` | See query DTO defaults / guard evidence |
| `From` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `To` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Sales.SaleHistoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-salehistoryrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Sales.SaleHistory](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-salehistory). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet,Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> History([FromQuery]SaleHistoryRequest r,CancellationToken ct)=>Execute(()=>sales.GetHistoryAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.GetHistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/sales?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-sales-id-guid"></a>
## GET /api/sales/{id:guid}
Source: `CampionsSystem.API/Controllers/SalesController.cs:12`, action `Details`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Immutable Sale/Payment financial branch scope; normal actor uses current branch, global explicit target still requires own Employee assignment/own open Shift for finance. Original commercial owner differs from cashier/current owner/registration.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Sales.SaleDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-saledetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"),Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Details(Guid id,CancellationToken ct)=>Execute(()=>sales.GetDetailsAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.GetDetailsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/sales/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "sale": {
    "id": "11111111-1111-4111-8111-111111111111",
    "memberId": "11111111-1111-4111-8111-111111111111",
    "branchId": "11111111-1111-4111-8111-111111111111",
    "businessDate": null,
    "createdAt": "2026-10-12T08:00:00Z",
    "originalPrice": 1200,
    "discount": 0,
    "finalPrice": 1200,
    "collected": 0,
    "refunded": 0,
    "outstanding": 1200,
    "status": "Open",
    "purchaseMode": null,
    "activationMode": null,
    "cancelled": false,
    "version": 1
  },
  "memberName": "FAKE memberName",
  "planName": null,
  "planType": null,
  "planId": null,
  "accessCityId": null,
  "salesEmployeeId": null,
  "salesAttribution": null,
  "commissionEligible": null,
  "targetEligible": null,
  "subscriptionId": null,
  "grantId": null,
  "activationStatus": "Active",
  "requestedStartDate": null,
  "reservationDueDate": null,
  "offerName": null,
  "extraDays": 0,
  "retainedCancellationRevenue": 0,
  "payments": [],
  "refunds": [],
  "installments": [],
  "offerId": null,
  "offerType": null,
  "offerValue": null,
  "offerEligibilityMode": null,
  "offerAudienceCategoryId": null,
  "retainedRevenueClassification": "CancellationFee",
  "retainedCommissionEligible": false,
  "retainedTargetEligible": false
}
```
<a id="post-api-sales-id-guid-payments"></a>
## POST /api/sales/{id:guid}/payments
Source: `CampionsSystem.API/Controllers/SalesController.cs:14`, action `Pay`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Immutable Sale/Payment financial branch scope; normal actor uses current branch, global explicit target still requires own Employee assignment/own open Shift for finance. Original commercial owner differs from cashier/current owner/registration.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Sales.RecordPaymentRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-recordpaymentrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Sales.PaymentDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-paymentdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/payments"),Authorize(Policy="Permission:Payments.Create")]
    public Task<IActionResult> Pay(Guid id,RecordPaymentRequest r,CancellationToken ct)=>Execute(()=>sales.RecordPaymentAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.RecordPaymentAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/sales/11111111-1111-4111-8111-111111111111/payments HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "amount": 100,
  "parts": [
    {
      "financialAccountId": "11111111-1111-4111-8111-111111111111",
      "amount": 100,
      "providerReference": null
    }
  ],
  "allocations": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "operationId": null,
  "branchId": "11111111-1111-4111-8111-111111111111",
  "businessDate": null,
  "businessDayId": null,
  "paidAt": "2026-10-12T08:00:00Z",
  "amount": 1200,
  "parts": [],
  "allocations": []
}
```
<a id="get-api-sales-id-guid-payments"></a>
## GET /api/sales/{id:guid}/payments
Source: `CampionsSystem.API/Controllers/SalesController.cs:16`, action `Payments`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Immutable Sale/Payment financial branch scope; normal actor uses current branch, global explicit target still requires own Employee assignment/own open Shift for finance. Original commercial owner differs from cashier/current owner/registration.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Sales.PaymentHistory](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-paymenthistory). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/payments"),Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Payments(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>sales.GetPaymentsAsync(id,page,pageSize,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.GetPaymentsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/sales/11111111-1111-4111-8111-111111111111/payments?page=1&pageSize=25 HTTP/1.1
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
<a id="get-api-sales-id-guid-installments"></a>
## GET /api/sales/{id:guid}/installments
Source: `CampionsSystem.API/Controllers/SalesController.cs:18`, action `Installments`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Immutable Sale/Payment financial branch scope; normal actor uses current branch, global explicit target still requires own Employee assignment/own open Shift for finance. Original commercial owner differs from cashier/current owner/registration.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Sales.InstallmentDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-sales-installmentdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/installments"),Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Installments(Guid id,CancellationToken ct)=>Execute(()=>sales.GetInstallmentsAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.GetInstallmentsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/sales/11111111-1111-4111-8111-111111111111/installments HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="post-api-sales-id-guid-refunds"></a>
## POST /api/sales/{id:guid}/refunds
Source: `CampionsSystem.API/Controllers/SalesController.cs:20`, action `Refund`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.Refund`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Manual chosen amount, including zero with empty parts; positive unrefunded collection cap. Newest-first source allocation; any finalized affected commission source rejects whole operation. Own Shift.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Sales.RefundRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-refundrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Sales.RefundDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-refunddetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/refunds"),Authorize(Policy="Permission:Payments.Refund")]
    public Task<IActionResult> Refund(Guid id,RefundRequest r,CancellationToken ct)=>Execute(()=>sales.RefundAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.RefundAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/sales/11111111-1111-4111-8111-111111111111/refunds HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "amount": 100,
  "parts": [
    {
      "financialAccountId": "11111111-1111-4111-8111-111111111111",
      "amount": 100,
      "providerReference": null
    }
  ],
  "reason": "FAKE manual refund"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "operationId": null,
  "branchId": "11111111-1111-4111-8111-111111111111",
  "businessDate": null,
  "createdAt": "2026-10-12T08:00:00Z",
  "amount": 1200,
  "type": "ManagerRefund",
  "reason": "FAKE reason",
  "parts": []
}
```
<a id="get-api-sales-id-guid-refunds"></a>
## GET /api/sales/{id:guid}/refunds
Source: `CampionsSystem.API/Controllers/SalesController.cs:22`, action `Refunds`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Immutable Sale/Payment financial branch scope; normal actor uses current branch, global explicit target still requires own Employee assignment/own open Shift for finance. Original commercial owner differs from cashier/current owner/registration.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Sales.RefundHistory](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-refundhistory). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/refunds"),Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Refunds(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>sales.GetRefundsAsync(id,page,pageSize,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.GetRefundsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/sales/11111111-1111-4111-8111-111111111111/refunds?page=1&pageSize=25 HTTP/1.1
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
<a id="post-api-sales-id-guid-quick-cancel"></a>
## POST /api/sales/{id:guid}/quick-cancel
Source: `CampionsSystem.API/Controllers/SalesController.cs:24`, action `QuickCancel`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Sales.QuickCancel`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Independent Sales.QuickCancel; exact 24-hour actual window and unused grant; full reversal to original accounts. Own Shift. No client refund formula.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Sales.QuickCancelRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-quickcancelrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Sales.RefundDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-refunddetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/quick-cancel"),Authorize(Policy="Permission:Sales.QuickCancel")]
    public Task<IActionResult> QuickCancel(Guid id,QuickCancelRequest r,CancellationToken ct)=>Execute(()=>sales.QuickCancelAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.QuickCancelAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/sales/11111111-1111-4111-8111-111111111111/quick-cancel HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "reason": "FAKE reason"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "operationId": null,
  "branchId": "11111111-1111-4111-8111-111111111111",
  "businessDate": null,
  "createdAt": "2026-10-12T08:00:00Z",
  "amount": 1200,
  "type": "ManagerRefund",
  "reason": "FAKE reason",
  "parts": []
}
```
<a id="get-api-sales-branch-totals"></a>
## GET /api/sales/branch-totals
Source: `CampionsSystem.API/Controllers/SalesController.cs:26`, action `Totals`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Immutable Sale/Payment financial branch scope; normal actor uses current branch, global explicit target still requires own Employee assignment/own open Shift for finance. Original commercial owner differs from cashier/current owner/registration.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `from` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `to` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Sales.BranchFinancialTotals](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-branchfinancialtotals). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("branch-totals"),Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Totals([FromQuery]Guid? branchId,[FromQuery]DateOnly? from,[FromQuery]DateOnly? to,CancellationToken ct)=>Execute(()=>sales.GetBranchTotalsAsync(branchId,from,to,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.GetBranchTotalsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/sales/branch-totals HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "cashCollected": 0,
  "instaPayCollected": 0,
  "cashRefunded": 0,
  "instaPayRefunded": 0,
  "grossCollected": 0,
  "refunds": 0,
  "netCollected": 0
}
```
