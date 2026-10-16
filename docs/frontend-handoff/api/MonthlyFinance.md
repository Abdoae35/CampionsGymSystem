# MonthlyFinance API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-financial-months"></a>
## POST /api/financial-months
Action: `MonthlyFinance.Open`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:9`.
Authorization: Bearer authentication; `Permission:FinancialMonths.Manage`.

Request body: `CampionsSystem.BLL.MonthlyFinance.OpenFinancialMonthRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-financial-months"></a>
## GET /api/financial-months
Action: `MonthlyFinance.History`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:11`.
Authorization: Bearer authentication; `Permission:FinancialMonths.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-financial-months-id-guid"></a>
## GET /api/financial-months/{id:guid}
Action: `MonthlyFinance.Get`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:13`.
Authorization: Bearer authentication; `Permission:FinancialMonths.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-financial-months-id-guid-close"></a>
## POST /api/financial-months/{id:guid}/close
Action: `MonthlyFinance.Close`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:15`.
Authorization: Bearer authentication; `Permission:FinancialMonths.Close`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.MonthlyFinance.CloseFinancialMonthRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-financial-months-id-guid-reopen"></a>
## POST /api/financial-months/{id:guid}/reopen
Action: `MonthlyFinance.Reopen`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:17`.
Authorization: Bearer authentication; `Permission:FinancialMonths.Reopen`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.MonthlyFinance.ReopenFinancialMonthRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-financial-months-id-guid-report"></a>
## GET /api/financial-months/{id:guid}/report
Action: `MonthlyFinance.Report`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:19`.
Authorization: Bearer authentication; `Permission:FinancialMonths.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `revision` | query | `int?` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.MonthlyReport`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-financial-months-id-guid-transactions"></a>
## GET /api/financial-months/{id:guid}/transactions
Action: `MonthlyFinance.Transactions`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:21`.
Authorization: Bearer authentication; `Permission:FinancialMonths.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `revision` | query | `int?` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.FinancialTransactionReportRow>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-financial-months-id-guid-revisions"></a>
## GET /api/financial-months/{id:guid}/revisions
Action: `MonthlyFinance.Revisions`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:23`.
Authorization: Bearer authentication; `Permission:FinancialMonths.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.MonthlyFinance.RevisionDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-financial-months-id-guid-events"></a>
## GET /api/financial-months/{id:guid}/events
Action: `MonthlyFinance.Events`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:25`.
Authorization: Bearer authentication; `Permission:FinancialMonths.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.MonthEventDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-financial-months-id-guid-export"></a>
## GET /api/financial-months/{id:guid}/export
Action: `MonthlyFinance.Export`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:27`.
Authorization: Bearer authentication; `Permission:FinancialMonths.Export`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `revision` | query | `int?` | — |
Request body: none.
Success: XLSX binary download with `X-Workbook-SHA256` header.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-financial-months-id-guid-employees-employeeid-guid-salary"></a>
## GET /api/financial-months/{id:guid}/employees/{employeeId:guid}/salary
Action: `MonthlyFinance.Salary`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:34`.
Authorization: Bearer authentication; `Permission:Salaries.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `employeeId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.SalaryEntitlement`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-financial-months-consolidated"></a>
## GET /api/financial-months/consolidated
Action: `MonthlyFinance.Consolidated`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:36`.
Authorization: Bearer authentication; `Permission:FinancialMonths.ViewConsolidated`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `monthIds` | query | `Guid[]` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.ConsolidatedMonthSummary`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-financial-months-transactions-ledgerid-guid-original-occurrence"></a>
## POST /api/financial-months/transactions/{ledgerId:guid}/original-occurrence
Action: `MonthlyFinance.Occurrence`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:38`.
Authorization: Bearer authentication; `Permission:FinancialMonths.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `ledgerId` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.MonthlyFinance.RecordOccurrenceRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `System.Guid`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
