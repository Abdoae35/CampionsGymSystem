# Crm API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-crm-my-follow-ups"></a>
## GET /api/crm/my-follow-ups
Action: `Crm.Mine`; source: `CampionsSystem.API/Controllers/CrmController.cs:9`.
Authorization: Bearer authentication; `Permission:CRM.ViewMemberFollowUps`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Crm.FollowUpListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Crm.FollowUpListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-crm-house-follow-ups"></a>
## GET /api/crm/house-follow-ups
Action: `Crm.House`; source: `CampionsSystem.API/Controllers/CrmController.cs:11`.
Authorization: Bearer authentication; `Permission:CRM.ManageHouseFollowUps`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Crm.FollowUpListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Crm.FollowUpListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-crm-members-id-guid-follow-ups"></a>
## POST /api/crm/members/{id:guid}/follow-ups
Action: `Crm.Open`; source: `CampionsSystem.API/Controllers/CrmController.cs:13`.
Authorization: Bearer authentication; `Permission:CRM.ManageMemberFollowUps`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Crm.MemberFollowUpRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `System.Guid`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-crm-member-follow-ups-id-guid-complete"></a>
## POST /api/crm/member-follow-ups/{id:guid}/complete
Action: `Crm.Complete`; source: `CampionsSystem.API/Controllers/CrmController.cs:15`.
Authorization: Bearer authentication; `Permission:CRM.ManageMemberFollowUps`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `System.Guid`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-crm-house-follow-ups-id-guid-assign"></a>
## POST /api/crm/house-follow-ups/{id:guid}/assign
Action: `Crm.Assign`; source: `CampionsSystem.API/Controllers/CrmController.cs:17`.
Authorization: Bearer authentication; `Permission:CRM.ManageHouseFollowUps`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Members.SalesAssignmentRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Members.MemberDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
