# MaintenancePlans API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-maintenance-plans"></a>
## POST /api/maintenance-plans
Action: `MaintenancePlans.Configure`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:64`.
Authorization: Bearer authentication; `Permission:Maintenance.Manage`.

Request body: `CampionsSystem.BLL.MonthlyFinance.MaintenancePlanRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.MaintenanceForecast`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-maintenance-plans"></a>
## GET /api/maintenance-plans
Action: `MaintenancePlans.Plans`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:66`.
Authorization: Bearer authentication; `Permission:Maintenance.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
| `asOf` | query | `DateOnly?` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.MaintenanceForecast>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-maintenance-plans-summary"></a>
## GET /api/maintenance-plans/summary
Action: `MaintenancePlans.Summary`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:68`.
Authorization: Bearer authentication; `Permission:Maintenance.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
| `asOf` | query | `DateOnly?` | — |
| `horizonDays` | query | `int` | `90` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.MaintenanceBranchSummary`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-maintenance-plans-reminders"></a>
## GET /api/maintenance-plans/reminders
Action: `MaintenancePlans.Reminders`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:71`.
Authorization: Bearer authentication; `Permission:Maintenance.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
| `asOf` | query | `DateOnly?` | — |
| `horizonDays` | query | `int` | `90` |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.MaintenanceForecast>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-maintenance-plans-id-guid-history"></a>
## GET /api/maintenance-plans/{id:guid}/history
Action: `MaintenancePlans.History`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:75`.
Authorization: Bearer authentication; `Permission:Maintenance.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.MaintenanceForecast>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
