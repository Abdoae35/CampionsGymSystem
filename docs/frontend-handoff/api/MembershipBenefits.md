# MembershipBenefits API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-membership-benefits"></a>
## GET /api/membership-benefits
Action: `MembershipBenefits.List`; source: `CampionsSystem.API/Controllers/MembershipBenefitsController.cs:8`.
Authorization: Bearer authentication; `Permission:MembershipPlans.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Plans.CatalogListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Plans.CatalogListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-membership-benefits-id-guid"></a>
## GET /api/membership-benefits/{id:guid}
Action: `MembershipBenefits.Get`; source: `CampionsSystem.API/Controllers/MembershipBenefitsController.cs:10`.
Authorization: Bearer authentication; `Permission:MembershipPlans.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Plans.CatalogDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-membership-benefits"></a>
## POST /api/membership-benefits
Action: `MembershipBenefits.Create`; source: `CampionsSystem.API/Controllers/MembershipBenefitsController.cs:12`.
Authorization: Bearer authentication; `Permission:MembershipPlans.Manage`.

Request body: `CampionsSystem.BLL.Plans.SaveBenefitRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Plans.CatalogDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-membership-benefits-id-guid"></a>
## PUT /api/membership-benefits/{id:guid}
Action: `MembershipBenefits.Update`; source: `CampionsSystem.API/Controllers/MembershipBenefitsController.cs:14`.
Authorization: Bearer authentication; `Permission:MembershipPlans.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Plans.SaveBenefitRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Plans.CatalogDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="patch-api-membership-benefits-id-guid-status"></a>
## PATCH /api/membership-benefits/{id:guid}/status
Action: `MembershipBenefits.Status`; source: `CampionsSystem.API/Controllers/MembershipBenefitsController.cs:16`.
Authorization: Bearer authentication; `Permission:MembershipPlans.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Plans.ChangePlanStatusRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Plans.CatalogDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
