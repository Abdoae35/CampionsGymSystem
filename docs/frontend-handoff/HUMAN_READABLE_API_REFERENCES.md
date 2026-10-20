# Backend V1 human-readable references — frontend integration

Source: `feature/HumanReadableApiReferences` at `2857260` plus this documentation commit, based on `origin/master` `9c5b92f` (2026-10-20). API version remains V1; no route or request change. JSON names below use the API's camelCase serialization. Every added field is `string | null`; existing GUIDs, amounts, statuses, dates, page envelopes, filters and permissions remain. The names reflect the current linked record at read time, so they may change after a rename. Missing or unlinked identities return `null`. Identity User IDs resolve through `ApplicationUser.Id → Employee.UserId → Employee.FullName`; an inactive employee can still supply a name. No email or username is used as an actor fallback.

## Contract-change manifest

| Response DTO | Existing endpoint(s) or result path | Added JSON fields |
|---|---|---|
| `FinancialMovementDetails` | `GET /api/vault/movements` | `actorName`, `accountName` |
| `CashCountDetails` | `GET /api/vault/counts`, count write result, monthly report `cashCounts[]` | `actorName` |
| `FinancialAuditDetails` | `GET /api/vault/audit` | `actorName` |
| `DrawerPosition` | `GET /api/vault/position`, monthly report `drawer` | `accountName` |
| `SaleSummary` | `GET /api/sales` `items[]` | `memberName`, `memberCode` |
| `CheckInDetails` | `GET /api/check-ins`, `GET /api/members/{memberId}/check-ins/current-business-day`, Check-In write result `checkIn` | `actorName` |
| `SubscriptionSummary` | `GET /api/members/{memberId}/subscriptions` when `includePending` is absent/false, `GET /api/members/{memberId}/subscriptions/current` nested `subscription`, `GET /api/subscriptions/{id}` nested `subscription` | `originBranchName`, `accessCityName` |
| `SubscriptionHistoryItem` | `GET /api/members/{memberId}/subscriptions?includePending=true` `items[]` | `originBranchName`, `accessCityName` |
| `ShiftDetails` | `GET /api/shifts`, `/current`, `/{id}` and shared write/report results | `branchName`, `closedByName` |
| `BusinessDayDetails` | `GET /api/business-days/current` and open/close results | `branchName`, `openedByName`, `closedByName` |
| `ExpenseDetails` | `GET /api/expenses`, `/{id}` and shared write/report results | `recordingEmployeeName`, `recordedByName`, `accountName` |
| `AdvanceDetails` | `GET /api/employee-advances`, `/{id}` and shared write/report results | `recordingEmployeeName`, `accountName` |
| `LeadInteractionDetails` | `GET /api/leads/{leadId}/interactions`, interaction write result | `employeeName` |
| `LeadAssignmentDetails` | `GET /api/leads/{leadId}/assignment`, `/assignment-history`, assignment write result | `assignedByName` |
| `BranchAssignmentDetails` | `GET /api/employees/{employeeId}/branch-assignment`, `/branch-history`, assignment/transfer results | `assignedByName` |
| `CommissionRuleDetails` | `GET /api/commissions/rules`, rule write result | `employeeName` |
| `CommissionSourceLine` | `GET /api/commissions/periods/{id}/sources`, monthly report `salesSources[]` | `memberName`, `memberCode`, `originalSalesEmployeeName`, `employeeName` (credited employee) |
| `PrivateDetails`, nested assignments | `GET /api/coaching/private/subscriptions`, `/{id}` and purchase/result paths | `memberName`, `memberCode`, `originalCoachEmployeeName`; nested assignment `coachEmployeeName` |
| `PrivateAttendanceDetails`, `PrivateAssignmentDetails`, `PrivateExpiryDetails` | Private sessions/history, transfers and expiry settlement routes under `/api/coaching/private/subscriptions/{id}` | attendance `memberName`, `memberCode`, `coachEmployeeName`; assignment/expiry `coachEmployeeName` |
| `ClassesSubscriptionDetails`, `ClassTypeDetails`, `ClassOccurrenceDetails`, `ClassAttendanceDetails`, `ClassCoachStatistics` | `/api/coaching/classes/subscriptions`, `/types`, `/occurrences`, occurrence/member attendance, coach statistics and shared writes | subscription `memberName`, `memberCode`; type `coachEmployeeName`; occurrence `classTypeName`, `coachEmployeeName`; attendance `memberName`, `memberCode`, `coachEmployeeName`, `classTypeName`; statistics `coachEmployeeName` |
| `CoachTargetDetails`, `CoachProgress`, `CoachEarningDetails`, `CoachBalance`, `CoachPayoutDetails` | `/api/coaching/earnings/periods/{period}/targets` and `/progress`, coach balance/history/payouts and shared writes | each `coachEmployeeName`; payout also `authorizedByName` |
| `MonthEventDetails`, `RevisionDetails` | `GET /api/financial-months/{id}/events`, `/revisions` | `actorName` |
| `FinancialTransactionReportRow` | `GET /api/financial-months/{id}/transactions`, report `transactions[]` | `actorName` |
| `SalarySettingDetails` | `GET/POST /api/employee-salaries/{employeeId}/settings` | `actorName` |
| `PayrollPaymentDetails` | `GET/POST /api/payroll/{id}/payments`, report `employeePayments[]` | `actorName`, `employeeName` |
| `AccessAuditDetails` | `GET /api/access-administration/history` `items[]` | `actorName`, `targetUserName`, `roleName`, `permissionCode` |

`FinancialAuditDetails.entitySummary` was skipped: no reliable entity-type/event summary source exists for all audit rows. Existing named role/catalog/user inventory fields were retained rather than duplicated. `GET /api/sales/{id}` already has `memberName`.

The exact Coaching read routes touched by those shared DTOs are `GET /api/coaching/private/subscriptions?branchId={branchId}&memberId={memberId}`, `GET /api/coaching/private/subscriptions/{id}`, `GET /api/coaching/private/subscriptions/{id}/sessions`, `GET /api/coaching/private/subscriptions/{id}/expiry-settlements`, `GET /api/coaching/classes/subscriptions?branchId={branchId}&memberId={memberId}`, `GET /api/coaching/classes/members/{memberId}/attendance?branchId={branchId}`, `GET /api/coaching/classes/types?branchId={branchId}`, `GET /api/coaching/classes/occurrences?branchId={branchId}&from={date}&to={date}`, `GET /api/coaching/classes/occurrences/{id}/attendance`, `GET /api/coaching/classes/coaches/{coachId}/statistics?branchId={branchId}&from={date}&to={date}`, `GET /api/coaching/earnings/periods/{periodId}/targets`, `GET /api/coaching/earnings/periods/{periodId}/progress`, `GET /api/coaching/earnings/coaches/{coachId}/balance?branchId={branchId}`, `GET /api/coaching/earnings/coaches/{coachId}/history?branchId={branchId}`, and `GET /api/coaching/earnings/coaches/{coachId}/payouts?branchId={branchId}`. Existing paged routes accept `page` and `pageSize` where declared by their controllers; no page default or total count changed.

## Example fragments

Only relevant fields are shown; keep all other fields from the current contract.

```json
{
  "items": [{
    "id": "11111111-1111-4111-8111-111111111111",
    "memberId": "22222222-2222-4222-8222-222222222222",
    "memberName": "Amina Hassan",
    "memberCode": "M-000123",
    "finalPrice": 500.00
  }],
  "totalCount": 1,
  "page": 1,
  "pageSize": 25
}
```

Before this change, a Sale summary included the same ID, Member ID and financial fields but omitted `memberName` and `memberCode`.

```json
{
  "id": "33333333-3333-4333-8333-333333333333",
  "actorId": "44444444-4444-4444-8444-444444444444",
  "actorName": "Reception employee",
  "memberName": "Amina Hassan",
  "memberCode": "M-000123"
}
```

A Check-In row previously carried `actorId` but omitted `actorName`. For an unlinked Owner or deleted Employee, `actorName` is `null` while `actorId` stays intact.

### Subscription history, including pending First Check-In

`GET /api/members/{memberId}/subscriptions?page=1&pageSize=25&includePending=true` uses the existing `SubscriptionPurchaseHistory` envelope. Optional filters are `branchId` (UUID), `status` (subscription status enum), `planType` (plan category enum), `from` and `to` (`YYYY-MM-DD`). Page starts at 1; page size defaults to 25 and must be 1–100. Filters, authorization, sorting, total count and page selection apply to the combined history. With `includePending` absent/false the original `SubscriptionHistory` shape is preserved. Both require `Subscriptions.View` and Member/origin-branch scope. `/subscriptions/current?branchId={visitedBranchId}` remains the access decision and may return `hasAccess:false` for a paid pending purchase.

The `includePending=true` item contract is:

| Field | Type | Notes |
|---|---|---|
| `subscriptionId`, `saleId` | UUID or null | Pending purchase has `subscriptionId:null` and a Sale ID; created period has its subscription ID and `saleId:null`. |
| `memberId`, `planId`, `originBranchId`, `accessCityId` | UUID | Existing references. |
| `planName`, `planType`, `originBranchName`, `accessCityName` | string or null | Current plan/branch/city labels where available; purchased plan details come from the sale snapshot. |
| `mode` | string | Subscription mode. |
| `startDate`, `endDate` | date string or null | Pending start/end are null. |
| `activationStatus` | string | `PendingFirstCheckIn` for eligible paid sale before entry; created subscription uses its current status. |
| `totalSessions`, `remainingSessions` | integer or null | Pending session-based sale uses full purchased count for both. |
| `saleBusinessDate` | date string or null | Pending Sale business date; null on created periods. |
| `purchaseMode`, `paymentStatus` | string or null | Pending Sale information; null on created periods. |
| `purchasedDurationDays` | integer or null | Pending snapshot duration; null on created periods. |
| `finalPrice`, `collected` | number or null | Pending Sale monetary data; null on created periods. |

Pending item example:

```json
{
  "subscriptionId": null,
  "saleId": "55555555-5555-4555-8555-555555555555",
  "memberId": "22222222-2222-4222-8222-222222222222",
  "planId": "66666666-6666-4666-8666-666666666666",
  "planName": "Basic 12",
  "planType": "Normal",
  "originBranchId": "77777777-7777-4777-8777-777777777777",
  "accessCityId": "88888888-8888-4888-8888-888888888888",
  "mode": "SessionBased",
  "startDate": null,
  "endDate": null,
  "activationStatus": "PendingFirstCheckIn",
  "totalSessions": 12,
  "remainingSessions": 12,
  "saleBusinessDate": "2026-10-18",
  "purchaseMode": "FullPayment",
  "purchasedDurationDays": 365,
  "finalPrice": 500.00,
  "collected": 500.00,
  "paymentStatus": "Paid",
  "originBranchName": "Tanta branch",
  "accessCityName": "Tanta"
}
```

Created subscription item example after the first Check-In:

```json
{
  "subscriptionId": "99999999-9999-4999-8999-999999999999",
  "saleId": null,
  "memberId": "22222222-2222-4222-8222-222222222222",
  "planId": "66666666-6666-4666-8666-666666666666",
  "planName": "Basic 12",
  "planType": "Normal",
  "originBranchId": "77777777-7777-4777-8777-777777777777",
  "accessCityId": "88888888-8888-4888-8888-888888888888",
  "mode": "SessionBased",
  "startDate": "2026-10-18",
  "endDate": "2027-10-17",
  "activationStatus": "Active",
  "totalSessions": 12,
  "remainingSessions": 11,
  "saleBusinessDate": null,
  "purchaseMode": null,
  "purchasedDurationDays": null,
  "finalPrice": null,
  "collected": null,
  "paymentStatus": null,
  "originBranchName": "Tanta branch",
  "accessCityName": "Tanta"
}
```

After the first valid Check-In, the pending sale item is removed and the created subscription appears once. This existing history projection does not carry Sale payment fields after activation; use `GET /api/sales/{saleId}` for Sale details if the frontend retains the Sale ID. A `status` filter excludes pending purchases by design because pending is not a subscription status. The frontend should show the pending item as a paid purchase awaiting activation; it must use `/subscriptions/current` or Check-In evaluation for access, never infer entry permission from history.

## Scope, fallback and compatibility

Existing endpoint policies remain in controller source: `Vault.View`, `Payments.View`, `CheckIns.View`/`CheckIns.Create`, `Subscriptions.View`, `Shifts.View`, `Expenses.View`, `Advances.View`, `Leads.View`, `Employees.View`, `Commission.View`, `Private.View`, `Classes.View`, `CoachEarnings.View`, `FinancialMonths.View`, `Salaries.View`, `Payroll.View` and Owner-only Access Administration with both `Users.View` and `Roles.View` for history. Shared write responses retain their existing manage/create permissions. Existing branch checks remain service-enforced; labels are resolved after authorization from referenced IDs and do not grant a new directory. Paginated rows use bounded batch lookups; no per-item identity fetch is introduced.

Render a nullable label when present. If null, use a neutral UI placeholder such as “Unknown” or hide that secondary label; keep the GUID available for diagnostics and actions. Do not display a private username/email as an automatic fallback. No DTO removal or required-field change was made. Clients accepting additional JSON properties need no migration; strict decoders must add the fields listed above. The existing `includePending=true` history shape is distinct from the default history shape and requires its `SubscriptionHistoryItem` model.

## OpenAPI provenance

`openapi.json` was recaptured from this feature's Development `/openapi/v1.json` on 2026-10-20 with an unreachable dummy local database target and throwaway JWT settings. Only OpenAPI was requested; no database endpoint or business API was called. The temporary local API process was stopped. The raw document contains 189 paths, 232 operations and 131 schemas. Runtime OpenAPI omits many `IActionResult` success schemas; `openapi.integration.json` is an older derived V1 supplement without a regeneration script in this repository and is **not updated for this feature**. Use this manifest and the current DTO source for changed response shapes. The raw document's local `servers` URL is capture-only.
