# Sales API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-sales"></a>
## POST /api/sales
Action: `Sales.Create`; source: `CampionsSystem.API/Controllers/SalesController.cs:8`.
Authorization: Bearer authentication; `Permission:Payments.Create`.

Request body: `CampionsSystem.BLL.Sales.CreateSaleRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Sales.SaleReceipt`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-sales"></a>
## GET /api/sales
Action: `Sales.History`; source: `CampionsSystem.API/Controllers/SalesController.cs:10`.
Authorization: Bearer authentication; `Permission:Payments.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Sales.SaleHistoryRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Sales.SaleHistory`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-sales-id-guid"></a>
## GET /api/sales/{id:guid}
Action: `Sales.Details`; source: `CampionsSystem.API/Controllers/SalesController.cs:12`.
Authorization: Bearer authentication; `Permission:Payments.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Sales.SaleDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-sales-id-guid-payments"></a>
## POST /api/sales/{id:guid}/payments
Action: `Sales.Pay`; source: `CampionsSystem.API/Controllers/SalesController.cs:14`.
Authorization: Bearer authentication; `Permission:Payments.Create`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Sales.RecordPaymentRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Sales.PaymentDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-sales-id-guid-payments"></a>
## GET /api/sales/{id:guid}/payments
Action: `Sales.Payments`; source: `CampionsSystem.API/Controllers/SalesController.cs:16`.
Authorization: Bearer authentication; `Permission:Payments.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Sales.PaymentHistory`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-sales-id-guid-installments"></a>
## GET /api/sales/{id:guid}/installments
Action: `Sales.Installments`; source: `CampionsSystem.API/Controllers/SalesController.cs:18`.
Authorization: Bearer authentication; `Permission:Payments.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.InstallmentDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-sales-id-guid-refunds"></a>
## POST /api/sales/{id:guid}/refunds
Action: `Sales.Refund`; source: `CampionsSystem.API/Controllers/SalesController.cs:20`.
Authorization: Bearer authentication; `Permission:Payments.Refund`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Sales.RefundRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Sales.RefundDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-sales-id-guid-refunds"></a>
## GET /api/sales/{id:guid}/refunds
Action: `Sales.Refunds`; source: `CampionsSystem.API/Controllers/SalesController.cs:22`.
Authorization: Bearer authentication; `Permission:Payments.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Sales.RefundHistory`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-sales-id-guid-quick-cancel"></a>
## POST /api/sales/{id:guid}/quick-cancel
Action: `Sales.QuickCancel`; source: `CampionsSystem.API/Controllers/SalesController.cs:24`.
Authorization: Bearer authentication; `Permission:Sales.QuickCancel`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Sales.QuickCancelRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Sales.RefundDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-sales-branch-totals"></a>
## GET /api/sales/branch-totals
Action: `Sales.Totals`; source: `CampionsSystem.API/Controllers/SalesController.cs:26`.
Authorization: Bearer authentication; `Permission:Payments.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
| `from` | query | `DateOnly?` | — |
| `to` | query | `DateOnly?` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Sales.BranchFinancialTotals`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
