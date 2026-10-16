# MembershipPlans API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-membership-plans"></a>
## GET /api/membership-plans
Action: `MembershipPlans.List`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:8`.
Authorization: Bearer authentication; `Permission:MembershipPlans.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `r` | query | `Plans.MembershipPlanListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Plans.MembershipPlanListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-membership-plans-id-guid"></a>
## GET /api/membership-plans/{id:guid}
Action: `MembershipPlans.Get`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:10`.
Authorization: Bearer authentication; `Permission:MembershipPlans.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Plans.MembershipPlanDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-membership-plans"></a>
## POST /api/membership-plans
Action: `MembershipPlans.Create`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:12`.
Authorization: Bearer authentication; `Permission:MembershipPlans.Manage`.

Request body: `CampionsSystem.BLL.Plans.CreateMembershipPlanRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Plans.MembershipPlanDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-membership-plans-id-guid"></a>
## PUT /api/membership-plans/{id:guid}
Action: `MembershipPlans.Update`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:14`.
Authorization: Bearer authentication; `Permission:MembershipPlans.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Plans.UpdateMembershipPlanRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Plans.MembershipPlanDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="patch-api-membership-plans-id-guid-status"></a>
## PATCH /api/membership-plans/{id:guid}/status
Action: `MembershipPlans.Status`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:16`.
Authorization: Bearer authentication; `Permission:MembershipPlans.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Plans.ChangePlanStatusRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Plans.MembershipPlanDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-membership-plans-id-guid-benefits"></a>
## GET /api/membership-plans/{id:guid}/benefits
Action: `MembershipPlans.Benefits`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:18`.
Authorization: Bearer authentication; `Permission:MembershipPlans.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Plans.PlanBenefitDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-membership-plans-id-guid-benefits"></a>
## PUT /api/membership-plans/{id:guid}/benefits
Action: `MembershipPlans.SetBenefits`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:20`.
Authorization: Bearer authentication; `Permission:MembershipPlans.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Plans.SetPlanBenefitsRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Plans.PlanBenefitDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-membership-plans-id-guid-eligible-members"></a>
## GET /api/membership-plans/{id:guid}/eligible-members
Action: `MembershipPlans.Eligible`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:22`.
Authorization: Bearer authentication; `Permission:FreelancePlans.ManageEligibleMembers`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `r` | query | `Plans.CatalogListRequest` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Plans.EligibleMemberListResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-membership-plans-id-guid-eligible-members-candidates"></a>
## GET /api/membership-plans/{id:guid}/eligible-members/candidates
Action: `MembershipPlans.Candidates`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:24`.
Authorization: Bearer authentication; `Permission:FreelancePlans.ManageEligibleMembers`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `r` | query | `Plans.EligibleMemberCandidateRequest` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Plans.EligibleMemberDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-membership-plans-id-guid-eligible-members"></a>
## POST /api/membership-plans/{id:guid}/eligible-members
Action: `MembershipPlans.AddEligible`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:26`.
Authorization: Bearer authentication; `Permission:FreelancePlans.ManageEligibleMembers`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Plans.EligibleMemberRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `bool`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="delete-api-membership-plans-id-guid-eligible-members-memberid-guid"></a>
## DELETE /api/membership-plans/{id:guid}/eligible-members/{memberId:guid}
Action: `MembershipPlans.RemoveEligible`; source: `CampionsSystem.API/Controllers/MembershipPlansController.cs:28`.
Authorization: Bearer authentication; `Permission:FreelancePlans.ManageEligibleMembers`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `memberId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `bool`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
