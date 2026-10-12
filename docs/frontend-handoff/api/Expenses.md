# Expenses endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Shifts.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE at completed backend a2a0c56, inspected at 476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80. Runtime business operations were not exercised by this documentation task.

**Scope/actor rules:** Branch/account financial history; paid/return operations require own Shift. Categories are global shared catalog; category writes additionally require Branches.AccessAll. Returns use original source account and actual funds.

<a id="post-api-expenses"></a>
## POST /api/expenses
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:8`, action `Paid`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Expenses.Create`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account financial history; paid/return operations require own Shift. Categories are global shared catalog; category writes additionally require Branches.AccessAll. Returns use original source account and actual funds.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Shifts.PaidExpenseRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-paidexpenserequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.ExpenseDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-expensedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:Expenses.Create")]
    public Task<IActionResult> Paid(PaidExpenseRequest r,CancellationToken ct)=>Execute(()=>expenses.RecordPaidAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.RecordPaidAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/expenses HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "expenseCategoryId": "11111111-1111-4111-8111-111111111111",
  "financialAccountId": "11111111-1111-4111-8111-111111111111",
  "amount": 1200,
  "description": "FAKE description",
  "receiptReference": null,
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "shiftId": null,
  "recordingEmployeeId": null,
  "categoryId": "11111111-1111-4111-8111-111111111111",
  "categoryName": "FAKE categoryName",
  "accountId": "11111111-1111-4111-8111-111111111111",
  "method": 0,
  "amount": 1200,
  "returnedAmount": 0,
  "description": "FAKE description",
  "receiptReference": null,
  "occurredAt": "2026-10-12T08:00:00Z",
  "businessDate": null,
  "paidAt": null,
  "originalStatus": 0
}
```
<a id="get-api-expenses"></a>
## GET /api/expenses
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:10`, action `History`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Expenses.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account financial history; paid/return operations require own Shift. Categories are global shared catalog; category writes additionally require Branches.AccessAll. Returns use original source account and actual funds.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `AccountId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `From` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `To` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Shifts.VaultHistoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-vaulthistoryrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.ShiftPage<Shifts.ExpenseDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-expensedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet,Authorize(Policy="Permission:Expenses.View")]
    public Task<IActionResult> History([FromQuery]VaultHistoryRequest r,CancellationToken ct)=>Execute(()=>expenses.HistoryAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.HistoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/expenses?Page=1&PageSize=25 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "items": [],
  "total": 0,
  "page": 1,
  "pageSize": 25
}
```
<a id="get-api-expenses-id-guid"></a>
## GET /api/expenses/{id:guid}
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:12`, action `Get`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Expenses.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account financial history; paid/return operations require own Shift. Categories are global shared catalog; category writes additionally require Branches.AccessAll. Returns use original source account and actual funds.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.ExpenseDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-expensedetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"),Authorize(Policy="Permission:Expenses.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>expenses.GetAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.GetAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/expenses/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "shiftId": null,
  "recordingEmployeeId": null,
  "categoryId": "11111111-1111-4111-8111-111111111111",
  "categoryName": "FAKE categoryName",
  "accountId": "11111111-1111-4111-8111-111111111111",
  "method": 0,
  "amount": 1200,
  "returnedAmount": 0,
  "description": "FAKE description",
  "receiptReference": null,
  "occurredAt": "2026-10-12T08:00:00Z",
  "businessDate": null,
  "paidAt": null,
  "originalStatus": 0
}
```
<a id="post-api-expenses-id-guid-returns"></a>
## POST /api/expenses/{id:guid}/returns
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:14`, action `Return`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Expenses.Reverse`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account financial history; paid/return operations require own Shift. Categories are global shared catalog; category writes additionally require Branches.AccessAll. Returns use original source account and actual funds.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Shifts.ReverseExpenseRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-reverseexpenserequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.ExpenseReversalDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-expensereversaldetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/returns"),Authorize(Policy="Permission:Expenses.Reverse")]
    public Task<IActionResult> Return(Guid id,ReverseExpenseRequest r,CancellationToken ct)=>Execute(()=>expenses.ReverseAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.ReverseAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/expenses/11111111-1111-4111-8111-111111111111/returns HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "returnedAmount": 50,
  "fundsActuallyReturned": true,
  "reason": "FAKE actual funds returned",
  "returnReference": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "expenseId": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "shiftId": "11111111-1111-4111-8111-111111111111",
  "amount": 1200,
  "reason": "FAKE reason",
  "returnReference": null,
  "occurredAt": "2026-10-12T08:00:00Z",
  "businessDate": "2026-10-12"
}
```
<a id="get-api-expenses-id-guid-returns"></a>
## GET /api/expenses/{id:guid}/returns
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:16`, action `Returns`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Expenses.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account financial history; paid/return operations require own Shift. Categories are global shared catalog; category writes additionally require Branches.AccessAll. Returns use original source account and actual funds.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.ShiftPage<Shifts.ExpenseReversalDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-expensereversaldetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/returns"),Authorize(Policy="Permission:Expenses.View")]
    public Task<IActionResult> Returns(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>expenses.ReturnsAsync(id,page,pageSize,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.ReturnsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/expenses/11111111-1111-4111-8111-111111111111/returns?page=1&pageSize=25 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "items": [],
  "total": 0,
  "page": 1,
  "pageSize": 25
}
```
<a id="get-api-expenses-categories"></a>
## GET /api/expenses/categories
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:18`, action `Categories`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Expenses.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account financial history; paid/return operations require own Shift. Categories are global shared catalog; category writes additionally require Branches.AccessAll. Returns use original source account and actual funds.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.ShiftPage<Shifts.ExpenseCategoryDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-expensecategorydetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("categories"),Authorize(Policy="Permission:Expenses.View")]
    public Task<IActionResult> Categories([FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>expenses.CategoriesAsync(page,pageSize,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.CategoriesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/expenses/categories?page=1&pageSize=25 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "items": [],
  "total": 0,
  "page": 1,
  "pageSize": 25
}
```
<a id="post-api-expenses-categories"></a>
## POST /api/expenses/categories
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:20`, action `Category`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Expenses.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account financial history; paid/return operations require own Shift. Categories are global shared catalog; category writes additionally require Branches.AccessAll. Returns use original source account and actual funds.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Shifts.ExpenseCategoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-expensecategoryrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.ExpenseCategoryDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-expensecategorydetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("categories"),Authorize(Policy="Permission:Expenses.Manage")]
    public Task<IActionResult> Category(ExpenseCategoryRequest r,CancellationToken ct)=>Execute(()=>expenses.SetCategoryAsync(null,r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.SetCategoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/expenses/categories HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "name": "FAKE name",
  "isActive": true
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "isActive": true
}
```
<a id="put-api-expenses-categories-id-guid"></a>
## PUT /api/expenses/categories/{id:guid}
Source: `CampionsSystem.API/Controllers/ExpensesController.cs:22`, action `Category`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Expenses.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch/account financial history; paid/return operations require own Shift. Categories are global shared catalog; category writes additionally require Branches.AccessAll. Returns use original source account and actual funds.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Shifts.ExpenseCategoryRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-expensecategoryrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.ExpenseCategoryDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-expensecategorydetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("categories/{id:guid}"),Authorize(Policy="Permission:Expenses.Manage")]
    public Task<IActionResult> Category(Guid id,ExpenseCategoryRequest r,CancellationToken ct)=>Execute(()=>expenses.SetCategoryAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ExpenseService.SetCategoryAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/expenses/categories/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "name": "FAKE name",
  "isActive": true
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "isActive": true
}
```
