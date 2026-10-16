# EmployeeBranches API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-employees-employeeid-guid-branch-assignment"></a>
## GET /api/employees/{employeeId:guid}/branch-assignment
Action: `EmployeeBranches.Current`; source: `CampionsSystem.API/Controllers/EmployeeBranchesController.cs:11`.
Authorization: Bearer authentication; `Permission:Employees.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `employeeId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Branches.BranchAssignmentDetails?`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-employees-employeeid-guid-branch-history"></a>
## GET /api/employees/{employeeId:guid}/branch-history
Action: `EmployeeBranches.History`; source: `CampionsSystem.API/Controllers/EmployeeBranchesController.cs:14`.
Authorization: Bearer authentication; `Permission:Employees.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `employeeId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Branches.BranchAssignmentDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-employees-employeeid-guid-assign-branch"></a>
## POST /api/employees/{employeeId:guid}/assign-branch
Action: `EmployeeBranches.Assign`; source: `CampionsSystem.API/Controllers/EmployeeBranchesController.cs:17`.
Authorization: Bearer authentication; `Permission:Employees.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `employeeId` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Branches.AssignBranchRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Branches.BranchAssignmentDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-employees-employeeid-guid-transfer-branch"></a>
## POST /api/employees/{employeeId:guid}/transfer-branch
Action: `EmployeeBranches.Transfer`; source: `CampionsSystem.API/Controllers/EmployeeBranchesController.cs:19`.
Authorization: Bearer authentication; `Permission:Employees.Transfer`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `employeeId` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Branches.TransferBranchRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Branches.BranchAssignmentDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
