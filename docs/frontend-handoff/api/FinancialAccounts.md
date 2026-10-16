# FinancialAccounts API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-financial-accounts"></a>
## GET /api/financial-accounts
Action: `FinancialAccounts.Accounts`; source: `CampionsSystem.API/Controllers/FinancialAccountsController.cs:8`.
Authorization: Bearer authentication; `Permission:Payments.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.FinancialAccountDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-financial-accounts"></a>
## POST /api/financial-accounts
Action: `FinancialAccounts.Create`; source: `CampionsSystem.API/Controllers/FinancialAccountsController.cs:10`.
Authorization: Bearer authentication; `Permission:Branches.Manage` AND `Permission:Branches.AccessAll`.

Request body: `CampionsSystem.BLL.Sales.CreateFinancialAccountRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Sales.FinancialAccountDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
