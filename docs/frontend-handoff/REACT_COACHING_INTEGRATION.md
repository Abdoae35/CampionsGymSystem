# React integration for completed Coaching workflows

Inspect the separate frontend repository before choosing libraries; adapt these instructions to its established React/TypeScript HTTP, Router, Query, form and validation conventions. [Shared setup](INTEGRATION_SETUP_AND_EXAMPLES.md) and [auth/branch rules](AUTH_PERMISSIONS_BRANCH_CONTEXT.md) apply to all modules.

## API/auth/context adapter

Configure a public API base URL (for example VITE_API_BASE_URL if using Vite); keep secrets outside frontend variables. With a same-origin /api proxy, avoid adding /api twice. One HTTP adapter attaches Bearer and JSON headers, handles AbortSignal, parses success JSON/204, and normalizes ProblemDetails, ValidationProblemDetails and empty 401/403/404. Login then GET /api/auth/me; login contains no effective capabilities. Clear protected caches on local logout/401 or user/branch context change; 403 is a denied action, not a re-login loop. No refresh or server branch-switch endpoint exists.

Use /me.permissions with exact case-sensitive keys. Role names do not grant access; direct Deny wins. Discretionary expiry/target/recognition/payout buttons require their own key AND Branches.AccessAll. Attendance/completion additionally require the authenticated Employee to be the actual assigned coach; global scope cannot impersonate another coach. Purchases require module Manage + Payments.Create + Sales.Create, and Installments.Manage for installment mode.

Each Coaching query needing branchId must send the authorized operational branch explicitly; ordinary staff use /me.currentBranchId. Resource-only routes derive scope from the saved resource; do not append guessed branch headers. Include user, branch, resource, filters and page in query keys. Use camelCase response types, numeric PurchaseMode=0 or 2, YYYY-MM-DD calendar strings and offset timestamp instants. Do not calculate effective BusinessDate from browser midnight.

## Pagination, errors and retries

Coaching pages are `{items,totalCount,page,pageSize}` with page>=1, default 1/25, pageSize 1..100; other modules can use total/plain arrays. Debounce phone/code/name search, cancel stale requests and reset page when filters change. Current API branch/date query metadata is permissive at binding, but business guards remain authoritative. Send documented required context, not Guid.Empty.

Generate crypto.randomUUID() for each intentional operation exposing operationId and preserve that ID **and original payload** on a network-uncertain retry. Never retry with a new ID just because a response was lost. Prevent duplicate taps. Save expectedVersion from authoritative details, then refresh after stale 400/409 before intentional resubmission. Catalog edits have no general operation identity. Write DTOs reject unknown fields; omit local/UI-only fields. No optimistic session/money deduction.

Keep loading, empty, denied, configuration-missing, expired, insufficient-balance and conflict states separate. Parse backend detail/errors without matching brittle English strings to business policies. A 400 can mean stale class completion or changed retry payload, not just invalid form syntax. Check-in APIs can deny inside HTTP 200; Coaching registration returns a real result or an error. Preserve nullable target/rate/legacy evidence as unavailable, not zero.

## Private delivery, transfers and expiry

1. Read packages and purchase using authoritative catalog price through Coaching purchase; further payments/refunds use existing Sales routes. Capture subscriptionId, saleId and paymentIds. FullPayment=0 or Installment=2 with explicit positive initial collection/schedule; no offer/reservation options in this contract. Finance requires own shift/accounts.
2. Assigned coach searches GET /api/coaching/private/members with branchId/search and reads member subscriptions/details/history. Identify a legitimate reception Gym CheckIn from the existing check-in workflow for the same member/branch/effective BusinessDate and valid regular membership. Reception never consumes Private.
3. Explicit POST subscription/{id}/sessions sends operationId/gymCheckInId/optional notes. One accepted request consumes one Private session; one reception entry supports at most one Private session. Refresh details/history/search after success. Missing reception, expired membership/package, unauthorized coach/city or overdue obligations reject.
4. Authorized transfer selects a real eligible coach and sends operationId/coachEmployeeId/expectedVersion/reason. Preserve past delivered attribution. Show incoming commission fixed 50% on that purchased package; a new purchase restores normal independent rules.
5. Owner expiry authorization sends operationId/commissionPeriodId/reason only after inclusive expiration, for unused uncancelled quantity and a valid open period. Show all unused quantity attributed to the coach assigned at expiry, fixed 50% if transferred. It creates evidence for earnings only; never render synthetic attendance or decrement actual remaining/used counters as though delivered. Refresh expiry history, progress and details separately.

## Classes delivery and coach statistics

1. Sell a shared Classes package; omit/null coachEmployeeId. Sessions work across any type in the purchased branch. Classes attendance does not require reception or regular membership.
2. Read/configure types and create an occurrence with operationId/classTypeId/coachEmployeeId/startsAt. StartsAt must be future on the current gym-local calendar day; the returned BusinessDate may differ during overnight operation. No booking/capacity/waitlist workflow exists.
3. Assigned coach searches Classes members and registers each before occurrence start using operationId/memberId/optional classesSubscriptionId. Omitted subscription chooses eligible earliest-expiry balance. Backend consumes one shared session and prevents duplicate member/occurrence across packages. Register individually, then refresh occurrence/member histories and shared balance.
4. Complete after start on the same effective BusinessDate using operationId/expectedVersion/cancelled. Cancellation is allowed only with no attendance; completed history is immutable. Refresh occurrence version before submission.
5. Statistics require branchId/from/to (<=367 days). Count eligible completed occurrences including zero-attendee classes; total valid attendance / completed count; no occurrences yields zero. Optional periodId must exactly match period bounds to compare explicit ClassesAverageTarget. Preserve null targetReached when unconfigured. CompensationStatus="NotConfigured" means **no Classes earnings formula exists**. Do not show a calculated Classes earnings/payout estimate.

## Owner recognition and partial payouts

Configure explicit independent per-coach period thresholds with no defaults; both desired dimensions are supplied in one immutable target request. Attendance remains enabled without targets. Progress shows eligible net-funded value, deferred value, normal rate/target result and projected earnings; unavailable normal settings remain null. Fixed transferred sources always use 50%, independent of attainment. Historical coach attribution survives later collection.

Recognition requires ended effective period and preceding coach periods recognized (including zero). It snapshots Private earnings and does not pay cash or close Sales calculations. Re-fetch recognized history and balance. Member payments FROM members increase eligible funding; payouts TO coaches reduce earned outstanding separately.

Payout form selects coach, actual branch account/category, positive amount <= returned outstanding, reason and optional receiptReference. Owner capability+global scope+own active shift+available actual funds apply. EGP 4000 earned, 2000 paid now, 2000 later yields outstanding 4000→2000→0; never recompute earnings at payout. Refresh balance/history/payouts, linked expense, account/drawer/shift reports. Do not create a second expense/debit from the frontend: the payout endpoint already posts exactly once. Generic expense reversal of payout is blocked. A finalized-earnings refund affecting funded eligible value is blocked until an adjustment policy exists.

## Frontend verification

Use fabricated fixtures to exercise assigned-coach visibility, direct Deny, branch switches/cache clearing, missing reception/expired regular membership, Classes without reception, shared Yoga/Zumba deduction, duplicate/uncertain retry, stale versions, null targets, 50/60 versus fixed transfer, expiry without fake attendance, deferred collection, partial payout and available-funds rejection. Do not send financial actions against Production for frontend tests. Follow [Postman examples](postman/README.md) and [exact contracts](api/Coaching.md); runtime environment setup is documented separately.
