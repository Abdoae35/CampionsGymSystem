# Subscriptions API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-members-memberid-guid-subscriptions"></a>
## GET /api/members/{memberId:guid}/subscriptions
Action: `Subscriptions.History`; source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:9`.
Authorization: Bearer authentication; `Permission:Subscriptions.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `memberId` | path | `Guid` | — |
| `request` | query | `Subscriptions.SubscriptionHistoryRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Subscriptions.SubscriptionHistory`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-members-memberid-guid-subscriptions-current"></a>
## GET /api/members/{memberId:guid}/subscriptions/current
Action: `Subscriptions.Current`; source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:12`.
Authorization: Bearer authentication; `Permission:Subscriptions.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `memberId` | path | `Guid` | — |
| `branchId` | query | `Guid?` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Subscriptions.MemberEntitlement`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-subscriptions-id-guid"></a>
## GET /api/subscriptions/{id:guid}
Action: `Subscriptions.Details`; source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:15`.
Authorization: Bearer authentication; `Permission:Subscriptions.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Subscriptions.SubscriptionDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-subscriptions-id-guid-freeze"></a>
## POST /api/subscriptions/{id:guid}/freeze
Action: `Subscriptions.Freeze`; source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:17`.
Authorization: Bearer authentication; `Permission:Subscriptions.Freeze`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Subscriptions.FreezeRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Subscriptions.FreezeDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-subscriptions-id-guid-freezes-freezeid-guid-complete"></a>
## POST /api/subscriptions/{id:guid}/freezes/{freezeId:guid}/complete
Action: `Subscriptions.Complete`; source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:20`.
Authorization: Bearer authentication; `Permission:Subscriptions.Freeze`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `freezeId` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Subscriptions.CompleteFreezeRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Subscriptions.FreezeDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-subscriptions-id-guid-custom-extension"></a>
## POST /api/subscriptions/{id:guid}/custom-extension
Action: `Subscriptions.Extend`; source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:23`.
Authorization: Bearer authentication; `Permission:Subscriptions.Extend`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Subscriptions.CustomExtensionRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Subscriptions.FreezeDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-employees-id-guid-membership-entitlement"></a>
## POST /api/employees/{id:guid}/membership-entitlement
Action: `Subscriptions.Employee`; source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:26`.
Authorization: Bearer authentication; `Permission:Subscriptions.Manage` AND `Permission:Employees.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Subscriptions.EmployeeEntitlementRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Subscriptions.SubscriptionGrantResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-subscriptions-official-holidays"></a>
## POST /api/subscriptions/official-holidays
Action: `Subscriptions.Holiday`; source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:29`.
Authorization: Bearer authentication; `Permission:Subscriptions.Extend` AND `Permission:Branches.AccessAll`.

Request body: `CampionsSystem.BLL.Subscriptions.OfficialHolidayRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Subscriptions.HolidayApplicationResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-subscriptions-official-holidays-id-guid-apply"></a>
## POST /api/subscriptions/official-holidays/{id:guid}/apply
Action: `Subscriptions.ApplyHoliday`; source: `CampionsSystem.API/Controllers/SubscriptionsController.cs:32`.
Authorization: Bearer authentication; `Permission:Subscriptions.Extend` AND `Permission:Branches.AccessAll`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Subscriptions.HolidayApplicationResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
