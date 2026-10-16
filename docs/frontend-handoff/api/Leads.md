# Leads API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-leads"></a>
## GET /api/leads
Action: `Leads.List`; source: `CampionsSystem.API/Controllers/LeadsController.cs:9`.
Authorization: Bearer authentication; `Permission:Leads.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Crm.LeadListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Crm.LeadListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-leads-my"></a>
## GET /api/leads/my
Action: `Leads.Mine`; source: `CampionsSystem.API/Controllers/LeadsController.cs:11`.
Authorization: Bearer authentication; `Permission:Leads.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Crm.LeadListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Crm.LeadListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-leads-lookup"></a>
## GET /api/leads/lookup
Action: `Leads.Lookup`; source: `CampionsSystem.API/Controllers/LeadsController.cs:13`.
Authorization: Bearer authentication; `Permission:Leads.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid` | — |
| `phone` | query | `string` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Crm.LeadCreationResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-leads-id-guid"></a>
## GET /api/leads/{id:guid}
Action: `Leads.Get`; source: `CampionsSystem.API/Controllers/LeadsController.cs:15`.
Authorization: Bearer authentication; `Permission:Leads.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Crm.LeadDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-leads"></a>
## POST /api/leads
Action: `Leads.Create`; source: `CampionsSystem.API/Controllers/LeadsController.cs:17`.
Authorization: Bearer authentication; `Permission:Leads.Manage`.

Request body: `CampionsSystem.BLL.Crm.CreateLeadRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Crm.LeadCreationResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-leads-bulk"></a>
## POST /api/leads/bulk
Action: `Leads.Bulk`; source: `CampionsSystem.API/Controllers/LeadsController.cs:19`.
Authorization: Bearer authentication; `Permission:Leads.Assign`.

Request body: `System.Text.Json.JsonElement[]` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.LeadCreationResult>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-leads-id-guid"></a>
## PUT /api/leads/{id:guid}
Action: `Leads.Update`; source: `CampionsSystem.API/Controllers/LeadsController.cs:21`.
Authorization: Bearer authentication; `Permission:Leads.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Crm.UpdateLeadRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Crm.LeadDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-leads-id-guid-assign"></a>
## POST /api/leads/{id:guid}/assign
Action: `Leads.Assign`; source: `CampionsSystem.API/Controllers/LeadsController.cs:23`.
Authorization: Bearer authentication; `Permission:Leads.Assign`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Crm.LeadAssignmentRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Crm.LeadDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-leads-id-guid-assignment"></a>
## GET /api/leads/{id:guid}/assignment
Action: `Leads.Assignment`; source: `CampionsSystem.API/Controllers/LeadsController.cs:25`.
Authorization: Bearer authentication; `Permission:Leads.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Crm.LeadAssignmentDetails?`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-leads-id-guid-assignment-history"></a>
## GET /api/leads/{id:guid}/assignment-history
Action: `Leads.AssignmentHistory`; source: `CampionsSystem.API/Controllers/LeadsController.cs:27`.
Authorization: Bearer authentication; `Permission:Leads.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.LeadAssignmentDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-leads-id-guid-interactions"></a>
## POST /api/leads/{id:guid}/interactions
Action: `Leads.Contact`; source: `CampionsSystem.API/Controllers/LeadsController.cs:29`.
Authorization: Bearer authentication; `Permission:Leads.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Crm.LeadInteractionRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Crm.LeadDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-leads-id-guid-interactions"></a>
## GET /api/leads/{id:guid}/interactions
Action: `Leads.Contacts`; source: `CampionsSystem.API/Controllers/LeadsController.cs:31`.
Authorization: Bearer authentication; `Permission:Leads.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.LeadInteractionDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="patch-api-leads-id-guid-status"></a>
## PATCH /api/leads/{id:guid}/status
Action: `Leads.Status`; source: `CampionsSystem.API/Controllers/LeadsController.cs:33`.
Authorization: Bearer authentication; `Permission:Leads.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Crm.LeadStatusRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Crm.LeadDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-leads-id-guid-convert"></a>
## POST /api/leads/{id:guid}/convert
Action: `Leads.Convert`; source: `CampionsSystem.API/Controllers/LeadsController.cs:35`.
Authorization: Bearer authentication; `Permission:Leads.Convert`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Crm.ConvertLeadRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Members.MemberDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
