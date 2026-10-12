# Commissions endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Commissions.md) · [shared scope](../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE at completed backend a2a0c56, inspected at 476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80. Runtime business operations were not exercised by this documentation task.

**Scope/actor rules:** Selling/collection branch scope; permission lets read branch statements, not just own employee. Rules/open/finalize additionally require Branches.AccessAll. No own Shift requirement for commission read/config/admin.

<a id="post-api-commissions-rules"></a>
## POST /api/commissions/rules
Source: `CampionsSystem.API/Controllers/CommissionsController.cs:8`, action `Configure`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Commission.ManageRules`.
Scope/actor: apply module rules above and [guard evidence](rules/Commissions.md). Selling/collection branch scope; permission lets read branch statements, not just own employee. Rules/open/finalize additionally require Branches.AccessAll. No own Shift requirement for commission read/config/admin.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Commissions.ConfigureCommissionRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-configurecommissionrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [IReadOnlyList<Commissions.CommissionRuleDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-commissions-commissionruledetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("rules"), Authorize(Policy = "Permission:Commission.ManageRules")]
    public Task<IActionResult> Configure(ConfigureCommissionRequest r, CancellationToken ct) => Execute(() => commissions.ConfigureAsync(r, ct));
```
Service entry points: `CampionsSystem.BLL.Commissions.CommissionService.ConfigureAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/commissions/rules HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "employeeId": null,
  "effectiveFrom": "2026-10-12",
  "newClients": {
    "target": 30000,
    "belowRate": 5,
    "reachedRate": 7
  },
  "renewals": {
    "target": 30000,
    "belowRate": 3,
    "reachedRate": 5
  },
  "branchId": "11111111-1111-4111-8111-111111111111"
}
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="get-api-commissions-rules"></a>
## GET /api/commissions/rules
Source: `CampionsSystem.API/Controllers/CommissionsController.cs:10`, action `Rules`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Commission.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Commissions.md). Selling/collection branch scope; permission lets read branch statements, not just own employee. Rules/open/finalize additionally require Branches.AccessAll. No own Shift requirement for commission read/config/admin.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `employeeId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Commissions.CommissionPage<Commissions.CommissionRuleDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionruledetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("rules"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> Rules([FromQuery] Guid? employeeId, [FromQuery] Guid? branchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => Execute(() => commissions.RulesAsync(employeeId, branchId, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Commissions.CommissionService.RulesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/commissions/rules?page=1&pageSize=25 HTTP/1.1
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
<a id="post-api-commissions-periods"></a>
## POST /api/commissions/periods
Source: `CampionsSystem.API/Controllers/CommissionsController.cs:13`, action `Open`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Commission.ClosePeriod`.
Scope/actor: apply module rules above and [guard evidence](rules/Commissions.md). Selling/collection branch scope; permission lets read branch statements, not just own employee. Rules/open/finalize additionally require Branches.AccessAll. No own Shift requirement for commission read/config/admin.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Commissions.OpenCommissionPeriodRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-opencommissionperiodrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Commissions.CommissionPeriodDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-commissionperioddetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("periods"), Authorize(Policy = "Permission:Commission.ClosePeriod")]
    public Task<IActionResult> Open(OpenCommissionPeriodRequest r, CancellationToken ct) => Execute(() => commissions.OpenAsync(r, ct));
```
Service entry points: `CampionsSystem.BLL.Commissions.CommissionService.OpenAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/commissions/periods HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE October period",
  "startDate": "2026-10-12",
  "endDate": "2026-10-31",
  "branchId": "11111111-1111-4111-8111-111111111111"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "startDate": null,
  "endDate": null,
  "status": "Open",
  "version": 1,
  "finalizedAt": null
}
```
<a id="get-api-commissions-periods"></a>
## GET /api/commissions/periods
Source: `CampionsSystem.API/Controllers/CommissionsController.cs:15`, action `History`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Commission.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Commissions.md). Selling/collection branch scope; permission lets read branch statements, not just own employee. Rules/open/finalize additionally require Branches.AccessAll. No own Shift requirement for commission read/config/admin.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Commissions.CommissionPage<Commissions.CommissionPeriodDetails>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionperioddetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("periods"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> History([FromQuery] Guid? branchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => Execute(() => commissions.PeriodsAsync(branchId, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Commissions.CommissionService.PeriodsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/commissions/periods?page=1&pageSize=25 HTTP/1.1
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
<a id="post-api-commissions-periods-id-guid-finalize"></a>
## POST /api/commissions/periods/{id:guid}/finalize
Source: `CampionsSystem.API/Controllers/CommissionsController.cs:18`, action `Finalize`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Commission.ClosePeriod`.
Scope/actor: apply module rules above and [guard evidence](rules/Commissions.md). Selling/collection branch scope; permission lets read branch statements, not just own employee. Rules/open/finalize additionally require Branches.AccessAll. No own Shift requirement for commission read/config/admin.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Commissions.FinalizeCommissionPeriodRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-finalizecommissionperiodrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Commissions.CommissionPeriodDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-commissionperioddetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: operationId in body: nonempty UUID, preserve exact payload/actor on retry; business-specific matching in guard evidence. expectedVersion is required by this body; stale EF concurrency returns 409. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("periods/{id:guid}/finalize"), Authorize(Policy = "Permission:Commission.ClosePeriod")]
    public Task<IActionResult> Finalize(Guid id, FinalizeCommissionPeriodRequest r, CancellationToken ct) => Execute(() => commissions.FinalizeAsync(id, r, ct));
```
Service entry points: `CampionsSystem.BLL.Commissions.CommissionService.FinalizeAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/commissions/periods/11111111-1111-4111-8111-111111111111/finalize HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "operationId": "11111111-1111-4111-8111-111111111111",
  "expectedVersion": 0
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE name",
  "startDate": null,
  "endDate": null,
  "status": "Open",
  "version": 1,
  "finalizedAt": null
}
```
<a id="get-api-commissions-periods-id-guid"></a>
## GET /api/commissions/periods/{id:guid}
Source: `CampionsSystem.API/Controllers/CommissionsController.cs:20`, action `Progress`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Commission.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Commissions.md). Selling/collection branch scope; permission lets read branch statements, not just own employee. Rules/open/finalize additionally require Branches.AccessAll. No own Shift requirement for commission read/config/admin.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Commissions.CommissionProgress](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-commissionprogress). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("periods/{id:guid}"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> Progress(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => Execute(() => commissions.ProgressAsync(id, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Commissions.CommissionService.ProgressAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/commissions/periods/11111111-1111-4111-8111-111111111111?page=1&pageSize=25 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "period": {
    "id": "11111111-1111-4111-8111-111111111111",
    "branchId": "11111111-1111-4111-8111-111111111111",
    "name": "FAKE name",
    "startDate": null,
    "endDate": null,
    "status": "Open",
    "version": 1,
    "finalizedAt": null
  },
  "finalized": false,
  "grossCollections": 0,
  "refunds": 0,
  "retainedCollections": 0,
  "employeeEligibleCollections": 0,
  "houseCollections": 0,
  "houseNewClientCollections": 0,
  "houseRenewalCollections": 0,
  "excludedCollections": 0,
  "newClientEligibleCollections": 0,
  "renewalEligibleCollections": 0,
  "totalCommission": 0,
  "employeeCount": 0,
  "employees": [],
  "page": 1,
  "pageSize": 25
}
```
<a id="get-api-commissions-periods-id-guid-employees-employeeid-guid"></a>
## GET /api/commissions/periods/{id:guid}/employees/{employeeId:guid}
Source: `CampionsSystem.API/Controllers/CommissionsController.cs:23`, action `Employee`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Commission.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Commissions.md). Selling/collection branch scope; permission lets read branch statements, not just own employee. Rules/open/finalize additionally require Branches.AccessAll. No own Shift requirement for commission read/config/admin.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `employeeId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Commissions.EmployeeCommissionStatement](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-employeecommissionstatement). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("periods/{id:guid}/employees/{employeeId:guid}"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> Employee(Guid id, Guid employeeId, CancellationToken ct) => Execute(() => commissions.EmployeeStatementAsync(id, employeeId, ct));
```
Service entry points: `CampionsSystem.BLL.Commissions.CommissionService.EmployeeStatementAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/commissions/periods/11111111-1111-4111-8111-111111111111/employees/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "employeeId": "11111111-1111-4111-8111-111111111111",
  "employeeName": "FAKE employeeName",
  "newClients": {
    "category": "NewMembership",
    "grossCollections": 0,
    "refunds": 0,
    "eligibleCollections": 0,
    "configured": false,
    "target": null,
    "targetReached": false,
    "belowRate": null,
    "reachedRate": null,
    "appliedRate": null,
    "commission": 0,
    "ruleId": null,
    "retainedCollections": 0
  },
  "renewals": {
    "category": "Renewal",
    "grossCollections": 0,
    "refunds": 0,
    "eligibleCollections": 0,
    "configured": false,
    "target": null,
    "targetReached": false,
    "belowRate": null,
    "reachedRate": null,
    "appliedRate": null,
    "commission": 0,
    "ruleId": null,
    "retainedCollections": 0
  },
  "totalCommission": 0,
  "periodId": "11111111-1111-4111-8111-111111111111",
  "finalized": false,
  "houseCollections": 0,
  "excludedCollections": 0
}
```
<a id="get-api-commissions-periods-id-guid-sources"></a>
## GET /api/commissions/periods/{id:guid}/sources
Source: `CampionsSystem.API/Controllers/CommissionsController.cs:25`, action `Sources`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Commission.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Commissions.md). Selling/collection branch scope; permission lets read branch statements, not just own employee. Rules/open/finalize additionally require Branches.AccessAll. No own Shift requirement for commission read/config/admin.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `employeeId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Commissions.CommissionPage<Commissions.CommissionSourceLine>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionsourceline). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("periods/{id:guid}/sources"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> Sources(Guid id, [FromQuery] Guid? employeeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => Execute(() => commissions.SourcesAsync(id, employeeId, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Commissions.CommissionService.SourcesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/commissions/periods/11111111-1111-4111-8111-111111111111/sources?page=1&pageSize=25 HTTP/1.1
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
<a id="get-api-commissions-periods-id-guid-refund-allocations"></a>
## GET /api/commissions/periods/{id:guid}/refund-allocations
Source: `CampionsSystem.API/Controllers/CommissionsController.cs:28`, action `RefundAllocations`.
Status: **VERIFIED FROM CODE / completed backend**. Deployment readiness is separate; see [rollout limitations](../OPEN_QUESTIONS_AND_GAPS.md).
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Commission.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Commissions.md). Selling/collection branch scope; permission lets read branch statements, not just own employee. Rules/open/finalize additionally require Branches.AccessAll. No own Shift requirement for commission read/config/admin.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":1}` | 1 |
| `pageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32","default":25}` | 25 |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Commissions.CommissionPage<Commissions.CommissionRefundSource>](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionrefundsource). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("periods/{id:guid}/refund-allocations"), Authorize(Policy = "Permission:Commission.View")]
    public Task<IActionResult> RefundAllocations(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => Execute(() => commissions.RefundSourcesAsync(id, page, pageSize, ct));
```
Service entry points: `CampionsSystem.BLL.Commissions.CommissionService.RefundSourcesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/commissions/periods/11111111-1111-4111-8111-111111111111/refund-allocations?page=1&pageSize=25 HTTP/1.1
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
