# AccessAdministration API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-access-administration-permissions"></a>
## GET /api/access-administration/permissions
Action: `AccessAdministration.Catalog`; source: `CampionsSystem.API/Controllers/AccessAdministrationController.cs:17`.
Authorization: Bearer authentication; `OwnerAccessAdministration` AND `Permission:Roles.View`.

Request body: none.
Success: HTTP 200 `PermissionModule[]`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `401`, `403`, `400`, `404`, `409`, `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-access-administration-roles"></a>
## GET /api/access-administration/roles
Action: `AccessAdministration.Roles`; source: `CampionsSystem.API/Controllers/AccessAdministrationController.cs:20`.
Authorization: Bearer authentication; `OwnerAccessAdministration` AND `Permission:Roles.View`.

Request body: none.
Success: HTTP 200 `AccessRole[]`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `401`, `403`, `400`, `404`, `409`, `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-access-administration-roles-roleid-guid"></a>
## GET /api/access-administration/roles/{roleId:guid}
Action: `AccessAdministration.Role`; source: `CampionsSystem.API/Controllers/AccessAdministrationController.cs:23`.
Authorization: Bearer authentication; `OwnerAccessAdministration` AND `Permission:Roles.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `roleId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `RoleAccessDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `401`, `403`, `400`, `404`, `409`, `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-access-administration-users"></a>
## GET /api/access-administration/users
Action: `AccessAdministration.Users`; source: `CampionsSystem.API/Controllers/AccessAdministrationController.cs:26`.
Authorization: Bearer authentication; `OwnerAccessAdministration` AND `Permission:Users.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `query` | query | `Authentication.AccessInventoryQuery` | — |
Request body: none.
Success: HTTP 200 `AccessPage<AccessUserItem>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `401`, `403`, `400`, `404`, `409`, `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-access-administration-users-userid-guid"></a>
## GET /api/access-administration/users/{userId:guid}
Action: `AccessAdministration.UserAccess`; source: `CampionsSystem.API/Controllers/AccessAdministrationController.cs:29`.
Authorization: Bearer authentication; `OwnerAccessAdministration` AND `Permission:Users.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `userId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `UserAccessDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `401`, `403`, `400`, `404`, `409`, `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-access-administration-history"></a>
## GET /api/access-administration/history
Action: `AccessAdministration.History`; source: `CampionsSystem.API/Controllers/AccessAdministrationController.cs:32`.
Authorization: Bearer authentication; `OwnerAccessAdministration` AND `Permission:Users.View` AND `Permission:Roles.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `query` | query | `Authentication.AccessAuditQuery` | — |
Request body: none.
Success: HTTP 200 `AccessPage<AccessAuditDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `401`, `403`, `400`, `404`, `409`, `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-access-administration-users-userid-guid-roles-roleid-guid"></a>
## PUT /api/access-administration/users/{userId:guid}/roles/{roleId:guid}
Action: `AccessAdministration.Membership`; source: `CampionsSystem.API/Controllers/AccessAdministrationController.cs:35`.
Authorization: Bearer authentication; `OwnerAccessAdministration` AND `Permission:Users.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `userId` | path | `Guid` | — |
| `roleId` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Authentication.RoleMembershipRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `AccessChangeResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `401`, `403`, `400`, `404`, `409`, `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-access-administration-roles-roleid-guid-permissions-permissionid-guid"></a>
## PUT /api/access-administration/roles/{roleId:guid}/permissions/{permissionId:guid}
Action: `AccessAdministration.Grant`; source: `CampionsSystem.API/Controllers/AccessAdministrationController.cs:39`.
Authorization: Bearer authentication; `OwnerAccessAdministration` AND `Permission:Roles.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `roleId` | path | `Guid` | — |
| `permissionId` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Authentication.RoleGrantRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `AccessChangeResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `401`, `403`, `400`, `404`, `409`, `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-access-administration-users-userid-guid-permissions-permissionid-guid"></a>
## PUT /api/access-administration/users/{userId:guid}/permissions/{permissionId:guid}
Action: `AccessAdministration.Override`; source: `CampionsSystem.API/Controllers/AccessAdministrationController.cs:43`.
Authorization: Bearer authentication; `OwnerAccessAdministration` AND `Permission:Users.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `userId` | path | `Guid` | — |
| `permissionId` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Authentication.PermissionOverrideRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `AccessChangeResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `401`, `403`, `400`, `404`, `409`, `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
