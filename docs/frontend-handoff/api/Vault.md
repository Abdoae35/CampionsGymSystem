# Vault API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="post-api-vault-setup"></a>
## POST /api/vault/setup
Action: `Vault.Setup`; source: `CampionsSystem.API/Controllers/VaultController.cs:8`.
Authorization: Bearer authentication; `Permission:Vault.Manage`.

Request body: `CampionsSystem.BLL.Shifts.SetupDrawerRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.DrawerPosition`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-vault-position"></a>
## GET /api/vault/position
Action: `Vault.Position`; source: `CampionsSystem.API/Controllers/VaultController.cs:10`.
Authorization: Bearer authentication; `Permission:Vault.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.DrawerPosition`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-vault-counts"></a>
## POST /api/vault/counts
Action: `Vault.Count`; source: `CampionsSystem.API/Controllers/VaultController.cs:12`.
Authorization: Bearer authentication; `Permission:Vault.Manage`.

Request body: `CampionsSystem.BLL.Shifts.CountCashRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.CashCountDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-vault-counts"></a>
## GET /api/vault/counts
Action: `Vault.Counts`; source: `CampionsSystem.API/Controllers/VaultController.cs:14`.
Authorization: Bearer authentication; `Permission:Vault.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Shifts.VaultHistoryRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.CashCountDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-vault-movements"></a>
## GET /api/vault/movements
Action: `Vault.Movements`; source: `CampionsSystem.API/Controllers/VaultController.cs:16`.
Authorization: Bearer authentication; `Permission:Vault.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Shifts.VaultHistoryRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.FinancialMovementDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-vault-withdrawals"></a>
## POST /api/vault/withdrawals
Action: `Vault.Withdraw`; source: `CampionsSystem.API/Controllers/VaultController.cs:18`.
Authorization: Bearer authentication; `Permission:Vault.Withdraw`.

Request body: `CampionsSystem.BLL.Shifts.WithdrawalRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Shifts.CashMovementDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-vault-summary"></a>
## GET /api/vault/summary
Action: `Vault.Summary`; source: `CampionsSystem.API/Controllers/VaultController.cs:20`.
Authorization: Bearer authentication; `Permission:Vault.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
| `from` | query | `DateOnly?` | — |
| `to` | query | `DateOnly?` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.BranchFinancialSummary`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-vault-audit"></a>
## GET /api/vault/audit
Action: `Vault.Audit`; source: `CampionsSystem.API/Controllers/VaultController.cs:22`.
Authorization: Bearer authentication; `Permission:Vault.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
| `entityId` | query | `Guid?` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.FinancialAuditDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
