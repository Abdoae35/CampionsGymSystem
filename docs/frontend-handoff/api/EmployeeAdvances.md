# EmployeeAdvances API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-employee-advances"></a>
## POST /api/employee-advances
Action: `EmployeeAdvances.Paid`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:28`.
Authorization: Bearer authentication; `Permission:Advances.Create`.

Request body: `CampionsSystem.BLL.Shifts.AdvanceRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.AdvanceDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-employee-advances"></a>
## GET /api/employee-advances
Action: `EmployeeAdvances.History`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:30`.
Authorization: Bearer authentication; `Permission:Advances.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Shifts.VaultHistoryRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.AdvanceDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-employee-advances-id-guid"></a>
## GET /api/employee-advances/{id:guid}
Action: `EmployeeAdvances.Get`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:32`.
Authorization: Bearer authentication; `Permission:Advances.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.AdvanceDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
