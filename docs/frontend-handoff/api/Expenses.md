# Expenses API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-expenses"></a>
## POST /api/expenses
Action: `Expenses.Paid`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:8`.
Authorization: Bearer authentication; `Permission:Expenses.Create`.

Request body: `CampionsSystem.BLL.Shifts.PaidExpenseRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ExpenseDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-expenses"></a>
## GET /api/expenses
Action: `Expenses.History`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:10`.
Authorization: Bearer authentication; `Permission:Expenses.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Shifts.VaultHistoryRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-expenses-id-guid"></a>
## GET /api/expenses/{id:guid}
Action: `Expenses.Get`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:12`.
Authorization: Bearer authentication; `Permission:Expenses.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ExpenseDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-expenses-id-guid-returns"></a>
## POST /api/expenses/{id:guid}/returns
Action: `Expenses.Return`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:14`.
Authorization: Bearer authentication; `Permission:Expenses.Reverse`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Shifts.ReverseExpenseRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ExpenseReversalDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-expenses-id-guid-returns"></a>
## GET /api/expenses/{id:guid}/returns
Action: `Expenses.Returns`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:16`.
Authorization: Bearer authentication; `Permission:Expenses.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseReversalDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-expenses-categories"></a>
## GET /api/expenses/categories
Action: `Expenses.Categories`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:18`.
Authorization: Bearer authentication; `Permission:Expenses.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseCategoryDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-expenses-categories"></a>
## POST /api/expenses/categories
Action: `Expenses.Category`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:20`.
Authorization: Bearer authentication; `Permission:Expenses.Manage`.

Request body: `CampionsSystem.BLL.Shifts.ExpenseCategoryRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ExpenseCategoryDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-expenses-categories-id-guid"></a>
## PUT /api/expenses/categories/{id:guid}
Action: `Expenses.Category`; source: `CampionsSystem.API/Controllers/ExpensesController.cs:22`.
Authorization: Bearer authentication; `Permission:Expenses.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Shifts.ExpenseCategoryRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ExpenseCategoryDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
