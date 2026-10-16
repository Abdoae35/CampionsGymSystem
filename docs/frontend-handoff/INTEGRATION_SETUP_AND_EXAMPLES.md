# React integration setup and examples

The API uses `/api` routes, bearer JWT authentication and JSON camelCase. Development profiles in source are HTTP `http://localhost:5236` and HTTPS `https://localhost:7186`; configure the actual deployed base URL. `GET /openapi/v1.json` and `/health/database` are Development-only. `Program.cs` does not configure CORS, so use an approved same-origin proxy for local React development or arrange a backend CORS policy separately. Set a public frontend base-URL variable only; keep signing keys, database settings and credentials on the server.

1. Login with `POST /api/auth/login`; store the token using the frontend project's security policy. Call `GET /api/auth/me` before protected screens and after access changes. Attach `Authorization: Bearer <token>`. On 401, clear session and reauthenticate; on 403, retain login and show permission/scope denial.
2. Use `/me.permissions` to show actions, and the Owner access-administration inventory for grant explanations. Deny wins. Do not infer branch access from a role name or provide a synthetic branch header. Keep current user/branch in query cache keys and invalidate after assignment/role changes.
3. Send JSON `Content-Type: application/json` for bodies. `DateOnly` is `YYYY-MM-DD`; timestamps have offsets. Backend decimals and IDs are authoritative. Do not use JS binary floating point to recompute monetary facts. Enums are mixed numeric/string by property; use generated schemas and [DTO reference](DTO_AND_ENUM_REFERENCE.md).
4. Lists expose their own page shape: many use `{items,totalCount,page,pageSize}`, while Shift/Access pages use `{items,total,page,pageSize}`. Read the individual response schema; do not assume one generic envelope. Page defaults/maximums are route-specific. Keep filters in query keys; debounce name/phone/code search; request bounded pages for histories.
5. Retry financial writes only with the **same** operation ID, actor and payload when the first result is uncertain. A new ID is a new action. Reload after conflict. Read [error contracts](API_ERRORS.md); 204 may be empty, and some access decisions return 200 with `allowed:false`.

Example login and bootstrap (fabricated credentials and token):

```http
POST /api/auth/login
Content-Type: application/json

{"userNameOrEmail":"example.reception","password":"EXAMPLE_ONLY"}
```

```http
GET /api/auth/me
Authorization: Bearer EXAMPLE_TOKEN
```

Example branch-period Shift review before closure:

```http
GET /api/shifts/11111111-1111-4111-8111-111111111111/summary
Authorization: Bearer EXAMPLE_TOKEN
```

```http
GET /api/shifts/11111111-1111-4111-8111-111111111111/branch-transactions?page=1&pageSize=25
Authorization: Bearer EXAMPLE_TOKEN
```

Example optional provider reference: a payment/refund part may send `"providerReference":null`; when present, the exact stored reference is exported in monthly financial evidence. Never infer bank verification from it. See the exact [Sales DTO](api/Sales.md) and [Monthly Finance export](api/MonthlyFinance.md).

Example informational plan creation uses [validated complete body](PLANNED_PERIODIC_EXPENSES.md); Owner membership, `Maintenance.Manage` and `Branches.AccessAll` are all required. The plan does not fund an account. The existing [Postman collection](postman/README.md) and [V1 examples](postman/Campeones.V1.Final.postman_collection.json) provide additional request shapes. No example was executed against customer data.
