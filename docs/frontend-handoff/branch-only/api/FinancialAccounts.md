# FinancialAccounts endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Sales.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. 

**Scope/actor rules:** Account list at resolved authorized operational branch; provisioning requires Branches.Manage AND Branches.AccessAll and creates zero-opening Cash/InstaPay account.

<a id="get-api-financial-accounts"></a>
## GET /api/financial-accounts
Source: `CampionsSystem.API/Controllers/FinancialAccountsController.cs:8`, action `Accounts`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Payments.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Account list at resolved authorized operational branch; provisioning requires Branches.Manage AND Branches.AccessAll and creates zero-opening Cash/InstaPay account.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Sales.FinancialAccountDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-sales-financialaccountdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet,Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Accounts([FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>sales.GetAccountsAsync(branchId,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.GetAccountsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/financial-accounts HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="post-api-financial-accounts"></a>
## POST /api/financial-accounts
Source: `CampionsSystem.API/Controllers/FinancialAccountsController.cs:10`, action `Create`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Branches.Manage`, `Permission:Branches.AccessAll`.
Scope/actor: apply module rules above and [guard evidence](rules/Sales.md). Account list at resolved authorized operational branch; provisioning requires Branches.Manage AND Branches.AccessAll and creates zero-opening Cash/InstaPay account.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Sales.CreateFinancialAccountRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-createfinancialaccountrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Sales.FinancialAccountDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-financialaccountdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:Branches.Manage"),Authorize(Policy="Permission:Branches.AccessAll")]
    public Task<IActionResult> Create(CreateFinancialAccountRequest r,CancellationToken ct)=>Execute(()=>sales.CreateAccountAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Sales.SalesService.CreateAccountAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/financial-accounts HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "name": "FAKE name",
  "method": 0,
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "method": "Cash",
  "isActive": true
}
```
