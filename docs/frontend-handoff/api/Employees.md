# Employees API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-employees"></a>
## GET /api/employees
Action: `Employees.List`; source: `CampionsSystem.API/Controllers/EmployeesController.cs:11`.
Authorization: Bearer authentication; `Permission:Employees.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `request` | query | `Employees.EmployeeListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Employees.EmployeeListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-employees"></a>
## POST /api/employees
Action: `Employees.Create`; source: `CampionsSystem.API/Controllers/EmployeesController.cs:14`.
Authorization: Bearer authentication; `Permission:Employees.Manage`.

Request body: `CampionsSystem.BLL.Employees.CreateEmployeeRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Employees.EmployeeDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-employees-id-guid"></a>
## GET /api/employees/{id:guid}
Action: `Employees.Get`; source: `CampionsSystem.API/Controllers/EmployeesController.cs:17`.
Authorization: Bearer authentication; `Permission:Employees.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Employees.EmployeeDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-employees-id-guid"></a>
## PUT /api/employees/{id:guid}
Action: `Employees.Update`; source: `CampionsSystem.API/Controllers/EmployeesController.cs:20`.
Authorization: Bearer authentication; `Permission:Employees.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Employees.EmployeeProfileRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Employees.EmployeeDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="patch-api-employees-id-guid-status"></a>
## PATCH /api/employees/{id:guid}/status
Action: `Employees.Status`; source: `CampionsSystem.API/Controllers/EmployeesController.cs:23`.
Authorization: Bearer authentication; `Permission:Employees.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Employees.ChangeEmployeeStatusRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Employees.EmployeeDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-branches-branchid-guid-employees"></a>
## GET /api/branches/{branchId:guid}/employees
Action: `Employees.BranchEmployees`; source: `CampionsSystem.API/Controllers/EmployeesController.cs:26`.
Authorization: Bearer authentication; `Permission:Employees.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | path | `Guid` | — |
| `request` | query | `Employees.EmployeeListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Employees.EmployeeListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
