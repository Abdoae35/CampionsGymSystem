# Implemented functionality and operational limitations

## Implemented and completed

Private, shared Classes, independent coach targets, net-funded Private earnings and partial Owner payouts are complete in source, with all 36 operations documented. Implementation history records 822 passing tests, clean build and no EF drift. The earlier draft/build/publication blockers are superseded. Attendance never depends on target setup. Confirmed expiry beneficiary, partial-collection funding and Private 50/60/fixed-transfer policy are settled business rules; see [feature rules](PRIVATE_CLASSES.md).

## Rollout prerequisites, not missing APIs

- Coaching migrations `20261012101654_ImplementPrivateClassesAndCoachEarnings` and `20261012104721_EnforceCoachingEvidenceCompleteness` were generated/tested only; application/Production application was not performed by the implementation handoff. This task did not inspect deployed migration history or apply migrations. Verify the target environment separately.
- Eight added catalog capabilities are not grants: Private.RegisterSession, Private.Transfer, Private.SettleExpiry, Classes.RegisterAttendance, CoachEarnings.View, CoachEarnings.ManageTargets, CoachEarnings.Recognize, CoachEarnings.Pay. Provision explicitly, preserving direct Deny overrides. Discretionary expiry/targets/recognition/payout additionally require Branches.AccessAll. Read/manage existing module capabilities are also needed.
- Historical Offers.Manage, Installments.Manage and Sales.QuickCancel were active but ungranted at earlier guarded checks. Current deployment grants are unknown; an authorized operator must recheck/provision. No permission-administration HTTP API is supplied.
- Configure branch CommissionPeriods with explicit contiguous inclusive BusinessDates and positive per-coach Private and/or Classes-average thresholds. No default threshold, Sales-target substitution or Classes commission rate exists. Targets are immutable per coach/period; configure both desired dimensions together.
- Provide authorized accounts/Employee assignments/current branch, active cities/catalogs, package/type coaches, branch accounts, actual drawer initialization, expense categories and own active employee shifts for money operations. A global user still needs their own shift for purchases/collections/payouts. No fictitious money/data should be seeded by the frontend.
- No CORS policy is registered. Use an agreed same-origin development proxy or obtain approved backend origin configuration. Deployed API URL/certificates are not established here. Local profiles are HTTPS 7186 / HTTP 5236; capture server 18748 is not the application URL.

## Intentionally unavailable business functionality

- **Classes compensation is unimplemented until the Owner defines its formula.** Reports return CompensationStatus="NotConfigured". Actual attendance, completed counts/averages and explicit target comparisons are available. Do not infer earnings from Private/Sales rates.
- Refunds reducing retained collections below finalized Private eligible funding reject atomically for unpaid or paid earnings until an adjustment policy exists. Existing finalized Sales-source protections also apply. Coach payout expense reversal/correction has no authorized adjustment flow.
- Sales commission payout/payroll, advances repayment, full monthly closing, automatic expiry recognition, forced/automatic shifts, discrepancy settlement, routine external Cash-In, booking/waitlist/capacity and sync/messaging/notifications are outside scope.
- No refresh/logout/revocation/password reset/registration/account-grant administration endpoints. Local logout clears client state; expired sessions reauthenticate. City and some legacy activity catalogs require out-of-band provisioning.

## Contract and historical-data limitations

- Raw OpenAPI lacks many IActionResult success/error/auth schemas and misrepresents some property converters; the source-derived openapi.integration.json adds response/auth/converter metadata. Use the compiler supplement, catalog, DTO tables and guard evidence for remaining guards. There is no universal envelope, branch header, ETag or universal idempotency header. Selected stale-version guards return 400, while EF conflicts return 409; use per-action evidence.
- Decimal numeric(18,2), bigint ClientCode and long versions can exceed JavaScript precision. No decimal-string transport policy exists. Use decimal-safe input/display and returned totals, and review supported frontend value bounds explicitly.
- Incomplete legacy Private assignment/value/date facts fail closed for mutation; undated legacy Classes do not enter dated target reporting. Unknown historical subscriptions/financial attribution need explicit reconciliation; frontend must not backfill invented facts.
- Four compatibility routes deliberately return 410: three old Classes writes and Freelance compatibility purchase. Historical reports remain available.

No database, deployment, grants or operational data were inspected or changed in this documentation task. These prerequisites require environment verification by the backend operator, not frontend guesses.
