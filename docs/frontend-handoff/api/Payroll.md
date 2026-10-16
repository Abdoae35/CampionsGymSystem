# Payroll API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-payroll"></a>
## GET /api/payroll
Action: `Payroll.History`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:52`.
Authorization: Bearer authentication; `Permission:Payroll.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
| `employeeId` | query | `Guid?` | — |
| `monthId` | query | `Guid?` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.PayrollDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-payroll-commission-recognitions"></a>
## POST /api/payroll/commission-recognitions
Action: `Payroll.Recognize`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:54`.
Authorization: Bearer authentication; `Permission:Payroll.Manage`.

Request body: `CampionsSystem.BLL.MonthlyFinance.RecognizePayrollCommissionsRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.MonthlyFinance.PayrollDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-payroll-id-guid-payments"></a>
## POST /api/payroll/{id:guid}/payments
Action: `Payroll.Pay`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:56`.
Authorization: Bearer authentication; `Permission:Payroll.RecordPayment`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.MonthlyFinance.RecordPayrollPaymentRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.MonthlyFinance.PayrollPaymentDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-payroll-id-guid-payments"></a>
## GET /api/payroll/{id:guid}/payments
Action: `Payroll.Payments`; source: `CampionsSystem.API/Controllers/MonthlyFinanceController.cs:58`.
Authorization: Bearer authentication; `Permission:Payroll.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.PayrollPaymentDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
