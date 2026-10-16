# Shifts API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-shifts"></a>
## POST /api/shifts
Action: `Shifts.Open`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:8`.
Authorization: Bearer authentication; `Permission:Shifts.Manage`.

Request body: `CampionsSystem.BLL.Shifts.OpenShiftRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-shifts-id-guid-close"></a>
## POST /api/shifts/{id:guid}/close
Action: `Shifts.Close`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:10`.
Authorization: Bearer authentication; `Permission:Shifts.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Shifts.CloseShiftRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-shifts-id-guid-close-override"></a>
## POST /api/shifts/{id:guid}/close-override
Action: `Shifts.CloseOverride`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:12`.
Authorization: Bearer authentication; `Permission:Shifts.CloseOthers`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Shifts.OverrideCloseShiftRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-shifts-current"></a>
## GET /api/shifts/current
Action: `Shifts.Current`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:14`.
Authorization: Bearer authentication; `Permission:Shifts.View`.

Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftDetails?`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-shifts"></a>
## GET /api/shifts
Action: `Shifts.History`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:16`.
Authorization: Bearer authentication; `Permission:Shifts.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Shifts.ShiftHistoryRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ShiftDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-shifts-id-guid"></a>
## GET /api/shifts/{id:guid}
Action: `Shifts.Get`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:18`.
Authorization: Bearer authentication; `Permission:Shifts.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-shifts-id-guid-summary"></a>
## GET /api/shifts/{id:guid}/summary
Action: `Shifts.Summary`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:20`.
Authorization: Bearer authentication; `Permission:Shifts.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftFinancialSummary`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-shifts-id-guid-transactions"></a>
## GET /api/shifts/{id:guid}/transactions
Action: `Shifts.Transactions`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:22`.
Authorization: Bearer authentication; `Permission:Shifts.View` AND `Permission:Vault.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ShiftFinancialTransactionDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-shifts-id-guid-branch-transactions"></a>
## GET /api/shifts/{id:guid}/branch-transactions
Action: `Shifts.BranchTransactions`; source: `CampionsSystem.API/Controllers/ShiftsController.cs:25`.
Authorization: Bearer authentication; `Permission:Shifts.View` AND `Permission:Vault.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ShiftBranchTransactionDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
