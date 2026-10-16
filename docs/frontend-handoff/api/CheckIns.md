# CheckIns API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-check-ins"></a>
## POST /api/check-ins
Action: `CheckIns.Create`; source: `CampionsSystem.API/Controllers/CheckInsController.cs:8`.
Authorization: Bearer authentication; `Permission:CheckIns.Create`.

Request body: `CampionsSystem.BLL.CheckIns.CreateCheckInRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.CheckIns.CheckInResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-check-ins-evaluate"></a>
## POST /api/check-ins/evaluate
Action: `CheckIns.Evaluate`; source: `CampionsSystem.API/Controllers/CheckInsController.cs:10`.
Authorization: Bearer authentication; `Permission:CheckIns.Create`.

Request body: `CampionsSystem.BLL.CheckIns.AccessRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.CheckIns.AccessDecision`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-check-ins"></a>
## GET /api/check-ins
Action: `CheckIns.History`; source: `CampionsSystem.API/Controllers/CheckInsController.cs:12`.
Authorization: Bearer authentication; `Permission:CheckIns.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `request` | query | `CheckIns.CheckInHistoryRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.CheckIns.CheckInHistory`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-check-ins-members"></a>
## GET /api/check-ins/members
Action: `CheckIns.Lookup`; source: `CampionsSystem.API/Controllers/CheckInsController.cs:14`.
Authorization: Bearer authentication; `Permission:CheckIns.Create`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `request` | query | `CheckIns.CheckInMemberSearchRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.CheckIns.CheckInMemberMatches`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-members-memberid-guid-check-ins-current-business-day"></a>
## GET /api/members/{memberId:guid}/check-ins/current-business-day
Action: `CheckIns.Today`; source: `CampionsSystem.API/Controllers/CheckInsController.cs:16`.
Authorization: Bearer authentication; `Permission:CheckIns.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `memberId` | path | `Guid` | — |
| `branchId` | query | `Guid?` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.CheckIns.CheckInHistory`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-members-memberid-guid-borrowed-gym-sessions"></a>
## GET /api/members/{memberId:guid}/borrowed-gym-sessions
Action: `CheckIns.Debts`; source: `CampionsSystem.API/Controllers/CheckInsController.cs:19`.
Authorization: Bearer authentication; `Permission:CheckIns.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `memberId` | path | `Guid` | — |
| `request` | query | `CheckIns.BorrowedDebtHistoryRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.CheckIns.BorrowedDebtHistory`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-check-ins-trials"></a>
## POST /api/check-ins/trials
Action: `CheckIns.Trial`; source: `CampionsSystem.API/Controllers/CheckInsController.cs:21`.
Authorization: Bearer authentication; `Permission:CheckIns.Create`.

Request body: `CampionsSystem.BLL.CheckIns.RedeemTrialRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.CheckIns.TrialCheckInResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
