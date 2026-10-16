# Operations API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-activities-coaches"></a>
## POST /api/activities/coaches
Action: `Operations.CreateCoach`; source: `CampionsSystem.API/Controllers/OperationsController.cs:16`.
Authorization: Bearer authentication; `Permission:Activities.ManageCoaches`.

Request body: `CampionsSystem.BLL.Operations.CreateActivityCoachRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `<anonymous type: System.Guid Id>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-activities-coaches-id-guid-percentage"></a>
## PUT /api/activities/coaches/{id:guid}/percentage
Action: `Operations.Percentage`; source: `CampionsSystem.API/Controllers/OperationsController.cs:21`.
Authorization: Bearer authentication; `Permission:Activities.ManageCoaches`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.API.Controllers.OperationsController.PercentageRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `<anonymous type: bool Updated>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-activities-id-guid-coach-assignments"></a>
## POST /api/activities/{id:guid}/coach-assignments
Action: `Operations.Assign`; source: `CampionsSystem.API/Controllers/OperationsController.cs:26`.
Authorization: Bearer authentication; `Permission:Activities.ManageCoaches`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.API.Controllers.OperationsController.AssignmentRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `<anonymous type: System.Guid Id>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="delete-api-activities-coach-assignments-id-guid"></a>
## DELETE /api/activities/coach-assignments/{id:guid}
Action: `Operations.Unassign`; source: `CampionsSystem.API/Controllers/OperationsController.cs:31`.
Authorization: Bearer authentication; `Permission:Activities.ManageCoaches`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `<anonymous type: bool Updated>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-activities-purchases"></a>
## POST /api/activities/purchases
Action: `Operations.PurchaseActivity`; source: `CampionsSystem.API/Controllers/OperationsController.cs:36`.
Authorization: Bearer authentication; `Permission:Activities.Manage`.

Request body: `CampionsSystem.BLL.Operations.ActivityPurchaseRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Operations.PurchaseResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-activities-coach-revenue-periodid-guid"></a>
## GET /api/activities/coach-revenue/{periodId:guid}
Action: `Operations.ActivityRevenue`; source: `CampionsSystem.API/Controllers/OperationsController.cs:41`.
Authorization: Bearer authentication; `Permission:Activities.ViewCoachRevenue`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `periodId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Operations.ActivityCoachReport>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-activities-coach-revenue-periodid-guid-finalize"></a>
## POST /api/activities/coach-revenue/{periodId:guid}/finalize
Action: `Operations.FinalizeActivity`; source: `CampionsSystem.API/Controllers/OperationsController.cs:45`.
Authorization: Bearer authentication; `Permission:Commission.ClosePeriod`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `periodId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `<anonymous type: bool Finalized>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-classes-sessions"></a>
## POST /api/classes/sessions
Action: `Operations.CreateSession`; source: `CampionsSystem.API/Controllers/OperationsController.cs:50`.
Authorization: Bearer authentication; `Permission:Classes.Manage`.

Request body: `CampionsSystem.BLL.Operations.SaveClassSessionRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 410; disabled legacy write.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-classes-sessions-id-guid"></a>
## PUT /api/classes/sessions/{id:guid}
Action: `Operations.UpdateSession`; source: `CampionsSystem.API/Controllers/OperationsController.cs:55`.
Authorization: Bearer authentication; `Permission:Classes.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Operations.SaveClassSessionRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 410; disabled legacy write.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-classes-sessions-id-guid-attendance-memberid-guid"></a>
## POST /api/classes/sessions/{id:guid}/attendance/{memberId:guid}
Action: `Operations.Attendance`; source: `CampionsSystem.API/Controllers/OperationsController.cs:60`.
Authorization: Bearer authentication; `Permission:Classes.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `memberId` | path | `Guid` | — |
Request body: none.
Success: HTTP 410; disabled legacy write.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-classes-attendance-report"></a>
## GET /api/classes/attendance-report
Action: `Operations.ClassReport`; source: `CampionsSystem.API/Controllers/OperationsController.cs:65`.
Authorization: Bearer authentication; `Permission:Classes.ViewAttendanceReport`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid` | — |
| `from` | query | `DateTimeOffset` | — |
| `until` | query | `DateTimeOffset` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Operations.ClassCoachReport>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-freelance-plans"></a>
## POST /api/freelance-plans
Action: `Operations.CreatePlan`; source: `CampionsSystem.API/Controllers/OperationsController.cs:70`.
Authorization: Bearer authentication; `Permission:FreelancePlans.Manage`.

Request body: `CampionsSystem.BLL.Operations.SaveFreelancePlanRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `<anonymous type: System.Guid Id>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-freelance-plans-id-guid"></a>
## PUT /api/freelance-plans/{id:guid}
Action: `Operations.UpdatePlan`; source: `CampionsSystem.API/Controllers/OperationsController.cs:75`.
Authorization: Bearer authentication; `Permission:FreelancePlans.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Operations.SaveFreelancePlanRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `<anonymous type: System.Guid Id>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-freelance-plans-id-guid-eligible-members-memberid-guid"></a>
## PUT /api/freelance-plans/{id:guid}/eligible-members/{memberId:guid}
Action: `Operations.AddEligible`; source: `CampionsSystem.API/Controllers/OperationsController.cs:80`.
Authorization: Bearer authentication; `Permission:FreelancePlans.ManageEligibleMembers`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `memberId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `<anonymous type: bool Eligible>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="delete-api-freelance-plans-id-guid-eligible-members-memberid-guid"></a>
## DELETE /api/freelance-plans/{id:guid}/eligible-members/{memberId:guid}
Action: `Operations.RemoveEligible`; source: `CampionsSystem.API/Controllers/OperationsController.cs:85`.
Authorization: Bearer authentication; `Permission:FreelancePlans.ManageEligibleMembers`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `memberId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `<anonymous type: bool Eligible>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-freelance-plans-purchases"></a>
## POST /api/freelance-plans/purchases
Action: `Operations.PurchaseFreelance`; source: `CampionsSystem.API/Controllers/OperationsController.cs:90`.
Authorization: Bearer authentication; `Permission:FreelancePlans.Manage`.

Request body: `CampionsSystem.BLL.Operations.FreelancePurchaseRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 410; disabled legacy write.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-freelance-plans-id-guid-report"></a>
## GET /api/freelance-plans/{id:guid}/report
Action: `Operations.FreelanceReport`; source: `CampionsSystem.API/Controllers/OperationsController.cs:95`.
Authorization: Bearer authentication; `Permission:FreelancePlans.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `branchId` | query | `Guid` | — |
| `from` | query | `DateTimeOffset` | — |
| `until` | query | `DateTimeOffset` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Operations.FreelancePlanReport>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
