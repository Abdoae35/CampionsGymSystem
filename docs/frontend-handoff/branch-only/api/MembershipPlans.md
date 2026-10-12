# MembershipPlans endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Plans.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. 

**Scope/actor rules:** Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.

<a id="get-api-membership-plans"></a>
## GET /api/membership-plans
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:8`, action `List`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `BranchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `CityId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `PlanType` / query | false | `{"$ref":"#/components/schemas/PlanCategory"}` | See query DTO defaults / guard evidence |
| `AudienceCategoryId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Mode` / query | false | `{"$ref":"#/components/schemas/AccessMode"}` | See query DTO defaults / guard evidence |
| `FreelanceCoachEmployeeId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `IncludeInternal` / query | false | `{"type":"boolean"}` | See query DTO defaults / guard evidence |
| `Search` / query | false | `{"type":"string"}` | See query DTO defaults / guard evidence |
| `IsActive` / query | false | `{"type":"boolean"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Plans.MembershipPlanListRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-membershipplanlistrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Plans.MembershipPlanListResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-membershipplanlistresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet,Authorize(Policy="Permission:MembershipPlans.View")]
    public Task<IActionResult> List([FromQuery]MembershipPlanListRequest r,CancellationToken ct)=>Execute(()=>plans.GetPlansAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.GetPlansAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/membership-plans?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-membership-plans-id-guid"></a>
## GET /api/membership-plans/{id:guid}
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:10`, action `Get`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Plans.MembershipPlanDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-membershipplandetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}"),Authorize(Policy="Permission:MembershipPlans.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>plans.GetPlanByIdAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.GetPlanByIdAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/membership-plans/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "plan": {
    "id": "11111111-1111-4111-8111-111111111111",
    "name": "FAKE name",
    "planType": "Normal",
    "branch": null,
    "city": {
      "id": "11111111-1111-4111-8111-111111111111",
      "name": "FAKE name"
    },
    "audienceCategory": null,
    "mode": "Unlimited",
    "price": 1200,
    "durationDays": 30,
    "sessions": null,
    "allowFreeze": false,
    "freezeDays": 0,
    "isActive": true,
    "freelanceCoach": null,
    "customerPurchasable": false
  },
  "createdAt": "2026-10-12T08:00:00Z",
  "benefits": [],
  "eligibleMemberCount": 0
}
```
<a id="post-api-membership-plans"></a>
## POST /api/membership-plans
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:12`, action `Create`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Plans.CreateMembershipPlanRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-createmembershipplanrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Plans.MembershipPlanDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-membershipplandetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost,Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Create(CreateMembershipPlanRequest r,CancellationToken ct)=>Execute(()=>plans.CreatePlanAsync(r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.CreatePlanAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/membership-plans HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "cityId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE Unlimited Plan",
  "planType": "Normal",
  "mode": "Unlimited",
  "durationDays": 30,
  "sessions": null,
  "price": 1200,
  "allowFreeze": false,
  "freezeDays": 0
}
```
Schema-shaped response (not an executed response):
```json
{
  "plan": {
    "id": "11111111-1111-4111-8111-111111111111",
    "name": "FAKE name",
    "planType": "Normal",
    "branch": null,
    "city": {
      "id": "11111111-1111-4111-8111-111111111111",
      "name": "FAKE name"
    },
    "audienceCategory": null,
    "mode": "Unlimited",
    "price": 1200,
    "durationDays": 30,
    "sessions": null,
    "allowFreeze": false,
    "freezeDays": 0,
    "isActive": true,
    "freelanceCoach": null,
    "customerPurchasable": false
  },
  "createdAt": "2026-10-12T08:00:00Z",
  "benefits": [],
  "eligibleMemberCount": 0
}
```
<a id="put-api-membership-plans-id-guid"></a>
## PUT /api/membership-plans/{id:guid}
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:14`, action `Update`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Plans.UpdateMembershipPlanRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-updatemembershipplanrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Plans.MembershipPlanDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-membershipplandetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("{id:guid}"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Update(Guid id,UpdateMembershipPlanRequest r,CancellationToken ct)=>Execute(()=>plans.UpdatePlanAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.UpdatePlanAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/membership-plans/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "branchId": "11111111-1111-4111-8111-111111111111",
  "cityId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE Unlimited Plan",
  "planType": "Normal",
  "mode": "Unlimited",
  "durationDays": 30,
  "sessions": null,
  "price": 1200,
  "allowFreeze": false,
  "freezeDays": 0
}
```
Schema-shaped response (not an executed response):
```json
{
  "plan": {
    "id": "11111111-1111-4111-8111-111111111111",
    "name": "FAKE name",
    "planType": "Normal",
    "branch": null,
    "city": {
      "id": "11111111-1111-4111-8111-111111111111",
      "name": "FAKE name"
    },
    "audienceCategory": null,
    "mode": "Unlimited",
    "price": 1200,
    "durationDays": 30,
    "sessions": null,
    "allowFreeze": false,
    "freezeDays": 0,
    "isActive": true,
    "freelanceCoach": null,
    "customerPurchasable": false
  },
  "createdAt": "2026-10-12T08:00:00Z",
  "benefits": [],
  "eligibleMemberCount": 0
}
```
<a id="patch-api-membership-plans-id-guid-status"></a>
## PATCH /api/membership-plans/{id:guid}/status
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:16`, action `Status`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Plans.ChangePlanStatusRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-changeplanstatusrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Plans.MembershipPlanDetails](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-membershipplandetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPatch("{id:guid}/status"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> Status(Guid id,ChangePlanStatusRequest r,CancellationToken ct)=>Execute(()=>plans.ChangePlanStatusAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.ChangePlanStatusAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PATCH /api/membership-plans/11111111-1111-4111-8111-111111111111/status HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "isActive": true
}
```
Schema-shaped response (not an executed response):
```json
{
  "plan": {
    "id": "11111111-1111-4111-8111-111111111111",
    "name": "FAKE name",
    "planType": "Normal",
    "branch": null,
    "city": {
      "id": "11111111-1111-4111-8111-111111111111",
      "name": "FAKE name"
    },
    "audienceCategory": null,
    "mode": "Unlimited",
    "price": 1200,
    "durationDays": 30,
    "sessions": null,
    "allowFreeze": false,
    "freezeDays": 0,
    "isActive": true,
    "freelanceCoach": null,
    "customerPurchasable": false
  },
  "createdAt": "2026-10-12T08:00:00Z",
  "benefits": [],
  "eligibleMemberCount": 0
}
```
<a id="get-api-membership-plans-id-guid-benefits"></a>
## GET /api/membership-plans/{id:guid}/benefits
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:18`, action `Benefits`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Plans.PlanBenefitDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-plans-planbenefitdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/benefits"),Authorize(Policy="Permission:MembershipPlans.View")]
    public Task<IActionResult> Benefits(Guid id,CancellationToken ct)=>Execute(()=>plans.GetPlanBenefitsAsync(id,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.GetPlanBenefitsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/membership-plans/11111111-1111-4111-8111-111111111111/benefits HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="put-api-membership-plans-id-guid-benefits"></a>
## PUT /api/membership-plans/{id:guid}/benefits
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:20`, action `SetBenefits`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:MembershipPlans.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Plans.SetPlanBenefitsRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-setplanbenefitsrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [IReadOnlyList<Plans.PlanBenefitDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-plans-planbenefitdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("{id:guid}/benefits"),Authorize(Policy="Permission:MembershipPlans.Manage")]
    public Task<IActionResult> SetBenefits(Guid id,SetPlanBenefitsRequest r,CancellationToken ct)=>Execute(()=>plans.SetPlanBenefitsAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.SetPlanBenefitsAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/membership-plans/11111111-1111-4111-8111-111111111111/benefits HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "benefits": []
}
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="get-api-membership-plans-id-guid-eligible-members"></a>
## GET /api/membership-plans/{id:guid}/eligible-members
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:22`, action `Eligible`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:FreelancePlans.ManageEligibleMembers`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Search` / query | false | `{"type":"string"}` | See query DTO defaults / guard evidence |
| `IsActive` / query | false | `{"type":"boolean"}` | See query DTO defaults / guard evidence |
| `Page` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
| `PageSize` / query | false | `{"pattern":"^-?(?:0\|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | See query DTO defaults / guard evidence |
Query DTO: [Plans.CatalogListRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-cataloglistrequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [Plans.EligibleMemberListResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-eligiblememberlistresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/eligible-members"),Authorize(Policy="Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> Eligible(Guid id,[FromQuery]CatalogListRequest r,CancellationToken ct)=>Execute(()=>plans.GetEligibleMembersAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.GetEligibleMembersAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/membership-plans/11111111-1111-4111-8111-111111111111/eligible-members?Page=1&PageSize=25 HTTP/1.1
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
<a id="get-api-membership-plans-id-guid-eligible-members-candidates"></a>
## GET /api/membership-plans/{id:guid}/eligible-members/candidates
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:24`, action `Candidates`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:FreelancePlans.ManageEligibleMembers`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `MemberId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `Phone` / query | false | `{"maxLength":64,"type":"string"}` | See query DTO defaults / guard evidence |
| `MemberCode` / query | false | `{"maxLength":256,"type":"string"}` | See query DTO defaults / guard evidence |
Query DTO: [Plans.EligibleMemberCandidateRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-eligiblemembercandidaterequest) (all inherited fields included).
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Plans.EligibleMemberDetails>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-plans-eligiblememberdetails). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("{id:guid}/eligible-members/candidates"),Authorize(Policy="Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> Candidates(Guid id,[FromQuery]EligibleMemberCandidateRequest r,CancellationToken ct)=>Execute(()=>plans.FindEligibleMemberCandidatesAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.FindEligibleMemberCandidatesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/membership-plans/11111111-1111-4111-8111-111111111111/eligible-members/candidates HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="post-api-membership-plans-id-guid-eligible-members"></a>
## POST /api/membership-plans/{id:guid}/eligible-members
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:26`, action `AddEligible`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:FreelancePlans.ManageEligibleMembers`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Plans.EligibleMemberRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-eligiblememberrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [bool](../DTO_AND_ENUM_REFERENCE.md#bool). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("{id:guid}/eligible-members"),Authorize(Policy="Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> AddEligible(Guid id,EligibleMemberRequest r,CancellationToken ct)=>Execute(()=>plans.AddEligibleMemberAsync(id,r,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.AddEligibleMemberAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/membership-plans/11111111-1111-4111-8111-111111111111/eligible-members HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "memberId": "11111111-1111-4111-8111-111111111111"
}
```
Schema-shaped response (not an executed response):
```json
false
```
<a id="delete-api-membership-plans-id-guid-eligible-members-memberid-guid"></a>
## DELETE /api/membership-plans/{id:guid}/eligible-members/{memberId:guid}
Source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:28`, action `RemoveEligible`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:FreelancePlans.ManageEligibleMembers`.
Scope/actor: apply module rules above and [guard evidence](rules/Plans.md). Plan owning BranchId controls administration, CityId controls future access. Global/unmapped/internal rules differ; Employee detail/list opt-in also needs MembershipPlans.Manage. Freelance eligibility uses plan scope without restricting Member registration branch.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `memberId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [bool](../DTO_AND_ENUM_REFERENCE.md#bool). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 validation/business; 401 unauthenticated; 403 capability/scope/ownership; 404 scoped/missing resources where BranchNotFoundException is thrown; 409 stale version/unique/serialization/deadlock; 500 unexpected. Exact applicable guard conditions and messages: linked module evidence, not a claim every status is reachable for every invocation. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpDelete("{id:guid}/eligible-members/{memberId:guid}"),Authorize(Policy="Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> RemoveEligible(Guid id,Guid memberId,CancellationToken ct)=>Execute(()=>plans.RemoveEligibleMemberAsync(id,memberId,ct));
```
Service entry points: `CampionsSystem.BLL.Plans.MembershipPlanManagementService.RemoveEligibleMemberAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
DELETE /api/membership-plans/11111111-1111-4111-8111-111111111111/eligible-members/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
false
```
