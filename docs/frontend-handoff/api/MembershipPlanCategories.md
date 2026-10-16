# MembershipPlanCategories API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-membership-plan-categories"></a>
## GET /api/membership-plan-categories
Action: `MembershipPlanCategories.List`; source: `CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs:8`.
Authorization: Bearer authentication; `Permission:MembershipPlans.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Plans.CatalogListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Plans.CatalogListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-membership-plan-categories"></a>
## POST /api/membership-plan-categories
Action: `MembershipPlanCategories.Create`; source: `CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs:10`.
Authorization: Bearer authentication; `Permission:MembershipPlans.Manage`.

Request body: `CampionsSystem.BLL.Plans.SaveCategoryRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Plans.CatalogDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-membership-plan-categories-id-guid"></a>
## PUT /api/membership-plan-categories/{id:guid}
Action: `MembershipPlanCategories.Update`; source: `CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs:12`.
Authorization: Bearer authentication; `Permission:MembershipPlans.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Plans.SaveCategoryRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Plans.CatalogDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="patch-api-membership-plan-categories-id-guid-status"></a>
## PATCH /api/membership-plan-categories/{id:guid}/status
Action: `MembershipPlanCategories.Status`; source: `CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs:14`.
Authorization: Bearer authentication; `Permission:MembershipPlans.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Plans.ChangePlanStatusRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Plans.CatalogDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
