# Operations endpoints

[Master catalog](../API_ENDPOINT_CATALOG.md) · [DTO reference](../DTO_AND_ENUM_REFERENCE.md) · [module guard evidence](rules/Operations.md) · [shared scope](../../AUTH_PERMISSIONS_BRANCH_CONTEXT.md)

**Status for every endpoint:** VERIFIED FROM CODE / available at committed baseline in the frozen current branch working tree; this is not stable master availability. Runtime operations were not exercised here. Legacy partial module, not the new Private/Classes feature.

**Scope/actor rules:** Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.

<a id="post-api-activities-coaches"></a>
## POST /api/activities/coaches
Source: `CampionsSystem.API/Controllers/OperationsController.cs:16`, action `CreateCoach`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Activities.ManageCoaches`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Operations.CreateActivityCoachRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-operations-createactivitycoachrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [<anonymous type: Guid Id>](../DTO_AND_ENUM_REFERENCE.md#anonymous-type-system-guid-id). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("activities/coaches")]
    [Authorize(Policy = "Permission:Activities.ManageCoaches")]
    public Task<IActionResult> CreateCoach(CreateActivityCoachRequest request, CancellationToken ct) =>
        Execute(async () => new { Id = await activities.CreateCoachAsync(request, ct) });
```
Service entry points: `CampionsSystem.BLL.Operations.ActivityManagementService.CreateCoachAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/activities/coaches HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "coachType": 1,
  "employeeId": null,
  "externalName": "FAKE External Coach",
  "externalPhone": null,
  "percentage": 50
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111"
}
```
<a id="put-api-activities-coaches-id-guid-percentage"></a>
## PUT /api/activities/coaches/{id:guid}/percentage
Source: `CampionsSystem.API/Controllers/OperationsController.cs:21`, action `Percentage`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Activities.ManageCoaches`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [OperationsController.PercentageRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-api-controllers-operationscontroller-percentagerequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [<anonymous type: bool Updated>](../DTO_AND_ENUM_REFERENCE.md#anonymous-type-bool-updated). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("activities/coaches/{id:guid}/percentage")]
    [Authorize(Policy = "Permission:Activities.ManageCoaches")]
    public Task<IActionResult> Percentage(Guid id, PercentageRequest request, CancellationToken ct) =>
        Execute(async () => { await activities.SetPercentageAsync(id, request.Percentage, ct); return new { Updated = true }; });
```
Service entry points: `CampionsSystem.BLL.Operations.ActivityManagementService.SetPercentageAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/activities/coaches/11111111-1111-4111-8111-111111111111/percentage HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "percentage": 50
}
```
Schema-shaped response (not an executed response):
```json
{
  "updated": true
}
```
<a id="post-api-activities-id-guid-coach-assignments"></a>
## POST /api/activities/{id:guid}/coach-assignments
Source: `CampionsSystem.API/Controllers/OperationsController.cs:26`, action `Assign`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Activities.ManageCoaches`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [OperationsController.AssignmentRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-api-controllers-operationscontroller-assignmentrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [<anonymous type: Guid Id>](../DTO_AND_ENUM_REFERENCE.md#anonymous-type-system-guid-id). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("activities/{id:guid}/coach-assignments")]
    [Authorize(Policy = "Permission:Activities.ManageCoaches")]
    public Task<IActionResult> Assign(Guid id, AssignmentRequest request, CancellationToken ct) =>
        Execute(async () => new { Id = await activities.AssignAsync(id, request.ActivityCoachId, ct) });
```
Service entry points: `CampionsSystem.BLL.Operations.ActivityManagementService.AssignAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/activities/11111111-1111-4111-8111-111111111111/coach-assignments HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "activityCoachId": "11111111-1111-4111-8111-111111111111"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111"
}
```
<a id="delete-api-activities-coach-assignments-id-guid"></a>
## DELETE /api/activities/coach-assignments/{id:guid}
Source: `CampionsSystem.API/Controllers/OperationsController.cs:31`, action `Unassign`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Activities.ManageCoaches`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [<anonymous type: bool Updated>](../DTO_AND_ENUM_REFERENCE.md#anonymous-type-bool-updated). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpDelete("activities/coach-assignments/{id:guid}")]
    [Authorize(Policy = "Permission:Activities.ManageCoaches")]
    public Task<IActionResult> Unassign(Guid id, CancellationToken ct) =>
        Execute(async () => { await activities.UnassignAsync(id, ct); return new { Updated = true }; });
```
Service entry points: `CampionsSystem.BLL.Operations.ActivityManagementService.UnassignAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
DELETE /api/activities/coach-assignments/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "updated": true
}
```
<a id="post-api-activities-purchases"></a>
## POST /api/activities/purchases
Source: `CampionsSystem.API/Controllers/OperationsController.cs:36`, action `PurchaseActivity`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Activities.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy creates entitlement/purchase and amount due, not collection; activity/package/coach prerequisites lack full public CRUD.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Operations.ActivityPurchaseRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-operations-activitypurchaserequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [Operations.PurchaseResult](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-operations-purchaseresult). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("activities/purchases")]
    [Authorize(Policy = "Permission:Activities.Manage")]
    public Task<IActionResult> PurchaseActivity(ActivityPurchaseRequest request, CancellationToken ct) =>
        Execute(() => purchases.PurchaseActivityAsync(request, ct));
```
Service entry points: `CampionsSystem.BLL.Operations.SubscriptionPurchaseService.PurchaseActivityAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/activities/purchases HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "memberId": "11111111-1111-4111-8111-111111111111",
  "activityPackageId": "11111111-1111-4111-8111-111111111111",
  "activityCoachId": "11111111-1111-4111-8111-111111111111",
  "existingSubscriptionId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "subscriptionId": "11111111-1111-4111-8111-111111111111",
  "transactionId": "11111111-1111-4111-8111-111111111111",
  "saleTransactionId": "11111111-1111-4111-8111-111111111111",
  "gymAmountDue": 0
}
```
<a id="get-api-activities-coach-revenue-periodid-guid"></a>
## GET /api/activities/coach-revenue/{periodId:guid}
Source: `CampionsSystem.API/Controllers/OperationsController.cs:41`, action `ActivityRevenue`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Activities.ViewCoachRevenue`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `periodId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Operations.ActivityCoachReport>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-operations-activitycoachreport). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("activities/coach-revenue/{periodId:guid}")]
    [Authorize(Policy = "Permission:Activities.ViewCoachRevenue")]
    public Task<IActionResult> ActivityRevenue(Guid periodId, CancellationToken ct) => Execute(() => reports.ActivitiesAsync(periodId, ct));
```
Service entry points: `CampionsSystem.BLL.Operations.CoachReportService.ActivitiesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/activities/coach-revenue/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="post-api-activities-coach-revenue-periodid-guid-finalize"></a>
## POST /api/activities/coach-revenue/{periodId:guid}/finalize
Source: `CampionsSystem.API/Controllers/OperationsController.cs:45`, action `FinalizeActivity`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Commission.ClosePeriod`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Requires closed period with final EndAt; explicit activity snapshot, no payout; cannot finalize twice.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `periodId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [<anonymous type: bool Finalized>](../DTO_AND_ENUM_REFERENCE.md#anonymous-type-bool-finalized). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("activities/coach-revenue/{periodId:guid}/finalize")]
    [Authorize(Policy = "Permission:Commission.ClosePeriod")]
    public Task<IActionResult> FinalizeActivity(Guid periodId, CancellationToken ct) =>
        Execute(async () => { await reports.FinalizeActivitiesAsync(periodId, ct); return new { Finalized = true }; });
```
Service entry points: `CampionsSystem.BLL.Operations.CoachReportService.FinalizeActivitiesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/activities/coach-revenue/11111111-1111-4111-8111-111111111111/finalize HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "finalized": true
}
```
<a id="post-api-classes-sessions"></a>
## POST /api/classes/sessions
Source: `CampionsSystem.API/Controllers/OperationsController.cs:50`, action `CreateSession`.
Status: **VERIFIED FROM CODE / exposed but disabled (410)**.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Operations.SaveClassSessionRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-operations-saveclasssessionrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success body: **none; intentionally 410 ProblemDetails**.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("classes/sessions")]
    [Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> CreateSession(SaveClassSessionRequest request, CancellationToken ct) =>
        Task.FromResult<IActionResult>(Problem(statusCode: 410, title: "Use /api/coaching/classes/occurrences with an operation identity."));
```
Service entry points: direct auth/token/410 adapter. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/classes/sessions HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "groupClassId": "11111111-1111-4111-8111-111111111111",
  "classScheduleId": null,
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "startedAt": "2026-10-12T08:00:00Z",
  "endedAt": null,
  "status": 0
}
```
Schema-shaped response (not an executed response):
```json
{
  "type": "about:blank",
  "title": "Use /api/coaching/classes/occurrences with an operation identity.",
  "status": 410
}
```
<a id="put-api-classes-sessions-id-guid"></a>
## PUT /api/classes/sessions/{id:guid}
Source: `CampionsSystem.API/Controllers/OperationsController.cs:55`, action `UpdateSession`.
Status: **VERIFIED FROM CODE / exposed but disabled (410)**.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Operations.SaveClassSessionRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-operations-saveclasssessionrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success body: **none; intentionally 410 ProblemDetails**.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("classes/sessions/{id:guid}")]
    [Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> UpdateSession(Guid id, SaveClassSessionRequest request, CancellationToken ct) =>
        Task.FromResult<IActionResult>(Problem(statusCode: 410, title: "Use /api/coaching/classes/occurrences/{id}/complete."));
```
Service entry points: direct auth/token/410 adapter. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/classes/sessions/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "groupClassId": "11111111-1111-4111-8111-111111111111",
  "classScheduleId": null,
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "startedAt": "2026-10-12T08:00:00Z",
  "endedAt": null,
  "status": 0
}
```
Schema-shaped response (not an executed response):
```json
{
  "type": "about:blank",
  "title": "Use /api/coaching/classes/occurrences/{id}/complete.",
  "status": 410
}
```
<a id="post-api-classes-sessions-id-guid-attendance-memberid-guid"></a>
## POST /api/classes/sessions/{id:guid}/attendance/{memberId:guid}
Source: `CampionsSystem.API/Controllers/OperationsController.cs:60`, action `Attendance`.
Status: **VERIFIED FROM CODE / exposed but disabled (410)**.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy attendance active Member/session + duplicate checks only. It does not implement new Classes subscription/earnings policies.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `memberId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success body: **none; intentionally 410 ProblemDetails**.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("classes/sessions/{id:guid}/attendance/{memberId:guid}")]
    [Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> Attendance(Guid id, Guid memberId, CancellationToken ct) =>
        Task.FromResult<IActionResult>(Problem(statusCode: 410, title: "Use /api/coaching/classes/occurrences/{id}/attendance with a Classes package and operation identity."));
```
Service entry points: direct auth/token/410 adapter. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/classes/sessions/11111111-1111-4111-8111-111111111111/attendance/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "type": "about:blank",
  "title": "Use /api/coaching/classes/occurrences/{id}/attendance with a Classes package and operation identity.",
  "status": 410
}
```
<a id="get-api-classes-attendance-report"></a>
## GET /api/classes/attendance-report
Source: `CampionsSystem.API/Controllers/OperationsController.cs:65`, action `ClassReport`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:Classes.ViewAttendanceReport`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `from` / query | false | `{"type":"string","format":"date-time"}` | See query DTO defaults / guard evidence |
| `until` / query | false | `{"type":"string","format":"date-time"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Operations.ClassCoachReport>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-operations-classcoachreport). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("classes/attendance-report")]
    [Authorize(Policy = "Permission:Classes.ViewAttendanceReport")]
    public Task<IActionResult> ClassReport(Guid branchId, DateTimeOffset from, DateTimeOffset until, CancellationToken ct) =>
        Execute(() => reports.ClassesAsync(branchId, from, until, ct));
```
Service entry points: `CampionsSystem.BLL.Operations.CoachReportService.ClassesAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/classes/attendance-report HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
<a id="post-api-freelance-plans"></a>
## POST /api/freelance-plans
Source: `CampionsSystem.API/Controllers/OperationsController.cs:70`, action `CreatePlan`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:FreelancePlans.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Operations.SaveFreelancePlanRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-operations-savefreelanceplanrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [<anonymous type: Guid Id>](../DTO_AND_ENUM_REFERENCE.md#anonymous-type-system-guid-id). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("freelance-plans")]
    [Authorize(Policy = "Permission:FreelancePlans.Manage")]
    public Task<IActionResult> CreatePlan(SaveFreelancePlanRequest request, CancellationToken ct) =>
        Execute(async () => new { Id = await freelance.SaveAsync(null, request, ct) });
```
Service entry points: `CampionsSystem.BLL.Operations.FreelancePlanService.SaveAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/freelance-plans HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "cityId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE Fixed Gym Fee",
  "price": 300,
  "durationDays": 30,
  "accessMode": 0,
  "sessionCount": null,
  "isActive": true,
  "branchId": "11111111-1111-4111-8111-111111111111"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111"
}
```
<a id="put-api-freelance-plans-id-guid"></a>
## PUT /api/freelance-plans/{id:guid}
Source: `CampionsSystem.API/Controllers/OperationsController.cs:75`, action `UpdatePlan`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:FreelancePlans.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: [Operations.SaveFreelancePlanRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-operations-savefreelanceplanrequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success: **200**, body [<anonymous type: Guid Id>](../DTO_AND_ENUM_REFERENCE.md#anonymous-type-system-guid-id). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("freelance-plans/{id:guid}")]
    [Authorize(Policy = "Permission:FreelancePlans.Manage")]
    public Task<IActionResult> UpdatePlan(Guid id, SaveFreelancePlanRequest request, CancellationToken ct) =>
        Execute(async () => new { Id = await freelance.SaveAsync(id, request, ct) });
```
Service entry points: `CampionsSystem.BLL.Operations.FreelancePlanService.SaveAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/freelance-plans/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "cityId": "11111111-1111-4111-8111-111111111111",
  "coachEmployeeId": "11111111-1111-4111-8111-111111111111",
  "name": "FAKE Fixed Gym Fee",
  "price": 300,
  "durationDays": 30,
  "accessMode": 0,
  "sessionCount": null,
  "isActive": true,
  "branchId": "11111111-1111-4111-8111-111111111111"
}
```
Schema-shaped response (not an executed response):
```json
{
  "id": "11111111-1111-4111-8111-111111111111"
}
```
<a id="put-api-freelance-plans-id-guid-eligible-members-memberid-guid"></a>
## PUT /api/freelance-plans/{id:guid}/eligible-members/{memberId:guid}
Source: `CampionsSystem.API/Controllers/OperationsController.cs:80`, action `AddEligible`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:FreelancePlans.ManageEligibleMembers`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `memberId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [<anonymous type: bool Eligible>](../DTO_AND_ENUM_REFERENCE.md#anonymous-type-bool-eligible). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPut("freelance-plans/{id:guid}/eligible-members/{memberId:guid}")]
    [Authorize(Policy = "Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> AddEligible(Guid id, Guid memberId, CancellationToken ct) =>
        Execute(async () => { await freelance.SetEligibilityAsync(id, memberId, true, ct); return new { Eligible = true }; });
```
Service entry points: `CampionsSystem.BLL.Operations.FreelancePlanService.SetEligibilityAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
PUT /api/freelance-plans/11111111-1111-4111-8111-111111111111/eligible-members/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "eligible": true
}
```
<a id="delete-api-freelance-plans-id-guid-eligible-members-memberid-guid"></a>
## DELETE /api/freelance-plans/{id:guid}/eligible-members/{memberId:guid}
Source: `CampionsSystem.API/Controllers/OperationsController.cs:85`, action `RemoveEligible`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:FreelancePlans.ManageEligibleMembers`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `memberId` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [<anonymous type: bool Eligible>](../DTO_AND_ENUM_REFERENCE.md#anonymous-type-bool-eligible). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpDelete("freelance-plans/{id:guid}/eligible-members/{memberId:guid}")]
    [Authorize(Policy = "Permission:FreelancePlans.ManageEligibleMembers")]
    public Task<IActionResult> RemoveEligible(Guid id, Guid memberId, CancellationToken ct) =>
        Execute(async () => { await freelance.SetEligibilityAsync(id, memberId, false, ct); return new { Eligible = false }; });
```
Service entry points: `CampionsSystem.BLL.Operations.FreelancePlanService.SetEligibilityAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
DELETE /api/freelance-plans/11111111-1111-4111-8111-111111111111/eligible-members/11111111-1111-4111-8111-111111111111 HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
{
  "eligible": true
}
```
<a id="post-api-freelance-plans-purchases"></a>
## POST /api/freelance-plans/purchases
Source: `CampionsSystem.API/Controllers/OperationsController.cs:90`, action `PurchaseFreelance`.
Status: **VERIFIED FROM CODE / exposed but disabled (410)**.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:FreelancePlans.Manage`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Intentionally disabled route: always 410 after request authentication/binding. No success purchase response.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| none | — | — | — |
### JSON body and response
Body: [Operations.FreelancePurchaseRequest](../DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-operations-freelancepurchaserequest); exact required/nullability/limits/defaults and nested types in linked DTO and `openapi.json`.
Success body: **none; intentionally 410 ProblemDetails**.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404. 410 intentional disabled purchase. See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpPost("freelance-plans/purchases")]
    [Authorize(Policy = "Permission:FreelancePlans.Manage")]
    public Task<IActionResult> PurchaseFreelance(FreelancePurchaseRequest request, CancellationToken ct) =>
        Task.FromResult<IActionResult>(Problem(statusCode: 410, title: "Freelance purchasing is disabled pending Sales/Payments integration."));
```
Service entry points: direct auth/token/410 adapter. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
POST /api/freelance-plans/purchases HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
Content-Type: application/json

{
  "memberId": "11111111-1111-4111-8111-111111111111",
  "membershipPlanId": "11111111-1111-4111-8111-111111111111",
  "branchId": "11111111-1111-4111-8111-111111111111",
  "existingSubscriptionId": null
}
```
Schema-shaped response (not an executed response):
```json
{
  "type": "about:blank",
  "title": "Freelance purchasing is disabled pending Sales/Payments integration.",
  "status": 410
}
```
<a id="get-api-freelance-plans-id-guid-report"></a>
## GET /api/freelance-plans/{id:guid}/report
Source: `CampionsSystem.API/Controllers/OperationsController.cs:95`, action `FreelanceReport`.
Status: **VERIFIED FROM CODE / working-tree snapshot**. Coaching actions are branch-only/uncommitted; other routes must be compared with stable baseline catalog.
Auth: Bearer authentication required. Exact controller policies (AND): `Permission:FreelancePlans.View`.
Scope/actor: apply module rules above and [guard evidence](rules/Operations.md). Legacy narrow compatibility operations: activity assignment/purchase use activity branch; shared coach profile percentage management passes null branch; class session/attendance uses actual group/session branch; reports use explicit branch/period; Freelance definitions use plan scope. No new Coach owner-only mobile contract.
### Path/query inputs
| Name / location | Required | Exact OpenAPI type/constraints | Declared default |
|---|---|---|---|
| `id` / path | true | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `branchId` / query | false | `{"type":"string","format":"uuid"}` | See query DTO defaults / guard evidence |
| `from` / query | false | `{"type":"string","format":"date-time"}` | See query DTO defaults / guard evidence |
| `until` / query | false | `{"type":"string","format":"date-time"}` | See query DTO defaults / guard evidence |
### JSON body and response
Body: none; do not invent a payload.
Success: **200**, body [IReadOnlyList<Operations.FreelancePlanReport>](../DTO_AND_ENUM_REFERENCE.md#system-collections-generic-ireadonlylist-campionssystem-bll-operations-freelanceplanreport). No generic wrapper; arrays/primitives remain direct JSON.
Errors/envelope: 400 BusinessRuleException; 401/403 auth/scope; 404 BranchNotFoundException; 409 unique/serialization/deadlock mapped by controller; 500 unexpected. Missing legacy operation resources often deliberately use 400 business messages rather than 404.  See master error rules; check-in/trial can deny inside 200.
Concurrency/idempotency: No general operation key in this action contract. No client version field on this action unless linked contract says otherwise. No ETag/If-Match. Backend locks/uniqueness may still return conflict.
Actual action adapter:
```csharp
[HttpGet("freelance-plans/{id:guid}/report")]
    [Authorize(Policy = "Permission:FreelancePlans.View")]
    public Task<IActionResult> FreelanceReport(Guid id, Guid branchId, DateTimeOffset from, DateTimeOffset until, CancellationToken ct) =>
        Execute(() => reports.FreelanceAsync(id, branchId, from, until, ct));
```
Service entry points: `CampionsSystem.BLL.Operations.CoachReportService.FreelanceAsync`. Required frontend behavior: show authoritative returned state, maintain scope, handle validation/denial/conflict; apply [workflow rules](../../FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md). Unknown deployment/grants/fixture state remains **UNKNOWN**, not inferred from examples.
### FAKE request/response example
Fixture prerequisites: active authorized user and referenced IDs, permitted lifecycle state, and operational setup/Shift where required. Every UUID here is a placeholder; replace different entity kinds with their own real test IDs.
```http
GET /api/freelance-plans/11111111-1111-4111-8111-111111111111/report HTTP/1.1
Host: <configured-api-host>
Authorization: Bearer <TOKEN_PLACEHOLDER>
```
Schema-shaped response (not an executed response):
```json
[]
```
