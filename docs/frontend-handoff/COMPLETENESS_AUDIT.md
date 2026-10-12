# Completeness and contract audit

Latest source check: **191 controller actions / 36 Coaching**; one late Member attendance read fully documented in [source addendum](LATE_SOURCE_ADDENDUM.md). No DTO contract drift. Occurrence future-date concern is corrected in later source. Captured OpenAPI retains its exact earlier snapshot.


Stable commit `0bc4476ef184dd95a4f771624d4a8860c00f1332`; current branch snapshot `2026-10-12T10:44:04.604445+00:00` at HEAD `bf733e2502a85603b80f8ad8a01747264a0f518b` plus uncommitted operational work. The repository changed while audited, so contracts are frozen to both explicit snapshots. Not all current-branch code is published/verified feature behavior.

| Module/controller | Independently counted source actions | Stable documented | Current branch documented | Missing |
|---|---:|---:|---:|---:|
| Auth | 2 | 2 | 2 | 0 |
| Branches | 4 | 4 | 4 | 0 |
| BusinessDays | 3 | 3 | 3 | 0 |
| CheckIns | 7 | 7 | 7 | 0 |
| Coaching | 36 | 0 | 36 | 0 |
| Commissions | 9 | 9 | 9 | 0 |
| Crm | 5 | 5 | 5 | 0 |
| EmployeeAdvances | 3 | 3 | 3 | 0 |
| EmployeeBranches | 4 | 4 | 4 | 0 |
| Employees | 6 | 6 | 6 | 0 |
| Expenses | 8 | 8 | 8 | 0 |
| FinancialAccounts | 2 | 2 | 2 | 0 |
| Leads | 14 | 14 | 14 | 0 |
| Members | 9 | 9 | 9 | 0 |
| MembershipBenefits | 5 | 5 | 5 | 0 |
| MembershipPlanCategories | 4 | 4 | 4 | 0 |
| MembershipPlans | 11 | 11 | 11 | 0 |
| Offers | 9 | 9 | 9 | 0 |
| Operations | 17 | 17 | 17 | 0 |
| Sales | 10 | 10 | 10 | 0 |
| Shifts | 6 | 6 | 6 | 0 |
| Subscriptions | 9 | 9 | 9 | 0 |
| Vault | 8 | 8 | 8 | 0 |
| **Total** | **191** | **155** | **191** | **0** |

23 current controllers / 22 stable. Two additional Development-only registrations documented: database health and OpenAPI document serving. Current total 192 exposed route/method registrations; OpenAPI excludes itself and captures 191. Stable totals 157 exposed / 156 OpenAPI. Four current disabled 410 routes vs one stable; route exposure is not feature completeness.

## Actual verification performed
- Read required instructions/history/README/skills guide, architecture/feature/API docs, controller actions and Program registration/middleware, authentication/policy/permission/branch implementations, service contract types/guards/query ordering, current EF domain/configuration/migration evidence, relevant test assertions and Postman JSON. No business source was edited by this audit.
- Git current/remote-tracking branches, HEAD/ancestry/status and live read-only ls-remote: initial master/SalesCommissions&Targets/PrivateCoaching&Classes all 0bc4476; no branch switch. During audit separate feature work published bf733e2 domain and added operational drafts. No commit/push by this task. Live final remote state is not inferred from earlier read.
- Initial current build failed CS8999 in superseded uncommitted migration. Baseline archived source built. Later frozen branch application source built successfully. Temporary extraction tool alone has three nullable warnings. Never equate these builds with runtime financial regression.
- Actual isolated OpenAPI HTTP 200 exports: stable 124 paths/156 operations/90 schemas; working 152 paths/191 operations/102 schemas. Disabled DB target, ephemeral config, no seed/migration flags, only document GET, stopped both processes. Exact captured JSON preserved, including temporary server URLs.
- Registered compiled MVC actions cross-checked against independent Http attribute counts per concrete class and method/route keys in actual OpenAPI and documentation headings. Zero omissions/unexpected actions for both snapshots. Abstract base is not an exposed controller; leftover WeatherForecast record has no mapped route.
- Compiler-resolved endpoint argument types, Execute<T> return types, anonymous adapters, getters/inherited fields, enum constants, attributes/constructor defaults: stable 311 and working 366 reachable type shapes. Clean controller semantic extraction after removing duplicate assembly reference. Full source-provenance schemas plus exact JSON casing and mixed enum converters. OpenAPI body schema required/strict/nullable differs from runtime business guards; both are documented.
- Both source snapshots map actual policy metadata and additional BLL permission/scope checks; exact policy inventory separate from future/inactive/catalog-only keys. No operational role-grant verification. Date/city/financial source separation and money/rate/operation/version guards cross-referenced in source-local evidence.
- Request examples: 73 stable + 90 current structurally checked against captured required/enum/nullability/additional-property/length/pattern schema constraints. This lightweight check is not full JSON Schema validation or proof fixture/lifecycle validity. 155 stable + 190 output examples parsed as JSON; schema-derived, not observed application responses.
- Fresh test execution: isolated baseline CheckInContractTests **5 passed / 0 failed / 0 skipped** (mass-assignment rejection + enum/default compatibility), no DB fixture. No full suite or new Coaching financial/HTTP/concurrency tests run here. Historical 772/772 and new feature domain/initial operational results are clearly attributed to project history.
- Postman collection/environment JSON parsed; examples not executed. Coverage comparisons below distinguish canonical examples from obsolete/disabled compatibility paths. No secrets/environment files copied.
- Internal file/anchor links checked and repaired; portable ZIP CRC/member/content checks and file manifest performed at delivery. See final file list/hash evidence.

## Limitations and backend attention
Current deployment database schema/accounts/grants/configuration, production runtime URLs, live bank balances, approved origins, real business API responses and full regression are UNKNOWN. No DB read/write, operational seeding, migrations, deployment, auth requests or Postman write runner were used. WebApplicationFactory and direct sandbox export startup timed out; safe export outside sandbox succeeded. Offline restores emitted nonfatal CSSM diagnostic. Current branch may continue changing after snapshot; refresh before enabling draft flows. Missing Classes compensation/late-refund adjustment and occurrence timestamp issue are in [branch status](BRANCH_ONLY_PRIVATE_CLASSES_STATUS.md). CORS/provisioning/legacy facts remain in [gaps](OPEN_QUESTIONS_AND_GAPS.md).

## Postman cross-check
Collection JSON valid; 211 request examples found. IDs/tokens/environment values not included. Postman is supporting documentation, not the route authority. Current snapshot may have new routes not yet in collection; use compiled catalog.
Conservative method/path matching covers 142 / 155 stable route identities. Missing example coverage does **not** mean an endpoint is missing; zero endpoint-catalog omissions above. No new Coaching examples are asserted without matching actual collection.
