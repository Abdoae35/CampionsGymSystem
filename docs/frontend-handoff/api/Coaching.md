# Coaching API contract
Current master `e3b8cc3`. Exact request schemas: [runtime OpenAPI](../openapi.json); responses hidden by `IActionResult`: [integration supplement](../openapi.integration.json). [DTO reference](../DTO_AND_ENUM_REFERENCE.md) and [error guide](../API_ERRORS.md).
Controller policies below are ANDed. Branch, Owner membership, employee status, Shift and business rules are enforced in services where applicable.

<a id="get-api-coaching-private-packages"></a>
## GET /api/coaching/private/packages
Action: `Coaching.PrivatePackages`; source: `CampionsSystem.API/Controllers/CoachingController.cs:12`.
Authorization: Bearer authentication; `Permission:Private.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachingPackageDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-private-packages"></a>
## POST /api/coaching/private/packages
Action: `Coaching.CreatePrivatePackage`; source: `CampionsSystem.API/Controllers/CoachingController.cs:14`.
Authorization: Bearer authentication; `Permission:Private.Manage`.

Request body: `CampionsSystem.BLL.Coaching.CoachingPackageRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPackageDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-coaching-private-packages-id-guid"></a>
## PUT /api/coaching/private/packages/{id:guid}
Action: `Coaching.UpdatePrivatePackage`; source: `CampionsSystem.API/Controllers/CoachingController.cs:16`.
Authorization: Bearer authentication; `Permission:Private.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Coaching.CoachingPackageRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPackageDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-private-purchases"></a>
## POST /api/coaching/private/purchases
Action: `Coaching.PurchasePrivate`; source: `CampionsSystem.API/Controllers/CoachingController.cs:18`.
Authorization: Bearer authentication; `Permission:Private.Manage` AND `Permission:Payments.Create`.

Request body: `CampionsSystem.BLL.Coaching.CoachingPurchaseRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPurchaseReceipt`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-private-members"></a>
## GET /api/coaching/private/members
Action: `Coaching.PrivateMembers`; source: `CampionsSystem.API/Controllers/CoachingController.cs:20`.
Authorization: Bearer authentication; `Permission:Private.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid` | — |
| `search` | query | `string` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachingMemberResult>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-private-subscriptions"></a>
## GET /api/coaching/private/subscriptions
Action: `Coaching.PrivateSubscriptions`; source: `CampionsSystem.API/Controllers/CoachingController.cs:22`.
Authorization: Bearer authentication; `Permission:Private.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid` | — |
| `memberId` | query | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.PrivateDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-private-subscriptions-id-guid"></a>
## GET /api/coaching/private/subscriptions/{id:guid}
Action: `Coaching.PrivateSubscription`; source: `CampionsSystem.API/Controllers/CoachingController.cs:24`.
Authorization: Bearer authentication; `Permission:Private.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.PrivateDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-private-subscriptions-id-guid-sessions"></a>
## POST /api/coaching/private/subscriptions/{id:guid}/sessions
Action: `Coaching.RegisterPrivate`; source: `CampionsSystem.API/Controllers/CoachingController.cs:26`.
Authorization: Bearer authentication; `Permission:Private.RegisterSession`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Coaching.PrivateAttendanceRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.PrivateAttendanceDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-private-subscriptions-id-guid-sessions"></a>
## GET /api/coaching/private/subscriptions/{id:guid}/sessions
Action: `Coaching.PrivateSessions`; source: `CampionsSystem.API/Controllers/CoachingController.cs:28`.
Authorization: Bearer authentication; `Permission:Private.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.PrivateAttendanceDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-private-subscriptions-id-guid-transfers"></a>
## POST /api/coaching/private/subscriptions/{id:guid}/transfers
Action: `Coaching.Transfer`; source: `CampionsSystem.API/Controllers/CoachingController.cs:30`.
Authorization: Bearer authentication; `Permission:Private.Transfer`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Coaching.PrivateTransferRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.PrivateAssignmentDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-private-subscriptions-id-guid-expiry-settlements"></a>
## POST /api/coaching/private/subscriptions/{id:guid}/expiry-settlements
Action: `Coaching.SettleExpiry`; source: `CampionsSystem.API/Controllers/CoachingController.cs:32`.
Authorization: Bearer authentication; `Permission:Private.SettleExpiry`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Coaching.PrivateExpiryRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.PrivateExpiryDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-private-subscriptions-id-guid-expiry-settlements"></a>
## GET /api/coaching/private/subscriptions/{id:guid}/expiry-settlements
Action: `Coaching.ExpiryHistory`; source: `CampionsSystem.API/Controllers/CoachingController.cs:34`.
Authorization: Bearer authentication; `Permission:Private.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.PrivateExpiryDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-classes-packages"></a>
## GET /api/coaching/classes/packages
Action: `Coaching.ClassesPackages`; source: `CampionsSystem.API/Controllers/CoachingController.cs:37`.
Authorization: Bearer authentication; `Permission:Classes.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachingPackageDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-classes-packages"></a>
## POST /api/coaching/classes/packages
Action: `Coaching.CreateClassesPackage`; source: `CampionsSystem.API/Controllers/CoachingController.cs:39`.
Authorization: Bearer authentication; `Permission:Classes.Manage`.

Request body: `CampionsSystem.BLL.Coaching.CoachingPackageRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPackageDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-coaching-classes-packages-id-guid"></a>
## PUT /api/coaching/classes/packages/{id:guid}
Action: `Coaching.UpdateClassesPackage`; source: `CampionsSystem.API/Controllers/CoachingController.cs:41`.
Authorization: Bearer authentication; `Permission:Classes.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Coaching.CoachingPackageRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPackageDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-classes-purchases"></a>
## POST /api/coaching/classes/purchases
Action: `Coaching.PurchaseClasses`; source: `CampionsSystem.API/Controllers/CoachingController.cs:43`.
Authorization: Bearer authentication; `Permission:Classes.Manage` AND `Permission:Payments.Create`.

Request body: `CampionsSystem.BLL.Coaching.CoachingPurchaseRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPurchaseReceipt`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-classes-members"></a>
## GET /api/coaching/classes/members
Action: `Coaching.ClassMembers`; source: `CampionsSystem.API/Controllers/CoachingController.cs:45`.
Authorization: Bearer authentication; `Permission:Classes.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid` | — |
| `search` | query | `string` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachingMemberResult>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-classes-subscriptions"></a>
## GET /api/coaching/classes/subscriptions
Action: `Coaching.ClassSubscriptions`; source: `CampionsSystem.API/Controllers/CoachingController.cs:47`.
Authorization: Bearer authentication; `Permission:Classes.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid` | — |
| `memberId` | query | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassesSubscriptionDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-classes-members-member-guid-attendance"></a>
## GET /api/coaching/classes/members/{member:guid}/attendance
Action: `Coaching.ClassMemberAttendance`; source: `CampionsSystem.API/Controllers/CoachingController.cs:49`.
Authorization: Bearer authentication; `Permission:Classes.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `member` | path | `Guid` | — |
| `branchId` | query | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassAttendanceDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-classes-types"></a>
## GET /api/coaching/classes/types
Action: `Coaching.ClassTypes`; source: `CampionsSystem.API/Controllers/CoachingController.cs:51`.
Authorization: Bearer authentication; `Permission:Classes.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassTypeDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-classes-types"></a>
## POST /api/coaching/classes/types
Action: `Coaching.CreateClassType`; source: `CampionsSystem.API/Controllers/CoachingController.cs:53`.
Authorization: Bearer authentication; `Permission:Classes.Manage`.

Request body: `CampionsSystem.BLL.Coaching.ClassTypeRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.ClassTypeDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="put-api-coaching-classes-types-id-guid"></a>
## PUT /api/coaching/classes/types/{id:guid}
Action: `Coaching.UpdateClassType`; source: `CampionsSystem.API/Controllers/CoachingController.cs:55`.
Authorization: Bearer authentication; `Permission:Classes.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Coaching.ClassTypeRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.ClassTypeDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-classes-occurrences"></a>
## GET /api/coaching/classes/occurrences
Action: `Coaching.Occurrences`; source: `CampionsSystem.API/Controllers/CoachingController.cs:57`.
Authorization: Bearer authentication; `Permission:Classes.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `branchId` | query | `Guid` | — |
| `from` | query | `DateOnly` | — |
| `to` | query | `DateOnly` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassOccurrenceDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-classes-occurrences"></a>
## POST /api/coaching/classes/occurrences
Action: `Coaching.CreateOccurrence`; source: `CampionsSystem.API/Controllers/CoachingController.cs:59`.
Authorization: Bearer authentication; `Permission:Classes.Manage`.

Request body: `CampionsSystem.BLL.Coaching.ClassOccurrenceRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.ClassOccurrenceDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-classes-occurrences-id-guid-complete"></a>
## POST /api/coaching/classes/occurrences/{id:guid}/complete
Action: `Coaching.CompleteOccurrence`; source: `CampionsSystem.API/Controllers/CoachingController.cs:61`.
Authorization: Bearer authentication; `Permission:Classes.Manage`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Coaching.CompleteClassRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.ClassOccurrenceDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-classes-occurrences-id-guid-attendance"></a>
## POST /api/coaching/classes/occurrences/{id:guid}/attendance
Action: `Coaching.RegisterClass`; source: `CampionsSystem.API/Controllers/CoachingController.cs:63`.
Authorization: Bearer authentication; `Permission:Classes.RegisterAttendance`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
Request body: `CampionsSystem.BLL.Coaching.ClassAttendanceRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.ClassAttendanceDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-classes-occurrences-id-guid-attendance"></a>
## GET /api/coaching/classes/occurrences/{id:guid}/attendance
Action: `Coaching.ClassAttendance`; source: `CampionsSystem.API/Controllers/CoachingController.cs:65`.
Authorization: Bearer authentication; `Permission:Classes.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `id` | path | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassAttendanceDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-classes-coaches-coach-guid-statistics"></a>
## GET /api/coaching/classes/coaches/{coach:guid}/statistics
Action: `Coaching.ClassStatistics`; source: `CampionsSystem.API/Controllers/CoachingController.cs:67`.
Authorization: Bearer authentication; `Permission:Classes.ViewAttendanceReport`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `coach` | path | `Guid` | — |
| `branchId` | query | `Guid` | — |
| `from` | query | `DateOnly` | — |
| `to` | query | `DateOnly` | — |
| `periodId` | query | `Guid?` | `None` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.ClassCoachStatistics`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-earnings-targets"></a>
## POST /api/coaching/earnings/targets
Action: `Coaching.Targets`; source: `CampionsSystem.API/Controllers/CoachingController.cs:70`.
Authorization: Bearer authentication; `Permission:CoachEarnings.ManageTargets`.

Request body: `CampionsSystem.BLL.Coaching.CoachTargetRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachTargetDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-earnings-periods-period-guid-targets"></a>
## GET /api/coaching/earnings/periods/{period:guid}/targets
Action: `Coaching.TargetHistory`; source: `CampionsSystem.API/Controllers/CoachingController.cs:72`.
Authorization: Bearer authentication; `Permission:CoachEarnings.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `period` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Coaching.CoachTargetDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-earnings-periods-period-guid-progress"></a>
## GET /api/coaching/earnings/periods/{period:guid}/progress
Action: `Coaching.Progress`; source: `CampionsSystem.API/Controllers/CoachingController.cs:74`.
Authorization: Bearer authentication; `Permission:CoachEarnings.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `period` | path | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachEarningsProgress`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-earnings-recognitions"></a>
## POST /api/coaching/earnings/recognitions
Action: `Coaching.Recognize`; source: `CampionsSystem.API/Controllers/CoachingController.cs:76`.
Authorization: Bearer authentication; `Permission:CoachEarnings.Recognize`.

Request body: `CampionsSystem.BLL.Coaching.RecognizeCoachEarningsRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachEarningsProgress`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-earnings-coaches-coach-guid-balance"></a>
## GET /api/coaching/earnings/coaches/{coach:guid}/balance
Action: `Coaching.Balance`; source: `CampionsSystem.API/Controllers/CoachingController.cs:78`.
Authorization: Bearer authentication; `Permission:CoachEarnings.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `coach` | path | `Guid` | — |
| `branchId` | query | `Guid` | — |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachBalance`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-earnings-coaches-coach-guid-history"></a>
## GET /api/coaching/earnings/coaches/{coach:guid}/history
Action: `Coaching.EarningsHistory`; source: `CampionsSystem.API/Controllers/CoachingController.cs:80`.
Authorization: Bearer authentication; `Permission:CoachEarnings.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `coach` | path | `Guid` | — |
| `branchId` | query | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachEarningDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="post-api-coaching-earnings-payouts"></a>
## POST /api/coaching/earnings/payouts
Action: `Coaching.Payout`; source: `CampionsSystem.API/Controllers/CoachingController.cs:82`.
Authorization: Bearer authentication; `Permission:CoachEarnings.Pay`.

Request body: `CampionsSystem.BLL.Coaching.CoachPayoutRequest` (JSON; exact fields and requiredness in linked OpenAPI schema).
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachPayoutDetails`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.

<a id="get-api-coaching-earnings-coaches-coach-guid-payouts"></a>
## GET /api/coaching/earnings/coaches/{coach:guid}/payouts
Action: `Coaching.PayoutHistory`; source: `CampionsSystem.API/Controllers/CoachingController.cs:84`.
Authorization: Bearer authentication; `Permission:CoachEarnings.View`.

| Parameter | Location | C# type | Declared default |
|---|---|---|---|
| `coach` | path | `Guid` | — |
| `branchId` | query | `Guid` | — |
| `page` | query | `int` | `1` |
| `pageSize` | query | `int` | `25` |
Request body: none.
Success: HTTP 200 `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachPayoutDetails>`; see integration schema for JSON fields.
Runtime OpenAPI response codes: `200`. Service validation may also produce 400/403/404/409 as described in the error guide.
