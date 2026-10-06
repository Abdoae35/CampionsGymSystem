# Simple manual API testing

1. Start the API: `dotnet run --project CampionsSystem.API --launch-profile http`.
2. Import `Campions.Auth.postman_collection.json`. Open **Campions Auth - Simple Login**.
3. Select the **Campions Auth - LOCAL PRIVATE** environment (contains demoPassword).
4. Send **1. Login**. Default username is `dev.reception`; change it in Body for another account.
5. Send **2. Current user**. Login saves the token in this collection and Current user sends it automatically. No copying/pasting.

Only two auth endpoints exist: POST /api/auth/login and GET /api/auth/me.
Leave **Optional automated test cases** collapsed. They are examples of successful/failed requests to the same endpoints, not separate APIs. Some intentionally return 401.

If you previously imported the old collection, use the new **Campions Auth - Simple Login** collection. Your imported Postman copy does not update when repository files change.

Login must return 200 and run its Scripts / Post-response script. Current user sends the latest login token; logging in as another account replaces it. If token expires, send Login again. The collection starts with an empty token and passwords are never committed. Do not share/export a collection after login without clearing accessToken.

Private local environment is ignored by Git. Regenerate it with `python3 scripts/setup-local-auth-demo.py` if needed. See [local setup](../docs/authentication/LOCAL_API_TESTING.md).

Automated permission 200/403 tests remain separate; no business/debug endpoints added.
