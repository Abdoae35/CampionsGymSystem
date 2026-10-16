# EmployeeSalaries API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-employee-salaries-employeeid-guid-settings"></a>
## POST /api/employee-salaries/{employeeId:guid}/settings
Action: `EmployeeSalaries.Configure`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:44`.
Authorization: Bearer authentication; `Permission:Salaries.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `employeeId` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.MonthlyFinance.SetSalaryRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.SalarySettingDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-employee-salaries-employeeid-guid-settings"></a>
## GET /api/employee-salaries/{employeeId:guid}/settings
Action: `EmployeeSalaries.History`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:46`.
Authorization: Bearer authentication; `Permission:Salaries.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `employeeId` | path | `Guid` | — |
| `branchId` | query | `Guid?` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.MonthlyFinance.SalarySettingDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
