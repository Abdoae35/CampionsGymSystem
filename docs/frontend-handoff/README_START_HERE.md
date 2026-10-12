# Campeones backend → frontend handoff

Latest source check: **191 controller actions / 36 Coaching**; one late Member attendance read fully documented in [source addendum](LATE_SOURCE_ADDENDUM.md). No DTO contract drift. Occurrence future-date concern is corrected in later source. Captured OpenAPI retains its exact earlier snapshot.


Latest branch update: frozen 2026-10-12T10:44:04.604445+00:00 at `bf733e2502a85603b80f8ad8a01747264a0f518b` plus uncommitted operational files adds **35 Coaching routes** (190 current actions total). The later isolated snapshot builds; initial CS8999 was superseded by concurrent domain publication. Stable contracts below remain at 0bc4476; new draft contracts/confirmed policies and three legacy Class 410 changes are in [branch-only status](BRANCH_ONLY_PRIVATE_CLASSES_STATUS.md). **Use that update before interpreting any earlier pending/no-route statement below.**


Reviewed 2026-10-12. Repository: `https://github.com/Abdoae35/CampionsGymSystem`.
Current branch: `PrivateCoaching-Classes-Implementation`; committed baseline: `0bc4476ef184dd95a4f771624d4a8860c00f1332`.
Working-tree audit snapshot: **2026-10-12T10:07:06.774981+00:00**. Other work continued during this audit; this is a versioned briefing, not a promise about later edits. Live remote `master`, `SalesCommissions&Targets`, and `PrivateCoaching&Classes` were read and all matched this commit. At initial inspection implementation branch had no upstream; concurrent domain publication later created one. No branch was switched.

ASP.NET Core/.NET 10, API → BLL → EF Core/PostgreSQL, Guid Identity, Bearer JWT, capability permissions and branch scope. Source controllers, service contracts/guards, serializers, registrations, configurations, migrations and test assertions are the authority. Older docs contain superseded “future” statements.

| Module | Verified availability at committed baseline |
|---|---|
| Auth/permissions | Login and current-user APIs; permission evaluation implemented; administration UI APIs absent |
| Branches/employees | Scoped profiles, assignment/transfer/status; account provisioning separate |
| Members/CRM | Registration, ownership, leads, conversion, interactions and explicit follow-ups |
| Plans/benefits | Branch plans, global catalogs, Freelance eligibility |
| Subscriptions/check-in | Paid lifecycle through Sales; freezes/extensions/holidays, free employee entitlement, entrance/trials/borrowing |
| Sales/payments/offers | Customer gym finance, deposits/installments/split tender, cancellation/refunds/Quick Cancel |
| Shifts/vault/expenses/advances | Manual shifts, shared drawer, direct paid expenses/returns, advances and reports |
| Sales commissions | Independent New/Renewal rules, periods, progress, source statements/finalization; payouts deferred |
| Legacy Activities/Classes/Freelance | Narrow existing operations; **partial**, including one deliberately disabled 410 route |
| New Private/Classes/coach earnings | **Domain committed, 35 operational routes uncommitted/in progress** in later snapshot; full branch contracts linked above |
| Payroll, commission payouts, sync, messaging, member financial branch transfer | Planned/deferred; no integration contract supplied |

Evidence labels: **VERIFIED FROM CODE** = reviewed committed source; **TESTED** = fresh checks specifically listed in audit; **DOCUMENTED ONLY** = historical report not rerun; **PLANNED OR NOT IMPLEMENTED** = absent public contract or draft; **UNKNOWN** = cannot establish from this inspection. Source-verified does not mean runtime exercised against operational data.

Read in this order:

1. [Auth/permissions/branch context](AUTH_PERMISSIONS_BRANCH_CONTEXT.md), then [setup](INTEGRATION_SETUP_AND_EXAMPLES.md).
2. [Endpoint master index](API_ENDPOINT_CATALOG.md) and its per-controller files; [DTOs/enums](DTO_AND_ENUM_REFERENCE.md).
3. [Workflows/business rules](FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md).
4. [Gaps/blockers](OPEN_QUESTIONS_AND_GAPS.md), [OpenAPI provenance](OPENAPI_STATUS.md), [completeness audit](COMPLETENESS_AUDIT.md).
5. Paste [new-chat starter](HANDOFF_FOR_NEW_CHAT.txt) into a separate conversation with this whole folder attached.

Recommended implementation: auth → effective capabilities/current branch → employee own shift → members/CRM and product browsing → Sales/entitlements/check-in → operational finance → commission reports/admin → approved legacy narrow screens. Defer new Coaching screens until a verified published contract exists. Inspect the separate React/TS/Vite/Tailwind/shadcn-style/Router/TanStack Query/RHF/Zod repository first; no existing adapters are assumed.

Blockers: initial working tree failed compilation; later frozen branch source builds, but feature release verification is pending. Live draft files changed during audit. CORS is not configured. Credentials, grants, branch/city catalogs, account/drawer initialization and commission configuration must be supplied by authorized backend administration. The historically missing three Sales grants are **not rechecked against a database**. The exported baseline OpenAPI omits many IActionResult response schemas and authorization metadata; use the supplemental catalog, not generated clients alone.

Every file in this folder is portable. Source references are repo-relative provenance; all handoff cross-links stay inside the folder. All examples are fabricated and must be mapped to configured test fixtures. Do not execute them against production.

[Current branch contract update](BRANCH_ONLY_PRIVATE_CLASSES_STATUS.md) · [All current branch endpoints](branch-only/API_ENDPOINT_CATALOG.md) · [Every portable file](PORTABLE_FILE_INDEX.md). The root contracts are stable baseline; branch-only/ contains the complete later snapshot with explicit pending availability.
