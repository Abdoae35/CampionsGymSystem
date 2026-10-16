# Members API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-members"></a>
## GET /api/members
Action: `Members.List`; source: `CampionsSystem.API/Controllers/MembersController.cs:11`.
Authorization: Bearer authentication; `Permission:Members.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `request` | query | `Members.MemberListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Members.MemberListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-members-id-guid"></a>
## GET /api/members/{id:guid}
Action: `Members.Get`; source: `CampionsSystem.API/Controllers/MembersController.cs:13`.
Authorization: Bearer authentication; `Permission:Members.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Members.MemberDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-members"></a>
## POST /api/members
Action: `Members.Create`; source: `CampionsSystem.API/Controllers/MembersController.cs:15`.
Authorization: Bearer authentication; `Permission:Members.Create`.

Request body: `CampionsSystem.BLL.Members.CreateMemberRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Members.MemberDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-members-id-guid"></a>
## PUT /api/members/{id:guid}
Action: `Members.Update`; source: `CampionsSystem.API/Controllers/MembersController.cs:17`.
Authorization: Bearer authentication; `Permission:Members.Edit`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Members.UpdateMemberRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Members.MemberDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="patch-api-members-id-guid-status"></a>
## PATCH /api/members/{id:guid}/status
Action: `Members.Status`; source: `CampionsSystem.API/Controllers/MembersController.cs:19`.
Authorization: Bearer authentication; `Permission:Members.ChangeStatus`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Members.ChangeMemberStatusRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Members.MemberDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-members-id-guid-sales-assignment"></a>
## GET /api/members/{id:guid}/sales-assignment
Action: `Members.CurrentSales`; source: `CampionsSystem.API/Controllers/MembersController.cs:21`.
Authorization: Bearer authentication; `Permission:Members.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Members.SalesAssignmentDetails?`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-members-id-guid-sales-history"></a>
## GET /api/members/{id:guid}/sales-history
Action: `Members.SalesHistory`; source: `CampionsSystem.API/Controllers/MembersController.cs:23`.
Authorization: Bearer authentication; `Permission:Members.ViewSalesHistory`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Members.SalesAssignmentHistoryItem>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-members-id-guid-assign-sales"></a>
## POST /api/members/{id:guid}/assign-sales
Action: `Members.AssignSales`; source: `CampionsSystem.API/Controllers/MembersController.cs:25`.
Authorization: Bearer authentication; `Permission:Members.AssignSales`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Members.SalesAssignmentRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Members.MemberDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-members-id-guid-return-to-house"></a>
## POST /api/members/{id:guid}/return-to-house
Action: `Members.ReturnToHouse`; source: `CampionsSystem.API/Controllers/MembersController.cs:27`.
Authorization: Bearer authentication; `Permission:Members.AssignSales`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Members.ReturnToHouseRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Members.MemberDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
