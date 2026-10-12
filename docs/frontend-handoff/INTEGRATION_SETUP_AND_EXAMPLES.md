# Integration setup and examples

Latest branch update: frozen 2026-10-12T10:44:04.604445+00:00 at `bf733e2502a85603b80f8ad8a01747264a0f518b` plus uncommitted operational files adds **35 Coaching routes** (190 current actions total). The later isolated snapshot builds; initial CS8999 was superseded by concurrent domain publication. Stable contracts below remain at 0bc4476; new draft contracts/confirmed policies and three legacy Class 410 changes are in [branch-only status](BRANCH_ONLY_PRIVATE_CLASSES_STATUS.md). **Use that update before interpreting any earlier pending/no-route statement below.**


**VERIFIED FROM CODE**: `API/Properties/launchSettings.json`: profile http `http://localhost:5236`; https `https://localhost:7186` plus http port 5236, Development. API routes use `/api`; Development OpenAPI is `/openapi/v1.json`. No Swagger UI registration or `/swagger` contract. `UseHttpsRedirection` is active; use configured HTTPS/dev certificate or a same-origin development proxy. Actual deployed host/URL/certificate availability is **UNKNOWN**.

The backend needs a securely supplied DefaultConnection, Jwt issuer/audience/base64 key (minimum 32 random bytes), valid AuthContext timezone, and compatible database schema. **No values are included here.** Startup requires configuration but does not automatically migrate/seed. Run an approved configured baseline via `dotnet run --project CampionsSystem.API --launch-profile https`; do not start the unfinished branch until it builds and its release is approved. Do not use seed/setup/migration switches just to make frontend screens work. Authorized backend administration must provision credentials/capabilities, cities/branches/employees, accounts/drawer/catalogs and commission rules/periods. No City CRUD API exists.

**PROPOSED FRONTEND VARIABLE** (not present/verified in a separate frontend repo): `VITE_API_BASE_URL=https://localhost:7186`. Inspect repo conventions first. Vite variables are public; never place database/JWT signing secrets/passwords there. No backend CORS policy exists. **RECOMMENDATION**: proxy `/api` through Vite in development or obtain a narrowly configured approved CORS policy. No origin list is currently guaranteed; no frontend port is assumed.

## HTTP conventions

Bearer auth; JSON bodies `Content-Type: application/json`; `Accept: application/json`. No custom operation/branch headers: operationId/version/branch are JSON or query fields where listed. Routes have Guid constraints but OpenAPI paths omit `:guid`; catalog preserves exact route templates. JSON is camelCase via MVC System.Text.Json Web defaults, case-insensitive reads, decimals emitted as JSON numbers; enum serialization is **mixed per type/property**, not globally string. Read DTO reference. Strict mutation types reject unknown fields; send explicit null on positional nullable fields when shape requires it. Nonnullable value-type parameters can default to zero when omitted; business guards still validate. API [Required] != C# nonnullable != schema required.

Most lists use `{items,totalCount,page,pageSize}`; Shift/Commission pages use `total`; CommissionProgress has employeeCount/Employees rather than normal total. Some routes return plain arrays/Guid/bool or null 204. There is no universal `{data,success}` wrapper. Business errors are ProblemDetails (`type,title,status,detail?,instance?,traceId?`); model errors ValidationProblemDetails add `errors` keyed by property. 401/403 may be empty Bearer responses; unknown route 404 can also lack an envelope. Unhandled failures use AddProblemDetails/UseExceptionHandler, normally 500 with deployment/content-negotiation details unknown. No backend retry-after/rate-limit/ETag convention is configured.

All examples below and per-endpoint examples use **FAKE values**, schema-shaped documentation only; no operational requests were sent. IDs must be replaced with authorized configured test fixtures; dates/counts/prices must match those fixtures. Authentication/CSRF cookies are not used.

```sh
curl --request POST "$API_BASE_URL/api/auth/login" \
  --header 'Content-Type: application/json' \
  --data '{"userNameOrEmail":"fake.frontend.operator","password":"FAKE_PASSWORD_PLACEHOLDER"}'
curl "$API_BASE_URL/api/auth/me" --header "Authorization: Bearer $ACCESS_TOKEN"
curl "$API_BASE_URL/api/members?search=FAKE&page=1&pageSize=25" --header "Authorization: Bearer $ACCESS_TOKEN"
curl "$API_BASE_URL/api/financial-accounts" --header "Authorization: Bearer $ACCESS_TOKEN"
```

Login response placeholder (not a valid token):

```json
{"accessToken":"FAKE_TOKEN_PLACEHOLDER","expiresAt":"2026-10-12T12:00:00Z","user":{"id":"11111111-1111-4111-8111-111111111111","userName":"fake.frontend.operator","employeeId":"22222222-2222-4222-8222-222222222222","fullName":"FAKE Operator","roles":["Reception"]}}
```

Check-in read-only preview:

```sh
curl --request POST "$API_BASE_URL/api/check-ins/evaluate" \
  --header "Authorization: Bearer $ACCESS_TOKEN" --header 'Content-Type: application/json' \
  --data '{"memberId":"33333333-3333-4333-8333-333333333333","memberCode":null,"branchId":null}'
```

Paid gym purchase body: assuming active offered plan price 1200, own shift/drawer, eligible Member and a configured account. New real operation UUID; retry preserves it.

```json
{"operationId":"44444444-4444-4444-8444-444444444444","memberId":"33333333-3333-4333-8333-333333333333","membershipPlanId":"55555555-5555-4555-8555-555555555555","purchaseMode":0,"activationMode":0,"paymentParts":[{"financialAccountId":"66666666-6666-4666-8666-666666666666","amount":1200,"providerReference":null}],"offerId":null,"startDate":null,"reservationDueDate":null,"installments":null,"branchId":null,"initialPaymentRequired":null}
```

Send to `POST /api/sales`; result is SaleReceipt `{saleId,paymentIds}`. Fetch `/api/sales/{saleId}` and subscription/current after success. Later collection body is `{operationId,amount,parts,allocations?}`, not the create-sale paymentParts shape. Full and paginated response examples for all workflows are linked from [catalog](API_ENDPOINT_CATALOG.md).

Expense actual return body for `POST /api/expenses/{id}/returns`:

```json
{"operationId":"77777777-7777-4777-8777-777777777777","returnedAmount":50,"fundsActuallyReturned":true,"reason":"FAKE actual returned funds","returnReference":"FAKE-RETURN-001"}
```

Commission initial fallback body for `POST /api/commissions/rules` (illustrative inputs, not seeded defaults):

```json
{"operationId":"88888888-8888-4888-8888-888888888888","employeeId":null,"effectiveFrom":"2026-10-12","newClients":{"target":30000,"belowRate":5,"reachedRate":7},"renewals":{"target":30000,"belowRate":3,"reachedRate":5},"branchId":"99999999-9999-4999-8999-999999999999"}
```

## Dates and money

DateOnly is YYYY-MM-DD; preserve as calendar text, never convert through UTC Date midnight. DateTimeOffset values are instants; backend stored actual timestamps are UTC. Configured default timezone Africa/Cairo observes platform timezone rules; no hardcoded UTC+2 assumption. The open branch BusinessDay can retain yesterday after midnight; backend BusinessDate is authoritative for money/access/periods. Employee auth validity and collection-time commission employment rights use gym-local actual date independently. Historic nullable dates remain unknown. Date ranges/sort/defaults vary and are enumerated in module evidence; do not add unsupported `sortBy`.

Decimal amounts reject excess precision and backend money bounds. Database numeric(18,2) can exceed JavaScript safe integer precision even for integer portions; bigint ClientCode/long Shift version likewise need explicit precision review. **RECOMMENDATION**: use decimal-safe input/math and bounded UI values; backend totals/quote/commission/access are authoritative. Do not send locale separators or currency symbols. Exact arithmetic/rate rounding lives on backend; formatting is not business calculation.

## Client and mock recommendations

**RECOMMENDATIONS**: one typed HTTP adapter, AbortSignal for cancellable searches, debounce around 250–400 ms, keep page/filters in query keys, include user and explicit branch context, cancel/remove old scoped queries on auth/branch changes. TanStack Query invalidate member/search/ownership/CRM after profile/conversion; subscriptions/access/history/Sales after finance/freeze; Shift/drawer/expenses/audit/totals after payouts/counts; commissions after collections/refunds/config/finalization. Select bounded pages explicitly. No automatic retry for irreversible/non-idempotent mutations; exact stable-operation retries only after inspecting failure.

Mock development can use fabricated catalog examples and recorded **schema shapes**, without copied real customer records/tokens. Simulate 400 ValidationProblemDetails, blank 401/403, 404, 409, business-denial HTTP 200, null 204, stale context and network-uncertain idempotent retry. Label mock mode visibly and never represent fixtures as seeded backend facts.

No multipart/file upload/download, PDF/Excel receipt/report export, push notification endpoint, SignalR hub, barcode generation or message sending is registered. Reports are JSON. Receipt/reference strings are text, not attachment APIs. Lead bulk JSON is not a spreadsheet import.
