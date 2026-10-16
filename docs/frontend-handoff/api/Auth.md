# Auth API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-auth-login"></a>
## POST /api/auth/login
Action: `Auth.Login`; source: `CampionsSystem.API/Controllers/AuthController.cs:13`.
Authorization: Anonymous login.

Request body: `CampionsSystem.BLL.Authentication.LoginRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Authentication.LoginResponse`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-auth-me"></a>
## GET /api/auth/me
Action: `Auth.Me`; source: `CampionsSystem.API/Controllers/AuthController.cs:22`.
Authorization: Bearer authentication; authenticated user.

Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Authentication.CurrentUserContext`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
