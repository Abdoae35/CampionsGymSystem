# Commissions API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-commissions-rules"></a>
## POST /api/commissions/rules
Action: `Commissions.Configure`; source: `CampionsSystem.API/Controllers/CommissionsController.cs:8`.
Authorization: Bearer authentication; `Permission:Commission.ManageRules`.

Request body: `CampionsSystem.BLL.Commissions.ConfigureCommissionRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Commissions.CommissionRuleDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-commissions-rules"></a>
## GET /api/commissions/rules
Action: `Commissions.Rules`; source: `CampionsSystem.API/Controllers/CommissionsController.cs:10`.
Authorization: Bearer authentication; `Permission:Commission.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `employeeId` | query | `Guid?` | — |
| `branchId` | query | `Guid?` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionRuleDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-commissions-periods"></a>
## POST /api/commissions/periods
Action: `Commissions.Open`; source: `CampionsSystem.API/Controllers/CommissionsController.cs:13`.
Authorization: Bearer authentication; `Permission:Commission.ClosePeriod`.

Request body: `CampionsSystem.BLL.Commissions.OpenCommissionPeriodRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Commissions.CommissionPeriodDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-commissions-periods"></a>
## GET /api/commissions/periods
Action: `Commissions.History`; source: `CampionsSystem.API/Controllers/CommissionsController.cs:15`.
Authorization: Bearer authentication; `Permission:Commission.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionPeriodDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-commissions-periods-id-guid-finalize"></a>
## POST /api/commissions/periods/{id:guid}/finalize
Action: `Commissions.Finalize`; source: `CampionsSystem.API/Controllers/CommissionsController.cs:18`.
Authorization: Bearer authentication; `Permission:Commission.ClosePeriod`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Commissions.FinalizeCommissionPeriodRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Commissions.CommissionPeriodDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-commissions-periods-id-guid"></a>
## GET /api/commissions/periods/{id:guid}
Action: `Commissions.Progress`; source: `CampionsSystem.API/Controllers/CommissionsController.cs:20`.
Authorization: Bearer authentication; `Permission:Commission.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Commissions.CommissionProgress`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-commissions-periods-id-guid-employees-employeeid-guid"></a>
## GET /api/commissions/periods/{id:guid}/employees/{employeeId:guid}
Action: `Commissions.Employee`; source: `CampionsSystem.API/Controllers/CommissionsController.cs:23`.
Authorization: Bearer authentication; `Permission:Commission.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `employeeId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Commissions.EmployeeCommissionStatement`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-commissions-periods-id-guid-sources"></a>
## GET /api/commissions/periods/{id:guid}/sources
Action: `Commissions.Sources`; source: `CampionsSystem.API/Controllers/CommissionsController.cs:25`.
Authorization: Bearer authentication; `Permission:Commission.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `employeeId` | query | `Guid?` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionSourceLine>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-commissions-periods-id-guid-refund-allocations"></a>
## GET /api/commissions/periods/{id:guid}/refund-allocations
Action: `Commissions.RefundAllocations`; source: `CampionsSystem.API/Controllers/CommissionsController.cs:28`.
Authorization: Bearer authentication; `Permission:Commission.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionRefundSource>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
