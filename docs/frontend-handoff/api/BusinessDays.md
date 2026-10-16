# BusinessDays API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-business-days-current"></a>
## GET /api/business-days/current
Action: `BusinessDays.Current`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:32`.
Authorization: Bearer authentication; `Permission:Shifts.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.BusinessDayDetails?`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-business-days"></a>
## POST /api/business-days
Action: `BusinessDays.Open`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:34`.
Authorization: Bearer authentication; `Permission:Shifts.Manage`.

Request body: `CampionsSystem.BLL.Shifts.OpenBusinessDayRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.BusinessDayDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-business-days-id-guid-close"></a>
## POST /api/business-days/{id:guid}/close
Action: `BusinessDays.Close`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:36`.
Authorization: Bearer authentication; `Permission:Shifts.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.BusinessDayDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
