# Branches API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-branches"></a>
## GET /api/branches
Action: `Branches.List`; source: `CampionsSystem.API/Controllers/BranchesController.cs:11`.
Authorization: Bearer authentication; `Permission:Branches.View`.

Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Branches.BranchDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-branches-id-guid"></a>
## GET /api/branches/{id:guid}
Action: `Branches.Get`; source: `CampionsSystem.API/Controllers/BranchesController.cs:14`.
Authorization: Bearer authentication; `Permission:Branches.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Branches.BranchDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-branches"></a>
## POST /api/branches
Action: `Branches.Create`; source: `CampionsSystem.API/Controllers/BranchesController.cs:17`.
Authorization: Bearer authentication; `Permission:Branches.Manage`.

Request body: `CampionsSystem.BLL.Branches.SaveBranchRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Branches.BranchDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-branches-id-guid"></a>
## PUT /api/branches/{id:guid}
Action: `Branches.Update`; source: `CampionsSystem.API/Controllers/BranchesController.cs:20`.
Authorization: Bearer authentication; `Permission:Branches.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Branches.SaveBranchRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Branches.BranchDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
