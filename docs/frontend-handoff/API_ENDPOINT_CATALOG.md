# Complete API endpoint catalog

Current source `476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80` on `master` contains **191 controller actions across 23 controllers**, including **36 completed Coaching operations**. Four legacy Operations writes intentionally return 410. OpenAPI contains **192 operations** (191 controller actions + Development database health); the separate Development OpenAPI document route makes **193 exposed registrations**. Health and document serving are not production business APIs.

All controller responses use exact camelCase shapes/types linked below, no generic wrapper. Source-action snippets prove thin adapters and wrapper behavior; compiler-resolved Execute<T> supplies otherwise undeclared success type. Cross-cutting errors: MVC binding/validation 400 ValidationProblemDetails; protected 401/403 may be empty; BranchControllerBase maps BusinessRuleException 400, BranchNotFoundException 404, stale EF version/unique/serialization/deadlock 409; unforeseen errors ProblemDetails 500 via UseExceptionHandler. Guid route mismatch/unknown route 404. Unsupported media type 415 or negotiation behavior follows MVC; no per-action custom convention. Null Ok results can become 204 by default HttpNoContentOutputFormatter. Method status is from return statements, not guessed Swagger codes.

All writes are JSON unless explicitly bodyless; use application/json (actual exported metadata also permits text/json/application/*+json). Headers: Authorization: Bearer for protected endpoints; no branch/operation/ETag header. Bodyless query/path endpoints have no content-type requirement. `operationId` and `expectedVersion` are only implemented where their request schemas expose them. No ETag/rowversion header/If-Match or generic idempotency header exists. Exact operation identity/payload/actor retries matter; many mismatches produce 400. Capability policies and extra BLL checks both apply.

Each endpoint includes an HTTP example and complete schema-shaped output example. **All data is FAKE**; requests are syntactically valid but require matching configured authorized fixtures and permitted lifecycle state, not actual seeded data. Empty arrays/pages are valid possible response examples. Responses illustrate schema, not an observed operational execution. Conditional business validity is stated in module rules and workflows. Read request schema/required fields in both OpenAPI and DTO reference; filters are model-bound case-insensitively and emitted query names are preserved from OpenAPI.

| Controller / functional module | Actions | Detail |
|---|---:|---|
| Auth | 2 | [Auth](api/Auth.md) |
| Branches | 4 | [Branches](api/Branches.md) |
| BusinessDays | 3 | [BusinessDays](api/BusinessDays.md) |
| CheckIns | 7 | [CheckIns](api/CheckIns.md) |
| Coaching | 36 | [Coaching](api/Coaching.md) |
| Commissions | 9 | [Commissions](api/Commissions.md) |
| Crm | 5 | [Crm](api/Crm.md) |
| EmployeeAdvances | 3 | [EmployeeAdvances](api/EmployeeAdvances.md) |
| EmployeeBranches | 4 | [EmployeeBranches](api/EmployeeBranches.md) |
| Employees | 6 | [Employees](api/Employees.md) |
| Expenses | 8 | [Expenses](api/Expenses.md) |
| FinancialAccounts | 2 | [FinancialAccounts](api/FinancialAccounts.md) |
| Leads | 14 | [Leads](api/Leads.md) |
| Members | 9 | [Members](api/Members.md) |
| MembershipBenefits | 5 | [MembershipBenefits](api/MembershipBenefits.md) |
| MembershipPlanCategories | 4 | [MembershipPlanCategories](api/MembershipPlanCategories.md) |
| MembershipPlans | 11 | [MembershipPlans](api/MembershipPlans.md) |
| Offers | 9 | [Offers](api/Offers.md) |
| Operations | 17 | [Operations](api/Operations.md) |
| Sales | 10 | [Sales](api/Sales.md) |
| Shifts | 6 | [Shifts](api/Shifts.md) |
| Subscriptions | 9 | [Subscriptions](api/Subscriptions.md) |
| Vault | 8 | [Vault](api/Vault.md) |

## Development infrastructure (actual Program registrations)
| Method | Exact route | Status/auth/body/response | Source |
|---|---|---|---|
| GET | `/health/database` | Development only, anonymous; no inputs; 200 `{ "status": "healthy" }` or 503 no declared body; CanConnectAsync is diagnostic, not an application CRUD route | `API/Program.cs` |
| GET | `/openapi/{documentName}.json` (v1: `/openapi/v1.json`) | Development only, anonymous; generated OpenAPI JSON, no body/branch/capability; invalid document name framework behavior not runtime tested | `API/Program.cs app.MapOpenApi()` |
No WeatherForecast mapping despite leftover record. No Minimal API business routes, SignalR hubs, static export/file endpoints or additional controllers registered. Development docs endpoint does not list itself. These are not production integration APIs.
Fake infrastructure examples: `GET /health/database` → possible 200 `{ "status": "healthy" }`; `GET /openapi/v1.json` → [actual captured spec](openapi.json), with **capture server URL 18748**, not configured application base URL. The audit did not request database health.
[DTOs/enums](DTO_AND_ENUM_REFERENCE.md) · [permission inventory](api/PERMISSION_REFERENCE.md) · [guards/validators](api/RULES_INDEX.md) · [completeness audit](COMPLETENESS_AUDIT.md)
