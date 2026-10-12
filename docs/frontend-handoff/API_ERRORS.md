# Errors and response handling

The API has no universal `{data,success,error}` envelope. Successful arrays, primitives, objects and pages are returned directly. Null Ok results may be HTTP 204; never call JSON.parse on an empty body. Business errors in BranchControllerBase use **title** for their message; detail is optional. MVC binding errors use ValidationProblemDetails with an `errors` dictionary. JWT 401/403 and unknown-route 404 may have empty bodies.

| Status | Meaning and frontend response |
|---|---|
| 400 | Binding/validation or BusinessRuleException; show field errors or title/detail. Includes changed operation payload and selected stale-version guards, such as Class completion. |
| 401 | Missing/expired/invalid identity; clear protected state and reauthenticate. There is no refresh endpoint. |
| 403 | Effective capability, branch/city, ownership or delivering-coach denial; retain authenticated state and show denied action. |
| 404 | Missing scoped resource, invalid Guid route or unknown route; do not reveal presumed cross-branch existence. |
| 409 | EF concurrency, unique overlap, serialization/deadlock conflict; reload authoritative state and preserve uncertain operation identity/payload. |
| 410 | Disabled compatibility write; use documented replacement. Three legacy Classes writes and Freelance purchase are disabled. |
| 415 | Unsupported request media type; send JSON Content-Type for JSON bodies. |
| 500 | Unexpected error via ProblemDetails; show a recoverable failure, preserve operation identity and avoid blind resubmission of financial actions. |

Illustrative error shapes below show relevant fields; generated `type`, `traceId`, `instance` and negotiation details may vary. These are fabricated, not executed responses.

```json
{"title":"Occurrence is legacy, finalized or has a stale version.","status":400}
```

```json
{"title":"Record changed; reload and retry with its current version.","status":409}
```

```json
{"title":"One or more validation errors occurred.","status":400,"errors":{"operationId":["Invalid identifier."]}}
```

Check-in preview/registration and trial redemption can return HTTP 200 with a business denial in their documented result. Inspect their allowed/reason fields; do not equate all HTTP 200 responses with granted gym access. Coaching session registration returns actual attendance evidence on success or an error and never consumes on rejected input.

For network uncertainty, an operationId endpoint should reuse the exact original UUID/body/actor. Changed payload or a new UUID is a different action, not a safe retry. Catalog edits and other endpoints without operationId have no generic retry key. No ETag, If-Match or universal idempotency header is implemented. The backend remains authoritative for balances and financial side effects.

[Endpoint catalog](API_ENDPOINT_CATALOG.md) · [Integration setup](INTEGRATION_SETUP_AND_EXAMPLES.md) · [Coaching workflows](REACT_COACHING_INTEGRATION.md)
