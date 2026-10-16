# Offers API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-offers"></a>
## GET /api/offers
Action: `Offers.Eligible`; source: `CampionsSystem.API/Controllers/OffersController.cs:8`.
Authorization: Bearer authentication; `Permission:Payments.Create`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `memberId` | query | `Guid` | — |
| `membershipPlanId` | query | `Guid` | — |
| `branchId` | query | `Guid?` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.OfferDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-offers-quote"></a>
## GET /api/offers/quote
Action: `Offers.Quote`; source: `CampionsSystem.API/Controllers/OffersController.cs:10`.
Authorization: Bearer authentication; `Permission:Payments.Create`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `memberId` | query | `Guid` | — |
| `membershipPlanId` | query | `Guid` | — |
| `offerId` | query | `Guid?` | — |
| `branchId` | query | `Guid?` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Sales.PricingResult`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-offers-management"></a>
## GET /api/offers/management
Action: `Offers.Management`; source: `CampionsSystem.API/Controllers/OffersController.cs:12`.
Authorization: Bearer authentication; `Permission:Offers.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid?` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Sales.OfferHistory`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-offers-id-guid"></a>
## GET /api/offers/{id:guid}
Action: `Offers.Details`; source: `CampionsSystem.API/Controllers/OffersController.cs:14`.
Authorization: Bearer authentication; `Permission:Offers.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Sales.OfferDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-offers"></a>
## POST /api/offers
Action: `Offers.Create`; source: `CampionsSystem.API/Controllers/OffersController.cs:16`.
Authorization: Bearer authentication; `Permission:Offers.Manage`.

Request body: `CampionsSystem.BLL.Sales.OfferRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Sales.OfferDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-offers-id-guid"></a>
## PUT /api/offers/{id:guid}
Action: `Offers.Update`; source: `CampionsSystem.API/Controllers/OffersController.cs:18`.
Authorization: Bearer authentication; `Permission:Offers.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Sales.OfferRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Sales.OfferDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-offers-id-guid-member-entitlements"></a>
## POST /api/offers/{id:guid}/member-entitlements
Action: `Offers.Grant`; source: `CampionsSystem.API/Controllers/OffersController.cs:20`.
Authorization: Bearer authentication; `Permission:Offers.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Sales.GrantMemberOfferRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Sales.MemberOfferGrantDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="delete-api-offers-id-guid-member-entitlements-grantid-guid"></a>
## DELETE /api/offers/{id:guid}/member-entitlements/{grantId:guid}
Action: `Offers.Revoke`; source: `CampionsSystem.API/Controllers/OffersController.cs:22`.
Authorization: Bearer authentication; `Permission:Offers.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `grantId` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Sales.MemberOfferGrantDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-members-memberid-guid-audience-categories"></a>
## PUT /api/members/{memberId:guid}/audience-categories
Action: `Offers.Audiences`; source: `CampionsSystem.API/Controllers/OffersController.cs:24`.
Authorization: Bearer authentication; `Permission:Members.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `memberId` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Sales.SetMemberAudienceRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.MemberAudienceDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
