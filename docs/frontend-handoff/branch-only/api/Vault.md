# Vault endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Shifts.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. 

**Scope/actor rules:** Branch scope. Setup/withdrawal additionally need Branches.AccessAll; counts/withdrawals need own Shift (setup does not). Current shared physical position differs from historical observations/book position.

<a id="post-api-vault-setup"></a>
## POST /api/vault/setup
Source: `CampionsSystem.API/Controllers/VaultController.cs:8`, action `Setup`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Vault.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). BLL additionally requires Branches.AccessAll; explicit immutable drawer mapping and actual initial count. No operational sale revenue.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Shifts.SetupDrawerRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-setupdrawerrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.DrawerPosition](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-drawerposition). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("setup"),Authorize(Policy="Permission:Vault.Manage")]
    public Task<IActionResult> Setup(SetupDrawerRequest r,CancellationToken ct)=>Execute(()=>vault.SetupAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.VaultService.SetupAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/vault/setup HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "financialAccountId": "11111111-1111-4111-8111-111111111111",
  "countedCash": 1200,
  "reason": "FAKE reason",
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "accountId": "11111111-1111-4111-8111-111111111111",
  "bookCash": 0,
  "expectedPhysicalCash": 0,
  "lastCountedCash": 0,
  "movementSequence": 0,
  "lastCountId": null,
  "observedDiscrepancies": 0
}
```
<a id="get-api-vault-position"></a>
## GET /api/vault/position
Source: `CampionsSystem.API/Controllers/VaultController.cs:10`, action `Position`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Vault.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch scope. Setup/withdrawal additionally need Branches.AccessAll; counts/withdrawals need own Shift (setup does not). Current shared physical position differs from historical observations/book position.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.DrawerPosition](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-drawerposition). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("position"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Position([FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>vault.PositionAsync(branchId,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.VaultService.PositionAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/vault/position HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "accountId": "11111111-1111-4111-8111-111111111111",
  "bookCash": 0,
  "expectedPhysicalCash": 0,
  "lastCountedCash": 0,
  "movementSequence": 0,
  "lastCountId": null,
  "observedDiscrepancies": 0
}
```
<a id="post-api-vault-counts"></a>
## POST /api/vault/counts
Source: `CampionsSystem.API/Controllers/VaultController.cs:12`, action `Count`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Vault.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch scope. Setup/withdrawal additionally need Branches.AccessAll; counts/withdrawals need own Shift (setup does not). Current shared physical position differs from historical observations/book position.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Shifts.CountCashRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-countcashrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.CashCountDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-cashcountdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("counts"),Authorize(Policy="Permission:Vault.Manage")]
    public Task<IActionResult> Count(CountCashRequest r,CancellationToken ct)=>Execute(()=>vault.CountAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.VaultService.CountAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/vault/counts HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "countedCash": 1200,
  "reason": "FAKE reason",
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "shiftId": null,
  "kind": "FAKE kind",
  "expectedCash": 0,
  "countedCash": 1200,
  "bookCash": 0,
  "difference": 0,
  "occurredAt": "2026-10-12T08:00:00Z",
  "businessDate": "2026-10-12",
  "actorId": "11111111-1111-4111-8111-111111111111",
  "reason": null,
  "observationOrder": 0
}
```
<a id="get-api-vault-counts"></a>
## GET /api/vault/counts
Source: `CampionsSystem.API/Controllers/VaultController.cs:14`, action `Counts`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Vault.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch scope. Setup/withdrawal additionally need Branches.AccessAll; counts/withdrawals need own Shift (setup does not). Current shared physical position differs from historical observations/book position.
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
Success: **200**, body [Shifts.ShiftPage<Shifts.CashCountDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-cashcountdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("counts"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Counts([FromQuery]VaultHistoryRequest r,CancellationToken ct)=>Execute(()=>vault.CountsAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.VaultService.CountsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/vault/counts?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-vault-movements"></a>
## GET /api/vault/movements
Source: `CampionsSystem.API/Controllers/VaultController.cs:16`, action `Movements`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Vault.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch scope. Setup/withdrawal additionally need Branches.AccessAll; counts/withdrawals need own Shift (setup does not). Current shared physical position differs from historical observations/book position.
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
Success: **200**, body [Shifts.ShiftPage<Shifts.FinancialMovementDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-financialmovementdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("movements"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Movements([FromQuery]VaultHistoryRequest r,CancellationToken ct)=>Execute(()=>vault.MovementsAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.VaultService.MovementsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/vault/movements?Page=1&PageSize=25 HTTP/1.1
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
<a id="post-api-vault-withdrawals"></a>
## POST /api/vault/withdrawals
Source: `CampionsSystem.API/Controllers/VaultController.cs:18`, action `Withdraw`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Vault.Withdraw`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch scope. Setup/withdrawal additionally need Branches.AccessAll; counts/withdrawals need own Shift (setup does not). Current shared physical position differs from historical observations/book position.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Shifts.WithdrawalRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-withdrawalrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Shifts.CashMovementDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-cashmovementdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("withdrawals"),Authorize(Policy="Permission:Vault.Withdraw")]
    public Task<IActionResult> Withdraw(WithdrawalRequest r,CancellationToken ct)=>Execute(()=>vault.WithdrawAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.VaultService.WithdrawAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/vault/withdrawals HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "amount": 1200,
  "reason": "FAKE reason",
  "branchId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "shiftId": null,
  "kind": "FAKE kind",
  "amount": 1200,
  "occurredAt": "2026-10-12T08:00:00Z",
  "businessDate": "2026-10-12",
  "reason": "FAKE reason"
}
```
<a id="get-api-vault-summary"></a>
## GET /api/vault/summary
Source: `CampionsSystem.API/Controllers/VaultController.cs:20`, action `Summary`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Vault.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch scope. Setup/withdrawal additionally need Branches.AccessAll; counts/withdrawals need own Shift (setup does not). Current shared physical position differs from historical observations/book position.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `from` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
| `to` / query | false | `{"type":"string","format":"date"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.BranchFinancialSummary](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-branchfinancialsummary). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("summary"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Summary([FromQuery]Guid? branchId,[FromQuery]DateOnly? from,[FromQuery]DateOnly? to,CancellationToken ct)=>Execute(()=>reports.BranchAsync(branchId,from,to,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftReportService.BranchAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/vault/summary HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "from": null,
  "to": null,
  "activity": [],
  "currentSharedDrawer": {
    "branchId": "11111111-1111-4111-8111-111111111111",
    "accountId": "11111111-1111-4111-8111-111111111111",
    "bookCash": 0,
    "expectedPhysicalCash": 0,
    "lastCountedCash": 0,
    "movementSequence": 0,
    "lastCountId": null,
    "observedDiscrepancies": 0
  }
}
```
<a id="get-api-vault-audit"></a>
## GET /api/vault/audit
Source: `CampionsSystem.API/Controllers/VaultController.cs:22`, action `Audit`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Vault.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Shifts.md). Branch scope. Setup/withdrawal additionally need Branches.AccessAll; counts/withdrawals need own Shift (setup does not). Current shared physical position differs from historical observations/book position.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `entityId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Shifts.ShiftPage<Shifts.FinancialAuditDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-financialauditdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("audit"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Audit([FromQuery]Guid? branchId,[FromQuery]Guid? entityId,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>reports.AuditAsync(branchId,entityId,page,pageSize,ct));
```
Service entry points: `CampionsSystem.BLL.Shifts.ShiftReportService.AuditAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/vault/audit?page=1&pageSize=25 HTTP/1.1
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
