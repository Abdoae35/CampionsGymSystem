# Backend gaps and integration blockers

Latest source check: **191 controller actions / 36 Coaching**; one late Member attendance read fully documented in [source addendum](LATE_SOURCE_ADDENDUM.md). No DTO contract drift. Occurrence future-date concern is corrected in later source. Captured OpenAPI retains its exact earlier snapshot.


Latest branch update: frozen 2026-10-12T10:44:04.604445+00:00 at `bf733e2502a85603b80f8ad8a01747264a0f518b` plus uncommitted operational files adds **35 Coaching routes** (190 current actions total). The later isolated snapshot builds; initial CS8999 was superseded by concurrent domain publication. Stable contracts below remain at 0bc4476; new draft contracts/confirmed policies and three legacy Class 410 changes are in [branch-only status](BRANCH_ONLY_PRIVATE_CLASSES_STATUS.md). **Use that update before interpreting any earlier pending/no-route statement below.**


## P0 — choose a buildable released baseline

Initial branch `PrivateCoaching-Classes-Implementation` was at `0bc4476ef184dd95a4f771624d4a8860c00f1332` with no upstream; later domain publication added bf733e2 and an upstream. Live remote master/SalesCommissions&Targets/PrivateCoaching&Classes matched the commit. The initial `dotnet build CampionsGymSystem.slnx --no-restore` failed with **CS8999**, `DAL/Migrations/20261012095814_ImplementPrivateClassesAndCoachEarnings.cs:16` raw-string whitespace. Audit did not fix it. Other development continued and later files may differ. The baseline export comes from an isolated exact Git archive, not stale bin output or a claim of current draft readiness.

Working snapshot 2026-10-12T10:07:06.774981+00:00 contains uncommitted DAL entities/configuration/snapshot/new migration for Private/Classes/earnings, Coaching contracts/service drafts, Sales Coaching integration and three changed existing BLL integration files. **Branch-only, uncommitted, not tested by this audit**. At the **earlier** snapshot no Coaching controller/registration/public routes existed; the later catalog includes all 35 draft routes. No stable Private/Coach earnings integration contract should be inferred from entities, draft DTOs, migration permission inserts or a feature plan. Working-tree drift is reported in [audit](COMPLETENESS_AUDIT.md); get a newly verified published package before those screens.

## P1 — browser/auth/operational setup

- CORS has no policy/registration. Backend team must establish allowed origins or agree a dev proxy. Actual deployed URL/HTTPS certificate and proxy behavior UNKNOWN.
- No refresh/logout/revocation/password reset/registration/account/permission administration API. Frontend can bootstrap with login/me and local logout only. Provision accounts/grants out of band; do not add guessed settings screens.
- Historically Offers.Manage, Installments.Manage, Sales.QuickCancel were active but ungranted. Current DB grants **UNKNOWN**, not rechecked; ask authorized administrator for intended provisioning. Fresh role templates do not update existing roles.
- Active cities/branch calling codes, Employee-account/assignment, mapped financial accounts/drawer initial count, ExpenseCategory and intended capabilities are prerequisites. No City catalog CRUD/list or complete SportsActivity/ActivityPackage/GroupClass/Schedule management API. Some operations need IDs currently unavailable through a self-service catalog route.
- Sales commission fallback/employee rules and branch period boundaries must be explicitly configured; missing rules show unconfigured and block finalization. No default 30k/rates automatically exist.

## P1 — current Private/Classes release blockers

Latest source and confirmed project-history decisions: unused expired Private value belongs to coach assigned at expiry; partial net collections fund delivered/expiry value; explicit Private target uses normal 50/60 and transferred fixed 50. These are no longer unanswered questions. **Classes compensation formula remains undefined and disabled**; actual statistics/average target exist in branch draft. Refund adjustments invalidating recognized/paid Private earnings remain deferred; protected refunds reject. See branch-only status for exact implemented draft sequences and later corrected occurrence-timestamp guard. Operational final regression/publication/deployed migration/grants are not established by this audit.

## P2 — existing compatibility and API gaps

- Legacy Activity purchase records entitlement/amount due, without managed collection API; legacy Class attendance lacks new sale-linked entitlement check. Legacy reports are not new Private/Classes coach earnings or payout. `/api/freelance-plans/purchases` is exposed but intentionally **410 disabled**.
- OpenAPI uses IActionResult and omits most actual success body types/policies. Supplemental catalog recovers Execute generic types, anonymous wrappers and exact source. Generated TS clients must be reviewed/augmented; OpenAPI alone is incomplete. Query/body required flags and nonnullable C# fields need distinction from business guards.
- Serialization is mixed: many DTO status fields are strings; several actual enum-valued outputs/inputs are numeric, selected property/type converters are string. Pagination count uses totalCount vs total vs employeeCount. Some successful null results become 204. No universal data envelope.
- No ETag/If-Match or universal retry key. Some mutations have operationId, selected expectedVersion, others neither. A duplicate/rebound operation can be 400, not uniformly 409. Check-in/trial denial is 200 with false.
- Monetary numeric(18,2), ClientCode bigint and Shift long version can exceed JS precision. No decimal-string transport policy is implemented. Backend should decide any contract change separately; frontend must not treat binary floats as financial authority.
- Same-city access is separate from administrative branch scope. CRM cross-branch ownership does not grant Member directory/financial rights. Member branch financial transfer is absent.
- No automatic holiday apply, follow-up generation, midnight/forced Shift closure, routine external Cash-In, discrepancy settlement, advance repayment/payroll, monthly closing, Sales commission payouts or late-finalized-refund adjustment flow.
- No file/report export, invitation issuance, hardware scanner integration, message delivery, notifications or offline sync APIs.

## P2 — legacy reconciliation

Null historical plan ownership/snapshots/BusinessDate/Shift/period attribution remain unknown. Ambiguous phone/assignments/entitlements, unmanaged paid expense, undated commission sources/legacy periods and incomplete coach revenue attribution fail closed or exclude/reconcile explicitly. Frontend cannot backfill facts or infer them from current templates. Historical migration application and current production schema/grants are **UNKNOWN** to this audit: no DB accessed/applied/deployed.

Remaining backend attention: build/verify/publish pending branch, approve outstanding Coaching policies, supply approved browser origins/dev integration and test identities/grants/catalog/drawer/rule configuration, then recapture contract. No application changes, migrations, grants or operational data changes were performed for this documentation task.
